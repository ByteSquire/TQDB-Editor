using Microsoft.Extensions.Configuration;
using Prism.Services.Dialogs;
using System;
using TQDBEditor.Dialogs;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.Dialogs
{
    public partial class ArrayEditDialogViewModel : ConfirmationDialogViewModelBase
    {
        public override string Title => "Pick a file";

        public override event Action<IDialogResult>? RequestClose;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            var entry = parameters.GetSelectedEntry();
        }

        public override bool CanConfirmDialog() => true;

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override IDialogParameters? OnDialogCancelled(EventArgs e)
        {
            throw new NotImplementedException();
        }

        public ArrayEditDialogViewModel(IConfiguration configuration)
        {
            ;
        }
    }
}
