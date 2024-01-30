using Shadowfront.Backend.Board.BoardPieces.Behaviors;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using System.Collections.Generic;
using System.Linq;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class BoardPiece : DisposableNode2D
    {
        public string Faction { get; set; } = string.Empty;

        public BoardPieceHealth? BoardPieceHealth { get; private set; }

        public BoardPieceMovement? BoardPieceMovement { get; private set; }

        public List<BoardPieceInteraction> Interactions { get; private set; } = [];

        public override void _Ready()
        {
            base._Ready();

            BoardPieceHealth = FindChild("BoardPieceHealth") as BoardPieceHealth;
            BoardPieceMovement = FindChild("BoardPieceMovement") as BoardPieceMovement;

            Interactions = GetChildren()
                .Where(f => f is BoardPieceInteraction)
                .Cast<BoardPieceInteraction>()
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            EventBus.Emit(new BoardPiece_DisposingEvent(this));

            base.Dispose(disposing);
        }
    }

    public readonly record struct BoardPiece_DisposingEvent(BoardPiece BoardPiece) : IEventType;
}
