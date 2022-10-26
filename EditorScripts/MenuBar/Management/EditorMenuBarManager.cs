using Godot;
using System;

namespace TQDBEditor.EditorScripts
{
    public partial class EditorMenuBarManager : MenuBar
    {
        private EditorWindow editorWindow;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            editorWindow = GetParent().GetParent<EditorWindow>();

            InitEditManagement();
            InitFileManagement();
        }
    }
}