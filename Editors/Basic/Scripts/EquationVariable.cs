using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class EquationVariable : EditorDialog
    {
        [Export]
        private ItemList variableList;
        [Export]
        private LineEdit equationEdit;
        [Export]
        private RichTextLabel equationPreview;
        [Export]
        private Button addVariableButton;

        private List<string> validVariables = new();

        protected override string GetChangedValue()
        {
            return equationEdit.Text;
        }

        protected override void InitVariable(DBREntry entry)
        {
            var variables = DBRFile.TemplateRoot.GetVariables(true).Where(x => x.Type == VariableType.eqnVariable);
            foreach (var variable in variables)
            {
                variableList.AddItem(variable.DefaultValue);
                validVariables.Add(variable.DefaultValue);
            }

            addVariableButton.Pressed += OnAddVarPressed;
            equationEdit.Text = entry.Value;
            equationEdit.TextSubmitted += OnEquationSubmitted;
            equationEdit.TextChanged += SetPreviewValue;
            equationEdit.CaretColumn = equationEdit.Text.Length;

            SetPreviewValue(entry.Value);
        }

        private void OnAddVarPressed()
        {
            if (variableList.GetSelectedItems().Length == 0)
                return;

            var varString = validVariables[variableList.GetSelectedItems()[0]] + ' ';
            equationEdit.GrabFocus();

            //equationEdit.InsertTextAtCaret(varString);
            // workaround to enable undo/redo inside text edit, apparently InsertTextAtCaret doesn't fire do action
            equationEdit.Text = equationEdit.Text[..equationEdit.CaretColumn] + varString + equationEdit.Text[equationEdit.CaretColumn..];

            SetPreviewValue(equationEdit.Text);
        }

        private void SetPreviewValue(string value)
        {
            equationPreview.Clear();
            entry.Template.ValidateEqnValue(value, validVariables, out var invalidIndices);
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                AddCheckedText(invalidIndices.Contains(i), c.ToString());
            }

            void AddCheckedText(bool isInvalid, string text)
            {
                if (isInvalid)
                    equationPreview.PushColor(Colors.Red);

                equationPreview.AddText(text);

                if (isInvalid)
                    equationPreview.Pop();
            }
        }

        private void OnEquationSubmitted(string newText)
        {
            SetInputAsHandled();
            GuiReleaseFocus();
            SetPreviewValue(newText);
        }
    }
}