using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowfront.Backend.Utilities
{
    public static class PathfindingUtils
    {

        //public static HashSet<Vector2I> ComputeCellsInRange(Vector2I origin, int minRange, int maxRange, CellRules rules)
        //{
        //    if (minRange > maxRange)
        //        return [];

        //    if (maxRange <= 0)
        //        return [];

        //    if (rules == CellRules.None)
        //        return [];

        //    var requiresPath = rules.HasFlag(CellRules.RequiresTraversablePath);

        //    var cellsInRange = requiresPath
        //        ? HexTileMapUtils.GetActualCellsWithinRange([], origin, maxRange)
        //        : HexTileMapUtils.GetHypotheticalCellsWithinRange(origin, maxRange);

        //    if (!rules.HasFlag(CellRules.IncludeOwnTile))
        //        cellsInRange = cellsInRange.Where(f => f != origin);

        //    if (!rules.HasFlag(CellRules.IncludeOwnTile))
        //        cellsInRange = cellsInRange.Where(f => f != origin);

        //    return HexTileMapUtils
        //        .GetActualCellsWithinRange(_availableCells, Position, MaxRange)
        //        .Where(f => f != Position)
        //        .ToHashSet();
        //}
    }
}
