using Godot;
using System;
using System.Collections.Generic;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.EditorScripts;

//namespace TQDBEditor.GenericEditor
//{
    public partial class GroupsView : Tree
    {
        [Export]
        private EditorWindow editorWindow;

        [Signal]
        public delegate void GroupSelectedEventHandler();

        private GroupBlock dbrTemplate;

        private TreeItem root;

        private Dictionary<TreeItem, GroupBlock> groups;

        public override void _Ready()
        {
            dbrTemplate = editorWindow.DBRFile.TemplateRoot;
            groups = new(dbrTemplate.GetGroups(true).Count);

            ItemSelected += OnItemSelected;

            Init();
        }

        private void OnItemSelected()
        {
            EmitSignal(nameof(GroupSelected));
        }

        public GroupBlock GetSelectedGroup()
        {
            return groups[GetSelected()];
        }

        public void SelectEntry()
        {
            var entry = editorWindow.GetFocussedEntry();
            foreach (var pair in groups)
            {
                if (pair.Value.IsChild(entry.Template, true))
                    pair.Key.UncollapseTree();
                if (pair.Value.IsChild(entry.Template))
                {
                    pair.Key.Select(0);
                    ScrollToItem(pair.Key, true);
                    EnsureCursorIsVisible();
                }
            }
        }

        private void Init()
        {
            Clear();

            root = CreateItem();
            root.SetText(0, "All Groups");
            groups.Add(root, dbrTemplate);

            foreach (var group in dbrTemplate.GetGroups())
            {
                AddGroup(root, group);
            }
        }

        private void AddGroup(TreeItem parentGroup, GroupBlock group)
        {
            //GD.Print("Adding group: " + group.Name + " from: " + group.FileName);
            var newGroup = CreateItem(parentGroup);
            newGroup.Collapsed = true;
            newGroup.SetText(0, group.Name);
            groups.Add(newGroup, group);

            foreach (var subGroup in group.GetGroups())
            {
                AddGroup(newGroup, subGroup);
            }
        }
    }
//}