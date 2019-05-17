using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CodeIDX.ViewModels.Commands
{
    public static class ApplicationViewCommands
    {
        public static RoutedCommand CreateIndexCommand;

        public static void Init(MainWindow mainWindow)
        {
            CreateIndexCommand = new RoutedCommand("CreateIndex", typeof(MainWindow));

            mainWindow.CommandBindings.Add(new CommandBinding(CreateIndexCommand,
                                                            ApplicationView_CreateIndexCommand.Instance.ExecuteHandler,
                                                            ApplicationView_CreateIndexCommand.Instance.CanExecuteHandler));
        }

    }
}
