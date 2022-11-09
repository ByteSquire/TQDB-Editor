using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TQDBEditor
{
    public partial class Table : ScrollContainer
    {
        [Export]
        private Godot.Collections.Array<string> columns;

        [Export]
        private PackedScene header_column;
        [Export]
        private PackedScene column_button;
        [Export]
        private PackedScene column;
        [Export]
        private int CellHeight
        {
            get => cellHeight;
            set
            {
                if (cellHeight != value)
                {
                    cellHeight = value;
                    OnChangedCellSize();
                }
            }
        }
        private int cellHeight;
        [Export]
        private int SeparatorHeight
        {
            get => separatorHeight;
            set
            {
                if (separatorHeight != value)
                {
                    separatorHeight = value;
                    OnChangedSeparatorHeight();
                }
            }
        }
        private int separatorHeight;

        private Godot.Collections.Array<Container> _columns;

        private Control _content;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _content = GetChild<Control>(0);
            _columns = new();

            GetViewport().GuiFocusChanged += OnGuiFocusChanged;

            if (columns is not null && columns.Count > 0)
            {
                var node = _content;

                var node1 = node.GetChild(0);

                for (int i = 0; i < columns.Count; i++)
                {
                    var header = header_column.Instantiate<Control>();
                    (header.GetChild(0) as Button).Text = columns[i];

                    header.GetChild(0).Name = "Column" + (i + 1);

                    var _column = column.Instantiate();

                    _column.GetChild(0).Set("cell_height", cellHeight);
                    _column.GetChild(0).Set("separator_height", separatorHeight);
                    _column.GetChild(0).Name = "Column" + (i + 1);

                    header.Set("other", _column);
                    _column.Set("other", header);

                    node.AddChild(header);
                    node = header;

                    node1.AddChild(_column);
                    node1 = _column;

                    _columns.Add(_column.GetChild<Container>(0));
                }
                node.AddChild(new Control());
                node1.AddChild(new Control());
                (node as HSplitContainer).Dragged += (x) =>
                {
                    _content.CustomMinimumSize += new Vector2i((int)x - lastDrag, 0);
                    lastDrag = (int)x;
                };

                _content.MoveChild(_content.GetChild(0), 1);

                _content.CustomMinimumSize = new Vector2i((int)_content.GetMinimumSize().x + 77, 0);
            }
        }

        private void OnGuiFocusChanged(Control node)
        {
            if (!node.HasMeta("is_table_cell"))
            {
                foreach (var selectedRowIndex in GetFocussedRows())
                    UnFocusRow(selectedRowIndex);
                return;
            }
            var index = GetCellPosition(node).y;
            if (index < 0)
                return;

            if (Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Shift))
            {
                if (Input.IsKeyPressed(Key.Shift))
                {
                    var firstFocussed = GetFocussedRows()[0];
                    var lastFocussed = GetFocussedRows()[^1];
                    if (index < firstFocussed || index > lastFocussed)
                    {
                        bool up = index < firstFocussed;
                        var focussed = up ? firstFocussed : lastFocussed;

                        var next = up ? focussed - 1 : focussed + 1;
                        for (int i = next; i != index && i != focussed;)
                        {
                            FocusRow(i);
                            if (up)
                                i--;
                            else
                                i++;
                        }
                    }
                }
            }
            else
                foreach (var selectedRowIndex in GetFocussedRows())
                    UnFocusRow(selectedRowIndex);

            FocusRow(index);
            GetViewport().GuiReleaseFocus();
        }

        public IReadOnlyList<int> GetFocussedRows()
        {
            if (_columns is null || _columns.Count < 1)
                return new List<int>();

            var ret = _columns[0].GetChildren()
                .Where(x => x.HasMeta("is_focussed"))
                .Select(x => GetCellPosition(x as Control).y)
                .ToList();
            ret.Sort();
            return ret;
        }

        int lastDrag = 0;

        public int AddRow(Godot.Collections.Array<Control> values)
        {
            if (values.Count != columns.Count)
            {
                GD.PrintErr("You must pass an array that contains a value for every column!");
                return -1;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (_columns[i].GetChildCount() > 0)
                    _columns[i].AddChild(new HSeparator());
                values[i].SetMeta("is_table_cell", true);
                _columns[i].AddChild(values[i]);
            }

            return ConvertToRowIndex(_columns[0].GetChildCount() - 1);
        }

        public int InsertRow(int index, Godot.Collections.Array<Control> values)
        {
            if (values.Count != columns.Count)
            {
                GD.PrintErr("You must pass an array that contains a value for every column!");
                return -1;
            }
            var myIndex = ConvertToChildIndex(index);
            // +1 because the separators count as children too, so we can insert up to count - 2
            if (index < 0 || myIndex > _columns[0].GetChildCount() + 1)
            {
                GD.PrintErr("You must pass a non-negative index that's in range");
                return -1;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (myIndex > 0)
                {
                    var sep = new HSeparator();
                    _columns[i].AddChild(sep);
                    _columns[i].MoveChild(sep, myIndex - 1);
                }
                values[i].SetMeta("is_table_cell", true);
                _columns[i].AddChild(values[i]);
                _columns[i].MoveChild(values[i], myIndex);
            }

            return myIndex;
        }

        public void RemoveRow(int index)
        {
            if (_columns is null)
                return;
            if (index < 0)
                return;

            var myIndex = ConvertToChildIndex(index);
            foreach (var column in _columns)
            {
                var maxChildIndex = column.GetChildCount() - 1;
                if (maxChildIndex > index)
                {
                    var child = column.GetChild(myIndex);
                    GD.Print(child.GetIndex());
                    GD.Print(index);
                    if (maxChildIndex > index + 1)
                    {
                        var childSep = column.GetChild(myIndex + 1);
                        column.RemoveChild(childSep);
                        childSep.QueueFree();
                    }
                    column.RemoveChild(child);
                    child.QueueFree();
                }
            }
        }

        public void SwapRows(int indexA, int indexB)
        {
            if (_columns is null)
                return;

            var rowA = GetRow(indexA);
            var rowB = GetRow(indexB);
            var myIndexA = ConvertToChildIndex(indexA);
            var myIndexB = ConvertToChildIndex(indexB);
            for (int i = 0; i < _columns.Count; i++)
            {
                var column = _columns[i];
                column.MoveChild(rowA[i], myIndexB);
                column.MoveChild(rowB[i], myIndexA);
                //rowA[i].SetMeta("table_cell_position", new Vector2i(i, indexB));
                //rowB[i].SetMeta("table_cell_position", new Vector2i(i, indexA));
            }
        }

        public Control GetCellAt(Vector2i cellPosition)
        {
            var myIndex = ConvertToChildIndex(cellPosition.y);
            return _columns[cellPosition.x].GetChild<Control>(myIndex);
        }

        public Godot.Collections.Array<Control> GetRow(int index)
        {
            var myIndex = ConvertToChildIndex(index);
            var ret = new Godot.Collections.Array<Control>();
            if (_columns[0].GetChildCount() <= myIndex)
            {
                GD.PrintErr("The passed index " + index + " is out of range for this table");
                return ret;
            }

            foreach (var column in _columns)
                ret.Add(column.GetChild<Control>(myIndex));

            return ret;
        }

        public Vector2i GetCellPosition(Control cell)
        {
            if (!cell.HasMeta("is_table_cell"))
            {
                GD.PrintErr("The passed node is not part of a table");
                return new Vector2i(-1, -1);
            }
            else
            {
                var parent = cell.GetParent<Container>();
                if (parent != null)
                {
                    var cIdx = _columns.IndexOf(parent);
                    if (cIdx >= 0)
                        return new Vector2i(cIdx, cell.GetIndex() / 2);
                }
            }

            GD.PrintErr("The passed node is not part of this table");
            return new Vector2i(-1, -1);
        }

        public IEnumerable<Control> EnumerateColumn(int index)
        {
            if (_columns is null)
                yield break;
            if (_columns.Count <= index)
                yield break;

            var numChildren = _columns[index].GetChildCount();
            for (int i = 0; i < numChildren; i += 2)
            {
                yield return _columns[index].GetChild<Control>(i);
            }
        }

        public void FocusRow(int index)
        {
            if (_columns is null)
                return;
            var myIndex = ConvertToChildIndex(index);
            if (myIndex > _columns[0].GetChildCount() - 1)
                return;
            _columns[0].GetChild<Control>(myIndex).GrabFocus();

            foreach (var column in _columns)
            {
                var cChild = column.GetChild<Control>(myIndex);
                cChild.AddThemeStyleboxOverride("normal", new StyleBoxFlat() { BgColor = Colors.Blue });
                cChild.SetMeta("is_focussed", true);
            }
        }

        public void UnFocusRow(int index)
        {
            if (_columns is null)
                return;

            var myIndex = ConvertToChildIndex(index);
            foreach (var column in _columns)
            {
                var cChild = column.GetChild<Control>(myIndex);
                cChild.RemoveThemeStyleboxOverride("normal");
                cChild.RemoveMeta("is_focussed");
            }
        }

        public void Clear()
        {
            if (_columns is null)
                return;
            foreach (var column in _columns)
                ClearContainer(column);
        }

        private static int ConvertToChildIndex(int index) => index * 2;
        private static int ConvertToRowIndex(int childIndex) => childIndex / 2;

        private static void ClearContainer(Container container)
        {
            foreach (var child in container.GetChildren())
            {
                container.RemoveChild(child);
                child.QueueFree();
            }
        }

        private void OnChangedCellSize()
        {
            if (_columns is null)
                return;
            foreach (var column in _columns)
            {
                column.Set("cell_height", cellHeight);
            }
        }

        private void OnChangedSeparatorHeight()
        {
            if (_columns is null)
                return;
            foreach (var column in _columns)
            {
                column.Set("separator_height", separatorHeight);
            }
        }
    }
}