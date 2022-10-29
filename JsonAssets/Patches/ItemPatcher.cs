using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HarmonyLib;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Tools;

using SObject = StardewValley.Object;

namespace JsonAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Item"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ItemPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
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
                if (__instance is SObject obj && __result && !obj.bigCraftable.Value)
                {
                    var objData = Mod.instance.Objects.FirstOrDefault(od => od.GetObjectId() == obj.ParentSheetIndex);
                    if (objData?.CanTrash == false)
                        __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_CanBeDropped)} for {__instance} #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }

        /// <summary>The method to call after <see cref="Item.canBeTrashed"/>.</summary>
        private static void After_CanBeTrashed(Item __instance, ref bool __result)
        {
            if (!__result)
                return;

            try
            {
                if (__instance is SObject obj)
                {
                    if (!obj.bigCraftable.Value && Mod.instance.ObjectIds.Values.Contains(obj.ParentSheetIndex))
                    {
                        var objData = Mod.instance.Objects.FirstOrDefault(od => od.GetObjectId() == obj.ParentSheetIndex);
                        if (objData?.CanTrash == false)
                            __result = false;
                    }
                }
                else if (__instance is MeleeWeapon weapon)
                {
                    var weaponData = Mod.instance.Weapons.FirstOrDefault(wd => wd.GetWeaponId() == weapon.ParentSheetIndex);
                    if (weaponData?.CanTrash == false)
                        __result = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(After_CanBeTrashed)} for {__instance} #{__instance?.ParentSheetIndex} {__instance?.Name}:\n{ex}");
            }
        }
    }
}
