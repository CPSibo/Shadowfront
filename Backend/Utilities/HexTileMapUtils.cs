using Shadowfront.Backend.Board;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

namespace Shadowfront.Backend.Utilities
{
    public static class HexTileMapUtils
    {
        /// <summary>
        /// Calculates how many neighbors a given cell might have within the given range.
        /// </summary>
        public static int GetMaximumNumberOfCellsInRange(int range)
        {
            if (range <= 0)
                return 0;

            // Equation to calculate the maximum number of cells
            // within r steps of a given cell. This is useful for
            // pre-allocating our neighbor list collection.
            //
            //         r * (r + 1)
            // n = 6 * -----------
            //              2
            return 6 * ((range * (range + 1)) / 2);
        }

        /// <summary>
        /// Returns the positions of all neighbors that might exist withing the range.
        /// </summary>
        public static IEnumerable<Vector2I> GetHypotheticalCellsWithinRange(Vector2I originCell, int range)
        {
            if (range <= 0)
                return Enumerable.Empty<Vector2I>();

            return GetHypotheticalCellsWithinRangeIterator(originCell, range);
        }

        /// <summary>
        /// Returns the positions of all neighbors that might exist withing the range.
        /// </summary>
        private static IEnumerable<Vector2I> GetHypotheticalCellsWithinRangeIterator(Vector2I originCell, int range)
        {
            var leftEdge = originCell.X - range;
            var rightEdge = originCell.X + range;
            var topEdge = originCell.Y - range;
            var bottomEdge = originCell.Y + range;

            var currentCellIsOffset = Math.Abs(originCell.X) % 2 > 0;

            for (var x = leftEdge; x <= rightEdge; x++)
            {
                var xIsOffset = Math.Abs(x) % 2 > 0;

                for (var y = topEdge; y <= bottomEdge; y++)
                {
                    if (x == originCell.X && y == originCell.Y)
                        continue;

                    // Circle the square.
                    if ((x == leftEdge || x == rightEdge) && (y == topEdge || y == bottomEdge) && xIsOffset == currentCellIsOffset)
                        continue;

                    // Get rid of farthest cells in the alternate rows.
                    if (currentCellIsOffset && !xIsOffset && y == topEdge)
                        continue;

                    if (!currentCellIsOffset && xIsOffset && y == bottomEdge)
                        continue;

                    yield return new(x, y);
                }
            }
        }

        /// <summary>
        /// Returns the positions of all neighbors that actually exist withing the range.
        /// </summary>
        public static IEnumerable<Vector2I> GetActualCellsWithinRange(IEnumerable<Vector2I> availableCells, Vector2I originCell, int range)
        {
            var hypotheticalCells = GetHypotheticalCellsWithinRange(originCell, range);

            var lookup = availableCells.ToHashSet();

            foreach(var cell in hypotheticalCells)
            {
                if (!lookup.Contains(cell))
                    continue;

                yield return cell;
            }
        }
    }
}
