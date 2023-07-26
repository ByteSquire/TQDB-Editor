using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TQDBEditor.ViewModels
{
    public partial class Node : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Node>? _subNodes;
        [ObservableProperty]
        private string _title;

        public string Path { get; set; }

        public Node(string path, string title)
        {
            Path = path;
            Title = title;
        }

        public Node(string path, string title, ObservableCollection<Node> subNodes) : this(path, title)
        {
            SubNodes = subNodes;
        }

        public void AddSubNode(Node node)
        {
            SubNodes ??= new();
            SubNodes.Add(node);
        }
    }
}
