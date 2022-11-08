using Godot;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class PickVariable : VariableControl
    {
        [Export]
        private OptionButton pickButton;

        public override string GetChangedValue()
        {
            return pickButton.GetItemText(pickButton.Selected);
        }

        protected override void InitVariable(DBREntry entry)
        {
            var options = entry.Template.DefaultValue.Split(";");

            for (int i = 0; i < options.Length; i++)
            {
                pickButton.AddItem(options[i], i);
                if (entry.Value == options[i])
                    pickButton.Select(pickButton.GetItemIndex(i));
            }

            if (!entry.IsValid())
                pickButton.Select(-1);

            pickButton.ItemSelected += (id) => OnConfirmed();
        }
    }
}
