using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( StardewValley.Object ), nameof( StardewValley.Object.performObjectDropInAction ) )]
    public static class ObjectSmeltOresPatch
    {
        public static bool Prefix( StardewValley.Object __instance, Item dropInItem, bool probe, Farmer who, ref bool __result )
        {
            if ( __instance.name == "Furnace" )
            {
                if ( who.IsLocalPlayer && __instance.GetTallyOfObject( who, 382, false ) > 0 && __instance.heldObject.Value == null && dropInItem is IDGAItem ditem && ditem.FullId == ItemIds.MythiciteOre && dropInItem.Stack >= 5 )
                {
                    __instance.heldObject.Value = ( StardewValley.Object ) Mod.dga.SpawnDGAItem(ItemIds.MythiciteBar);
                    __instance.minutesUntilReady.Value = 720;

                    if (probe)
                    {
                        __result = true;
                    }
                    else
                    {
                        who.currentLocation.playSound("furnace");
                        __instance.initializeLightSource(__instance.tileLocation);
                        __instance.showNextIndex.Value = true;
                        // TODO: Don't be lazy and actually do this when Game1.multiplayer is public
                        /*Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(30, this.tileLocation.Value * 64f + new Vector2(0f, -16f), Color.White, 4, flipped: false, 50f, 10, 64, (this.tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
                        {
                            alphaFade = 0.005f
                        });*/
                        __instance.ConsumeInventoryItem(who, 382, 1);
                        dropInItem.Stack -= 5;
                        if (dropInItem.Stack <= 0)
                        {
                            __result = true;
                        }
                        __result = false;
                        return false;
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
