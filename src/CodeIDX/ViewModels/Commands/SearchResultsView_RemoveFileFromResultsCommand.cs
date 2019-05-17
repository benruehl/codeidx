using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_RemoveFileFromResultsCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_RemoveFileFromResultsCommand Instance = new SearchResultsView_RemoveFileFromResultsCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            contextViewModel.Parent.FilterResults(contextViewModel.GetFilePath(), FilterKind.RemoveFile);
        }

        protected override bool CanExecute(SearchResultViewModel contextViewModel)
        {
            return contextViewModel != null &&
                contextViewModel.Parent != null;
        }
    }
}
