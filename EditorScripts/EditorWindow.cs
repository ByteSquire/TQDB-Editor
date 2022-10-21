using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDB_Parser.DBR;

namespace TQDBEditor.EditorScripts
{
    public partial class EditorWindow : Window
    {
        [Export]
        private Label footBarPathLabel;

        [Signal]
        public delegate void ReinitEventHandler();

        public DBRFile DBRFile { get; set; }

        private UndoRedo undoRedo;

        public override void _Ready()
        {
            undoRedo = new();
            CloseRequested += OnCloseEditor;
            Title = Path.GetFileName(DBRFile.FileName);
            footBarPathLabel.Text += Title;
        }

        public Variant UndoRedoProp
        {
            get
            {
                // Should never be called
                GD.Print("Getting");
                return Variant.CreateFrom(0);
            }
            set
            {
                GD.Print("Doing: " + value);
                var input = (string[])value;
                (var key, var v) = (input[0], input[1]);

                DBRFile[key].UpdateValue(v);
                EmitSignal(nameof(Reinit));
            }
        }

        public void Do(string key, string value)
        {
            undoRedo.CreateAction("Write value");

            undoRedo.AddDoProperty(this, nameof(UndoRedoProp),
                Variant.CreateFrom(new string[] { key, value }));
            undoRedo.AddUndoProperty(this, nameof(UndoRedoProp),
                Variant.CreateFrom(new string[] { key, DBRFile[key].Value }));

            undoRedo.CommitAction();
        }

        public void Undo() => undoRedo.Undo();

        public void Redo() => undoRedo.Redo();

        protected virtual void OnClose() { }

        public void OnCloseEditor()
        {
            OnClose();
            CallDeferred("queue_free");
        }
    }
}
