using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using TQDBEditor.ClassicViewModule.ViewModels;

namespace TQDBEditor.ClassicViewModule.Views
{
    public partial class ClassicView : UserControl
    {
        public ClassicView()
        {
            InitializeComponent();
            // Always expand first item in Tree
            Tree.PropertyChanged += (s, args) =>
            {
                if (args.Property == ItemsControl.ItemsSourceProperty)
                {
                    var treeViewItem = (TreeViewItem?)Tree.ContainerFromIndex(0);
                    if (treeViewItem != null)
                        treeViewItem.IsExpanded = true;
                }
            };
        }

        public void OnNodeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ClassicViewViewModel viewModel)
            {
                viewModel.OnNodeSelected(e.AddedItems[0] as Node);
            }
        }

        public void OnTabSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ClassicViewViewModel viewModel)
            {
                viewModel.OnViewSelected((e.AddedItems[0] as TabItem)?.Header?.ToString());
            }
        }
    }
}
