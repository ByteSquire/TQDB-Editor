using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;

using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using TQDBEditor.Controls;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

using TQDB_Parser.DBRMeta;

namespace TQDBEditor.ClassicViewModule.ViewModels
{
    public partial class ClassicViewViewModel : ViewModelBase
    {
        public static string[] KnownDirs => new string[] { "Source", "Assets", "Database" };

        [ObservableProperty]
        private ObservableCollection<Node> _treeNodes = new();
        private readonly Dictionary<string, Node> _cachedNodes = new();

        private readonly ObservableCollection<MyFileInfos> _files = new();
        [ObservableProperty]
        private FlatTreeDataGridSource<MyFileInfos> _dataGridSource;

        private string? _workingDir;
        private string? WorkingModsFolder => _workingDir is null ? null : Path.Combine(_workingDir, "CustomMaps");
        private string? FullModDir => WorkingModsFolder is null || _modDir is null ? null : Path.Combine(WorkingModsFolder, _modDir);
        private string? _selectedView;
        private string? _modDir;
        private readonly ILogger _logger;
        private readonly FileSystemWatcher _watcher;

        public ClassicViewViewModel(IObservableConfiguration configuration, ILoggerProvider loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(TQDBEditor.ClassicViewModule));
            _workingDir = configuration.GetWorkingDir();
            _watcher = new()
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };
            _watcher.Deleted += DirectoryDeleted;
            _watcher.Created += DirectoryCreated;
            _watcher.Renamed += DirectoryRenamed;

            OnModDirChanged(configuration.GetModDir());
            configuration.AddWorkingDirChangeListener(x => _workingDir = x);
            configuration.AddModDirChangeListener(OnModDirChanged);

            DataGridSource = CreateBasicDataGridSource();
        }

        private void DirectoryCreated(object sender, FileSystemEventArgs e)
        {
            if (!TryGetRelativeToModSplit(e.FullPath, out var pathSegments))
                return;

            var root = pathSegments[0];
            if (!KnownDirs.Any(x => x.Equals(root, StringComparison.OrdinalIgnoreCase)))
                return;
            var currNode = _cachedNodes[root.ToLower()];
            for (int i = 1; i < pathSegments.Length; i++)
            {
                var currDir = pathSegments[i];
                var child = currNode.SubNodes?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
                if (child != null)
                    currNode = child;
                else
                {
                    var newPath = Path.Combine(pathSegments[..(i + 1)].Prepend(FullModDir!).ToArray());
                    currNode.AddSubNode(new(newPath, currDir, InitDirectory(newPath)));
                }
            }
        }

        private void DirectoryRenamed(object sender, RenamedEventArgs e)
        {
            if (!TryGetRelativeToModSplit(e.OldFullPath, out var pathSegments))
                return;

            var root = pathSegments[0];
            if (!KnownDirs.Any(x => x.Equals(root, StringComparison.OrdinalIgnoreCase)))
                return;
            var currNode = _cachedNodes[root.ToLower()];
            for (int i = 1; i < pathSegments.Length; i++)
            {
                var currDir = pathSegments[i];
                var child = currNode.SubNodes?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
                if (child != null)
                    currNode = child;
            }
            var newDirName = Path.GetFileName(e.Name);
            if (currNode.Title == pathSegments[^1] && newDirName != null)
            {
                var newPath = e.FullPath;
                currNode.Title = newDirName;
                currNode.Path = newPath;
            }
        }

        private void DirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            if (!TryGetRelativeToModSplit(e.FullPath, out var pathSegments))
                return;

            var root = pathSegments[0];
            if (!KnownDirs.Any(x => x.Equals(root, StringComparison.OrdinalIgnoreCase)))
                return;
            var currNode = _cachedNodes[root.ToLower()];
            for (int i = 1; i < pathSegments.Length; i++)
            {
                var currDir = pathSegments[i];
                var child = currNode.SubNodes?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
                if (child != null)
                {
                    if (Directory.Exists(child.Path))
                        currNode = child;
                    else
                    {
                        currNode.SubNodes?.Remove(child);
                        return;
                    }
                }
            }
        }

        private bool TryGetRelativeToModSplit(string path, [NotNullWhen(true)] out string[]? relativeSplit)
        {
            relativeSplit = null;
            if (FullModDir == null)
                return false;
            var relative = Path.GetRelativePath(FullModDir, path);
            var root = relative.Split(Path.DirectorySeparatorChar)[0];
            if (KnownDirs.Any(x => x.Equals(root, StringComparison.OrdinalIgnoreCase)))
            {
                relativeSplit = relative.Split(Path.DirectorySeparatorChar);
                return true;
            }
            else
                return false;
        }

        private FlatTreeDataGridSource<MyFileInfos> CreateBasicDataGridSource()
        {
            return new FlatTreeDataGridSource<MyFileInfos>(_files)
            {
                Columns =
                {
                    new TextColumn<MyFileInfos, string>("Name", x => Path.GetFileName(x.FullPath)),
                }
            };
        }

        private void OnModDirChanged(string? path)
        {
            Reset();
            if (path != null && WorkingModsFolder != null)
            {
                _modDir = path;
                var modPath = FullModDir!;
                _watcher.Path = modPath;
                _watcher.EnableRaisingEvents = true;
                foreach (var known in KnownDirs)
                {
                    var rootPath = Path.Combine(modPath, known.ToLower());
                    Node root = new(rootPath, Path.Combine(_modDir!, known.ToLower()), InitDirectory(rootPath));
                    _cachedNodes[known.ToLower()] = root;
                }
                UpdateTreeNodes();
            }
        }

        private void UpdateTreeNodes(string? changedDir = null)
        {
            if (_selectedView != null && (changedDir == null || !_selectedView.Equals(changedDir, StringComparison.OrdinalIgnoreCase)))
                TreeNodes = new() { _cachedNodes[_selectedView.ToLower()] };
        }

        private ObservableCollection<Node> InitDirectory(string sourcePath)
        {
            var infos = Directory.CreateDirectory(sourcePath).EnumerateDirectories().ToList();
            var ret = new ObservableCollection<Node>();
            foreach (var info in infos)
            {
                ret.Add(new(info.FullName, info.Name, InitDirectory(info.FullName)));
            }
            return ret;
        }

        public void OnNodeSelected(Node? selectedNode)
        {
            _files.Clear();
            if (selectedNode != null)
                foreach (var info in Directory.CreateDirectory(selectedNode.Path).EnumerateFiles())
                    _files.Add(new(info.FullName, _logger));
        }

        public void OnViewSelected(string? view)
        {
            if (_selectedView == view)
                return;
            _selectedView = view;
            var dataGridSource = CreateBasicDataGridSource();
            if (_selectedView != null)
            {
                if (KnownDirs.Any(x => x.Equals(_selectedView, StringComparison.Ordinal)))
                    UpdateTreeNodes();
                else
                    _logger.LogWarning("Somehow selected a directory \"{dir}\" that is not one of the known ones ({known})", _selectedView, KnownDirs);
                switch (view)
                {
                    case "Assets":
                        dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Status", x => string.Empty));
                        break;
                    case "Database":
                        dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Description", x => x.Metadata.FileDescription));
                        dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Template", x => x.Metadata.FileDescription));
                        break;
                }
            }
            DataGridSource = dataGridSource;
        }

        private void Reset()
        {
            TreeNodes.Clear();
            _files.Clear();
        }

        public override string ToString()
        {
            return "Classic (ArtManager)";
        }
    }

    public class MyFileInfos
    {
        public string FullPath { get; set; }
        public string FileExtension { get; }
        private DBRMetadata? _metadata;
        private readonly ILogger? _logger;
        public DBRMetadata Metadata => _metadata ??= TryGetMetadataOrDefault();

        private DBRMetadata TryGetMetadataOrDefault()
        {
            if (FileExtension.Equals(".dbr"))
                try
                {
                    return DBRMetaParser.ParseFile(FullPath, _logger);
                }
                catch { }
            return new();
        }

        public MyFileInfos(string fullPath, ILogger? logger = null)
        {
            FullPath = fullPath;
            FileExtension = Path.GetExtension(FullPath);
            _logger = logger;
        }

    }

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
