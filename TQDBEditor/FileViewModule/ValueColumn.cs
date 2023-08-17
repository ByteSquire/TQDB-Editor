using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Styling;
using Prism.Services.Dialogs;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDB_Parser.Extensions;
using TQDBEditor.Controls;
using TQDBEditor.FileViewModule.Controls;
using TQDBEditor.FileViewModule.ViewModels;

namespace TQDBEditor.FileViewModule
{
    public interface IValueColumnFactory
    {
        ValueColumn CreateValueColumn(DBRFile file, GridLength? width = default, ColumnOptions<MyVariableRow>? options = null);
    }

    internal class ValueColumnFactory : IValueColumnFactory
    {
        private readonly ICreateControlForVariable _controlProvider;

        public ValueColumnFactory(ICreateControlForVariable controlProvider)
        {
            _controlProvider = controlProvider;
        }

        public ValueColumn CreateValueColumn(DBRFile file, GridLength? width = null, ColumnOptions<MyVariableRow>? options = null)
        {
            return new(_controlProvider, file, width, options);
        }
    }

    public interface ICreateControlForVariable
    {
        Control? CreateControl(DBRFile file, VariableBlock varTpl, ObservableEntry varEntry, bool editing);
    }

    public class VariableControlProvider : ICreateControlForVariable
    {
        private readonly IDialogService _dialogService;

        public VariableControlProvider(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public Control? CreateControl(DBRFile file, VariableBlock varTpl, ObservableEntry varEntry, bool editing)
        {
            Control? ret = null;
            var binding = new Binding
            {
                Source = varEntry,
                Path = nameof(ObservableEntry.Value),
                ConverterParameter = varEntry,
            };
            if (!editing)
            {
                binding.Converter = new BBCodeConverter();
                var richBlock = new RichTextBlock() { UseBBCode = true };
                richBlock.Bind(TextBlock.TextProperty, binding);
                ret = richBlock;
            }
            else
            {
                binding.Converter = new TQStringConverter();
                switch (varTpl.Class)
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
                    switch (varTpl.Type)
                    {
                        case TQDB_Parser.VariableType.@bool:
                            var checkBox = new CheckBox();
                            checkBox.Bind(ToggleButton.IsCheckedProperty, binding);
                            ret = checkBox;
                            break;
                        case TQDB_Parser.VariableType.@int:
                        case TQDB_Parser.VariableType.real:
                            var numeric = new NumericUpDown() { Increment = varTpl.Type == TQDB_Parser.VariableType.real ? 0.1M : 1, ButtonSpinnerLocation = Location.Left };
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
                                DataContext = new EquationEditViewModel(file.TemplateRoot, varEntry, _dialogService),
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
                if (value is string sValue && parameter is ObservableEntry entry)
                {
                    var varEntry = (DBREntry)entry;
                    var varValueBBC = sValue;
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
                if (targetType == typeof(string) && parameter is ObservableEntry entry)
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
                            ret = ((DBREntry)entry).Template.Type == TQDB_Parser.VariableType.real ? fd.ToTQString() : id.ToTQString();
                            break;
                        case string s:
                            ret = s;
                            break;
                    }
                    //if (ret != null)
                    entry.UpdateValue(ret ?? string.Empty);
                    return ret;
                }

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }
        }
    }

    public class ValueColumn : ColumnBase<MyVariableRow>
    {
        private readonly DBRFile _file;
        private readonly ICreateControlForVariable _controlProvider;

        public ValueColumn(ICreateControlForVariable controlProvider, DBRFile file, GridLength? width = default, ColumnOptions<MyVariableRow>? options = null) : base(file.FileName, width, options ?? new())
        {
            _file = file;
            _controlProvider = controlProvider;
        }

        public override ICell CreateCell(IRow<MyVariableRow> row)
        {
            var model = row.Model;
            if (model.VariableBlock.Class == TQDB_Parser.VariableClass.@static)
                return new TemplateCell(model, x => CreateCellDataTemplate(), null, null);
            return new TemplateCell(model, x => CreateCellDataTemplate(), x => CreateCellDataTemplate(true), null);
        }

        public override Comparison<MyVariableRow?>? GetComparison(ListSortDirection direction)
        {
            if (!(Options.CanUserSortColumn ?? true))
                return null;
            return (a, b) => CompareVariableRows(a, b) * (direction == ListSortDirection.Descending ? -1 : 1);
        }

        private int CompareVariableRows(MyVariableRow? a, MyVariableRow? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return string.Compare(_file[a.VariableBlock.Name].Value, _file[b.VariableBlock.Name].Value);
        }

        private IDataTemplate CreateCellDataTemplate(bool editing = false)
        {
            var ret = new FuncDataTemplate<MyVariableRow>((x, _) => _controlProvider.CreateControl(_file, x.VariableBlock, new(_file[x.VariableBlock.Name]), editing), true);
            return ret;
        }
    }
}
