using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace CodeIDX.Views
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();

            tbDescription.Text = "Application for indexing source code using Lucene.NET";

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            tbProductName.Text = info.ProductName;
            tbVersion.Text = info.ProductVersion;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch { }
        }

    }
}
