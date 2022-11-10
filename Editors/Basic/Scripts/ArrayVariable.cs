using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
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
        private CheckBox seriesOverwrite;

        [Export]
        private Table table;


        private VariableControl setAllValue;
        private List<string> values;
        private List<float> fValues;
        private VariableBlock tpl;

        private PackedScene variableControlScene;

        public override void _Ready()
        {
            base._Ready();

            multByValue.MaxValue = float.MaxValue;
            multByValue.MinValue = float.MinValue;
            multByValue.CustomArrowStep = 1;
            multByValue.Step = 0.000001f;
            switch (entry.Template.Type)
            {
                case VariableType.real:
                    incrByValue.MaxValue = float.MaxValue;
                    incrByValue.MinValue = float.MinValue;
                    incrByValue.CustomArrowStep = 1;
                    incrByValue.Step = 0.000001f;
                    break;
                case VariableType.@int:
                    incrByValue.MaxValue = int.MaxValue;
                    incrByValue.MinValue = int.MinValue;
                    break;
                default:
                    DisableTools();
                    break;
            }
            incrByValue.GetLineEdit().TextSubmitted += (str) => SetInputAsHandled();
            multByValue.GetLineEdit().TextSubmitted += (str) => SetInputAsHandled();
            incrSeriesValue.TextSubmitted += (str) => SetInputAsHandled();
            incrSeriesValue.TextChanged += OnIncrSeriesChanged;

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
            SetInputAsHandled();
            for (int i = 0; i < values.Count; i++)
            {
                if (fValues.Count > i && TQNumberString.TryParseTQString(setAllValue.GetChangedValue(), out float fVal))
                    fValues[i] = fVal;
                values[i] = setAllValue.GetChangedValue();
            }
            Init();
        }

        private void OnIncrBy()
        {
            SetInputAsHandled();
            for (int i = 0; i < values.Count; i++)
                fValues[i] = fValues[i] + (float)incrByValue.Value;

            Init();
        }

        private void OnMultBy()
        {
            SetInputAsHandled();
            for (int i = 0; i < values.Count; i++)
                fValues[i] = fValues[i] * (float)multByValue.Value;

            Init();
        }

        private static IReadOnlyList<string> GetIncrSeriesElements(string value)
        {
            List<string> split = new();
            var semiSplit = value.Split(';');
            foreach (var semiS in semiSplit)
            {
                var commaSplit = semiS.Split(',');
                foreach (var commaS in commaSplit)
                    split.Add(commaS);
            }
            return split;
        }

        private void OnIncrSeriesChanged(string value)
        {
            var error = false;
            var split = GetIncrSeriesElements(value);
            foreach (var s in split)
            {
                if (entry.Template.Type == VariableType.real)
                {
                    if (!TQNumberString.TryParseTQString(s, out float _))
                    {
                        error = true;
                        break;
                    }
                }
                else if (!TQNumberString.TryParseTQString(s, out int _))
                {
                    error = true;
                    break;
                }
            }

            if (error)
            {
                incrSeriesValue.AddThemeColorOverride("font_color", Colors.Red);
                incrSeriesButton.Disabled = true;
            }
            else
            {
                incrSeriesValue.RemoveThemeColorOverride("font_color");
                incrSeriesButton.Disabled = false;
            }
        }

        private void OnIncrSeries()
        {
            SetInputAsHandled();

            var split = GetIncrSeriesElements(incrSeriesValue.Text);
            var series = new List<float>();

            foreach (var s in split)
            {
                if (TQNumberString.TryParseTQString(s, out float fVal))
                    series.Add(fVal);
                else
                {
                    series.Clear();
                    break;
                }
            }

            var shouldOverwrite = seriesOverwrite?.ButtonPressed ?? false;
            if (series.Count > 0)
            {
                int seriesIndex = 0;
                var lastValue = fValues[0];
                for (int i = 0; i < fValues.Count; i++)
                {
                    if (shouldOverwrite)
                    {
                        fValues[i] = lastValue + series[seriesIndex];
                        lastValue = fValues[i];
                    }
                    else
                    {
                        lastValue = fValues[i];
                        fValues[i] = lastValue + series[seriesIndex];
                    }

                    if (++seriesIndex >= series.Count)
                        seriesIndex = 0;
                }
            }

            Init();
        }

        private void OnNewRow()
        {
            SetInputAsHandled();
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
            SetInputAsHandled();
            var indices = table.GetFocussedRows();
            if (indices.Count == 0)
                return;
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

            var nextIndex = indices[0];
            if (nextIndex == 0)
                return;
            if (nextIndex > values.Count - 1)
                nextIndex--;
            table.FocusRow(nextIndex);
        }

        private void OnMoveRowUp()
        {
            SetInputAsHandled();
            var indices = table.GetFocussedRows();
            if (indices.Count == 0)
                return;
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
            SetInputAsHandled();
            var indices = table.GetFocussedRows();
            if (indices.Count == 0)
                return;
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
            SetInputAsHandled();
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
                cVar.DBRFile = DBRFile;
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
                case VariableType.@int:
                    if (fValues.Count > index)
                    {
                        value = ((int)fValues[index]).ToTQString();
                        break;
                    }
                    if (TQNumberString.TryParseTQString(value, out int iValue))
                        fValues.Add(iValue);
                    else
                        fValues.Add(0);
                    break;
                case VariableType.real:
                    if (fValues.Count > index)
                    {
                        value = fValues[index].ToTQString();
                        break;
                    }
                    if (TQNumberString.TryParseTQString(value, out float fValue))
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
                cVar.DBRFile = DBRFile;
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