using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        [Signal]
        public delegate void BuildArchiveClickedEventHandler();
        [Signal]
        public delegate void CompactArchiveClickedEventHandler();

        public void _on_archive_build()
        {
            GD.Print("Archive -> Build");
            EmitSignal(nameof(BuildArchiveClicked));
        }

        public void _on_archive_compact()
        {
            GD.Print("Archive -> Compact");
            EmitSignal(nameof(CompactArchiveClicked));
        }

        public void _on_archive_show_archive_stats()
        {
            GD.Print("Archive -> Show archive stats");
            _on_database_show_archive_stats();
        }
    }
}