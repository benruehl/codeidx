using CodeIDX.Services.Lucene;
using CodeIDX.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.Services
{
    public class FileWatcherService
    {

        private bool _IsEnabled;
        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            set
            {
                if (_IsEnabled != value)
                {
                    _IsEnabled = value;

                    //update watchers
                    foreach (var watcher in _watchers)
                        watcher.EnableRaisingEvents = _IsEnabled;
                }
            }
        }

        private ApplicationViewModel ApplicationView
        {
            get { return ApplicationService.ApplicationView; }
        }

        public FileWatcherService()
        {
            IsEnabled = true;
        }

        public void WatchDirectories(IEnumerable<string> directories)
        {
            if (directories == null)
                return;

            DisableWatchers();
            foreach (var dir in directories)
                WatchDirectory(dir);
        }

        private void WatchDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return;

            //init watcher
            var watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.Renamed += Watcher_Renamed;

            _watchers.Add(watcher);
        }

        private void DisableWatchers()
        {
            if (_watchers.Count == 0)
                return;

            foreach (var watch in _watchers)
            {
                watch.EnableRaisingEvents = false;
                watch.Dispose();
            }

            _watchers.Clear();
        }

        void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            LuceneIndexer.Instance.DeleteDocument(e.OldFullPath, ApplicationView.CurrentIndexFile);
            LuceneIndexer.Instance.DeleteDocumentDirectory(e.OldFullPath, ApplicationView.CurrentIndexFile);

            LuceneIndexer.Instance.AddDocument(e.FullPath, ApplicationView.CurrentIndexFile);
            LuceneIndexer.Instance.AddDocumentDirectory(e.FullPath, ApplicationView.CurrentIndexFile);
        }

        void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            LuceneIndexer.Instance.DeleteDocument(e.FullPath, ApplicationView.CurrentIndexFile);
            LuceneIndexer.Instance.DeleteDocumentDirectory(e.FullPath, ApplicationView.CurrentIndexFile);
        }

        void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            LuceneIndexer.Instance.AddDocument(e.FullPath, ApplicationView.CurrentIndexFile);
        }

        void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            LuceneIndexer.Instance.UpdateDocument(e.FullPath, ApplicationView.CurrentIndexFile);
        }

    }
}
