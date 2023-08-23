using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;
using TQDBEditor.FileViewModule.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class ArrayEditViewModel : AdvancedEditViewModelBase
    {
        private readonly GroupBlock _fileTpl;

        public ArrayEditViewModel(GroupBlock fileTpl, IVariableProvider dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        {
            _fileTpl = fileTpl;
        }

        protected override void ShowDialog(IDialogService dialogService, Action<string> callback)
        {
            dialogService.ShowArrayEdit(callback, _variableProvider, _fileTpl);
        }
    }
}
