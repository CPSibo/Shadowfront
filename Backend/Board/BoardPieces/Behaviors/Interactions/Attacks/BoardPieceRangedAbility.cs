using Godot;
using System;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Attacks
{
    public abstract partial class BoardPieceRangedAbility : BoardPieceInteraction
    {
        [Export]
        public int MinimumRange { get; set; }

        [Export]
        public int MaximumRange { get; set; }

        [Export]
        public bool Enabled { get; set; } = true;

        [Export]
        public bool CanTargetOwnTeam { get; set; }

        private BoardPiece _parent { get; set; } = null!;

        public override void _Ready()
        {
            base._Ready();

            _parent = GetParent<BoardPiece>()
                ?? throw new Exception($"Cannot find parent of type {nameof(BoardPiece)}");
        }

        public abstract void Effect(BoardPiece target);

        public bool CanAttack(BoardPiece target)
        {
            if(!Enabled)
                return false;

            if(MaximumRange <= 0)
                return false;

            if(_parent.Faction == target.Faction && !CanTargetOwnTeam)
                return false;

            // TODO

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
