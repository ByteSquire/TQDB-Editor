using Microsoft.Extensions.Configuration;
using Prism.Services.Dialogs;
using System;
using TQDBEditor.Dialogs;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public partial class DBFilePickerViewModel : EditDialogViewModelBase
    {
        public override string Title => "Pick a file";

        public override bool CanConfirmDialog() => true;

        public DBFilePickerViewModel(IConfiguration configuration)
        {
            ;
        }
    }
}
