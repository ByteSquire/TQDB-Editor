using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Table : Control
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

            _content.CustomMinimumSize = (Vector2i)_content.Size;
        }
    }

    private void OnGuiFocusChanged(Control node)
    {
        var index = GetCellPosition(node).y;
        if (index < 0)
            return;
        if (!Input.IsKeyPressed(Key.Shift))
            foreach (var selectedRowIndex in GetFocussedRows())
                UnFocusRow(selectedRowIndex);
        FocusRow(index);
    }

    public IReadOnlyList<int> GetFocussedRows()
    {
        if (_columns is null || _columns.Count < 1)
            return new List<int>();

        return _columns[0].GetChildren()
            .Where(x => x.HasMeta("is_focussed"))
            //.Where(x => (bool)x.GetMeta("is_focussed"))
            .Select(x => GetCellPosition(x as Control).y)
            .ToList();
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
            values[i].SetMeta("table_cell_position", new Vector2i(i, _columns[i].GetChildCount()));
            _columns[i].AddChild(values[i]);
        }

        return _columns[0].GetChildCount() - 1;
    }

    public void RemoveRow(int index)
    {
        if (_columns is null)
            return;

        foreach (var column in _columns)
        {
            if (column.GetChildCount() < index)
            {
                var child = column.GetChild(index);
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
        for (int i = 0; i < _columns.Count; i++)
        {
            var column = _columns[i];
            column.MoveChild(rowA[i], indexB);
            column.MoveChild(rowB[i], indexA);
            rowA[i].SetMeta("table_cell_position", new Vector2i(i, indexB));
            rowB[i].SetMeta("table_cell_position", new Vector2i(i, indexA));
        }
    }

    public Control GetCellAt(Vector2i cellPosition)
    {
        return _columns[cellPosition.x].GetChild<Control>(cellPosition.y);
    }

    public Godot.Collections.Array<Control> GetRow(int index)
    {
        if (_columns[0].GetChildCount() <= index)
        {
            GD.PrintErr("The passed index " + index + " is out of range for this table");
            return null;
        }
        var ret = new Godot.Collections.Array<Control>();

        foreach (var column in _columns)
            ret.Add(column.GetChild<Control>(index));

        return ret;
    }

    public Vector2i GetCellPosition(Control cell)
    {
        if (!cell.HasMeta("table_cell_position"))
        {
            GD.PrintErr("The passed node is not part of a table");
            return new Vector2i(-1, -1);
        }
        else
        {
            var parent = cell.GetParent<Container>();
            if (parent != null && _columns.Contains(parent))
                return cell.GetMeta("table_cell_position").AsVector2i();
        }

        GD.PrintErr("The passed node is not part of this table");
        return new Vector2i(-1, -1);
    }

    public void FocusRow(int index)
    {
        if (_columns is null)
            return;
        _columns[0].GetChild<Control>(index).GrabFocus();

        foreach (var column in _columns)
        {
            var cChild = column.GetChild<Control>(index);
            cChild.AddThemeStyleboxOverride("normal", new StyleBoxFlat() { BgColor = Colors.Blue });
            cChild.SetMeta("is_focussed", true);
        }
    }

    public void UnFocusRow(int index)
    {
        if (_columns is null)
            return;

        foreach (var column in _columns)
        {
            var cChild = column.GetChild<Control>(index);
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
