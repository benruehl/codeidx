using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.ViewModels;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeIDX
{
    public class MainWindowCommandHandler
    {

        private ApplicationViewModel ApplicationView
        {
            get
            {
                return ApplicationService.ApplicationView;
            }
        }

        private MainWindow _MainWindow;
        private bool _IsToggleFiletypeFilterExecuting = false;

        public MainWindowCommandHandler(MainWindow mainWindow)
        {
            _MainWindow = mainWindow;
        }

        private string GetSingleLine(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (!text.Contains(Environment.NewLine))
                return text;

            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
                return string.Empty;
            if (lines.Length == 1)
                return lines.First();

            //return last non empty entry
            for (int i = lines.Length - 1; i > 0; i--)
            {
                var curLine = lines[i].Trim();
                if (!string.IsNullOrEmpty(curLine))
                    return curLine;
            }

            return string.Empty;
        }

        private string GetTextAtCursor()
        {
            var focusedElement = FocusManager.GetFocusedElement(_MainWindow) as DependencyObject;

            var focusedAutoTextBox = focusedElement as Xceed.Wpf.Toolkit.AutoSelectTextBox;
            if (focusedAutoTextBox != null && !string.IsNullOrEmpty(focusedAutoTextBox.SelectedText))
                return focusedAutoTextBox.SelectedText;

            var focusedTextBox = TreeHelper.FindVisualAncestor<TextEditor>(focusedElement);
            if (focusedTextBox != null && !string.IsNullOrEmpty(focusedTextBox.SelectedText))
                return focusedTextBox.SelectedText;

            return string.Empty;
        }

        internal void FocusSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string selectedText = GetTextAtCursor();
            if (!string.IsNullOrEmpty(selectedText))
                ApplicationView.CurrentSearch.SearchText = GetSingleLine(selectedText);

            _MainWindow.FocusSearchText();
        }

        internal void NewSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string selectedText = GetTextAtCursor();
            if (Settings.CodeIDXSettings.Search.InsertTextFromClipBoard &&
                string.IsNullOrEmpty(selectedText))
                selectedText = Clipboard.GetText();

            ApplicationView.AddSearch();
            ApplicationView.CurrentSearch.SearchText = GetSingleLine(selectedText);
            _MainWindow.FocusSearchText();
        }

        internal void TogglePreview_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_MainWindow.PreviewGridRow.ActualHeight == 0)
            {
                ApplicationView.UserSettings.IsPreviewVisible = true;
                _MainWindow.PreviewGridRow.Height = new GridLength(ApplicationView.UserSettings.LastPreviewHeight);
            }
            else
            {
                ApplicationView.UserSettings.IsPreviewVisible = false;
                _MainWindow.PreviewGridRow.Height = new GridLength(0);
            }
        }

        internal void ToggleFiletypeFilter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_IsToggleFiletypeFilterExecuting)
                return;

            _IsToggleFiletypeFilterExecuting = true;
            try
            {
                bool isCtrlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

                string clickedFiletype = e.Parameter.ToString();
                if (isCtrlPressed && isShiftPressed)
                {
                    IEnumerable<string> invertedFiletypeFilter = ApplicationView.CurrentSearch.AvailableFileFilters.Except(new[] { clickedFiletype });
                    ApplicationView.CurrentSearch.SetActiveFiletypeFilters(invertedFiletypeFilter);
                }
                else if (isCtrlPressed)
                    ApplicationView.CurrentSearch.SetActiveFiletypeFilters(new[] { clickedFiletype });

                ApplicationView.CurrentSearch.RefreshFilter();
            }
            finally
            {
                _IsToggleFiletypeFilterExecuting = false;
            }
        }

        private string RemoveInvalidFileNameChars(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return string.Empty;

            foreach (char curChar in Path.GetInvalidFileNameChars())
                filename = filename.Replace(curChar.ToString(), "");

            return filename;
        }

        internal void ExportResults_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".txt",
                Filter = "Text File (*.txt)|*.txt|All Files (*.*)|*.*",
                CheckPathExists = true,
                RestoreDirectory = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                FileName = RemoveInvalidFileNameChars(ApplicationView.CurrentSearch.LastSearchText)
            };

            if (saveDialog.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    //write info
                    writer.WriteLine(string.Format("Index: {0}", ApplicationView.CurrentIndexFile.IndexFile));
                    writer.WriteLine(string.Format("Search: {0}", ApplicationView.CurrentSearch.LastSearchText));
                    writer.WriteLine(string.Format("Results: {0}", ApplicationView.CurrentSearch.SearchResultsView.Count));

                    string lastFile = string.Empty;
                    foreach (var curResult in ApplicationView.CurrentSearch.SearchResultsView.OfType<SearchResultViewModel>())
                    {
                        string curFile = curResult.GetFilePath();
                        if (lastFile != curFile)
                        {
                            writer.WriteLine();
                        }

                        writer.Write(curFile);
                        writer.Write("\t");
                        writer.Write(curResult.LineNumber);
                        writer.Write(" - ");
                        writer.Write(curResult.MatchingLine);

                        writer.WriteLine();
                        lastFile = curFile;
                    }
                }
            }
        }

        internal void ExportResults_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ApplicationView.CurrentSearch != null &&
                ApplicationView.CurrentSearch.SearchResults.Count > 0;
        }
    }
}
