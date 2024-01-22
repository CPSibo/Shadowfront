using Godot;
using System;

namespace Shadowfront.Backend.Board
{
    public class GameBoardCell
    {
        private readonly GameBoard _gameBoard;

        public bool IsHovered { get; set; }

        public bool IsSelected { get; set; }

        public bool IsInAttackRange { get; set; }

        public bool IsInMovementRange { get; set; }

        public bool IsDefault => 
            !IsHovered 
            && !IsSelected 
            && !IsInAttackRange 
            && !IsInMovementRange;

        public Vector2I BoardPosition { get; private set; }

        public UnitToken? UnitToken { get; private set; }

        public GameBoardCell(GameBoard gameBoard, Vector2I boardPosition)
        {
            _gameBoard = gameBoard;
            BoardPosition = boardPosition;
        }

        public void SetToken(UnitToken token)
        {
            UnitToken = token;
        }

        public UnitToken? RemoveToken()
        {
            var token = UnitToken;

            UnitToken = null;

            return token;
        }

        public void OnHover()
        {
            if (IsHovered)
                return;

            if (!IsDefault)
            {
                _gameBoard.ClearHoveredCell();

                return;
            }

            _gameBoard.HoverCell(this);
        }

        public void OnPrimaryTouch()
        {
            if (_gameBoard.SelectedCell == this)
            {
                _gameBoard.ClearSelectedCell();

                return;
            }

            if (UnitToken is not null)
            {
                _gameBoard.SelectCell(this);

                return;
            }

            if (_gameBoard.SelectedCell is not null && _gameBoard.SelectedToken is not null)
            {
                if (_gameBoard.CellsWithinMovementRange is null || _gameBoard.CellsWithinMovementRange.Count == 0)
                    return;

                if (!_gameBoard.CellsWithinMovementRange.Contains(this))
                    return;

                _gameBoard.MoveToken(_gameBoard.SelectedToken, _gameBoard.SelectedCell, this);

                _gameBoard.ClearSelectedCell();

                return;
            }

            var faction = GD.Load<PlayerFactionResource>("res://Data/Factions/PlayerFactionResource.tres"); // DevWindow.Instance.SelectedFaction;
            var tokenType = GD.Load<Claire>("res://Data/Units/Claire.tres"); // DevWindow.Instance.SelectedTokenType;

            if (faction is null)
                throw new Exception("No faction given");

            if (tokenType is null)
                throw new Exception("No token type given");

            _gameBoard.SpawnToken(
                this,
                tokenType,
                faction
            );
        }

        public void OnSecondaryTouch()
        {
            if (_gameBoard.SelectedCell == this)
            {
                // NOOP

                return;
            }

            if (_gameBoard.SelectedToken is not null && UnitToken is not null)
            {
                // Attack
                _gameBoard.SelectedToken.Attack(UnitToken);

                _gameBoard.ClearSelectedCell();

                return;
            }

            // NOOP
        }
    }
}
