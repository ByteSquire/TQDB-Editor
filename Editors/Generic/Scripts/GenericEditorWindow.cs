using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TQDBEditor.GenericEditor;

namespace TQDBEditor.EditorScripts
{
    public partial class GenericEditorWindow : EditorWindow
    {
        [Export]
        private VariablesView variablesView;

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
                menuBarManager.Copy += Copy;
                menuBarManager.Paste += Paste;
                menuBarManager.Saved += OnFileSaved;
                menuBarManager.ToggleStatusbar += () => statusBar.Visible = false;
            }
        }

        protected override List<string> GetSelectedVariables()
        {
            return variablesView.GetSelectedVariables();
        }

        protected override bool TryGetNextVariable(string currentVar, out string varName)
        {
            return variablesView.TryGetNextVariable(currentVar, out varName);
        }

        protected override void OnClose()
        {
        }
    }
}
