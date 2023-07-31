using Prism.Services.Dialogs;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class ArrayEditViewModel : AdvancedEditViewModelBase
    {
        public ArrayEditViewModel(DBREntry dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        { }

        protected override void ShowDialog(IDialogService dialogService)
        {
            dialogService.ShowArrayEdit(_dbrEntry);
        }
    }
}
