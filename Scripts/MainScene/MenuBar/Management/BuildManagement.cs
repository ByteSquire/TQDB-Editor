using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        [Signal]
        public delegate void BuildClickedEventHandler();
        [Signal]
        public delegate void StopBuildClickedEventHandler();
        [Signal]
        public delegate void VerifyBuildClickedEventHandler();

        public void _on_build_build()
        {
            GD.Print("Build -> Build");
            EmitSignal(nameof(BuildClicked));
        }

        public void _on_build_stop()
        {
            GD.Print("Build -> Stop");
            EmitSignal(nameof(StopBuildClicked));
        }

        public void _on_build_verify_build()
        {
            GD.Print("Build -> Verify build");
            EmitSignal(nameof(VerifyBuildClicked));
        }

        public void _on_build_set_verify_option(int index)
        {
            GD.Print("Build -> Set verify build option " + index);
        }
    }
}