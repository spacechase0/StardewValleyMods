using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Utility"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class UtilityPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.areThereAnyOtherAnimalsWithThisName)),
                prefix: this.GetHarmonyMethod(nameof(Before_AreThereAnyOtherAnimalsWithThisName))
            );

            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.getAnimal)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetAnimal))
            );

            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.fixAllAnimals)),
                prefix: this.GetHarmonyMethod(nameof(Before_FixAllAnimals))
            );

            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.numSilos)),
                postfix: this.GetHarmonyMethod(nameof(After_NumSilos))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Utility.areThereAnyOtherAnimalsWithThisName"/>.</summary>
        private static bool Before_AreThereAnyOtherAnimalsWithThisName(string name, ref bool __result)
        {
            var locs = Mod.GetAllLocations();
            foreach (var loc in locs)
            {
                if (loc is IAnimalLocation animalLocation)
                {
                    foreach (var animal in animalLocation.Animals.Values)
                    {
                        if (animal.displayName == name)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            __result = false;
            return false;
        }

        /// <summary>The method to call before <see cref="Utility.getAnimal"/>.</summary>
        private static bool Before_GetAnimal(long id, ref FarmAnimal __result)
        {
            var locs = Mod.GetAllLocations();
            foreach (var loc in locs)
            {
                if (loc is IAnimalLocation animalLocation)
                {
                    if (animalLocation.Animals.TryGetValue(id, out FarmAnimal animal))
                    {
                        __result = animal;
                        return false;
                    }
                }
            }
            __result = null;
            return false;
        }

        /// <summary>The method to call before <see cref="Utility.fixAllAnimals"/>.</summary>
        private static bool Before_FixAllAnimals()
        {
            if (!Game1.IsMasterGame)
                return false;

            foreach (var loc in Mod.GetAllLocations())
            {
                if (loc is BuildableGameLocation farm && loc is IAnimalLocation farm_animals)
                {
                    foreach (Building building in farm.buildings)
                    {
                        if (building.indoors.Value is AnimalHouse house)
                        {
                            foreach (long id in house.animalsThatLiveHere)
                            {
                                FarmAnimal animal = Utility.getAnimal(id);
                                if (animal != null)
                                {
                                    animal.home = building;
                                    animal.homeLocation.Value = new Vector2(building.tileX.Value, building.tileY.Value);
                                }
                            }
                        }
                    }
                    List<FarmAnimal> farmAnimalList1 = new List<FarmAnimal>();
                    foreach (FarmAnimal allFarmAnimal in UtilityPatcher.GetAllFarmAnimals(farm))
                    {
                        if (allFarmAnimal.home == null)
                            farmAnimalList1.Add(allFarmAnimal);
                    }
                    foreach (FarmAnimal farmAnimal in farmAnimalList1)
                    {
                        NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>.PairsCollection pairs;
                        KeyValuePair<long, FarmAnimal> keyValuePair;
                        foreach (Building building in farm.buildings)
                        {
                            if (building.indoors.Value is AnimalHouse house)
                            {
                                for (int index = house.animals.Count() - 1; index >= 0; --index)
                                {
                                    pairs = house.animals.Pairs;
                                    keyValuePair = pairs.ElementAt(index);
                                    if (keyValuePair.Value.Equals(farmAnimal))
                                    {
                                        NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = house.animals;
                                        pairs = house.animals.Pairs;
                                        keyValuePair = pairs.ElementAt(index);
                                        long key = keyValuePair.Key;
                                        animals.Remove(key);
                                    }
                                }
                            }
                        }
                        for (int index = farm_animals.Animals.Count() - 1; index >= 0; --index)
                        {
                            pairs = farm_animals.Animals.Pairs;
                            keyValuePair = pairs.ElementAt(index);
                            if (keyValuePair.Value.Equals(farmAnimal))
                            {
                                NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = farm_animals.Animals;
                                pairs = farm_animals.Animals.Pairs;
                                keyValuePair = pairs.ElementAt(index);
                                long key = keyValuePair.Key;
                                animals.Remove(key);
                            }
                        }
                    }
                    foreach (Building building in farm.buildings)
                    {
                        if (building.indoors.Value is AnimalHouse house)
                        {
                            for (int index = house.animalsThatLiveHere.Count - 1; index >= 0; --index)
                            {
                                if (Utility.getAnimal(house.animalsThatLiveHere[index]).home != building)
                                    house.animalsThatLiveHere.RemoveAt(index);
                            }
                        }
                    }
                    foreach (FarmAnimal farmAnimal in farmAnimalList1)
                    {
                        foreach (Building building in farm.buildings)
                        {
                            if (building.buildingType.Contains(farmAnimal.buildingTypeILiveIn.Value) && building.indoors.Value is AnimalHouse house && !house.isFull())
                            {
                                farmAnimal.home = building;
                                farmAnimal.homeLocation.Value = new Vector2(building.tileX.Value, building.tileY.Value);
                                farmAnimal.setRandomPosition(farmAnimal.home.indoors.Value);
                                (farmAnimal.home.indoors.Value as AnimalHouse).animals.Add(farmAnimal.myID.Value, farmAnimal);
                                (farmAnimal.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(farmAnimal.myID.Value);
                                break;
                            }
                        }
                    }
                    List<FarmAnimal> farmAnimalList2 = new List<FarmAnimal>();
                    foreach (FarmAnimal farmAnimal in farmAnimalList1)
                    {
                        if (farmAnimal.home == null)
                            farmAnimalList2.Add(farmAnimal);
                    }
                    foreach (FarmAnimal farmAnimal in farmAnimalList2)
                    {
                        farmAnimal.Position = Utility.recursiveFindOpenTileForCharacter(farmAnimal, farm, new Vector2(40f, 40f), 200) * 64f;
                        if (!farm_animals.Animals.ContainsKey(farmAnimal.myID.Value))
                            farm_animals.Animals.Add(farmAnimal.myID.Value, farmAnimal);
                    }
                }
            }
            return false;
        }

        /// <summary>The method to call after <see cref="Utility.numSilos"/>.</summary>
        private static void After_NumSilos(ref int __result)
        {
            Farm farm = Game1.getFarm();
            foreach (var loc in Mod.GetAllLocations())
            {
                if (loc != farm && loc is BuildableGameLocation bgl)
                {
                    foreach (var building in bgl.buildings)
                    {
                        if (building.buildingType.Value == "Silo" && building.daysOfConstructionLeft.Value <= 0)
                            ++__result;
                    }
                }
            }
        }

        private static List<FarmAnimal> GetAllFarmAnimals(BuildableGameLocation loc)
        {
            List<FarmAnimal> list = new List<FarmAnimal>();

            if (loc is IAnimalLocation animalLocation)
                list = animalLocation.Animals.Values.ToList();

            foreach (Building building in loc.buildings)
            {
                if (building.indoors.Value is AnimalHouse animalHouse)
                    list.AddRange(animalHouse.animals.Values);
            }

            return list;
        }
    }
}
