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
    public class SearchView_LoadAllLazyResultsCommand : ViewModelCommand<SearchViewModel>
    {

        public static SearchView_LoadAllLazyResultsCommand Instance = new SearchView_LoadAllLazyResultsCommand();

        protected override void Execute(SearchViewModel contextViewModel)
        {
            contextViewModel.LoadAllLazyResults();
        }

        protected override bool CanExecute(SearchViewModel contextViewModel)
        {
            return contextViewModel != null;
        }

    }
}
