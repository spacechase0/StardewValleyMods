using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace CustomCritters.Framework.CritterData
{
    internal class SpawnCondition
    {
        public bool Not { get; set; } = false;
        public string[] Seasons { get; set; } = Array.Empty<string>();
        public string[] Locations { get; set; } = Array.Empty<string>();
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

                    ret = this.ChildrenCombine switch
                    {
                        "and" => ret && childCheck,
                        "or" => ret || childCheck,
                        "xor" => ret ^ childCheck,
                        _ => ret
                    };
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
            else if (this.Seasons != null && this.Seasons.Any() && !this.Seasons.Contains(Game1.currentSeason))
                ret = false;
            else if (this.Locations != null && this.Locations.Any() && !this.Locations.Contains(loc.Name))
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
}
