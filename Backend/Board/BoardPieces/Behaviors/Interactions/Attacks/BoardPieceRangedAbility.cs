using Godot;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using Shadowfront.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Shadowfront.Backend.Utilities.HexTileMapUtils;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Attacks
{
    public abstract partial class BoardPieceRangedAbility : BoardPieceInteraction, IHasRange
    {
        [Export]
        public int MinRange { get; set; }

        [Export]
        public int MaxRange { get; set; }

        public abstract Color RangeColor { get; }

        [Export]
        public bool Enabled { get; set; } = true;

        [Export]
        public bool CanTargetOwnTeam { get; set; }

        private HashSet<Vector2I> _cellsInrange = [];

        private HashSet<Vector2I> _validCellsInRange = [];

        public HashSet<Vector2I> CellsInRange => _cellsInrange;

        public HashSet<Vector2I> ValidCellsInRange => _validCellsInRange;

        public abstract CellSearchRules CellSearchRules { get; }

        public override void _Ready()
        {
            base._Ready();

            EventBus.Subscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);
            EventBus.Subscribe<GameBoard_BoardPiecePlacedEvent>(GameBoard_BoardPiecePlaced);
        }

        public override void _ExitTree()
        {
            EventBus.Unsubscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);
            EventBus.Unsubscribe<GameBoard_BoardPiecePlacedEvent>(GameBoard_BoardPiecePlaced);

            base._ExitTree();
        }

        private void BoardPieceMovement_PositionChanged(BoardPieceMovement_PositionChangedEvent e)
        {
            (_validCellsInRange, _cellsInrange) = ComputeCellsInRange();
        }

        private void GameBoard_BoardPiecePlaced(GameBoard_BoardPiecePlacedEvent e)
        {
            (_validCellsInRange, _cellsInrange) = ComputeCellsInRange();
        }

        /// <summary>
        /// Compute the valid movement targets for the piece.
        /// </summary>
        private (HashSet<Vector2I>, HashSet<Vector2I>) ComputeCellsInRange()
        {
            if (MaxRange <= 0)
                return ([], []);

            if (_boardPiece.BoardPieceMovement is null)
                return ([], []);

            var args = new CellSearchArguments()
            {
                MaxRange = MaxRange,
                MinRange = MinRange,
                Origin = _boardPiece.BoardPieceMovement.Position,
                GameBoard = GameBoard.Instance,
                Rules = CellSearchRules
            };

            var (valid, all) = GetValidCellsWithinRange(args);

            return (valid.ToHashSet(), all.ToHashSet());
        }

        public abstract void Effect(BoardPiece target);

        public bool CanAttack(BoardPiece target)
        {
            if(!Enabled)
                return false;

            if(MaxRange <= 0)
                return false;

            if(_boardPiece.Faction == target.Faction && !CanTargetOwnTeam)
                return false;

            if (target.BoardPieceMovement is null)
                return false;

            var targetPositon = target.BoardPieceMovement.Position;

            if (!_validCellsInRange.Contains(targetPositon))
                return false;

            return true;
        }

        public override bool Perform(BoardPieceInteractionArguments args)
        {
            var target = args.TargetPiece;

            if(target is null)
                return false;

            if (!CanAttack(target))
                return false;

            EventBus.Emit(new BoardPieceRangedAbility_AttackEvent(_boardPiece, target, Effect));

            Effect(target);

            return true;
        }
    }

    public readonly record struct BoardPieceRangedAbility_AttackEvent(BoardPiece Attacker, BoardPiece Target, Action<BoardPiece> Effect) : IEventType;
}
