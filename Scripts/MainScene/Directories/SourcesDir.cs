using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Transactions;

public partial class SourcesDir : Tree
{
    [Export(PropertyHint.NodeType, "Node")]
    private Config configNode;

    private readonly FileSystemWatcher sourceWatcher;

    private string sourcesDirPath;

    private TreeItem root;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sourcesDirPath = Path.Combine(configNode.ModDir, "source");

        root = CreateItem();
        root.SetText(0, "source");

        if (!Directory.Exists(sourcesDirPath))
        {
            GD.PrintErr($"The source directory {sourcesDirPath} could not be found, creating...");
            Directory.CreateDirectory(sourcesDirPath);
        }

        var sourceWatcher = new FileSystemWatcher(sourcesDirPath)
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
        AddDirectoryToTree(root, sourcesDirPath);
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    //public override void _Process(double delta)
    //{
    //}

    public override void _ExitTree()
    {
        sourceWatcher.Dispose();
        base._ExitTree();
    }
}
