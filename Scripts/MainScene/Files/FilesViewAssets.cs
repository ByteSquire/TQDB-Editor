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
        private VBoxContainer column2; // status

        protected override Func<string, bool> IsSupportedFileExtension => x => true;

        protected override VBoxContainer[] GetAdditionalColumns()
        {
            return new VBoxContainer[] { column2 };
        }

        protected override bool InitFile(string path)
        {

            return true;
        }
    }
}