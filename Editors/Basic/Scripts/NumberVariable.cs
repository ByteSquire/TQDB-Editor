using Godot;
using System;
using System.Globalization;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class NumberVariable : VariableControl
    {
        [Export]
        private SpinBox variableBox;

        [Export]
        private bool isReal;

        public override string GetChangedValue()
        {
            return variableBox.Value.ToString("F6", CultureInfo.InvariantCulture);
        }

        protected override void InitVariable(DBREntry entry)
        {
            if (isReal)
                variableBox.Prefix = "real: ";
            else
                variableBox.Prefix = "int: ";
            variableBox.GetLineEdit().TextSubmitted += (str) =>
            {
                GetViewport().SetInputAsHandled();
                GetViewport().GuiReleaseFocus();
                CallDeferred(nameof(OnConfirmed));
            };

            if (isReal)
            {
                variableBox.MinValue = float.MinValue;
                variableBox.MaxValue = float.MaxValue;
                variableBox.Step = 0.000001;
                if (float.TryParse(entry.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var floatValue))
                    variableBox.Value = floatValue;
            }
            else
            {
                variableBox.MinValue = int.MinValue;
                variableBox.MaxValue = int.MaxValue;
                variableBox.Step = 1;
                if (int.TryParse(entry.Value, out var intValue))
                    variableBox.Value = intValue;
            }
        }
    }
}