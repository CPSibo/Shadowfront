using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CellOutliner : Node2D
{
    public class CellOutlineData
    {
        public Vector2 Position { get; set; }

        public Color Color { get; set; }

        public Vector2[] Points { get; set; } = [];
    }

    private const float SQUARE_ROOT_THREE = 1.73205080756887729352744f;

    [Export]
    public float TileSideLength = 50.83f;

    private float _halfHexagonSideLength = 0f;

    private float _squareRootThreeTimesHalfSideLength = 0f;

    private Dictionary<string, List<CellOutlineData>> _cellsList = [];

    public CellOutliner()
    {
        _halfHexagonSideLength = TileSideLength / 2f;
        _squareRootThreeTimesHalfSideLength = SQUARE_ROOT_THREE * _halfHexagonSideLength;
    }

    public void ClearAllCells()
    {
        _cellsList.Clear();

        QueueRedraw();
    }

    public override void _Draw()
    {
        var allCellData = _cellsList.SelectMany(f => f.Value).ToList();

        foreach (var cell in allCellData)
        {
            DrawColoredPolygon(cell.Points, cell.Color);
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
            a,
            b,
            c,
            d,
            e,
            f
        ];
    }

    public void AddCellStyle(string source, Vector2 position, Color color)
    {
        QueueRedraw();

        if (!_cellsList.TryGetValue(source, out var cellData))
        {
            cellData = [];
            _cellsList.Add(source, cellData);
        }

        cellData.Add(new()
        {
            Position = position,
            Color = color,
            Points = GetHexagonPoints(position)
        });
    }

    public void AddCellStyle(string source, IEnumerable<Vector2> positions, Color color)
    {
        foreach(var position in positions)
        {
            AddCellStyle(source, position, color);
        }
    }

    public void RemoveSource(string source)
    {
        _cellsList.Remove(source);

        QueueRedraw();
    }
}
