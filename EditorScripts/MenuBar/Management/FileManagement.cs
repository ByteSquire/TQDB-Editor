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

        private Vector2i minSize = new(600, 500);

        private void InitFileManagement()
        {
            changeTemplatePopup.FileSelected += ChangeTemplate;
            var templatePath = Path.Combine(this.GetTemplates().TemplateManager.TemplateBaseDir,
                editorWindow.DBRFile.TemplateRoot.FileName);
            changeTemplatePopup.CurrentFile = templatePath;

            saveAsPopup.FileSelected += SaveFile;
            saveAsPopup.CurrentFile = editorWindow.DBRFile.FilePath;
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
        }

        public void _on_file_save_as()
        {
            GD.Print("File -> Save as");

            saveAsPopup.PopupCentered(minSize);
        }

        private void SaveFile(string path)
        {
            editorWindow.DBRFile.SaveFile(saveAs: path);
        }

        public void _on_file_set_template()
        {
            GD.Print("File -> Set template");

            changeTemplatePopup.PopupCentered(minSize);
        }

        private void ChangeTemplate(string path)
        {
            var manager = this.GetTemplates().TemplateManager;
            var parser = new DBRParser(manager, this.GetConsoleLogger());

            var templateName = manager.GetTemplateName(path);
            editorWindow.DBRFile = parser.ChangeFileTemplate(editorWindow.DBRFile, templateName);
        }
    }
}