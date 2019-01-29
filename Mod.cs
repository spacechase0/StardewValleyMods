using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace ProfitCalculator
{
    public class Mod : StardewModdingAPI.Mod
    {
        internal class ProfitData
        {
            public int profit = 0;
            public string crop = "invalid";
        }

        public override void Entry(IModHelper helper)
        {
            helper.ConsoleCommands.Add("profits_crops", "Calculate profits for crops", perPlanting);
        }

        private void perPlanting(string cmd, string[] args)
        {
            var season = "";
            if (args.Length >= 1)
                season = args[0];
            Monitor.Log((season == "") ? "Doing for all seasons" : $"Doing for season {season}", LogLevel.Info);
            Monitor.Log("NOTE: This takes into account your farming level", LogLevel.Info);

            var tmp = Game1.player;


            var profits = new List<ProfitData>();
            var objectInfo = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            var cropInfo = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");

            foreach (var crop in cropInfo)
            {
                var seedObjData = objectInfo[crop.Key].Split('/');
                var cropData = crop.Value.Split('/');
                var productObjData = objectInfo[int.Parse(cropData[3])].Split('/');

                var name = productObjData[0];
                var cost = int.Parse(seedObjData[1]);
                var value = int.Parse(productObjData[1]);

                //Monitor.Log("Doing for " + name);
                
                if (season != "" && !cropData[1].Split(' ').Contains(season))
                    continue;

                var phases = cropData[0].Split(' ').Select<string, int>(p => int.Parse(p));
                int total = phases.Sum();
                int regrowth = int.Parse(cropData[4]);

                int profit = (value - cost) * (28 / total);
                if ( regrowth != -1 )
                {
                    int days = 28;
                    int harvests = 0;

                    days -= total + 1; ++harvests;
                    harvests += days / regrowth;
                    //Monitor.Log("harvests for " + name + " " + harvests + " w/ " + total + " " + regrowth);

                    int avgPerHarvest = 1;
                    if ( cropData[6].StartsWith("true ") )
                    {
                        var multiStrs = cropData[6].Split(' ');
                        int min = int.Parse(multiStrs[1]);
                        int max = int.Parse(multiStrs[2]);
                        int bonus = int.Parse(multiStrs[3]);
                        double chance = double.Parse(multiStrs[4]);

                        int farmLevel = 0;
                        if (Game1.player != null)
                            farmLevel = Game1.player.FarmingLevel;

                        int newMax = Math.Min(min + 1, max + 1 + farmLevel / bonus);
                        // TODO: Integrate chance?
                        /*
                    while (random.NextDouble() < Math.Min(0.9, (double)((NetFieldBase<double, NetDouble>)this.chanceForExtraCrops)))
                        ++num1;
                        */

                        avgPerHarvest = (min + max) / 2;
                    }

                    //Monitor.Log($"{name} {value} {harvests} {avgPerHarvest} {cost}");
                    profit = (value * harvests * avgPerHarvest) - cost;
                }

                var data = new ProfitData()
                {
                    profit = profit,
                    crop = name,
                };
                profits.Add(data);
            }
            
            profits.Sort(Comparer<ProfitData>.Create((p1, p2) => p2.profit - p1.profit));
            for ( int i = 0; i < profits.Count; ++i )
            {
                var p = profits[i];
                Monitor.Log($"{i+1}. " + string.Format("{0,-20}", p.crop) + string.Format("{0,10}", p.profit) + "g", LogLevel.Info);
            }
        }
    }
}
