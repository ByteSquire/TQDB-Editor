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

        public DBRFile DBRFile { get; set; }

        public override void _Ready()
        {
            CloseRequested += OnCloseEditor;
            Title = Path.GetFileName(DBRFile.FileName);
            footBarPathLabel.Text += Title;
        }

        protected Stack<(string key, string value)> undo_stack;
        protected Stack<(string key, string value)> redo_stack;

        protected int changed;

        public void Do(string key, string value)
        {
            var entry = DBRFile[key];

            undo_stack ??= new();
            undo_stack.Push((key, entry.Value));
            changed++;

            entry.UpdateValue(value);
        }

        public void Undo()
        {
            var (key, value) = undo_stack.Pop();
            var entry = DBRFile[key];

            redo_stack ??= new();
            redo_stack.Push((key, entry.Value));
            changed--;

            DBRFile[key].UpdateValue(value);
        }

        public void Redo()
        {
            var (key, value) = redo_stack.Pop();
            Do(key, value);
        }

        protected virtual void OnClose() { }

        public void OnCloseEditor()
        {
            OnClose();
            CallDeferred("queue_free");
        }
    }
}
