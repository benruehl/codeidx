using CodeIDX.Services;
using CodeIDX.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;

namespace CodeIDX.Views.Resources.Controls
{
    /// <summary>
    /// Interaction logic for EditableAvalonEditor.xaml
    /// </summary>
    public partial class EditableAvalonEditor : UserControl
    {
        private SearchPanel _PreviewSearchPanel;
        private ColorizeAvalonEdit _PreviewColorizer;

        private ApplicationViewModel ApplicationView
        {
            get
            {
                return ApplicationService.ApplicationView;
            }
        }

        public bool IsEditEnabled
        {
            get { return (bool)GetValue(IsEditEnabledProperty); }
            set { SetValue(IsEditEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEditEnabledProperty =
            DependencyProperty.Register("IsEditEnabled", typeof(bool), typeof(EditableAvalonEditor), new PropertyMetadata(false));

        public EditableAvalonEditor()
        {
            InitializeComponent();

            InitPreviewEditor();
            Loaded += EditableAvalonEditor_Loaded;
        }

        void EditableAvalonEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _PreviewSearchPanel = SearchPanel.Install(PreviewEditor);
        }

        private void InitPreviewEditor()
        {
            _PreviewColorizer = new ColorizeAvalonEdit();
            PreviewEditor.TextArea.TextView.LineTransformers.Add(_PreviewColorizer);

            PreviewEditor.TextArea.SelectionCornerRadius = 0;
            PreviewEditor.TextArea.SelectionBrush = new SolidColorBrush(Color.FromArgb(0xB1, 0x83, 0xCA, 0xEF));
            PreviewEditor.TextArea.SelectionBorder = new Pen(new SolidColorBrush(Color.FromArgb(0xB1, 0x00, 0x78, 0xF3)), 1);
            PreviewEditor.TextArea.SelectionForeground = Brushes.Black;
        }

        private void AvalonEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextEditor avalonEditor = sender as TextEditor;
            if (avalonEditor == null)
                return;

            if (avalonEditor.Document == null)
                avalonEditor.Document = new TextDocument();

            var searchResult = e.NewValue as SearchResultViewModel;
            if (searchResult == null)
            {
                avalonEditor.Document.Text = string.Empty;
            }
            else
            {
                string file = searchResult.GetFilePath();
                if (File.Exists(file))
                {
                    avalonEditor.Load(file);
                    avalonEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(searchResult.Extension));

                    avalonEditor.ScrollToLine(searchResult.LineNumber);

                    if (Settings.CodeIDXSettings.Results.SelectMatchInPreview)
                    {
                        int lastMatchStartIndex = GetLastMatchIndex(avalonEditor.Text,
                                                      searchResult.LineNumber,
                                                      ApplicationView.CurrentSearch.LastSearchText,
                                                      ApplicationView.CurrentSearch.MatchCase);
                        if (lastMatchStartIndex != -1)
                        {
                            int selectionLength = ApplicationView.CurrentSearch.LastSearchText.Length;
                            var matchingLine = avalonEditor.Document.GetLineByNumber(searchResult.LineNumber);
                            avalonEditor.Select(matchingLine.Offset + lastMatchStartIndex, selectionLength);
                        }
                    }

                    _PreviewSearchPanel.SearchPattern = ApplicationView.CurrentSearch.LastSearchText;
                }
            }
        }

        private int GetLastMatchIndex(string fileText, int lineNumber, string searchText, bool matchCase)
        {
            if (string.IsNullOrEmpty(fileText))
                return -1;

            string line = string.Empty;
            using (StringReader reader = new StringReader(fileText))
            {
                for (int i = 0; i < lineNumber - 1; i++)
                    reader.ReadLine();

                line = reader.ReadLine();
            }

            if (matchCase)
                return line.IndexOf(searchText, StringComparison.InvariantCulture);
            else
                return line.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ApplicationView.Status != StatusKind.Ready)
            {
                ApplicationView.SignalOperationInProgress();
                return;
            }

            var control = (EditableAvalonEditor)sender;
            var searchResult = control.DataContext as SearchResultViewModel;
            if (searchResult == null)
                return;

            try
            {
                control.PreviewEditor.Save(searchResult.GetFilePath());
                ApplicationView.SignalPreviewSaved();
            }
            catch
            {
                ApplicationView.SignalOperationCancelled();
            }
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var control = (EditableAvalonEditor)sender;
            e.CanExecute = control.IsEditEnabled &&
                control.DataContext is SearchResultViewModel;
        }
    }
}
