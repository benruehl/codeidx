using CodeIDX.Settings;
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
            Closing += IndexDialog_Closing;
            InitializeComponent();
        }

        void IndexDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CodeIDXSettings.SaveAll();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
                return;

            BindingOperations.GetBindingExpression(tbName, TextBox.TextProperty).UpdateSource();
            BindingOperations.GetBindingExpression(tbStorePath, TextBox.TextProperty).UpdateSource();
            BindingOperations.GetBindingExpression(tbSourceDirectories, TextBox.TextProperty).UpdateSource();
            BindingOperations.GetBindingExpression(tbFileFilters, TextBox.TextProperty).UpdateSource();

            DialogResult = true;
        }

        private bool Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(tbName.Text))
                error = "The name must not be empty.";
            else if (string.IsNullOrEmpty(tbStorePath.Text))
                error = "The location must not be empty.";
            else if (string.IsNullOrEmpty(tbSourceDirectories.Text))
                error = "The index sources must not be empty.";
            else
            {
                string storePath = tbStorePath.Text;
                //check if store path and any of the source directories overlap at any point
                List<string> sourceDirectories = tbSourceDirectories.Text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (sourceDirectories.Any(cur => storePath.Contains(cur)))
                    error = "The index location cannot be indexed.\nPlease verify the index sources.";
            }

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void ChooseStorePath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Select Index Location",
                UseDescriptionForTitle = true,
                SelectedPath = CodeIDXSettings.Default.LastIndexLocationPath
            };

            if (folderBrowser.ShowDialog() == true)
            {
                tbStorePath.Text = folderBrowser.SelectedPath;
                CodeIDXSettings.Default.LastIndexLocationPath = folderBrowser.SelectedPath;
            }
        }

        private void AddDirectory_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = false,
                SelectedPath = CodeIDXSettings.Default.LastIndexSourcePath
            };

            if (folderBrowser.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(tbSourceDirectories.Text))
                    tbSourceDirectories.Text += "\n";

                tbSourceDirectories.Text += folderBrowser.SelectedPath;
                CodeIDXSettings.Default.LastIndexSourcePath = folderBrowser.SelectedPath;
            }
        }
    }
}
