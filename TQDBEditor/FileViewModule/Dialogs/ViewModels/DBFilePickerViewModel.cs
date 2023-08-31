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
        protected override string SubTitle => "Pick a file";

        public override bool CanConfirmDialog() => TreeSource.RowSelection?.SelectedItem != null && TreeSource.RowSelection.SelectedItem.SubNodes == null;

        [ObservableProperty]
        private HierarchicalTreeDataGridSource<DBNode> _treeSource;
        private readonly ObservableCollection<DBNode> _nodes;

        private readonly string? _toolsDir;
        private readonly string? _databaseDir;
        private readonly string? _modName;
        private readonly string? _modArzPath;
        private readonly ILogger _logger;
        private static readonly Dictionary<string, IList<string>> _archiveFilesMap = new();
        private static readonly Dictionary<string, FileSystemWatcher> _archiveFileWatcher = new();

        public DBFilePickerViewModel(IConfiguration configuration, ILoggerProvider loggerProvider)
        {
            _nodes = new();
            TreeSource = new(_nodes)
            {
                Columns =
                {
                    new HierarchicalExpanderColumn<DBNode>(
                        new TextColumn<DBNode, string>("File", x => x.Title, width: GridLength.Auto),
                        x => new ObservableCollection<DBNode>(x.SubNodes!.Cast<DBNode>().OrderBy(x => x.Title)),
                        x => x.SubNodes != null, x => x.IsExpanded
                        ),
                    new TextColumn<DBNode, string>("Source", x => x.SubNodes == null ? x.Source : null),
                }
            };
            if (TreeSource.RowSelection != null)
                TreeSource.RowSelection.SingleSelect = true;
            _modName = configuration.GetModName();
            _toolsDir = configuration.GetToolsDir();
            _databaseDir = configuration.GetModDir() is null ? null : Path.Combine(configuration.GetModDir()!, "database");
            if (_modName != null)
            {
                var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var outputDir = Path.Combine(docs, "My Games", "Titan Quest - Immortal Throne", "CustomMaps", _modName);
                _modArzPath = Path.Combine(outputDir, "database", _modName + ".arz");
            }
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
            var tmpList = new List<DBNode>();
            if (fExtensions.ToArray()[0] == ".dbr")
            {
                if (_toolsDir != null)
                    AddNodesFromArchive(Path.Combine(_toolsDir, "database", "database.arz"), "vanilla database", tmpList);

                if (_modArzPath != null)
                    AddNodesFromArchive(_modArzPath, "mod database", tmpList);
            }
            if (_databaseDir != null)
                foreach (var file in fExtensions.SelectMany(x => Directory.EnumerateFiles(_databaseDir, '*' + x, SearchOption.AllDirectories)))
                {
                    AddTreeNode(Path.GetRelativePath(_databaseDir, file), "mod file system", tmpList);
                }

            _nodes.AddRange(tmpList);
        }

        private void AddNodesFromArchive(string archivePath, string source, IList<DBNode> existingNodes)
        {
            foreach (var file in GetFilesFromArchive(archivePath))
                AddTreeNode(file, source, existingNodes, true);
        }

        private IList<string> GetFilesFromArchive(string archivePath)
        {
            if (_archiveFilesMap.TryGetValue(archivePath, out var files))
                return files;

            if (!File.Exists(archivePath))
                return Array.Empty<string>();

            var arzReader = new ArzReader(archivePath, _logger);
            var arzStringArr = arzReader.GetStringList().ToArray();
            var arzFileNames = arzReader.GetDBRFileInfos().Select(x => arzStringArr[x.NameID]).ToList();
            AddArchiveToMap(archivePath, arzFileNames);

            return arzFileNames;
        }

        private static void AddArchiveToMap(string archivePath, IList<string> arzFileNames)
        {
            _archiveFilesMap[archivePath] = arzFileNames;
            var fsWatcher = new FileSystemWatcher(Path.GetDirectoryName(archivePath)!, Path.GetFileName(archivePath))
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            };
            _archiveFileWatcher[archivePath] = fsWatcher;
            fsWatcher.Changed += (_, _) => RemoveFromMap();
            fsWatcher.Deleted += (_, _) => RemoveFromMap();
            fsWatcher.Renamed += (_, _) => RemoveFromMap();

            void RemoveFromMap()
            {
                _archiveFilesMap.Remove(archivePath);
                _archiveFileWatcher.Remove(archivePath);
            }
        }

        public override IDialogParameters? OnDialogConfirmed(EventArgs e)
        {
            if (LocalVariable != null)
                // RowSelection and SelectedItem cannot be null, due to CanConfirm check
                LocalVariable.Value = TreeSource.RowSelection!.SelectedItem!.Title;
            return base.OnDialogConfirmed(e);
        }

        private void AddTreeNode(string path, string source, IList<DBNode> list, bool overwrite = false)
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
            if (node != null)
            {
                node.Source = !overwrite ? node.Source + " - " + source : source;
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
