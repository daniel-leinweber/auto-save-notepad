using AutoSaveNotepad.Localization;
using AutoSaveNotepad.ViewModels;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace AutoSaveNotepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainWindowViewModel();
            _vm.RequestClose += this.Close;
            this.DataContext = _vm;

            this.SourceInitialized += MainWindow_SourceInitialized;
            this.Closing += MainWindow_Closing;

            CreateRecentFilesMenu();
        }

        private void CreateRecentFilesMenu()
        {
            _vm.LastSavedDocuments.Reverse().ToList().ForEach(filename =>
            {
                this.RecentFiles.Items.Add(new MenuItem { Header = filename, Command = _vm.OpenRecentFileCommand, CommandParameter = filename });
            });
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.Top = Properties.Settings.Default.Application_Top;
            this.Left = Properties.Settings.Default.Application_Left;
            this.Height = Properties.Settings.Default.Application_Height;
            this.Width = Properties.Settings.Default.Application_Width;

            if (Properties.Settings.Default.Application_Maximized == true)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {// Use the RestoreBounds when maximized
                Properties.Settings.Default.Application_Top = RestoreBounds.Top;
                Properties.Settings.Default.Application_Left = RestoreBounds.Left;
                Properties.Settings.Default.Application_Height = RestoreBounds.Height;
                Properties.Settings.Default.Application_Width = RestoreBounds.Width;
                Properties.Settings.Default.Application_Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Application_Top = this.Top;
                Properties.Settings.Default.Application_Left = this.Left;
                Properties.Settings.Default.Application_Height = this.Height;
                Properties.Settings.Default.Application_Width = this.Width;
                Properties.Settings.Default.Application_Maximized = false;
            }

            Properties.Settings.Default.LastSavedDocuments = string.Join(";", _vm.LastSavedDocuments.TakeLast(5));
            Properties.Settings.Default.Application_Language = _vm.SelectedLanguage;

            Properties.Settings.Default.Save();
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = TextEditor.GetLineIndexFromCharacterIndex(TextEditor.CaretIndex);
            int column = TextEditor.CaretIndex - TextEditor.GetCharacterIndexFromLineIndex(row);

            StatusBarCursorPosition.Text = $"{AppResources.MainWindow_StatusBar_Line} {row + 1}, {AppResources.MainWindow_StatusBar_Character} {column + 1}";
        }
    }
}
