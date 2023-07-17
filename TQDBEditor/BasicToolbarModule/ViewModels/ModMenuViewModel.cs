﻿using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Prism.Services.Dialogs;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using TQDBEditor.DataTypes;
using TQDBEditor.ViewModels;
using TQDBEditor.Dialogs.NewMod;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Prism.Events;

namespace TQDBEditor.BasicToolbarModule.ViewModels
{
    public partial class ModMenuViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ModMenuItem> _detectedMods = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DetectedMods))]
        private IStorageFolder? _workingDir;

        private readonly IDialogService _dialogService;
        private static bool _shouldActivateNextMod = false;
        private string? _workingModsFolder = null;
        private string? WorkingModsFolder => _workingModsFolder ?? (WorkingDir is null ? null : (_workingModsFolder = Path.Combine(WorkingDir.Path.LocalPath, "CustomMaps")));

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
                OnPropertyChanged(nameof(ActiveMod));
                if (_activeMod != null)
                    _activeMod.IsActive = true;
            }
        }

        [RelayCommand]
        private void NewMod()
        {
            _dialogService.ShowNewModDialog(DetectedMods.Select(x => x.Name), CreateAndSelectMod);

            void CreateAndSelectMod(string modName)
            {
                if (WorkingDir != null)
                {
                    _shouldActivateNextMod = true;
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

        public ModMenuViewModel(IDialogService dialogService, IEventAggregator ea)
        {
            _dialogService = dialogService;
            ea.GetEvent<WorkingDirChangedEvent>().Subscribe(x => WorkingDir = x);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(WorkingDir))
                OnWorkingDirChanged();
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

            var newMod = CreateNewModMenuItem(name, _shouldActivateNextMod);
            _shouldActivateNextMod = false;
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
                App.MainThreadContext?.Post(x => _dialogService.ShowInfoDialog("WARNING!", "The currently selected mod has been deleted"), this);
            }
        }

        private void ModDirRenamed(object sender, RenamedEventArgs e)
        {
            RemoveModDir(e.OldName!);
            AddModDir(e.Name!);
        }

        private ModMenuItem CreateNewModMenuItem(string name, bool isActive = false)
        {
            var ret = new ModMenuItem(name, isActive, SelectModCommand);
            if (isActive)
                ActiveMod = ret;
            return ret;
        }

        ~ModMenuViewModel()
        {
            if (watcher != null)
            {
                watcher.Created -= ModDirCreated;
                watcher.Deleted -= ModDirDeleted;
                watcher.Renamed -= ModDirRenamed;
                watcher.Dispose();
            }
        }
    }
}