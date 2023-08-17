using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class EquationEditViewModel : AdvancedEditViewModelBase
    {
        private readonly GroupBlock _template;
        public EquationEditViewModel(GroupBlock template, ObservableEntry dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        {
            _template = template;
        }

        protected override void ShowDialog(IDialogService dialogService, Action<string> callback)
        {
            dialogService.ShowEquationEdit(callback, _dbrEntry, _template);
        }
    }
}
