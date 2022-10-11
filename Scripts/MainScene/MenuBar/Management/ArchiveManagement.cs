using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_archive_build()
        {
            GD.Print("Archive -> Build");
        }

        public void _on_archive_compact()
        {
            GD.Print("Archive -> Compact");
        }

        public void _on_archive_show_archive_stats()
        {
            GD.Print("Archive -> Show archive stats");
            _on_database_show_archive_stats();
        }
    }
}