using System.Collections.Generic;
using CustomCritters.Framework.CritterData;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomCritters.Framework
{
    internal class CritterEntry
    {
        public string Id { get; set; }
        public CritterSpriteData SpriteData { get; set; } = new();
        public Dictionary<string, Animation> Animations { get; set; } = new();
        public List<SpawnCondition> SpawnConditions { get; set; } = new();
        public BehaviorModel Behavior { get; set; }
        public List<SpawnLocation> SpawnLocations { get; set; } = new();
        public int SpawnAttempts { get; set; } = 3;
        public LightModel Light { get; set; } = null;

        public virtual bool Check(GameLocation loc)
        {
            foreach (var cond in this.SpawnConditions)
            {
                if (!cond.Check(loc))
                    return false;
            }

            return true;
        }

        public virtual Vector2? PickSpot(GameLocation loc)
        {
            foreach (var sl in this.SpawnLocations)
            {
                var ret = sl.PickSpot(loc);
                if (ret.HasValue)
                    return ret.Value;
            }
            return null;
        }

        public virtual Critter MakeCritter(Vector2 pos)
        {
            return new CustomCritter(pos + new Vector2(1, 1) * (Game1.tileSize / 2), this);
        }

        internal static Dictionary<string, CritterEntry> Critters = new();
        public static void Register(CritterEntry entry)
        {
            CritterEntry.Critters.Add(entry.Id, entry);
        }
    }
}
