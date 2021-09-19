using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.countsForShippedCollection)),
                prefix: this.GetHarmonyMethod(nameof(Before_CountsForShippedCollection))
            );
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.isIndexOkForBasicShippedCategory)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsIndexOkForBasicShippedCategory))
            );
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.isSapling)),
                prefix: this.GetHarmonyMethod(nameof(Before_IsSapling))
            );
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.performToolAction)),
                postfix: this.GetHarmonyMethod(nameof(After_PerformToolAction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.countsForShippedCollection"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_CountsForShippedCollection(SObject __instance, ref bool __result)
        {
            if (__instance is CustomObject obj)
            {
                __result = !obj.Data.HideFromShippingCollection;
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="SObject.isIndexOkForBasicShippedCategory"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_IsIndexOkForBasicShippedCategory(int index, ref bool __result)
        {
            if (Mod.itemLookup.ContainsKey(index))
            {
                if (Mod.Find(Mod.itemLookup[index]) is ObjectPackData data) // This means it was disabled
                    __result = !data.HideFromShippingCollection;
                else
                    __result = false;
                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="SObject.isSapling"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_IsSapling(SObject __instance, ref bool __result)
        {
            if (__instance is CustomObject obj && !string.IsNullOrEmpty(obj.Data.Plants))
            {
                var data = Mod.Find(obj.Data.Plants);
                if (data is FruitTreePackData)
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call after <see cref="SObject.performToolAction"/>.</summary>
        private static void After_PerformToolAction(SObject __instance, Tool t, GameLocation location, ref bool __result)
        {
            if (__instance is CustomBigCraftable)
            {
                Vector2 tile = __instance.TileLocation;
                if (tile == Vector2.Zero)
                {
                    KeyValuePair<Vector2, SObject> obj = location.Objects.Pairs.SingleOrDefault(obj => obj.Value == __instance);
                    tile = obj.Value is not null ? obj.Key : Game1.player.GetToolLocation() / 64;
                    tile.X = (int)tile.X;
                    tile.Y = (int)tile.Y;
                }
                if (location.Objects[tile].Type.Equals("Crafting") && location.Objects[tile].Fragility != 2)
                {
                    var debris = new Debris(
                        objectIndex: __instance.ParentSheetIndex,
                        debrisOrigin: Game1.player.GetToolLocation(),
                        playerPosition: new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y))
                    {
                        item = __instance,
                    };
                    location.debris.Add(debris);
                }
                location.Objects[tile].performRemoveAction(tile, location);
                location.Objects.Remove(tile);
                __result = false;
            }
        }
    }
}
