using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using CodeIDX.Settings;
using CodeIDX.Services;

namespace CodeIDX.ViewModels
{
    public class UserSettingsViewModel : ViewModel
    {
        private const double MinPreviewHeight = 50;

        private double _LastPreviewHeight = 100;
        private ObservableCollection<RecentIndexSetting> _RecentIndicesInternal;
        public ReadOnlyObservableCollection<RecentIndexSetting> RecentIndices { get; private set; }

        public double LastPreviewHeight
        {
            get
            {
                return _LastPreviewHeight;
            }
            set
            {
                _LastPreviewHeight = value;
                if (_LastPreviewHeight < MinPreviewHeight)
                    _LastPreviewHeight = MinPreviewHeight;
            }
        }

        private bool _IsEditEnabled;
        public bool IsEditEnabled
        {
            get
            {
                return _IsEditEnabled;
            }
            set
            {
                _IsEditEnabled = value;
                FirePropertyChanged("IsEditEnabled");
            }
        }

        private bool _IsBlacklistEnabled;
        public bool IsBlacklistEnabled
        {
            get
            {
                return _IsBlacklistEnabled;
            }
            set
            {
                _IsBlacklistEnabled = value;
                FirePropertyChanged("IsBlacklistEnabled");
            }
        }

        private bool _IsPreviewVisible;
        public bool IsPreviewVisible
        {
            get
            {
                return _IsPreviewVisible;
            }
            set
            {
                _IsPreviewVisible = value;
                FirePropertyChanged("IsPreviewVisible");
            }
        }

        public UserSettingsViewModel()
        {
            _RecentIndicesInternal = new ObservableCollection<RecentIndexSetting>();
            RecentIndices = new ReadOnlyObservableCollection<RecentIndexSetting>(_RecentIndicesInternal);
        }

        public void Init()
        {
            foreach (var recentIndex in CodeIDXSettings.Default.RecentIndices)
            {
                if (!RecentIndices.Any(cur => cur.Name == recentIndex.Name && cur.IndexFile == recentIndex.IndexFile))
                    _RecentIndicesInternal.Add(recentIndex);
            }

            IsPreviewVisible = CodeIDXSettings.Default.IsPreviewVisible;
            LastPreviewHeight = CodeIDXSettings.Default.LastPreviewHeight;
            IsBlacklistEnabled = CodeIDXSettings.Default.IsBlacklistEnabled;
            IsEditEnabled = CodeIDXSettings.Default.IsEditEnabled;
        }

        public void AddRecentIndex(string name, string indexFile)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(indexFile))
                return;

            var matchingRecentIndex = RecentIndices.FirstOrDefault(cur => cur.IndexFile == indexFile);
            if (matchingRecentIndex != null)
            {
                _RecentIndicesInternal.Remove(matchingRecentIndex);
                _RecentIndicesInternal.Insert(0, matchingRecentIndex);
            }
            else
            {
                _RecentIndicesInternal.Insert(0, new RecentIndexSetting { Name = name, IndexFile = indexFile });
            }

            SaveInternal();
        }

        private void SaveInternal()
        {
            CodeIDXSettings.Default.RecentIndices = RecentIndices.ToList();
            CodeIDXSettings.Default.IsPreviewVisible = IsPreviewVisible;
            CodeIDXSettings.Default.LastPreviewHeight = LastPreviewHeight;
            CodeIDXSettings.Default.IsBlacklistEnabled = IsBlacklistEnabled;
            CodeIDXSettings.Default.IsEditEnabled = IsEditEnabled;

            CodeIDXSettings.SaveAll();
        }

        public void Save()
        {
            if (CodeIDXSettings.UserInterface.LoadLastSearches)
                UpdateRecentIndices();

            SaveInternal();
        }

        private void UpdateRecentIndices()
        {
            if (ApplicationService.ApplicationView.CurrentIndexFile == null)
                return;

            var openSearchTabs = ApplicationService.ApplicationView.Searches;
            if (openSearchTabs.Count == 0)
                return;

            var currentRecentIndexSetting = RecentIndices.FirstOrDefault(cur => cur.IndexFile == ApplicationService.ApplicationView.CurrentIndexFile.IndexFile);
            if (currentRecentIndexSetting == null)
                return;

            List<SearchTabSettings> tabSettings = new List<SearchTabSettings>();
            foreach (var tab in openSearchTabs)
            {
                if (string.IsNullOrEmpty(tab.LastSearchText))
                    continue;

                tabSettings.Add(new SearchTabSettings
                {
                    SearchText = tab.LastSearchText,
                    MatchCase = tab.MatchCase,
                    EnableWildcards = tab.EnableWildcards,
                    MatchWholeWord = tab.MatchWholeWord,
                    FileFilters = (tab.ActiveFileFilters.Any() ? tab.ActiveFileFilters.ToList() : null)
                });
            }

            currentRecentIndexSetting.SearchTabs = tabSettings;
        }

        internal void RemoveRecentIndex(string indexFile)
        {
            if (string.IsNullOrEmpty(indexFile))
                return;

            var indexToRemove = RecentIndices.FirstOrDefault(cur => cur.IndexFile == indexFile);
            if (indexToRemove != null)
            {
                _RecentIndicesInternal.Remove(indexToRemove);
                Save();
            }
        }

    }
}
