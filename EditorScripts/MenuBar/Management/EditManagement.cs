using Godot;
using System;

namespace TQDBEditor.EditorScripts
{
    public partial class EditorMenuBarManager : MenuBar
    {
        [Signal]
        public delegate void EditEntryEventHandler();
        [Signal]
        public delegate void FindEventHandler();
        [Signal]
        public delegate void UndoEventHandler();
        [Signal]
        public delegate void RedoEventHandler();

        private void InitEditManagement()
        {
        }

        public void _on_edit_edit_entry()
        {
            GD.Print("Edit -> Edit entry");

            EmitSignal(nameof(EditEntry));
        }

        public void _on_edit_find()
        {
            GD.Print("Edit -> Find");

            EmitSignal(nameof(Find));
        }

        public void _on_edit_undo()
        {
            GD.Print("Edit -> Undo");

            EmitSignal(nameof(Undo));
        }

        public void _on_edit_redo()
        {
            GD.Print("Edit -> Redo");

            EmitSignal(nameof(Redo));
        }

        public void _on_edit_set_default_width()
        {
            GD.Print("Edit -> Set default width");

        }
    }
}