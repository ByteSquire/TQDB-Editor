using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TQArchive_Wrapper;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.Dialogs.ViewModels
{
    public partial class DBFilePickerViewModel : EditDialogViewModelBase
    {
        public override string Title => "Pick a file";

        public override bool CanConfirmDialog() => TreeSource.RowSelection?.SelectedItem != null;

        [ObservableProperty]
        private HierarchicalTreeDataGridSource<DBNode> _treeSource;
        private readonly ObservableCollection<DBNode> _nodes;

        private readonly string? _toolsDir;
        private readonly string? _modDir;
        private readonly ILogger _logger;

        public DBFilePickerViewModel(IConfiguration configuration, ILoggerProvider loggerProvider)
        {
            _nodes = new();
            TreeSource = new(_nodes)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<DBNode>(
                        new TextColumn<DBNode, string>("File", x => x.Title, width: GridLength.Star),
                        x => new ObservableCollection<DBNode>(x.SubNodes!.Cast<DBNode>().OrderBy(x => x.Title)),
                        x => x.SubNodes != null, x => x.IsExpanded
                        ),
                    new TextColumn<DBNode, string>("Source", x => x.SubNodes == null ? x.Source : null),
                }
            };
            if (TreeSource.RowSelection != null)
                TreeSource.RowSelection.SingleSelect = true;
            _toolsDir = configuration.GetToolsDir();
            _modDir = configuration.GetModDir();
            _logger = loggerProvider.CreateLogger("DBFilePicker");
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (LocalVariable == null)
                return;

            var fExtensions = LocalVariable.FileExtensions;
            if (!fExtensions.Any())
                return;
            if (fExtensions.ToArray()[0] == ".dbr")
            {
                if (_toolsDir == null)
                    return;
                var reader = new ArzReader(Path.Combine(_toolsDir, "database", "database.arz"), _logger);
                var stringArr = reader.GetStringList().ToArray();
                var fileNames = reader.GetDBRFileInfos().Select(x => stringArr[x.NameID]).ToList();
                var tmpList = new List<DBNode>();
                foreach (var file in fileNames)
                    AddTreeNode(file, "vanilla database", tmpList);
                _nodes.AddRange(tmpList);
            }
        }

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            if (LocalVariable != null)
                // RowSelection and SelectedItem cannot be null, due to CanConfirm check
                LocalVariable.Value = TreeSource.RowSelection!.SelectedItem!.Title;
            return base.OnDialogConfirmed(e);
        }

        private void AddTreeNode(string path, string source, IList<DBNode> list)
        {
            var pathSegments = path.Split('\\');
            var root = list.FirstOrDefault(x => x.Title == pathSegments[0]);
            if (root == null)
            {
                root = new(pathSegments[0], source)
                {
                    IsExpanded = true,
                };
                list.Add(root);
            }
            var node = root;
            for (int i = 1; i < pathSegments.Length; i++)
            {
                var pathSegment = pathSegments[i];
                DBNode? tNode = null;
                if ((tNode = node.SubNodes?.FirstOrDefault(x => x.Title == pathSegment) as DBNode) != null)
                {
                    node = tNode;
                    continue;
                }
                DBNode nNode = new(pathSegment, source);
                AddChildren(nNode, source, pathSegments[(i + 1)..]);
                (node.SubNodes ??= new ObservableCollection<NodeBase>()).Add(nNode);
                break;
            }
        }

        private void AddChildren(DBNode node, string source, string[] childPathSegments)
        {
            if (childPathSegments.Length > 0)
            {
                DBNode nNode = new(childPathSegments[0], source);
                (node.SubNodes ??= new ObservableCollection<NodeBase>()).Add(nNode);
                if (childPathSegments.Length > 1)
                    AddChildren(nNode, source, childPathSegments[1..]);
            }
        }

        public partial class DBNode : NodeBase
        {
            [ObservableProperty]
            private string _source;

            public DBNode(string title, string? source = null, IEnumerable<DBNode>? subNodes = null) : base(title, subNodes)
            {
                Source = source ?? string.Empty;
            }
        }
    }
}
