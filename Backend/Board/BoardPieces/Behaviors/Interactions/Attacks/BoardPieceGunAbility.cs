namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Attacks
{
    public partial class BoardPieceGunAbility : BoardPieceRangedAbility
    {
        public BoardPieceGunAbility()
        {
            MaximumRange = 2;
            MinimumRange = 1;
            CanTargetOwnTeam = false;
        }

        public override void Effect(BoardPiece target)
        {
            var targetHealth = target.BoardPieceHealth;

            if (targetHealth is null)
                return;

            targetHealth.AddCurrentHealth(-5f);
        }
    }
}
