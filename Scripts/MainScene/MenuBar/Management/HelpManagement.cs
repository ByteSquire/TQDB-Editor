using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_help_about()
        {
            GD.Print("Help -> About...");
        }

        public void _on_help_help()
        {
            GD.Print("Help -> Help...");
        }

        public void _on_help_install_templates()
        {
            GD.Print("Help -> Install templates...");
        }

        public void _on_help_install_tutorials()
        {
            GD.Print("Help -> Install tutorials...");
        }

    }
}