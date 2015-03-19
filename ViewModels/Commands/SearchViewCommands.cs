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

        public static void Init(MainWindow mainWindow)
        {
            RunSearchCommand = new RoutedCommand("RunSearch", typeof(MainWindow));

            mainWindow.CommandBindings.Add(new CommandBinding(RunSearchCommand,
                                                            SearchView_RunSearchCommand.Instance.ExecuteHandler,
                                                            SearchView_RunSearchCommand.Instance.CanExecuteHandler));
        }

    }
}
