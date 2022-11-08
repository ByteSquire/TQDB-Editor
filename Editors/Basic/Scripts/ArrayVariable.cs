using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class ArrayVariable : EditorDialog
    {
        [Export]
        private Button setAllButton;
        [Export]
        private Button incrByButton;
        [Export]
        private Button multByButton;
        [Export]
        private Button incrSeriesButton;

        [Export]
        private Button newRowButton;
        [Export]
        private Button deleteRowButton;
        [Export]
        private Button moveRowUpButton;
        [Export]
        private Button moveRowDownButton;


        [Export]
        private Control setAllPlaceholder;
        [Export]
        private SpinBox incrByValue;
        [Export]
        private SpinBox multByValue;

        [Export]
        private LineEdit incrSeriesValue;

        [Export]
        private GridContainer toolGrid;

        [Export]
        private Table table;


        private VariableControl setAllValue;
        private List<string> values;
        private List<double> fValues;
        private VariableBlock tpl;

        private PackedScene variableControlScene;

        public override void _Ready()
        {
            base._Ready();

            if (this.TryGetEntryControlScene(out var cScene, tpl.Name, VariableClass.variable.ToString(), tpl.Type.ToString()))
                variableControlScene = cScene;

            switch (entry.Template.Type)
            {
                case VariableType.real:
                case VariableType.@int:
                    break;
                default:
                    DisableTools();
                    break;
            }

            setAllButton.Pressed += OnSetAll;
            incrByButton.Pressed += OnIncrBy;
            multByButton.Pressed += OnMultBy;
            incrSeriesButton.Pressed += OnIncrSeries;

            newRowButton.Pressed += OnNewRow;
            deleteRowButton.Pressed += OnDelRow;
            moveRowUpButton.Pressed += OnMoveRowUp;
            moveRowDownButton.Pressed += OnMoveRowDown;
        }

        private void DisableTools()
        {
            incrByValue.Visible = false;
            incrByButton.Visible = false;
            multByValue.Visible = false;
            multByButton.Visible = false;
            incrSeriesButton.Visible = false;
            incrSeriesValue.Visible = false;
        }

        private void OnSetAll()
        {
            for (int i = 0; i < values.Count; i++)
                values[i] = setAllValue.GetChangedValue();

            Init();
        }

        private void OnIncrBy()
        {
            for (int i = 0; i < values.Count; i++)
                fValues[i] = fValues[i] + incrByValue.Value;

            Init();
        }

        private void OnMultBy()
        {
            for (int i = 0; i < values.Count; i++)
                fValues[i] = fValues[i] * multByValue.Value;

            Init();
        }

        private void OnIncrSeries()
        {


            Init();
        }

        private void OnNewRow()
        {
            values.Add(string.Empty);

            AddRow(string.Empty, values.Count);
        }

        private void OnDelRow()
        {
            var index = table.GetCellPosition(GuiGetFocusOwner()).y;
            if (index < 0)
                return;

            values.RemoveAt(index);
            table.RemoveRow(index);
        }

        private void OnMoveRowUp()
        {

        }

        private void OnMoveRowDown()
        {

        }

        protected override string GetChangedValue()
        {
            return string.Join(";", values);
        }

        protected override void InitVariable(DBREntry entry)
        {
            tpl = entry.Template;
            values = entry.Value.Split(';').ToList();
            fValues = new(values.Count);

            try
            {
                var cVar = variableControlScene.Instantiate<VariableControl>();
                cVar.Entry = new DBREntry(tpl, string.Empty);
                cVar.Submitted += () => setAllButton._Pressed();
                setAllValue = cVar;
                toolGrid.AddChild(setAllValue);
                toolGrid.MoveChild(setAllValue, setAllPlaceholder.GetIndex());
                toolGrid.RemoveChild(setAllPlaceholder);
            }
            catch (InvalidCastException)
            {
                this.GetConsoleLogger()?.LogError("Error instantiating {scene}, does not extend {type}", variableControlScene.ResourcePath, typeof(VariableControl));
            }

            Init();
        }

        private void Init()
        {
            table.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                AddRow(value, i);
            }
        }

        private void AddRow(string value, int index)
        {
            switch (entry.Template.Type)
            {
                case VariableType.real:
                case VariableType.@int:
                    if (float.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var fValue))
                        fValues.Add(fValue);
                    else
                        fValues.Add(0);
                    break;
            }
            var row = new Control[2];
            row[0] = new Label()
            {
                Text = index.ToString(),
            };
            Control second = new();
            try
            {
                var cVar = variableControlScene.Instantiate<VariableControl>();
                cVar.Entry = new DBREntry(tpl, value);
                cVar.Submitted += () => values[index] = cVar.GetChangedValue();
                second = cVar;
            }
            catch (InvalidCastException)
            {
                this.GetConsoleLogger()?.LogError("Error instantiating {scene}, does not extend {type}", variableControlScene.ResourcePath, typeof(VariableControl));
            }
            row[1] = second;
            table.AddRow(new Godot.Collections.Array<Control>(row));
        }
    }

    static class Test
    {
        public static bool TryGetVariableControl(this Node self, out VariableControl variable, string vName = null, string vClass = null, string vType = null)
        {
            var pckHandler = self.GetPCKHandler();
            var controls = pckHandler.GetEntryControls(vName, vClass, vType);
            variable = null;
            if (controls != null && controls.Count > 0)
            {
                var controlScene = controls[0];
                try
                {
                    var entryControl = controlScene.Instantiate<VariableControl>();
                    variable = entryControl;
                }
                catch (InvalidCastException)
                {
                    self.GetConsoleLogger()?.LogError("Error instantiating {scene}, does not extend {type}", controlScene.ResourcePath, typeof(VariableControl));
                    return false;
                }
            }
            return true;
        }

        public static bool TryGetEntryControlScene(this Node self, out PackedScene controlScene, string vName = null, string vClass = null, string vType = null)
        {
            var pckHandler = self.GetPCKHandler();
            var controls = pckHandler.GetEntryControls(vName, vClass, vType);
            controlScene = null;
            if (controls != null && controls.Count > 0)
            {
                controlScene = controls[0];
            }
            return true;
        }
    }
}