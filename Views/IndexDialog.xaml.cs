using CodeIDX.ViewModels;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for IndexDialog.xaml
    /// </summary>
    public partial class IndexDialog : Window
    {

        private IndexViewModel DialogModel
        {
            get
            {
                return (IndexViewModel)DataContext;
            }
        }

        public bool IsNew { get; set; }

        public IndexDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ChooseStorePath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Select Index Location",
                UseDescriptionForTitle = true
            };

            if (folderBrowser.ShowDialog() == true)
            {
                DialogModel.StorePath = folderBrowser.SelectedPath;
            }
        }

        private void AddDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            if (folderBrowser.ShowDialog() == true)
            {
                DialogModel.AddSourceDirectory(folderBrowser.SelectedPath);
            }
        }
    }
}
