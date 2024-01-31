using Godot;
using Shadowfront.Backend.Board.BoardPieces.Behaviors;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shadowfront.Backend.Board.BoardPieces
{
    public partial class BoardPiece : DisposableNode2D
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Faction { get; set; } = string.Empty;

        public BoardPieceHealth? BoardPieceHealth { get; private set; }

        public BoardPieceMovement? BoardPieceMovement { get; private set; }

        public List<BoardPieceInteraction> Interactions { get; private set; } = [];

        public ObjectAttributes? ObjectAttributes { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            BoardPieceHealth = this.GetChildByType<BoardPieceHealth>();
            BoardPieceMovement = this.GetChildByType<BoardPieceMovement>();
            ObjectAttributes = this.GetChildByType<ObjectAttributes>();

            Interactions = this.GetChildrenByType<BoardPieceInteraction>()
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
