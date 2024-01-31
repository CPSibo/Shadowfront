using Godot;
using System;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions
{
    public readonly record struct BoardPieceInteractionArguments
    (
        BoardPiece? ActingPiece = null,
        GameBoardCell? ActingCell = null,
        BoardPiece? TargetPiece = null,
        GameBoardCell? TargetCell = null
    );

    public abstract partial class BoardPieceInteraction : BoardPieceBehavior
    {
        public abstract bool Perform(BoardPieceInteractionArguments args);
    }
}
