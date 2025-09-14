using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MoonMisadventures
{
    public static class Extensions
    {
        public static void falseExplode( this GameLocation loc, Vector2 tileLocation, int radius, Farmer who, bool damageFarmers = true, int damage_amount = -1)
        {

            bool insideCircle = false;
            loc.updateMap();
            Vector2 currentTile = new Vector2(Math.Min(loc.map.Layers[0].LayerWidth - 1, Math.Max(0f, tileLocation.X - (float)radius)), Math.Min(loc.map.Layers[0].LayerHeight - 1, Math.Max(0f, tileLocation.Y - (float)radius)));
            bool[,] circleOutline = Game1.getCircleOutlineGrid(radius);
            Microsoft.Xna.Framework.Rectangle areaOfEffect = new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - (float)radius) * 64, (int)(tileLocation.Y - (float)radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64);
            if ( damage_amount > 0 )
            {
                loc.damageMonster( areaOfEffect, damage_amount, damage_amount, isBomb: true, who );
            }
            else
            {
                loc.damageMonster( areaOfEffect, radius * 6, radius * 8, isBomb: true, who );
            }
            TemporaryAnimatedSpriteList sprites = new();
            sprites.Add( new TemporaryAnimatedSprite( 23, 9999f, 6, 1, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), flicker: false, ( Game1.random.NextDouble() < 0.5 ) ? true : false )
            {
                light = true,
                lightRadius = radius,
                lightcolor = Color.Black,
                alphaFade = 0.03f - ( float ) radius * 0.003f,
                Parent = loc
            } );
            Mod.instance.Helper.Reflection.GetMethod( loc, "rumbleAndFade" ).Invoke( 300 + radius * 100 );
            if ( damageFarmers )
            {
                if ( damage_amount > 0 )
                {
                    Mod.instance.Helper.Reflection.GetMethod( loc, "damagePlayers" ).Invoke( areaOfEffect, damage_amount );
                }
                else
                {
                    Mod.instance.Helper.Reflection.GetMethod( loc, "damagePlayers" ).Invoke( areaOfEffect, radius * 3 );
                }
            }
            /*
            for ( int k = loc.terrainFeatures.Count() - 1; k >= 0; k-- )
            {
                KeyValuePair<Vector2, TerrainFeature> n = loc.terrainFeatures.Pairs.ElementAt(k);
                if ( n.Value.getBoundingBox( n.Key ).Intersects( areaOfEffect ) && n.Value.performToolAction( null, radius / 2, n.Key, this ) )
                {
                    this.terrainFeatures.Remove( n.Key );
                }
            }
            */
            for ( int j = 0; j < radius * 2 + 1; j++ )
            {
                for ( int l = 0; l < radius * 2 + 1; l++ )
                {
                    if ( j == 0 || l == 0 || j == radius * 2 || l == radius * 2 )
                    {
                        insideCircle = circleOutline[ j, l ];
                    }
                    else if ( circleOutline[ j, l ] )
                    {
                        insideCircle = !insideCircle;
                        if ( !insideCircle )
                        {
                            /*
                            if ( this.objects.ContainsKey( currentTile ) && this.objects[ currentTile ].onExplosion( who, this ) )
                            {
                                this.destroyObject( currentTile, who );
                            }
                            */
                            if ( Game1.random.NextDouble() < 0.45 )
                            {
                                if ( Game1.random.NextDouble() < 0.5 )
                                {
                                    sprites.Add( new TemporaryAnimatedSprite( 362, Game1.random.Next( 30, 90 ), 6, 1, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), flicker: false, ( Game1.random.NextDouble() < 0.5 ) ? true : false )
                                    {
                                        delayBeforeAnimationStart = Game1.random.Next( 700 )
                                    } );
                                }
                                else
                                {
                                    sprites.Add( new TemporaryAnimatedSprite( 5, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), Color.White, 8, flipped: false, 50f )
                                    {
                                        delayBeforeAnimationStart = Game1.random.Next( 200 ),
                                        scale = ( float ) Game1.random.Next( 5, 15 ) / 10f
                                    } );
                                }
                            }
                        }
                    }
                    if ( insideCircle )
                    {
                        loc.explosionAt( currentTile.X, currentTile.Y );
                        /*
                        if ( this.objects.ContainsKey( currentTile ) && this.objects[ currentTile ].onExplosion( who, this ) )
                        {
                            this.destroyObject( currentTile, who );
                        }
                        */
                        if ( Game1.random.NextDouble() < 0.45 )
                        {
                            if ( Game1.random.NextDouble() < 0.5 )
                            {
                                sprites.Add( new TemporaryAnimatedSprite( 362, Game1.random.Next( 30, 90 ), 6, 1, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), flicker: false, ( Game1.random.NextDouble() < 0.5 ) ? true : false )
                                {
                                    delayBeforeAnimationStart = Game1.random.Next( 700 )
                                } );
                            }
                            else
                            {
                                sprites.Add( new TemporaryAnimatedSprite( 5, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), Color.White, 8, flipped: false, 50f )
                                {
                                    delayBeforeAnimationStart = Game1.random.Next( 200 ),
                                    scale = ( float ) Game1.random.Next( 5, 15 ) / 10f
                                } );
                            }
                        }
                        sprites.Add( new TemporaryAnimatedSprite( 6, new Vector2( currentTile.X * 64f, currentTile.Y * 64f ), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance( currentTile, tileLocation ) * 20f ) );
                    }
                    currentTile.Y += 1f;
                    currentTile.Y = Math.Min( loc.map.Layers[ 0 ].LayerHeight - 1, Math.Max( 0f, currentTile.Y ) );
                }
                currentTile.X += 1f;
                currentTile.Y = Math.Min( loc.map.Layers[ 0 ].LayerWidth - 1, Math.Max( 0f, currentTile.X ) );
                currentTile.Y = tileLocation.Y - ( float ) radius;
                currentTile.Y = Math.Min( loc.map.Layers[ 0 ].LayerHeight - 1, Math.Max( 0f, currentTile.Y ) );
            }
            var Game1_multiplayer = Mod.instance.Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
            Game1_multiplayer.broadcastSprites( loc, sprites );
            radius /= 2;
            circleOutline = Game1.getCircleOutlineGrid( radius );
            currentTile = new Vector2( ( int ) ( tileLocation.X - ( float ) radius ), ( int ) ( tileLocation.Y - ( float ) radius ) );
            /*
            for ( int i = 0; i < radius * 2 + 1; i++ )
            {
                for ( int m = 0; m < radius * 2 + 1; m++ )
                {
                    if ( i == 0 || m == 0 || i == radius * 2 || m == radius * 2 )
                    {
                        insideCircle = circleOutline[ i, m ];
                    }
                    else if ( circleOutline[ i, m ] )
                    {
                        insideCircle = !insideCircle;
                        if ( !insideCircle && !this.objects.ContainsKey( currentTile ) && Game1.random.NextDouble() < 0.9 && this.doesTileHaveProperty( ( int ) currentTile.X, ( int ) currentTile.Y, "Diggable", "Back" ) != null && !this.isTileHoeDirt( currentTile ) )
                        {
                            this.checkForBuriedItem( ( int ) currentTile.X, ( int ) currentTile.Y, explosion: true, detectOnly: false, who );
                            this.makeHoeDirt( currentTile );
                        }
                    }
                    if ( insideCircle && !this.objects.ContainsKey( currentTile ) && Game1.random.NextDouble() < 0.9 && this.doesTileHaveProperty( ( int ) currentTile.X, ( int ) currentTile.Y, "Diggable", "Back" ) != null && !this.isTileHoeDirt( currentTile ) )
                    {
                        this.checkForBuriedItem( ( int ) currentTile.X, ( int ) currentTile.Y, explosion: true, detectOnly: false, who );
                        this.makeHoeDirt( currentTile );
                    }
                    currentTile.Y += 1f;
                    currentTile.Y = Math.Min( this.map.Layers[ 0 ].LayerHeight - 1, Math.Max( 0f, currentTile.Y ) );
                }
                currentTile.X += 1f;
                currentTile.Y = Math.Min( this.map.Layers[ 0 ].LayerWidth - 1, Math.Max( 0f, currentTile.X ) );
                currentTile.Y = tileLocation.Y - ( float ) radius;
                currentTile.Y = Math.Min( this.map.Layers[ 0 ].LayerHeight - 1, Math.Max( 0f, currentTile.Y ) );
            }
            */
        }
    }
}
