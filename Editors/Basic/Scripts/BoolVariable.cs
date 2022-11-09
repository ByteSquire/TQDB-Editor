using Godot;
using System;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class BoolVariable : VariableControl
    {
        [Export]
        private Button button;

        public override string GetChangedValue()
        {
            return button.ButtonPressed ? "1" : "0";
        }

        protected override void InitVariable(DBREntry entry)
        {
            if (TQNumberString.TryParseTQString(entry.Value, out bool boolValue))
            {
                button.ButtonPressed = boolValue;
                button.Text = boolValue ? "1 (true)" : "0 (false)";
            }

            button.Toggled += (toggled) => OnConfirmed();
        }
    }
}