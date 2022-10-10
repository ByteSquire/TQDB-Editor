using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQDB_Parser;
using TQDB_Parser.DBRMeta;

namespace TQDBEditor.Files
{
    public partial class FilesViewDatabase : FilesViewBase
    {
        [Export]
        private ItemList column2; // descriptions
        [Export]
        private ItemList column3; // templates

        protected override Func<string, bool> IsSupportedFileExtension => x => x == ".dbr";

        protected override ItemList[] GetAdditionalColumns()
        {
            return new ItemList[] { column2, column3 };
        }

        protected override bool InitFile(string path)
        {
            try
            {
                var metaData = DBRMetaParser.ParseFile(path, this.GetConsoleLogger());
                var desc = metaData.FileDescription;
                var tplName = metaData.TemplateName;
                var index2 = column2.AddItem(string.IsNullOrEmpty(desc) ? " " : desc, selectable: false);
                var index3 = column3.AddItem(string.IsNullOrEmpty(tplName) ? " " : tplName, selectable: false);
                if (index2 != index3)
                    GD.PrintErr("What? Rows don't match!!");

                // add to vboxcontainer
                //column2.AddChild(new Label
                //{
                //    Text = metaData.FileDescription,
                //    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
                //});
                //column3.AddChild(new Label
                //{
                //    Text = metaData.TemplateName,
                //    TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
                //});
            }
            catch (ParseException)
            {
                var index2 = column2.AddItem(" ", selectable: false);
                var index3 = column3.AddItem(" ", selectable: false);
                if (index2 != index3)
                    GD.PrintErr("What? Rows don't match!!");

                // add to vboxcontainer
                //column2.AddChild(new Label());
                //column3.AddChild(new Label());
                return false;
            }
            return true;
        }
    }
}