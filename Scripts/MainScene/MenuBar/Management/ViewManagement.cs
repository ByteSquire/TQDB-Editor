using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
    {
        [Signal]
        public delegate void RefreshClickedEventHandler();
        [Signal]
        public delegate void ToggleStatusBarEventHandler();

        public void _on_view_refresh()
        {
            GD.Print("View -> Refresh");
            EmitSignal(nameof(RefreshClicked));
        }

        public void _on_view_toggle_status_bar()
        {
            GD.Print("View -> Status bar");
            EmitSignal(nameof(ToggleStatusBar));
        }
    }
}