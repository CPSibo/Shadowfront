using Godot;
using System;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Buffs
{
    public partial class BoardPieceFindCover : BoardPieceInteraction
    {
        public bool EffectIsActive { get; private set; } = false;

        public override bool Perform(BoardPieceInteractionArguments args)
        {
            if (EffectIsActive)
                return false;



            EffectIsActive = true;

            return true;
        }
    }
}
