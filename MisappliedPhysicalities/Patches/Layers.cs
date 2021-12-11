using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.Game;
using MisappliedPhysicalities.Game.Objects;
using MisappliedPhysicalities.VirtualProperties;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MisappliedPhysicalities.Patches
{
    [HarmonyPatch( typeof( GameLocation ), "initNetFields" )]
    public static class GameLocationAddNetFieldsPatch
    {
        public static void Postfix( GameLocation __instance )
        {
            __instance.NetFields.AddField( __instance.get_BelowGroundObjects() );
            __instance.NetFields.AddField( __instance.GetConstantProcessingForLayer( Layer.Underground ) );
            __instance.NetFields.AddField( __instance.GetConstantProcessingForLayer( Layer.GroundLevel ) );
            __instance.NetFields.AddField( __instance.GetConstantProcessingForLayer( Layer.Elevated ) );

            __instance.get_BelowGroundObjects().OnValueAdded += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.Underground ).Add( key ); };
            __instance.get_BelowGroundObjects().OnValueRemoved += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.Underground ).Remove( key ); };
            __instance.netObjects.OnValueAdded += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.GroundLevel ).Add( key ); };
            __instance.netObjects.OnValueRemoved += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.GroundLevel ).Remove( key ); };
            __instance.get_ElevatedObjects().OnValueAdded += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.Elevated ).Add( key ); };
            __instance.get_ElevatedObjects().OnValueRemoved += ( key, value ) => { if ( value is IUpdatesEvenWithoutFarmer ) __instance.GetConstantProcessingForLayer( Layer.Elevated ).Remove( key ); };
        }
    }

    [HarmonyPatch( typeof( GameLocation ), nameof( GameLocation.updateEvenIfFarmerIsntHere ) )]
    public static class GameLocationUpdateWithoutFarmerPatch
    {
        public static void Postfix( GameLocation __instance, GameTime time )
        {
            var objs = __instance.get_BelowGroundObjects();
            foreach ( var vec in __instance.GetConstantProcessingForLayer( Layer.Underground ) )
                ( objs[ vec ] as IUpdatesEvenWithoutFarmer ).UpdateEvenWithoutFarmer( __instance, time );

            objs = __instance.netObjects;
            foreach ( var vec in __instance.GetConstantProcessingForLayer( Layer.GroundLevel ) )
                ( objs[ vec ] as IUpdatesEvenWithoutFarmer ).UpdateEvenWithoutFarmer( __instance, time );

            objs = __instance.get_ElevatedObjects();
            foreach ( var vec in __instance.GetConstantProcessingForLayer( Layer.Elevated ) )
                ( objs[ vec ] as IUpdatesEvenWithoutFarmer ).UpdateEvenWithoutFarmer( __instance, time );
        }
    }

    [HarmonyPatch( typeof( GameLocation ), nameof( GameLocation.drawFloorDecorations ) )]
    public static class GameLocationDrawNothingWithGogglesFloorPatch
    {
        public static bool Prefix( GameLocation __instance, SpriteBatch b )
        {
            if ( !Game1.eventUp && Mod.dga.GetDGAItemId( Game1.player.hat.Value ) == Items.XrayGogglesId )
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( GameLocation ), nameof( GameLocation.drawAboveFrontLayer ) )]
    public static class GameLocationDrawNothingWithGogglesTFPatch
    {
        public static bool Prefix( GameLocation __instance, SpriteBatch b )
        {
            if ( !Game1.eventUp && Mod.dga.GetDGAItemId( Game1.player.hat.Value ) == Items.XrayGogglesId )
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( GameLocation ), nameof( GameLocation.draw ) )]
    public static class GameLocationDrawUndergroundPatch
    {
        public static bool Prefix( GameLocation __instance, SpriteBatch b )
        {
            if ( !Game1.eventUp && Mod.dga.GetDGAItemId( Game1.player.hat.Value ) == Items.XrayGogglesId )
            {
                DoImpl( __instance, b );
                return false;
            }
            else
            {
                var mineshaftTex = Game1.content.Load< Texture2D >( "Maps/Mines/mine_desert" );

                var below = __instance.get_BelowGroundObjects();
                Vector2 tile = default(Vector2);
                for ( int y = Game1.viewport.Y / 64 - 1; y < ( Game1.viewport.Y + Game1.viewport.Height ) / 64 + 3; y++ )
                {
                    for ( int x = Game1.viewport.X / 64 - 1; x < ( Game1.viewport.X + Game1.viewport.Width ) / 64 + 1; x++ )
                    {
                        tile.X = x;
                        tile.Y = y;
                        if ( below.ContainsKey( tile ) && !__instance.Objects.ContainsKey( tile ) )
                        {
                            var pos = Game1.GlobalToLocal( Game1.viewport, new Vector2( x * 64, y * 64 ) );
                            b.Draw( mineshaftTex, pos, new Rectangle( 224, 160, 16, 16 ), Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, pos.Y / 20000f );
                        }
                    }
                }
                // TODO: Elevated draw
            }

            return true;
        }

        private static void DoImpl( GameLocation loc, SpriteBatch b )
        {
            Mod.instance.Helper.Reflection.GetMethod( loc, "drawFarmers" ).Invoke( b );

            var below = loc.get_BelowGroundObjects();

            Vector2 tile = default(Vector2);
            for ( int y = Game1.viewport.Y / 64 - 1; y < ( Game1.viewport.Y + Game1.viewport.Height ) / 64 + 3; y++ )
            {
                for ( int x = Game1.viewport.X / 64 - 1; x < ( Game1.viewport.X + Game1.viewport.Width ) / 64 + 1; x++ )
                {
                    tile.X = x;
                    tile.Y = y;
                    if ( below.ContainsKey( tile ) )
                    {
                        byte drawSum = 0;
                        if ( below.ContainsKey( tile + new Vector2( -1, 0 ) ) )
                            drawSum += 8;
                        if ( below.ContainsKey( tile + new Vector2( 1, 0 ) ) )
                            drawSum += 2;
                        if ( below.ContainsKey( tile + new Vector2( 0, -1 ) ) )
                            drawSum += 1;
                        if ( below.ContainsKey( tile + new Vector2( 0, 1 ) ) )
                            drawSum += 4;

                        int sri = HoeDirt.drawGuide[ drawSum ];
                        Rectangle sr = new Rectangle(sri % 4 * 16, sri / 4 * 16, 16, 16);

                        var pos = Game1.GlobalToLocal( Game1.viewport, new Vector2( x * 64, y * 64 ) );
                        b.Draw( HoeDirt.darkTexture, pos, sr, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, pos.Y / 20000f );

                        var o = below[ tile ];
                        if ( o is not NullObject )
                            o.draw( b, ( int ) tile.X, ( int ) tile.Y );
                    }
                }
            }
        }
    }
}
