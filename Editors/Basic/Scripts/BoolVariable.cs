using Godot;
using System;
using TQDB_Parser.DBR;
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
            if (int.TryParse(entry.Value, out var intValue))
            {
                button.ButtonPressed = intValue != 0;
                button.Text = intValue != 0 ? "1 (true)" : "0 (false)";
            }

            button.Toggled += (toggled) => OnConfirmed();
        }
    }
}