using Avalonia.Controls;
using Prism.Services.Dialogs;

namespace TQDBEditor.Dialogs
{
    public partial class BaseDialogWindow : Window, IDialogWindow
    {
        public BaseDialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult? Result { get; set; }

        object? IDialogWindow.Content { get => null; set { if (value != null) Dock.Children[1] = (Control)value; } }
    }
}
