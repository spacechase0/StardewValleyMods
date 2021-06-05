using Microsoft.Xna.Framework;

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
        public Color Sky = Color.Black;

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
            if ( ix < 0 || iy < 0 || ix >= Size.X || iy >= Size.Y )
                return true;

            return Floor[ ix, iy ] == FloorTile.Lava || Walls[ ix, iy ] != WallTile.Empty;
        }

        public bool IsAirSolid( float x, float y )
        {
            int ix = ( int ) x, iy = ( int ) y;
            if ( ix < 0 || iy < 0 || ix >= Size.X || iy >= Size.Y )
                return true;

            return Walls[ ix, iy ] != WallTile.Empty;
        }
    }
}
