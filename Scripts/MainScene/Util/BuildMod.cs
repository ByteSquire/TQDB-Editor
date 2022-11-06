using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TQDBEditor.Common;
using TQArchive_Wrapper;
using TQDB_Parser;
using TQDB_Parser.DBR;

namespace TQDBEditor
{
    public partial class BuildMod : Node
    {
        [Export]
        private RichTextLabel statusLabel;
        [Export]
        private ProgressBar progress;

        [Signal]
        public delegate void ToggleBuildEventHandler();

        private ArzWriter arzWriter;

        private static readonly CancellationTokenSource cancelSource = new(5 * 60 * 1000);

        private Task work;
        private Task copy;

        private object progressLock = new();

        private DateTime startTime;

        public override void _Ready()
        {
        }

        public void Build()
        {
            if (work is not null)
            {
                if (work.Status == TaskStatus.Running)
                    return;
                //if (copy.Status == TaskStatus.Running)
                //    return;
            }
            startTime = DateTime.Now;
            var config = this.GetEditorConfig();
            var database = Path.Combine(config.ModDir, "database");
            var docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var outputDir = Path.Combine(docs, "My Games", "Titan Quest - Immortal Throne",
                "CustomMaps", config.ModName);
            var outputDatabase = Path.Combine(outputDir, "database");

            var filesToCopy = Directory.EnumerateFiles(database, "*.dbr", SearchOption.AllDirectories);

            var old = arzWriter;
            arzWriter = new ArzWriter(Path.Combine(outputDatabase, config.ModName + ".arz"),
                this.GetConsoleLogger());

            if (old is null)
                arzWriter.FileDone += ArzWriter_FileDone;

            progress.MaxValue = filesToCopy.Count() * 2;
            progress.Value = 0;
            statusLabel.Clear();
            statusLabel.PushColor(Colors.Orange);
            statusLabel.AddText("Building...");

            cancelSource.TryReset();
            EmitSignal(nameof(ToggleBuild));
            //copy = CopyFiles(filesToCopy, database, outputDatabase);
            work = Work(filesToCopy);
        }

        private void ArzWriter_FileDone(string obj)
        {
            this.GetConsoleLogger().LogInformation("Added {filename} to archive", obj);
            lock (progressLock)
                progress.Value += 1;
        }

        private void FileCopy_FileDone(string obj)
        {
            this.GetConsoleLogger().LogInformation("Copied {filename} to CustomMaps", obj);
            lock (progressLock)
                progress.Value += 1;
        }

        public void CancelBuild()
        {
            cancelSource.Cancel();
            this.GetConsoleLogger().LogInformation("Build cancelled");
        }

        private async Task CopyFiles(IEnumerable<string> filesToCopy, string inputDatabase, string outputDatabase)
        {
            foreach (var file in filesToCopy)
            {
                GD.Print("Copy from " + file);
                using FileStream sourceStream = File.Open(file, FileMode.Open);
                var relativePath = Path.GetRelativePath(inputDatabase, file);
                GD.Print("Copy to " + Path.Combine(outputDatabase, relativePath));
                using FileStream destinationStream = File.Create(Path.Combine(outputDatabase, relativePath));
                await sourceStream.CopyToAsync(destinationStream, cancelSource.Token);
                FileCopy_FileDone(file);
            }
        }

        private void OnDone()
        {
            var duration = (DateTime.Now - startTime).ToString("mm':'ss");
            this.GetConsoleLogger().LogInformation("Build done in {duration}", args: duration);
            ResetBuildInfo();
        }

        private void ResetBuildInfo()
        {
            statusLabel.Clear();
            statusLabel.PushColor(Colors.Green);
            statusLabel.AddText("Ready");
            EmitSignal(nameof(ToggleBuild));
        }

        private async Task Work(IEnumerable<string> filesToCopy)
        {
            var ret = new Task(DoWrite, state: filesToCopy, cancelSource.Token);
            ret.Start();
            await ret;
        }

        private void DoWrite(object filesToCopy)
        {
            var manager = this.GetTemplateManager();
            manager.ParseAllTemplates();
            manager.ResolveAllIncludes();
            var logger = this.GetConsoleLogger();
            if (filesToCopy is IEnumerable<string> files)
            {
                var dbrFiles = new ConcurrentQueue<DBRFile>();
                var po = new ParallelOptions
                {
                    CancellationToken = cancelSource.Token,
                    MaxDegreeOfParallelism = System.Environment.ProcessorCount,
                };
                try
                {
                    Parallel.ForEach(files, po, x =>
                        {
                            try
                            {
                                dbrFiles.Enqueue(new DBRParser(manager, logger).ParseFile(x));
                                lock (progressLock)
                                    progress.Value += 1;
                            }
                            catch (ParseException) { }
                        });
                }
                catch (OperationCanceledException) { }
                arzWriter.Write(dbrFiles);
            }
            OnDone();
        }

        public override void _ExitTree()
        {
            cancelSource.Dispose();
            base._ExitTree();
        }
    }
}