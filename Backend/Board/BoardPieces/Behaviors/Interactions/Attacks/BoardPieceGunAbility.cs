using Godot;
using static Shadowfront.Backend.Utilities.HexTileMapUtils;

namespace Shadowfront.Backend.Board.BoardPieces.Behaviors.Interactions.Attacks
{
    public partial class BoardPieceGunAbility : BoardPieceRangedAbility
    {
        public override Color RangeColor { get; } = new(0xef555555);

        public override CellSearchRules CellSearchRules =>
              CellSearchRules.ExcludeOwnTeamTiles
            | CellSearchRules.ExcludeOwnTile
            | CellSearchRules.ExcludeEmptyTiles;

        public BoardPieceGunAbility()
        {
            MaxRange = 2;
            MinRange = 1;
            CanTargetOwnTeam = false;
        }

        public override void Effect(BoardPiece target)
        {
            var targetHealth = target.ObjectAttributes?[DefaultObjectAttributes.Keys.HEALTH];

            if (targetHealth is null)
                return;

            targetHealth.Value.Current -= 5f;


            //var targetHealth = target.BoardPieceHealth;

            //if (targetHealth is null)
            //    return;

            //targetHealth.AddCurrentHealth(-5f);
        }
    }
}
