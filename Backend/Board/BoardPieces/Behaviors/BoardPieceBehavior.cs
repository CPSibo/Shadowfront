using Godot;
using System;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors
{
    public partial class BoardPieceBehavior : GameObject
    {
        protected BoardPiece _boardPiece = null!;

        public override void _Ready()
        {
            base._Ready();

            _boardPiece = GetParent<BoardPiece>()
                ?? throw new Exception($"Cannot find parent of type {nameof(BoardPiece)}");
        }
    }
}
