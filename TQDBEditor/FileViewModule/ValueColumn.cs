﻿using Avalonia;
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using TQDB_Parser;
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
        ValueColumn<T> CreateValueColumn<T>(DBRFile file, GridLength? width = default, ColumnOptions<T>? options = null) where T : IVariableRow;
    }

    internal class ValueColumnFactory : IValueColumnFactory
    {
        private readonly ICreateControlForVariable _controlProvider;

        public ValueColumnFactory(ICreateControlForVariable controlProvider)
        {
            _controlProvider = controlProvider;
        }

        public ValueColumn<T> CreateValueColumn<T>(DBRFile file, GridLength? width = null, ColumnOptions<T>? options = null) where T : IVariableRow
        {
            return new(_controlProvider, file, width, options);
        }
    }

    public interface ICreateControlForVariable
    {
        Control? CreateControl(GroupBlock fileTpl, IVariableProvider varProvider, bool editing);
    }

    public interface IVariableProvider : INotifyPropertyChanged
    {
        VariableClass Class { get; }

        VariableType Type { get; }

        string DefaultValue { get; }

        bool IsValid { get; }

        IReadOnlyList<int> InvalidIndices { get; }

        string? Value { get; set; }
    }

    public class DBREntryVariableProvider : IVariableProvider
    {
        public VariableClass Class => _entry.Template.Class;

        public VariableType Type => _entry.Template.Type;

        public string DefaultValue => _entry.Template.GetDefaultValue();

        public bool IsValid => _entry.IsValid();

        public IReadOnlyList<int> InvalidIndices => _entry.InvalidIndices;

        public string? Value
        {
            get => _entry.Value;
            set
            {
                value ??= string.Empty;
                if (value != Value)
                {
                    _entry.UpdateValue(value);
                    PropertyChanged?.Invoke(this, new(nameof(Value)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly DBREntry _entry;

        public DBREntryVariableProvider(DBREntry entry)
        {
            _entry = entry;
        }
    }

    public class VariableControlProvider : ICreateControlForVariable
    {
        private readonly IDialogService _dialogService;

        public VariableControlProvider(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public Control? CreateControl(GroupBlock fileTpl, IVariableProvider varProvider, bool editing)
        {
            if (varProvider is null)
                return null;
            Control? ret = null;
            var binding = new Binding
            {
                Source = varProvider,
                Path = nameof(varProvider.Value),
                ConverterParameter = varProvider,
            };
            if (!editing)
            {
                binding.Converter = new BBCodeConverter();
                var richBlock = new RichTextBlock() { UseBBCode = true, [!TextBlock.TextProperty] = binding };
                ret = richBlock;
            }
            else
            {
                binding.Converter = new TQStringConverter();
                switch (varProvider.Class)
                {
                    case VariableClass.picklist:
                        var validValues = varProvider.DefaultValue.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        binding.Converter = null;
                        var comboBox = new ComboBox() { ItemsSource = validValues, [!SelectingItemsControl.SelectedItemProperty] = binding };
                        ret = comboBox;
                        break;
                    case VariableClass.array:
                        var arrayEdit = new AdvancedEdit()
                        {
                            DataContext = new ArrayEditViewModel(fileTpl, varProvider, _dialogService),
                        };
                        ret = arrayEdit;
                        break;
                };
                if (ret is null)
                    switch (varProvider.Type)
                    {
                        case VariableType.@bool:
                            var checkBox = new CheckBox() { [!ToggleButton.IsCheckedProperty] = binding };
                            ret = checkBox;
                            break;
                        case VariableType.@int:
                        case VariableType.real:
                            var numeric = new NumericUpDown()
                            {
                                Increment = varProvider.Type == VariableType.real ? 0.1M : 1,
                                ButtonSpinnerLocation = Location.Left,
                                [!NumericUpDown.ValueProperty] = binding,
                            };
                            ret = numeric;
                            break;
                        case VariableType.file:
                            var fileEdit = new AdvancedEdit()
                            {
                                DataContext = new FileEditViewModel(varProvider, _dialogService),
                            };
                            ret = fileEdit;
                            break;
                        case VariableType.equation:
                            var eqnEdit = new AdvancedEdit()
                            {
                                DataContext = new EquationEditViewModel(fileTpl, varProvider, _dialogService),
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
                if (value is string sValue && parameter is IVariableProvider variable)
                {
                    var varValueBBC = sValue;
                    if (!variable.IsValid)
                    {
                        var valSplit = varValueBBC.Split(';');
                        if (valSplit.Length > 1)
                        {
                            foreach (var index in variable.InvalidIndices)
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
                if (targetType == typeof(string) && parameter is IVariableProvider variable)
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
                            ret = variable.Type == VariableType.real ? fd.ToTQString() : id.ToTQString();
                            break;
                        case string s:
                            ret = s;
                            break;
                    }
                    if (ret != null)
                        return ret;
                }

                return new BindingNotification(null, BindingErrorType.DataValidationError);
            }
        }
    }

    public interface IVariableRow
    {
        public VariableBlock VariableBlock { get; }
    }

    public class ValueColumn<T> : ColumnBase<T> where T : IVariableRow
    {
        private readonly DBRFile _file;
        private readonly ICreateControlForVariable _controlProvider;

        public ValueColumn(ICreateControlForVariable controlProvider, DBRFile file, GridLength? width = default, ColumnOptions<T>? options = null) : base(file.FileName, width, options ?? new())
        {
            _file = file;
            _controlProvider = controlProvider;
        }

        public override ICell CreateCell(IRow<T> row)
        {
            var model = row.Model;
            if (model.VariableBlock.Class == VariableClass.@static)
                return new TemplateCell(model, x => CreateCellDataTemplate(), null, null);
            return new TemplateCell(model, x => CreateCellDataTemplate(), x => CreateCellDataTemplate(true), null);
        }

        public override Comparison<T?>? GetComparison(ListSortDirection direction)
        {
            if (!(Options.CanUserSortColumn ?? true))
                return null;
            return (a, b) => CompareVariableRows(a, b) * (direction == ListSortDirection.Descending ? -1 : 1);
        }

        private int CompareVariableRows(T? a, T? b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return string.Compare(_file[a.VariableBlock.Name].Value, _file[b.VariableBlock.Name].Value);
        }

        private IDataTemplate CreateCellDataTemplate(bool editing = false)
        {
            var ret = new FuncDataTemplate<IVariableRow>((x, _) => _controlProvider.CreateControl(_file.TemplateRoot, new DBREntryVariableProvider(_file[x.VariableBlock.Name]), editing), true);
            return ret;
        }
    }
}
