using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace CustomCritters.Framework.CritterData
{
    internal class SpawnLocation
    {
        public string LocationType { get; set; } = "random";
        //public Vector2 Offset { get; set; } = new Vector2();
        public List<SpawnLocationConditionEntry> Conditions { get; set; } = new();

        public bool Check(object obj)
        {
            foreach (var cond in this.Conditions)
            {
                if (!cond.Check(obj))
                    return false;
            }

            return true;
        }

        public Vector2? PickSpot(GameLocation loc)
        {
            switch (this.LocationType)
            {
                case "random":
                    return this.Check(null)
                        ? loc.getRandomTile() * Game1.tileSize
                        : null;

                case "terrainfeature":
                    {
                        var keys = loc.terrainFeatures.Keys.ToList();
                        keys.Shuffle();
                        foreach (var key in keys)
                        {
                            if (this.Check(loc.terrainFeatures[key]))
                                return key * Game1.tileSize;
                        }

                        return null;
                    }

                case "object":
                    {
                        var keys = loc.objects.Keys.ToList();
                        keys.Shuffle();
                        foreach (var key in keys)
                        {
                            if (this.Check(loc.objects[key]))
                                return key * Game1.tileSize;
                        }

                        return null;
                    }

                default:
                    throw new ArgumentException("Bad location type");
            }
        }
    }
}
