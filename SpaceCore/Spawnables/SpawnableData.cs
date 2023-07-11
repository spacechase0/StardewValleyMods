using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SpaceCore.Spawnables
{
    public class ItemData
    {
        public string QualifiedId { get; set; }
        public int Quantity { get; set; }
        public int Quality { get; set; }
    }

    // Probably should change this to virtual class...
    public class SpawnableData
    {
        public enum SpawnableType
        {
            SetPiece,
            Forageable,
            Minable,
            LargeMinable,
            Breakable,
            Furniture,
            Monster,
        }
        public SpawnableType Type { get; set; }
        public string Id { get; set; } // forageable (object), minable (object), 

        public int SetPieceSizeX { get; set; }
        public int SetPieceSizeY { get; set; }

    }

    public class SpawnableRegion
    {
        public List<Rectangle> IncludeRegions { get; set; }
        public List<Rectangle> ExcludeRegions { get; set; }

        public double RecurringChance { get; set; } // while ( r.NextDouble() <= RecurringChance ) spawn();
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }
}
