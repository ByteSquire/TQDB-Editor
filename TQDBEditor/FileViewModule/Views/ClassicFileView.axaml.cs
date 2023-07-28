using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TQDBEditor.FileViewModule.ViewModels;

namespace TQDBEditor.FileViewModule.Views
{
    public partial class ClassicFileView : UserControl
    {
        public ClassicFileView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (ValueData.Scroll is ScrollViewer valueScroll && VariableData.Scroll is ScrollViewer variableScroll)
            {
                variableScroll.VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden;
                valueScroll.PropertyChanged += (s, e) => { if (e.Property == ScrollViewer.OffsetProperty) SyncScrollY(valueScroll.Offset, variableScroll); };
                variableScroll.PropertyChanged += (s, e) => { if (e.Property == ScrollViewer.OffsetProperty) SyncScrollY(variableScroll.Offset, valueScroll); };
            }
        }

        static void SyncScrollY(Vector offset, ScrollViewer target)
        {
            target.Offset = target.Offset.WithY(offset.Y);
        }

        public void OnNodeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ClassicFileViewViewModel viewModel && e.AddedItems.Count > 0)
            {
                viewModel.OnNodeSelected(e.AddedItems[0] as Node);
            }
        }
    }
}
