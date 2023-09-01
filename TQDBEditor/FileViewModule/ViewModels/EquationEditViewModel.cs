using Prism.Services.Dialogs;
using System;
using TQDB_Parser.Blocks;
using TQDBEditor.FileViewModule.Dialogs;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class EquationEditViewModel : AdvancedEditViewModelBase
    {
        private readonly GroupBlock _template;
        public EquationEditViewModel(GroupBlock template, IVariableProvider dbrEntry, IDialogService dialogService) : base(dbrEntry, dialogService)
        {
            _template = template;
        }

        protected override void ShowDialog(IDialogService dialogService)
        {
            dialogService.ShowEquationEdit(_variableProvider, _template);
        }
    }
}
