using Avalonia.Controls;
using TQDBEditor.FileViewModule.ViewModels;

namespace TQDBEditor.FileViewModule.Views
{
    public partial class ClassicFileView : UserControl
    {
        public ClassicFileView()
        {
            InitializeComponent();
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
