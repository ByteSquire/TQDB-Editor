using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TQDBEditor.Services;

namespace TQDBEditor.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly string baseTitle = "TQDBEditor";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private string _activeMod = string.Empty;

        public string Title
        {
            get
            {
                if (ActiveMod != null)
                    return baseTitle + " - " + ActiveMod;
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

        public MainWindowViewModel(IObservableConfiguration config)
        {
            ActiveMod = config.GetModName() ?? string.Empty;
            config.AddModNameChangeListener(x => ActiveMod = x ?? string.Empty);
        }
    }
}
