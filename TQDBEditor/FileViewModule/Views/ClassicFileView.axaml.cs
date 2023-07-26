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
                valueScroll.PropertyChanged += (s, e) => { if (e.Property == ScrollViewer.OffsetProperty) SyncScroll(valueScroll.Offset, variableScroll); };
                variableScroll.PropertyChanged += (s, e) => { if (e.Property == ScrollViewer.OffsetProperty) SyncScroll(variableScroll.Offset, valueScroll); };
            }
        }

        static void SyncScroll(Vector offset, ScrollViewer target)
        {
            target.Offset = offset;
        }

        public void OnNodeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ClassicFileViewViewModel viewModel)
            {
                viewModel.OnNodeSelected(e.AddedItems[0] as ClassicFileViewViewModel.Node);
            }
        }
    }
}
