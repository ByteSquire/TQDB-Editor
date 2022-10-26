using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.GenericEditor
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
        [Export]
        private EditorWindow editorWindow;

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
            editorWindow.Reinit += OnGroupSelected;

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
            {
                container.RemoveChild(child);
                child.QueueFree();
            }
        }

        private IReadOnlyList<VariableBlock> currentVariables;
        private Dictionary<string, int> variableRowMap;
        private GroupBlock currentGroup;

        private void OnGroupSelected()
        {
            Clear();
            var group = currentGroup = groupsView.GetSelectedGroup();
            var variables = currentVariables = editorWindow.DBRFile[group].Select(x => x.Template).ToList();
            var file = editorWindow.DBRFile;
            variableRowMap = new();

            foreach (var variable in variables)
            {
                var row = new Control[5];
                var nameLabel = variableCell.Instantiate<RichTextLabel>();
                nameLabel.Text = variable.Name;
                nameLabel.Connect("activated", new Callable(OnLabelDoubleClicked));
                var classLabel = variableCell.Instantiate<RichTextLabel>();
                classLabel.Text = variable.Class.ToString();
                classLabel.Connect("activated", new Callable(OnLabelDoubleClicked));
                var typeLabel = variableCell.Instantiate<RichTextLabel>();
                typeLabel.Text = variable.Type.ToString();
                typeLabel.Connect("activated", new Callable(OnLabelDoubleClicked));
                var descriptionLabel = variableCell.Instantiate<RichTextLabel>();
                descriptionLabel.Text = variable.Description;
                descriptionLabel.Connect("activated", new Callable(OnLabelDoubleClicked));

                row[0] = nameLabel;
                row[1] = classLabel;
                row[2] = typeLabel;
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

                var valueElement = CreateValueControl(variable, value, file);
                if (valueElement is null)
                    continue;
                valueElement.TooltipText = variable.DefaultValue;

                row[4] = valueElement;

                variableRowMap.Add(variable.Name, table.AddRow(new Godot.Collections.Array<Control>(row)));
            }
        }

        private Control CreateValueControl(VariableBlock variable, string value, DBRFile file)
        {
            var valueLabel = variableCell.Instantiate<RichTextLabel>();
            var entry = file[variable.Name];
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

            valueLabel.Connect("activated", new Callable(OnLabelDoubleClicked));
            return valueLabel;

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
                    });

                    var lineEdit = new LineEdit
                    {
                        Text = value,
                        PlaceholderText = variable.GetDefaultValue(),
                    };
                    lineEdit.TextSubmitted += x => file.UpdateEntry(variable.Name, x);

                    valueElement.AddChild(lineEdit);
                    break;
                default:
                    return null;
            }
            return valueElement;
        }

        public void SelectEntry()
        {
            var entry = editorWindow.GetFocussedEntry();
            var row = table.GetRow(variableRowMap[entry.Name]);

            row[0].FocusMode = FocusModeEnum.All;
            row[0].GrabFocus();
        }

        private void OnLabelDoubleClicked(RichTextLabel obj)
        {
            var index = table.GetCellPosition(obj).y;
            var row = table.GetRow(index);
            var vName = (row[0] as RichTextLabel).Text;
            var vClass = (row[1] as RichTextLabel).Text;
            var vType = (row[2] as RichTextLabel).Text;

            var handler = this.GetPCKHandler();
            var availableEditors = handler.GetEntryEditors(vName, vClass, vType);

            EditorDialog editor;
            if (availableEditors is null || availableEditors.Count < 1)
            {
                this.GetConsoleLogger().LogError("No editors found for variable {var}", vName);
                return;
            }
            else
                editor = availableEditors[0].Instantiate<EditorDialog>();

            editor.DBRFile = editorWindow.DBRFile;
            editor.VarName = vName;

            editor.Confirmed += () => CallDeferred(nameof(OnGroupSelected));

            editorWindow.CallDeferred("add_child", editor);
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