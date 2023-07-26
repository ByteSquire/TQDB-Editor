using Prism.Events;
using System.Linq;
using TQDBEditor.Events;
using TQDBEditor.FileViewModule.Views;

namespace TQDBEditor.FileViewModule
{
    public class DBRAccessHandler
    {
        public DBRAccessHandler(IEventAggregator ea)
        {
            ea.GetEvent<DBRAccessEvent>().Subscribe(OnDBRAccess);
        }

        private void OnDBRAccess(DBRAccessEventPayload payload)
        {
            var loads = payload.Accessed.GroupBy(x => x.dbr.TemplateRoot);
            foreach (var group in loads)
            {
                var template = group.Key;
                var files = group.Select(x => x.dbr);
                var fileViewWindow = new FileViewWindow(template, files);

                fileViewWindow.Show();
            }
        }
    }
}
