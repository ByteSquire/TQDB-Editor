using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TQDBEditor.Controls;
using TQDBEditor.Services;
using TQDBEditor.ViewModels;

using TQDB_Parser.DBRMeta;
using Microsoft.Extensions.Logging;

namespace TQDBEditor.ClassicViewModule.ViewModels
{
    public partial class ClassicViewViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<Node> _treeNodes = new();

        private readonly ObservableCollection<MyFileInfos> _files = new();
        [ObservableProperty]
        private FlatTreeDataGridSource<MyFileInfos> _dataGridSource;

        private string? _workingDir;
        private string? WorkingModsFolder => _workingDir is null ? null : Path.Combine(_workingDir, "CustomMaps");
        private string? _selectedView;
        private string? _modDir;
        private readonly ILogger _logger;

        public ClassicViewViewModel(IObservableConfiguration configuration, ILoggerProvider loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(TQDBEditor.ClassicViewModule));
            _workingDir = configuration.GetWorkingDir();
            _modDir = configuration.GetModDir();
            OnViewSelected("Sources");
            configuration.AddWorkingDirChangeListener(x => _workingDir = x);
            configuration.AddModDirChangeListener(OnModDirChanged);

            DataGridSource = CreateBasicDataGridSource();
        }

        private FlatTreeDataGridSource<MyFileInfos> CreateBasicDataGridSource()
        {
            return new FlatTreeDataGridSource<MyFileInfos>(_files)
            {
                Columns =
                {
                    new TextColumn<MyFileInfos, string>("Name", x => Path.GetFileName(x.FullPath)),
                    //new TemplateColumn<FileInfo>("Test", new FuncDataTemplate<FileInfo>((_,_) => new Button { Content="Hi" })),
                }
            };
        }

        private void OnModDirChanged(string? path)
        {
            Reset();
            if (path != null && WorkingModsFolder != null && _selectedView != null)
            {
                _modDir = path;
                var modPath = Path.Combine(WorkingModsFolder, _modDir);
                TreeNodes = InitDirectory(Path.Combine(modPath, _selectedView.ToLower()));
            }
        }

        private ObservableCollection<Node> InitDirectory(string sourcePath)
        {
            var infos = Directory.CreateDirectory(sourcePath).EnumerateDirectories().ToList();
            Node root = new(sourcePath, _modDir! + "\\" + _selectedView);
            var ret = new ObservableCollection<Node>
            {
                root
            };
            foreach (var info in infos)
            {
                root.AddSubNode(new(info.FullName, info.Name, InitDirectory(info.FullName)));
            }
            return ret;
        }

        public void OnNodeSelected(Node? selectedNode)
        {
            _files.Clear();
            if (selectedNode != null)
                foreach (var info in Directory.CreateDirectory(selectedNode.Path).EnumerateFileSystemInfos().OfType<FileInfo>())
                    _files.Add(new(info.FullName, _logger));
        }

        public void OnViewSelected(string? view)
        {
            _selectedView = view;
            OnModDirChanged(_modDir);
            var dataGridSource = CreateBasicDataGridSource();
            switch (view)
            {
                case "Assets":
                    dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Status", x => string.Empty));
                    break;
                case "Database":
                    dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Description", x => x.Metadata.GetValueOrDefault().FileDescription));
                    dataGridSource.Columns.Add(new TextColumn<MyFileInfos, string>("Template", x => x.Metadata.GetValueOrDefault().FileDescription));
                    break;
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
        private DBRMetadata? _metadata;
        private readonly ILogger? _logger;
        public DBRMetadata? Metadata => _metadata ??= DBRMetaParser.ParseFile(FullPath, _logger);

        public MyFileInfos(string fullPath, ILogger? logger = null)
        {
            FullPath = fullPath;
            _logger = logger;
        }

    }

    public partial class Node : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Node>? _subNodes;
        [ObservableProperty]
        private string _title;

        public string Path { get; }

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
