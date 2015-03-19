using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.ViewModels.Commands;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace CodeIDX
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            if(!SingleInstanceService.Start())
                Environment.Exit(0);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ErrorProvider.Instance.Init();
            ErrorProvider.Instance.LogInfo("Starting …");

            Settings.CodeIDXSettings.UpgradeAll();

            Exit += App_Exit;
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            SingleInstanceService.Stop();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorProvider.Instance.LogError(string.Empty, (Exception)e.ExceptionObject);
            MessageBox.Show("An error occured.\nSee the log for details.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

            SingleInstanceService.Stop();
            Environment.Exit(1);
        }

    }
}
