using Godot;
using Microsoft.Extensions.Logging;
using System;

namespace TQDBEditor
{
    public static class AutoloadsExtension
    {
        public static Config GetEditorConfig(this Node me)
        {
            return me.GetNode<Config>("/root/Config");
        }

        public static ILogger GetConsoleLogger(this Node me)
        {
            return me.GetNode<ConsoleLogHandler>("/root/Logging").Logger;
        }
    }
}