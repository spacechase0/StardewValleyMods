using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomCritters
{
    public class CritterEntry
    {
        public string Id { get; set; }
        public class SpriteData_
        {
            public int Variations { get; set; }
            public int FrameWidth { get; set; }
            public int FrameHeight { get; set; }
            public float Scale { get; set; } = 4;
            public Boolean Flying { get; set; } = true;
        }
        public SpriteData_ SpriteData { get; set; } = new SpriteData_();

        public class Animation_
        {
            public class AnimationFrame_
            {
                public int Frame;
                public int Duration;
            }

            public List<AnimationFrame_> Frames = new List<AnimationFrame_>();
        }
        public Dictionary<string, Animation_> Animations { get; set; } = new Dictionary<string, Animation_>();

        public class SpawnCondition_
        {
            public Boolean Not { get; set; } = false;
            public string[] Seasons { get; set; } = new string[0];
            public string[] Locations { get; set; } = new string[0];
            public int MinTimeOfDay { get; set; } = -1;
            public int MaxTimeOfDay { get; set; } = -1;
            public double ChancePerTile { get; set; } = 1.0 / 15000;
            public bool RequireDarkOut { get; set; } = false;
            public bool AllowRain { get; set; } = false;
            public string ChildrenCombine { get; set; } = "and";
            public List<SpawnCondition_> Children { get; set; } = new List<SpawnCondition_>();

            public bool check( GameLocation loc )
            {
                bool ret = true;

                if ( Children.Count > 0 )
                {
                    if (ChildrenCombine != "and")
                        ret = false;

                    int totalMet = 0;
                    foreach ( var child in Children )
                    {
                        bool childCheck = child.check(loc);
                        if (childCheck)
                            ++totalMet;

                        switch ( ChildrenCombine )
                        {
                            case "and": ret = ret && childCheck; break;
                            case "or": ret = ret || childCheck; break;
                            case "xor": ret = ret ^ childCheck; break;
                        }
                    }

                    if ( ChildrenCombine.StartsWith( "atleast" ) )
                    {
                        ret = totalMet >= int.Parse(ChildrenCombine.Substring(7));
                    }
                    else if ( ChildrenCombine.StartsWith( "exactly" ) )
                    {
                        ret = totalMet == int.Parse(ChildrenCombine.Substring(7));
                    }
                    else if ( ChildrenCombine != "and" && ChildrenCombine != "or" && ChildrenCombine != "xor" )
                    {
                        throw new ArgumentException("Bad ChildrenCombine: " + ChildrenCombine);
                    }
                }
                else if (MinTimeOfDay != -1 && Game1.timeOfDay < MinTimeOfDay)
                    ret = false;
                else if (MaxTimeOfDay != -1 && Game1.timeOfDay > MaxTimeOfDay)
                    ret = false;
                else if (Seasons != null && Seasons.Count() > 0 && !Seasons.Contains(Game1.currentSeason))
                    ret = false;
                else if (Locations != null && Locations.Count() > 0 && !Locations.Contains(loc.Name))
                    ret = false;
                else if (Game1.random.NextDouble() >= Math.Max(0.15, (Math.Min(0.5, loc.map.Layers[0].LayerWidth * loc.map.Layers[0].LayerHeight / ChancePerTile))))
                    ret = false;
                else if (RequireDarkOut && !Game1.isDarkOut())
                    ret = false;
                else if (!AllowRain && Game1.isRaining)
                    ret = false;

                if (Not)
                    ret = !ret;
                return ret;
            }
        }
        public List<SpawnCondition_> SpawnConditions { get; set; } = new List<SpawnCondition_>();

        public class Behavior_
        {
            public string Type { get; set; }
            public float Speed { get; set; }
            
            public class PatrolPoint_
            {
                public string Type { get; set; } = "start";
                public float X { get; set; }
                public float Y { get; set; }
            }
            public List<PatrolPoint_> PatrolPoints { get; set; } = new List<PatrolPoint_>();
            public int PatrolPointDelay { get; set; }
            public int PatrolPointDelayAddRandom { get; set; }
        }
        public Behavior_ Behavior { get; set; }

        public class SpawnLocation_
        {
            public string LocationType { get; set; } = "random";
            //public Vector2 Offset { get; set; } = new Vector2();

            public class ConditionEntry_
            {
                public bool Not { get; set; } = false;

                public double Chance { get; set; } = 1.0;
                public string Variable { get; set; }
                public bool RequireNotNull { get; set; }
                public string Is { get; set; }
                public string ValueEquals { get; set; }

                public string ChildrenCombine { get; set; } = "and";
                public List<ConditionEntry_> Children { get; set; } = new List<ConditionEntry_>();

                public bool check( object obj )
                {
                    bool ret = true;

                    if (Children.Count > 0)
                    {
                        if (ChildrenCombine != "and")
                            ret = false;

                        int totalMet = 0;
                        foreach (var child in Children)
                        {
                            bool childCheck = child.check(obj);
                            if (childCheck)
                                ++totalMet;

                            switch (ChildrenCombine)
                            {
                                case "and": ret = ret && childCheck; break;
                                case "or": ret = ret || childCheck; break;
                                case "xor": ret = ret ^ childCheck; break;
                            }
                        }

                        if (ChildrenCombine.StartsWith("atleast"))
                        {
                            ret = totalMet >= int.Parse(ChildrenCombine.Substring(7));
                        }
                        else if (ChildrenCombine.StartsWith("exactly"))
                        {
                            ret = totalMet == int.Parse(ChildrenCombine.Substring(7));
                        }
                        else if (ChildrenCombine != "and" && ChildrenCombine != "or" && ChildrenCombine != "xor")
                        {
                            throw new ArgumentException("Bad ChildrenCombine: " + ChildrenCombine);
                        }
                    }
                    else
                    {
                        if (Chance != 1.0 && Game1.random.NextDouble() > Chance)
                            ret = false;
                        if (Variable != null && Variable != "")
                        {
                            string[] toks = Variable.Split('.');

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
                                if (Is != null && Is != "" && !o.GetType().IsInstanceOfType(Type.GetType(Is)))
                                    ret = false;
                                else if (ValueEquals != null && ValueEquals != "" && !o.ToString().Equals(ValueEquals))
                                    ret = false;
                            }
                            else if (RequireNotNull)
                                ret = false;
                        }
                    }

                    if (Not)
                        ret = !ret;

                    return ret;
                }
            }
            public List<ConditionEntry_> Conditions { get; set; } = new List<ConditionEntry_>();
            
            public bool check(object obj)
            {
                foreach (var cond in Conditions)
                {
                    if (!cond.check(obj))
                        return false;
                }

                return true;
            }

            public Vector2? pickSpot( GameLocation loc )
            {
                if (LocationType == "random")
                {
                    if ( check( null ) )
                        return loc.getRandomTile() * Game1.tileSize;
                    return null;
                }
                else if (LocationType == "terrainfeature")
                {
                    var keys = loc.terrainFeatures.Keys.ToList();
                    keys.Shuffle();
                    foreach ( var key in keys )
                    {
                        if (check(loc.terrainFeatures[key]))
                            return key * Game1.tileSize;
                    }

                    return null;
                }
                else if (LocationType == "object")
                {
                    var keys = loc.objects.Keys.ToList();
                    keys.Shuffle();
                    foreach (var key in keys)
                    {
                        if (check(loc.objects[key]))
                            return key * Game1.tileSize;
                    }

                    return null;
                }
                else throw new ArgumentException("Bad location type");
            }
        }
        public List<SpawnLocation_> SpawnLocations { get; set; } = new List<SpawnLocation_>();

        public int SpawnAttempts { get; set; } = 3;

        public class Light_
        {
            public int VanillaLightId = 3;
            public float Radius { get; set; } = 0.5f;
            public class Color_
            {
                public int R { get; set; } = 255;
                public int G { get; set; } = 255;
                public int B { get; set; } = 255;
            }
            public Color_ Color { get; set; } = new Color_();
        }
        public Light_ Light { get; set; } = null;

        public virtual bool check( GameLocation loc )
        {
            foreach ( var cond in SpawnConditions )
            {
                if (!cond.check(loc))
                    return false;
            }

            return true;
        }

        public virtual Vector2? pickSpot( GameLocation loc )
        {
            foreach ( var sl in SpawnLocations )
            {
                var ret = sl.pickSpot(loc);
                if (ret.HasValue)
                    return ret.Value;
            }
            return null;
        }

        public virtual Critter makeCritter(Vector2 pos)
        {
            return new CustomCritter(pos + new Vector2( 1, 1 ) *  (Game1.tileSize / 2), this);
        }

        internal static Dictionary<string, CritterEntry> critters = new Dictionary<string, CritterEntry>();
        public static void Register( CritterEntry entry )
        {
            critters.Add(entry.Id, entry);
        }
    }
}
