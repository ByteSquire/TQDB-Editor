using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TQDB_Parser.Blocks;
using TQDBEditor.Dialogs;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public partial class EquationEditDialogViewModel : EditDialogViewModelBase
    {
        public override string Title => "Edit the Equation";

        [ObservableProperty]
        private string? _equation;

        [ObservableProperty]
        private IList<string>? _equationVariables;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            Equation = LocalVariable?.Value;
            EquationVariables = parameters.GetTemplateGroup().GetVariables().Where(x => x.Type == TQDB_Parser.VariableType.eqnVariable).Select(x => x.DefaultValue).ToList();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Equation) &&
                LocalVariable != null && Equation != null && Equation != LocalVariable.Value)
            {
                LocalVariable.Value = Equation;
            }
        }

        public override bool CanConfirmDialog() => true;

        public void AddVariable(int index, string? variable)
        {
            if (Equation == null || variable == null)
                return;

            Equation = Equation.Insert(index, variable);
        }
    }
}
