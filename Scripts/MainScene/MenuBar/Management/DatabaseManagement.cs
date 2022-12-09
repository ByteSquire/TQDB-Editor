using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using TQArchive_Wrapper;
using TQDBEditor.Common;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
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

            var config = this.GetEditorConfig();
            var logger = this.GetConsoleLogger();
            var archivePath = config.GetCurrentOutputArchivePath();
            try
            {
                var arzReader = new ArzReader(archivePath, logger);

                logger.LogInformation("{archive}:\nNumber of Files in archive: {numFiles}\nNumber of Strings in archive: {numStrings}",
                    archivePath,
                    arzReader.GetDBRFileInfos().Count(),
                    arzReader.GetStringList().Count());
            }
            catch (Exception) { }
        }

        public void _on_database_stop()
        {
            GD.Print("Database -> Stop");
            _on_build_stop();
        }
    }
}