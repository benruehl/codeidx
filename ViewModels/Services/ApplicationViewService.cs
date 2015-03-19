using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.Services.Lucene;
using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CodeIDX.ViewModels.Services
{
    public static class ApplicationViewService
    {

        public static ApplicationViewModel ApplicationView
        {
            get
            {
                return ApplicationService.ApplicationView;
            }
        }

        public static void RunSearch(SearchViewModel searchView)
        {
            ApplicationView.AddToSearchHistory(searchView.SearchText);
            RunSearch(searchView, searchView.SearchText);
        }

        internal static string RunPreviousSearch(SearchViewModel searchView)
        {
            if (searchView == null)
                return string.Empty;

            string previousSearch = string.Empty;
            if (string.IsNullOrEmpty(searchView.LastSearchText))
                previousSearch = ApplicationView.SearchHistory.FirstOrDefault();
            else
            {
                int lastSearchIndex = Math.Max(0, ApplicationView.SearchHistory.IndexOf(searchView.LastSearchText));
                previousSearch = ApplicationView.SearchHistory.ElementAtOrDefault(lastSearchIndex + 1);
            }

            if (string.IsNullOrEmpty(previousSearch))
                return string.Empty;

            RunSearch(searchView, previousSearch);
            return previousSearch;
        }

        internal static string RunNextSearch(SearchViewModel searchView)
        {
            if (searchView == null)
                return string.Empty;

            if (string.IsNullOrEmpty(searchView.LastSearchText))
                return string.Empty;

            int lastSearchIndex = Math.Max(0, ApplicationView.SearchHistory.IndexOf(searchView.LastSearchText));
            string nextSearch = ApplicationView.SearchHistory.ElementAtOrDefault(lastSearchIndex - 1);

            if (string.IsNullOrEmpty(nextSearch))
                return string.Empty;

            RunSearch(searchView, nextSearch);
            return nextSearch;
        }

        public static void RunLastSearch(SearchViewModel searchView)
        {
            RunSearch(searchView, searchView.LastSearchText);
        }

        private static async void RunSearch(SearchViewModel searchView, string searchText)
        {
            if (string.IsNullOrEmpty(searchText) ||
                ApplicationView.CurrentIndexFile == null ||
                !LuceneHelper.IsValidIndexDirectory(ApplicationView.CurrentIndexFile.IndexDirectory))
            {
                return;
            }

            CancellationToken cancelToken;
            if (!ApplicationView.BeginOperation(StatusKind.Searching, out cancelToken))
                return;

            try
            {
                if (searchView.IsSearchingInResults)
                    await Task.Run(() => searchView.RunFileSearch(ApplicationView.CurrentIndexFile, searchText, searchView.FixedResultFiles, cancelToken), cancelToken);
                else
                    await Task.Run(() => searchView.RunSearch(ApplicationView.CurrentIndexFile, searchText, cancelToken), cancelToken);
            }
            finally
            {
                ApplicationView.EndOperation();
            }
        }

        public static async Task<bool> LoadIndex(string indexFile)
        {
            if (ApplicationView.CurrentIndexFile != null && ApplicationView.CurrentIndexFile.IndexFile == indexFile)
                return true;
            if (!ApplicationView.BeginOperation(StatusKind.Loading))
                return false;

            try
            {
                ApplicationView.CurrentIndexFile = IndexViewModel.Load(indexFile);
                if (ApplicationView.CurrentIndexFile == null)
                {
                    if (MessageBox.Show(indexFile + " not found.\n\nThe index file was not found.\nDo you want to remove it from the recent indices?", "Index file not found",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                    {
                        ApplicationView.UserSettings.RemoveRecentIndex(indexFile);
                    }

                    return false;
                }

                bool validIndexDirectoryFound = await Task.Run<bool>(() => LuceneHelper.IsValidIndexDirectory(ApplicationView.CurrentIndexFile.IndexDirectory));
                if (!validIndexDirectoryFound &&
                    MessageBox.Show(ApplicationView.CurrentIndexFile.IndexDirectory + " not found.\n\nThe index was moved or deleted.\nDo you want to select a new index directory?", "Index not found",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
                    {
                        SelectedPath = Path.GetDirectoryName(ApplicationView.CurrentIndexFile.IndexDirectory),
                        Description = "New Index Location",
                        UseDescriptionForTitle = true
                    };

                    if (openFolderDialog.ShowDialog() == true)
                    {
                        ApplicationView.CurrentIndexFile.IndexDirectory = openFolderDialog.SelectedPath;
                        ApplicationView.CurrentIndexFile.SaveIndexFile();
                        validIndexDirectoryFound = await Task.Run<bool>(() => LuceneHelper.IsValidIndexDirectory(openFolderDialog.SelectedPath));
                    }
                }

                if (!validIndexDirectoryFound)
                {
                    ApplicationView.CurrentIndexFile = null;
                    ApplicationView.UserSettings.RemoveRecentIndex(indexFile);
                    return false;
                }

                ApplicationView.UserSettings.AddRecentIndex(ApplicationView.CurrentIndexFile.Name, ApplicationView.CurrentIndexFile.IndexFile);
                ApplicationView.UpdateSearchFilter();

                ApplicationView.FileWatcher.WatchDirectories(ApplicationView.CurrentIndexFile.SourceDirectories);
            }
            finally
            {
                ApplicationView.EndOperation();
            }

            RefreshIndexAtStartup(ApplicationView.CurrentIndexFile);
            return true;
        }

        private static async void RefreshIndexAtStartup(IndexViewModel indexViewModel)
        {
            if (indexViewModel == null)
                return;
            if (CodeIDXSettings.General.RefreshIndexAtStartup == RefreshAtStartupKind.Never)
                return;

            if (CodeIDXSettings.General.RefreshIndexAtStartup == RefreshAtStartupKind.Always)
            {
                await UpdateIndex();
            }
            else if (CodeIDXSettings.General.RefreshIndexAtStartup == RefreshAtStartupKind.FirstStartup)
            {
                bool wasUpdatedToday = (ApplicationView.CurrentIndexFile.LastFullRefresh.Date == DateTime.Today);
                if (!wasUpdatedToday)
                    await UpdateIndex();
            }
        }

        public static async void LoadLastIndex()
        {
            if (CodeIDXSettings.General.LoadLastIndexOnStartup)
                await ApplicationViewService.LoadIndex(ApplicationView.UserSettings.RecentIndices.FirstOrDefault(), true);
        }

        /// <param name="silent">True, to load the index without messageBoxes in case anything fails</param>
        internal static async Task<bool> LoadIndex(RecentIndexSetting recentIndex, bool silent = false)
        {
            if (recentIndex == null)
                return false;

            if (silent)
            {
                if (!(await LoadIndexSilent(recentIndex.IndexFile)))
                    return false;
            }
            else if (!(await LoadIndex(recentIndex.IndexFile)))
            {
                return false;
            }

            if (CodeIDXSettings.UserInterface.LoadLastSearches && recentIndex.SearchTabs != null)
            {
                foreach (var tabSetting in recentIndex.SearchTabs)
                {
                    //skip if tab with search already exists
                    if (ApplicationView.Searches.Any(cur => cur.LastSearchText == tabSetting.SearchText || cur.SearchText == tabSetting.SearchText))
                        continue;

                    //use the current search, if it's empty
                    if ((!string.IsNullOrEmpty(ApplicationView.CurrentSearch.SearchText)) || (!string.IsNullOrEmpty(ApplicationView.CurrentSearch.LastSearchText)))
                        ApplicationView.AddSearch();

                    ApplicationView.CurrentSearch.SearchText = tabSetting.SearchText;
                    ApplicationView.CurrentSearch.MatchCase = tabSetting.MatchCase;
                    ApplicationView.CurrentSearch.EnableWildcards = tabSetting.EnableWildcards;
                    ApplicationView.CurrentSearch.MatchWholeWord = tabSetting.MatchWholeWord;
                    ApplicationView.CurrentSearch.SetActiveFiletypeFilters(tabSetting.FileFilters);
                }
            }

            //select first search
            ApplicationView.CurrentSearch = ApplicationView.Searches.FirstOrDefault();

            return true;
        }

        private static async Task<bool> LoadIndexSilent(string indexFile)
        {
            if (ApplicationView.CurrentIndexFile != null && ApplicationView.CurrentIndexFile.IndexFile == indexFile)
                return true;
            if (!ApplicationView.BeginOperation(StatusKind.Loading))
                return false;

            try
            {
                ApplicationView.CurrentIndexFile = IndexViewModel.Load(indexFile);
                if (ApplicationView.CurrentIndexFile == null)
                    return false;

                bool validIndexDirectoryFound = await Task.Run<bool>(() => LuceneHelper.IsValidIndexDirectory(ApplicationView.CurrentIndexFile.IndexDirectory));
                if (!validIndexDirectoryFound)
                {
                    ApplicationView.CurrentIndexFile = null;
                    return false;
                }

                ApplicationView.UpdateSearchFilter();
                ApplicationView.FileWatcher.WatchDirectories(ApplicationView.CurrentIndexFile.SourceDirectories);
            }
            finally
            {
                ApplicationView.EndOperation();
            }

            RefreshIndexAtStartup(ApplicationView.CurrentIndexFile);
            return true;
        }

        public static async Task UpdateIndex()
        {
            CancellationToken cancelToken;
            if (!ApplicationView.BeginOperation(StatusKind.Updating, out cancelToken))
                return;

            if (!ApplicationView.HasValidIndexDirectory)
                return;

            try
            {
                await Task.Run(() => LuceneIndexer.Instance.UpdateIndexDirectory(ApplicationView.CurrentIndexFile, cancelToken), cancelToken);
                ApplicationView.UpdateSearchFilter();

                ApplicationView.CurrentIndexFile.LastFullRefresh = DateTime.Now;
                ApplicationView.CurrentIndexFile.SaveIndexFile();
            }
            finally
            {
                ApplicationView.EndOperation();
            }
        }

    }
}
