using Godot;
using Microsoft.Extensions.Logging;
using System;
using TQDB_Parser;

namespace TQDBEditor
{
    public static class AutoloadsExtension
    {
        public static Config GetEditorConfig(this Node me)
        {
            return me.GetNode<Config>("/root/Config");
        }

        public static TemplateManager GetTemplateManager(this Node me)
        {
            return me.GetNode<Templates>("/root/Templates").TemplateManager;
        }

        public static PCKHandler GetPCKHandler(this Node me)
        {
            return me.GetNode<PCKHandler>("/root/PckHandler");
        }

        public static ILogger GetConsoleLogger(this Node me)
        {
            return me.GetNode<ConsoleLogHandler>("/root/Logging").Logger;
        }
    }
}