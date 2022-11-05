using Godot;
using System;
using System.IO;

namespace TQDBEditor
{
    public partial class ValidityCheck : RichTextLabel
    {
        private static bool IsValid(string path)
        {
            return Directory.Exists(path);
        }

        private bool workingValid;
        private bool toolsValid;
        private bool buildValid;

        //public override void _Ready()
        //{
        //    _on_working_dir_text_text_changed(GetParent().GetNode<LineEdit>("GridContainer/WorkingDirText").Text);
        //    _on_tools_dir_text_text_changed(GetParent().GetNode<LineEdit>("GridContainer/ToolsDirText").Text);
        //    _on_build_dir_text_text_changed(GetParent().GetNode<LineEdit>("GridContainer/BuildDirText").Text);
        //    ReDraw();
        //}

        private void ReDraw()
        {
            Clear();

            if (workingValid)
                PushColor(Colors.Green);
            else
                PushColor(Colors.Red);
            AddText($"Working dir is {(workingValid ? "valid" : "invalid")}");
            Newline();

            if (buildValid)
                PushColor(Colors.Green);
            else
                PushColor(Colors.Red);
            AddText($"Build dir is {(buildValid ? "valid" : "invalid")}");
            Newline();

            if (toolsValid)
                PushColor(Colors.Green);
            else
                PushColor(Colors.Red);
            AddText($"Tools dir is {(toolsValid ? "valid" : "invalid")}");
            Newline();

            var optionsDialog = GetParent().GetParent<ConfirmationDialog>();
            var okButton = optionsDialog.GetOkButton();
            okButton.Disabled = !(workingValid && buildValid && toolsValid);
        }

        public void _on_working_dir_text_text_changed(string text)
        {
            workingValid = IsValid(text);
            ReDraw();
        }

        public void _on_tools_dir_text_text_changed(string text)
        {
            toolsValid = IsValid(text);
            if (toolsValid)
                toolsValid = File.Exists(Path.Combine(text, "ArchiveTool.exe"));
            ReDraw();
        }

        public void _on_build_dir_text_text_changed(string text)
        {
            buildValid = IsValid(text);
            ReDraw();
        }

        public void _on_addi_dir_text_text_changed(string text)
        {

        }
    }

}