using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor.EditorScripts
{
    public partial class GenericEditorWindow : EditorWindow
    {
        public override void _Ready()
        {
            base._Ready();

            if (menuBar is EditorMenuBarManager menuBarManager)
            {
                menuBarManager.EditEntry += () =>
                {
                    var focussed = GuiGetFocusOwner();
                    if (focussed == null || !focussed.HasMethod("activate"))
                        return;

                    focussed.Call("activate");
                };
                menuBarManager.Find += ShowFind;
                menuBarManager.Undo += Undo;
                menuBarManager.Redo += Redo;
                menuBarManager.Saved += OnFileSaved;
                menuBarManager.ToggleStatusbar += () => statusBar.Visible = false;
            }
        }

        protected override void OnClose()
        {
        }
    }
}
