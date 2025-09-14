using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;

namespace SpaceCore.VanillaAssetExpansion
{
    public class FarmExtensionData
    {
        public class BuildingData
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public Vector2 Position { get; set; }
            public Dictionary<string, string> Animals { get; set; }
        }

        public class FenceData
        {
            public string Id { get; set; }
            public string FenceId { get; set; }
            public Rectangle Area { get; set; }
            public bool IsGate { get; set; }
        }

        public List<BuildingData> Buildings { get; set; } = new();
        public List<FenceData> Fences { get; set; } = new();
    }

    [HarmonyPatch(typeof(Farm), nameof(Farm.onNewGame))]
    public static class FarmStartersPatch
    {
        public static void Postfix(Farm __instance)
        {
            var dict = Game1.content.Load<Dictionary<string, FarmExtensionData>>("spacechase0.SpaceCore/FarmExtensionData");
            string keyToCheck = Game1.whichModFarm?.Id;
            if(string.IsNullOrEmpty(keyToCheck))
            {
                //same values as CP's {{FarmType}} token for ease of use
                keyToCheck = new[] { "Standard", "Beach", "FourCorners", "Forest", "HillTop", "Riverland", "Wilderness" }[Game1.whichFarm];
            }
            if (!dict.ContainsKey(keyToCheck))
                return;
            var fdata = dict[keyToCheck];

            foreach (var bfdata in fdata.Buildings)
            {
                var building = Building.CreateInstanceFromId(bfdata.Type, bfdata.Position);
                if (building == null)
                    continue;
                building.FinishConstruction(true);
                building.LoadFromBuildingData(building.GetData(), false, true);

                foreach (var animalPair in bfdata.Animals)
                {
                    FarmAnimal animal = new FarmAnimal(animalPair.Value, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
                    animal.Name = animalPair.Key;
                    (building.GetIndoors() as AnimalHouse).adoptAnimal(animal);
                }

                __instance.buildings.Add(building);
            }

            foreach (var fence in fdata.Fences)
            {
                for (int ix = fence.Area.X; ix < fence.Area.X + fence.Area.Width; ++ix)
                {
                    for (int iy = fence.Area.Y; iy < fence.Area.Y + fence.Area.Height; ++iy)
                    {
                        Vector2 pos = new Vector2(ix, iy);
                        if (__instance.objects.ContainsKey(pos))
                        {
                            __instance.objects.Remove(pos);
                        }
                        __instance.objects.Add(pos, new Fence(new(ix, iy), fence.FenceId, fence.IsGate));
                    }
                }
            }
        }
    }
}
