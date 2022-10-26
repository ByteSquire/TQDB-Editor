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

    private Godot.Collections.Array<Container> _columns;

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

                _column.GetChild(0).Set("cell_height", cellHeight);
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
}
