using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using TQDBEditor.Common;

namespace TQDBEditor
{
    public partial class NewModValidity : LineEdit
    {
        public override void _Ready()
        {
            GetParent().GetParent<ConfirmationDialog>().RegisterTextEnter(this);
            base._Ready();
        }

        public void _on_text_changed(string text)
        {
            CheckValid();
        }

        private void CheckValid()
        {
            var config = this.GetEditorConfig();
            var modName = Text;
            var CreateButton = GetParent().GetParent<ConfirmationDialog>().GetOkButton();
            if (!PathValidity.IsDirValid(modName))
            {
                if (string.IsNullOrEmpty(modName))
                    modName = "[None]";
                TooltipText = $"The provided directory name: {modName} for the new mod is invalid!";
                CreateButton.Disabled = true;
                AddThemeColorOverride("font_color", Colors.Red);
                return;
            }
            if (Directory.Exists(Path.Combine(config.ModsDir, modName)))
            {
                TooltipText = $"The provided directory name: {modName} for the new mod already exists!";
                CreateButton.Disabled = true;
                AddThemeColorOverride("font_color", Colors.Red);
                return;
            }

            TooltipText = string.Empty;
            AddThemeColorOverride("font_color", Colors.White);
            CreateButton.Disabled = false;
        }

        public void _on_about_to_popup()
        {
            CheckValid();
        }
    }
}