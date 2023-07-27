using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;

using CommunityToolkit.Mvvm.ComponentModel;

using DynamicData;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using TQDBEditor.Controls;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

using TQDB_Parser.DBRMeta;
using Prism.Events;
using TQDBEditor.Events;
using TQDB_Parser;

namespace TQDBEditor.ClassicViewModule.ViewModels
{
    public partial class ClassicViewViewModel : ViewModelBase
    {
        public static string[] KnownDirs => new string[] { "Source", "Assets", "Database" };

        public TreeDataGrid? FileTable { get; set; }

        [ObservableProperty]
        private ObservableCollection<Node> _treeNodes = new();
        private readonly Dictionary<string, Node> _cachedNodes = new();

        private readonly ObservableCollection<MyFileInfos> _files = new();
        [ObservableProperty]
        private FlatTreeDataGridSource<MyFileInfos> _dataGridSource;
        [ObservableProperty]
        private bool _allFilesSelected;
        private bool _innerSelect = false;

        [ObservableProperty]
        private string? _workingDir;
        private string? WorkingModsFolder => WorkingDir is null ? null : Path.Combine(WorkingDir, "CustomMaps");
        private string? FullModDir => WorkingModsFolder is null || _modDir is null ? null : Path.Combine(WorkingModsFolder, _modDir);
        private string? _selectedView;
        private string? _modDir;
        private readonly ILogger _logger;
        private readonly FileSystemWatcher _modWatcher;
        private readonly FileSystemWatcher _dirWatcher;

        private TemplateManager? _templateManager;
        private readonly DBRAccessEvent _accessEvent;

        public ClassicViewViewModel(IObservableConfiguration configuration, IEventAggregator ea, ILoggerProvider loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(TQDBEditor.ClassicViewModule));
            WorkingDir = configuration.GetWorkingDir();
            _modWatcher = new()
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName
            };
            _modWatcher.Deleted += DirectoryDeleted;
            _modWatcher.Created += DirectoryCreated;
            _modWatcher.Renamed += DirectoryRenamed;

            _dirWatcher = new()
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _dirWatcher.Deleted += FileDeleted;
            _dirWatcher.Created += FileCreated;
            _dirWatcher.Renamed += FileRenamed;
            _dirWatcher.Changed += FileChanged;

            OnModDirChanged(configuration.GetModDir());
            configuration.AddWorkingDirChangeListener(x => WorkingDir = x);
            configuration.AddModDirChangeListener(OnModDirChanged);

            _accessEvent = ea.GetEvent<DBRAccessEvent>();

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
                var child = currNode.SubNodes?.Cast<Node>()?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
                if (child != null)
                    currNode = child;
                else
                {
                    var newPath = Path.Combine(pathSegments[..(i + 1)].Prepend(FullModDir!).ToArray());
                    currNode.AddSubNode(new Node(newPath, currDir, InitDirectory(newPath)));
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
                var child = currNode.SubNodes?.Cast<Node>()?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
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
                var child = currNode.SubNodes?.Cast<Node>()?.SingleOrDefault(x => x!.Title.Equals(currDir, StringComparison.OrdinalIgnoreCase), null);
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

        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            _files.Add(CreateNewFileInfo(e.FullPath));
        }

        private void FileRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                App.MainThreadContext?.Post(x => _files.Single(x => x.FullPath == e.OldFullPath).FullPath = e.FullPath, this);
            }
            catch
            {
                ;
            }
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                App.MainThreadContext?.Post(x => _files.Single(x => x.FullPath == e.FullPath).Changed(), this);
            }
            catch
            {
                ;
            }
        }

        private void FileDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                RemoveFileInfo(_files.Single(x => x.FullPath == e.FullPath));
            }
            catch
            {
                ;
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
            var basicSource = new FlatTreeDataGridSource<MyFileInfos>(_files)
            {
                Columns =
                {
                    new TemplateColumn<MyFileInfos>(
                        new CheckBox(){ [!CheckBox.IsCheckedProperty] = new Binding(nameof(AllFilesSelected)), },
                        new FuncDataTemplate<MyFileInfos>(x => true, x => new Border(){ Padding = new(4,2,4,2), Child = new CheckBox(){ [!CheckBox.IsCheckedProperty] = new Binding(nameof(MyFileInfos.IsSelected)) } }),
                        options: new() { CanUserResizeColumn = false, CanUserSortColumn = false, BeginEditGestures = BeginEditGestures.None }),
                    new TextColumn<MyFileInfos, string>("Name", x => Path.GetFileName(x.FullPath), (x, value) =>
                    {
                        try
                        {
                            var nPath = Path.Combine(Path.GetDirectoryName(x.FullPath)!, value!);
                            File.Move(x.FullPath, nPath);
                        }
                        catch (Exception ex)
                        {
                            switch (ex)
                            {
                                case ArgumentException ae:
                                case PathTooLongException pe:
                                case NotSupportedException se:
                                    break;
                                default:
                                    throw;
                            };
                        }
                    }, options: new() { BeginEditGestures = BeginEditGestures.F2 }),
                },
            };
            var selection = basicSource.RowSelection!;
            selection.SingleSelect = false;

            if (DataGridSource?.RowSelection != null)
                DataGridSource.RowSelection.SelectionChanged -= Selection_SelectionChanged;
            selection.SelectionChanged += Selection_SelectionChanged;

            return basicSource;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (!_innerSelect && e.PropertyName == nameof(AllFilesSelected) && DataGridSource?.RowSelection != null)
                if (AllFilesSelected)
                    Selection_SelectionChanged(DataGridSource.RowSelection, new(selectedIndexes: Enumerable.Range(0, _files.Count).Select(x => new IndexPath(x)).ToList(), selectedItems: _files));
                else
                    Selection_SelectionChanged(DataGridSource.RowSelection, new(deselectedIndexes: Enumerable.Range(0, _files.Count).Select(x => new IndexPath(x)).ToList(), deselectedItems: _files));

            if (e.PropertyName == nameof(WorkingDir))
                if (WorkingDir != null)
                {
                    var t = new TemplateManager(WorkingDir, logger: _logger);
                    t.ResolveAllIncludes();
                    _templateManager = t;
                }
        }

        private void Selection_SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<MyFileInfos> e)
        {
            if (sender is ITreeSelectionModel model)
            {
                _innerSelect = true;
                AllFilesSelected = model.SelectedItems.OfType<MyFileInfos>().Count() == model.Source!.Cast<MyFileInfos>().Count();
                _innerSelect = false;
            }

            if (e.SelectedItems.Any())
            {
                var rowIndex = DataGridSource.Rows.ModelIndexToRowIndex(e.SelectedIndexes[^1]);
                FileTable?.TryGetCell(1, rowIndex)?.Focus();
                foreach (var selected in e.SelectedItems.OfType<MyFileInfos>())
                    selected.IsSelected = true;
            }
            foreach (var deselected in e.DeselectedItems.OfType<MyFileInfos>())
                deselected.IsSelected = false;
        }

        public void BeginOpening()
        {
            var selected = DataGridSource.RowSelection?.SelectedItems;
            if (selected != null && selected.Count > 0 && WorkingDir != null)
            {
                var parser = new DBRParser(_templateManager!, _logger);
                _accessEvent.Publish(new(selected.OfType<MyFileInfos>().Select(x => parser.ParseFile(x.FullPath))));
            }
        }

        private void OnModDirChanged(string? path)
        {
            Reset();
            if (path != null && WorkingModsFolder != null)
            {
                _modDir = path;
                var modPath = FullModDir!;
                _modWatcher.Path = modPath;
                _modWatcher.EnableRaisingEvents = true;
                foreach (var known in KnownDirs)
                {
                    var rootPath = Path.Combine(modPath, known.ToLower());
                    Node root = new(rootPath, Path.Combine(_modDir!, known.ToLower()), InitDirectory(rootPath)) { IsExpanded = true };
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
            ClearFiles();
            if (selectedNode != null)
            {
                _dirWatcher.Path = selectedNode.Path;
                _dirWatcher.EnableRaisingEvents = true;
                foreach (var info in Directory.CreateDirectory(selectedNode.Path).EnumerateFiles())
                    _files.Add(CreateNewFileInfo(info.FullName));
            }
        }

        private MyFileInfos CreateNewFileInfo(string fullPath)
        {
            var fileInfo = new MyFileInfos(fullPath, _logger);
            fileInfo.PropertyChanged += FileInfoPropertyChanged;
            return fileInfo;
        }

        private void FileInfoPropertyChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MyFileInfos.IsSelected) && s is MyFileInfos infos)
            {
                var index = DataGridSource.Items.IndexOf(infos);
                if (infos.IsSelected)
                    DataGridSource.RowSelection?.Select(index);
                else
                    DataGridSource.RowSelection?.Deselect(index);
            }
        }

        private void RemoveFileInfo(MyFileInfos info)
        {
            info.PropertyChanged -= FileInfoPropertyChanged;
            _files.Remove(info);
        }

        private void ClearFiles()
        {
            foreach (var file in _files)
            {
                file.PropertyChanged -= FileInfoPropertyChanged;
            }
            _files.Clear();
            _innerSelect = true;
            AllFilesSelected = false;
            _innerSelect = false;
        }

        public void OnViewSelected(string? view)
        {
            if (_selectedView == view)
                return;
            Reset();
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
                        dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Template", x => x.Metadata.TemplateName));
                        break;
                }
            }
            DataGridSource = dataGridSource;
        }

        private void Reset()
        {
            TreeNodes.Clear();
            ClearFiles();
            _modWatcher.EnableRaisingEvents = false;
            _dirWatcher.EnableRaisingEvents = false;
        }

        public override string ToString()
        {
            return "Classic (ArtManager)";
        }
    }

    public partial class Node : NodeBase
    {
        public string Path { get; set; }

        public Node(string path, string title, ObservableCollection<Node>? subNodes = null) : base(title, subNodes is null ? null : new(subNodes.Cast<NodeBase>()))
        {
            Path = path;
        }
    }

    public partial class MyFileInfos : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Metadata))]
        private string _fullPath;
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
            return new(null, null);
        }

        public MyFileInfos(string fullPath, ILogger? logger = null)
        {
            FullPath = fullPath;
            FileExtension = Path.GetExtension(FullPath);
            _logger = logger;
            PropertyChanged += (s, args) => { if (args.PropertyName == nameof(FullPath)) _metadata = TryGetMetadataOrDefault(); };
        }

        public void Changed()
        {
            var nMetadata = TryGetMetadataOrDefault();
            if (!nMetadata.Equals(_metadata))
            {
                _metadata = nMetadata;
                OnPropertyChanged(nameof(Metadata));
            }
        }
    }
}
