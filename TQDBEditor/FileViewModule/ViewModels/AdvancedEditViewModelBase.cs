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
            _variableProvider.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IVariableProvider.Value))
                    OnPropertyChanged(nameof(Value));
            };
        }

        public string? Value { get => _variableProvider.Value; set => _variableProvider.Value = value; }

        public virtual void OnClick()
        {
            ShowDialog(_dialogService);
        }

        protected abstract void ShowDialog(IDialogService dialogService);
    }
}
