using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TQDB_Parser.DBR;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public abstract partial class AdvancedEditViewModelBase : ViewModelBase
    {
        protected readonly IVariableProvider _variableProvider;
        private readonly IDialogService _dialogService;

        public AdvancedEditViewModelBase(IVariableProvider variableProvider, IDialogService dialogService)
        {
            _variableProvider = variableProvider;
            _dialogService = dialogService;
            _value = _variableProvider.Value;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Value))
            {
                _variableProvider.Value = Value;
            }
        }

        [ObservableProperty]
        private string? _value;

        public virtual void OnClick()
        {
            ShowDialog(_dialogService, UpdateValue);
        }

        protected virtual void UpdateValue(string? value)
        {
            Value = value;
        }

        protected abstract void ShowDialog(IDialogService dialogService, Action<string> callback);
    }
}
