using Avalonia.Controls.ApplicationLifetimes;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var fileViewWindow = new FileViewWindow();

            fileViewWindow.Show();
        }
    }
}
