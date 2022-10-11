using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_database_check_records()
        {
            GD.Print("Database -> Check records");
        }

        public void _on_database_import_record()
        {
            GD.Print("Database -> Import record");
        }

        public void _on_database_move_records()
        {
            GD.Print("Database -> Move records");
        }

        public void _on_database_show_archive_stats()
        {
            GD.Print("Database -> Show archive stats");
        }

        public void _on_database_stop()
        {
            GD.Print("Database -> Stop");
        }
    }
}