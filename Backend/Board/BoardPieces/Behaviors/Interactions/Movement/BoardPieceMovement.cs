using Godot;
using System.Collections.Generic;
using System.Linq;
using static Shadowfront.Backend.Utilities.HexTileMapUtils;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement
{
    public partial class BoardPieceMovement : BoardPieceInteraction, IHasRange
    {
        private Vector2I _position = Vector2I.MinValue;

        /// <summary>
        /// The item's current position.
        /// </summary>
        public Vector2I Position => _position;

        /// <summary>
        /// The maximum number of tiles the piece can move.
        /// </summary>
        [Export]
        public int MaxRange { get; set; }

        /// <summary>
        /// The minimum number of tiles the piece can move.
        /// </summary>
        [Export]
        public int MinRange { get; set; }

        /// <summary>
        /// Whether the current piece can attempt to move.
        /// </summary>
        [Export]
        public bool Enabled { get; set; } = true;

        public Color RangeColor { get; } = new(0x00aaef55);

        private HashSet<Vector2I> _cellsInrange = [];

        private HashSet<Vector2I> _validCellsInRange = [];

        public HashSet<Vector2I> CellsInRange => _cellsInrange;

        public HashSet<Vector2I> ValidCellsInRange => _validCellsInRange;

        private bool _rangeCalculationPending = false;

        public override void _Ready()
        {
            base._Ready();

            EventBus.Subscribe<GameBoard_NavGraphChangedEvent>(GameBoard_NavGraphChanged);
        }

        public override void _ExitTree()
        {
            EventBus.Unsubscribe<GameBoard_NavGraphChangedEvent>(GameBoard_NavGraphChanged);

            base._ExitTree();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if(_rangeCalculationPending)
                (_validCellsInRange, _cellsInrange) = ComputeCellsInRange();
        }

        private void GameBoard_NavGraphChanged(GameBoard_NavGraphChangedEvent e)
        {
            _rangeCalculationPending = true;
        }

        /// <summary>
        /// Compute the valid movement targets for the piece.
        /// </summary>
        private (HashSet<Vector2I>, HashSet<Vector2I>) ComputeCellsInRange()
        {
            _rangeCalculationPending = false;

            if (MaxRange <= 0)
                return ([], []);

            var args = new CellSearchArguments()
            {
                MaxRange = MaxRange,
                MinRange = MinRange,
                Origin = Position,
                GameBoard = GameBoard.Instance,
                Rules = CellSearchRules.ExcludeOtherTiles
                      | CellSearchRules.ExcludeOwnTile
                      | CellSearchRules.ExcludeOwnTeamTiles
                      | CellSearchRules.ExcludeOtherTeamTiles
                      | CellSearchRules.RequiresTraversablePath
            };

            var (valid, all) = GetValidCellsWithinRange(args);

            return (valid.ToHashSet(), all.ToHashSet());
        }

        /// <summary>
        /// Whether the piece is able to move to the given position.
        /// </summary>
        /// <remarks>
        /// <para>This takes into account if movement is disabled, if the target is the
        /// current position, and if the target is within range.</para>
        /// </remarks>
        public bool CanMoveTo(Vector2I position)
        {
            if(!Enabled)
                return false;

            if (MaxRange <= 0)
                return false;

            if (position == _position)
                return false;

            if (!_validCellsInRange.Contains(position))
                return false;

            return true;
        }

        /// <summary>
        /// Forces the position to the new value.
        /// </summary>
        public void ForcePosition(Vector2I desiredPosition)
        {
            var previousPosition = _position;

            _position = desiredPosition;

            EventBus.Emit(new BoardPieceMovement_PositionChangedEvent(_boardPiece, previousPosition, _position));
        }

        /// <summary>
        /// Attempts to update the position to the new value.
        /// </summary>
        /// <remarks>
        /// <para>Will check against the valid movement targets within <see cref="CellsInRange"/>.</para>
        /// </remarks>
        /// <param name="desiredPosition">Target movement cell.</param>
        /// <returns>Whether the movement was allowed.</returns>
        public override bool Perform(BoardPieceInteractionArguments args)
        {
            if (args.TargetCell is null)
                return false;

            var desiredPosition = args.TargetCell.BoardPosition;

            if (!CanMoveTo(desiredPosition))
                return false;

            var previousPosition = _position;

            _position = desiredPosition;

            EventBus.Emit(new BoardPieceMovement_PositionChangedEvent(_boardPiece, previousPosition, _position));

            return true;
        }
    }

    public readonly record struct BoardPieceMovement_PositionChangedEvent(BoardPiece BoardPiece, Vector2I PreviousPosition, Vector2I NewPosition) : IEventType;
}