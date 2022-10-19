using Godot;
using System;
using System.Linq;
using TQDBEditor.Files;
using TQDB_Parser;
using TQDB_Parser.DBR;
using Microsoft.Extensions.Logging;
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
        private ILogger logger;

        public override void _Ready()
        {
            logger = this.GetConsoleLogger();
            if (files is null)
                return;
            tplManager = this.GetTemplateManager();
            if (tplManager is null)
                return;

            GetTree().Root.GuiEmbedSubwindows = false;

            sourceView = files.GetNode<FilesViewSource>("FilesViewSource");
            assetsView = files.GetNode<FilesViewAssets>("FilesViewAssets");
            databaseView = files.GetNode<FilesViewDatabase>("FilesViewDatabase");

            sourceView.FileActivated += OnFileActivated;
            assetsView.FileActivated += OnAssetActivated;
            databaseView.FileActivated += OnDBRActivated;
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


                var genericEditor = ResourceLoader.Load<PackedScene>("res://Editors/Generic.tscn")
                    .Instantiate<EditorWindow>();

                genericEditor.DBRFile = dbrFile;

                genericEditor.Position = GetTree().Root.Position + new Vector2i(40, 40);

                GetTree().Root.CallDeferred("add_child", genericEditor);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to parse file {file}", filePath);
            }
        }
    }
}
