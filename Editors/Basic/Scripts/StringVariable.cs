using Godot;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class StringVariable : VariableControl
    {
        [Export]
        private LineEdit textEdit;

        public override string GetChangedValue()
        {
            return textEdit.Text;
        }

        protected override void InitVariable(DBREntry entry)
        {
            textEdit.Text = entry.Value;
            textEdit.PlaceholderText = entry.Value;

            textEdit.TextSubmitted += (str) =>
            {
                GetViewport().SetInputAsHandled();
                GetViewport().GuiReleaseFocus();
                CallDeferred(nameof(OnConfirmed));
            };
        }
    }
}
