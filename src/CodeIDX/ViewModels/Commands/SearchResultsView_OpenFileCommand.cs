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
            OpenFileInDefaultEditor(contextViewModel);
        }

        public static void OpenFileInDefaultEditor(SearchResultViewModel contextViewModel)
        {
            if (contextViewModel == null)
                return;

            try
            {
                string file = contextViewModel.GetFilePath();
                bool success = false;

                if (CodeIDXSettings.Results.UseCustomEditorAsDefault &&
                    !string.IsNullOrWhiteSpace(CodeIDXSettings.Results.DefaultEditorCommandLineOptions))
                {
                    string commandLineOptions = CodeIDXSettings.Results.DefaultEditorCommandLineOptions;
                    int firstWhitespaceIndex = commandLineOptions.IndexOf(" ");
                    if (firstWhitespaceIndex != -1)
                    {
                        string editor = commandLineOptions.Substring(0, firstWhitespaceIndex);
                        string arguments = commandLineOptions.Substring(firstWhitespaceIndex + 1).ToLower();

                        arguments = arguments.Replace("$file", "\"" + file + "\"")
                                             .Replace("$directory", contextViewModel.Directory)
                                             .Replace("$line", contextViewModel.LineNumber.ToString());

                        Process.Start(editor, arguments);
                        success = true;
                    }
                }
                else if (CodeIDXSettings.Results.UseVisualStudioAsDefault)
                {
                    success = Win32Helper.OpenInVisualStudio(file, contextViewModel.LineNumber);
                }
                else if (CodeIDXSettings.Results.UseNotepadAsDefault)
                {
                    success = SearchResultsView_OpenFileInNotePadCommand.OpenInNotepad(file, contextViewModel.LineNumber);
                }

                if (!success)
                {
                    //use default application
                    Process.Start(file);
                }
            }
            catch { }
        }

    }
}
