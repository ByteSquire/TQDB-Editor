using Godot;
using System;
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
    private Vector2i CellSize
    {
        get => cellSize;
        set
        {
            if (cellSize != value)
            {
                cellSize = value;
                OnChangedCellSize();
            }
        }
    }
    private Vector2i cellSize;

    private Godot.Collections.Array<VBoxContainer> _columns;

    private Control _content;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _content = GetChild<Control>(0);
        _columns = new();

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

                _column.GetChild(0).Name = "Column" + (i + 1);


                header.Set("other", _column);

                _column.Set("other", header);

                node.AddChild(header);

                node = header;

                node1.AddChild(_column);

                node1 = _column;

                _columns.Add(_column.GetChild<VBoxContainer>(0));
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

    int lastDrag = 0;

    public int AddRow(Godot.Collections.Array<Control> values)
    {

        if (values.Count != columns.Count)
        {
            GD.PrintErr("You must pass an array that contains a value for every column!");
            return -1;
        }
        GD.Print("adding children: " + values);

        for (int i = 0; i < values.Count; i++)
        {
            values[i].CustomMinimumSize = cellSize;
            values[i].SetMeta("table_cell_position", new Vector2i(i, _columns[i].GetChildCount()));
            _columns[i].AddChild(values[i]);
            _columns[i].AddChild(new HSeparator());
        }

        return _columns[0].GetChildCount();
    }

    public Control GetCellAt(Vector2i cellPosition)
    {
        return _columns[cellPosition.x].GetChild<Control>(cellPosition.y);
    }

    public Godot.Collections.Array<Control> GetRow(int index)
    {
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
            var parent = cell.Owner;
            foreach (var column in _columns)
                if (column == parent)
                    return cell.GetMeta("table_cell_position").AsVector2i();
        }

        GD.PrintErr("The passed node is not part of this table");
        return new Vector2i(-1, -1);
    }

    private void OnChangedCellSize()
    {
        if (_columns is null)
            return;
        foreach (var column in _columns)
        {
            foreach (var child in column.GetChildren())
                if (child is Control controlChild)
                    controlChild.CustomMinimumSize = cellSize;
        }
    }
}
