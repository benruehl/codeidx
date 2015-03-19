using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_FilterFileCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_FilterFileCommand Instance = new SearchResultsView_FilterFileCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            contextViewModel.Parent.FilterResults(contextViewModel.GetFilePath(), FilterKind.LeaveFile);
        }

        protected override bool CanExecute(SearchResultViewModel contextViewModel)
        {
            return contextViewModel != null &&
                contextViewModel.Parent != null;
        }
    }
}
