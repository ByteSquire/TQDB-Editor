using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        [Export]
        private FileDialog workingDirDialog;

        [Signal]
        public delegate void RenameFileClickedEventHandler();

        public void _on_file_set_working_folder()
        {
            GD.Print("File -> Set working folder");
            var current = config.WorkingDir;

            if (!string.IsNullOrWhiteSpace(current))
                workingDirDialog.CurrentPath = current;

            workingDirDialog.DirSelected -= OnWorkingDirSelected;
            workingDirDialog.DirSelected += OnWorkingDirSelected;

            workingDirDialog.PopupCenteredRatio(.5f);
        }

        private void OnWorkingDirSelected(string dir)
        {
            GD.Print("Setting working dir to: " + dir);
            config.WorkingDir = dir;
        }

        public void _on_file_exit()
        {
            GD.Print("File -> Exit");
        }

        public void _on_file_rename()
        {
            GD.Print("File -> Rename");
            EmitSignal(nameof(RenameFileClicked));
        }
    }
}