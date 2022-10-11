using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_build_build()
        {
            GD.Print("Build -> Build");
        }

        public void _on_build_stop()
        {
            GD.Print("Build -> Stop");
        }

        public void _on_build_verify_build()
        {
            GD.Print("Build -> Verify build");
        }

        public void _on_build_set_verify_option(int index)
        {
            GD.Print("Build -> Set verify build option " + index);
        }
    }
}