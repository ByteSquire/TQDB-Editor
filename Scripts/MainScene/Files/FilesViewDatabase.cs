using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQDB_Parser;
using TQDB_Parser.DBRMeta;

public partial class FilesViewDatabase : FilesViewBase
{
    [Export]
    private VBoxContainer column2; // descriptions
    [Export]
    private VBoxContainer column3; // templates

    protected override Func<string, bool> IsSupportedFileExtension => x => x == ".dbr";

    protected override VBoxContainer[] GetAdditionalColumns()
    {
        return new VBoxContainer[] { column2, column3 };
    }

    protected override bool InitFile(string path)
    {
        try
        {
            var metaData = DBRMetaParser.ParseFile(path);
            column2.AddChild(new Label
            {
                Text = metaData.FileDescription,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
            });
            column3.AddChild(new Label
            {
                Text = metaData.TemplateName,
                TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
            });
        }
        catch (ParseException e)
        {
            GD.PrintErr(e);
            return false;
        }
        return true;
    }
}
