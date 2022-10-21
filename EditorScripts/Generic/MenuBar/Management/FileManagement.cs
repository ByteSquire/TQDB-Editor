using Godot;
using System;
using System.IO;
using TQDB_Parser;
using TQDB_Parser.DBR;

namespace TQDBEditor.EditorScripts
{
    public partial class MenuBarManager : MenuBar
    {
        [Export]
        private FileDialog saveAsPopup;

        [Export]
        private FileDialog changeTemplatePopup;

        [Signal]
        public delegate void SavedEventHandler();

        private Vector2i minSize = new(600, 500);

        private void InitFileManagement()
        {
            changeTemplatePopup.FileSelected += ChangeTemplate;
            var templatePath = Path.Combine(this.GetTemplateManager().TemplateBaseDir,
                editorWindow.DBRFile.TemplateRoot.FileName.ToLowerInvariant());
            // currently overwrites currentpath, not useful
            //changeTemplatePopup.RootSubfolder = this.GetEditorConfig().WorkingDir;
            changeTemplatePopup.CurrentPath = templatePath;

            saveAsPopup.FileSelected += SaveFile;
            // currently overwrites currentpath, not useful
            //saveAsPopup.RootSubfolder = this.GetEditorConfig().ModDir;
            saveAsPopup.CurrentPath = editorWindow.DBRFile.FilePath;
        }

        public void _on_file_exit()
        {
            GD.Print("File -> Exit");

            editorWindow.OnCloseEditor();
        }

        public void _on_file_save()
        {
            GD.Print("File -> Save");

            editorWindow.DBRFile.SaveFile();
            EmitSignal(nameof(Saved));
        }

        public void _on_file_save_as()
        {
            GD.Print("File -> Save as");

            // doesn't work, for some reason GetSelected still returns null
            //var saveTree = saveAsPopup.GetVbox().GetChild(2, true).GetChild<Tree>(0, true);
            //saveTree.GrabFocus();
            //saveTree.ScrollToItem(saveTree.GetSelected());
            saveAsPopup.PopupCentered(minSize);
            EmitSignal(nameof(Saved));
        }

        private void SaveFile(string path)
        {
            editorWindow.DBRFile.SaveFile(saveAs: path);
        }

        public void _on_file_set_template()
        {
            GD.Print("File -> Set template");

            // doesn't work, for some reason GetSelected still returns null
            //var changeTplTree = changeTemplatePopup.GetVbox().GetChild(2, true).GetChild<Tree>(0, true);
            //changeTplTree.GrabFocus();
            //changeTplTree.ScrollToItem(changeTplTree.GetSelected());
            changeTemplatePopup.PopupCentered(minSize);
        }

        private void ChangeTemplate(string path)
        {
            var manager = this.GetTemplateManager();
            var parser = new DBRParser(manager, this.GetConsoleLogger());

            var templateName = manager.GetTemplateName(path);
            editorWindow.DBRFile = parser.ChangeFileTemplate(editorWindow.DBRFile, templateName);
        }
    }
}