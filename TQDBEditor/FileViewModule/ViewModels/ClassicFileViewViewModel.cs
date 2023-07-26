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
                    new TextColumn<MyVariableRow, string>("Description", x => x.VariableBlock.Description),
                    new TextColumn<MyVariableRow, string>("DefaultValue", x => x.VariableBlock.DefaultValue),
                },
            };
        }

        public void OnNodeSelected(Node? node)
        {
            if (node != null && _files != null)
            {
                _blocks.Clear();
                var fList = _files.ToList();
                var vars = node.Block.GetVariables(node.Title.Equals("All Groups"));
                foreach (var variable in vars)
                {
                    _blocks.Add(new(variable, fList));
                }
                for (int i = 0; i < fList.Count; i++)
                {
                    var index = i;
                    ValSource.Columns.Add(new TextColumn<MyVariableRow, string>(fList[i].FileName, x => x.Entries[index], (x, value) => x.Entries[index].Value = value));
                }
            }
        }

        public override string ToString()
        {
            return "Classic (ArtManager)";
        }

        private IEnumerable<DBRFile>? _files;
        public override void InitFiles(GroupBlock template, IEnumerable<DBRFile> files)
        {
            if (_blocks.Count == 0)
            {
                _files = files;
                TreeNodes.Add(NodeFromGroupBlock(template));
            }
        }

        private Node NodeFromGroupBlock(GroupBlock block)
        {
            return new Node(block, block.Name, new(block.GetGroups().Select(NodeFromGroupBlock)));
        }

        public partial class Node : ObservableObject
        {
            [ObservableProperty]
            private GroupBlock _block;

            [ObservableProperty]
            private string _title;

            [ObservableProperty]
            private ObservableCollection<Node> _subNodes;

            public Node(GroupBlock block, string title, ObservableCollection<Node> subNodes)
            {
                Block = block;
                Title = title;
                SubNodes = subNodes;
            }
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
