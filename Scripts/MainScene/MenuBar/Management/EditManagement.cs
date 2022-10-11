using Godot;
using System;

namespace TQDBEditor
{
    public partial class MenuBarMananger : MenuBar
    {
        [Signal]
        public delegate void EditCopyClickedEventHandler();
        [Signal]
        public delegate void EditDeleteClickedEventHandler();
        [Signal]
        public delegate void EditPasteClickedEventHandler();
        [Signal]
        public delegate void EditSelectAllClickedEventHandler();
        [Signal]
        public delegate void EditUndoClickedEventHandler();


        public void _on_edit_copy()
        {
            GD.Print("Edit -> Copy");
            EmitSignal(nameof(EditCopyClicked));
        }

        public void _on_edit_delete()
        {
            GD.Print("Edit -> Delete");
            EmitSignal(nameof(EditDeleteClicked));
        }

        public void _on_edit_paste()
        {
            GD.Print("Edit -> Paste");
            EmitSignal(nameof(EditPasteClicked));
        }

        public void _on_edit_select_all()
        {
            GD.Print("Edit -> Select All");
            EmitSignal(nameof(EditSelectAllClicked));
        }

        public void _on_edit_undo()
        {
            GD.Print("Edit -> Undo");
            EmitSignal(nameof(EditUndoClicked));
        }
    }
}