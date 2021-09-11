using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="IndoorPot"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class IndoorPotPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<IndoorPot>(nameof(IndoorPot.performObjectDropInAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformObjectDropInAction))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="IndoorPot.performObjectDropInAction"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_PerformObjectDropInAction(IndoorPot __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
        {
            if (dropInItem is CustomObject obj && !string.IsNullOrEmpty(obj.Data.Plants))
            {
                __result = IndoorPotPatcher.Impl(__instance, obj, probe, who);
                return false;
            }
            return true;
        }

        private static bool Impl(IndoorPot this_, CustomObject dropInItem, bool probe, Farmer who)
        {
            if (who != null && dropInItem != null && this_.bush.Value == null && dropInItem.CanPlantThisSeedHere(this_.hoeDirt.Value, (int)this_.tileLocation.X, (int)this_.tileLocation.Y, dropInItem.Category == -19))
            {
                //if ((int)dropInItem.parentSheetIndex == 805)
                //{
                //    if (!probe)
                //    {
                //        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                //    }
                //    return false;
                //}
                //if ((int)dropInItem.parentSheetIndex == 499)
                //{
                //    if (!probe)
                //    {
                //        Game1.playSound("cancel");
                //        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Objects:AncientFruitPot"));
                //    }
                //    return false;
                //}
                if (!probe)
                {
                    if (!dropInItem.Plant(this_.hoeDirt.Value, (int)this_.tileLocation.X, (int)this_.tileLocation.Y, who, dropInItem.Category == -19, who.currentLocation))
                    {
                        return false;
                    }
                }
                else
                {
                    this_.heldObject.Value = new StardewValley.Object();
                }
                return true;
            }
            //if (who != null && dropInItem != null && this_.hoeDirt.Value.crop == null && this_.bush.Value == null && dropInItem is StardewValley.Object && !(dropInItem as StardewValley.Object).bigCraftable && (int)dropInItem.parentSheetIndex == 251)
            //{
            //    if (probe)
            //    {
            //        this_.heldObject.Value = new StardewValley.Object();
            //    }
            //    else
            //    {
            //        this_.bush.Value = new Bush(this_.tileLocation, 3, who.currentLocation);
            //        if (!who.currentLocation.IsOutdoors)
            //        {
            //            this_.bush.Value.greenhouseBush.Value = true;
            //            this_.bush.Value.loadSprite();
            //            Game1.playSound("coin");
            //        }
            //    }
            //    return true;
            //}
            return false;
        }
    }
}
