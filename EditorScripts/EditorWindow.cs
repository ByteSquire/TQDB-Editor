using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDB_Parser.DBR;

namespace TQDBEditor.EditorScripts
{
    public partial class EditorWindow : Window
    {
        public DBRFile DBRFile { get; set; }

        public override void _Ready()
        {
            CloseRequested += OnCloseEditor;
        }

        protected virtual void OnClose() { }

        private void OnCloseEditor()
        {
            OnClose();
            CallDeferred("queue_free");
        }
    }
}
