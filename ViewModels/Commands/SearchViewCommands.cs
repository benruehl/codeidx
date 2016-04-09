using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeIDX.ViewModels.Commands
{
    public static class SearchViewCommands
    {
        public static RoutedCommand RunSearchCommand;
        public static RoutedCommand LoadNextLazyResultsCommand;
        public static RoutedCommand LoadAllLazyResultsCommand;

        public static void Init(MainWindow mainWindow)
        {
            RunSearchCommand = new RoutedCommand("RunSearch", typeof(MainWindow));
            LoadNextLazyResultsCommand = new RoutedCommand("LoadNextLazyResults", typeof(MainWindow));
            LoadAllLazyResultsCommand = new RoutedCommand("LoadAllLazyResults", typeof(MainWindow));

            mainWindow.CommandBindings.Add(new CommandBinding(RunSearchCommand,
                                                            SearchView_RunSearchCommand.Instance.ExecuteHandler,
                                                            SearchView_RunSearchCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(LoadNextLazyResultsCommand,
                                                            SearchView_LoadNextLazyResultsCommand.Instance.ExecuteHandler,
                                                            SearchView_LoadNextLazyResultsCommand.Instance.CanExecuteHandler));
            mainWindow.CommandBindings.Add(new CommandBinding(LoadAllLazyResultsCommand,
                                                            SearchView_LoadAllLazyResultsCommand.Instance.ExecuteHandler,
                                                            SearchView_LoadAllLazyResultsCommand.Instance.CanExecuteHandler));
        }

    }
}
