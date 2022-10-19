using Godot;
using System;
using System.IO;

namespace TQDBEditor.EditorScripts
{
    public partial class MenuBarManager : MenuBar
    {
        [Export]
        private AcceptDialog showTemplate;

        [Signal]
        public delegate void ToggleStatusbarEventHandler();
        [Signal]
        public delegate void ToggleToolbarEventHandler();
        [Signal]
        public delegate void ToggleDescriptionsEventHandler();

        public void _on_view_show_template()
        {
            GD.Print("View -> Show template");

            var templatePath = Path.Combine(this.GetTemplates().TemplateManager.TemplateBaseDir,
                editorWindow.DBRFile.TemplateRoot.FileName);
            showTemplate.GetNode<Label>("PathText").Text = templatePath;
            showTemplate.PopupCentered();
        }

        public void _on_view_toggle_descriptions()
        {
            GD.Print("View -> Toggle descriptions");

            EmitSignal(nameof(ToggleDescriptions));
        }

        public void _on_view_toggle_statusbar()
        {
            GD.Print("View -> Toggle statusbar");

            EmitSignal(nameof(ToggleStatusbar));
        }

        public void _on_view_toggle_toolbar()
        {
            GD.Print("View -> Toggle toolbar");

            EmitSignal(nameof(ToggleToolbar));
        }
    }
}