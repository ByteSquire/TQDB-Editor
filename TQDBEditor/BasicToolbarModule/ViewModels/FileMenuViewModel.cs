using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

namespace TQDBEditor.BasicToolbarModule.ViewModels
{
    public partial class FileMenuViewModel : ViewModelBase
    {
        private readonly IConfiguration _config;
        private readonly IStorageProvider _storageProvider;
        private readonly ILogger _logger;

        [ObservableProperty]
        private string? _workingDir;

        [RelayCommand]
        private async Task SetWorkingFolder()
        {
            var storageProvider = _storageProvider;
            if (!storageProvider.CanOpen)
            {
                _logger.LogError("The current IStorage provider {providerType} does not support opening files!", storageProvider.GetType());
                return;
            }
            var startFolder = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            if (startFolder != null)
            {
                var myGamesPath = Path.Combine(startFolder.Path.LocalPath, "My Games");
                var myGames = await storageProvider.TryGetFolderFromPathAsync(myGamesPath);
                if (myGames != null)
                {
                    startFolder = myGames;
                    var tqPath = Path.Combine(startFolder.Path.LocalPath, "Titan Quest - Immortal Throne");
                    var tqFolder = await storageProvider.TryGetFolderFromPathAsync(tqPath);
                    if (tqFolder != null)
                    {
                        startFolder = tqFolder;
                    }
                    else
                    {
                        tqPath = Path.Combine(startFolder.Path.LocalPath, "Titan Quest");
                        tqFolder = await storageProvider.TryGetFolderFromPathAsync(tqPath);
                        if (tqFolder != null)
                            startFolder = tqFolder;
                    }
                }
                var pickedFolder = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() { AllowMultiple = false, Title = "Select a working directory...", SuggestedStartLocation = startFolder });
                if (pickedFolder.Any())
                    WorkingDir = pickedFolder.Single().Path.LocalPath;
            }
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

        public FileMenuViewModel(ILoggerProvider loggerProvider, IStorageProvider storageProvider, IConfiguration config)
        {
            _logger = loggerProvider.CreateLogger("File Menu");
            _storageProvider = storageProvider;
            _config = config;
            _workingDir = config.GetWorkingDir();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(WorkingDir))
                _config.SetWorkingDir(WorkingDir);
        }
    }
}
