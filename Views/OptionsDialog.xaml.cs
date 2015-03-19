using CodeIDX.ViewModels;
using CodeIDX.ViewModels.Options;
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

            //Loaded += OptionsDialog_Loaded;
            Closing += OptionsDialog_Closing;
        }

        void OptionsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //if (DialogModel.LastOptionsPageType == typeof(ResultOptionsViewModel))
            //    SelectedOptionsItem = DialogModel.Results;
            //else if (DialogModel.LastOptionsPageType == typeof(UIOptionsViewModel))
            //    SelectedOptionsItem = DialogModel.UserInterface;
            //else if (DialogModel.LastOptionsPageType == typeof(SearchOptionsViewModel))
            //    SelectedOptionsItem = DialogModel.Search;
            //else if (DialogModel.LastOptionsPageType == typeof(BlacklistOptionsViewModel))
            //    SelectedOptionsItem = DialogModel.Blacklist;
            //else
            //    SelectedOptionsItem = DialogModel.General;
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

    }
}
