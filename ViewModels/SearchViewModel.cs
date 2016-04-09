using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.Services.Lucene;
using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid;

namespace CodeIDX.ViewModels
{
    public class SearchViewModel : ViewModel, IDataErrorInfo
    {
        public event Action NewSearchResultsLoaded;

        private IEnumerable<SearchResultViewModel> _LazyResults;
        private bool _IsSearchingInResults;
        private ObservableCollection<string> _ActiveFileFilters = new ObservableCollection<string>();
        private const int FilterDelayInMilliseconds = 600;
        private List<string> _FixedResultFiles = new List<string>();

        /// <summary>
        /// TODO better name
        /// The files in which to search only when searching in results
        /// </summary>
        public IEnumerable<string> FixedResultFiles
        {
            get
            {
                return _FixedResultFiles.ToList();
            }
        }

        public bool IsSearchingInResults
        {
            get
            {
                return _IsSearchingInResults;
            }
            set
            {
                _IsSearchingInResults = value;
                UpdateFixedResultFiles();
            }
        }

        private void UpdateFixedResultFiles()
        {
            if (!IsSearchingInResults)
            {
                _FixedResultFiles.Clear();
            }
            else
            {
                var currentResultFiles = SearchResults.Select(cur => cur.GetFilePath()).Distinct();
                _FixedResultFiles.AddRange(currentResultFiles);
            }
        }

        public string DirectoryFilterExpression
        {
            get
            {
                return _DirectoryFilterExpression;
            }
            set
            {
                if (_DirectoryFilterExpression != value)
                {
                    _DirectoryFilterExpression = value;
                    UpdateResultFilterWithDelay();

                    FirePropertyChanged("DirectoryFilterExpression");
                }
            }
        }

        public string FileFilterExpression
        {
            get
            {
                return _FileFilterExpression;
            }
            set
            {
                if (_FileFilterExpression != value)
                {
                    _FileFilterExpression = value;
                    UpdateResultFilterWithDelay();

                    FirePropertyChanged("FileFilterExpression");
                }
            }
        }

        public bool IsFilterEnabled
        {
            get
            {
                return _IsFilterEnabled;
            }
            set
            {
                if (_IsFilterEnabled != value)
                {
                    _IsFilterEnabled = value;
                    RefreshFilter();

                    FirePropertyChanged("IsFilterEnabled");
                }
            }
        }

        public bool MatchWholeWord
        {
            get
            {
                return _MatchWholeWord;
            }
            set
            {
                if (_MatchWholeWord != value)
                {
                    _MatchWholeWord = value;
                    FirePropertyChanged("MatchWholeWord");
                }
            }
        }

        public bool MatchCase
        {
            get
            {
                return _MatchCase;
            }
            set
            {
                if (_MatchCase != value)
                {
                    _MatchCase = value;
                    FirePropertyChanged("MatchCase");
                }
            }
        }

        public bool EnableWildcards
        {
            get
            {
                return _EnableWildcards;
            }
            set
            {
                if (_EnableWildcards != value)
                {
                    _EnableWildcards = value;
                    FirePropertyChanged("EnableWildcards");
                }
            }
        }

        private DispatcherTimer _UpdateFilterTimer;

        private string _DirectoryFilterExpression = string.Empty;
        private string _FileFilterExpression = string.Empty;
        private bool _IsFilterEnabled = true;
        private SearchResultViewModel _SelectedResult;
        private bool _MatchCase;
        private bool _EnableWildcards;
        private bool _MatchWholeWord;
        private ObservableCollection<SearchResultViewModel> _SearchResultsInternal;
        private ReadOnlyCollection<string> _AvailableFileFilters;

        public CollectionViewSource SearchResultsView { get; private set; }

        public ReadOnlyObservableCollection<SearchResultViewModel> SearchResults { get; private set; }

        public ReadOnlyCollection<string> AvailableFileFilters
        {
            get
            {
                return _AvailableFileFilters;
            }
            private set
            {
                _AvailableFileFilters = value;
                FirePropertyChanged("AvailableFileFilters");
            }
        }

        public ObservableCollection<string> ActiveFileFilters
        {
            get
            {
                return _ActiveFileFilters;
            }
            set
            {
                if (value == null)
                    value = new ObservableCollection<string>();

                var validFilters = value.Where(cur => AvailableFileFilters.Contains(cur));
                _ActiveFileFilters = new ObservableCollection<string>(validFilters);

                RefreshFilter();
                FirePropertyChanged("ActiveFileFilters");
            }
        }

        public SearchResultViewModel SelectedResult
        {
            get
            {
                return _SelectedResult;
            }
            set
            {
                if (_SelectedResult != value)
                {
                    _SelectedResult = value;
                    FirePropertyChanged("SelectedResult");
                }
            }
        }

        private string _SearchText;
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                if (_SearchText != value)
                {
                    _SearchText = value;
                    FirePropertyChanged("SearchText");
                }
            }
        }

        private string _LastSearchText;
        public string LastSearchText
        {
            get
            {
                return _LastSearchText;
            }
            set
            {
                if (_LastSearchText != value)
                {
                    _LastSearchText = value;
                    FirePropertyChanged("LastSearchText");
                }
            }
        }

        private int _FileCount;
        public int FileCount
        {
            get
            {
                return _FileCount;
            }
            set
            {
                if (_FileCount != value)
                {
                    _FileCount = value;
                    FirePropertyChanged("FileCount");
                }
            }
        }

        public SearchViewModel()
        {
            AvailableFileFilters = new ReadOnlyCollection<string>(new List<string>());

            _SearchResultsInternal = new ObservableCollection<SearchResultViewModel>();
            SearchResults = new ReadOnlyObservableCollection<SearchResultViewModel>(_SearchResultsInternal);
            SearchResultsView = new CollectionViewSource
            {
                Source = SearchResults
            };

            _UpdateFilterTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(FilterDelayInMilliseconds)
            };

            _UpdateFilterTimer.Tick += (s, e) => RefreshFilter();
            Settings.CodeIDXSettings.Search.SettingChanging += Search_SettingChanging;
        }

        void Search_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            if (e.SettingName == "EnableFileFilter" && (bool)e.NewValue == false)
                FileFilterExpression = string.Empty;
            else if (e.SettingName == "EnableDirectoryFilter" && (bool)e.NewValue == false)
                DirectoryFilterExpression = string.Empty;
        }

        public void RunSearch(IndexViewModel index, string searchText, CancellationToken cancelToken, Action lazySearchFinishedAction)
        {
            LastSearchText = searchText;
            LazyResults = null;

            Task<IEnumerable<SearchResultViewModel>> ongoingSearchTask;
            var results = LuceneSearcher.Instance.Search(index, searchText, MatchCase, MatchWholeWord, EnableWildcards, out ongoingSearchTask, cancelToken);
            if (cancelToken.IsCancellationRequested)
                return;

            if (ongoingSearchTask != null)
            {
                ongoingSearchTask.ContinueWith(lateResultsTask =>
                    {
                        if (CodeIDXSettings.Search.LoadRemainingLazyResults)
                            AddItemsToResultsView(lateResultsTask.Result, false);
                        else
                            LazyResults = lateResultsTask.Result;
                    });
            }

            AddItemsToResultsView(results, true);
            CountFiles();
            if (lazySearchFinishedAction != null)
            {
                if (ongoingSearchTask != null)
                    ongoingSearchTask.ContinueWith(task => lazySearchFinishedAction());
                else
                    lazySearchFinishedAction();
            }
        }

        public void RunFileSearch(IndexViewModel index, string searchText, IEnumerable<string> files, CancellationToken cancelToken, Action lazySearchFinishedAction)
        {
            LastSearchText = searchText;

            Task<IEnumerable<SearchResultViewModel>> ongoingSearchTask;
            var results = LuceneSearcher.Instance.Search(index, searchText, MatchCase, MatchWholeWord, EnableWildcards, out ongoingSearchTask, CancellationToken.None, files);
            if (cancelToken.IsCancellationRequested)
                return;

            AddItemsToResultsView(results, true);
            if (ongoingSearchTask != null)
                ongoingSearchTask.ContinueWith(lateResultsTask => AddItemsToResultsView(lateResultsTask.Result, false));

            CountFiles();
            if (lazySearchFinishedAction != null)
            {
                if (ongoingSearchTask != null)
                    ongoingSearchTask.ContinueWith(task => lazySearchFinishedAction());
                else
                    lazySearchFinishedAction();
            }
        }

        private void Dispatch(Action action)
        {
            if (action != null)
                App.Current.Dispatcher.Invoke(action);
        }

        private void CountFiles()
        {
            Dispatch(() =>
                {
                    FileCount = SearchResultsView.View.OfType<SearchResultViewModel>().GroupBy(cur => cur.GetFilePath()).Count();
                });
        }

        public void UpdateAvailableFileFilters(IEnumerable<string> availableFileFilters)
        {
            if (availableFileFilters != null)
            {
                AvailableFileFilters = new ReadOnlyCollection<string>(availableFileFilters.ToList());
                if (!ActiveFileFilters.Any())
                    ActiveFileFilters = new ObservableCollection<string>(availableFileFilters);
            }
        }

        internal void FilterResults(string filter, FilterKind filterKind)
        {
            if (string.IsNullOrEmpty(filter))
                return;

            if (filterKind == FilterKind.LeaveDirectory)
            {
                //TODO use search -> faster
                foreach (var cur in _SearchResultsInternal.ToList())
                {
                    bool isDirectory = cur.Directory == filter;
                    bool isSubDirectory = cur.Directory.StartsWith(filter + Path.DirectorySeparatorChar);

                    if (!isDirectory && !isSubDirectory)
                        _SearchResultsInternal.Remove(cur);
                }
            }
            else if (filterKind == FilterKind.RemoveDirectory)
            {
                foreach (var cur in _SearchResultsInternal.ToList())
                {
                    bool isDirectory = cur.Directory == filter;
                    bool isSubDirectory = cur.Directory.StartsWith(filter + Path.DirectorySeparatorChar);

                    if (isDirectory || isSubDirectory)
                        _SearchResultsInternal.Remove(cur);
                }
            }
            else if (filterKind == FilterKind.LeaveFile)
            {
                //RunFileSearch(ApplicationViewModel.Instance.CurrentIndexFile, filter);

                var fileResults = SearchResults.Where(cur => cur.GetFilePath() == filter).ToList();
                _SearchResultsInternal.Clear();
                foreach (var match in fileResults)
                    _SearchResultsInternal.Add(match);
            }
            else if (filterKind == FilterKind.RemoveFile)
            {
                foreach (var cur in _SearchResultsInternal.ToList())
                {
                    bool isFile = cur.Directory == Path.GetDirectoryName(filter) &&
                        cur.Filename == Path.GetFileNameWithoutExtension(filter) &&
                        cur.Extension == Path.GetExtension(filter);

                    if (isFile)
                        _SearchResultsInternal.Remove(cur);
                }
            }

            CountFiles();
        }

        internal void Reset()
        {
            LastSearchText = string.Empty;
            SearchText = string.Empty;
            FileFilterExpression = string.Empty;
            DirectoryFilterExpression = string.Empty;
            MatchCase = false;
            MatchWholeWord = false;
            FileCount = 0;

            if (_SearchResultsInternal != null)
                _SearchResultsInternal.Clear();

            ActiveFileFilters = new ObservableCollection<string>(AvailableFileFilters);
        }

        private void UpdateResultFilterWithDelay()
        {
            //if already running, reset
            if (_UpdateFilterTimer.IsEnabled)
                _UpdateFilterTimer.Stop();

            _UpdateFilterTimer.Start();
        }

        public void RefreshFilter()
        {
            _UpdateFilterTimer.Stop();

            //update filter
            if (!IsFilterEnabled)
            {
                SearchResultsView.View.Filter = null;
            }
            else if (SearchResultsView.View.Filter == null)
            {
                SearchResultsView.View.Filter = ItemFilter;
            }
            else
            {
                //coming in here, when changing an existing filter
                SearchResultsView.View.Refresh();
            }

            CountFiles();
        }

        private bool ItemFilter(object item)
        {
            var searchResult = (SearchResultViewModel)item;
            try
            {
                string regexFileFriendlyFilter = RegexHelper.WildcardToRegex(FileFilterExpression);
                string regexDirectoryFriendlyFilter = RegexHelper.WildcardToRegex(DirectoryFilterExpression);

                bool isFileFilterValid = string.IsNullOrEmpty(FileFilterExpression) || RegexHelper.IsValidRegex(RegexHelper.WildcardToRegex(FileFilterExpression));
                bool isDirectoryFilterValid = string.IsNullOrEmpty(DirectoryFilterExpression) || RegexHelper.IsValidRegex(RegexHelper.WildcardToRegex(DirectoryFilterExpression));

                bool hasValidExtension = !ActiveFileFilters.Any() || ActiveFileFilters.Any(cur => cur.TrimStart('*') == searchResult.Extension);
                return hasValidExtension &&
                    (!isFileFilterValid || Regex.IsMatch(searchResult.Filename, regexFileFriendlyFilter, RegexOptions.IgnoreCase)) &&
                    (!isDirectoryFilterValid || Regex.IsMatch(searchResult.Directory, regexDirectoryFriendlyFilter, RegexOptions.IgnoreCase));
            }
            catch
            {
                return true;
            }
        }

        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "FilterExpression")
                {
                    if (!RegexHelper.IsValidRegex(RegexHelper.WildcardToRegex(FileFilterExpression)))
                        return "Invalid filter expression";
                }

                return string.Empty;
            }
        }


        internal void SetActiveFiletypeFilters(IEnumerable<string> filetypeFilters)
        {
            ActiveFileFilters = new ObservableCollection<string>(filetypeFilters);
        }

        public double VerticalScrollPosition { get; set; }

        private IEnumerable<SearchResultViewModel> LazyResults
        {
            get
            {
                return _LazyResults;
            }
            set
            {
                _LazyResults = value;
                FirePropertyChanged("HasLazyResults");
                FirePropertyChanged("LazyResultsCount");
            }
        }

        public int LazyResultsCount
        {
            get
            {
                return LazyResults == null ? 0 : LazyResults.Count();
            }
        }
        public bool HasLazyResults
        {
            get { return LazyResults != null && LazyResults.Any(); }
        }

        internal void LoadNextLazyResults()
        {
            if (!HasLazyResults)
                return;

            int pageSize = CodeIDXSettings.Search.PageSize;
            AddItemsToResultsView(LazyResults.Take(pageSize), false);
            LazyResults = LazyResults.Skip(pageSize);
            if (!LazyResults.Any())
                LazyResults = null;

            CountFiles();
        }

        private void AddItemsToResultsView(IEnumerable<SearchResultViewModel> items, bool clearItems)
        {
            Dispatch(() =>
            {
                //filter all items for much better performance when adding items
                var lastFilter = SearchResultsView.View.Filter;
                SearchResultsView.View.Filter = FilterAll;

                try
                {
                    using (SearchResultsView.DeferRefresh())
                    {
                        if (clearItems)
                        {
                            if (NewSearchResultsLoaded != null)
                                NewSearchResultsLoaded();

                            _SearchResultsInternal.Clear();
                        }

                        foreach (var item in items)
                        {
                            item.Parent = this;
                            _SearchResultsInternal.Add(item);
                        }
                    }
                }
                finally
                {
                    SearchResultsView.View.Filter = lastFilter;
                }
            });
        }

        private bool FilterAll(object item)
        {
            return false;
        }

        internal void LoadAllLazyResults()
        {
            if (!HasLazyResults)
                return;

            AddItemsToResultsView(LazyResults, false);
            LazyResults = null;
            CountFiles();
        }

    }
}
