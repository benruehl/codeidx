using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_OpenLocationCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_OpenLocationCommand Instance = new SearchResultsView_OpenLocationCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            try
            {
                string argument = string.Format(@"/select, {0}", contextViewModel.GetFilePath());
                Process.Start("explorer.exe", argument);
            }
            catch
            {
                Process.Start(contextViewModel.Directory);
            }
        }
    }
}
