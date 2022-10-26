using Godot;
using System;

namespace TQDBEditor.EditorScripts
{
    public partial class EditorMenuBarManager : MenuBar
    {
        [Export]
        private ConfirmationDialog findDialog;

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
            findDialog.Confirmed += OnFindConfirmed;
        }

        public void _on_edit_edit_entry()
        {
            GD.Print("Edit -> Edit entry");

            EmitSignal(nameof(EditEntry));
        }

        public void _on_edit_find()
        {
            GD.Print("Edit -> Find");

            findDialog.PopupCentered(new Vector2i(300, 100));
        }

        private string searchString;

        private void OnFindConfirmed()
        {
            searchString = findDialog.GetNode<TextEdit>("SearchText").Text;
            findDialog.GetNode<TextEdit>("SearchText").Text = string.Empty;

            EmitSignal(nameof(Find));
        }

        public string GetFindString() => searchString;

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