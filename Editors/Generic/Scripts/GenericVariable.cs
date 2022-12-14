using Godot;
using System;
using TQDBEditor;
using TQDBEditor.EditorScripts;
using TQDB_Parser.DBR;

namespace TQDBEditor.GenericEditor
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