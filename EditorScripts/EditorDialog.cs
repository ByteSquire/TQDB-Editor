using Godot;
using System;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;

namespace TQDBEditor
{
    public abstract partial class EditorDialog : ConfirmationDialog
    {
        public DBRFile DBRFile { get; set; }

        public string VarName { get; set; }

        protected DBREntry entry;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Title = DBRFile.FileName + ':' + VarName;
            OkButtonText = "Apply";

            Confirmed += OnConfirmed;

            entry = DBRFile[VarName];
            InitVariable(entry);

            PopupCenteredRatio(.4f);
        }

        protected abstract void InitVariable(DBREntry entry);

        protected abstract string GetChangedValue();

        private void OnConfirmed()
        {
            entry.UpdateValue(GetChangedValue());
        }
    }
}