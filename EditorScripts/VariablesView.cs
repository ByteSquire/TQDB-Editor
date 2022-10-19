using Godot;
using System;
using System.Collections.Generic;
using TQDB_Parser;

namespace TQDBEditor.EditorScripts
{
    struct TableColumn
    {
        public SplitContainer Heading { get; set; }
        public Container Column { get; set; }
    }

    public partial class VariablesView : Control
    {
        [Export]
        private GroupsView groupsView;

        [Export]
        private Table table;

        [Export]
        private PackedScene variableCell;

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
            valueHeading.Call("set_synced_offset", config.DefaultValueColumnWidth - 100);

            var nameColumnSplit = table.GetChild(0).GetChild(1).GetChild<SplitContainer>(0);
            var classColumnSplit = nameColumnSplit.GetChild<SplitContainer>(1);
            var typeColumnSplit = classColumnSplit.GetChild<SplitContainer>(1);
            var descriptionColumnSplit = typeColumnSplit.GetChild<SplitContainer>(1);
            var valueColumnSplit = descriptionColumnSplit.GetChild<SplitContainer>(1);

            nameColumn = new TableColumn
            { Heading = nameHeading, Column = nameColumnSplit.GetChild<Container>(0) };
            classColumn = new TableColumn
            { Heading = classHeading, Column = classColumnSplit.GetChild<Container>(0) };
            typeColumn = new TableColumn
            { Heading = typeHeading, Column = typeColumnSplit.GetChild<Container>(0) };
            descriptionColumn = new TableColumn
            {
                Heading = descriptionHeading,
                Column = descriptionColumnSplit.GetChild<Container>(0)
            };
            valueColumn = new TableColumn
            { Heading = valueHeading, Column = valueColumnSplit.GetChild<Container>(0) };
        }

        private void Clear()
        {
            ClearContainer(nameColumn.Column);
            ClearContainer(classColumn.Column);
            ClearContainer(typeColumn.Column);
            ClearContainer(descriptionColumn.Column);
            ClearContainer(valueColumn.Column);
        }

        private void ClearContainer(Container container)
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
                var nameLabel = variableCell.Instantiate<RichTextLabel>();
                nameLabel.Text = variable.Name;
                var classLabel = variableCell.Instantiate<RichTextLabel>();
                classLabel.Text = variable.Class.ToString();
                var typeLabel = variableCell.Instantiate<RichTextLabel>();
                typeLabel.Text = variable.Type.ToString();
                var descriptionLabel = variableCell.Instantiate<RichTextLabel>();

                row[0] = nameLabel;
                row[1] = classLabel;
                row[2] = typeLabel;

                var desc = variable.Description;
                if (string.IsNullOrEmpty(desc))
                    desc = " ";
                descriptionLabel.Text = desc;
                row[3] = descriptionLabel;

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
                    case VariableClass.variable:
                    case VariableClass.@static:
                        valueElement = new LineEdit
                        {
                            Editable = variable.Class == VariableClass.variable,
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                        };
                        (valueElement as LineEdit).TextSubmitted += x => file.UpdateEntry(variable.Name, x);
                        break;
                    case VariableClass.picklist:
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
                    case VariableClass.array:
                        valueElement = new HBoxContainer();
                        valueElement.AddChild(new Button
                        {
                            Text = "...",
                            SizeFlagsVertical = (int)SizeFlags.Fill,
                        });

                        var lineEdit = new LineEdit
                        {
                            Text = value,
                            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                            SizeFlagsVertical = (int)SizeFlags.ExpandFill,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = size,
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

                row[4] = valueElement;

                GD.Print("adding row: " + string.Join<Control>(", ", row));
                table.AddRow(new Godot.Collections.Array<Control>(row));
            }
        }

        public override void _ExitTree()
        {
            config.NameColumnWidth = nameColumn.Heading.SplitOffset + 100;
            config.ClassColumnWidth = classColumn.Heading.SplitOffset + 100;
            config.TypeColumnWidth = typeColumn.Heading.SplitOffset + 100;
            config.DescriptionColumnWidth = descriptionColumn.Heading.SplitOffset + 100;
            config.DefaultValueColumnWidth = valueColumn.Heading.SplitOffset + 100;
            base._ExitTree();
        }
    }
}