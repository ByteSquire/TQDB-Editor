using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor.ViewModels
{
    public partial class NodeBase : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NodeBase>? _subNodes;
        [ObservableProperty]
        private string _title;
        [ObservableProperty]
        private bool _isExpanded = false;
        [ObservableProperty]
        private bool _isSelected = false;

        public NodeBase(string title, ObservableCollection<NodeBase>? subNodes = null)
        {
            Title = title;
            SubNodes = subNodes;
            if (SubNodes?.Count == 1)
                SubNodes[0].IsExpanded = true;
        }

        public void AddSubNode(NodeBase node)
        {
            SubNodes ??= new();
            SubNodes.Add(node);
            if (SubNodes?.Count == 1)
                SubNodes[0].IsExpanded = true;
        }
    }
}
