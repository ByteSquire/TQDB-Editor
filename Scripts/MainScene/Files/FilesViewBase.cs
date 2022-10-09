using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public abstract partial class FilesViewBase : ScrollContainer
{
    [Export]
    protected DirViewBase dirView;
    [Export]
    protected Config configNode;
    [Export]
    private VBoxContainer column1;

    protected FileSystemWatcher fileDirWatcher;

    protected string fileDirPath;

    protected abstract Func<string, bool> IsSupportedFileExtension { get; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (dirView is null)
            return;
        if (configNode is null)
            return;
        if (column1 is null)
            return;

        dirView.DirSelected += OnSourceDirSelected;

        fileDirWatcher = new FileSystemWatcher()
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.FileName,
        };
        fileDirWatcher.Created += FileCreated;
        fileDirWatcher.Deleted += FileDeleted;
        fileDirWatcher.Renamed += FileRenamed;
    }

    protected abstract VBoxContainer[] GetAdditionalColumns();

    private void ClearTable()
    {
        var baseChildren = column1.GetChildren();
        for (int i = 2; i < baseChildren.Count; i++)
        {
            var child = baseChildren[i];
            child.QueueFree();
        }

        foreach (var column in GetAdditionalColumns())
        {
            var children = column.GetChildren();
            for (int i = 2; i < children.Count; i++)
            {
                var child = children[i];
                child.QueueFree();
            }
        }
    }

    protected abstract bool InitFile(string path);

    protected void InitDir(string path)
    {
        if (!Directory.Exists(path))
        {
            GD.PrintErr("Path: " + path + " could not be found");
            fileDirWatcher.EnableRaisingEvents = false;
            return;
        }

        fileDirWatcher.Path = path;
        fileDirWatcher.EnableRaisingEvents = true;
        fileDirPath = path;

        var filePaths = Directory.EnumerateFiles(path);

        foreach (var filePath in filePaths)
        {
            if (IsSupportedFile(filePath))
            {
                if (!InitFile(filePath))
                    continue;
                var fileName = Path.GetFileName(filePath);
                column1.AddChild(new Label
                {
                    Text = fileName,
                    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
                });
            }
        }
    }

    protected bool IsSupportedFile(string filePath)
    {
        return IsSupportedFileExtension(Path.GetExtension(filePath));
    }

    protected void FileCreated(object sender, FileSystemEventArgs e)
    {
        AddFile(e.FullPath);
    }

    protected void FileDeleted(object sender, FileSystemEventArgs e)
    {
        DeleteFile(e.FullPath);
    }

    protected void FileRenamed(object sender, RenamedEventArgs e)
    {
        DeleteFile(e.OldFullPath);
        AddFile(e.FullPath);
    }

    protected void DeleteFile(string filePath)
    {
        if (IsSupportedFile(filePath))
        {
            var fileName = Path.GetFileName(filePath);
            var baseChildren = column1.GetChildren();
            for (int i = 2; i < baseChildren.Count; i++)
            {
                var child = baseChildren[i] as Label;
                if (child.Text == fileName)
                {
                    child.QueueFree();

                    foreach (var column in GetAdditionalColumns())
                    {
                        var additionalChild = column.GetChild(i);
                        additionalChild.QueueFree();
                    }
                    break;
                }
            }
        }
    }

    protected void AddFile(string filePath)
    {
        if (IsSupportedFile(filePath))
        {
            var fileName = Path.GetFileName(filePath);
            column1.AddChild(new Label
            {
                Text = fileName,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
            });
            InitFile(filePath);
        }
    }

    public void OnSourceDirSelected(string path)
    {
        GD.Print("Hello " + path);
        ClearTable();
        InitDir(path);
    }

    public override void _ExitTree()
    {
        fileDirWatcher?.Dispose();
        base._ExitTree();
    }
}
