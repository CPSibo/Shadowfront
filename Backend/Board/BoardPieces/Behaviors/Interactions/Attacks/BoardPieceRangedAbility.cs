using Godot;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using Shadowfront.Backend.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private BoardPiece _parent { get; set; } = null!;

        private HashSet<Vector2I> _availableCells = [];

        private HashSet<Vector2I> _cellsInrange = [];

        public HashSet<Vector2I> CellsInRange => _cellsInrange;

        public override void _Ready()
        {
            base._Ready();

            _parent = GetParent<BoardPiece>()
                ?? throw new Exception($"Cannot find parent of type {nameof(BoardPiece)}");

            EventBus.Subscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);
        }

        public override void _ExitTree()
        {
            EventBus.Unsubscribe<BoardPieceMovement_PositionChangedEvent>(BoardPieceMovement_PositionChanged);

            base._ExitTree();
        }

        private void BoardPieceMovement_PositionChanged(BoardPieceMovement_PositionChangedEvent e)
        {
            if (e.BoardPiece != _parent)
                return;

            _cellsInrange = ComputeCellsInRange();
        }

        public void SetAvailableCells(IEnumerable<Vector2I> availableCells)
        {
            _availableCells = [.. availableCells];

            _cellsInrange = ComputeCellsInRange();
        }

        /// <summary>
        /// Compute the valid movement targets for the piece.
        /// </summary>
        private HashSet<Vector2I> ComputeCellsInRange()
        {
            if (MaxRange <= 0)
                return [];

            if (_parent.BoardPieceMovement is null)
                return [];

            return HexTileMapUtils
                .GetHypotheticalCellsWithinRange(_parent.BoardPieceMovement.Position, MaxRange)
                .ToHashSet();
        }

        public abstract void Effect(BoardPiece target);

        public bool CanAttack(BoardPiece target)
        {
            if(!Enabled)
                return false;

            if(MaxRange <= 0)
                return false;

            if(_parent.Faction == target.Faction && !CanTargetOwnTeam)
                return false;

            if (target.BoardPieceMovement is null)
                return false;

            var targetPositon = target.BoardPieceMovement.Position;

            if (!CellsInRange.Contains(targetPositon))
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

            EventBus.Emit(new BoardPieceRangedAbility_AttackEvent(_parent, target, Effect));

            Effect(target);

            return true;
        }
    }

    public readonly record struct BoardPieceRangedAbility_AttackEvent(BoardPiece Attacker, BoardPiece Target, Action<BoardPiece> Effect) : IEventType;
}
