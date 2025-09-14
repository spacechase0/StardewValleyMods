using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game.Items;
using MoonMisadventures.VirtualProperties;
using Netcode;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace MoonMisadventures.Game.Projectiles
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_AsteroidProjectile" )]
    public class AsteroidProjectile : Projectile
    {
        public readonly NetBool isBig = new();
        public readonly NetInt index = new();
        public readonly NetBool isMagnetic = new();
        public int rotationOffset;
        public bool rotationDir;
        public bool flipped;

        public AsteroidProjectile()
        {
            rotationOffset = Game1.random.Next( 360 );
            rotationDir = Game1.random.NextDouble() < 0.5f;
            flipped = Game1.random.NextDouble() < 0.5f;
        }

        public AsteroidProjectile( Vector2 pos, Vector2 velDir )
        :   this()
        {
            ignoreLocationCollision.Value = true;
            position.Value = pos;

            isBig.Value = Game1.recentMultiplayerRandom.NextDouble() < 0.2;
            index.Value = Game1.recentMultiplayerRandom.Next( isBig.Value ? 9 : 15 );
            isMagnetic.Value = ( index / ( isBig.Value ? 3 : 5 ) ) == 1;

            Vector2 dir = velDir;
            dir.Normalize();
            float force = isBig.Value ? Game1.recentMultiplayerRandom.Next( 4, 7 ) : Game1.recentMultiplayerRandom.Next( 8, 11 );
            xVelocity.Value = dir.X * force;
            yVelocity.Value = dir.Y * force;
        }

        public override Rectangle getBoundingBox()
        {
            int size = ( int )( (isBig.Value ? 32 : 16) * Game1.pixelZoom * 0.7f );
            return new Rectangle( ( int ) position.X - size / 2, ( int ) position.Y - size / 2, size, size ); 
        }


        public override void draw( SpriteBatch b )
        {
            int spritesheetSize = isBig.Value ? 3 : 5;
            int tileSize = isBig.Value ? 32 : 16;
            Rectangle srcRect = new Rectangle( index.Value % spritesheetSize * tileSize, index.Value / spritesheetSize * tileSize, tileSize, tileSize );
            float rot = MathHelper.WrapAngle( MathHelper.ToRadians( rotationOffset + Game1.ticks * 5 / ( isBig.Value ? 3 : 1 ) * ( rotationDir ? 1 : -1 ) ) );
            //b.Draw( Game1.staminaRect, Game1.GlobalToLocal( Game1.viewport, getBoundingBox() ), Color.Red );
            b.Draw( isBig.Value ? Assets.AsteroidsBig : Assets.AsteroidsSmall, Game1.GlobalToLocal( Game1.viewport, position.Value ), srcRect, Color.White, rot, new Vector2( tileSize / 2, tileSize / 2 ), Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1 );
        }

        public override void behaviorOnCollisionWithPlayer( GameLocation location, Farmer player )
        {
            int damage = isBig.Value ? Game1.recentMultiplayerRandom.Next( 17, 41 ) : Game1.recentMultiplayerRandom.Next( 5, 16 );
            player.takeDamage( damage, isBig.Value, null );

            string tex = "assets/asteroids-" + ( isBig.Value ? "large" : "small" ) + ".png";
            int spritesheetSize = isBig.Value ? 3 : 5;
            int tileSize = isBig.Value ? 32 : 16;
            Rectangle srcRect = new Rectangle( index.Value % spritesheetSize * tileSize, index.Value / spritesheetSize * tileSize, tileSize, tileSize );
            Game1.createRadialDebris( location, Mod.instance.Helper.ModContent.GetInternalAssetName(tex).BaseName, srcRect, (int)player.Tile.X, (int)player.Tile.Y, isBig.Value ? 18 : 7 );
        }

        public override void behaviorOnCollisionWithTerrainFeature( TerrainFeature t, Vector2 tileLocation, GameLocation location )
        {
        }

        public override void behaviorOnCollisionWithOther( GameLocation location )
        {
        }

        public override void behaviorOnCollisionWithMonster( NPC n, GameLocation location )
        {
        }

        public override bool update( GameTime time, GameLocation location )
        {
            if ( isMagnetic.Value && Game1.IsMasterGame )
            {
                Farmer closest = null;
                foreach ( var player in location.farmers )
                {
                    if ( player.GetAppliedMagneticRadius() > 128 && !player.HasNecklace( Necklace.Type.Lunar ) ) // 128 is default
                    {
                        float dist = Vector2.Distance( position.Value, player.getStandingPosition() );
                        if ( dist >= player.GetAppliedMagneticRadius() )
                            continue;

                        if ( closest == null )
                            closest = player;
                        else
                        {
                            float distC = Vector2.Distance( position.Value, closest.getStandingPosition() );
                            if ( player.GetAppliedMagneticRadius() - dist < closest.GetAppliedMagneticRadius() - distC )
                                closest = player;
                        }
                    }
                }

                if ( closest != null &&
                     new Vector2( xVelocity.Value, yVelocity.Value ).Length() < Vector2.Distance( closest.getStandingPosition(), position.Value ) )
                {
                    Vector2 dirVec = new Vector2( xVelocity.Value, yVelocity.Value );
                    Vector2 dirVecToClosest = closest.getStandingPosition() - position.Value;
                    dirVec.Normalize();
                    dirVecToClosest.Normalize();

                    double dir = Math.Atan2( -dirVec.Y, dirVec.X );
                    double mag = new Vector2( xVelocity.Value, yVelocity.Value ).Length();
                    double dirToClosest = Math.Atan2( -dirVecToClosest.Y, dirVecToClosest.X );

                    if ( Math.Abs( dir - dirToClosest ) < ( isBig.Value ? 1 : 3 ) * Math.PI / 180 )
                    {
                        dir = dirToClosest;
                        mag += ( isBig.Value ? 0.05 : 0.1 );
                    }
                    else
                    {
                        dir += ( ( isBig.Value ? 1 : 3 ) * Math.PI / 180 ) * Math.Sign( dirToClosest - dir );
                    }

                    xVelocity.Value = ( float ) ( Math.Cos( dir ) * mag );
                    yVelocity.Value = ( float ) ( -Math.Sin( dir ) * mag );
                }
            }

            return base.update( time, location );
        }

        public override void updatePosition( GameTime time )
        {
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }
    }
}
