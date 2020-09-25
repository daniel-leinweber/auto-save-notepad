using AutoSaveNotepad.Helper;
using AutoSaveNotepad.Localization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutoSaveNotepad.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            PageTitle = AppResources.MainWindow_PageTitle;
            TemporaryFilename = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSaveNotepad", Guid.NewGuid().ToString())}.txt";
            SelectedLanguage = Properties.Settings.Default.Application_Language;

            LoadLastDocuments();

            if (App.Args != null && App.Args.Length > 0)
            {
                OpenFile(App.Args[0]);
            }
        }

        public event Action RequestClose;
        private string Filename { get; set; }
        private string TemporaryFilename { get; set; }
        private bool UnsavedChanges { get; set; } = false;
        internal ObservableCollection<string> LastSavedDocuments { get; set; } = new ObservableCollection<string>();

        public bool IsEnglishActive
        {
            get { return _isEnglishActive; }
            set { SetProperty(ref _isEnglishActive, value); }
        }
        private bool _isEnglishActive = true;

        public bool IsGermanActive
        {
            get { return _isGermanActive; }
            set { SetProperty(ref _isGermanActive, value); }
        }
        private bool _isGermanActive = true;

        public string SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            private set
            {
                SetProperty(ref _selectedLanguage, value);
                IsEnglishActive = value.Equals("EN");
                IsGermanActive = value.Equals("DE");
            }
        }
        private string _selectedLanguage = null;

        public string DocumentText
        {
            get { return _documentText; }
            set
            {
                UnsavedChanges = _documentText != value;
                if (IsAutoSaveEnabled)
                {
                    SaveFile(true);
                }
                SetProperty(ref _documentText, value);
            }
        }
        private string _documentText;

        public bool IsAutoSaveEnabled
        {
            get { return _isAutoSaveEnabled; }
            set { SetProperty(ref _isAutoSaveEnabled, value); }
        }
        private bool _isAutoSaveEnabled = true;

        public ICommand NewFileCommand => new RelayCommand(param => CreateNewFile(), param => true);
        public ICommand OpenFileCommand => new RelayCommand(param => OpenFile(), param => true);
        public ICommand SaveFileCommand => new RelayCommand(param => SaveFile(), param => true);
        public ICommand SaveFileAsCommand => new RelayCommand(param => SaveFileAs(), param => true);
        public ICommand CloseCommand => new RelayCommand(param => Close(), param => { return true; });
        public ICommand SwitchLanguageCommand => new RelayCommand(param => SwitchLanguage(), param => true);
        public ICommand OpenRecentFileCommand => new RelayCommand((file) => OpenRecentFile(file), param => true);

        private void OpenRecentFile(object file)
        {
            OpenFile(file as string);
        }

        private void LoadLastDocuments()
        {
            var lastDocuments = string.IsNullOrEmpty(Properties.Settings.Default.LastSavedDocuments) == false ? Properties.Settings.Default.LastSavedDocuments.Split(";").ToList() : new List<string>();
            lastDocuments.ForEach(x => LastSavedDocuments.Add(x));
        }

        private void SwitchLanguage()
        {
            string language = SelectedLanguage.Equals("DE") ? "EN" : "DE";
            LanguageHelper.SetApplicationLanguage(language);
            SelectedLanguage = language;

            MessageBox.Show(
                AppResources.SwitchLanguage_Message,
                AppResources.SwitchLanguage_Caption,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void SaveFileAs()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    DefaultExt = ".txt",
                    Filter = AppResources.MainWindow_FileDialog_DocumentFilter,
                    OverwritePrompt = true
                };

                var result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    Filename = saveFileDialog.FileName;
                    if (File.Exists(TemporaryFilename))
                    {
                        File.Delete(TemporaryFilename);
                    }
                    TemporaryFilename = string.Empty;
                    SaveFile();
                }
            }
            catch
            {
                MessageBox.Show(
                    AppResources.MainWindow_Save_Failed,
                    AppResources.MainWindow_Save_Failed_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SaveFile(bool autoSave = false)
        {
            try
            {
                if (autoSave == true)
                {
                    Task.Run(() =>
                    {
                        Save(string.IsNullOrEmpty(TemporaryFilename) ? Filename : TemporaryFilename);
                    });
                }
                else
                {
                    if (string.IsNullOrEmpty(Filename) == true)
                    {
                        SaveFileAs();
                    }
                    else
                    {
                        Save(Filename);
                    }
                }
            }
            catch
            {
                MessageBox.Show(
                    AppResources.MainWindow_Save_Failed,
                    AppResources.MainWindow_Save_Failed_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save(string filename)
        {
            File.WriteAllText(filename, DocumentText, Encoding.UTF8);
            UnsavedChanges = false;

            if (LastSavedDocuments.Contains(filename) == true)
            {
                LastSavedDocuments.Remove(filename);
            }
            LastSavedDocuments.Add(filename);

            UpdatePageTitle(filename);
        }

        private void UpdatePageTitle(string filename)
        {
            var file = Path.GetFileName(filename);
            PageTitle = $"{AppResources.MainWindow_PageTitle} ({file} - {AppResources.MainWindow_LastSaved}: {DateTime.Now})";
        }

        private void OpenFile(string filename = null)
        {
            try
            {
                if (ProcessedUnsavedChanges() == true)
                {
                    if (string.IsNullOrEmpty(filename) == true)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog
                        {
                            DefaultExt = ".txt",
                            Filter = AppResources.MainWindow_FileDialog_DocumentFilter,
                        };

                        var result = openFileDialog.ShowDialog();

                        if (result == true)
                        {
                            filename = openFileDialog.FileName;
                        }
                    }

                    if (string.IsNullOrEmpty(filename) == false)
                    {
                        if (File.Exists(filename))
                        {
                            Filename = filename;
                            TemporaryFilename = string.Empty;
                            DocumentText = File.ReadAllText(Filename);
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show(
                    AppResources.MainWindow_Open_Failed,
                    AppResources.MainWindow_Open_Failed_Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreateNewFile()
        {
            if (ProcessedUnsavedChanges() == true)
            {
                Filename = null;
                TemporaryFilename = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoSaveNotepad", Guid.NewGuid().ToString())}.txt";
                DocumentText = string.Empty;
                UnsavedChanges = false;
            }
        }

        private bool ProcessedUnsavedChanges()
        {
            bool output = UnsavedChanges == false;

            if (UnsavedChanges)
            {
                var result = MessageBox.Show(
                    AppResources.MainWindow_UnsavedChanges_Message,
                    AppResources.MainWindow_UnsavedChanges_Title,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile();
                    output = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    UnsavedChanges = false;
                    output = true;
                }
            }

            return output;
        }

        public virtual void Close()
        {
            if (ProcessedUnsavedChanges() == true)
            {
                RequestClose?.Invoke();
            }
        }
    }
}
