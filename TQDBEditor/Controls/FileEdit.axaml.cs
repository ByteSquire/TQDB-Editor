using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TQDB_Parser.DBR;
using TQDBEditor.Controls.ViewModels;

namespace TQDBEditor.Controls
{
    public partial class FileEdit : UserControl
    {
        public FileEdit()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            TxtBox.AttachedToVisualTree += TxtAttachedToVisual;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            TxtBox.AttachedToVisualTree -= TxtAttachedToVisual;
        }

        private void TxtAttachedToVisual(object? sender, VisualTreeAttachmentEventArgs e)
        {
            TxtBox.Focus();
        }

        public void OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is FileEditViewModel viewModel)
            {
                viewModel.OnClick();
            }
        }
    }
}
