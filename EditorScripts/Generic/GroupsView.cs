using Godot;
using System;
using System.Collections.Generic;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;

namespace TQDBEditor.EditorScripts
{
    public partial class GroupsView : Tree
    {
        [Export]
        private EditorWindow editorWindow;

        [Signal]
        public delegate void GroupSelectedEventHandler();

        private GroupBlock dbrTemplate;

        private TreeItem root;

        private List<GroupBlock> groups;

        public override void _Ready()
        {
            dbrTemplate = editorWindow.DBRFile.TemplateRoot;
            groups = new List<GroupBlock>(dbrTemplate.GetGroups(true).Count);

            ItemSelected += OnItemSelected;

            Init();
        }

        private void OnItemSelected()
        {
            EmitSignal(nameof(GroupSelected));
        }

        public GroupBlock GetSelectedGroup()
        {
            return groups[GetSelected().GetMeta("group_index").AsInt32()];
        }

        private void Init()
        {
            Clear();

            root = CreateItem();
            root.SetText(0, "All Groups");
            groups.Add(dbrTemplate);
            root.SetMeta("group_index", Variant.CreateFrom(groups.Count - 1));

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
            groups.Add(group);
            newGroup.SetMeta("group_index", Variant.CreateFrom(groups.Count - 1));

            foreach (var subGroup in group.GetGroups())
            {
                AddGroup(newGroup, subGroup);
            }
        }
    }
}