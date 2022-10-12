using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        [Signal]
        public delegate void CheckDBRecordsClickedEventHandler();
        [Signal]
        public delegate void ImportDBRecordClickedEventHandler();
        [Signal]
        public delegate void MoveDBRecordClickedEventHandler();
        [Signal]
        public delegate void ShowArchiveStatsClickedEventHandler();

        public void _on_database_check_records()
        {
            GD.Print("Database -> Check records");
            EmitSignal(nameof(CheckDBRecordsClicked));
        }

        public void _on_database_import_record()
        {
            GD.Print("Database -> Import record");
            EmitSignal(nameof(ImportDBRecordClicked));
        }

        public void _on_database_move_records()
        {
            GD.Print("Database -> Move records");
            EmitSignal(nameof(MoveDBRecordClicked));
        }

        public void _on_database_show_archive_stats()
        {
            GD.Print("Database -> Show archive stats");
            EmitSignal(nameof(ShowArchiveStatsClicked));
        }

        public void _on_database_stop()
        {
            GD.Print("Database -> Stop");
            _on_build_stop();
        }
    }
}