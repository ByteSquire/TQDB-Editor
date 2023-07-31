using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System.ComponentModel;
using TQDB_Parser.DBR;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public abstract partial class AdvancedEditViewModelBase : ViewModelBase
    {
        protected readonly DBREntry _dbrEntry;
        private readonly IDialogService _dialogService;

        public AdvancedEditViewModelBase(DBREntry dbrEntry, IDialogService dialogService)
        {
            _dbrEntry = dbrEntry;
            _dialogService = dialogService;
            _value = _dbrEntry.Value;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Value))
            {
                _dbrEntry.UpdateValue(Value ?? string.Empty);
            }
        }

        [ObservableProperty]
        private string? _value;

        public virtual void OnClick()
        {
            ShowDialog(_dialogService);
        }

        protected abstract void ShowDialog(IDialogService dialogService);
    }
}
