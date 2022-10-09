using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class FilesViewAssets : FilesViewBase
{
    [Export]
    private VBoxContainer column2;

    protected override Func<string, bool> IsSupportedFileExtension => x => true;

    protected override VBoxContainer[] GetAdditionalColumns()
    {
        return new VBoxContainer[] { column2 };
    }

    protected override void InitFile(string path)
    {

    }
}
