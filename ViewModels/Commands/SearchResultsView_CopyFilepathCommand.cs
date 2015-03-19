using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_CopyFilepathCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_CopyFilepathCommand Instance = new SearchResultsView_CopyFilepathCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            Clipboard.SetText(contextViewModel.GetFilePath());
        }
    }
}
