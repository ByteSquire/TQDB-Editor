using Godot;
using System;
using TQDB_Parser.DBR;
using TQDBEditor.Common;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.BasicEditor
{
    public partial class FileVariable : VariableControl
    {
        [Export]
        private Button button;

        private FileDialog fileDialog;

        public override string GetChangedValue()
        {
            return button.Text;
        }

        protected override void InitVariable(DBREntry entry)
        {
            fileDialog = new FileDialog()
            {
                CurrentFile = button.Text,
                FileMode = FileDialog.FileModeEnum.OpenFile
            };

            fileDialog.FileSelected += (filePath) =>
            {
                button.Text = filePath;
                OnConfirmed();
            };
            AddChild(fileDialog);

            if (!entry.IsValid())
                button.AddThemeColorOverride("font_color", Colors.Red);

            button.Text = entry.Value;
            button.Pressed += OnButtonPressed;
        }

        private void OnButtonPressed()
        {
            fileDialog.PopupCenteredRatio(.3f);
        }
    }
}
