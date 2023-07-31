using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TQDBEditor.FileViewModule.ViewModels;

namespace TQDBEditor.FileViewModule.Controls
{
    public partial class AdvancedEdit : UserControl
    {
        public AdvancedEdit()
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
            if (DataContext is AdvancedEditViewModelBase viewModel)
            {
                viewModel.OnClick();
            }
        }
    }
}
