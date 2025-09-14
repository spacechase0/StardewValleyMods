using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceShared;

namespace SpaceCore.Dungeons
{
    // re: trigger actions - make note about hostonly (for spawnables)
    // General
    // tile action: spacechase0.SpaceCore_DungeonEntrance dungeonId
    // In dungeon:
    // map prop: spacechase0.SpaceCore_DungeonLadderEntrance x y
    // map prop: spacechase0.SpaceCore_DungeonElevatorEntrance x y
    // tile action: spacechase0.SpaceCore_DungeonElevatorMenu dungeonId
    // tile action: spacechase0.SpaceCore_DungeonLadderExit
    // tile action: spacechase0.SpaceCore_DungeonLadder
    // tile action: spacechase0.SpaceCore_DungeonMineshaft
    public class DungeonData
    {
        public class LevelRange
        {
            public int Begin { get; set; }
            public int End { get; set; } // inclusive
        }

        public class DungeonRegion
        {
            public LevelRange LevelRange { get; set; }
            public List<Weighted<string>> MapPool { get; set; } = new();
            public List<string> TriggerActionsOnEntry { get; set; } = new();
            public string LocationDataEntry { get; set; }
        }

        // Might need something for defining the world map region to show up at?

        public Dictionary<string, DungeonRegion> Regions { get; set; }

        public string LadderExitLocation { get; set; }
        public Point LadderExitTile { get; set; }
        public string ElevatorExitLocation { get; set; }
        public Point ElevatorExitTile { get; set; }
        public int[] FloorsWithElevator { get; set; }

        public bool SpawnLadders { get; set; } = true;
        public bool SpawnMineshafts { get; set; } = false;

        public string LadderTileSheet { get; set; } = "Maps/Mines/mine_desert";
        public int LadderTileIndex { get; set; } = 173;
        public string MineshaftTileSheet { get; set; } = "Maps/Mines/mine_desert";
        public int MineshaftTileIndex { get; set; } = 174;

        public int AdditionalTimeMilliseconds { get; set; } = 2000;
        public bool ViewportFollowsPlayer { get; set; } = true;

        public int GetMaxLevel()
        {
            int ret = 0;
            foreach (var region in Regions)
                ret = Math.Max(ret, region.Value.LevelRange.End);
            return ret;
        }
    }
}
