using Godot;
using System;
using TQDBEditor;
using TQDBEditor.EditorScripts;

namespace TQDBEditor.GenericEditor
{
    public partial class GroupsVariables : HSplitContainer
    {
        [Export]
        private EditorWindow editorWindow;

        private GroupsView groupsView;
        private VariablesView variablesView;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            groupsView = GetNode<GroupsView>("Groups");
            variablesView = GetNode<VariablesView>("Variables");

            editorWindow.FocusOnEntry += OnFocusOnEntry;
        }

        public void OnFocusOnEntry()
        {
            groupsView.SelectEntry();
            variablesView.CallDeferred(nameof(variablesView.SelectEntry));
        }
    }
}
