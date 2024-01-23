using Godot;

namespace Shadowfront.Backend.Board
{
    public partial class UnitToken : DisposableNode2D
    {
        public float Damage { get; set; }

        public string Faction { get; set; } = string.Empty;

        public BoardPieceHealth? BoardPieceHealth { get; private set; }

        public BoardPieceMovement? BoardPieceMovement { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            BoardPieceHealth = FindChild("BoardPieceHealth") as BoardPieceHealth;
            BoardPieceMovement = FindChild("BoardPieceMovement") as BoardPieceMovement;
        }

        public void Attack(UnitToken target)
        {
            //
        }
    }
}
