using Microsoft.Xna.Framework;

namespace PyromancersJourney.Framework
{
    internal class Map
    {
        public Color Sky = Color.Black;

        public Vector2 Size { get; }

        public FloorTile[,] Floor { get; }
        public WallTile[,] Walls { get; }

        public Map(Vector2 size)
        {
            this.Size = size;
            this.Floor = new FloorTile[(int)size.X, (int)size.Y];
            this.Walls = new WallTile[(int)size.X, (int)size.Y];
        }

        public bool IsSolid(float x, float y)
        {
            int ix = (int)x, iy = (int)y;
            if (ix < 0 || iy < 0 || ix >= this.Size.X || iy >= this.Size.Y)
                return true;

            return this.Floor[ix, iy] == FloorTile.Lava || this.Walls[ix, iy] != WallTile.Empty;
        }

        public bool IsAirSolid(float x, float y)
        {
            int ix = (int)x, iy = (int)y;
            if (ix < 0 || iy < 0 || ix >= this.Size.X || iy >= this.Size.Y)
                return true;

            return this.Walls[ix, iy] != WallTile.Empty;
        }
    }
}
