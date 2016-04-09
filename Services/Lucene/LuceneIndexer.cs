using CodeIDX.Helpers;
using CodeIDX.ViewModels;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Lucene.Net.QueryParsers;
using System.Diagnostics;

namespace CodeIDX.Services.Lucene
{
    public class LuceneIndexer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static LuceneIndexer Instance { get; private set; }

        private bool _IsIndexing;
        public bool IsIndexing
        {
            get
            {
                return _IsIndexing;
            }
            private set
            {
                _IsIndexing = value;
                FirePropertyChanged("IsIndexing");
            }
        }

        static LuceneIndexer()
        {
            Instance = new LuceneIndexer();
        }

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public void CreateIndexDirectory(IEnumerable<string> directoriesToIndex, IEnumerable<string> fileFilter, string targetIndexDirectory, CancellationToken cancelToken)
        {
            if (IsIndexing)
                return;

            IsIndexing = true;

            ErrorProvider.Instance.LogInfo("CreateIndexDirectory " + targetIndexDirectory);

            try
            {
                if (System.IO.Directory.Exists(targetIndexDirectory))
                    System.IO.Directory.Delete(targetIndexDirectory, true);

                System.IO.Directory.CreateDirectory(targetIndexDirectory);
                using (var indexDirectory = FSDirectory.Open(targetIndexDirectory))
                using (var indexWriter = InitIndexWriter(indexDirectory))
                {
                    var allFiles = GetAllFiles(directoriesToIndex, fileFilter);

                    List<Document> documents = new List<Document>();
                    foreach (var curFile in allFiles)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return;

                        var curDoc = BuildDocument(curFile);
                        if (curDoc != null)
                            indexWriter.AddDocument(curDoc);
                    }

                    indexWriter.Optimize();
                }
            }
            finally
            {
                IsIndexing = false;
            }
        }

        private IndexWriter InitIndexWriter(FSDirectory indexDirectory)
        {
            return new IndexWriter(indexDirectory, new WhitespaceAnalyzer(), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        private IEnumerable<string> GetAllFiles(IEnumerable<string> directoriesToIndex, IEnumerable<string> fileFilter)
        {
            if (directoriesToIndex == null)
                return Enumerable.Empty<string>();

            ConcurrentBag<string> resultCollection = new ConcurrentBag<string>();
            Parallel.ForEach<string>(directoriesToIndex, sourceDirectory =>
                {
                    if (string.IsNullOrEmpty(sourceDirectory) || !System.IO.Directory.Exists(sourceDirectory))
                        return;

                    var filteredFiles = FilterFiles(System.IO.Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories), fileFilter);
                    foreach (var file in filteredFiles)
                        resultCollection.Add(file);
                });

            return resultCollection;
        }

        private IEnumerable<string> FilterFiles(IEnumerable<string> files, IEnumerable<string> fileFilters)
        {
            var filteredFiles = files;
            if (fileFilters != null &&
                fileFilters.Any() &&
                !fileFilters.Contains("*.*"))
            {
                var validExtensions = fileFilters.Select(cur => cur.TrimStart('*')).ToList();
                filteredFiles = filteredFiles.Where(cur =>
                {
                    var extension = Path.GetExtension(cur);
                    if (string.IsNullOrEmpty(extension))
                        return false;

                    return validExtensions.Contains(extension);
                });
            }

            return filteredFiles;
        }

        private bool IsValidFile(string file, List<string> fileFilter)
        {
            return fileFilter == null ||
                fileFilter.Count() == 0 ||
                fileFilter.Contains("*.*") ||
                fileFilter.Any(filter =>
                {
                    var extension = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(extension))
                        return false;

                    return fileFilter.Any(curExt => extension == curExt.Replace("*", ""));
                });
        }

        private Document BuildDocument(string file)
        {
            if (string.IsNullOrEmpty(file))
                return null;

            Document doc = new Document();
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string fileDirectory = Path.GetDirectoryName(file);
                    doc.Add(new Field(Constants.IndexFields.Directory, fileDirectory, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field(Constants.IndexFields.Filename, Path.GetFileNameWithoutExtension(file), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field(Constants.IndexFields.Extension, Path.GetExtension(file), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field(Constants.IndexFields.LastModified, new FileInfo(file).LastWriteTime.Ticks.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    //TODO split content when too long ( > int.max )

                    string content = sr.ReadToEnd();
                    doc.Add(new Field(Constants.IndexFields.AnalizedDirectory, LuceneHelper.ExpandTokenBreak(fileDirectory.ToLower()), Field.Store.NO, Field.Index.ANALYZED));
                    doc.Add(new Field(Constants.IndexFields.Content, LuceneHelper.ExpandTokenBreak(content), Field.Store.NO, Field.Index.ANALYZED));
                    doc.Add(new Field(Constants.IndexFields.ContentCaseInsensitive, LuceneHelper.ExpandTokenBreak(content.ToLower()), Field.Store.NO, Field.Index.ANALYZED));
                }
            }
            catch
            {
                return null;
            }

            return doc;
        }

        public void UpdateIndexDirectory(IndexViewModel index, CancellationToken cancelToken)
        {
            if (IsIndexing)
                return;

            List<string> filesToUpdate = new List<string>();
            List<int> deletedDocuments = new List<int>();

            IsIndexing = true;
            try
            {
                using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                {
                    using (var reader = IndexReader.Open(indexDirectory, false))
                    using (var searcher = new IndexSearcher(reader))
                    {
                        var allFiles = GetAllFiles(index.SourceDirectories, index.FileFilters).ToList();

                        //find deleted files
                        Parallel.For(0, reader.MaxDoc, (docNum, loopState) =>
                        {
                            if (cancelToken.IsCancellationRequested)
                                loopState.Stop();

                            Document curDocument = reader.Document(docNum);
                            string curFilename = GetFilename(curDocument);

                            if (!allFiles.Contains(curFilename))
                                deletedDocuments.Add(docNum);
                        });
                        if (cancelToken.IsCancellationRequested)
                            return;

                        //find documents to update
                        Parallel.ForEach(allFiles, (file, loopState) =>
                        {
                            if (cancelToken.IsCancellationRequested)
                                loopState.Stop();

                            FileInfo curFileInfo = new FileInfo(file);
                            int docNum = FindDocument(searcher, curFileInfo);
                            if (docNum != -1)
                            {
                                var doc = reader.Document(docNum);
                                var fileLastWrite = curFileInfo.LastWriteTime.Ticks;
                                long docLastFileWrite;
                                var lastModifiedField = doc.GetField(Constants.IndexFields.LastModified);
                                if (lastModifiedField != null && long.TryParse(lastModifiedField.StringValue, out docLastFileWrite))
                                {
                                    if (fileLastWrite > docLastFileWrite)
                                    {
                                        deletedDocuments.Add(docNum);
                                        filesToUpdate.Add(file);
                                    }
                                }
                            }
                            else
                            {
                                filesToUpdate.Add(file);
                            }
                        });

                        //delete documents
                        foreach (var docNum in deletedDocuments)
                            reader.DeleteDocument(docNum);

                        if (cancelToken.IsCancellationRequested)
                            return;
                    }

                    if (filesToUpdate.Count > 0)
                    {
                        using (var writer = InitIndexWriter(indexDirectory))
                        {
                            foreach (var file in filesToUpdate)
                            {
                                if (cancelToken.IsCancellationRequested)
                                    return;

                                writer.AddDocument(BuildDocument(file));
                            }

                            writer.Optimize();
                        }
                    }
                }
            }
            finally
            {
                IsIndexing = false;
            }
        }

        private string GetFilename(Document document)
        {
            var directoryField = document.GetField(Constants.IndexFields.Directory);
            var filenameField = document.GetField(Constants.IndexFields.Filename);
            var extensionField = document.GetField(Constants.IndexFields.Extension);
            if (directoryField == null || filenameField == null || extensionField == null)
                return string.Empty;

            string directory = directoryField.StringValue;
            string name = filenameField.StringValue;
            string extension = extensionField.StringValue;

            return Path.Combine(directory, name + extension);
        }

        private int FindDocument(IndexSearcher indexSearcher, FileInfo fileInfo)
        {
            BooleanQuery boolQuery = new BooleanQuery();
            boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Directory, fileInfo.Directory.FullName)), Occur.MUST));
            boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Filename, Path.GetFileNameWithoutExtension(fileInfo.Name))), Occur.MUST));
            boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Extension, fileInfo.Extension)), Occur.MUST));

            var result = indexSearcher.Search(boolQuery, 1).ScoreDocs.FirstOrDefault();
            if (result != null)
                return result.Doc;

            return -1;
        }

        private IEnumerable<int> FindDocuments(IndexSearcher indexSearcher, DirectoryInfo directoryInfo)
        {
            var directoryQuery = new TermQuery(new Term(Constants.IndexFields.Directory, directoryInfo.FullName));
            var subDirectoryQuery = new PrefixQuery(new Term(Constants.IndexFields.Directory, directoryInfo.FullName + "\\"));

            BooleanQuery query = new BooleanQuery { MinimumNumberShouldMatch = 1 };
            query.Add(directoryQuery, Occur.SHOULD);
            query.Add(subDirectoryQuery, Occur.SHOULD);

            TopDocs results = indexSearcher.Search(query, int.MaxValue);
            foreach (var match in results.ScoreDocs)
            {
                yield return match.Doc;
            }
        }

        public void DeleteDocument(string deletedFile, IndexViewModel index)
        {
            if (string.IsNullOrEmpty(deletedFile))
                return;

            try
            {
                using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                using (var reader = IndexReader.Open(indexDirectory, false))
                using (var searcher = new IndexSearcher(reader))
                {
                    int docNum = FindDocument(searcher, new FileInfo(deletedFile));
                    if (docNum != -1)
                        reader.DeleteDocument(docNum);
                }
            }
            catch (Exception e)
            {
                ErrorProvider.Instance.LogError("DeleteDocument Exception " + e.ToString());
            }
        }

        internal void DeleteDocumentDirectory(string deletedDirectory, IndexViewModel index)
        {
            if (string.IsNullOrEmpty(deletedDirectory))
                return;

            try
            {
                using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                using (var reader = IndexReader.Open(indexDirectory, false))
                using (var searcher = new IndexSearcher(reader))
                {
                    //delete all files in deleted directory
                    IEnumerable<int> deletedDocs = FindDocuments(searcher, new DirectoryInfo(deletedDirectory));
                    foreach (var docNum in deletedDocs)
                    {
                        if (docNum != -1)
                            reader.DeleteDocument(docNum);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorProvider.Instance.LogError("DeleteDocumentDirectory Exception " + e.ToString());
            }
        }

        public void AddDocumentDirectory(string newDirectory, IndexViewModel index)
        {
            if (string.IsNullOrEmpty(newDirectory) || !System.IO.Directory.Exists(newDirectory))
                return;

            try
            {
                using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                using (var writer = InitIndexWriter(indexDirectory))
                {
                    var filteredFiles = FilterFiles(System.IO.Directory.EnumerateFiles(newDirectory, "*.*", SearchOption.AllDirectories), index.FileFilters);
                    foreach (var newFile in filteredFiles)
                    {
                        writer.AddDocument(BuildDocument(newFile));
                        writer.Optimize();
                    }
                }
            }
            catch (Exception e)
            {
                ErrorProvider.Instance.LogError("AddDocumentDirectory Exception " + e.ToString());
            }
        }

        public void AddDocument(string newFile, IndexViewModel index)
        {
            if (string.IsNullOrEmpty(newFile) || !File.Exists(newFile) || !IsValidFile(newFile, index.FileFilters))
                return;

            ErrorProvider.Instance.LogInfo("AddDocument " + newFile);

            try
            {
                using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                using (var writer = InitIndexWriter(indexDirectory))
                {
                    writer.AddDocument(BuildDocument(newFile));
                    writer.Optimize();
                }
            }
            catch (Exception e)
            {
                ErrorProvider.Instance.LogError("AddDocument Exception " + e.ToString());
            }
        }

        public void UpdateDocument(string file, IndexViewModel index)
        {
            if (string.IsNullOrEmpty(file))
                return;

            if (!File.Exists(file))
            {
                DeleteDocument(file, index);
            }
            else
            {
                bool fileWasModified = false;

                try
                {
                    //check for modified file, delete if modified
                    using (var indexDirectory = FSDirectory.Open(index.IndexDirectory))
                    using (var reader = IndexReader.Open(indexDirectory, false))
                    using (var searcher = new IndexSearcher(reader))
                    {
                        //check if file was modified
                        FileInfo fileInfo = new FileInfo(file);
                        var docNum = FindDocument(searcher, fileInfo);
                        if (docNum != -1)
                        {
                            var doc = reader.Document(docNum);
                            var fileLastWrite = fileInfo.LastWriteTime.Ticks;
                            long docLastFileWrite;
                            var lastModifiedField = doc.GetField(Constants.IndexFields.LastModified);
                            if (lastModifiedField != null && long.TryParse(lastModifiedField.StringValue, out docLastFileWrite))
                            {
                                if (fileLastWrite > docLastFileWrite)
                                {
                                    fileWasModified = true;
                                    reader.DeleteDocument(docNum);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ErrorProvider.Instance.LogError("UpdateDocument Exception " + e.ToString());
                }

                if (fileWasModified)
                {
                    AddDocument(file, index);
                }
            }

        }

    }

}
