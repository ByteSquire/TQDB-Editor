using Godot;
using System;
using System.Collections.Generic;

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
        private VBoxContainer column5;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView.GroupSelected += OnGroupSelected;

            if (column1 is FileList col1)
                col1.otherLists = new ItemList[] { column2, column3, column4 };

            if (column2 is FileList col2)
                col2.otherLists = new ItemList[] { column1, column3, column4 };

            if (column3 is FileList col3)
                col3.otherLists = new ItemList[] { column2, column1, column4 };

            if (column4 is FileList col4)
                col4.otherLists = new ItemList[] { column2, column3, column1 };
        }

        private void Clear()
        {
            column1.Clear();
            column2.Clear();
            column3.Clear();
            column4.Clear();
            foreach (var child in column5.GetChildren())
                child.QueueFree();
        }

        private void OnGroupSelected()
        {
            Clear();
            GD.Print("Hello from VariablesView.OnGroupSelected");
            (var variables, var file) = groupsView.GetSelectedAndFile();

            foreach (var variable in variables)
            {
                column1.AddItem(variable.Name);
                column2.AddItem(variable.Class.ToString());
                column3.AddItem(variable.Type.ToString());

                var desc = variable.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = " ";
                column4.AddItem(desc);

                string value;
                try
                {
                    value = file[variable.Name].Value;
                }
                catch (KeyNotFoundException)
                {
                    value = variable.GetDefaultValue();
                }
                if (string.IsNullOrEmpty(value))
                    value = " ";
                Control valueElement;
                switch (variable.Class)
                {
                    case TQDB_Parser.VariableClass.variable:
                        valueElement = new TextEdit
                        {
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = new Vector2i(10, 27),
                            Size = new Vector2i(10, 27),
                        };
                        (valueElement.GetChild(0, true) as ScrollBar).Scale = new Vector2(0, 0);
                        (valueElement.GetChild(1, true) as ScrollBar).Scale = new Vector2(0, 0);
                        break;
                    case TQDB_Parser.VariableClass.@static:
                        valueElement = new TextEdit
                        {
                            Editable = false,
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = new Vector2i(10, 27),
                            Size = new Vector2i(10, 27),
                        };
                        (valueElement.GetChild(0, true) as ScrollBar).Scale = new Vector2(0, 0);
                        (valueElement.GetChild(1, true) as ScrollBar).Scale = new Vector2(0, 0);
                        break;
                    case TQDB_Parser.VariableClass.picklist:
                        valueElement = new OptionButton { ClipContents = true };
                        var defaultValues = variable.DefaultValue.Split(';');
                        var valueId = -1;
                        for (int i = 0; i < defaultValues.Length; i++)
                        {
                            (valueElement as OptionButton).AddItem(defaultValues[i], i);
                            if (defaultValues[i].Equals(value))
                                valueId = i;
                        }

                        (valueElement as OptionButton).GetItemIndex(valueId);
                        break;
                    case TQDB_Parser.VariableClass.array:
                        valueElement = new HBoxContainer();
                        valueElement.AddChild(new Button
                        {
                            Text = "...",
                            SizeFlagsVertical = (int)SizeFlags.Fill,
                            CustomMinimumSize = new Vector2i(10, 27),
                            Size = new Vector2i(10, 27),
                        });

                        var textEdit = new TextEdit
                        {
                            Text = value,
                            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                            SizeFlagsVertical = (int)SizeFlags.ExpandFill,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = new Vector2i(10, 27),
                            Size = new Vector2i(10, 27),
                        };
                        (textEdit.GetChild(0, true) as ScrollBar).Scale = new Vector2(0, 0);
                        (textEdit.GetChild(1, true) as ScrollBar).Scale = new Vector2(0, 0);

                        valueElement.AddChild(textEdit);
                        break;
                    default:
                        continue;
                }
                valueElement.TooltipText = variable.DefaultValue;
                valueElement.SizeFlagsHorizontal = (int)SizeFlags.Fill;
                valueElement.Size = valueElement.CustomMinimumSize = new Vector2i(100, 27);

                column5.AddChild(valueElement);
            }
        }
    }
}