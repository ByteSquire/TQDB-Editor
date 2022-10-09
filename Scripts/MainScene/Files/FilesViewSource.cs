using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class FilesViewSource : FilesViewBase
{
    protected override Func<string, bool> IsSupportedFileExtension => x => true;

    protected override VBoxContainer[] GetAdditionalColumns()
    {
        // no additional columns
        return System.Array.Empty<VBoxContainer>();
    }

    protected override void InitFile(string path)
    {
        // nothing additional to do
        return;
    }
}
