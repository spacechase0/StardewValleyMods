using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared;

namespace GreatCaveOffensive.Data;

public class BiomeData
{
    public string BaseTilesheet { get; set; }

    public class GroundTile
    {
        public Point Size;
        public TileReference[] Tiles;

        public GroundTile() { }
        public GroundTile(int i)
        {
            Size = new(1, 1);
            Tiles = [new TileReference(i)];
        }
    }
    public Weighted<GroundTile>[] GroundTiles { get; set; }

    public class WallType
    {
        public int Height;

        public double DecorationChance;
        public Weighted<TileReference[]>[] Decorations;
    }
    public Weighted<WallType>[] Walls { get; }

}
