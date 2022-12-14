using Godot;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace TQDBEditor
{
    public partial class MenuBarManager : MenuBar
    {
        [Export]
        private ConfirmationDialog createModDialog;
        [Export]
        private ConfirmationDialog selectNewModDialog;

        [Signal]
        public delegate void ModDeleteClickedEventHandler();
        [Signal]
        public delegate void ModNewClickedEventHandler();

        private void InitModManagement()
        {
            createModDialog.Confirmed += OnNewModSelected;
            selectNewModDialog.Confirmed += OnSelectNewMod;
        }

        public void _on_mod_delete()
        {
            GD.Print("Mod -> Delete");
        }

        public void _on_mod_new()
        {
            GD.Print("Mod -> New");

            var existingList = createModDialog.GetNode<ItemList>("Grid/ExistingMods");
            var existingMods = Directory.EnumerateDirectories(config.ModsDir, "*", SearchOption.TopDirectoryOnly);
            existingList.Clear();
            foreach (var mod in existingMods)
                existingList.AddItem(Path.GetFileName(mod), selectable: false);

            createModDialog.PopupCenteredRatio(.5f);
        }

        private void OnNewModSelected()
        {
            var lineEdit = createModDialog.GetNode<LineEdit>("Grid/NewModText");
            var modName = lineEdit.Text;
            lineEdit.Clear();

            Directory.CreateDirectory(Path.Combine(config.ModsDir, modName));

            selectNewModDialog.GetNode<Label>("ModScroll/ModName").Text = modName;

            selectNewModDialog.PopupCentered();
        }

        private void OnSelectNewMod()
        {
            _on_mod_select(selectNewModDialog.GetNode<Label>("ModScroll/ModName").Text);
        }

        public void _on_mod_select(string modName)
        {
            GD.Print("Mod -> Select " + modName);
            config.ModName = modName;
        }
    }
}