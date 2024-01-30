using Shadowfront.Backend.Board;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

namespace Shadowfront.Backend.Utilities
{
    public static class HexTileMapUtils
    {
        [Flags]
        public enum PathfindingRules
        {
            None = 0,
            RequiresTraversablePath = 1,
            IncludeOwnTile = 2,
            IncludeGroundTiles = 4,
            IncludeOtherTiles = 8,
            IncludeOwnTeamTiles = 16,
            IncludeOtherTeamTiles = 32,
        }

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
        public static IEnumerable<Vector2I> GetHypotheticalCellsWithinRange(Vector2I originCell, int minRange, int maxRange)
        {
            // If we have no range, return nothing.
            if(maxRange < 0)
                return Enumerable.Empty<Vector2I>();

            // If we have misconfigured ranges, return nothing.
            if (minRange > maxRange)
                return Enumerable.Empty<Vector2I>();

            // If we have zero range, return the origin.
            if (maxRange == 0)
                return [originCell];

            // We have a non-zero range, so get all the possible
            // cells between the origin and the outer ring, inclusive.
            var outerRing = GetHypotheticalCellsWithinRangeIterator(originCell, maxRange);

            // If we have no min range, return the whole outer range.
            if (minRange <= 0)
                return outerRing;

            // If the min range is 1, just exclude the origin cell.
            if (minRange == 1)
                return outerRing.Where(f => f != originCell);

            // Otherwise, get the cells that fall within the min range, exclusive.
            var innerRing = GetHypotheticalCellsWithinRangeIterator(originCell, minRange - 1);

            // And substract them from the outer range.
            //
            // eg. min=3, max=3 should produce a ring of cells at
            // distance 3 from the origin.
            return outerRing.Except(innerRing);
        }

        /// <summary>
        /// Returns the positions of all neighbors that might exist withing the range.
        /// </summary>
        private static IEnumerable<Vector2I> GetHypotheticalCellsWithinRangeIterator(Vector2I originCell, int range)
        {
            if (range < 0)
                yield break;

            if (range == 0)
            {
                yield return originCell;
                yield break;
            }

            var diagonalYRange = (int)Math.Floor(range / 2d);
            var diagonalYRangeOffset = (int)Math.Ceiling(range / 2d);

            var currentCellIsOffset = Math.Abs(originCell.X) % 2 > 0;
            var southDiagonalYOffset = currentCellIsOffset ? diagonalYRange : diagonalYRangeOffset;
            var northDiagonalYOffset = currentCellIsOffset ? diagonalYRangeOffset : diagonalYRange;

            var leftX = originCell.X - range;
            var rightX = originCell.X + range;
            var topY = originCell.Y - northDiagonalYOffset;
            var bottomY = originCell.Y + southDiagonalYOffset;

            if (currentCellIsOffset)
                topY++;
            else
                bottomY--;

            var outerYTop = topY;
            var outerYBottom = bottomY;

            // Scan each column from left to right.
            for (var x = leftX; x <= rightX; x++)
            {
                var mod = x % 2;

                // If we're on the left half of the hex...
                if (x < originCell.X)
                {
                    if (mod == 0) outerYBottom++;
                    else outerYTop--;
                }
                // If we're on the right half of the hex...
                else if (x > originCell.X)
                {
                    if (mod == 0) outerYTop++;
                    else outerYBottom--;
                }
                // If we're in the center column...
                else
                {
                    if (currentCellIsOffset)
                        outerYTop--;
                    else
                        outerYBottom++;
                }

                // Fill in all the cells between our top and bottom bounds.
                for(var i = outerYTop; i <= outerYBottom; i++)
                {
                    yield return new(x, i);
                }
            }
        }

        /// <summary>
        /// Returns the positions of all neighbors that actually exist withing the range.
        /// </summary>
        public static IEnumerable<Vector2I> GetActualCellsWithinRange(IEnumerable<Vector2I> availableCells, Vector2I originCell, int minRange, int maxRange)
        {
            var hypotheticalCells = GetHypotheticalCellsWithinRange(originCell, minRange, maxRange);

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
