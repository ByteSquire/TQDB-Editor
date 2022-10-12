using Godot;
using System;

namespace TQDBEditor.EditorScripts
{
    public partial class VariablesView : VBoxContainer
    {
        [Export]
        private GroupsView groupsView;

        [Export]
        private ItemList column1;
        [Export]
        private ItemList column2;
        [Export]
        private ItemList column3;
        [Export]
        private ItemList column4;
        [Export]
        private ItemList column5;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView.GroupSelected += OnGroupSelected;

            if (column1 is FileList col1)
                col1.otherLists = new ItemList[] { column2, column3, column4, column5 };

            if (column2 is FileList col2)
                col2.otherLists = new ItemList[] { column1, column3, column4, column5 };

            if (column3 is FileList col3)
                col3.otherLists = new ItemList[] { column2, column1, column4, column5 };

            if (column4 is FileList col4)
                col4.otherLists = new ItemList[] { column2, column3, column1, column5 };

            if (column5 is FileList col5)
                col5.otherLists = new ItemList[] { column2, column3, column4, column1 };
        }

        private void Clear()
        {
            column1.Clear();
            column2.Clear();
            column3.Clear();
            column4.Clear();
            column5.Clear();
        }

        private void OnGroupSelected()
        {
            Clear();
            (var group, var file) = groupsView.GetSelectedAndFile();
            var variables = group.GetVariables();

            foreach (var variable in variables)
            {
                column1.AddItem(variable.Name);
                column2.AddItem(variable.Class.ToString());
                column3.AddItem(variable.Type.ToString());

                var desc = variable.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = " ";
                column4.AddItem(desc);

                var value = file[variable.Name].Value;
                if (string.IsNullOrEmpty(value))
                    value = " ";
                column5.AddItem(value);
            }
        }
    }
}