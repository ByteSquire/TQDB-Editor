using Godot;
using System;
using System.Collections.Generic;

namespace TQDBEditor.EditorScripts
{
    struct TableColumn
    {
        public SplitContainer Heading { get; set; }
        public VBoxContainer Column { get; set; }
    }

    public partial class VariablesView : Control
    {
        [Export]
        private GroupsView groupsView;

        [Export]
        private Control table;

        private TableColumn nameColumn;
        private TableColumn classColumn;
        private TableColumn typeColumn;
        private TableColumn descriptionColumn;
        private TableColumn valueColumn;

        private Config config;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView.GroupSelected += OnGroupSelected;

            config = this.GetEditorConfig();

            var nameHeading = table.GetChild(0).GetChild<SplitContainer>(0);
            var classHeading = nameHeading.GetChild<SplitContainer>(1);
            var typeHeading = classHeading.GetChild<SplitContainer>(1);
            var descriptionHeading = typeHeading.GetChild<SplitContainer>(1);
            var valueHeading = descriptionHeading.GetChild<SplitContainer>(1);

            nameHeading.Call("set_synced_offset", config.NameColumnWidth - 100);
            classHeading.Call("set_synced_offset", config.ClassColumnWidth - 100);
            typeHeading.Call("set_synced_offset", config.TypeColumnWidth - 100);
            descriptionHeading.Call("set_synced_offset", config.DescriptionColumnWidth - 100);
            //valueHeading.Call("set_synced_offset", config.DefaultValueColumnWidth - 100);

            var nameColumnSplit = table.GetChild(0).GetChild(1).GetChild<SplitContainer>(0);
            var classColumnSplit = nameColumnSplit.GetChild<SplitContainer>(1);
            var typeColumnSplit = classColumnSplit.GetChild<SplitContainer>(1);
            var descriptionColumnSplit = typeColumnSplit.GetChild<SplitContainer>(1);
            var valueColumnSplit = descriptionColumnSplit.GetChild<SplitContainer>(1);

            nameColumn = new TableColumn
            { Heading = nameHeading, Column = nameColumnSplit.GetChild<VBoxContainer>(0) };
            classColumn = new TableColumn
            { Heading = classHeading, Column = classColumnSplit.GetChild<VBoxContainer>(0) };
            typeColumn = new TableColumn
            { Heading = typeHeading, Column = typeColumnSplit.GetChild<VBoxContainer>(0) };
            descriptionColumn = new TableColumn
            {
                Heading = descriptionHeading,
                Column = descriptionColumnSplit.GetChild<VBoxContainer>(0)
            };
            valueColumn = new TableColumn
            { Heading = valueHeading, Column = valueColumnSplit.GetChild<VBoxContainer>(0) };
        }

        private void Clear()
        {
            ClearBox(nameColumn.Column);
            ClearBox(classColumn.Column);
            ClearBox(typeColumn.Column);
            ClearBox(descriptionColumn.Column);
            ClearBox(valueColumn.Column);
        }

        private void ClearBox(BoxContainer container)
        {
            foreach (var child in container.GetChildren())
                child.QueueFree();
        }

        private void OnGroupSelected()
        {
            Clear();
            (var variables, var file) = groupsView.GetSelectedAndFile();

            var size = new Vector2i(100, 38);
            foreach (var variable in variables)
            {
                var row = new Control[5];
                row[0] = new RichTextLabel { Text = variable.Name, CustomMinimumSize = size, AutowrapMode = TextServer.AutowrapMode.Off, };
                row[1] = new RichTextLabel { Text = variable.Class.ToString(), CustomMinimumSize = size, AutowrapMode = TextServer.AutowrapMode.Off, };
                row[2] = new RichTextLabel { Text = variable.Type.ToString(), CustomMinimumSize = size, AutowrapMode = TextServer.AutowrapMode.Off, };

                var desc = variable.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = " ";
                row[3] = new RichTextLabel { Text = desc, CustomMinimumSize = size, AutowrapMode = TextServer.AutowrapMode.Off, };

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
                        valueElement = new LineEdit
                        {
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                            //CustomMinimumSize = size,
                            //Size = size,
                        };
                        (valueElement as LineEdit).TextSubmitted += x => file.UpdateEntry(variable.Name, x);
                        //(valueElement as TextEdit).GetVScrollBar().Scale = new Vector2(0, 0);
                        //(valueElement as TextEdit).GetHScrollBar().Scale = new Vector2(0, 0);
                        break;
                    case TQDB_Parser.VariableClass.@static:
                        valueElement = new LineEdit
                        {
                            Editable = false,
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                            //CustomMinimumSize = size,
                            //Size = size,
                        };
                        (valueElement as LineEdit).TextSubmitted += x => file.UpdateEntry(variable.Name, x);
                        //(valueElement as TextEdit).GetVScrollBar().Scale = new Vector2(0, 0);
                        //(valueElement as TextEdit).GetHScrollBar().Scale = new Vector2(0, 0);
                        break;
                    case TQDB_Parser.VariableClass.picklist:
                        valueElement = new OptionButton { ClipText = true };
                        var defaultValues = variable.DefaultValue.Split(';');
                        var valueId = -1;
                        for (int i = 0; i < defaultValues.Length; i++)
                        {
                            (valueElement as OptionButton).AddItem(defaultValues[i], i);
                            if (defaultValues[i].Equals(value))
                                valueId = i;
                        }

                        (valueElement as OptionButton).Select((valueElement as OptionButton).GetItemIndex(valueId));
                        (valueElement as OptionButton).ItemSelected += x => file.UpdateEntry(variable.Name, (valueElement as OptionButton).GetItemText((int)x));
                        break;
                    case TQDB_Parser.VariableClass.array:
                        valueElement = new HBoxContainer();
                        valueElement.AddChild(new Button
                        {
                            Text = "...",
                            SizeFlagsVertical = (int)SizeFlags.Fill,
                            //CustomMinimumSize = size,
                            //Size = size,
                        });

                        var lineEdit = new LineEdit
                        {
                            Text = value,
                            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                            SizeFlagsVertical = (int)SizeFlags.ExpandFill,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = size,
                            //Size = size,
                        };
                        lineEdit.TextSubmitted += x => file.UpdateEntry(variable.Name, x);
                        //textEdit.GetVScrollBar().Scale = new Vector2(0, 0);
                        //textEdit.GetHScrollBar().Scale = new Vector2(0, 0);

                        valueElement.AddChild(lineEdit);
                        break;
                    default:
                        continue;
                }
                valueElement.TooltipText = variable.DefaultValue;
                valueElement.SizeFlagsHorizontal = (int)SizeFlags.Fill;
                /*valueElement.Size = */
                valueElement.CustomMinimumSize = size;

                row[4] = valueElement;

                GD.Print("adding row: " + string.Join<Control>(", ", row));
                table.Call("AddRow", /*new Godot.Collections.Array<Control>*/(row));
            }
        }

        public override void _ExitTree()
        {
            config.NameColumnWidth = nameColumn.Heading.SplitOffset + 100;
            config.ClassColumnWidth = classColumn.Heading.SplitOffset + 100;
            config.TypeColumnWidth = typeColumn.Heading.SplitOffset + 100;
            config.DescriptionColumnWidth = descriptionColumn.Heading.SplitOffset + 100;
            base._ExitTree();
        }
    }
}