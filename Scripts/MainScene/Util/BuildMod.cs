using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TQDBEditor.Common;
using TQArchive_Wrapper;

namespace TQDBEditor
{
    public static class TQSpecialPaths
    {
        public static string GetCurrentDatabasePath(this Config me)
        {
            return Path.Combine(me.ModDir, "database");
        }

        public static string GetCurrentOutputPath(this Config me)
        {
            var docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var outputDir = Path.Combine(docs, "My Games", "Titan Quest - Immortal Throne", "CustomMaps", me.ModName);
            return outputDir;
        }

        public static string GetCurrentOutputDatabasePath(this Config me)
        {
            return Path.Combine(me.GetCurrentOutputPath(), "database");
        }

        public static string GetCurrentOutputArchivePath(this Config me)
        {
            return Path.Combine(me.GetCurrentOutputDatabasePath(), me.ModName + ".arz");
        }
    }

    class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GD.Print(formatter(state, exception));
            if (exception != null)
            {
                GD.Print(exception.ToString());
            }
        }
    }

    public partial class BuildMod : Node
    {
        [Export]
        private RichTextLabel statusLabel;
        [Export]
        private ProgressBar progress;

        [Signal]
        public delegate void ToggleBuildEventHandler();

        private static CancellationTokenSource cancelSource = new(5 * 60 * 1000);

        private Task work;
        private Task copy;

        private object progressLock = new();

        private DateTime startTime;

        private ILogger logger;

        public override void _Ready()
        {
            logger = this.GetConsoleLogger();
        }

        public override void _Process(double delta)
        {
            if (work is null)
                return;

            if (work.IsCompletedSuccessfully)
            {
                work = null;
                return;
            }

            if (work.IsFaulted)
                CancelBuild();
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
            var database = config.GetCurrentDatabasePath();
            var outputDatabase = config.GetCurrentOutputDatabasePath();
            Directory.CreateDirectory(outputDatabase);

            var filesToCopy = Directory.EnumerateFiles(database, "*.dbr", SearchOption.AllDirectories);

            progress.MaxValue = filesToCopy.Count() /** 2*/;
            progress.Value = 0;

            if (!cancelSource.TryReset())
            {
                cancelSource.Dispose();
                cancelSource = new(5 * 60 * 1000);
            }
            EmitSignal(nameof(ToggleBuild));
            //copy = CopyFiles(filesToCopy, database, outputDatabase);
            work = Work(filesToCopy, config.GetCurrentOutputArchivePath(), database);
        }

        private void SetStatus(Color color, string text)
        {
            statusLabel.Clear();
            statusLabel.PushColor(color);
            statusLabel.AddText(text);
        }

        private void ArzWriter_FileDone()
        {
            lock (progressLock)
                progress.Value += 1;
        }

        private void FileCopy_FileDone(string obj)
        {
            logger?.LogInformation("Copied {filename} to CustomMaps", obj);
            lock (progressLock)
                progress.Value += 1;
        }

        public void CancelBuild()
        {
            cancelSource.Cancel();
            try
            {
                work.Wait(500);
                logger?.LogWarning("Failed to cancel build");
            }
            catch (AggregateException e)
            {
                if (work.IsCanceled)
                {
                    GD.Print($"{nameof(TaskCanceledException)} thrown with message: {e.Message}");
                    logger?.LogInformation("Build cancelled");
                }
                else
                {
                    logger?.LogError(e, "Failed to build archive");
                }
            }
            work = null;
            ResetBuildInfo();
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
            logger?.LogInformation("Build done in {duration}", args: duration);
            ResetBuildInfo();
        }

        private void ResetBuildInfo()
        {
            SetStatus(Colors.Green, "Ready");
            progress.Value = 0;
            EmitSignal(nameof(ToggleBuild));
        }

        private Task Work(IEnumerable<string> filesToCopy, string archivePath, string databasePath)
        {
            var ret = new Task(() => DoWrite(filesToCopy, archivePath, databasePath), cancelSource.Token);
            ret.Start();
            return ret;
        }

        private void DoWrite(IEnumerable<string> filesToCopy, string archivePath, string database)
        {
            cancelSource.Token.ThrowIfCancellationRequested();
            GD.Print("Using archive " + archivePath);
            SetStatus(Colors.Orange, "Checking archive...");
            var manager = new ArzManager(archivePath, database, new DebugLogger());
            manager.FileDone += ArzWriter_FileDone;

            cancelSource.Token.ThrowIfCancellationRequested();
            var tplManager = this.GetTemplateManager();
            tplManager.ParseAllTemplates();
            tplManager.ResolveAllIncludes();
            cancelSource.Token.ThrowIfCancellationRequested();
            SetStatus(Colors.Orange, "Building...");

            //var dbrFiles = new ConcurrentQueue<DBRFile>();
            //var po = new ParallelOptions
            //{
            //    CancellationToken = cancelSource.Token,
            //    MaxDegreeOfParallelism = System.Environment.ProcessorCount,
            //};
            //try
            //{
            //    Parallel.ForEach(filesToCopy, po, x =>
            //        {
            //            try
            //            {
            //                dbrFiles.Enqueue(new DBRParser(tplManager, logger).ParseFile(x));
            //                lock (progressLock)
            //                    progress.Value += 1;
            //            }
            //            catch (ParseException) { }
            //        });
            //}
            //catch (OperationCanceledException) { }
            manager.SyncFiles(EnumerateFilesWithCancellation(), tplManager, true);
            cancelSource.Token.ThrowIfCancellationRequested();

            manager.WriteToDisk();
            OnDone();


            IEnumerable<string> EnumerateFilesWithCancellation()
            {
                foreach (var file in filesToCopy)
                {
                    cancelSource.Token.ThrowIfCancellationRequested();
                    yield return file;
                }
            }
        }

        public override void _ExitTree()
        {
            cancelSource.Dispose();
            base._ExitTree();
        }
    }
}