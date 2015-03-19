using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeIDX.ViewModels.Commands
{
    public static class SearchResultViewCommands
    {
        public static RoutedCommand OpenLocationCommand;
        public static RoutedCommand OpenFileCommand;
        public static RoutedCommand OpenFileInNotepadCommand;
        public static RoutedCommand RemoveFileFromResultsCommand;
        public static RoutedCommand FilterFileCommand;
        public static RoutedCommand CopyFilepathCommand;

        public static void Init(MainWindow mainWindow)
        {
            OpenLocationCommand = new RoutedCommand("OpenLocation", typeof(MainWindow));
            OpenFileCommand = new RoutedCommand("OpenFile", typeof(MainWindow));
            OpenFileInNotepadCommand = new RoutedCommand("OpenFileInNotepad", typeof(MainWindow));
            RemoveFileFromResultsCommand = new RoutedCommand("RemoveFileFromResults", typeof(MainWindow));
            FilterFileCommand = new RoutedCommand("FilterFile", typeof(MainWindow));
            CopyFilepathCommand = new RoutedCommand("CopyFilepath", typeof(MainWindow));

            mainWindow.CommandBindings.Add(new CommandBinding(OpenLocationCommand,
                                                            SearchResultsView_OpenLocationCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_OpenLocationCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(OpenFileCommand,
                                                            SearchResultsView_OpenFileCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_OpenFileCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(OpenFileInNotepadCommand,
                                                            SearchResultsView_OpenFileInNotePadCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_OpenFileInNotePadCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(RemoveFileFromResultsCommand,
                                                            SearchResultsView_RemoveFileFromResultsCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_RemoveFileFromResultsCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(FilterFileCommand,
                                                            SearchResultsView_FilterFileCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_FilterFileCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(CopyFilepathCommand,
                                                            SearchResultsView_CopyFilepathCommand.Instance.ExecuteHandler,
                                                            SearchResultsView_CopyFilepathCommand.Instance.CanExecuteHandler));
        }

    }
}
