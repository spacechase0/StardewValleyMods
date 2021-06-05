using System.Collections.Generic;
using Harmony;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace BuildableLocationsFramework.Patches
{
    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.loadDataToLocations))]
    public static class SaveGameLoadDataToLocationsPatch
    {
        internal static List<GameLocation> locs;

        public static void Prefix( List<GameLocation> gamelocations )
        {
            locs = gamelocations;

            foreach ( GameLocation gamelocation in gamelocations )
            {
                if ( gamelocation.Name == "Farm" )
                    continue;
                if ( gamelocation is BuildableGameLocation bgl )
                {
                    GameLocation locationFromName = Game1.getLocationFromName((string)(NetFieldBase<string, NetString>)gamelocation.name);
                    foreach ( Building building in ( ( BuildableGameLocation ) gamelocation ).buildings )
                        building.load();
                    ( ( BuildableGameLocation ) locationFromName ).buildings.Set( ( ICollection<Building> ) ( ( BuildableGameLocation ) gamelocation ).buildings );
                }
                else if ( gamelocation is IAnimalLocation al )
                {
                    foreach ( FarmAnimal farmAnimal in al.Animals.Values )
                        farmAnimal.reload( ( Building ) null );
                }
            }

        }

        public static void Postfix()
        {
            locs = null;
        }
    }
}
