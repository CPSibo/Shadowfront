using Godot;
using Shadowfront.Backend;
using Shadowfront.Backend.Board;
using Shadowfront.Backend.Board.BoardPieces;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Attacks;
using Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Movement;
using System;
using System.Collections.Generic;

namespace Shadowfront.Frontend.UI.Subscreens
{
    public partial class BoardPieceActionBar : Control
    {
        public BoardPiece? BoardPiece { get; set; }

        private Control _buttonContainer = null!;

        private List<Control> _interactionButtons = [];

        public override void _Ready()
        {
            base._Ready();

            _buttonContainer = FindChild("InteractionButtonContainer") as Control
                ?? throw new Exception("Cannot find InteractionButtonContainer");

            EventBus.Subscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
            EventBus.Subscribe<GameBoard_BoardPieceActivatedEvent>(GameBoard_BoardPieceActivated);
            EventBus.Subscribe<GameBoard_BoardPieceDectivatedEvent>(GameBoard_BoardPieceDeactivated);
        }

        public override void _ExitTree()
        {
            EventBus.Unsubscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
            EventBus.Unsubscribe<GameBoard_BoardPieceActivatedEvent>(GameBoard_BoardPieceActivated);
            EventBus.Unsubscribe<GameBoard_BoardPieceDectivatedEvent>(GameBoard_BoardPieceDeactivated);

            base._ExitTree();
        }

        private void GameBoard_BoardPieceActivated(GameBoard_BoardPieceActivatedEvent e)
        {
            BoardPiece = e.BoardPiece;

            AddBoardPieceInteractionButtons();

            Visible = true;
        }

        private void GameBoard_BoardPieceDeactivated(GameBoard_BoardPieceDectivatedEvent e)
        {
            Visible = false;
            BoardPiece = null;
        }

        private void AddBoardPieceInteractionButtons()
        {
            var children = _buttonContainer.GetChildren();

            foreach (var child in children)
            {
                child.QueueFree();
            }

            if (BoardPiece is null)
                return;

            var interactions = BoardPiece.Interactions;

            if (interactions.Count == 0)
                return;

            foreach(var interaction in interactions)
            {
                var interactionScenePath = "res://Frontend/UI/Controls/GameBoard/BoardPieceActionBar/BoardPieceActionBarButton.tscn";
                var packedScene = GD.Load<PackedScene>(interactionScenePath)
                    ?? throw new Exception("Could not load action bar button");

                var button = packedScene.Instantiate<BoardPieceActionBarButton>();

                button.Text = interaction switch
                {
                    BoardPieceMovement movement => "M",
                    BoardPieceGunAbility movement => "G",
                    _ => "?",
                };

                _interactionButtons.Add(button);
                _buttonContainer.AddChild(button);

                void InteractionButton_GuiInput(InputEvent @event)
                {
                    if (@event is not InputEventMouseButton mouseEvent)
                        return;

                    if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed())
                        EventBus.Emit(new BoardPieceActionBar_InteractionButtonClickedEvent(this, interaction));
                }

                button.GuiInput += InteractionButton_GuiInput;
                button.TreeExiting += () => button.GuiInput -= InteractionButton_GuiInput;
            }
        }

        private void BoardPiece_Disposing(BoardPiece_DisposingEvent e)
        {
            if(e.BoardPiece == BoardPiece)
                BoardPiece = null;
        }
    }

    public readonly record struct BoardPieceActionBar_InteractionButtonClickedEvent(BoardPieceActionBar sender, BoardPieceInteraction Interaction) : IEventType;
}