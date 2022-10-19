using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Transactions;

namespace TQDBEditor.Files
{
    public abstract partial class DirViewBase : Tree
    {
        private Config configNode;

        private FileSystemWatcher sourceWatcher;

        private string dirPath;

        private TreeItem root;

        protected ILogger logger;
        protected abstract string SubPath { get; }

        [Signal]
        public delegate void DirSelectedEventHandler();

        private string relativePath;
        public string SelectedDir => Path.Combine(dirPath, relativePath ?? string.Empty);

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            configNode = this.GetEditorConfig();
            logger = this.GetConsoleLogger();
            if (configNode is null)
                return;
            if (logger is null)
                return;

            ItemSelected += OnDirSelected;
            configNode.ModNameChanged += OnModChanged;
            Init();
        }

        private void OnModChanged()
        {
            Clear();
            sourceWatcher.Dispose();
            Init();
        }

        private void Init()
        {
            dirPath = Path.Combine(configNode.ModDir, SubPath);

            root = CreateItem();
            root.SetText(0, Path.Combine(configNode.ModName, SubPath));

            if (!Directory.Exists(dirPath))
            {
                logger.LogWarning("The {SubPath} directory {dirPath} could not be found, creating...", SubPath, dirPath);
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

        private void AddDir(string dirPath)
        {
            (var closest, var remaining) = FindClosestNode(dirPath);
            //GD.Print("closest " + closest.GetText(0));
            //GD.Print("remaining:");
            //GD.Print(remaining);
            //GD.Print("");
            foreach (var dir in remaining)
            {
                closest = AddChildItem(closest, dir);
            }
        }

        protected (TreeItem, string[]) FindClosestNode(string dirPath)
        {
            var currDir = root.GetChild(0);
            var dirs = dirPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            var lastDir = currDir;
            for (; i < dirs.Length - 1; i++)
            {
                while (currDir is not null && currDir.GetText(0) != dirs[i])
                {
                    currDir = currDir.GetNext();
                }
                if (currDir is null)
                {
                    currDir = lastDir;
                    break;
                }
                if (currDir.GetText(0) == dirs[i])
                {
                    if (currDir.GetChildCount() == 0)
                    {
                        i++;
                        break;
                    }
                    if (i < dirs.Length - 2)
                    {
                        lastDir = currDir;
                        currDir = currDir.GetChild(0);
                    }
                }
            }
            return (currDir, dirs[i..]);
        }

        private void RemoveDir(string dirPath)
        {
            (var closest, var remaining) = FindClosestNode(dirPath);
            //GD.Print("closest " + closest.GetText(0));
            //GD.Print("remaining:");
            //GD.Print(remaining);
            //GD.Print("");
            if (remaining.Length > 1)
            {
                logger.LogError("Whoops, trying to delete a directory inside a subdirectory that wasn't picked up yet");
                return;
            }
            var currDir = closest.GetChild(0);
            while (currDir is not null && currDir.GetText(0) != remaining[0])
            {
                currDir = currDir.GetNext();
            }
            if (currDir is null)
            {
                logger.LogError("Whoops, trying to delete a directory that wasn't picked up yet");
                return;
            }
            if (currDir.GetText(0) == remaining[0])
            {
                currDir.Free();
            }
        }

        private void DirectoryRenamed(object sender, RenamedEventArgs e)
        {
            RemoveDir(Path.GetRelativePath(dirPath, e.OldFullPath));
            AddDir(Path.GetRelativePath(dirPath, e.FullPath));
        }

        private void DirectoryDeleted(object sender, FileSystemEventArgs e)
        {
            RemoveDir(Path.GetRelativePath(dirPath, e.FullPath));
        }

        private void DirectoryCreated(object sender, FileSystemEventArgs e)
        {
            AddDir(Path.GetRelativePath(dirPath, e.FullPath));
        }

        public void OnDirSelected()
        {
            var selected = GetSelected();
            if (selected == root)
            {
                EmitSignal(nameof(DirSelected), dirPath);
                return;
            }
            var paths = new List<string>();
            var parent = selected.GetParent();

            while (parent is not null)
            {
                paths.Add(parent.GetText(0));
                parent = parent.GetParent();
            }
            if (paths.Count > 0)
            {
                paths.RemoveAt(paths.Count - 1);
                paths.Reverse();
            }
            paths.Add(selected.GetText(0));

            relativePath = Path.Combine(paths.ToArray());
            EmitSignal(nameof(DirSelected));
        }

        public string GetCurrentDir() => Path.Combine(dirPath, relativePath);

        public override void _ExitTree()
        {
            sourceWatcher.Dispose();
            base._ExitTree();
        }
    }
}