using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Options
{
    public class OptionsDialogModel : ViewModel
    {

        public GeneralOptionsViewModel General { get; private set; }
        public ResultOptionsViewModel Results { get; private set; }
        public SearchOptionsViewModel Search { get; private set; }
        public BlacklistOptionsViewModel Blacklist { get; private set; }
        public UIOptionsViewModel UserInterface { get; private set; }
        public Type LastOptionsPageType { get; set; }

        public OptionsDialogModel()
        {
            General = new GeneralOptionsViewModel
            {
                ShowTrayIcon = CodeIDXSettings.General.ShowTrayIcon,
                ExitToTray = CodeIDXSettings.General.ExitToTray,
                MinimizeToTray = CodeIDXSettings.General.MinimizeToTray,
                SingleClickTray = CodeIDXSettings.General.SingleClickTray,
                StartOnSystemStartup = CodeIDXSettings.General.StartOnSystemStartup,
                LoadLastIndexOnStartup = CodeIDXSettings.General.LoadLastIndexOnStartup,
                RefreshIndexAtStartup = CodeIDXSettings.General.RefreshIndexAtStartup
            };

            Search = new SearchOptionsViewModel
            {
                EnableFilterByDefault = CodeIDXSettings.Search.EnableFilterByDefault,
                InsertTextFromClipBoard = CodeIDXSettings.Search.InsertTextFromClipBoard,
                EnableSearchHistory = CodeIDXSettings.Search.EnableSearchHistory,
                EnableSearchInResults = CodeIDXSettings.Search.EnableSearchInResults,
                EnableFileFilter = CodeIDXSettings.Search.EnableFileFilter,
                EnableDirectoryFilter = CodeIDXSettings.Search.EnableDirectoryFilter
            };

            Results = new ResultOptionsViewModel
            {
                SelectMatchInPreview = CodeIDXSettings.Results.SelectMatchInPreview,
                UseVisualStudioAsDefault = CodeIDXSettings.Results.UseVisualStudioAsDefault,
                EnableEditMatchOnDoubleClick = CodeIDXSettings.Results.EnableEditMatchOnDoubleClick,
                FilterFileOnEnter = CodeIDXSettings.Results.FilterFileOnEnter
            };

            Blacklist = new BlacklistOptionsViewModel
            {
                Directories = CodeIDXSettings.Blacklist.BlacklistDirectories
            };

            UserInterface = new UIOptionsViewModel
            {
                ShowResultFileCount = CodeIDXSettings.UserInterface.ShowResultFileCount,
                LoadLastSearches = CodeIDXSettings.UserInterface.LoadLastSearches
            };
        }

        public void SaveOptions()
        {
            //General
            CodeIDXSettings.General.ShowTrayIcon = General.ShowTrayIcon;
            CodeIDXSettings.General.ExitToTray = General.ExitToTray;
            CodeIDXSettings.General.MinimizeToTray = General.MinimizeToTray;
            CodeIDXSettings.General.SingleClickTray = General.SingleClickTray;
            CodeIDXSettings.General.StartOnSystemStartup = General.StartOnSystemStartup;
            CodeIDXSettings.General.LoadLastIndexOnStartup = General.LoadLastIndexOnStartup;
            CodeIDXSettings.General.RefreshIndexAtStartup = General.RefreshIndexAtStartup;

            //Search
            CodeIDXSettings.Search.EnableFilterByDefault = Search.EnableFilterByDefault;
            CodeIDXSettings.Search.InsertTextFromClipBoard = Search.InsertTextFromClipBoard;
            CodeIDXSettings.Search.EnableSearchHistory = Search.EnableSearchHistory;
            CodeIDXSettings.Search.EnableSearchInResults = Search.EnableSearchInResults;
            CodeIDXSettings.Search.EnableDirectoryFilter = Search.EnableDirectoryFilter;
            CodeIDXSettings.Search.EnableFileFilter = Search.EnableFileFilter;

            //Results
            CodeIDXSettings.Results.SelectMatchInPreview = Results.SelectMatchInPreview;
            CodeIDXSettings.Results.UseVisualStudioAsDefault = Results.UseVisualStudioAsDefault;
            CodeIDXSettings.Results.EnableEditMatchOnDoubleClick = Results.EnableEditMatchOnDoubleClick;
            CodeIDXSettings.Results.FilterFileOnEnter = Results.FilterFileOnEnter;

            //Blacklist
            CodeIDXSettings.Blacklist.BlacklistDirectories = Blacklist.Directories;

            //UserInterface
            CodeIDXSettings.UserInterface.ShowResultFileCount = UserInterface.ShowResultFileCount;
            CodeIDXSettings.UserInterface.LoadLastSearches = UserInterface.LoadLastSearches;

            CodeIDXSettings.SaveAll();
        }

    }
}
