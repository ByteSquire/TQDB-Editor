using Godot;
using System;
using System.Linq;
using TQDBEditor.Files;
using TQDB_Parser;
using TQDB_Parser.DBR;
using Microsoft.Extensions.Logging;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;
using System.IO;

namespace TQDBEditor
{
    public partial class OpenFile : Node
    {
        [Export]
        private Node files;

        private FilesViewSource sourceView;
        private FilesViewAssets assetsView;
        private FilesViewDatabase databaseView;

        private TemplateManager tplManager;
        private PCKHandler pckHandler;
        private ILogger logger;

        public override void _Ready()
        {
            this.GetEditorConfig().TrulyReady += Init;
            if (this.GetEditorConfig().ValidateConfig())
                Init();
            logger = this.GetConsoleLogger();
            if (files is null)
                return;
            if (logger is null)
                return;
            pckHandler = this.GetPCKHandler();
            if (pckHandler is null)
                return;

            sourceView = files.GetNode<FilesViewSource>("FilesViewSource");
            assetsView = files.GetNode<FilesViewAssets>("FilesViewAssets");
            databaseView = files.GetNode<FilesViewDatabase>("FilesViewDatabase");

            sourceView.FileActivated += OnFileActivated;
            assetsView.FileActivated += OnAssetActivated;
            databaseView.FileActivated += OnDBRActivated;
        }

        private void Init()
        {
            tplManager = this.GetTemplateManager();
        }

        private void OnFileActivated()
        {
            OpenFileExternal(sourceView.GetActiveFile());
        }

        private void OnAssetActivated()
        {
            OpenFileExternal(assetsView.GetActiveFile());
        }

        private void OpenFileExternal(string filePath)
        {
            // TODO: use known file extensions like msh and so on to start the right tool
            OS.ShellOpen(filePath);
        }

        private void OnDBRActivated()
        {
            var filePath = databaseView.GetActiveFile();
            var template = databaseView.GetActiveTemplate();
            GD.Print(filePath, template);
            try
            {
                var dbrParser = new DBRParser(tplManager, logger);

                tplManager.ResolveIncludes(tplManager.GetRoot(template));
                var dbrFile = dbrParser.ParseFile(filePath);

                var availableEditors = pckHandler.GetFileEditors(Path.GetFileName(template));

                EditorWindow editor;
                if (availableEditors is null || availableEditors.Count < 1)
                {
                    logger.LogError("No editors found for file {file}", filePath);
                    return;
                }
                else
                    editor = availableEditors[0].Instantiate<EditorWindow>();
                editor.DBRFile = dbrFile;

                editor.Position = GetTree().Root.Position + new Vector2i(40, 40);

                GetTree().Root.CallDeferred("add_child", editor);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to parse file {file}", filePath);
            }
        }
    }
}
