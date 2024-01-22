using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shadowfront.Backend.Board
{
    public partial class GameBoard : DisposableRefCounted
    {
        [Signal]
        public delegate void CellSelectedEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellUnselectedEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellHoveredEventHandler(Vector2I cell);

        [Signal]
        public delegate void CellUnhoveredEventHandler(Vector2I cell);

        [Signal]
        public delegate void UnitTokenSelectedEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenUnselectedEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenMovedEventHandler(UnitToken token, Vector2I from, Vector2I to);

        [Signal]
        public delegate void UnitTokenCreatedEventHandler(UnitToken token, Vector2I cell);

        [Signal]
        public delegate void UnitTokenHealthChangedEventHandler(UnitToken token, float previousHealth, float newHealth);

        [Signal]
        public delegate void UnitTokenHealthReachedZeroEventHandler(UnitToken token);

        [Signal]
        public delegate void UnitTokenDisposingEventHandler(UnitToken token);

        [Signal]
        public delegate void ShowUnitTokenMovementRangeEventHandler(Godot.Collections.Array<Vector2I> cells);

        [Signal]
        public delegate void ClearUnitTokenMovementRangeEventHandler(Godot.Collections.Array<Vector2I> cells);

        public Dictionary<Vector2I, GameBoardCell> Cells { get; private set; } = [];

        private readonly List<UnitToken> _tokens = [];

        private readonly Dictionary<UnitToken, GameBoardCell> _tokenCells = [];

        public GameBoardCell? SelectedCell { get; private set; }

        public GameBoardCell? HoveredCell { get; private set; }

        public HashSet<GameBoardCell>? CellsWithinMovementRange { get; private set; }

        public HashSet<GameBoardCell>? CellsWithinAttackRange { get; private set; }

        public UnitToken? SelectedToken => SelectedCell?.UnitToken;

        public GameBoard(IEnumerable<Vector2I> cells)
        {
            CreateCells(cells);
        }

        public async Task LateReadyAsync()
        {
            var faction = GD.Load<PlayerFactionResource>("res://Data/Factions/PlayerFactionResource.tres");
            var tokenType = GD.Load<Claire>("res://Data/Units/Claire.tres");

            SpawnToken(Cells[new(0, 3)], tokenType, faction);
        }

        public void CellPrimaryTouch(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnPrimaryTouch();
        }

        public void CellSecondaryTouch(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnSecondaryTouch();
        }

        public void OnCellMouseOver(Vector2I cellCoordinates)
        {
            if (!Cells.TryGetValue(cellCoordinates, out var c))
                throw new KeyNotFoundException(cellCoordinates.ToString());

            c.OnHover();
        }

        public void CreateCells(IEnumerable<Vector2I> cells)
        {
            Cells = new(cells.Count());

            foreach (var cell in cells)
            {
                Cells[cell] = new GameBoardCell(this, cell);
            }
        }

        public void SelectCell(GameBoardCell cell)
        {
            ClearSelectedCell();

            SelectedCell = cell;
            cell.IsSelected = true;

            EmitSignal(SignalName.CellSelected, cell.BoardPosition);

            if (SelectedToken is not null)
            {
                EmitSignal(SignalName.UnitTokenSelected, SelectedToken);

                ShowTokenMovementRange(SelectedToken);
            }
        }

        public void ClearSelectedCell()
        {
            if (SelectedCell is null)
                return;

            var cell = SelectedCell.BoardPosition;
            var selectedToken = SelectedToken;

            SelectedCell.IsSelected = false;
            SelectedCell = null;

            EmitSignal(SignalName.CellUnselected, cell);

            if (selectedToken is not null)
            {
                EmitSignal(SignalName.UnitTokenUnselected, selectedToken);

                ClearTokenMovementRange();
            }
        }

        public void ClearHoveredCell()
        {
            if(HoveredCell is null)
                return;

            HoveredCell.IsHovered = false;
            var cell = HoveredCell;

            HoveredCell = null;

            EmitSignal(SignalName.CellUnhovered, cell.BoardPosition);
        }

        public void HoverCell(GameBoardCell cell)
        {
            ClearHoveredCell();

            cell.IsHovered = true;
            HoveredCell = cell;

            EmitSignal(SignalName.CellHovered, cell.BoardPosition);
        }

        public void MoveToken(UnitToken token, GameBoardCell from, GameBoardCell to)
        {
            from.RemoveToken();

            to.SetToken(token);

            _tokenCells[token] = to;

            EmitSignal(SignalName.UnitTokenMoved, token, from.BoardPosition, to.BoardPosition);

            ClearTokenMovementRange();
        }

        public UnitToken? SpawnToken(GameBoardCell cell, UnitResource unitResource, FactionResource faction)
        {
            if (cell.UnitToken is not null)
                return null;

            var token = new UnitToken(unitResource, faction);

            token.Disposing += Token_Disposing;
            token.HealthChanged += Token_HealthChanged;
            token.HealthReachedZero += Token_HealthReachedZero;

            cell.SetToken(token);
            _tokens.Add(token);
            _tokenCells.Add(token, cell);

            EmitSignal(SignalName.UnitTokenCreated, token, cell.BoardPosition);

            return token;
        }

        private void Token_Disposing(DisposableRefCounted sender)
        {
            if (sender is not UnitToken token)
                return;

            token.Disposing -= Token_Disposing;
            token.HealthChanged -= Token_HealthChanged;
            token.HealthReachedZero -= Token_HealthReachedZero;

            // Remove from the master list.
            _tokens.Remove(token);

            // Remove from the cell's items.
            _tokenCells[token].RemoveToken();

            EmitSignal(SignalName.UnitTokenDisposing, token);

            token.Free();
        }

        private void Token_HealthChanged(UnitToken token, float previousHealth, float newHealth)
        {
            EmitSignal(SignalName.UnitTokenHealthChanged, token, previousHealth, newHealth);
        }

        private void Token_HealthReachedZero(UnitToken token)
        {
            EmitSignal(SignalName.UnitTokenHealthReachedZero, token);

            token.Dispose();
        }

        public int GetMaximumNumberOfCellsInRange(int range)
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

        public IEnumerable<GameBoardCell> GetCellsWithinRangeOf(GameBoardCell originCell, int range)
        {
            if (range <= 0)
                return Enumerable.Empty<GameBoardCell>();

            var maximumNeighborCount = GetMaximumNumberOfCellsInRange(range);

            var cellsInRange = new List<GameBoardCell>(maximumNeighborCount);

            var leftEdge = originCell.BoardPosition.X - range;
            var rightEdge = originCell.BoardPosition.X + range;
            var topEdge = originCell.BoardPosition.Y - range;
            var bottomEdge = originCell.BoardPosition.Y + range;

            var currentCellIsOffset = Math.Abs(originCell.BoardPosition.X) % 2 > 0;

            for (var x = leftEdge; x <= rightEdge; x++)
            {
                var xIsOffset = Math.Abs(x) % 2 > 0;

                for (var y = topEdge; y <= bottomEdge; y++)
                {
                    // Circle the square.
                    if ((x == leftEdge || x == rightEdge) && (y == topEdge || y == bottomEdge))
                        continue;

                    // Get rid of farthest cells in the alternate rows.
                    if (currentCellIsOffset && !xIsOffset && y == topEdge)
                        continue;

                    if (!currentCellIsOffset && xIsOffset && y == bottomEdge)
                        continue;

                    if (!Cells.TryGetValue(new(x, y), out var neighborCell))
                        continue;

                    cellsInRange.Add(neighborCell);
                }
            }

            return cellsInRange;
        }

        public void ShowTokenMovementRange(UnitToken token)
        {
            var range = token.MaxMoveDistance;

            if (!_tokenCells.TryGetValue(token, out var tokenCell))
                return;

            var cellsInRange = GetCellsWithinRangeOf(tokenCell, range)
                .Where(f => f != SelectedCell);

            if (!cellsInRange.Any())
            {
                CellsWithinMovementRange?.Clear();

                return;
            }

            CellsWithinMovementRange = cellsInRange.ToHashSet();

            foreach (var cell in cellsInRange)
            {
                cell.IsInMovementRange = true;
            }

            EmitSignal(SignalName.ShowUnitTokenMovementRange,
                new Godot.Collections.Array<Vector2I>(cellsInRange.Select(f => f.BoardPosition)));
        }

        public void ClearTokenMovementRange()
        {
            if (CellsWithinMovementRange is null || CellsWithinMovementRange.Count == 0)
                return;

            foreach (var cell in CellsWithinMovementRange)
            {
                cell.IsInMovementRange = false;
            }

            EmitSignal(SignalName.ClearUnitTokenMovementRange,
                new Godot.Collections.Array<Vector2I>(CellsWithinMovementRange.Select(f => f.BoardPosition)));

            CellsWithinMovementRange?.Clear();
        }
    }
}
