using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public abstract class EditDialogViewModelBase : ConfirmationDialogViewModelBase
    {
        public override event Action<IDialogResult>? RequestClose;
        protected DBREntry? LocalEntry { get; private set; }

        public override IDialogParameters? OnDialogCancelled(EventArgs e)
        {
            return null;
        }

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            if (LocalEntry != null)
            {
                var dParams = new DialogParameters();
                dParams.AddChangedValue(LocalEntry.Value);
                return dParams;
            }
            return null;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var entry = parameters.GetSelectedEntry();
            LocalEntry = new DBREntry(((DBREntry)entry).Template, entry.Value);
        }
    }
}
