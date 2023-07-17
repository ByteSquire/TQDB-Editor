using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;

namespace TQDBEditor.Dialogs
{
    public partial class ConfirmationDialogWindow : Window, IConfirmationDialogWindow
    {
        public ConfirmationDialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult? Result { get; set; }

        object? IDialogWindow.Content { get => dialogContent; set { SetAndRaise(DialogContentProperty, ref dialogContent, value); ClientSize = ContentDock.DesiredSize; } }

        private object? dialogContent;
        public object? DialogContent => dialogContent;

        public static readonly DirectProperty<ConfirmationDialogWindow, object?> DialogContentProperty = AvaloniaProperty.RegisterDirect<ConfirmationDialogWindow, object?>(nameof(DialogContent), x => x.DialogContent);

        public event EventHandler? Confirmed;
        public event EventHandler? Cancelled;

        public void okBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Confirmed?.Invoke(this, EventArgs.Empty);
        }

        public void cnlBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}
