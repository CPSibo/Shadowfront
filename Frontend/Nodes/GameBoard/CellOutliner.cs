using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CellOutliner : Node2D
{
    public class CellOutlineData
    {
        public Vector2 Position { get; set; }

        public CellStyles Style { get; set; }

        public Vector2[] Points { get; set; } = [];
    }

    private const float SQUARE_ROOT_THREE = 1.73205080756887729352744f;

    [Export]
    public float TileSideLength = 50.83f;

    private float _halfHexagonSideLength = 0f;

    private float _squareRootThreeTimesHalfSideLength = 0f;

    public enum CellStyles
    {
        None,
        Hover,
        Selected,
        MovementRange,
        AttackRange,
    }

    private Dictionary<CellStyles, Color> _styleColors = new()
    {
        { CellStyles.None, Colors.Transparent },
        { CellStyles.Hover, new(0x00000055) },
        { CellStyles.Selected, new(0xffce9555) },
        { CellStyles.MovementRange, new(0x00aaef55) },
        { CellStyles.AttackRange, new(0xef555555) },
    };

    private List<CellOutlineData> _cellsList = [];

    public CellOutliner()
    {
        _halfHexagonSideLength = TileSideLength / 2f;
        _squareRootThreeTimesHalfSideLength = SQUARE_ROOT_THREE * _halfHexagonSideLength;
    }

    public void RemoveCellsWithStyle(CellStyles style)
    {
        _cellsList.RemoveAll(f => f.Style == style);

        QueueRedraw();
    }

    public void ClearAllCells()
    {
        _cellsList.Clear();

        QueueRedraw();
    }

    public void RemoveCell(Vector2 position)
    {
        SetCellStyle(position, CellStyles.None);
    }

    public void RemoveAllCellsInRange(IEnumerable<Vector2> range)
    {
        _cellsList.RemoveAll(f => range.Contains(f.Position));

        QueueRedraw();
    }

    public void SetCellStyle(Vector2 position, CellStyles style, CellStyles? onlyIfInStyle = null)
    {
        QueueRedraw();

        var allCellData = _cellsList.Where(f => f.Position == position).ToList();

        if (allCellData.Count == 0)
        {
            if (style == CellStyles.None)
                return;

            var cellData = new CellOutlineData()
            {
                Position = position,
                Style = style,
                Points = GetHexagonPoints(position),
            };

            _cellsList.Add(cellData);

            return;
        }

        foreach (var cellData in allCellData)
        {
            if (onlyIfInStyle is not null && cellData.Style != onlyIfInStyle)
                return;

            if (style == CellStyles.None)
            {
                _cellsList.Remove(cellData);

                return;
            }

            cellData.Style = style;
        }
    }

    public void SetCellStyle(IEnumerable<Vector2> positions, CellStyles style)
    {
        foreach(var position in positions)
        {
            SetCellStyle(position, style);
        }
    }

    public void RemoveCellStyle(Vector2 position, CellStyles style)
    {
        SetCellStyle(position, CellStyles.None, style);
    }

    public void RemoveCellStyle(IEnumerable<Vector2> positions, CellStyles style)
    {
        foreach (var position in positions)
        {
            RemoveCellStyle(position, style);
        }
    }

    public override void _Draw()
    {
        foreach (var cell in _cellsList)
        {
            DrawColoredPolygon(cell.Points, _styleColors[cell.Style]);
        }

        base._Draw();
    }

    private Vector2[] GetHexagonPoints(Vector2 origin)
    {
        /*
         *        C       B
         *        
         *        
         *        
         *   D                  A
         *   
         *   
         *   
         *        E       F
         */

        var a = new Vector2(TileSideLength, 0) + origin;
        var d = new Vector2(-TileSideLength, 0) + origin;

        var b = new Vector2(_halfHexagonSideLength, _squareRootThreeTimesHalfSideLength) + origin;
        var c = new Vector2(-_halfHexagonSideLength, _squareRootThreeTimesHalfSideLength) + origin;

        var f = new Vector2(_halfHexagonSideLength, -_squareRootThreeTimesHalfSideLength) + origin;
        var e = new Vector2(-_halfHexagonSideLength, -_squareRootThreeTimesHalfSideLength) + origin;

        return
        [
            a, b, c, d, e, f
        ];
    }
}
