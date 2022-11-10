using Godot;
using System;
using System.Collections.Generic;

namespace TQDBEditor
{
    public partial class FileList : ItemList
    {
        [Export]
        private Button sortButton;
        public ItemList[] otherLists;
        public VScrollBar[] syncedScrollBars;

        private bool sorted = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            EmptyClicked += OnEmptyClicked;
            GetVScrollBar().ValueChanged += OnScrollChanged;
            if (sortButton is not null)
                sortButton.Pressed += OnSortPressed;
        }

        private void OnScrollChanged(double value)
        {
            if (syncedScrollBars is not null)
                foreach (var scrollBar in syncedScrollBars)
                    scrollBar.Value = value;
        }

        private void OnSortPressed()
        {
            for (int i = 0; i < ItemCount; i++)
                SetItemMetadata(i, i);

            Sort(sorted);

            var map = new int[ItemCount];
            for (int i = 0; i < ItemCount; i++)
                map[i] = GetItemMetadata(i).AsInt32();

            if (otherLists is not null)
                foreach (var list in otherLists)
                    (list as FileList)?.SortByMap(map);

            sorted = !sorted;
        }

        private void SortByMap(int[] map)
        {
            //for (var i = 0; i < map.Length; i++)
            //    GD.Print(i + "->" + map[i]);
            if (map.Length != ItemCount)
            {
                GD.PrintErr("Trying to sort lists of differing sizes!");
                return;
            }
            var oldItems = new List<string>(ItemCount);

            for (int i = 0; i < ItemCount; i++)
                oldItems.Add(GetItemText(i));

            for (int i = 0; i < ItemCount; i++)
                SetItemText(i, oldItems[map[i]]);

            sorted = false;
        }

        private void Sort(bool asc)
        {
            if (asc)
                SortItemsByTextAsc();
            else
                SortItemsByText();
        }

        private void SortItemsByTextAsc()
        {
            SortItemsByText();

            //GD.Print(sorted);
            //var changed = false;
            //for (int i = 0; i < ItemCount; i++)
            //    if (GetItemMetadata(i).AsInt32() != i)
            //    {
            //        changed = true;
            //        GD.Print("changed at: " + i);
            //    }

            //if (changed)
            for (int i = 0; i < ItemCount; i++)
                MoveItem(0, ItemCount - 1 - i);
        }

        private void OnEmptyClicked(Vector2 atPosition, long mouseButtonIndex)
        {
            DeselectAll();
        }
    }
}