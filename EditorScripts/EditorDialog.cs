using Godot;
using System;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.EditorScripts;

namespace TQDBEditor
{
    public abstract partial class EditorDialog : ConfirmationDialog
    {
        public DBRFile DBRFile { get; set; }

        public string VarName { get; set; }

        protected DBREntry entry;

        protected EditorWindow parent;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Title = DBRFile.FileName + ':' + VarName;
            OkButtonText = "Apply";

            Confirmed += OnConfirmed;

            entry = DBRFile[VarName];
            parent = GetParent<EditorWindow>();

            InitVariable(entry);

            PopupCenteredRatio(.4f);
        }

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent)
                if (keyEvent.Pressed && keyEvent.Keycode == Key.Enter)
                {
                    EmitSignal(AcceptDialog.SignalName.Confirmed);
                    SetInputAsHandled();
                }
        }

        protected abstract void InitVariable(DBREntry entry);

        protected abstract string GetChangedValue();

        private void OnConfirmed()
        {
            parent.Do(VarName, GetChangedValue());
            CallDeferred("queue_free");
        }
    }
}