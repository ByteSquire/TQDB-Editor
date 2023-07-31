using Prism.Services.Dialogs;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class FileEditViewModel : AdvancedEditViewModelBase
    {
        public FileEditViewModel(DBREntry dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        { }

        protected override void ShowDialog(IDialogService dialogService)
        {
            dialogService.ShowDBFilePicker(_dbrEntry);
        }
    }
}
