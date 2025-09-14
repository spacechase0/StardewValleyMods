using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared;

namespace CombatOverhaulMod.Dungeons
{
    public class DungeonData
    {
        public string WorldMapLocatableName { get; set; } // The name detected by the world map, so that you can show it on custom world maps properly
        public string ResetLocation { get; set; } // Reset all instances of this dungeon (if no players are in any) when entering this location

        public class LevelData
        {
            public string MapPath { get; set; }
            public string CustomMapGeneratorType { get; set; } // C# type: "MyMod.MapGenerator, MyMod"

            public Dictionary<string, SpaceCore.SpawnableRegion> Spawnables { get; set; }

            // water spawnable regions?
            // tillable soil spawnable regions?
        }
        public Dictionary<string, LevelData> LevelTypes { get; set; }

        public List<Weighted<string>> RandomFloor { get; set; }
        public Dictionary<int, string> FixedFloors { get; set; }
    }
}
