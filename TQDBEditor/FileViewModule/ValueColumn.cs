using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
using TQDBEditor.Controls;
using TQDBEditor.FileViewModule.Controls;
using TQDBEditor.FileViewModule.ViewModels;

namespace TQDBEditor.FileViewModule
{
    public class ValueColumn : ColumnBase<MyVariableRow>
    {
        private readonly int _index;
        private readonly IDialogService _dialogService;

        public ValueColumn(object? header, int index, IDialogService dialogService, GridLength? width = default, ColumnOptions<MyVariableRow>? options = null) : base(header, width, options ?? new())
        {
            _index = index;
            _dialogService = dialogService;
        }

        public override ICell CreateCell(IRow<MyVariableRow> row)
        {
            var model = row.Model;
            if (model.VariableBlock.Class == TQDB_Parser.VariableClass.@static)
                return new TemplateCell(model, x => CreateCellDataTemplate(_index), null, null);
            return new TemplateCell(model, x => CreateCellDataTemplate(_index), x => CreateCellDataTemplate(_index, true), null);
        }

        public override Comparison<MyVariableRow?>? GetComparison(ListSortDirection direction)
        {
            if (!(Options.CanUserSortColumn ?? true))
                return null;
            return (a, b) => CompareVariableRows(a, b) * (direction == ListSortDirection.Descending ? -1 : 1);
        }

        private int CompareVariableRows(MyVariableRow? a, MyVariableRow? b)
        {
            return string.Compare(a?.Entries[_index].Value, b?.Entries[_index].Value);
        }

        private IDataTemplate CreateCellDataTemplate(int valueIndex, bool editing = false)
        {
            var ret = new FuncDataTemplate<MyVariableRow>((x, _) => CreateControlForVariable(x, valueIndex, editing), true);
            ret.Build(null);
            return ret;
        }

        private Control? CreateControlForVariable(MyVariableRow variable, int valueIndex, bool editing)
        {
            if (variable is null)
                return null;
            var varTpl = variable.VariableBlock;
            DBREntry varEntry = variable.Entries[valueIndex];
            Control? ret = null;
            if (!editing)
            {
                var binding = new Binding
                {
                    Source = varEntry,
                    Mode = BindingMode.OneWay,
                    Converter = new BBCodeConverter(),
                };
                var richBlock = new RichTextBlock() { UseBBCode = true };
                richBlock.Bind(TextBlock.TextProperty, binding);
                ret = richBlock;
            }
            else
            {
                var binding = new Binding
                {
                    Source = varEntry,
                    Path = nameof(DBREntry.Value),
                    ConverterParameter = varEntry,
                    Converter = new TQStringConverter(),
                };
                switch (variable.VariableBlock.Class)
                {
                    case TQDB_Parser.VariableClass.picklist:
                        var validValues = varTpl.DefaultValue.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        var comboBox = new ComboBox() { ItemsSource = validValues };
                        binding.Converter = null;
                        comboBox.Bind(SelectingItemsControl.SelectedItemProperty, binding);
                        ret = comboBox;
                        break;
                    case TQDB_Parser.VariableClass.array:
                        var arrayEdit = new AdvancedEdit()
                        {
                            DataContext = new ArrayEditViewModel(varEntry, _dialogService),
                        };
                        ret = arrayEdit;
                        break;
                };
                if (ret is null)
                    switch (variable.VariableBlock.Type)
                    {
                        case TQDB_Parser.VariableType.@bool:
                            var checkBox = new CheckBox();
                            checkBox.Bind(ToggleButton.IsCheckedProperty, binding);
                            ret = checkBox;
                            break;
                        case TQDB_Parser.VariableType.@int:
                        case TQDB_Parser.VariableType.real:
                            var numeric = new NumericUpDown() { Increment = variable.VariableBlock.Type == TQDB_Parser.VariableType.real ? 0.1M : 1, ButtonSpinnerLocation = Location.Left };
                            numeric.Bind(NumericUpDown.ValueProperty, binding);
                            ret = numeric;
                            break;
                        case TQDB_Parser.VariableType.file:
                            var fileEdit = new AdvancedEdit()
                            {
                                DataContext = new FileEditViewModel(varEntry, _dialogService),
                            };
                            ret = fileEdit;
                            break;
                        case TQDB_Parser.VariableType.equation:
                            var eqnEdit = new AdvancedEdit()
                            {
                                DataContext = new EquationEditViewModel(varEntry, _dialogService),
                            };
                            ret = eqnEdit;
                            break;
                    }
                if (ret is null)
                {
                    var textBox = new TextBox();
                    textBox.Bind(TextBox.TextProperty, binding);
                    ret = textBox;
                }
            }
            Thickness? padding = null;
            if ((Application.Current?.Styles.TryGetResource(typeof(TreeDataGridTextCell), Application.Current?.ActualThemeVariant, out var resource) ?? false) && resource is ControlTheme ctrlTheme)
            {
                padding = ctrlTheme.Setters.OfType<Setter>().SingleOrDefault(x => x.Property == TemplatedControl.PaddingProperty)?.Value as Thickness?;
            }
            ret.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            ret = new Border
            {
                Child = ret,
                Padding = padding ?? new(4, 2),
            };
            return ret;
        }

        class BBCodeConverter : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (value is DBREntry varEntry)
                {
                    var varValueBBC = varEntry.Value;
                    if (!varEntry.IsValid())
                    {
                        var valSplit = varValueBBC.Split(';');
                        if (valSplit.Length > 1)
                        {
                            var invalidIndices = varEntry.InvalidIndices;
                            foreach (var index in invalidIndices)
                                valSplit[index] = "[color=red]" + valSplit[index] + "[/color]";
                            varValueBBC = string.Join(';', valSplit);
                        }
                        else
                            varValueBBC = "[color=red]" + varValueBBC + "[/color]";
                    }
                    return varValueBBC;
                }

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }
        }

        class TQStringConverter : IValueConverter
        {
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if ((targetType == typeof(decimal) || targetType == typeof(decimal?)) && TQNumberString.TryParseTQString(value as string, out float fResult))
                    return (decimal?)fResult;
                if ((targetType == typeof(bool) || targetType == typeof(bool?)) && TQNumberString.TryParseTQString(value as string, out bool bResult))
                    return (bool?)bResult;
                if (targetType == typeof(string))
                    return new string(value as string);

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }

            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                if (targetType == typeof(string) && parameter is DBREntry entry)
                {
                    string? ret = null;
                    switch (value)
                    {
                        case int i:
                            ret = i.ToTQString();
                            break;
                        case bool b:
                            ret = b ? 1.ToTQString() : 0.ToTQString();
                            break;
                        case float f:
                            ret = f.ToTQString();
                            break;
                        case decimal d:
                            var fd = (float)d;
                            var id = (int)d;
                            ret = entry.Template.Type == TQDB_Parser.VariableType.real ? fd.ToTQString() : id.ToTQString();
                            break;
                        case string s:
                            ret = s;
                            break;
                    }
                    if (ret != null)
                        entry.UpdateValue(ret);
                    return ret;
                }

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }
        }
    }
}
