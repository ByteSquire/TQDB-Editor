using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
    {
        private Config config;

        public override void _Ready()
        {
            config = this.GetEditorConfig();

            InitModManagement();
            InitFileManagement();
        }
    }
}