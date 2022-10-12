using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDB_Parser;

namespace TQDBEditor
{
    public partial class Templates : Node
    {
        private TemplateManager templateManager;
        private Config config;
        private ILogger logger;

        public TemplateManager TemplateManager => templateManager;

        public override void _Ready()
        {
            config = this.GetEditorConfig();
            logger = this.GetConsoleLogger();
            templateManager = new TemplateManager(config.WorkingDir, logger);
        }

    }
}
