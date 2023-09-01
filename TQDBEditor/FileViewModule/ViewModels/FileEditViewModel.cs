using Prism.Services.Dialogs;
using System;
using TQDBEditor.FileViewModule.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class FileEditViewModel : AdvancedEditViewModelBase
    {
        public FileEditViewModel(IVariableProvider dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        { }

        protected override void ShowDialog(IDialogService dialogService)
        {
            dialogService.ShowDBFilePicker(_variableProvider);
        }
    }
}
