using System.Collections.Generic;
using System.Linq;
using SkillPrestige.Logging;
using StardewValley;

namespace SkillPrestige
{
    /// <summary>
    /// Makes any necessary adjustments to the game's strategy for determinations about animal produce.
    /// </summary>
    public static class AnimalProduceHandler
    {
        public static decimal QuantityIncreaseChance { get; set; }

        public static bool ShouldAnimalProductQuanityIncrease()
        {
            var randomizedValue = Game1.random.Next(1, 10);
            return QuantityIncreaseChance * 10 >= randomizedValue;
        }

        public static void HandleSpawnedAnimalProductQuantityIncrease()
        {
            var farmAnimalData = Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals");
            var animalProduce = farmAnimalData.Values.Select(y => y.Split('/')[2])
                .Union(farmAnimalData.Values.Select(y => y.Split('/')[3]));

            animalProduce = animalProduce.Where(x => x != "-1").Distinct().ToList();

            var animalHouseBuildings = Game1.getFarm().buildings.Where(x => x.indoors.Value is AnimalHouse);

            foreach (var produce in animalHouseBuildings.SelectMany(x => x.indoors.Value.objects.Values).Where(x => x.IsSpawnedObject && x.ParentSheetIndex.ToString().IsOneOf(animalProduce)))
            {
                if (ShouldAnimalProductQuanityIncrease())
                {
                    Logger.LogInformation($"Adding 1 to stack of object id: {produce.ParentSheetIndex}: {produce.Type}");
                    produce.addToStack(1);
                }
                //TODO: apply effect after each new day
                //TODO: handle doubling of milk or other farmer interaction doubling
            }
        }

    }
}