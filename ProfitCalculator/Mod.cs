using System;
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
            foreach (var crop in Game1.cropData)
            {
                if (season != "" && !crop.Value.Seasons.Select(s => s.ToString().ToLower()).Contains(season.ToLower()))
                    continue;

                var seed = ItemRegistry.Create(crop.Key);
                var produced = ItemRegistry.Create(crop.Value.HarvestItemId);

                string name = produced.DisplayName;
                int cost = seed.salePrice();
                int value = produced.sellToStorePrice();

                int totalPhases = crop.Value.DaysInPhase.Sum();
                float avgPerHarvest = (crop.Value.HarvestMinStack + crop.Value.HarvestMaxStack) / 2f;

                float profit;
                int regrowth = crop.Value.RegrowDays;
                if (regrowth != -1)
                {
                    int days = 28;
                    int harvests = 0;

                    days -= totalPhases + 1; ++harvests;
                    harvests += days / regrowth;
                    profit = value * harvests * avgPerHarvest - cost;
                }
                else {
                    profit = (value * avgPerHarvest - cost) * (28 / totalPhases);
                }

                var data = new ProfitData
                {
                    Profit = (int)Math.Floor(profit),
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
