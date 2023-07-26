using Prism.Events;
using System.Collections.Generic;
using System.Diagnostics;
using TQDB_Parser.DBR;
using TQDB_Parser.DBRMeta;

namespace TQDBEditor.Events
{
    public class DBRAccessEvent : PubSubEvent<DBRAccessEventPayload> { }

    public readonly struct DBRAccessEventPayload
    {
        public IReadOnlyList<(DBRMetadata metadata, DBRFile dbr)> Accessed { get; } = new List<(DBRMetadata metadata, DBRFile dbr)>();

        public DBRAccessEventPayload(IEnumerable<DBRFile> Accessed)
        {
            var list = new List<(DBRMetadata metadata, DBRFile dbr)>();
            foreach (var dbr in Accessed)
            {
                var description = string.Empty;
                try
                {
                    description = dbr["FileDescription"].Value;
                }
                catch (KeyNotFoundException) { }
                var metadata = new DBRMetadata(dbr.TemplateRoot.FileName, description);
                list.Add((metadata, dbr));
            }
            this.Accessed = list.AsReadOnly();
        }
    }
}
