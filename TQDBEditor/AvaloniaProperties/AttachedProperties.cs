using Avalonia.Controls;
using Avalonia;
using System.Windows.Input;
using System.Reactive;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Generic;

namespace TQDBEditor.AvaloniaProperties
{
    public class AttachedProperties : AvaloniaObject
    {
        static AttachedProperties()
        {
            CommandProperty.Changed.Subscribe(Observer.Create<AvaloniaPropertyChangedEventArgs<ICommand>>(x =>
                        HandleCommandChanged(x.Sender, x.NewValue.GetValueOrDefault<ICommand>())));
        }

        public static readonly AttachedProperty<ICommand> CommandProperty =
            AvaloniaProperty.RegisterAttached<AttachedProperties, MenuItem, ICommand>("Command");

        public static readonly AttachedProperty<ICommand> CommandTestProperty =
            AvaloniaProperty.RegisterAttached<AttachedProperties, MenuItem, ICommand>("CommandTest");

        public static readonly AttachedProperty<bool> PrependProperty =
            AvaloniaProperty.RegisterAttached<AttachedProperties, ItemsControl, bool>("Prepend", false, false, Avalonia.Data.BindingMode.OneWay, null);

        /// <summary>
        /// Accessor for Attached property <see cref="CommandParameterProperty"/>.
        /// </summary>
        public static void SetPrepend(AvaloniaObject element, object parameter)
        {
            element.SetValue(PrependProperty, parameter);
        }

        /// <summary>
        /// Accessor for Attached property <see cref="CommandParameterProperty"/>.
        /// </summary>
        public static object GetPrepend(AvaloniaObject element)
        {
            return element.GetValue(PrependProperty);
        }
        /// <summary>
        /// <see cref="CommandProperty"/> changed event handler.
        /// </summary>
        private static void HandleCommandChanged(AvaloniaObject element, ICommand? commandValue)
        {
            if (element is MenuItem menuItem)
            {
                if (commandValue != null)
                    menuItem.Click += Handler;
                else
                    menuItem.Click -= Handler;
            }

            static void Handler(object? s, RoutedEventArgs e)
            {
                if (s is MenuItem menuItem)
                {
                    // This is how we get the parameter off of the gui element.
                    var commandParameter = menuItem.GetValue(MenuItem.CommandParameterProperty);
                    var command = menuItem.GetValue(CommandProperty);
                    if (command?.CanExecute(commandParameter) == true)
                    {
                        command.Execute(commandParameter);
                    }
                }
            }
        }
    }

    public static class DirectPropertiesExtensions
    {
        public static bool GetShouldPrepend(this ItemsControl element)
        {
            return element.GetValue(AttachedProperties.PrependProperty);
        }
    }
}
