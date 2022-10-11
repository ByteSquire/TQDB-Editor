using Godot;
using System;
using System.IO;

namespace TQDBEditor
{
    public partial class ModSelectMenu : PopupMenu
    {
        [Signal]
        public delegate void selectEventHandler(string mod_name);

        public void _on_select_index_pressed(int index)
        {
            for (int i = 0; i < ItemCount; i++)
                SetItemChecked(i, index == i);

            EmitSignal(nameof(select), GetItemText(index));
        }

        public void _on_about_to_popup()
        {
            var config = this.GetEditorConfig();
            var mods = Directory.EnumerateDirectories(config.ModsDir);

            Clear();
            int i = 0;
            foreach (var mod in mods)
            {
                var modName = Path.GetFileName(mod);
                AddCheckItem(modName);
                SetItemChecked(i++, modName == config.ModName);
            }
        }

    }
}