using Prism.Services.Dialogs;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class FileEditViewModel : AdvancedEditViewModelBase
    {
        public FileEditViewModel(ObservableEntry dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        { }

        protected override void ShowDialog(IDialogService dialogService, Action<string> callback)
        {
            dialogService.ShowDBFilePicker(callback, _dbrEntry);
        }
    }
}
