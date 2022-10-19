using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TQDBEditor.Files
{
    public abstract partial class FilesViewBase : VBoxContainer
    {
        [Export]
        protected DirViewBase dirView;
        [Export]
        protected ItemList column1;
        //[Export]
        //protected PackedScene fileNameLabelTemplate;

        [Signal]
        public delegate void FileActivatedEventHandler();

        protected ILogger logger;
        protected Config configNode;

        protected FileSystemWatcher fileDirWatcher;

        protected string fileDirPath;

        protected abstract Func<string, bool> IsSupportedFileExtension { get; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            configNode = this.GetEditorConfig();
            logger = this.GetConsoleLogger();
            if (dirView is null)
                return;
            if (configNode is null)
                return;
            if (logger is null)
                return;
            if (column1 is null)
                return;

            if (column1 is FileList col1)
                col1.otherLists = GetAdditionalColumns();
            column1.ItemActivated += OnItemActivated;

            configNode.ModNameChanged += OnModChanged;
            Init();
        }

        protected string activeFile;

        protected void OnItemActivated(long index)
        {
            activeFile = Path.Combine(dirView.SelectedDir, column1.GetItemText((int)index));
            ActivateItem(index, Path.Combine(dirView.SelectedDir, column1.GetItemText((int)index)));

            EmitSignal(nameof(FileActivated));
        }

        public string GetActiveFile() => activeFile;

        protected abstract void ActivateItem(long index, string path);

        private void OnModChanged()
        {
            ClearTable();
            fileDirWatcher.Dispose();
            Init();
        }

        private void Init()
        {
            dirView.DirSelected += OnSourceDirSelected;

            fileDirWatcher = new FileSystemWatcher()
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };
            fileDirWatcher.Created += FileCreated;
            fileDirWatcher.Deleted += FileDeleted;
            fileDirWatcher.Renamed += FileRenamed;
            fileDirWatcher.Changed += FileChanged;
        }

        protected abstract ItemList[] GetAdditionalColumns();

        private void ClearTable()
        {
            column1.Clear();
            foreach (var column in GetAdditionalColumns())
                column.Clear();

            // clears a vboxcontainer with heading and separator
            //var baseChildren = column1.GetChildren();
            //for (int i = 2; i < baseChildren.Count; i++)
            //{
            //    var child = baseChildren[i];
            //    child.QueueFree();
            //}

            //foreach (var column in GetAdditionalColumns())
            //{
            //    var children = column.GetChildren();
            //    for (int i = 2; i < children.Count; i++)
            //    {
            //        var child = children[i];
            //        child.QueueFree();
            //    }
            //}
        }

        protected abstract bool InitFile(string path);

        protected void InitDir(string path)
        {
            if (!Directory.Exists(path))
            {
                logger.LogError("Path: {path} could not be found", path);
                fileDirWatcher.EnableRaisingEvents = false;
                return;
            }

            fileDirWatcher.Path = path;
            fileDirWatcher.EnableRaisingEvents = true;
            fileDirPath = path;

            var filePaths = Directory.EnumerateFiles(path);

            foreach (var filePath in filePaths)
                AddFile(filePath);
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

        protected void FileChanged(object sender, FileSystemEventArgs e)
        {
            DeleteFile(e.FullPath);
            AddFile(e.FullPath);
        }

        protected void DeleteFile(string filePath)
        {
            if (IsSupportedFile(filePath))
            {
                var fileName = Path.GetFileName(filePath);
                for (int i = 0; i < column1.ItemCount; i++)
                {
                    if (column1.GetItemText(i) == fileName)
                    {
                        column1.RemoveItem(i);
                        break;
                    }
                }

                // delete from vboxcontainer
                //var baseChildren = column1.GetChildren();
                //for (int i = 2; i < baseChildren.Count; i++)
                //{
                //    var child = baseChildren[i] as Label;
                //    if (child.Text == fileName)
                //    {
                //        child.QueueFree();

                //        foreach (var column in GetAdditionalColumns())
                //        {
                //            var additionalChild = column.GetChild(i);
                //            additionalChild.QueueFree();
                //        }
                //        break;
                //    }
                //}
            }
        }

        protected void AddFile(string filePath)
        {
            if (IsSupportedFile(filePath))
            {
                var fileName = Path.GetFileName(filePath);
                var index = column1.AddItem(fileName);
                if (!InitFile(filePath))
                    column1.SetItemCustomFgColor(index, Colors.Red);

                // add to vboxcontainer
                //var newLabel = fileNameLabelTemplate.Instantiate<Label>();
                //newLabel.Set("parentNode", this);
                //newLabel.Connect("label_selected", new Callable(OnLabelClicked));
                //newLabel.Text = fileName;
                //newLabel.ThemeTypeVariation = InitFile(filePath) ? "" : "RedTextLabel";
                //column1.AddChild(newLabel);
                //column1.AddChild(new Label
                //{
                //    Text = fileName,
                //    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
                //    MouseFilter = MouseFilterEnum.Stop,
                //    MouseDefaultCursorShape = CursorShape.PointingHand,
                //    ThemeTypeVariation = InitFile(filePath) ? "" : "RedTextLabel"
                //});
            }
        }

        //protected Label lastSelected;

        //protected virtual void OnLabelClicked(Label sender)
        //{
        //    if (lastSelected is not null)
        //        EmitSignal(nameof(DeselectLabel), sender);

        //    if (lastSelected != sender)
        //    {
        //        void handler() => lastSelected = null;
        //        if (lastSelected is not null)
        //            lastSelected.TreeExited -= handler;
        //        lastSelected = sender;
        //        lastSelected.TreeExited += handler;
        //    }
        //}

        public void OnSourceDirSelected()
        {
            ClearTable();
            InitDir(dirView.GetCurrentDir());
        }

        public override void _ExitTree()
        {
            fileDirWatcher?.Dispose();
            base._ExitTree();
        }
    }
}