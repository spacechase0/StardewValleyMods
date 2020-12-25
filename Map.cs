using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireArcadeGame
{
    public enum FloorTile
    {
        Stone,
        Lava,
    }

    public enum WallTile
    {
        Empty,
        Stone,
    }

    public class Map
    {
        public Vector2 Size { get; }

        public FloorTile[,] Floor { get; }
        public WallTile[,] Walls { get; }

        public Map( Vector2 size )
        {
            Size = size;
            Floor = new FloorTile[ (int) size.X, (int) size.Y ];
            Walls = new WallTile[ (int) size.X, (int) size.Y ];
        }

        public bool IsSolid( float x, float y )
        {
            int ix = ( int ) x, iy = ( int ) y;
            return Floor[ ix, iy ] == FloorTile.Lava || Walls[ ix, iy ] != WallTile.Empty;
        }
    }
}
