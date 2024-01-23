using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadowfront
{
    public static class TileMapExtensions
    {
        /// <summary>
        /// Global -> Local
        /// </summary>
        public static Vector2 GetLocalFromGlobal(this TileMap tilemap, Vector2 globalCoords)
            => tilemap.ToLocal(globalCoords);

        /// <summary>
        /// Cell -> Local
        /// </summary>
        public static Vector2 GetLocalFromCell(this TileMap tilemap, Vector2I cell)
            => tilemap.MapToLocal(cell);

        /// <summary>
        /// Cell -> Global
        /// </summary>
        public static Vector2 GetGlobalFromCell(this TileMap tilemap, Vector2I cell)
            => tilemap.ToGlobal(tilemap.GetLocalFromCell(cell));

        /// <summary>
        /// Local -> Global
        /// </summary>
        public static Vector2 GetGlobalFromLocal(this TileMap tilemap, Vector2 localCoords)
            => tilemap.ToGlobal(localCoords);

        /// <summary>
        /// Local -> Cell
        /// </summary>
        public static Vector2I GetCellFromLocal(this TileMap tilemap, Vector2 localCoords)
            => tilemap.LocalToMap(localCoords);

        /// <summary>
        /// Global -> Cell
        /// </summary>
        public static Vector2I GetCellFromGlobal(this TileMap tilemap, Vector2 globalCoords)
            => tilemap.GetCellFromLocal(tilemap.GetLocalFromGlobal(globalCoords));
    }
}
