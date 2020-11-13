using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace FileSizeEnumerator.UI
{
    internal sealed class MainViewModel : ModelBase, IDisposable
    {
        #region Constructor

        public MainViewModel()
        {
            this.enumerator = new FileEnumerator(Files);
            this.enumerator.Synchronization = SynchronizationContext;

            this.enumerator.IsWorkingObservable.ObserveOnDispatcher().Subscribe(value => IsWorking = value);

            BrowseCommand = new Command(() => Browse());
            CancelCommand = new ReactiveCommand
            (
                PropertyChangedObservable.Where(propertyName => propertyName == nameof(IsWorking)).Select(p => IsWorking),
                () => Cancel()
            );
            RunCommand = new ReactiveCommand
            (
                PropertyChangedObservable.Where(propertyName => propertyName == nameof(CanRun)).Select(p => CanRun),
                () => Run()
            );
            ShowInExplorerCommand = new ReactiveCommand
            (
                PropertyChangedObservable.Where(propertyName => propertyName == nameof(SelectedFile)).Select(p => SelectedFile != null),
                () => ShowInExplorer()
            );
            SaveToCsvCommand = new Command(() => SaveToCSV());
        }

        #endregion

        #region Fields

        private readonly FileEnumerator enumerator;
        private bool isWorking = false;
        private string path;
        private FileInfo selectedFile;

        #endregion

        #region Properties

        public bool IsWorking
        {
            get => this.isWorking;
            set => SetProperties(ref this.isWorking, value,
                nameof(IsWorking),
                nameof(CanRun),
                nameof(CanEditPath));
        }

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set => SetProperty(ref this.selectedFile, value);
        }

        public ulong MinimumSize
        {
            get => this.enumerator.MinimumFileSize;
        }

        public bool CanRun => !IsWorking && !string.IsNullOrWhiteSpace(Path);

        public bool CanEditPath => !IsWorking;

        public string Path
        {
            get => this.path;
            set => SetProperties(ref this.path, value, nameof(Path), nameof(CanRun));
        }

        public ObservableCollection<FileInfo> Files { get; } = new ObservableCollection<FileInfo>();

        public SynchronizationContext SynchronizationContext { get; set; } = SynchronizationContext.Current;

        public ICommand BrowseCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand RunCommand { get; }

        public ICommand ShowInExplorerCommand { get; }

        public ICommand SaveToCsvCommand { get; }

        #endregion

        #region Methods

        public void Cancel() => this.enumerator.Cancel();

        public void Run()
        {
            Files.Clear();

            this.enumerator.StartingPath = Path;
            try
            {
                this.enumerator.Start();
            }
            catch(DirectoryNotFoundException)
            {
                MessageBox.Show("Directory not found.", "File Size Enumerator", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Browse()
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = true,
                AllowNonFileSystemItems = false,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Multiselect = false,
                NavigateToShortcut = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                EnsurePathExists = true,
                Title = "Select directory"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok && string.IsNullOrEmpty(dialog.FileName)) return;

            if (Directory.Exists(dialog.FileName))
                Path = dialog.FileName;
        }

        private void ShowInExplorer()
        {
            if (SelectedFile == null ||
                string.IsNullOrEmpty(SelectedFile.FullName) ||
                !File.Exists(SelectedFile.FullName))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/e, /select, \"{SelectedFile.FullName}\""
            });
        }

        public void SaveToCSV()
        {
            if (Files?.Count < 1) return;

            var filename = $"{Guid.NewGuid().ToString()}.csv";
            var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), filename);

            using var file = File.CreateText(path);

        }

        public void Dispose()
        {
            this.enumerator.Dispose();
            Files.Clear();
        }

        #endregion
    }
}
