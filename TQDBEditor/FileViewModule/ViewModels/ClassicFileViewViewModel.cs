using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.ClassicViewModule;
using TQDBEditor.Services;
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
        private const bool CAN_SORT = false;
        private readonly string _modDir;
        private readonly IDialogService _dialogService;

        public ClassicFileViewViewModel(ILoggerProvider loggerProvider, IConfiguration config, IDialogService dialogService)
        {
            var working = config.GetModDir();
            var mod = config.GetModDir();
            if (working is null || mod is null)
                throw new ArgumentNullException(nameof(config), "Working and Mod directories have to be set");
            _modDir = Path.Combine(working, "CustomMaps", mod);
            _dialogService = dialogService;
            _blocks = new();
            VarSource = CreateBasicDataGridSource();
            ValSource = new(_blocks)
            {
                Selection = VarSource.Selection,
            };
        }

        private FlatTreeDataGridSource<MyVariableRow> CreateBasicDataGridSource()
        {
            var colOptions = new TextColumnOptions<MyVariableRow>() { CanUserSortColumn = CAN_SORT };
            return new(_blocks)
            {
                Columns =
                {
                    new TextColumn<MyVariableRow, string>("Name", x => x.VariableBlock.Name, options: colOptions),
                    new TextColumn<MyVariableRow, string>("Class", x => Enum.GetName(x.VariableBlock.Class), options: colOptions),
                    new TextColumn<MyVariableRow, string>("Type", x => x.VariableBlock.Type == TQDB_Parser.VariableType.file ? "file (" + string.Join(',', x.VariableBlock.FileExtensions) + ")" : Enum.GetName(x.VariableBlock.Type), options: colOptions),
                    new TextColumn<MyVariableRow, string>("Description", x => x.VariableBlock.Description, options: colOptions),
                    new TextColumn<MyVariableRow, string>("DefaultValue", x => x.VariableBlock.DefaultValue, options: colOptions),
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
                var colOptions = new ColumnOptions<MyVariableRow>() { CanUserSortColumn = CAN_SORT };
                for (int i = 0; i < _fList.Count; i++)
                {
                    var index = i;
                    ValSource.Columns.Add(new ValueColumn(_fList[index].FileName, index, _modDir, _dialogService, options: colOptions));
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

    public class MyVariableRow
    {
        public VariableBlock VariableBlock { get; }

        public IList<DBREntry> Entries { get; }

        public MyVariableRow(VariableBlock variableBlock, IEnumerable<DBRFile> files)
        {
            VariableBlock = variableBlock;
            Entries = files.Select(x => x[VariableBlock.Name]).ToList();
        }
    }
}
