using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor;
using TQDBEditor.Common;
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
        private PackedScene variableInfoCell;
        [Export]
        private EditorWindow editorWindow;

        private TableColumn nameColumn;
        private TableColumn classColumn;
        private TableColumn typeColumn;
        private TableColumn descriptionColumn;
        private TableColumn valueColumn;

        private Config config;
        private PCKHandler pckHandler;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView.GroupSelected += OnGroupSelected;
            editorWindow.Reinit += OnGroupSelected;

            config = this.GetEditorConfig();
            if (config.ValidateConfig())
                Init();
            config.TrulyReady += Init;

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

        private void Init()
        {
            pckHandler = this.GetPCKHandler();
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
            try
            {
                var variables = currentVariables = editorWindow.DBRFile[group].Select(x => x.Template).ToList();
                var file = editorWindow.DBRFile;
                variableRowMap = new();

                foreach (var variable in variables)
                {
                    var row = new Control[5];
                    var nameLabel = variableInfoCell.Instantiate<Label>();
                    nameLabel.Text = variable.Name;
                    nameLabel.Connect("activated", Callable.From<Label>(OnEditEntry));
                    var classLabel = variableInfoCell.Instantiate<Label>();
                    classLabel.Text = variable.Class.ToString();
                    classLabel.Connect("activated", Callable.From<Label>(OnEditEntry));
                    var typeLabel = variableInfoCell.Instantiate<Label>();
                    typeLabel.Text = variable.Type.ToString();
                    if (variable.Type == VariableType.file)
                        typeLabel.Text += '(' + string.Join(",", variable.FileExtensions) + ')';
                    typeLabel.Connect("activated", Callable.From<Label>(OnEditEntry));
                    var descriptionLabel = variableInfoCell.Instantiate<Label>();
                    descriptionLabel.Text = variable.Description;
                    descriptionLabel.Connect("activated", Callable.From<Label>(OnEditEntry));

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
            catch (ArgumentException e)
            {
                GD.PushError("oopsie, passed a group that's not inside the file, origin: GroupsView");
                GD.PushError(e.Message);
            }
        }

        private Control CreateValueControl(VariableBlock variable, string value, DBRFile file)
        {
            Control valueElement;

            var variableControls = pckHandler.GetEntryControls(variable.Name, variable.Class.ToString(), variable.Type.ToString());
            var variableEditors = pckHandler.GetEntryEditors(variable.Name, variable.Class.ToString(), variable.Type.ToString());
            if (variableControls != null && variableControls.Count > 0)
            {
                var controlScene = variableControls[0];
                try
                {
                    var variableControl = controlScene.Instantiate<VariableControl>();
                    variableControl.EditorWindow = editorWindow;
                    variableControl.VarName = variable.Name;
                    variableControl.DBRFile = file;

                    valueElement = variableControl;
                }
                catch (InvalidCastException)
                {
                    this.GetConsoleLogger()?.LogError("Error instantiating {scene}, does not extend {type}", controlScene.ResourcePath, typeof(VariableControl));
                    valueElement = new();
                }
            }
            else
            {
                this.GetConsoleLogger().LogError("No control found for variable {var}", variable.Name);
                valueElement = new();
            }

            if (variableEditors != null && variableControls.Count > 0)
            {
                var extendedElement = new HBoxContainer();
                var editButton = new Button()
                {
                    AnchorsPreset = (int)LayoutPreset.LeftWide,
                    Text = "..."
                };
                editButton.Pressed += () => OnEditEntry(extendedElement);

                extendedElement.AddChild(editButton);
                extendedElement.AddChild(valueElement);

                valueElement = extendedElement;
            }

            return valueElement;
        }

        private void OnEditEntry(Control controlInTable)
        {
            var index = table.GetCellPosition(controlInTable).y;
            EditEntry(index);
        }

        public void SelectEntry()
        {
            var entry = editorWindow.GetFocussedEntry();
            var row = table.GetRow(variableRowMap[entry.Name]);

            row[0].FocusMode = FocusModeEnum.All;
            row[0].GrabFocus();
        }

        private void EditEntry(int index)
        {
            var row = table.GetRow(index);
            var vName = (row[0] as Label).Text;
            var vClass = (row[1] as Label).Text;
            var vType = (row[2] as Label).Text.Split('(')[0];

            var availableEditors = pckHandler.GetEntryEditors(vName, vClass, vType);

            EditorDialog editor;
            if (availableEditors is null || availableEditors.Count < 1)
            {
                this.GetConsoleLogger().LogError("No editors found for variable {var}", vName);
                return;
            }
            else
            {
                var editorScene = availableEditors[0];
                try
                {
                    editor = editorScene.Instantiate<EditorDialog>();
                    editor.DBRFile = editorWindow.DBRFile;
                    editor.VarName = vName;

                    editor.Confirmed += () => CallDeferred(nameof(OnGroupSelected));

                    editorWindow.CallDeferred("add_child", editor);
                }
                catch (InvalidCastException)
                {
                    this.GetConsoleLogger()?.LogError("Error instantiating {scene}, does not extend {type}", editorScene.ResourcePath, typeof(EditorDialog));
                }
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