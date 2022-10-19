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

        protected virtual void OnClose() { }

        public void OnCloseEditor()
        {
            OnClose();
            CallDeferred("queue_free");
        }
    }
}
