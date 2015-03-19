using CodeIDX.Helpers;
using CodeIDX.Services;
using CodeIDX.Settings;
using CodeIDX.ViewModels;
using CodeIDX.ViewModels.Commands;
using CodeIDX.ViewModels.Options;
using CodeIDX.ViewModels.Services;
using CodeIDX.Views;
using Hardcodet.Wpf.TaskbarNotification;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Remotion.Linq.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Xceed.Wpf.DataGrid;

namespace CodeIDX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ApplicationViewModel ApplicationView
        {
            get
            {
                return ApplicationService.ApplicationView;
            }
        }

        //private AutoUpdater _AutoUpdater;
        private Point _DragDropStartPosition;
        private TaskbarIcon _TrayIcon;
        private Type _LastOptionsPageType;

        private MainWindowCommandHandler _CommandHandler;

        public static RoutedCommand FocusSearchCommand;
        public static RoutedCommand NewSearchCommand;
        public static RoutedCommand TogglePreviewCommand;
        public static RoutedCommand ExportResultsCommand;
        public static RoutedCommand ToggleFiletypeFilterCommand;

        public static bool GetIsSearchTextBox(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSearchTextBoxProperty);
        }

        public static void SetIsSearchTextBox(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSearchTextBoxProperty, value);
        }

        /// <summary>
        /// Used to get the searchTextBox of the current search tab
        /// </summary>
        public static readonly DependencyProperty IsSearchTextBoxProperty =
            DependencyProperty.RegisterAttached("IsSearchTextBox", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public static bool GetIsSearchResultsGrid(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSearchResultsGridProperty);
        }

        public static void SetIsSearchResultsGrid(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSearchResultsGridProperty, value);
        }

        /// <summary>
        /// Used to get the search results grid of the current search tab
        /// </summary>
        public static readonly DependencyProperty IsSearchResultsGridProperty =
            DependencyProperty.RegisterAttached("IsSearchResultsGrid", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

        public MainWindow()
        {
            _CommandHandler = new MainWindowCommandHandler(this);

            //Init commands
            FocusSearchCommand = new RoutedCommand("FocusSearch", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control) });
            NewSearchCommand = new RoutedCommand("NewSearch", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift) });
            TogglePreviewCommand = new RoutedCommand("TogglePreview", typeof(MainWindow), new InputGestureCollection() { new KeyGesture(Key.D, ModifierKeys.Control) });
            ToggleFiletypeFilterCommand = new RoutedCommand("ToggleFiletypeFilter", typeof(MainWindow));
            ExportResultsCommand = new RoutedCommand("ExportResults", typeof(MainWindow));

            CommandBindings.Add(new CommandBinding(FocusSearchCommand, _CommandHandler.FocusSearch_Executed));
            CommandBindings.Add(new CommandBinding(NewSearchCommand, _CommandHandler.NewSearch_Executed));
            CommandBindings.Add(new CommandBinding(TogglePreviewCommand, _CommandHandler.TogglePreview_Executed));
            CommandBindings.Add(new CommandBinding(ToggleFiletypeFilterCommand, _CommandHandler.ToggleFiletypeFilter_Executed));
            CommandBindings.Add(new CommandBinding(ExportResultsCommand, _CommandHandler.ExportResults_Executed, _CommandHandler.ExportResults_CanExecute));

            InitCommands();
            //InitAutoUpdater();

            //Init window
            DataContext = ApplicationView;
            InitializeComponent();

            InitTrayIcon();
            InitWindowSettings();
            InitPreviewSettings();

            CodeIDXSettings.General.SettingsSaving += General_SettingsSaving;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            Loaded += MainWindow_Loaded;
            //hide taskbarIcon
            StateChanged += MainWindow_StateChanged;
            //save user settings
            Closing += MainWindow_Closing;
        }

        private void InitAutoUpdater()
        {
            //_AutoUpdater = new AutoUpdater
            //{
            //    ConfigURL = "http://localhost/UpdateVersion.xml",
            //    LoginUserName = null,
            //    LoginUserPass = null,
            //    RestartWindow = null
            //};
        }

        private void InitCommands()
        {
            SearchResultViewCommands.Init(this);
            ApplicationViewCommands.Init(this);
            SearchViewCommands.Init(this);
        }

        private void InitPreviewSettings()
        {
            if (ApplicationView.UserSettings.IsPreviewVisible)
                PreviewGridRow.Height = new GridLength(ApplicationView.UserSettings.LastPreviewHeight);
        }

        private void HandleStartupStart()
        {
            string[] cmdArgs = Environment.GetCommandLineArgs();
            bool startMinimized = cmdArgs.Contains("/startMinimized");

            if (startMinimized)
            {
                WindowState = System.Windows.WindowState.Minimized;
                ShowInTaskbar = false;
            }
        }

        void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized &&
                (CodeIDXSettings.General.ShowTrayIcon && CodeIDXSettings.General.MinimizeToTray))
            {
                ShowInTaskbar = false;
            }

            if (WindowState != System.Windows.WindowState.Minimized)
                ShowInTaskbar = true;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CodeIDXSettings.General.ShowTrayIcon &&
                CodeIDXSettings.General.ExitToTray)
            {
                WindowState = System.Windows.WindowState.Minimized;
                ShowInTaskbar = false;
                e.Cancel = true;
            }

            ApplicationView.SaveSettings();
        }

        void General_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var newSettings = (GeneralSettings)sender;

            _TrayIcon.Visibility = newSettings.ShowTrayIcon ? Visibility.Visible : Visibility.Collapsed;

            if (!newSettings.ShowTrayIcon)
            {
                _TrayIcon.TrayMouseDoubleClick -= ActivateWindow;
                _TrayIcon.TrayLeftMouseDown -= ActivateWindow;
            }
            else if (newSettings.SingleClickTray)
            {
                _TrayIcon.TrayMouseDoubleClick -= ActivateWindow;

                _TrayIcon.TrayLeftMouseDown -= ActivateWindow;
                _TrayIcon.TrayLeftMouseDown += ActivateWindow;
            }
            else
            {
                _TrayIcon.TrayMouseDoubleClick -= ActivateWindow;
                _TrayIcon.TrayMouseDoubleClick += ActivateWindow;

                _TrayIcon.TrayLeftMouseDown -= ActivateWindow;
            }

            Win32Helper.SetStartOnStartup(newSettings.StartOnSystemStartup);
        }

        void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!ApplicationView.IsReady && e.Key == Key.Escape)
                ApplicationView.CancelCurrentOperation();
        }

        private void InitTrayIcon()
        {
            _TrayIcon = new TaskbarIcon();
            _TrayIcon.Icon = GetApplicationIcon();
            _TrayIcon.ToolTip = "CodeIDX";
            if (!CodeIDXSettings.General.ShowTrayIcon)
                _TrayIcon.Visibility = System.Windows.Visibility.Collapsed;

            //ContextMenu
            var trayContextMenu = new ContextMenu();
            var exitMenuItem = new MenuItem
            {
                Header = "Exit"
            };
            exitMenuItem.Click += (s, e) =>
                {
                    Closing -= MainWindow_Closing;
                    SaveWindowSettings();
                    CodeIDXSettings.SaveAll();
                    Close();
                };
            var showMenuItem = new MenuItem
            {
                Header = "Show CodeIDX"
            };
            showMenuItem.Click += ActivateWindow;
            var showOptionsItem = new MenuItem
            {
                Header = "Options …"
            };
            showOptionsItem.Click += ShowTrayOptions_Click;

            trayContextMenu.Items.Add(showMenuItem);
            trayContextMenu.Items.Add(new Separator());
            trayContextMenu.Items.Add(showOptionsItem);
            trayContextMenu.Items.Add(new Separator());
            trayContextMenu.Items.Add(exitMenuItem);
            _TrayIcon.ContextMenu = trayContextMenu;

            //Events
            if (CodeIDXSettings.General.SingleClickTray)
                _TrayIcon.TrayLeftMouseDown += ActivateWindow;
            else
                _TrayIcon.TrayMouseDoubleClick += ActivateWindow;
        }

        private void ShowTrayOptions_Click(object sender, RoutedEventArgs e)
        {
            ShowOptionsDialog();
        }

        private void ActivateWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                WindowState = System.Windows.WindowState.Normal;

            Activate();
        }

        private System.Drawing.Icon GetApplicationIcon()
        {
            try
            {
                return new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/CodeIDX;component/Views/Resources/Images/code.ico")).Stream);
            }
            catch
            {
                return System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationViewService.LoadLastIndex();
            HandleStartupStart();
            FocusSearchText();

            SingleInstanceService.InitWndProc(this);
        }

        internal void FocusSearchText()
        {
            var curSearchTextBox = TreeHelper.FindVisualChild<TextBox>(tcMain, (tb) => MainWindow.GetIsSearchTextBox(tb) == true);
            if (curSearchTextBox != null)
            {
                FocusManager.SetFocusedElement(this, curSearchTextBox);
                curSearchTextBox.SelectAll();
            }

            var curSearchComboBox = TreeHelper.FindVisualChild<ComboBox>(tcMain, (tb) => MainWindow.GetIsSearchTextBox(tb) == true);
            if (curSearchComboBox != null)
            {
                FocusManager.SetFocusedElement(this, curSearchComboBox);
                curSearchComboBox.Focus();
            }
        }

        internal void FocusSearchResults(bool selectFirstItem = false)
        {
            var curSearchResultsGrid = TreeHelper.FindVisualChild<DataGridControl>(tcMain, (tb) => MainWindow.GetIsSearchResultsGrid(tb) == true);
            if (curSearchResultsGrid != null)
            {
                if (selectFirstItem && curSearchResultsGrid.Items.Count > 0)
                    curSearchResultsGrid.SelectedIndex = 0;

                FocusManager.SetFocusedElement(this, curSearchResultsGrid);
            }
        }

        private void InitWindowSettings()
        {
            Point? location = CodeIDXSettings.Default.WindowLocation;
            Size size = CodeIDXSettings.Default.WindowSize;
            bool isMaximized = CodeIDXSettings.Default.IsWindowMaximized;

            if (location.HasValue)
            {
                Left = location.Value.X;
                Top = location.Value.Y;
                Width = size.Width;
                Height = size.Height;
            }

            SizeChanged += (s, e) => SaveWindowSettings();
            LocationChanged += (s, e) => SaveWindowSettings();
            StateChanged += (s, e) => SaveWindowSettings();

            if (isMaximized)
                Loaded += (s, e) => WindowState = System.Windows.WindowState.Maximized;
        }

        private void SaveWindowSettings()
        {
            //When minimized or just in tray, the sizes are all wrong
            if (WindowState == System.Windows.WindowState.Minimized)
                return;

            CodeIDXSettings.Default.WindowLocation = new Point(Left, Top);
            CodeIDXSettings.Default.WindowSize = new Size(Width, Height);
            CodeIDXSettings.Default.IsWindowMaximized = (WindowState == System.Windows.WindowState.Maximized);
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplicationViewService.RunSearch(ApplicationView.CurrentSearch);
            }
            if (e.Key == Key.Down)
            {
                ApplicationView.CurrentSearch.SelectedResult = ApplicationView.CurrentSearch.SearchResults.FirstOrDefault();
                FocusSearchResults(true);
                e.Handled = true;
            }
        }

        private async void UpdateIndex_Click(object sender, RoutedEventArgs e)
        {
            await ApplicationViewService.UpdateIndex();
        }

        private async void OpenIndex_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
             {
                 Filter = string.Format("Index file (*{0})|*{0}", Constants.IndexFileExtension)
             };

            if (fileDialog.ShowDialog() == true)
                await ApplicationViewService.LoadIndex(fileDialog.FileName);
        }

        private async void EditIndex_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.CurrentIndexFile == null)
                return;

            IndexDialog dialog = new IndexDialog
            {
                DataContext = ApplicationView.CurrentIndexFile,
                Owner = this,
                IsNew = false,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            if (dialog.ShowDialog() == true)
            {
                ApplicationView.CurrentIndexFile.SaveIndexFile();
                await ApplicationViewService.UpdateIndex();
            }
            else
            {
                await ApplicationViewService.LoadIndex(ApplicationView.CurrentIndexFile.IndexFile);
            }
        }

        private void SearchResults_LeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _DragDropStartPosition = e.GetPosition(null);
        }

        private void SearchResults_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _DragDropStartPosition - mousePos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var searchView = ((FrameworkElement)sender).DataContext as SearchViewModel;
                if (searchView == null || searchView.SelectedResult == null)
                    return;

                var draggedItem = searchView.SelectedResult;
                //exit if not over listViewItem, so scrolling still works
                if (TreeHelper.FindVisualAncestor<DataRow>(e.OriginalSource as DependencyObject) == null)
                    return;

                StringCollection fileList = new StringCollection();
                fileList.Add(draggedItem.GetFilePath());

                DataObject dragDropData = new DataObject();
                dragDropData.SetFileDropList(fileList);

                DragDrop.DoDragDrop(this, dragDropData, DragDropEffects.All);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog { Owner = this };
            aboutDialog.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Closing -= MainWindow_Closing;
            Close();
        }

        private void SearchResult_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //when trying to edit the matching line, disable the doubleClick command
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                return;
            //when currently editing matching line, abort
            if (TreeHelper.FindAncestor<Xceed.Wpf.Toolkit.AutoSelectTextBox>(e.OriginalSource as DependencyObject) != null)
                return;

            var selectedItem = ((FrameworkElement)sender).DataContext as SearchResultViewModel;
            if (selectedItem == null)
                return;

            try
            {
                string file = selectedItem.GetFilePath();
                if (!CodeIDXSettings.Results.UseVisualStudioAsDefault || !Win32Helper.OpenInVisualStudio(file, selectedItem.LineNumber))
                {
                    //use default application
                    Process.Start(file);
                }
            }
            catch { }
        }

        private void SearchTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            var searchViewModel = tabControl.SelectedItem as SearchViewModel;
            if (searchViewModel != null)
            {
                ApplicationView.CurrentSearch = searchViewModel;
            }
        }

        private void SearchResults_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton != MouseButtonState.Pressed)
                return;

            TabItem tabItem = TreeHelper.FindVisualAncestor<TabItem>(e.OriginalSource as DependencyObject);
            if (tabItem == null)
                return;

            var searchViewModel = tabItem.DataContext as SearchViewModel;
            if (searchViewModel == null)
                return;

            try
            {
                ApplicationView.RemoveSearch(searchViewModel);
            }
            catch
            {
                //Xceed.DataGridControl occasionally throws an exception here, ignore it
            }
        }

        private void CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationView.RemoveSearch(ApplicationView.CurrentSearch);
            }
            catch
            {
                //Xceed.DataGridControl occasionally throws an exception here, ignore it
            }
        }

        private async void LoadRecentIndex_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            var recentIndex = menuItem.DataContext as RecentIndexSetting;
            if (recentIndex == null)
                return;

            await ApplicationViewService.LoadIndex(recentIndex);
        }

        private void ShowOptionsDialog()
        {
            if (OwnedWindows.OfType<OptionsDialog>().Any())
                return;

            OptionsDialogModel optionsDialogModel = new OptionsDialogModel { LastOptionsPageType = _LastOptionsPageType };
            OptionsDialog dialog = new OptionsDialog
            {
                Owner = this,
                WindowStartupLocation = (WindowState == WindowState.Minimized ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner),
                DataContext = optionsDialogModel
            };

            if (dialog.ShowDialog() == true)
            {
                optionsDialogModel.SaveOptions();
                _LastOptionsPageType = optionsDialogModel.LastOptionsPageType;
            }
        }
        private void ShowOptions_Click(object sender, RoutedEventArgs e)
        {
            ShowOptionsDialog();
        }

        private void Result_KeyDown(object sender, KeyEventArgs e)
        {
            var dataGrid = (DataGridControl)sender;
            if (e.Key == Key.Delete)
            {
                //remove selected files
                var selectedResultFiles = dataGrid.SelectedItems.OfType<SearchResultViewModel>()
                    .Select(cur => cur.GetFilePath())
                    .Distinct();

                foreach (var delFile in selectedResultFiles.ToList())
                    ApplicationView.CurrentSearch.FilterResults(delFile, FilterKind.RemoveFile);
            }
            else if (e.Key == Key.Enter)
            {
                var searchResult = dataGrid.SelectedItem as SearchResultViewModel;
                if (searchResult == null)
                    return;

                if (CodeIDXSettings.Results.FilterFileOnEnter)
                {
                    //Filter selected file
                    string fileToFilter = searchResult.GetFilePath();
                    ApplicationView.CurrentSearch.FilterResults(fileToFilter, FilterKind.LeaveFile);
                }
                else
                {
                    try
                    {
                        string file = searchResult.GetFilePath();
                        if (!CodeIDXSettings.Results.UseVisualStudioAsDefault || !Win32Helper.OpenInVisualStudio(file, searchResult.LineNumber))
                        {
                            //use default application
                            Process.Start(file);
                        }
                    }
                    catch { }
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                //copy selected filename to clipboard
                var searchResult = dataGrid.SelectedItem as SearchResultViewModel;
                if (searchResult != null)
                    Clipboard.SetText(searchResult.GetFilePath());
            }
            else if (e.Key == Key.F5)
            {
                ApplicationViewService.RunLastSearch(ApplicationView.CurrentSearch);
            }
        }

        private void DataGridControl_CurrentChanged(object sender, DataGridCurrentChangedEventArgs e)
        {
            //SelectedResult must be set manually her!
            //On a tab switch, the dataGrid sets SelectedResult = null;
            //To change that the binding must be OneWay and the value set here.
            var dataGrid = sender as DataGridControl;
            if (dataGrid != null && ApplicationView.CurrentSearch != null)
                ApplicationView.CurrentSearch.SelectedResult = dataGrid.CurrentItem as SearchResultViewModel;
        }

        private void SearchResults_RightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedDataRow = TreeHelper.FindVisualAncestor<DataRow>(e.OriginalSource as DependencyObject);
            if (clickedDataRow != null && clickedDataRow.DataContext is SearchResultViewModel)
            {
                SearchResultViewModel clickedResult = (SearchResultViewModel)clickedDataRow.DataContext;
                ((DataGridControl)sender).CurrentItem = clickedResult;
                ApplicationView.CurrentSearch.SelectedResult = clickedResult;
            }
        }

        private void PreviewSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (e.VerticalChange == 0)
                return;

            ApplicationView.UserSettings.IsPreviewVisible = (PreviewGridRow.ActualHeight > 0);

            if (ApplicationView.UserSettings.IsPreviewVisible)
                ApplicationView.UserSettings.LastPreviewHeight = PreviewGridRow.ActualHeight;
            else
                ApplicationView.UserSettings.LastPreviewHeight = Math.Abs(e.VerticalChange);
        }

        private void PreviewSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ApplicationView.UserSettings.IsPreviewVisible)
            {
                //hide preview
                ApplicationView.UserSettings.LastPreviewHeight = PreviewGridRow.Height.Value;
                PreviewGridRow.Height = new GridLength(0);
                ApplicationView.UserSettings.IsPreviewVisible = false;
            }
            else
            {
                PreviewGridRow.Height = new GridLength(ApplicationView.UserSettings.LastPreviewHeight);
                ApplicationView.UserSettings.IsPreviewVisible = true;
            }
        }

        private void OpenChangelog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string readmePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ChangeLog.txt");
                if (File.Exists(readmePath))
                    Process.Start(readmePath);
            }
            catch { }
        }

        private void OpenReadme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string readmePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Readme.txt");
                if (File.Exists(readmePath))
                    Process.Start(readmePath);
            }
            catch { }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Mouse back button
            if (e.ChangedButton== MouseButton.XButton1)
            {
                string searchTerm = ApplicationViewService.RunPreviousSearch(ApplicationView.CurrentSearch);
                if (!string.IsNullOrEmpty(searchTerm))
                    ApplicationView.CurrentSearch.SearchText = searchTerm;
            }
            //Mouse forward button
            else if (e.ChangedButton == MouseButton.XButton2)
            {
                string searchTerm = ApplicationViewService.RunNextSearch(ApplicationView.CurrentSearch);
                if (!string.IsNullOrEmpty(searchTerm))
                    ApplicationView.CurrentSearch.SearchText = searchTerm;
            }
        }

    }
}
