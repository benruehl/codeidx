using CodeIDX.Helpers;
using CodeIDX.ViewModels;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using CodeIDX.Settings;
using System.Collections.Concurrent;

namespace CodeIDX.Services.Lucene
{
    public class LuceneSearcher : INotifyPropertyChanged
    {

        private class GetMatchingLinesArgs
        {
            private string _File;
            private string _SearchString;
            private bool _MatchCase;
            private bool _MatchWholeWord;
            private bool _UseWildcards;
            private List<string> _WildcardQueryParts;

            /// <summary>
            /// Summary for GetMatchingLinesArgs
            /// </summary>
            public GetMatchingLinesArgs(string file, string searchString, IEnumerable<string> wildpartQueryParts, bool matchCase, bool matchWholeWord)
            {
                _File = file;
                _SearchString = searchString;
                _MatchCase = matchCase;
                _MatchWholeWord = matchWholeWord;

                if (wildpartQueryParts != null)
                    _WildcardQueryParts = wildpartQueryParts.ToList();
            }

            public List<string> WildcardQueryParts
            {
                get
                {
                    return _WildcardQueryParts;
                }

            }
            public bool UseWildcards
            {
                get
                {
                    return _WildcardQueryParts != null;
                }
            }
            public string File
            {
                get
                {
                    return _File;
                }
            }
            public string SearchString
            {
                get
                {
                    return _SearchString;
                }
            }
            public bool MatchCase
            {
                get
                {
                    return _MatchCase;
                }
            }
            public bool MatchWholeWord
            {
                get
                {
                    return _MatchWholeWord;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static LuceneSearcher Instance { get; private set; }

        private bool _IsSearching;
        public bool IsSearching
        {
            get
            {
                return _IsSearching;
            }
            private set
            {
                _IsSearching = value;
                FirePropertyChanged("IsSearching");
            }
        }

        private static ApplicationViewModel ApplicationView
        {
            get
            {
                return ApplicationService.ApplicationView;
            }
        }

        private void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        static LuceneSearcher()
        {
            Instance = new LuceneSearcher();
        }

        /// <summary>
        /// TODO refactor
        /// </summary>
        public IEnumerable<SearchResultViewModel> Search(IndexViewModel indexDirectory,
            string searchString,
            bool matchCase,
            bool matchWholeWord,
            bool useWildcards,
            out Task<IEnumerable<SearchResultViewModel>> ongoingSearchTask,
            CancellationToken cancelToken,
            IEnumerable<string> fileFilters = null)
        {
            ongoingSearchTask = null;

            if (!LuceneHelper.IsValidIndexDirectory(indexDirectory.IndexDirectory) || string.IsNullOrWhiteSpace(searchString))
                return Enumerable.Empty<SearchResultViewModel>();

            List<SearchResultViewModel> results = new List<SearchResultViewModel>();
            IsSearching = true;

            try
            {
                using (var reader = IndexReader.Open(FSDirectory.Open(indexDirectory.IndexDirectory), true))
                using (var searcher = new IndexSearcher(reader))
                {
                    BooleanQuery resultQuery = new BooleanQuery();
                    if (matchWholeWord)
                        resultQuery.Add(new BooleanClause(BuildMatchWholeWordContentQuery(searchString, matchCase, useWildcards), Occur.MUST));
                    else
                        resultQuery.Add(new BooleanClause(BuildMatchAnywhereQuery(reader, searchString, matchCase, useWildcards), Occur.MUST));

                    //TODO use this instead of manual filter.
                    //Doesn't work now because recursive booleanQuery doesn't work and can't say to match one of following filters.

                    //Add fileFilter query
                    //if (fileFilters != null && fileFilters.Any())
                    //{
                    //    foreach (var query in BuildFileFilterQueries(fileFilters))
                    //        resultQuery.Add(new BooleanClause(query, Occur.MUST));
                    //}

                    //Add blacklist query
                    var blacklist = Settings.CodeIDXSettings.Blacklist.BlacklistDirectories;
                    if (ApplicationView.UserSettings.IsBlacklistEnabled)
                    {
                        foreach (var curClause in BuildBlacklistQueryClauses(blacklist))
                            resultQuery.Add(curClause);
                    }

                    Sort sort = new Sort(new SortField[]
                    {
                        new SortField(Constants.IndexFields.Directory, SortField.STRING),
                        new SortField(Constants.IndexFields.Filename, SortField.STRING),
                        new SortField(Constants.IndexFields.Extension, SortField.STRING)
                    });

                    TopFieldDocs resultCollector = searcher.Search(resultQuery, null, Int32.MaxValue, sort);

                    string adjustedSearchString = matchCase ? searchString : searchString.ToLower();
                    IEnumerable<string> patternParts = null;
                    if (useWildcards)
                        patternParts = GetWildcardPatternParts(adjustedSearchString);

                    //kein Parallel.Foreach verwenden!
                    //durch die grosse Anzahl der Threads die erstellt und verworfen werden ist die Performance sehr schlecht!

                    int lastMatchIndex = 0;
                    foreach (var match in resultCollector.ScoreDocs)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return results;

                        var curDoc = reader.Document(match.Doc);
                        string docDirectory = curDoc.Get(Constants.IndexFields.Directory);
                        string docFilename = curDoc.Get(Constants.IndexFields.Filename);
                        string docExtension = curDoc.Get(Constants.IndexFields.Extension);
                        string documentFilename = Path.Combine(docDirectory, docFilename) + docExtension;

                        if (fileFilters != null && !fileFilters.Contains(documentFilename))
                            continue;

                        IEnumerable<LineMatch> matchingLines = GetMatchingLines(new GetMatchingLinesArgs(documentFilename, adjustedSearchString, patternParts, matchCase, matchWholeWord));

                        bool isFirst = true;
                        foreach (var lineMatch in matchingLines)
                        {
                            results.Add(new SearchResultViewModel
                            (
                                isFirst,
                                docDirectory,
                                docFilename,
                                docExtension,
                                lineMatch.LineNumber,
                                lineMatch.Line,
                                lineMatch.Highlights
                            ));

                            isFirst = false;
                        }

                        if (results.Count >= CodeIDXSettings.Search.PageSize)
                        {
                            var docNumbers = resultCollector.ScoreDocs.Select(cur => cur.Doc).ToList();
                            ongoingSearchTask = Task.Run<IEnumerable<SearchResultViewModel>>(() => GetRemainingLazyDocuments(indexDirectory,
                                                                                                                           docNumbers,
                                                                                                                           lastMatchIndex,
                                                                                                                           adjustedSearchString,
                                                                                                                           patternParts,
                                                                                                                           matchCase,
                                                                                                                           matchWholeWord,
                                                                                                                           cancelToken,
                                                                                                                           fileFilters));

                            return results;
                        }

                        lastMatchIndex++;
                    }
                }
            }
            catch { }
            finally
            {
                if (ongoingSearchTask == null)
                    IsSearching = false;
            }

            return results;
        }

        private IEnumerable<SearchResultViewModel> GetRemainingLazyDocuments(IndexViewModel indexDirectory,
            IEnumerable<int> documentNumbers,
            int lastMatchIndex,
            string adjustedSearchString,
            IEnumerable<string> patternParts,
            bool matchCase,
            bool matchWholeWord,
            CancellationToken cancelToken,
            IEnumerable<string> fileFilters = null)
        {
            try
            {
                List<SearchResultViewModel> results = new List<SearchResultViewModel>();
                using (var subReader = IndexReader.Open(FSDirectory.Open(indexDirectory.IndexDirectory), true))
                {
                    for (int i = lastMatchIndex + 1; i < documentNumbers.Count(); i++)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return results;

                        var subCurDoc = subReader.Document(documentNumbers.ElementAtOrDefault(i));
                        string subDocDirectory = subCurDoc.Get(Constants.IndexFields.Directory);
                        string subDocFilename = subCurDoc.Get(Constants.IndexFields.Filename);
                        string subDocExtension = subCurDoc.Get(Constants.IndexFields.Extension);
                        string subDocumentFilename = Path.Combine(subDocDirectory, subDocFilename) + subDocExtension;
                        if (fileFilters != null && !fileFilters.Contains(subDocumentFilename))
                            continue;

                        IEnumerable<LineMatch> subMatchingLines = GetMatchingLines(new GetMatchingLinesArgs(subDocumentFilename, adjustedSearchString, patternParts, matchCase, matchWholeWord));
                        foreach (var lineMatch in subMatchingLines)
                        {
                            results.Add(new SearchResultViewModel(false, subDocDirectory, subDocFilename, subDocExtension, lineMatch.LineNumber, lineMatch.Line, lineMatch.Highlights));
                        }
                    }

                }

                return results;
            }
            finally
            {
                IsSearching = false;
            }
        }

        private IEnumerable<LineMatch> GetMatchingLines(GetMatchingLinesArgs args)
        {
            if (!File.Exists(args.File))
                return Enumerable.Empty<LineMatch>();

            List<LineMatch> matchingLines = new List<LineMatch>();

            using (FileStream fs = new FileStream(args.File, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                string curLine;
                int lineNumber = 1;
                while ((curLine = reader.ReadLine()) != null)
                {
                    IEnumerable<HighlightInfo> lineHighlights = GetLineHighlights(curLine, args);
                    if (lineHighlights.Any())
                    {
                        matchingLines.Add(new LineMatch
                        {
                            Highlights = lineHighlights,
                            Line = curLine,
                            LineNumber = lineNumber
                        });
                    }

                    lineNumber++;
                }
            }

            return matchingLines;
        }

        private bool GetIsValidWholeWordMatch(GetMatchingLinesArgs args, string adjustedSearchLine, HighlightInfo match)
        {
            bool previousCharacterMatters = !LuceneHelper.IsValidTokenBreakCharactor(args.SearchString.FirstOrDefault());
            char previousCharacter = adjustedSearchLine.ElementAtOrDefault(match.StartIndex - 1);

            bool nextCharacterMatters = !LuceneHelper.IsValidTokenBreakCharactor(args.SearchString.LastOrDefault());
            char nextCharacter = adjustedSearchLine.ElementAtOrDefault(match.EndIndex);

            bool isValidWholeWordMatch = (!previousCharacterMatters || LuceneHelper.IsValidTokenBreakCharactor(previousCharacter)) &&
                (!nextCharacterMatters || LuceneHelper.IsValidTokenBreakCharactor(nextCharacter));

            return isValidWholeWordMatch;
        }

        /// <param name="useWildcards">supported wildcards: *</param>
        private IEnumerable<HighlightInfo> GetLineHighlights(string line, GetMatchingLinesArgs args)
        {
            string adjustedSearchLine = args.MatchCase ? line : line.ToLower();

            int lastMatchEndIndex = 0;
            int curMatchStartIndex;

            List<HighlightInfo> results = new List<HighlightInfo>();
            if (args.UseWildcards)
            {
                //supported wildcards: *

                int curPartStartIndex = 0;
                int lastPartEndIndex = 0;
                while (curPartStartIndex != -1)
                {
                    WildcardHighlightInfo curMatch = new WildcardHighlightInfo();
                    // look for all parts
                    foreach (var curPart in args.WildcardQueryParts)
                    {
                        curPartStartIndex = adjustedSearchLine.IndexOf(curPart, lastPartEndIndex, StringComparison.Ordinal);
                        if (curPartStartIndex == -1)
                            break;

                        curMatch.AddPartMatch(new HighlightInfo(curPartStartIndex, curPart.Length));
                        lastPartEndIndex = curPartStartIndex + curPart.Length;
                    }

                    if (curPartStartIndex != -1)
                    {
                        curMatch.StartIndex = adjustedSearchLine.IndexOf(args.WildcardQueryParts.First(), lastMatchEndIndex, StringComparison.Ordinal);
                        curMatch.EndIndex = curPartStartIndex + args.WildcardQueryParts.Last().Length;

                        results.Add(curMatch);
                        lastMatchEndIndex = curMatch.EndIndex;
                    }
                }
            }
            else
            {
                while ((curMatchStartIndex = adjustedSearchLine.IndexOf(args.SearchString, lastMatchEndIndex, StringComparison.Ordinal)) != -1)
                {
                    lastMatchEndIndex = curMatchStartIndex + args.SearchString.Length;
                    results.Add(new HighlightInfo(curMatchStartIndex, lastMatchEndIndex - curMatchStartIndex));
                }
            }

            if (args.MatchWholeWord)
            {
                return results.Where(match =>
                    {
                        if (match is WildcardHighlightInfo)
                        {
                            //check matchWholeWord for every wildcard query part
                            return ((WildcardHighlightInfo)match).PartHighlights.All(partMatch => GetIsValidWholeWordMatch(args, adjustedSearchLine, partMatch));
                        }
                        else
                        {
                            return GetIsValidWholeWordMatch(args, adjustedSearchLine, match);
                        }
                    });
            }

            return results;
        }

        private static IEnumerable<string> GetWildcardPatternParts(string wildcardPattern)
        {
            //TODO use regex
            int lastSplitEndIndex = 0;
            int curSplitStartIndex;

            List<string> parts = new List<string>();
            while ((curSplitStartIndex = wildcardPattern.IndexOf("*", lastSplitEndIndex, StringComparison.Ordinal)) != -1)
            {
                //TODO
                //skip if '*' is escaped, don't skip if '\' is escaped though
                //char previousChar = wildcardPattern.ElementAtOrDefault(curSplitStartIndex - 1);
                //char previousPrevChar = wildcardPattern.ElementAtOrDefault(curSplitStartIndex - 2);
                //if (previousChar == '\\' && previousPrevChar != '\\')
                //    continue;

                parts.Add(wildcardPattern.Substring(lastSplitEndIndex, curSplitStartIndex - lastSplitEndIndex).Trim());
                lastSplitEndIndex = curSplitStartIndex + 1;
            }

            parts.Add(wildcardPattern.Substring(lastSplitEndIndex).Trim());
            return parts.Where(cur => !string.IsNullOrEmpty(cur));
        }

        private IEnumerable<BooleanClause> BuildBlacklistQueryClauses(IEnumerable<string> blacklist)
        {
            if (blacklist == null || !blacklist.Any())
                return Enumerable.Empty<BooleanClause>();

            List<BooleanClause> clauses = new List<BooleanClause>();
            foreach (var cur in blacklist)
            {
                string adjustedString = LuceneHelper.ExpandTokenBreak(cur.ToLower()).TrimEnd(Path.DirectorySeparatorChar);
                clauses.Add(new BooleanClause(BuildMatchWholeWordQuery(Constants.IndexFields.AnalizedDirectory, adjustedString), Occur.MUST_NOT));
            }

            return clauses;
        }

        private Query BuildMatchWholeWordContentQuery(string searchString, bool matchCase, bool useWildcards)
        {
            if (useWildcards)
            {
                BooleanQuery resultQuery = new BooleanQuery();
                var patternParts = GetWildcardPatternParts(searchString);

                foreach (var part in patternParts)
                    resultQuery.Add(BuildMatchWholeWordContentQuery(LuceneHelper.ExpandTokenBreak(part), matchCase), Occur.MUST);

                return resultQuery;
            }
            else
                return BuildMatchWholeWordContentQuery(LuceneHelper.ExpandTokenBreak(searchString), matchCase);
        }

        private Query BuildMatchWholeWordContentQuery(string searchString, bool matchCase)
        {
            string adjustedSearchString = searchString;
            string fieldToSearch = Constants.IndexFields.Content;

            if (!matchCase)
            {
                fieldToSearch = Constants.IndexFields.ContentCaseInsensitive;
                adjustedSearchString = adjustedSearchString.ToLower();
            }

            return BuildMatchWholeWordQuery(fieldToSearch, adjustedSearchString);
        }

        private static Query BuildMatchWholeWordQuery(string fieldToSearch, string searchString)
        {
            var searchTerms = searchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            PhraseQuery contentQuery = new PhraseQuery();
            foreach (var curTerm in searchTerms)
                contentQuery.Add(new Term(fieldToSearch, curTerm));

            return contentQuery;
        }

        private IEnumerable<Query> BuildFileFilterQueries(IEnumerable<string> files)
        {
            List<Query> resultQueries = new List<Query>();
            foreach (var file in files)
            {
                BooleanQuery boolQuery = new BooleanQuery();
                if (string.IsNullOrEmpty(file))
                    continue;

                string directory = Path.GetDirectoryName(file);
                string filename = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);

                boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Directory, directory)), Occur.MUST));
                boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Filename, filename)), Occur.MUST));
                boolQuery.Add(new BooleanClause(new TermQuery(new Term(Constants.IndexFields.Extension, extension)), Occur.MUST));

                resultQueries.Add(boolQuery);
            }

            return resultQueries;
        }

        private Query BuildMatchAnywhereQuery(IndexReader indexReader, string searchString, bool matchCase, bool useWildcards)
        {
            if (useWildcards)
            {
                BooleanQuery resultQuery = new BooleanQuery();
                var patternParts = GetWildcardPatternParts(searchString);

                foreach (var part in patternParts)
                    resultQuery.Add(BuildMatchAnywhereQuery(indexReader, LuceneHelper.ExpandTokenBreak(part), matchCase), Occur.MUST);

                return resultQuery;
            }
            else
                return BuildMatchAnywhereQuery(indexReader, LuceneHelper.ExpandTokenBreak(searchString), matchCase);
        }

        private Query BuildMatchAnywhereQuery(IndexReader indexReader, string expandedSearchString, bool matchCase)
        {
            List<string> searchTerms = null;
            string adjustedSearchString = expandedSearchString;
            string fieldToSearch = Constants.IndexFields.Content;

            if (!matchCase)
            {
                fieldToSearch = Constants.IndexFields.ContentCaseInsensitive;
                adjustedSearchString = adjustedSearchString.ToLower();
            }

            searchTerms = adjustedSearchString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            bool onlyOneTerm = searchTerms.Count == 1;
            var firstTerm = searchTerms.FirstOrDefault();
            var lastTerm = searchTerms.LastOrDefault();

            Query contentQuery = null;
            if (onlyOneTerm)
            {
                bool isFirstTermPunctuation = (firstTerm.Length == 1 && LuceneHelper.IsPunctuation(firstTerm.First()));
                if (isFirstTermPunctuation)
                    contentQuery = new TermQuery(new Term(fieldToSearch, firstTerm));
                else
                    contentQuery = new WildcardQuery(new Term(fieldToSearch, "*" + firstTerm + "*"));
            }
            else
            {
                MultiPhraseQuery phraseQuery = new MultiPhraseQuery();

                List<Term> firstTermMatches = new List<Term>();
                List<Term> lastTermMatches = new List<Term>();
                CollectFirstAndLastTermMatches(indexReader, fieldToSearch, firstTermMatches, lastTermMatches, firstTerm, lastTerm);

                if (firstTermMatches.Count > 0)
                    phraseQuery.Add(firstTermMatches.ToArray());

                bool includeFirstTerm = firstTermMatches.Count == 0;
                bool includeLastTerm = lastTermMatches.Count == 0;

                int startIndex = includeFirstTerm ? 0 : 1;
                int endIndex = searchTerms.Count - (includeLastTerm ? 0 : 1);

                for (int i = startIndex; i < endIndex; i++)
                    phraseQuery.Add(new Term(fieldToSearch, searchTerms[i]));

                if (lastTermMatches.Count > 0)
                    phraseQuery.Add(lastTermMatches.ToArray());

                contentQuery = phraseQuery;
            }

            return contentQuery;
        }

        private static void CollectFirstAndLastTermMatches(
            IndexReader indexReader,
            string fieldToSearch,
            List<Term> firstTermMatches,
            List<Term> lastTermMatches,
            string firstTerm,
            string lastTerm)
        {
            bool isFirstTermPunctuation = (firstTerm.Length == 1 && LuceneHelper.IsPunctuation(firstTerm.First()));
            bool isLastTermPunctuation = (lastTerm.Length == 1 && LuceneHelper.IsPunctuation(lastTerm.First()));

            //punctuation characters are always by themselves, no need to check for terms starting/ending with one!
            if (isFirstTermPunctuation && isLastTermPunctuation)
                return;

            // !! reader.Terms(new Term(..., ...)) does not as expected, so check every term !!
            var termEnum = indexReader.Terms();

            HashSet<string> addedFirstTerms = new HashSet<string>();
            HashSet<string> addedLastTerms = new HashSet<string>();

            //Use actions for adding first/last terms
            //This will yield better performance!
            //Since there are thousands of terms in the index,
            //performing the string operations for first and last term on all of them,
            //even though we already know they're not going to match (because punctuations stand alone),
            //will waste a lot of time!
            Action<Term> addFirstTermAction = (curTerm) =>
                {
                    if (addedFirstTerms.Contains(curTerm.Text) || !curTerm.Text.EndsWith(firstTerm))
                        return;

                    firstTermMatches.Add(curTerm);
                    addedFirstTerms.Add(curTerm.Text);
                };

            Action<Term> addLastTermAction = (curTerm) =>
            {
                if (addedLastTerms.Contains(curTerm.Text) || !curTerm.Text.StartsWith(lastTerm))
                    return;

                lastTermMatches.Add(curTerm);
                addedLastTerms.Add(curTerm.Text);
            };

            Action<Term> collectTermsAction = null;
            if (!isFirstTermPunctuation)
                collectTermsAction += addFirstTermAction;
            if (!isLastTermPunctuation)
                collectTermsAction += addLastTermAction;

            while (termEnum.Next())
            {
                var curTerm = termEnum.Term;
                //skip wrong fields
                if (curTerm.Field != fieldToSearch)
                    continue;

                collectTermsAction(curTerm);
            }
        }

        internal IEnumerable<string> GetAvailableFileFilters(IndexViewModel indexDirectory)
        {
            if (indexDirectory == null)
                return Enumerable.Empty<string>();

            if (!LuceneHelper.IsValidIndexDirectory(indexDirectory.IndexDirectory))
                return Enumerable.Empty<string>();

            var availableExtensions = new List<string>();
            using (var reader = IndexReader.Open(FSDirectory.Open(indexDirectory.IndexDirectory), true))
            {
                var termEnum = reader.Terms();
                while (termEnum.Next())
                {
                    var curTerm = termEnum.Term;
                    if (curTerm.Field != Constants.IndexFields.Extension)
                        continue;

                    if (!availableExtensions.Contains(curTerm.Text))
                        availableExtensions.Add("*" + curTerm.Text);
                }
            }

            return availableExtensions;
        }

    }
}
