using Godot;
using System.Collections.Generic;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions
{
    public interface IHasRange
    {
        int MaxRange { get; set; }

        int MinRange { get; set; }

        Color RangeColor { get; }

        HashSet<Vector2I> CellsInRange { get; }

        HashSet<Vector2I> ValidCellsInRange { get; }
    }
}
