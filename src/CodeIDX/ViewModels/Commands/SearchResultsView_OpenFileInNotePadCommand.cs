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
            OpenInNotepad(contextViewModel.GetFilePath(), contextViewModel.LineNumber);
        }

        public static bool OpenInNotepad(string file, int line)
        {
            try
            {
                string extension = Path.GetExtension(file);

                //try to open notepad and go to matching line
                string arguments = string.Format("\"{0}\" -n{1}", file, line);
                //notepad++ doesn't support xaml highlighting, xml does the trick
                if (extension == ".xaml")
                    arguments += " -lxml";

                Process.Start("notepad++.exe", arguments);
                return true;
            }
            catch
            {
                try
                {
                    Process.Start("notepad.exe", file);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
