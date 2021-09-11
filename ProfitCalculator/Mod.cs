using System.Collections.Generic;
using System.Linq;
using ProfitCalculator.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace ProfitCalculator
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            helper.ConsoleCommands.Add("profits_crops", "Calculate profits for crops", this.PerPlanting);
        }

        private void PerPlanting(string cmd, string[] args)
        {
            string season = "";
            if (args.Length >= 1)
                season = args[0];
            Log.Info((season == "") ? "Doing for all seasons" : $"Doing for season {season}");
            Log.Info("NOTE: This takes into account your farming level");

            var profits = new List<ProfitData>();
            var objectInfo = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            var cropInfo = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");

            foreach (var crop in cropInfo)
            {
                string[] seedObjData = objectInfo[crop.Key].Split('/');
                string[] cropData = crop.Value.Split('/');
                string[] productObjData = objectInfo[int.Parse(cropData[3])].Split('/');

                string name = productObjData[0];
                int cost = int.Parse(seedObjData[1]);
                int value = int.Parse(productObjData[1]);

                //Monitor.Log("Doing for " + name);

                if (season != "" && !cropData[1].Split(' ').Contains(season))
                    continue;

                var phases = cropData[0].Split(' ').Select<string, int>(int.Parse);
                int total = phases.Sum();
                int regrowth = int.Parse(cropData[4]);

                int profit = (value - cost) * (28 / total);
                if (regrowth != -1)
                {
                    int days = 28;
                    int harvests = 0;

                    days -= total + 1; ++harvests;
                    harvests += days / regrowth;
                    //Monitor.Log("harvests for " + name + " " + harvests + " w/ " + total + " " + regrowth);

                    int avgPerHarvest = 1;
                    if (cropData[6].StartsWith("true "))
                    {
                        string[] multiStrs = cropData[6].Split(' ');
                        int min = int.Parse(multiStrs[1]);
                        int max = int.Parse(multiStrs[2]);

                        // TODO: Integrate chance?
                        /*
                        int bonus = int.Parse(multiStrs[3]);
                        double chance = double.Parse(multiStrs[4]);

                        int farmLevel = 0;
                        if (Game1.player != null)
                            farmLevel = Game1.player.FarmingLevel;

                        int newMax = Math.Min(min + 1, max + 1 + farmLevel / (bonus == 0 ? 1 : bonus));

                        while (random.NextDouble() < Math.Min(0.9, (double)((NetFieldBase<double, NetDouble>)this.chanceForExtraCrops)))
                            ++num1;
                        */

                        avgPerHarvest = (min + max) / 2;
                    }

                    //Monitor.Log($"{name} {value} {harvests} {avgPerHarvest} {cost}");
                    profit = (value * harvests * avgPerHarvest) - cost;
                }

                var data = new ProfitData
                {
                    Profit = profit,
                    Crop = name
                };
                profits.Add(data);
            }

            profits.Sort(Comparer<ProfitData>.Create((p1, p2) => p2.Profit - p1.Profit));
            for (int i = 0; i < profits.Count; ++i)
            {
                var p = profits[i];
                Log.Info($"{i + 1}. {p.Crop,-20}{p.Profit,10}g");
            }
        }
    }
}
