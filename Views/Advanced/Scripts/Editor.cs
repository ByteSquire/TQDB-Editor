using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TQDB_Parser;
using TQDBEditor.Common;

namespace TQDBEditor
{
    public partial class Editor : Control
    {
        [Export]
        private NodePath tableNode;
        private ItemList table => GetNode(tableNode) as ItemList;

        private Config config;
        private PCKHandler pckHandler;
        private TemplateManager tplManager;
        private FileSystemWatcher fileDirWatcher;
        private System.Collections.Generic.Dictionary<string, List<int>> fileNodes;
        private bool templatesParsed;
        private bool needsInit;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            config = this.GetEditorConfig();
            pckHandler = this.GetPCKHandler();
            templatesParsed = false;
            needsInit = false;
            if (config.ValidateConfig())
                Init();
            config.ModNameChanged += Init;

            table.MaxColumns = 6;

            VisibilityChanged += ShowVisible;
        }

        private void Init()
        {
            tplManager = this.GetTemplateManager();

            fileDirWatcher = new FileSystemWatcher()
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            };
            fileDirWatcher.Created += FileCreated;
            fileDirWatcher.Deleted += FileDeleted;
            fileDirWatcher.Renamed += FileRenamed;
            fileDirWatcher.Changed += FileChanged;

            fileNodes = new();
            needsInit = true;
        }

        private void ShowVisible()
        {
            if (Visible == false || needsInit == false)
                return;

            if (!templatesParsed)
            {
                tplManager.ParseAllTemplates();
                tplManager.ResolveAllIncludes();
                templatesParsed = true;
            }
            var tasks = new List<Task<(string, Array<string[]>)>>();

            foreach (var file in Directory.EnumerateFiles(Path.Combine(config.ModDir, "database"), "*.dbr", SearchOption.AllDirectories))
            {
                tasks.Add(
                Task.Factory.StartNew(() => (file, CreateRows(file)))
  //.ContinueWith(tsk => rowsToAdd.TryAdd(file, tsk.Result))
  //.ContinueWith(tsk => AddFile(file, tsk.Result))
  );
            }

            foreach (var task in tasks)
            {
                task.Wait();
                AddFile(task.Result.Item1, task.Result.Item2);
            }

            needsInit = false;
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

        private static readonly object tableLock = new();

        private void AddFile(string filePath, Array<string[]> rows = null)
        {
            rows ??= CreateRows(filePath);
            foreach (var row in rows)
            {
                if (row.Length != table.MaxColumns)
                    GD.PrintErr("Nope!");
                foreach (var cell in row)
                {
                    lock (tableLock)
                    {
                        var rowIndex = table.AddItem(cell);
                        //GD.Print("Item added");
                        if (fileNodes.TryGetValue(filePath, out var existingList))
                            existingList.Add(rowIndex);
                        else
                            fileNodes.Add(filePath, new List<int>() { rowIndex });
                    }
                }
            }
        }

        private Array<string[]> CreateRows(string filePath)
        {
            var parser = new DBRParser(tplManager, this.GetConsoleLogger());

            var file = parser.ParseFile(filePath);

            var ret = new Array<string[]>();
            foreach (var entry in file.Entries)
            {
                var row = new string[]
                {
                    filePath,
                    entry.Name,
                    entry.Template.Type.ToString(),
                    entry.Template.Class.ToString(),
                    entry.Template.Description,
                    entry.Value,
                };
                ret.Add(row);
            }
            return ret;
        }

        private void DeleteFile(string filePath)
        {
            if (fileNodes.TryGetValue(filePath, out var rowIndices))
                foreach (var rowIndex in rowIndices)
                {
                    table.RemoveItem(rowIndex);
                }
        }
    }
}