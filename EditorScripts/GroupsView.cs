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

        public override void _Ready()
        {
            dbrTemplate = editorWindow.DBRFile.TemplateRoot;

            ItemSelected += OnItemSelected;

            Init();
        }

        private void OnItemSelected()
        {
            EmitSignal(nameof(GroupSelected));
        }

        public (GroupBlock, DBRFile) GetSelectedAndFile()
        {
            return (GetSelectedGroup(), editorWindow.DBRFile);
        }

        private GroupBlock GetSelectedGroup()
        {
            return (GetSelected().GetMeta("group_block").AsGodotObject() as GroupBlockHolder).Block;
        }

        private void Init()
        {
            Clear();

            root = CreateItem();
            root.SetText(0, "All Groups");

            foreach (var group in dbrTemplate.GetGroups())
            {
                AddGroup(root, group);
            }
        }

        private void AddGroup(TreeItem parentGroup, GroupBlock group)
        {
            var newGroup = CreateItem(parentGroup);
            newGroup.Collapsed = true;
            newGroup.SetText(0, group.Name);
            newGroup.SetMeta("group_block", Variant.CreateFrom(new GroupBlockHolder(group)));

            if (group.GetGroups().Count > 0)
                foreach (var subGroup in group.GetGroups())
                    AddGroup(newGroup, subGroup);
        }

        private partial class GroupBlockHolder : Godot.Object
        {
            public GroupBlock Block { get; private set; }

            public GroupBlockHolder(GroupBlock block)
            {
                Block = block;
            }
        }
    }
}