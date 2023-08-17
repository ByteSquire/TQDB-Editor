using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public partial class ArrayEditDialogViewModel : EditDialogViewModelBase
    {
        public override string Title => "Edit the Array";

        public override bool CanConfirmDialog() => true;

        [ObservableProperty]
        private string? _seriesText;

        [ObservableProperty]
        private Control? _setAllContent;

        [ObservableProperty]
        private Control? _increaseAllContent;

        [ObservableProperty]
        private Control? _multiplyAllContent;

        [ObservableProperty]
        private bool? _shouldOverwrite;

        private readonly ICreateControlForVariable _variableControlProvider;
        private ObservableEntry? _setAllEntry;
        private ObservableEntry? _increaseAllEntry;
        private ObservableEntry? _multiplyAllEntry;

        public ArrayEditDialogViewModel(ICreateControlForVariable variableControlProvider)
        {
            _variableControlProvider = variableControlProvider;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (LocalEntry == null)
                return;

            VariableBlock tmpTpl = new(string.Empty, string.Empty, new Dictionary<string, string> { { "name", "tmp" }, { "class", VariableClass.variable.ToString() }, { "type", LocalEntry.Template.Type.ToString() } }, Array.Empty<Block>());
            _setAllEntry = new(new(tmpTpl));
            _increaseAllEntry = new(new(tmpTpl));
            _multiplyAllEntry = new(new(tmpTpl));
            var file = parameters.GetValue<DBRFile>("file");

            SetAllContent = _variableControlProvider.CreateControl(file, tmpTpl, _setAllEntry, true);
            if (tmpTpl.Type == VariableType.real || tmpTpl.Type == VariableType.@int)
            {
                IncreaseAllContent = _variableControlProvider.CreateControl(file, tmpTpl, _increaseAllEntry, true);
                MultiplyAllContent = _variableControlProvider.CreateControl(file, tmpTpl, _multiplyAllEntry, true);
            }
        }

        public void SetAll(object? _)
        {
            if (_setAllEntry == null) return;
            var value = _setAllEntry.Value;
        }

        public void IncreaseAll(object? _)
        {
            if (_increaseAllEntry == null) return;
            var value = _increaseAllEntry.Value;
        }

        public void MultiplyAll(object? _)
        {
            if (_multiplyAllEntry == null) return;
            var value = _multiplyAllEntry.Value;
        }

        public void IncreaseBySeries(object? _)
        {
            if (SeriesText == null) return;
            ;
        }
    }
}
