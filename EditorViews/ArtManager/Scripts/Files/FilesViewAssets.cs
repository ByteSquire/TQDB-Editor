using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TQDBEditor.Files
{
    public partial class FilesViewAssets : FilesViewBase
    {
        [Export]
        private ItemList column2; // status

        protected override Func<string, bool> IsSupportedFileExtension => x => true;

        protected override void ActivateItem(long index, string path)
        {
            // TODO: use known file extensions like msh and so on to start the right tool
            OS.ShellOpen(path);
        }

        protected override ItemList[] GetAdditionalColumns()
        {
            if (column2 is FileList col2)
                col2.otherLists = new ItemList[] { column1 };
            column2.GetVScrollBar().Visible = false;

            return new ItemList[] { column2 };
        }

        protected override bool InitFile(string path) => true;

        protected override void RemoveFile(int index) { }
    }
}