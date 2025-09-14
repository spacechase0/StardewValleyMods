using DynamicGameAssets.Game;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Automate;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;

namespace DGAAutomate
{
    internal class MyAutomationFactory : IAutomationFactory
    {
        public IAutomatable GetFor(StardewValley.Object obj, GameLocation location, in Vector2 tile)
        {
            if (obj is CustomBigCraftable cbig)
            {
                if (Mod.CustomMachineRecipes.ContainsKey(cbig.FullId))
                    return new MyMachine(cbig, location, tile);
            }

            return null;
        }

        public IAutomatable GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile)
        {
            return null;
        }

        public IAutomatable GetFor(Building building, BuildableGameLocation location, in Vector2 tile)
        {
            return null;
        }

        public IAutomatable GetForTile(GameLocation location, in Vector2 tile)
        {
            return null;
        }
    }
}
