using CodeIDX.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchView_RunSearchCommand : ViewModelCommand<SearchViewModel>
    {

        public static SearchView_RunSearchCommand Instance = new SearchView_RunSearchCommand();

        protected override void Execute(SearchViewModel contextViewModel)
        {
            ApplicationViewService.RunSearch(contextViewModel);
        }

        protected override bool CanExecute(SearchViewModel contextViewModel)
        {
            return contextViewModel != null &&
                ApplicationViewService.ApplicationView.IsReady &&
                ApplicationViewService.ApplicationView.CurrentIndexFile != null &&
                !string.IsNullOrEmpty(contextViewModel.SearchText);
        }

    }
}
