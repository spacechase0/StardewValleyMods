using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch( typeof( IndoorPot ), nameof( IndoorPot.performObjectDropInAction ) )]
    public static class IndoorPotPerformDropInActionPatch
    {
        public static bool Prefix( IndoorPot __instance, Item dropInItem, bool probe, Farmer who, ref bool __result )
        {
            if ( dropInItem is CustomObject cobj && !string.IsNullOrEmpty( cobj.Data.Plants ) )
            {
                __result = IndoorPotPerformDropInActionPatch.Impl( __instance, cobj, probe, who );
                return false;
            }
            return true;
        }

        private static bool Impl( IndoorPot this_, CustomObject dropInItem, bool probe, Farmer who )
        {
            if ( who != null && dropInItem != null && this_.bush.Value == null && dropInItem.CanPlantThisSeedHere( this_.hoeDirt.Value, ( int ) this_.tileLocation.X, ( int ) this_.tileLocation.Y, dropInItem.Category == -19 ) )
            {
                /*
                if ( ( int ) dropInItem.parentSheetIndex == 805 )
                {
                    if ( !probe )
                    {
                        Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:Object.cs.13053" ) );
                    }
                    return false;
                }
                if ( ( int ) dropInItem.parentSheetIndex == 499 )
                {
                    if ( !probe )
                    {
                        Game1.playSound( "cancel" );
                        Game1.showGlobalMessage( Game1.content.LoadString( "Strings\\Objects:AncientFruitPot" ) );
                    }
                    return false;
                }*/
                if ( !probe )
                {
                    if ( !dropInItem.Plant( this_.hoeDirt.Value, ( int ) this_.tileLocation.X, ( int ) this_.tileLocation.Y, who, dropInItem.Category == -19, who.currentLocation ) )
                    {
                        return false;
                    }
                }
                else
                {
                    this_.heldObject.Value = new StardewValley.Object();
                }
                return true;
            }/*
            if ( who != null && dropInItem != null && this_.hoeDirt.Value.crop == null && this_.bush.Value == null && dropInItem is StardewValley.Object && !( dropInItem as StardewValley.Object ).bigCraftable && ( int ) dropInItem.parentSheetIndex == 251 )
            {
                if ( probe )
                {
                    this_.heldObject.Value = new StardewValley.Object();
                }
                else
                {
                    this_.bush.Value = new Bush( this_.tileLocation, 3, who.currentLocation );
                    if ( !who.currentLocation.IsOutdoors )
                    {
                        this_.bush.Value.greenhouseBush.Value = true;
                        this_.bush.Value.loadSprite();
                        Game1.playSound( "coin" );
                    }
                }
                return true;
            }*/
            return false;
        }
    }
}
