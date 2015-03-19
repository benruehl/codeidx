using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeIDX.ViewModels.Commands
{
    public class SearchResultsView_OpenFileInNotePadCommand : ViewModelCommand<SearchResultViewModel>
    {

        public static SearchResultsView_OpenFileInNotePadCommand Instance = new SearchResultsView_OpenFileInNotePadCommand();

        protected override void Execute(SearchResultViewModel contextViewModel)
        {
            string file = contextViewModel.GetFilePath();
            try
            {
                //try to open notepad and go to matching line
                string arguments = string.Format("\"{0}\" -n{1}", file, contextViewModel.LineNumber);
                //notepad++ doesn't support xaml highlighting, xml does the trick
                if (contextViewModel.Extension == ".xaml")
                    arguments += " -lxml";

                Process.Start("notepad++.exe", arguments);
            }
            catch
            {
                Process.Start("notepad.exe", file);
            }
        }
    }
}
