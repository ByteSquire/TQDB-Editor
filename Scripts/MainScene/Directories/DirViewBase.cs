using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;

public abstract partial class DirViewBase : Tree
{
    [Export(PropertyHint.NodeType, "Node")]
    private Config configNode;

    private FileSystemWatcher sourceWatcher;

    private string dirPath;

    private TreeItem root;

    protected abstract string SubPath { get; }

    [Signal]
    public delegate void DirSelectedEventHandler(string path);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (configNode is null)
            return;
        dirPath = Path.Combine(configNode.ModDir, SubPath);

        root = CreateItem();
        root.SetText(0, Path.Combine(configNode.ModName, SubPath));

        ItemSelected += OnDirSelected;

        if (!Directory.Exists(dirPath))
        {
            GD.PrintErr($"The source directory {dirPath} could not be found, creating...");
            Directory.CreateDirectory(dirPath);
        }

        sourceWatcher = new FileSystemWatcher(dirPath)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.DirectoryName,
        };
        sourceWatcher.Created += DirectoryCreated;
        sourceWatcher.Deleted += DirectoryDeleted;
        sourceWatcher.Renamed += DirectoryRenamed;

        AddDirectoriesToTree();
    }

    private void AddDirectoriesToTree()
    {
        AddDirectoryToTree(root, dirPath);
    }

    private void AddDirectoryToTree(TreeItem parent, string dirPath)
    {
        var paths = Directory.EnumerateDirectories(dirPath, "*", SearchOption.TopDirectoryOnly);
        //var roots = new Dictionary<TreeItem, string>();
        foreach (var path in paths)
        {
            var nextDir = Path.GetDirectoryName(path);
            var nodeName = Path.GetRelativePath(nextDir, path);
            var root = AddChildItem(parent, nodeName);

            AddDirectoryToTree(root, path);
        }
    }

    private TreeItem AddChildItem(TreeItem parent, string childName)
    {
        var newNode = CreateItem(parent);
        newNode.Collapsed = true;
        newNode.SetText(0, childName);
        return newNode;
    }

    private void DirectoryRenamed(object sender, RenamedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DirectoryDeleted(object sender, FileSystemEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void DirectoryCreated(object sender, FileSystemEventArgs e)
    {
        throw new NotImplementedException();
    }

    public void OnDirSelected()
    {
        var selected = GetSelected();
        var paths = new List<string>();
        var parent = selected.GetParent();

        while (parent is not null)
        {
            paths.Add(parent.GetText(0));
            parent = parent.GetParent();
        }

        paths.RemoveAt(paths.Count - 1);
        paths.Reverse();
        paths.Add(selected.GetText(0));

        var relativePath = Path.Combine(paths.ToArray());
        EmitSignal(nameof(DirSelected), Path.Combine(dirPath, relativePath));
    }

    public override void _ExitTree()
    {
        sourceWatcher.Dispose();
        base._ExitTree();
    }
}
