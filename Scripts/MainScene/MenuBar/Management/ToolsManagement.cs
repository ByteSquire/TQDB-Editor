using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
    {
        [Signal]
        public delegate void ExtractGameFilesClickedEventHandler();

        [Export]
        private ConfirmationDialog optionsDialog;

        public void _on_tools_extract_game_files()
        {
            GD.Print("Tools -> Extract game files...");
            EmitSignal(nameof(ExtractGameFilesClicked));
        }

        public void _on_tools_options()
        {
            GD.Print("Tools -> Options...");

            optionsDialog.PopupCenteredRatio(.4f);
        }
    }
}