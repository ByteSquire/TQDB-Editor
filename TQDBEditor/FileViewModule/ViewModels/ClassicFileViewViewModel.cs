using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.ViewModels
{
    public partial class ClassicFileViewViewModel : FileViewModelBase
    {
        [ObservableProperty]
        private FlatTreeDataGridSource<MyVariableRow> _varSource;

        [ObservableProperty]
        private FlatTreeDataGridSource<MyVariableRow> _valSource;

        [ObservableProperty]
        private ObservableCollection<Node> _treeNodes = new();

        private ObservableCollection<MyVariableRow> _blocks;
        private IList<DBRFile>? _fList;

        public ClassicFileViewViewModel(ILoggerProvider loggerProvider)
        {
            _blocks = new();
            VarSource = CreateBasicDataGridSource();
            ValSource = new(_blocks)
            {
                Selection = VarSource.Selection
            };
        }

        private FlatTreeDataGridSource<MyVariableRow> CreateBasicDataGridSource()
        {
            return new(_blocks)
            {
                Columns =
                {
                    new TextColumn<MyVariableRow, string>("Name", x => x.VariableBlock.Name),
                    new TextColumn<MyVariableRow, string>("Class", x => Enum.GetName(x.VariableBlock.Class)),
                    new TextColumn<MyVariableRow, string>("Type", x => Enum.GetName(x.VariableBlock.Type)),
                    new TextColumn<MyVariableRow, string>("Description", x => x.VariableBlock.Description, GridLength.Star),
                    new TextColumn<MyVariableRow, string>("DefaultValue", x => x.VariableBlock.DefaultValue, GridLength.Star),
                },
            };
        }

        public void OnNodeSelected(Node? node)
        {
            if (node != null && _fList != null)
            {
                _blocks.Clear();
                var vars = node.Block.GetVariables(true);
                foreach (var variable in vars)
                {
                    _blocks.Add(new(variable, _fList));
                }
            }
        }

        public override void InitFiles(GroupBlock template, IEnumerable<DBRFile> files)
        {
            if (_blocks.Count == 0)
            {
                _fList = files.ToList();
                for (int i = 0; i < _fList.Count; i++)
                {
                    var index = i;
                    ValSource.Columns.Add(new TextColumn<MyVariableRow, string>(_fList[i].FileName, x => x.Entries[index], (x, value) => x.Entries[index].Value = value));
                }
                var root = NodeFromGroupBlock(template);
                root.IsExpanded = true;
                TreeNodes.Add(root);
                root.IsSelected = true;
            }
        }

        private Node NodeFromGroupBlock(GroupBlock block)
        {
            return new Node(block, block.Name, new(block.GetGroups().Select(NodeFromGroupBlock)));
        }

        public override string ToString()
        {
            return "Classic (ArtManager)";
        }
    }

    public partial class Node : NodeBase
    {
        [ObservableProperty]
        private GroupBlock _block;

        public Node(GroupBlock block, string title, ObservableCollection<Node> subNodes) : base(title, subNodes is null ? null : new(subNodes.Cast<NodeBase>()))
        {
            Block = block;
        }
    }

    public partial class MyVariableRow : ObservableObject
    {
        public VariableBlock VariableBlock { get; }

        [ObservableProperty]
        private ObservableCollection<ObservableValue<string>> _entries;

        public MyVariableRow(VariableBlock variableBlock, IEnumerable<DBRFile>? files = null)
        {
            VariableBlock = variableBlock;
            Entries = files is null ? new() : new(files.Select(x => new ObservableValue<string>(x[VariableBlock.Name].Value, y => x.UpdateEntry(VariableBlock.Name, y))));
        }
    }

    public partial class ObservableValue<T> : ObservableObject
    {
        [ObservableProperty]
        private T? _value = default;

        public ObservableValue(T value, Action<T> setter)
        {
            Value = value;

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Value) && Value != null)
                    setter(Value);
            };
        }

        public static implicit operator T?(ObservableValue<T> value) { return value.Value; }
    }
}
