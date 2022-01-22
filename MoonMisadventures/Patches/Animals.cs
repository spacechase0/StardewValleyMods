using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using MoonMisadventures.Game;
using MoonMisadventures.Game.Locations;
using StardewValley;
using StardewValley.Tools;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch( typeof( MilkPail ), nameof( MilkPail.beginUsing ) )]
    public static class MilkPailProduceGalaxyMilkPatch1
    {
        public static bool Prefix( MilkPail __instance, GameLocation location, int x, int y, Farmer who, ref bool __result, ref FarmAnimal ___animal )
        {
            if (location is not LunarLocation)
                return true;

            x = ( int ) who.GetToolLocation().X;
            y = ( int ) who.GetToolLocation().Y;
            Rectangle r = new Rectangle(x - 32, y - 32, 64, 64);
            if ( location is Farm )
            {
                ___animal = Utility.GetBestHarvestableFarmAnimal( ( location as Farm ).animals.Values, __instance, r );
            }
            else if ( location is AnimalHouse )
            {
                ___animal = Utility.GetBestHarvestableFarmAnimal( ( location as AnimalHouse ).animals.Values, __instance, r );
            }
            else if ( location is IAnimalLocation aloc )
            {
                ___animal = Utility.GetBestHarvestableFarmAnimal( aloc.Animals.Values, __instance, r );
            }
            if ( ___animal != null && ( int ) ___animal.currentProduce > 0 && ( int ) ___animal.age >= ( byte ) ___animal.ageWhenMature && ___animal.toolUsedForHarvest.Equals( __instance.BaseName ) && who.couldInventoryAcceptThisObject( ___animal.currentProduce, 1 ) )
            {
                ___animal.doEmote( 20 );
                ___animal.friendshipTowardFarmer.Value = Math.Min( 1000, ( int ) ___animal.friendshipTowardFarmer + 5 );
                who.currentLocation.localSound( "Milking" );
                ___animal.pauseTimer = 1500;
            }
            else if ( ___animal != null && ( int ) ___animal.currentProduce > 0 && ( int ) ___animal.age >= ( byte ) ___animal.ageWhenMature )
            {
                if ( who != null && Game1.player.Equals( who ) )
                {
                    if ( !___animal.toolUsedForHarvest.Equals( __instance.BaseName ) )
                    {
                        if ( !( ___animal.toolUsedForHarvest == null ) && !___animal.toolUsedForHarvest.Equals( "null" ) )
                        {
                            Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:MilkPail.cs.14167", ___animal.toolUsedForHarvest ) );
                        }
                    }
                    else if ( !who.couldInventoryAcceptThisObject( ___animal.currentProduce, 1 ) )
                    {
                        Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:Crop.cs.588" ) );
                    }
                }
            }
            else if ( who != null && Game1.player.Equals( who ) )
            {
                DelayedAction.playSoundAfterDelay( "fishingRodBend", 300 );
                DelayedAction.playSoundAfterDelay( "fishingRodBend", 1200 );
                string toSay = "";
                if ( ___animal != null && !___animal.toolUsedForHarvest.Equals( __instance.BaseName ) )
                {
                    toSay = Game1.content.LoadString( "Strings\\StringsFromCSFiles:MilkPail.cs.14175", ___animal.displayName );
                }
                if ( ___animal != null && ___animal.isBaby() && ___animal.toolUsedForHarvest.Equals( __instance.BaseName ) )
                {
                    toSay = Game1.content.LoadString( "Strings\\StringsFromCSFiles:MilkPail.cs.14176", ___animal.displayName );
                }
                if ( ___animal != null && ( int ) ___animal.age >= ( byte ) ___animal.ageWhenMature && ___animal.toolUsedForHarvest.Equals( __instance.BaseName ) )
                {
                    toSay = Game1.content.LoadString( "Strings\\StringsFromCSFiles:MilkPail.cs.14177", ___animal.displayName );
                }
                if ( toSay.Length > 0 )
                {
                    DelayedAction.showDialogueAfterDelay( toSay, 1000 );
                }
            }
            who.Halt();
            int g = who.FarmerSprite.CurrentFrame;
            who.FarmerSprite.animateOnce( 287 + who.FacingDirection, 50f, 4 );
            who.FarmerSprite.oldFrame = g;
            who.UsingTool = true;
            who.CanMove = false;
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch( typeof( MilkPail ), nameof( MilkPail.DoFunction ) )]
    public static class MilkPailProduceGalaxyMilkPatch2
    {
        public static bool Prefix( MilkPail __instance, GameLocation location, int x, int y, int power, Farmer who, FarmAnimal ___animal, ref Farmer ___lastUser )
        {
            if ( ___animal is LunarAnimal lanimal && location is LunarLocation )
            {
                baseDoFunction( __instance, location, x, y, power, who, ref ___lastUser );
                who.Stamina -= 4f;
                __instance.CurrentParentTileIndex = 6;
                __instance.IndexOfMenuItemView = 6;
                if ( ___animal != null && ( int ) ___animal.currentProduce > 0 && ( int ) ___animal.age >= ( byte ) ___animal.ageWhenMature && ___animal.toolUsedForHarvest.Equals( __instance.BaseName ) && who.addItemToInventoryBool( new CustomObject( DynamicGameAssets.Mod.Find( ItemIds.GalaxyMilk ) as ObjectPackData )
                {
                    Quality = ___animal.produceQuality
                } ) )
                {
                    Utility.RecordAnimalProduce( ___animal, ___animal.currentProduce );
                    Game1.playSound( "coin" );
                    ___animal.currentProduce.Value = -1;
                    who.gainExperience( 0, 5 );
                }
                Mod.instance.Helper.Reflection.GetMethod( __instance, "finish" ).Invoke();
                return false;
            }

            return true;
        }

        private static void baseDoFunction( Tool __instance, GameLocation location, int x, int y, int power, Farmer who, ref Farmer ___lastUser )
        {
            ___lastUser = who;
            Game1.recentMultiplayerRandom = new Random( ( short ) Game1.random.Next( -32768, 32768 ) );
            ToolFactory.getIndexFromTool( __instance );
            if ( __instance.isHeavyHitter() && !( __instance is MeleeWeapon ) )
            {
                Rumble.rumble( 0.1f + ( float ) ( Game1.random.NextDouble() / 4.0 ), 100 + Game1.random.Next( 50 ) );
                location.damageMonster( new Rectangle( x - 32, y - 32, 64, 64 ), ( int ) __instance.upgradeLevel + 1, ( ( int ) __instance.upgradeLevel + 1 ) * 3, isBomb: false, who );
            }
            if ( __instance is MeleeWeapon && ( !who.UsingTool || Game1.mouseClickPolling >= 50 || ( int ) ( __instance as MeleeWeapon ).type == 1 || ( __instance as MeleeWeapon ).InitialParentTileIndex == 47 || MeleeWeapon.timedHitTimer > 0 || who.FarmerSprite.currentAnimationIndex != 5 || !( who.FarmerSprite.timer < who.FarmerSprite.interval / 4f ) ) )
            {
                if ( ( int ) ( __instance as MeleeWeapon ).type == 2 && ( __instance as MeleeWeapon ).isOnSpecial )
                {
                    ( __instance as MeleeWeapon ).triggerClubFunction( who );
                }
                else if ( who.FarmerSprite.currentAnimationIndex > 0 )
                {
                    MeleeWeapon.timedHitTimer = 500;
                }
            }
        }
    }
}
