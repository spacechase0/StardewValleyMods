using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JsonAssets.Data;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace JsonAssets.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="Item"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class ItemPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Item>(nameof(Item.canBeDropped)),
                postfix: this.GetHarmonyMethod(nameof(After_CanBeDropped))
            );

            harmony.Patch(
                original: this.RequireMethod<Item>(nameof(Item.canBeTrashed)),
                postfix: this.GetHarmonyMethod(nameof(After_CanBeTrashed))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Item.canBeDropped"/>.</summary>
        private static void After_CanBeDropped(Item __instance, ref bool __result)
        {
            try
            {
                if (__instance is StardewValley.Object obj)
                {
                    if (!obj.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(obj.ParentSheetIndex))
                    {
                        var objData = new List<ObjectData>(Mod.instance.objects).Find(od => od.GetObjectId() == obj.ParentSheetIndex);
                        if (objData != null && !objData.CanTrash)
                            __result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(After_CanBeDropped)} for {__instance} #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }

        /// <summary>The method to call after <see cref="Item.canBeTrashed"/>.</summary>
        private static void After_CanBeTrashed(Item __instance, ref bool __result)
        {
            try
            {
                if (__instance is StardewValley.Object obj)
                {
                    if (!obj.bigCraftable.Value && Mod.instance.objectIds.Values.Contains(obj.ParentSheetIndex))
                    {
                        var objData = new List<ObjectData>(Mod.instance.objects).Find(od => od.GetObjectId() == obj.ParentSheetIndex);
                        if (objData != null && !objData.CanTrash)
                            __result = false;
                    }
                }
                else if (__instance is MeleeWeapon weapon)
                {
                    if (Mod.instance.weaponIds.Values.Contains(weapon.ParentSheetIndex))
                    {
                        var weaponData = new List<WeaponData>(Mod.instance.weapons).Find(wd => wd.GetWeaponId() == weapon.ParentSheetIndex);
                        if (weaponData != null && !weaponData.CanTrash)
                            __result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(After_CanBeTrashed)} for {__instance} #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }
    }
}
