using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using ImTools;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TQDB_Parser;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
using static ImTools.ImMap;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public partial class ArrayEditDialogViewModel : EditDialogViewModelBase
    {
        public override string Title => "Edit the Array";

        public override bool CanConfirmDialog() => true;

        [ObservableProperty]
        private string? _seriesText;

        private readonly IBrush? _defaultSeriesForeground;
        [ObservableProperty]
        private IBrush? _seriesForeground;

        private bool _seriesValid = false;

        [ObservableProperty]
        private FlatTreeDataGridSource<TreeRow> _treeSource;
        private readonly ObservableCollection<TreeRow> _treeRows = new();

        [ObservableProperty]
        private Control? _setAllContent;

        [ObservableProperty]
        private Control? _increaseAllContent;

        [ObservableProperty]
        private Control? _multiplyAllContent;

        [ObservableProperty]
        private bool _shouldOverwrite = true;

        private readonly ICreateControlForVariable _variableControlProvider;
        private VariableBlock? _tmpTpl;
        private DBREntry? _setAllEntry;
        private DBREntry? _increaseAllEntry;
        private DBREntry? _multiplyAllEntry;

        public ArrayEditDialogViewModel(ICreateControlForVariable variableControlProvider)
        {
            _variableControlProvider = variableControlProvider;
            _treeSource = new(_treeRows)
            {
                Columns =
                {
                    new TextColumn<TreeRow, int>("index", x => x.Index),
                }
            };
            if (_treeSource.RowSelection != null)
                _treeSource.RowSelection.SingleSelect = false;

            if ((Application.Current?.Styles.TryGetResource(typeof(TextBox), Application.Current?.ActualThemeVariant, out var resource) ?? false) && resource is ControlTheme ctrlTheme)
            {
                var res = ctrlTheme.Setters.OfType<Setter>().SingleOrDefault(x => x.Property == TemplatedControl.ForegroundProperty)?.Value as DynamicResourceExtension;
                if (res?.ResourceKey != null && (Application.Current?.Styles.TryGetResource(res.ResourceKey, Application.Current?.ActualThemeVariant, out var resource1) ?? false) && resource1 is IBrush defaultBrush)
                {
                    _defaultSeriesForeground = defaultBrush;
                }
            }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (LocalVariable == null)
                return;
            var fileTpl = parameters.GetTemplateGroup();

            _tmpTpl = new(string.Empty, string.Empty, new Dictionary<string, string> { { "name", "tmp" }, { "class", VariableClass.variable.ToString() }, { "type", LocalVariable.Type.ToString() }, { "defaultValue", LocalVariable.DefaultValue } }, Array.Empty<Block>());

            var values = LocalVariable.Value!.Split(';');
            foreach (var item in values)
            {
                AddTreeElement(value: item);
            }
            TreeSource.Columns.Add(new TemplateColumn<TreeRow>(
                "value",
                new FuncDataTemplate<TreeRow>((x, _) => _variableControlProvider.CreateControl(fileTpl, x, true)), width: GridLength.Star)
            );

            _setAllEntry = new(_tmpTpl);
            _increaseAllEntry = new(_tmpTpl);
            _multiplyAllEntry = new(_tmpTpl);

            SetAllContent = _variableControlProvider.CreateControl(fileTpl, new DBREntryVariableProvider(_setAllEntry), true);
            if (_tmpTpl.Type == VariableType.real || _tmpTpl.Type == VariableType.@int)
            {
                IncreaseAllContent = _variableControlProvider.CreateControl(fileTpl, new DBREntryVariableProvider(_increaseAllEntry), true);
                MultiplyAllContent = _variableControlProvider.CreateControl(fileTpl, new DBREntryVariableProvider(_multiplyAllEntry), true);
            }
        }

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            if (LocalVariable != null)
                LocalVariable.Value = (string.Join(';', _treeRows.OrderBy(x => x.Index).Select(x => x.Value)));
            return base.OnDialogConfirmed(e);
        }

        private int? AddTreeElement(int? index = null, string? value = null)
        {
            if (_tmpTpl == null) return null;
            index ??= _treeRows.Count;
            _treeRows.Insert(index.Value, new(_treeRows, _tmpTpl) { Value = value ?? _tmpTpl.GetDefaultValue() });
            return index;
        }

        public void SetAll(object? _)
        {
            if (_setAllEntry == null) return;
            var value = _setAllEntry.Value;
            foreach (var treeRow in _treeRows)
            {
                treeRow.Value = value;
            }
        }

        public void IncreaseAll(object? _)
        {
            if (_increaseAllEntry == null) return;
            var value = _increaseAllEntry.Value;
            if (TQNumberString.TryParseTQString(value, out float res2))
                foreach (var treeRow in _treeRows)
                {
                    if (TQNumberString.TryParseTQString(treeRow.Value, out float res1))
                    {
                        treeRow.Value = (res1 + res2).ToTQString(treeRow.VariableBlock.Type == VariableType.@int);
                    }
                }
        }

        public void MultiplyAll(object? _)
        {
            if (_multiplyAllEntry == null) return;
            var value = _multiplyAllEntry.Value;
            if (TQNumberString.TryParseTQString(value, out float res2))
                foreach (var treeRow in _treeRows)
                {
                    if (TQNumberString.TryParseTQString(treeRow.Value, out float res1))
                    {
                        treeRow.Value = (res1 * res2).ToTQString(treeRow.VariableBlock.Type == VariableType.@int);
                    }
                }
        }

        private IReadOnlyList<string> GetIncrSeriesElements()
        {
            if (SeriesText == null) return Array.Empty<string>();
            List<string> split = new();
            var semiSplit = SeriesText.Split(';');
            foreach (var semiS in semiSplit)
            {
                var commaSplit = semiS.Split(',');
                foreach (var commaS in commaSplit)
                    split.Add(commaS);
            }
            return split;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(SeriesText))
                OnIncrSeriesChanged();
        }

        private void OnIncrSeriesChanged()
        {
            if (_tmpTpl == null) return;
            var error = false;
            var split = GetIncrSeriesElements();
            foreach (var s in split)
            {
                if (_tmpTpl.Type == VariableType.real)
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

            if (_defaultSeriesForeground != null)
                if (error)
                {
                    SeriesForeground = new SolidColorBrush(Colors.Red);
                    _seriesValid = false;
                }
                else
                {
                    SeriesForeground = _defaultSeriesForeground;
                    _seriesValid = true;
                }
        }

        public void IncreaseBySeries(object? _)
        {
            if (SeriesText == null || _tmpTpl == null || !_seriesValid) return;

            var split = GetIncrSeriesElements();
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

            var shouldOverwrite = ShouldOverwrite;
            if (series.Count > 0)
            {
                int seriesIndex = 0;
                var lastValue = _treeRows[0];
                foreach (var row in _treeRows)
                {
                    if (shouldOverwrite)
                    {
                        if (!TQNumberString.TryParseTQString(lastValue.Value, out float fVal)) continue;
                        row.Value = (fVal + series[seriesIndex]).ToTQString(_tmpTpl.Type == VariableType.@int);
                        lastValue = row;
                    }
                    else
                    {
                        lastValue = row;
                        if (!TQNumberString.TryParseTQString(lastValue.Value, out float fVal)) continue;
                        row.Value = (fVal + series[seriesIndex]).ToTQString(_tmpTpl.Type == VariableType.@int);
                    }

                    if (++seriesIndex >= series.Count)
                        seriesIndex = 0;
                }
            }
        }

        public void AddElement(object? _)
        {
            var selectedId = TreeSource.RowSelection?.SelectedIndex;
            int? index = selectedId?.Count > 0 ? TreeSource.RowSelection?.SelectedIndex[0] : null;
            if ((index = AddTreeElement(index)) != null && TreeSource.RowSelection != null)
                TreeSource.RowSelection.SelectedIndex = index.Value;
        }

        public void RemoveElements(object? _)
        {
            List<int>? indices = TreeSource.RowSelection?.SelectedIndexes.Where(x => x.Count > 0).Select(x => x[0]).ToList();
            if (indices != null)
            {
                indices.Sort();
                for (int i = indices.Count - 1; i >= 0; i--)
                    _treeRows.RemoveAt(indices[i]);
            }
        }

        public void MoveElementsUp(object? _)
        {
            List<int>? indices = TreeSource.RowSelection?.SelectedIndexes.Where(x => x.Count > 0).Select(x => x[0]).ToList();
            if (indices != null)
            {
                indices.Sort();
                foreach (var index in indices)
                {
                    if (index > 0)
                    {
                        var nIndex = index - 1;
                        (_treeRows[index], _treeRows[nIndex]) = (_treeRows[nIndex], _treeRows[index]);
                    }
                }
                TreeSource.RowSelection?.BeginBatchUpdate();
                foreach (var index in indices)
                {
                    if (index > 0)
                    {
                        var nIndex = index - 1;
                        TreeSource.RowSelection?.Select(nIndex);
                    }
                }
                TreeSource.RowSelection?.EndBatchUpdate();
            }
        }

        public void MoveElementsDown(object? _)
        {
            List<int>? indices = TreeSource.RowSelection?.SelectedIndexes.Where(x => x.Count > 0).Select(x => x[0]).ToList();
            if (indices != null)
            {
                indices.Sort();
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    var index = indices[i];
                    if (index < _treeRows.Count - 1)
                    {
                        var nIndex = index + 1;
                        (_treeRows[index], _treeRows[nIndex]) = (_treeRows[nIndex], _treeRows[index]);
                    }
                }
                TreeSource.RowSelection?.BeginBatchUpdate();
                foreach (var index in indices)
                {
                    if (index < _treeRows.Count - 1)
                    {
                        var nIndex = index + 1;
                        TreeSource.RowSelection?.Select(nIndex);
                    }
                }
                TreeSource.RowSelection?.EndBatchUpdate();
            }
        }

        public class TreeRow : IVariableRow, IVariableProvider
        {
            private readonly IList<TreeRow> _rows;
            public int Index => _rows.IndexOf(this);

            public VariableBlock VariableBlock { get; }

            public VariableClass Class => VariableBlock.Class;

            public VariableType Type => VariableBlock.Type;

            public string DefaultValue => VariableBlock.DefaultValue;

            public bool IsValid { get; private set; }

            public IReadOnlyList<int> InvalidIndices { get; private set; }

            private string? _value;
            public string? Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        if (Value != null && (IsValid = VariableBlock.ValidateValue(Value, out var invalid)))
                            InvalidIndices = invalid;
                        PropertyChanged?.Invoke(this, new(nameof(Value)));
                    }
                }
            }

            public TreeRow(IList<TreeRow> rows, VariableBlock variableBlock)
            {
                _rows = rows;
                if (rows is INotifyCollectionChanged notifyCollection)
                    notifyCollection.CollectionChanged += (_, _) => PropertyChanged?.Invoke(this, new(nameof(Index)));
                VariableBlock = variableBlock;
                InvalidIndices = Array.Empty<int>();
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
