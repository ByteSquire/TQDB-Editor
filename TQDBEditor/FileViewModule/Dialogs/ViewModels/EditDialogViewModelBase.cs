using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public abstract class EditDialogViewModelBase : ConfirmationDialogViewModelBase
    {
        public override string Title => (LocalVariable is null ? string.Empty : LocalVariable.Name + " : ") + SubTitle;
        protected abstract string SubTitle { get; }

        public override event Action<IDialogResult>? RequestClose;
        protected IVariableProvider? LocalVariable { get; private set; }

        public override IDialogParameters? OnDialogCancelled(EventArgs e)
        {
            return null;
        }

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            return null;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            LocalVariable = parameters.GetVariable();
            OnPropertyChanged(nameof(LocalVariable));
            OnPropertyChanged(nameof(Title));
        }
    }
}
