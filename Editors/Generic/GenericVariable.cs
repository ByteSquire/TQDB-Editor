using Godot;
using System;
using TQDB_Parser.DBR;

namespace TQDBEditor
{
    public partial class GenericVariable : EditorDialog
    {
        protected override string GetChangedValue()
        {
            return GetNode<LineEdit>("TextEdit").Text;
        }

        protected override void InitVariable(DBREntry entry)
        {
            var textEdit = GetNode<LineEdit>("TextEdit");
            textEdit.Text = entry.Value;
            textEdit.PlaceholderText = entry.Value;
        }
    }
}