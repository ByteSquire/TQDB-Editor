using Godot;
using System;
using System.Linq;
using TQDB_Parser;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class EquationVariableControl : VariableControl
    {
        [Export]
        private RichTextLabel valueLabel;

        public override string GetChangedValue()
        {
            return Entry.Value;
        }

        protected override void InitVariable(DBREntry entry)
        {
            var value = entry.Value;
            var validVars = DBRFile.TemplateRoot.GetVariables(true)
                .Where(x => x.Type == VariableType.eqnVariable)
                .Select(x => x.DefaultValue)
                .ToList();
            entry.Template.ValidateEqnValue(value, validVars, out var invalidIndices);
            for (int i = 0; i < value.Length; i++)
            {
                var c = value[i];
                AddCheckedText(invalidIndices.Contains(i), c.ToString());
            }

            void AddCheckedText(bool isInvalid, string text)
            {
                if (isInvalid)
                    valueLabel.PushColor(Colors.Red);

                valueLabel.AddText(text);

                if (isInvalid)
                    valueLabel.Pop();
            }
        }
    }
}