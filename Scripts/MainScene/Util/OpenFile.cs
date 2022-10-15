using Godot;
using System;
using System.Linq;
using TQDBEditor.Files;
using TQDB_Parser;
using TQDB_Parser.DBR;
using Microsoft.Extensions.Logging;
using TQDBEditor.EditorScripts;

namespace TQDBEditor
{
    public partial class OpenFile : Node
    {
        [Export]
        private Node files;

        private Templates templates;
        private TemplateManager tplManager;
        private ILogger logger;

        public override void _Ready()
        {
            templates = this.GetTemplates();
            if (templates is null)
                return;
            logger = this.GetConsoleLogger();
            if (files is null)
                return;
            tplManager = templates.TemplateManager;

            GetTree().Root.GuiEmbedSubwindows = false;

            files.GetNode<FilesViewSource>("FilesViewSource").FileActivated += OnFileActivated;
            files.GetNode<FilesViewAssets>("FilesViewAssets").FileActivated += OnFileActivated;
            files.GetNode<FilesViewDatabase>("FilesViewDatabase").FileActivated += OnDBRActivated;
        }

        private void OnFileActivated(string filePath)
        {
            // TODO: use known file extensions like msh and so on to start the right tool
            OS.ShellOpen(filePath);
        }

        private void OnDBRActivated(string filePath, string template)
        {
            tplManager.ResolveIncludes(tplManager.GetRoot(template));
            var dbrParser = new DBRParser(tplManager, logger);

            var dbrFile = dbrParser.ParseFile(filePath);
            //GD.Print(dbrFile);

            var genericEditor = ResourceLoader.Load<PackedScene>("res://Editors/Generic.tscn")
                .Instantiate<EditorWindow>();

            genericEditor.DBRFile = dbrFile;

            genericEditor.Position = GetTree().Root.Position + new Vector2i(40, 40);

            GetTree().Root.CallDeferred("add_child", genericEditor);
            //genericEditor.Show();
        }
    }
}
