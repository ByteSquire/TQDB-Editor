using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        public void _on_view_refresh()
        {
            GD.Print("View -> Refresh");
        }

        public void _on_view_toggle_status_bar()
        {
            GD.Print("View -> Status bar");
        }
    }
}