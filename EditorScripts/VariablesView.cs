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

        private Config config;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView.GroupSelected += OnGroupSelected;

            var column1Scroll = column1.GetVScrollBar();
            column1Scroll.Scale = new Vector2(0, 0);

            var column2Scroll = column2.GetVScrollBar();
            column2Scroll.Scale = new Vector2(0, 0);

            var column3Scroll = column3.GetVScrollBar();
            column3Scroll.Scale = new Vector2(0, 0);

            var column4Scroll = column4.GetVScrollBar();
            column4Scroll.Scale = new Vector2(0, 0);

            var column5Scroll = column5.GetParent<ScrollContainer>().GetVScrollBar();

            if (column1 is FileList col1)
            {
                col1.otherLists = new ItemList[] { column2, column3, column4 };
                col1.syncedScrollBars = new VScrollBar[]
                {
                    column2Scroll,
                    column3Scroll,
                    column4Scroll,
                    column5Scroll
                };
            }

            if (column2 is FileList col2)
            {
                col2.otherLists = new ItemList[] { column1, column3, column4 };
                col2.syncedScrollBars = new VScrollBar[]
                {
                    column1Scroll,
                    column3Scroll,
                    column4Scroll,
                    column5Scroll
                };
            }

            if (column3 is FileList col3)
            {
                col3.otherLists = new ItemList[] { column2, column1, column4 };
                col3.syncedScrollBars = new VScrollBar[]
                {
                    column2Scroll,
                    column1Scroll,
                    column4Scroll,
                    column5Scroll
                };
            }

            if (column4 is FileList col4)
            {
                col4.otherLists = new ItemList[] { column2, column3, column1 };
                col4.syncedScrollBars = new VScrollBar[]
                {
                    column2Scroll,
                    column3Scroll,
                    column1Scroll,
                    column5Scroll
                };
            }

            if (column5.GetParent() is ScrollContainer column5Parent)
            {
                column5Parent.Set("other_scrolls", new VScrollBar[]
                {
                    column2Scroll,
                    column3Scroll,
                    column1Scroll,
                    column4Scroll
                });
            }

            config = this.GetEditorConfig();

            column1.GetParent<SplitContainer>().Call("set_synced_offset", config.NameColumnWidth - 100);
            column2.GetParent<SplitContainer>().Call("set_synced_offset", config.ClassColumnWidth - 100);
            column3.GetParent<SplitContainer>().Call("set_synced_offset", config.TypeColumnWidth - 100);
            column4.GetParent<SplitContainer>().Call("set_synced_offset", config.DescriptionColumnWidth - 100);
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
                var size = new Vector2i(10, 28);
                switch (variable.Class)
                {
                    case TQDB_Parser.VariableClass.variable:
                        valueElement = new LineEdit
                        {
                            Text = value,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = size,
                            Size = size,
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
                            CustomMinimumSize = size,
                            Size = size,
                        };
                        (valueElement as LineEdit).TextSubmitted += x => file.UpdateEntry(variable.Name, x);
                        //(valueElement as TextEdit).GetVScrollBar().Scale = new Vector2(0, 0);
                        //(valueElement as TextEdit).GetHScrollBar().Scale = new Vector2(0, 0);
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

                        (valueElement as OptionButton).Select((valueElement as OptionButton).GetItemIndex(valueId));
                        (valueElement as OptionButton).ItemSelected += x => file.UpdateEntry(variable.Name, (valueElement as OptionButton).GetItemText((int)x));
                        break;
                    case TQDB_Parser.VariableClass.array:
                        valueElement = new HBoxContainer();
                        valueElement.AddChild(new Button
                        {
                            Text = "...",
                            SizeFlagsVertical = (int)SizeFlags.Fill,
                            CustomMinimumSize = size,
                            Size = size,
                        });

                        var lineEdit = new LineEdit
                        {
                            Text = value,
                            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                            SizeFlagsVertical = (int)SizeFlags.ExpandFill,
                            PlaceholderText = variable.GetDefaultValue(),
                            CustomMinimumSize = size,
                            Size = size,
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
                valueElement.Size = valueElement.CustomMinimumSize = new Vector2i(100, size.y);

                column5.AddChild(valueElement);
            }
        }

        public override void _ExitTree()
        {
            config.NameColumnWidth = column1.GetParent<SplitContainer>().SplitOffset + 100;
            config.ClassColumnWidth = column2.GetParent<SplitContainer>().SplitOffset + 100;
            config.TypeColumnWidth = column3.GetParent<SplitContainer>().SplitOffset + 100;
            config.DescriptionColumnWidth = column4.GetParent<SplitContainer>().SplitOffset + 100;
            base._ExitTree();
        }
    }
}