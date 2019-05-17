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
    public class SearchView_LoadNextLazyResultsCommand : ViewModelCommand<SearchViewModel>
    {

        public static SearchView_LoadNextLazyResultsCommand Instance = new SearchView_LoadNextLazyResultsCommand();

        protected override void Execute(SearchViewModel contextViewModel)
        {
            contextViewModel.LoadNextLazyResults();
        }

        protected override bool CanExecute(SearchViewModel contextViewModel)
        {
            return contextViewModel != null;
        }

    }
}
