using CodeIDX.Services.Lucene;
using CodeIDX.Settings;
using CodeIDX.ViewModels;
using CodeIDX.ViewModels.Options;
using CodeIDX.ViewModels.Services;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodeIDX.Views
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {

        private OptionsDialogModel DialogModel
        {
            get
            {
                return (OptionsDialogModel)DataContext;
            }
        }

        public OptionsDialog()
        {
            InitializeComponent();

            Closing += OptionsDialog_Closing;
        }

        void OptionsDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var selectedTab = OptionsTree.SelectedItem as FrameworkElement;
            if (selectedTab != null)
                DialogModel.LastOptionsPageType = selectedTab.DataContext.GetType();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void AddBlacklistDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            if (folderBrowser.ShowDialog() == true)
            {
                DialogModel.Blacklist.AddDirectory(folderBrowser.SelectedPath);
            }
        }

        private void OptimizeNow_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => LuceneIndexer.Instance.OptimizeIndex(ApplicationViewService.ApplicationView.CurrentIndexFile));
        }

    }
}
