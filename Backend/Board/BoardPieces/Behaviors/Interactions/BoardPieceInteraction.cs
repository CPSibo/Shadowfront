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

    public abstract partial class BoardPieceInteraction : Node
    {
        protected BoardPiece _boardPiece = null!;

        public override void _Ready()
        {
            base._Ready();

            _boardPiece = GetParent<BoardPiece>()
                ?? throw new Exception($"Cannot find parent of type {nameof(BoardPiece)}");
        }

        public abstract bool Perform(BoardPieceInteractionArguments args);
    }
}
