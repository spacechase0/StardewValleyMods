using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomCritters.Framework
{
    internal class CritterEntry
    {
        public string Id { get; set; }
        public class CritterSpriteData
        {
            public int Variations { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public float Scale { get; set; } = 4;
            public bool Flying { get; set; } = true;
        }
        public CritterSpriteData SpriteData { get; set; } = new();

        public class Animation
        {
            public class AnimationFrame
            {
                public int Frame;
                public int Duration;
            }

            public List<AnimationFrame> Frames = new();
        }
        public Dictionary<string, Animation> Animations { get; set; } = new();

        public class SpawnCondition
        {
            public bool Not { get; set; } = false;
            public string[] Seasons { get; set; } = new string[0];
            public string[] Locations { get; set; } = new string[0];
            public int MinTimeOfDay { get; set; } = -1;
            public int MaxTimeOfDay { get; set; } = -1;
            public double ChancePerTile { get; set; } = 1.0 / 15000;
            public bool RequireDarkOut { get; set; } = false;
            public bool AllowRain { get; set; } = false;
            public string ChildrenCombine { get; set; } = "and";
            public List<SpawnCondition> Children { get; set; } = new();

            public bool Check(GameLocation loc)
            {
                bool ret = true;

                if (this.Children.Count > 0)
                {
                    if (this.ChildrenCombine != "and")
                        ret = false;

                    int totalMet = 0;
                    foreach (var child in this.Children)
                    {
                        bool childCheck = child.Check(loc);
                        if (childCheck)
                            ++totalMet;

                        switch (this.ChildrenCombine)
                        {
                            case "and": ret = ret && childCheck; break;
                            case "or": ret = ret || childCheck; break;
                            case "xor": ret = ret ^ childCheck; break;
                        }
                    }

                    if (this.ChildrenCombine.StartsWith("atleast"))
                    {
                        ret = totalMet >= int.Parse(this.ChildrenCombine.Substring(7));
                    }
                    else if (this.ChildrenCombine.StartsWith("exactly"))
                    {
                        ret = totalMet == int.Parse(this.ChildrenCombine.Substring(7));
                    }
                    else if (this.ChildrenCombine != "and" && this.ChildrenCombine != "or" && this.ChildrenCombine != "xor")
                    {
                        throw new ArgumentException("Bad ChildrenCombine: " + this.ChildrenCombine);
                    }
                }
                else if (this.MinTimeOfDay != -1 && Game1.timeOfDay < this.MinTimeOfDay)
                    ret = false;
                else if (this.MaxTimeOfDay != -1 && Game1.timeOfDay > this.MaxTimeOfDay)
                    ret = false;
                else if (this.Seasons != null && this.Seasons.Count() > 0 && !this.Seasons.Contains(Game1.currentSeason))
                    ret = false;
                else if (this.Locations != null && this.Locations.Count() > 0 && !this.Locations.Contains(loc.Name))
                    ret = false;
                else if (Game1.random.NextDouble() >= Math.Max(0.15, (Math.Min(0.5, loc.map.Layers[0].LayerWidth * loc.map.Layers[0].LayerHeight / this.ChancePerTile))))
                    ret = false;
                else if (this.RequireDarkOut && !Game1.isDarkOut())
                    ret = false;
                else if (!this.AllowRain && Game1.isRaining)
                    ret = false;

                if (this.Not)
                    ret = !ret;
                return ret;
            }
        }
        public List<SpawnCondition> SpawnConditions { get; set; } = new();

        public class BehaviorModel
        {
            public string Type { get; set; }
            public float Speed { get; set; }

            public class PatrolPoint
            {
                public string Type { get; set; } = "start";
                public float X { get; set; }
                public float Y { get; set; }
            }
            public List<PatrolPoint> PatrolPoints { get; set; } = new();
            public int PatrolPointDelay { get; set; }
            public int PatrolPointDelayAddRandom { get; set; }
        }
        public BehaviorModel Behavior { get; set; }

        public class SpawnLocation
        {
            public string LocationType { get; set; } = "random";
            //public Vector2 Offset { get; set; } = new Vector2();

            public class ConditionEntry
            {
                public bool Not { get; set; } = false;

                public double Chance { get; set; } = 1.0;
                public string Variable { get; set; }
                public bool RequireNotNull { get; set; }
                public string Is { get; set; }
                public string ValueEquals { get; set; }

                public string ChildrenCombine { get; set; } = "and";
                public List<ConditionEntry> Children { get; set; } = new();

                public bool Check(object obj)
                {
                    bool ret = true;

                    if (this.Children.Count > 0)
                    {
                        if (this.ChildrenCombine != "and")
                            ret = false;

                        int totalMet = 0;
                        foreach (var child in this.Children)
                        {
                            bool childCheck = child.Check(obj);
                            if (childCheck)
                                ++totalMet;

                            switch (this.ChildrenCombine)
                            {
                                case "and": ret = ret && childCheck; break;
                                case "or": ret = ret || childCheck; break;
                                case "xor": ret = ret ^ childCheck; break;
                            }
                        }

                        if (this.ChildrenCombine.StartsWith("atleast"))
                        {
                            ret = totalMet >= int.Parse(this.ChildrenCombine.Substring(7));
                        }
                        else if (this.ChildrenCombine.StartsWith("exactly"))
                        {
                            ret = totalMet == int.Parse(this.ChildrenCombine.Substring(7));
                        }
                        else if (this.ChildrenCombine != "and" && this.ChildrenCombine != "or" && this.ChildrenCombine != "xor")
                        {
                            throw new ArgumentException("Bad ChildrenCombine: " + this.ChildrenCombine);
                        }
                    }
                    else
                    {
                        if (this.Chance != 1.0 && Game1.random.NextDouble() > this.Chance)
                            ret = false;
                        if (this.Variable != null && this.Variable != "")
                        {
                            string[] toks = this.Variable.Split('.');

                            var o = obj;
                            for (int i = 0; i < toks.Length; ++i)
                            {
                                if (o == null)
                                    break;
                                var f = o.GetType().GetField(toks[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                if (f == null)
                                {
                                    o = null;
                                    break;
                                }

                                o = f.GetValue(o);
                            }

                            if (o != null)
                            {
                                if (this.Is != null && this.Is != "" && !o.GetType().IsInstanceOfType(Type.GetType(this.Is)))
                                    ret = false;
                                else if (this.ValueEquals != null && this.ValueEquals != "" && !o.ToString().Equals(this.ValueEquals))
                                    ret = false;
                            }
                            else if (this.RequireNotNull)
                                ret = false;
                        }
                    }

                    if (this.Not)
                        ret = !ret;

                    return ret;
                }
            }
            public List<ConditionEntry> Conditions { get; set; } = new();

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
                if (this.LocationType == "random")
                {
                    if (this.Check(null))
                        return loc.getRandomTile() * Game1.tileSize;
                    return null;
                }
                else if (this.LocationType == "terrainfeature")
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
                else if (this.LocationType == "object")
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
                else throw new ArgumentException("Bad location type");
            }
        }
        public List<SpawnLocation> SpawnLocations { get; set; } = new();

        public int SpawnAttempts { get; set; } = 3;

        public class LightModel
        {
            public int VanillaLightId = 3;
            public float Radius { get; set; } = 0.5f;
            public class ColorModel
            {
                public int R { get; set; } = 255;
                public int G { get; set; } = 255;
                public int B { get; set; } = 255;
            }
            public ColorModel Color { get; set; } = new();
        }
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
