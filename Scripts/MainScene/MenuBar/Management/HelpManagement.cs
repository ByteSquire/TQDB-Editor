using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
    {
        [Export]
        private AcceptDialog aboutPopup;
        [Signal]
        public delegate void InstallTemplatesEventHandler();
        [Signal]
        public delegate void InstallTutorialsEventHandler();

        public void _on_help_about()
        {
            GD.Print("Help -> About...");
            aboutPopup.PopupCentered();
        }

        public void _on_help_help()
        {
            GD.Print("Help -> Help...");
            OS.ShellOpen("https://github.com/ByteSquire/TQDB-Editor/wiki");
        }

        public void _on_help_install_templates()
        {
            GD.Print("Help -> Install templates...");
            EmitSignal(nameof(InstallTemplates));
        }

        public void _on_help_install_tutorials()
        {
            GD.Print("Help -> Install tutorials...");
            EmitSignal(nameof(InstallTutorials));
        }

    }
}