using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Netcode;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SaveGame"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class SaveGamePatcher : BasePatcher
    {
        /*********
        ** Accessors
        *********/
        internal static List<GameLocation> locs;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.loadDataToLocations)),
                prefix: this.GetHarmonyMethod(nameof(Before_LoadDataToLocations)),
                postfix: this.GetHarmonyMethod(nameof(After_LoadDataToLocations))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SaveGame.loadDataToLocations"/>.</summary>
        private static void Before_LoadDataToLocations(List<GameLocation> gamelocations)
        {
            locs = gamelocations;

            foreach (GameLocation gamelocation in gamelocations)
            {
                if (gamelocation.Name == "Farm")
                    continue;
                if (gamelocation is BuildableGameLocation bgl)
                {
                    GameLocation locationFromName = Game1.getLocationFromName((string)(NetFieldBase<string, NetString>)gamelocation.name);
                    foreach (Building building in ((BuildableGameLocation)gamelocation).buildings)
                        building.load();
                    ((BuildableGameLocation)locationFromName).buildings.Set((ICollection<Building>)((BuildableGameLocation)gamelocation).buildings);
                }
                else if (gamelocation is IAnimalLocation al)
                {
                    foreach (FarmAnimal farmAnimal in al.Animals.Values)
                        farmAnimal.reload((Building)null);
                }
            }
        }

        /// <summary>The method to call after <see cref="SaveGame.loadDataToLocations"/>.</summary>
        private static void After_LoadDataToLocations()
        {
            locs = null;
        }
    }
}
