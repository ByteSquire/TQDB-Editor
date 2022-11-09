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
            var focussed = table.GetFocussedRows();
            int myIndex;
            if (focussed.Count > 0)
            {
                myIndex = focussed[^1];
                myIndex++;
                values.Insert(myIndex, string.Empty);
                if (entry.Template.Type == VariableType.@int || entry.Template.Type == VariableType.real)
                    fValues.Insert(myIndex, 0);

                table.InsertRow(myIndex, CreateRow(myIndex, string.Empty));
            }
            else
            {
                values.Add(string.Empty);
                myIndex = values.Count - 1;
                AddRow(string.Empty, myIndex);
            }

            UpdateIndices();

            table.FocusRow(myIndex);
        }

        private void OnDelRow()
        {
            var indices = table.GetFocussedRows();
            foreach (var index in indices.Reverse())
            {
                if (index < 0)
                    return;

                values.RemoveAt(index);
                if (fValues.Count > 0)
                    fValues.RemoveAt(index);

                table.RemoveRow(index);
            }

            UpdateIndices();

            table.FocusRow(indices[0]);
        }

        private void OnMoveRowUp()
        {
            var indices = table.GetFocussedRows();
            for (int i = 0; i < indices.Count; i++)
            {
                var index = indices[i];
                if (index < 1)
                    continue;
                if (indices[0] < 1 && i > 0 && index - indices[i - 1] <= 1)
                    continue;

                var indexB = index - 1;
                SwapValues(index, indexB);
            }

            UpdateIndices();
        }

        private void OnMoveRowDown()
        {
            var indices = table.GetFocussedRows();
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                var index = indices[i];
                if (index >= values.Count - 1)
                    continue;
                if (indices[^1] >= values.Count - 1 && i < values.Count - 1 && indices[i + 1] - index <= 1)
                    continue;

                var indexB = index + 1;
                SwapValues(index, indexB);
            }

            UpdateIndices();
        }

        private void SwapValues(int indexA, int indexB)
        {
            var valueA = values[indexA];
            var valueB = values[indexB];

            values[indexA] = valueB;
            values[indexB] = valueA;

            table.SwapRows(indexA, indexB);

            if (fValues.Count > 0)
            {
                var fValueA = fValues[indexA];
                var fValueB = fValues[indexB];
                fValues[indexA] = fValueB;
                fValues[indexB] = fValueA;
            }
        }

        private void UpdateIndices()
        {
            var indicesLabels = table.EnumerateColumn(0).Select(x => x as Label).Where(x => x != null);

            int i = 0;
            foreach (var indexLabel in indicesLabels)
            {
                indexLabel.Text = i++.ToString();
            }
        }

        protected override string GetChangedValue()
        {
            return string.Join(";", values);
        }

        protected override void InitVariable(DBREntry entry)
        {
            tpl = entry.Template;
            if (this.TryGetEntryControlScene(out var cScene, tpl.Name, VariableClass.variable.ToString(), tpl.Type.ToString()))
                variableControlScene = cScene;

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

            table.AddRow(CreateRow(index, value));
        }

        private Godot.Collections.Array<Control> CreateRow(int index, string value)
        {
            var row = new Godot.Collections.Array<Control>
            {
                new Label()
                {
                    Text = index.ToString(),
                    FocusMode = Control.FocusModeEnum.All,
                    MouseFilter = Control.MouseFilterEnum.Stop
                }
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
            row.Add(second);
            return row;
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