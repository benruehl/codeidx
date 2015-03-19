using CodeIDX.Helpers;
using CodeIDX.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_OpenFileCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_OpenFileCommand Instance = new SearchResultsView_OpenFileCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            try
            {
                string file = contextViewModel.GetFilePath();
                if (!CodeIDXSettings.Results.UseVisualStudioAsDefault || !Win32Helper.OpenInVisualStudio(file, contextViewModel.LineNumber))
                {
                    Process.Start(file);
                }
            }
            catch { }
        }
    }
}
