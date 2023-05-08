using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Prism.Services.Dialogs;

namespace TQDBEditor.Dialogs
{
    public partial class InformationDialogWindow : Window, IDialogWindow
    {
        public InformationDialogWindow()
        {
            InitializeComponent();
            Result = new DialogResult(ButtonResult.OK);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            OkButton.Focus();
        }

        public IDialogResult Result { get; set; }

        object? IDialogWindow.Content { get => dialogContent; set { SetAndRaise(DialogContentProperty, ref dialogContent, value); ClientSize = ContentDock.DesiredSize; } }

        private object? dialogContent;
        public object? DialogContent => dialogContent;

        public static readonly DirectProperty<InformationDialogWindow, object?> DialogContentProperty = AvaloniaProperty.RegisterDirect<InformationDialogWindow, object?>(nameof(DialogContent), x => x.DialogContent);

        public void okBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
