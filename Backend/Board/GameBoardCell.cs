using Godot;
using Shadowfront.Backend.Board.BoardPieces;

namespace Shadowfront.Backend.Board
{
    public class GameBoardCell
    {
        public long Id { get; private set; }

        public Vector2I BoardPosition { get; private set; }

        public BoardPiece? BoardPiece { get; private set; }

        public GameBoardCell(Vector2I boardPosition, long id)
        {
            BoardPosition = boardPosition;
            Id = id;

            EventBus.Subscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
        }

        ~GameBoardCell()
        {
            EventBus.Unsubscribe<BoardPiece_DisposingEvent>(BoardPiece_Disposing);
        }

        public void SetBoardPiece(BoardPiece token)
        {
            BoardPiece = token;
        }

        public void RemoveBoardPiece()
        {
            BoardPiece = null;
        }

        private void BoardPiece_Disposing(BoardPiece_DisposingEvent e)
        {
            if(e.BoardPiece == BoardPiece)
                RemoveBoardPiece();
        }
    }

    public readonly record struct GameBoardCell_MovementTargetSelectedEvent(GameBoardCell Target) : IEventType;
}
