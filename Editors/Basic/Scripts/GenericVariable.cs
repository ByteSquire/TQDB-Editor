using Godot;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class GenericVariable : VariableControl
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
            if (!entry.IsValid())
            {
                if (entry.InvalidIndex > -1)
                {
                    var split = value.Split(';');
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (i > 0)
                            valueLabel.AppendText(";");

                        if (i == entry.InvalidIndex)
                            valueLabel.PushColor(Colors.Red);

                        valueLabel.AppendText(split[i]);

                        if (i == entry.InvalidIndex)
                            valueLabel.Pop();
                    }
                }
                else
                {
                    valueLabel.PushColor(Colors.Red);
                    valueLabel.AddText(value);
                    valueLabel.Pop();
                }
            }
            else
                valueLabel.Text = value;
        }
    }
}