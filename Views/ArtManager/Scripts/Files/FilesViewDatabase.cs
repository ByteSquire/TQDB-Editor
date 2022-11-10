using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using TQDBEditor.Common;
using TQDB_Parser;
using TQDB_Parser.DBRMeta;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.Files
{
    public partial class FilesViewDatabase : FilesViewBase
    {
        [Export]
        private ItemList column2; // descriptions
        [Export]
        private ItemList column3; // templates

        protected override void ActivateItem(long index, string path)
        {
            OnDBRActivated(path, column3.GetItemText((int)index));
        }

        private void OnDBRActivated(string filePath, string template)
        {
            var tplManager = this.GetTemplateManager();
            var pckHandler = this.GetPCKHandler();
            GD.Print(string.Format("Opening file: {0} with template: {1}", filePath, template));
            try
            {
                var dbrParser = new DBRParser(tplManager, logger);

                tplManager.ResolveIncludes(tplManager.GetRoot(template));
                var dbrFile = dbrParser.ParseFile(filePath);

                var availableEditors = pckHandler.GetFileEditors(Path.GetFileName(template));

                EditorWindow editor;
                if (availableEditors is null || availableEditors.Count < 1)
                {
                    logger?.LogError("No editors found for file {file}", filePath);
                    return;
                }
                else
                {
                    var editorScene = availableEditors[0];
                    try
                    {
                        editor = editorScene.Instantiate<EditorWindow>();
                        editor.DBRFile = dbrFile;

                        editor.Position = GetTree().Root.Position + new Vector2i(40, 40);

                        GetTree().Root.CallDeferred("add_child", editor);
                    }
                    catch (InvalidCastException)
                    {
                        logger?.LogError("Error instantiating {scene}, does not extend {type}", editorScene.ResourcePath, typeof(EditorWindow));
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e, "Failed to parse file {file}", filePath);
            }
        }

        protected override Func<string, bool> IsSupportedFileExtension => x => x == ".dbr";

        protected override ItemList[] GetAdditionalColumns()
        {
            if (column2 is FileList col2)
                col2.otherLists = new ItemList[] { column1, column3 };
            column2.GetVScrollBar().Visible = false;

            if (column3 is FileList col3)
                col3.otherLists = new ItemList[] { column1, column2 };
            column3.GetVScrollBar().Visible = false;

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
            }
            catch (ParseException)
            {
                var index2 = column2.AddItem(" ", selectable: false);
                var index3 = column3.AddItem(" ", selectable: false);
                if (index2 != index3)
                    GD.PrintErr("What? Rows don't match!!");

                return false;
            }
            return true;
        }

        protected override void RemoveFile(int index)
        {
            column2.RemoveItem(index);
            column3.RemoveItem(index);
        }
    }
}