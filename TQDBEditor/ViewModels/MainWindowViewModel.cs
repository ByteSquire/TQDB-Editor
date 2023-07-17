using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using TQDBEditor.DataTypes;
using TQDBEditor.Dialogs.NewMod;

namespace TQDBEditor.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly string baseTitle = "TQDBEditor";

        [ObservableProperty]
        private ModMenuItem _activeMod = new("Test", true, null);
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

        [RelayCommand]
        private void ToggleStatusBar()
        {
            IsStatusBarVisible = !IsStatusBarVisible;
        }

        public MainWindowViewModel()
        {
        }
    }
}
