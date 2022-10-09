using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class FilesViewDatabase : FilesViewBase
{
    [Export]
    private VBoxContainer column2;
    [Export]
    private VBoxContainer column3;

    protected override Func<string, bool> IsSupportedFileExtension => x => x == ".dbr";

    protected override VBoxContainer[] GetAdditionalColumns()
    {
        return new VBoxContainer[] { column2, column3 };
    }

    protected override void InitFile(string path)
    {
        
    }
}
