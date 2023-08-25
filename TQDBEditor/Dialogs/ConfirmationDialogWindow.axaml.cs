using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
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
            UpdateMaxSize();
            PositionChanged += (_, _) => UpdateMaxSize();
        }

        private Screen? _lastScreen = null;
        private void UpdateMaxSize()
        {
            var screen = Screens.ScreenFromVisual(this);
            if (screen != null && screen != _lastScreen)
            {
                var screenBounds = screen.WorkingArea;
                MaxWidth = screenBounds.Width / screen.Scaling;
                MaxHeight = screenBounds.Height / screen.Scaling;
            }
            _lastScreen = screen;
        }

        public IDialogResult? Result { get; set; }

        object? IDialogWindow.Content { get => DialogContent.Content; set => DialogContent.Content = value; }


        public event EventHandler? Confirmed;
        public event EventHandler? Cancelled;

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Enter)
            {
                Confirmed?.Invoke(this, EventArgs.Empty);
            }
        }

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
