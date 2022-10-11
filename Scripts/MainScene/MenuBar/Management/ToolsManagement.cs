using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_tools_extract_game_files()
        {
            GD.Print("Tools -> Extract game files...");
        }

        public void _on_tools_options()
        {
            GD.Print("Tools -> Options...");
        }
    }
}