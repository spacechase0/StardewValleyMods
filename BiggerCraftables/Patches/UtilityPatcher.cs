using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace BiggerCraftables.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Utility"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class UtilityPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Utility>(nameof(Utility.playerCanPlaceItemHere)),
                prefix: this.GetHarmonyMethod(nameof(Before_PlayersCanPlaceItemHere))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Utility.playerCanPlaceItemHere"/>.</summary>
        private static bool Before_PlayersCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
        {
            if (!(item is StardewValley.Object obj && obj.bigCraftable.Value))
                return true;
            var entry = Mod.Entries.SingleOrDefault(cle => cle.Name == obj.Name);
            if (entry == null)
                return true;

            if (Utility.isPlacementForbiddenHere(location))
            {
                __result = false;
                return false;
            }
            if (item == null || item is Tool || Game1.eventUp || (bool)f.bathingClothes || f.onBridge.Value)
            {
                __result = false;
                return false;
            }
            bool withinRadius = false;
            Vector2 tileLocation = new Vector2(x / 64, y / 64);
            Vector2 playerTile = f.getTileLocation();
            for (int ix = (int)tileLocation.X; ix < (int)tileLocation.X + entry.Width; ++ix)
            {
                for (int iy = (int)tileLocation.Y; iy < (int)tileLocation.Y + entry.Length; ++iy)
                {
                    if (Math.Abs(ix - playerTile.X) <= 1 && Math.Abs(iy - playerTile.Y) <= 1)
                    {
                        withinRadius = true;
                    }
                }
            }

            if (withinRadius || (item is Wallpaper && location is DecoratableLocation) || (item is Furniture furniture && location.CanPlaceThisFurnitureHere(furniture)))
            {
                if (item.canBePlacedHere(location, tileLocation))
                {
                    if (!((StardewValley.Object)item).isPassable())
                    {
                        foreach (Farmer farmer in location.farmers)
                        {
                            for (int ix = (int)tileLocation.X; ix < (int)tileLocation.X + entry.Width; ++ix)
                            {
                                for (int iy = (int)tileLocation.Y; iy < (int)tileLocation.Y + entry.Length; ++iy)
                                {
                                    if (farmer.GetBoundingBox().Intersects(new Rectangle(ix * 64, iy * 64, 64, 64)))
                                    {
                                        __result = false;
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    var itemCanBePlaced = Mod.Instance.Helper.Reflection.GetMethod(typeof(Utility), "itemCanBePlaced");
                    if (itemCanBePlaced.Invoke<bool>(location, tileLocation, item) || Utility.isViableSeedSpot(location, tileLocation, item))
                    {
                        __result = true;
                        return false;
                    }
                }
            }

            __result = false;
            return false;
        }
    }
}
