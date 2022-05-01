using System;
using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;
using StardewValley.Objects;

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
                original: this.RequireMethod<SObject>(nameof(SObject.performObjectDropInAction)),
                postfix: this.GetHarmonyMethod(nameof(After_PerformObjectDropInAction))
            );
            harmony.Patch(
                original: this.RequireMethod<SObject>("loadDisplayName"),
                postfix: this.GetHarmonyMethod(nameof(After_LoadDisplayName))
            );
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: this.GetHarmonyMethod(nameof(After_Draw))
            );
            harmony.Patch(
                original: this.RequireMethod<CrabPot>(nameof(CrabPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: this.GetHarmonyMethod(nameof(After_Draw))
            );
            harmony.Patch(
                original: this.RequireMethod<WoodChipper>(nameof(WoodChipper.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: this.GetHarmonyMethod(nameof(After_Draw))
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

        /// <summary>The method to call after <see cref="SObject.performObjectDropInAction"/>.</summary>
        private static void After_PerformObjectDropInAction(SObject __instance, Item dropInItem, bool probe, Farmer who)
        {
            if (dropInItem is CustomObject dropIn)
            {
                if (__instance.name.Equals("Keg")) {
                    switch (dropIn.Category)
                    {
                        case -75:
                            __instance.heldObject.Value = new SObject(Vector2.Zero, 350, dropIn.Name + " Juice", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
                            __instance.heldObject.Value.Price = (int)((double)dropIn.Price * 2.25);
                            if (!probe)
                            {
                                __instance.heldObject.Value.name = dropIn.Name + " Juice";
                                __instance.heldObject.Value.preserve.Value = SObject.PreserveType.Juice;
                                __instance.heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
                                __instance.heldObject.Value.modData["spacechase0.DynamicGameAssets/preserved-parent-ID"] = dropIn.FullId;
                            }
                            return;
                        case -79:
                            __instance.heldObject.Value = new SObject(Vector2.Zero, 348, dropIn.Name + " Wine", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
                            __instance.heldObject.Value.Price = dropIn.Price * 3;
                            if (!probe)
                            {
                                __instance.heldObject.Value.name = dropIn.Name + " Wine";
                                __instance.heldObject.Value.preserve.Value = SObject.PreserveType.Wine;
                                __instance.heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
                                __instance.heldObject.Value.modData["spacechase0.DynamicGameAssets/preserved-parent-ID"] = dropIn.FullId;
                            }
                            return;
                    }
                }
                else if (__instance.name.Equals("Preserves Jar"))
                {
                    switch (dropIn.Category)
                    {
                        case -75:
                            __instance.heldObject.Value = new SObject(Vector2.Zero, 342, "Pickled " + dropIn.Name, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
                            __instance.heldObject.Value.Price = 50 + dropIn.Price * 2;
                            if (!probe)
                            {
                                __instance.heldObject.Value.name = "Pickled " + dropIn.Name;
                                __instance.heldObject.Value.preserve.Value = SObject.PreserveType.Pickle;
                                __instance.heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
                                __instance.heldObject.Value.modData["spacechase0.DynamicGameAssets/preserved-parent-ID"] = dropIn.FullId;
                            }
                            return;
                        case -79:
                            __instance.heldObject.Value = new SObject(Vector2.Zero, 344, dropIn.Name + " Jelly", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
                            __instance.heldObject.Value.Price = 50 + dropIn.Price * 2;
                            if (!probe)
                            {
                                __instance.minutesUntilReady.Value = 4000;
                                __instance.heldObject.Value.name = dropIn.Name + " Jelly";
                                __instance.heldObject.Value.preserve.Value = SObject.PreserveType.Jelly;
                                __instance.heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
                                __instance.heldObject.Value.modData["spacechase0.DynamicGameAssets/preserved-parent-ID"] = dropIn.FullId;
                            }
                            return;
                    }
                }
            }
        }

        /// <summary>The method to call after <see cref="SObject.loadDisplayName"/>.</summary>
        private static void After_LoadDisplayName(SObject __instance, ref string __result)
        {
            if (!__instance.preserve.Value.HasValue)
            {
                return;
            }
            
            if (__instance.modData.ContainsKey("spacechase0.DynamicGameAssets/preserved-parent-ID"))
            {
                string dga_parent_ID = __instance.modData["spacechase0.DynamicGameAssets/preserved-parent-ID"];
                if (Mod.Find(dga_parent_ID).ToItem() is CustomObject parentItem)
                {
                    switch (__instance.preserve.Value)
                    {
                        case SObject.PreserveType.Wine:
                            __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12730", parentItem.DisplayName);
                            return;
                        case SObject.PreserveType.Jelly:
                            __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12739", parentItem.DisplayName);
                            return;
                        case SObject.PreserveType.Pickle:
                            __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12735", parentItem.DisplayName);
                            return;
                        case SObject.PreserveType.Juice:
                            __result = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12726", parentItem.DisplayName);
                            return;
                        default:
                            break;
                    }
                }
            }
            return;
        }

        /// <summary>The method to call after <see cref="SObject.draw(SpriteBatch, int, int, float)"/>.</summary>
        private static void After_Draw(SObject __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!__instance.readyForHarvest)
            {
                return;
            }
            // Only get vanilla big craftables, who are ready for harvest, and have a held object
            if ((bool)__instance.bigCraftable && __instance.heldObject.Value != null)
            {
                if (__instance is not CustomBigCraftable && __instance.heldObject.Value is CustomObject custObj)
                {
                    float base_sort = (float)((y + 1) * 64) / 10000f + __instance.tileLocation.X / 50000f;
                    if ((int)__instance.parentSheetIndex == 105 || (int)__instance.parentSheetIndex == 264)
                    {
                        base_sort += 0.02f;
                    }
                    float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                    Vector2 custObjPosition = new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset);
                    custObj.drawWhenProduced(spriteBatch, custObjPosition, base_sort + 1E-05f);
                }
            }
            return;
        }
    }
}
