using Godot;
using System;
using System.Linq;
using TQDBEditor.Files;
using TQDB_Parser;
using TQDB_Parser.DBR;
using Microsoft.Extensions.Logging;

namespace TQDBEditor
{
    public partial class OpenFile : Node
    {
        [Export]
        private Node files;

        private Templates templates;
        private ILogger logger;

        public override void _Ready()
        {
            templates = this.GetTemplates();
            if (templates is null)
                return;
            logger = this.GetConsoleLogger();
            if (files is null)
                return;

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
            var dbrParser = new DBRParser(templates.TemplateManager, logger);

            var dbrFile = dbrParser.ParseFile(filePath);
            GD.Print(dbrFile);
        }
    }
}
