using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using TQDBEditor.DataTypes;
using TQDBEditor.Dialogs.NewMod;

namespace TQDBEditor.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly string baseTitle = "TQDBEditor";

        public string Title
        {
            get
            {
                if (ActiveMod != null)
                    return baseTitle + " - " + ActiveMod.Name;
                else
                    return baseTitle;
            }
        }

        public static IBrush? SeparatorBackground => new SolidColorBrush(Colors.Gray);

        [ObservableProperty]
        private double _footerCombiWidth = 200;

        [ObservableProperty]
        private bool _isStatusBarVisible = true;

        [ObservableProperty]
        private string _status = "Ready";

        [ObservableProperty]
        private ObservableCollection<ModMenuItem> _detectedMods = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DetectedMods))]
        private IStorageFolder? _workingDir;

        private string? WorkingModsFolder => WorkingDir is null ? null : Path.Combine(WorkingDir.Path.LocalPath, "CustomMaps");

        private ModMenuItem? _activeMod;
        public ModMenuItem? ActiveMod
        {
            get => _activeMod;
            set
            {
                if (value == _activeMod)
                    return;
                if (_activeMod != null)
                    _activeMod.IsActive = false;
                _activeMod = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(ActiveMod));
                if (_activeMod != null)
                    _activeMod.IsActive = true;
            }
        }

        private static bool shouldActivateNextMod = false;

        [RelayCommand]
        private void NewMod()
        {
            dialogService.ShowNewModDialog(DetectedMods.Select(x => x.Name), CreateAndSelectMod);

            void CreateAndSelectMod(string modName)
            {
                if (WorkingDir != null)
                {
                    shouldActivateNextMod = true;
                    Directory.CreateDirectory(Path.Combine(WorkingModsFolder!, modName));
                }
            }
        }

        [RelayCommand]
        private void SelectMod(string modName)
        {
            Debug.WriteLine(modName);
            ActiveMod = DetectedMods.Single(x => x.Name == modName);
        }

        [RelayCommand]
        private void ToggleCommand()
        {
            // dummy
        }

        [RelayCommand]
        private void ToggleStatusBar()
        {
            IsStatusBarVisible = !IsStatusBarVisible;
        }

        [RelayCommand]
        private void Exit()
        {
            switch (Application.Current?.ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.TryShutdown();
                    break;
                default:
                    throw new InvalidOperationException("Trying to exit an app that does not use the ClassicDesktopStyle lifetime");
            }
        }

        [RelayCommand]
        private async void SetWorkingFolder()
        {
            switch (Application.Current?.ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    var storageProvider = desktop.MainWindow!.StorageProvider;
                    var startFolder = await storageProvider.TryGetWellKnownFolder(WellKnownFolder.Documents);
                    if (startFolder != null)
                    {
                        var myGamesPath = Path.Combine(startFolder.Path.LocalPath, "My Games");
                        var myGames = await storageProvider.TryGetFolderFromPath(myGamesPath);
                        if (myGames != null)
                        {
                            startFolder = myGames;
                            var tqPath = Path.Combine(startFolder.Path.LocalPath, "Titan Quest - Immortal Throne");
                            var tqFolder = await storageProvider.TryGetFolderFromPath(tqPath);
                            if (tqFolder != null)
                            {
                                startFolder = tqFolder;
                            }
                            else
                            {
                                tqPath = Path.Combine(startFolder.Path.LocalPath, "Titan Quest");
                                tqFolder = await storageProvider.TryGetFolderFromPath(tqPath);
                                if (tqFolder != null)
                                    startFolder = tqFolder;
                            }
                        }
                        var pickedFolder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false, Title = "Select a working directory...", SuggestedStartLocation = startFolder });
                        WorkingDir = pickedFolder.Single();
                    }
                    break;
                default:
                    throw new InvalidOperationException("Trying to use the StorageProvider with an app that does not use the ClassicDesktopStyle lifetime");
            }
        }

        private readonly IDialogService dialogService;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            PropertyChanged += (s, e) => { if (e.PropertyName == nameof(WorkingDir)) OnWorkingDirChanged(); };
            //DetectedMods = new() { CreateNewModMenuItem("Test", true), CreateNewModMenuItem("Test1"), CreateNewModMenuItem("Test2"), CreateNewModMenuItem("Test3") };
        }

        private FileSystemWatcher? watcher;

        private void OnWorkingDirChanged()
        {
            if (WorkingModsFolder != null)
            {
                ReinitDetectedMods();
                if (watcher is null)
                {
                    watcher = new FileSystemWatcher(WorkingModsFolder!)
                    {
                        NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastAccess | NotifyFilters.LastWrite,
                        EnableRaisingEvents = true
                    };
                    watcher.Created += ModDirCreated;
                    watcher.Deleted += ModDirDeleted;
                    watcher.Renamed += ModDirRenamed;
                }
                else
                {
                    watcher.Path = WorkingModsFolder!;
                }
            }
        }

        private void ReinitDetectedMods()
        {
            DetectedMods.Clear();
            var workingModsPath = WorkingModsFolder!;
            var mods = Directory.CreateDirectory(workingModsPath).EnumerateDirectories().ToList();
            DetectedMods.AddRange(mods.Select(mod => CreateNewModMenuItem(mod.Name)));
        }

        private void ModDirCreated(object sender, FileSystemEventArgs e)
        {
            AddModDir(e.Name!);
        }

        private void AddModDir(string name)
        {
            if (DetectedMods.Any(x => x.Name == name))
                return;

            var newMod = CreateNewModMenuItem(name, shouldActivateNextMod);
            shouldActivateNextMod = false;
            var ordered = DetectedMods.Append(newMod).OrderBy(x => x.Name);
            DetectedMods.Insert(ordered.IndexOf(newMod), newMod);
        }

        private void ModDirDeleted(object sender, FileSystemEventArgs e)
        {
            RemoveModDir(e.Name!);
        }

        private void RemoveModDir(string name)
        {
            if (!DetectedMods.Any(x => x.Name == name))
                return;

            int index = -1;
            for (int i = 0; i < DetectedMods.Count; i++)
            {
                if (DetectedMods[i].Name == name)
                    index = i;
            }
            if (index > -1)
                DetectedMods.RemoveAt(index);

            if (ActiveMod != null && ActiveMod.Name == name)
            {
                ActiveMod = null;
                App.MainThreadContext?.Post(x => dialogService.ShowInfoDialog("WARNING!", "The currently selected mod has been deleted"), this);
            }
        }

        private void ModDirRenamed(object sender, RenamedEventArgs e)
        {
            RemoveModDir(e.OldName!);
            AddModDir(e.Name!);
        }

        ~MainWindowViewModel()
        {
            if (watcher != null)
            {
                watcher.Created -= ModDirCreated;
                watcher.Deleted -= ModDirDeleted;
                watcher.Renamed -= ModDirRenamed;
                watcher.Dispose();
            }
        }

        private ModMenuItem CreateNewModMenuItem(string name, bool isActive = false)
        {
            var ret = new ModMenuItem(name, isActive, SelectModCommand);
            if (isActive)
                ActiveMod = ret;
            return ret;
        }
    }
}
