using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace MoonMisadventures.Game.Projectiles
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_BoomProjectile" )]
    public class BoomProjectile : Projectile
    {
        public readonly NetVector2 target = new();
        public readonly NetVector2 prevTile = new();

        public BoomProjectile() { }
        public BoomProjectile( Vector2 pos, Vector2 targetPos )
        {
            NetFields.AddField(target, "target");
            NetFields.AddField(prevTile, "prevTile");

            ignoreMeleeAttacks.Value = true;
            ignoreLocationCollision.Value = true;
            position.Value = pos;
            target.Value = targetPos;
            prevTile.Value = new Vector2( ( int ) ( position.Value.X / Game1.tileSize ), ( int ) ( position.Value.Y / Game1.tileSize ) );

            Vector2 dir = targetPos - pos;
            dir.Normalize();
            xVelocity.Value = dir.X * 10;
            yVelocity.Value = dir.Y * 10;
        }

        private void GoBoom( GameLocation loc )
        {
            var dummy = new Farmer();
            loc.falseExplode( prevTile.Value, 2, dummy, damage_amount: 15 );
        }

        public override void behaviorOnCollisionWithPlayer( GameLocation location, Farmer player )
        {
            GoBoom( location );
        }

        public override bool update( GameTime time, GameLocation location )
        {
            if ( Vector2.Distance( target.Value, position.Value ) < new Vector2( xVelocity.Value, yVelocity.Value ).Length() )
            {
                GoBoom( location );
                return true;
            }
            else
            {
                var newTile = new Vector2( ( int ) ( position.Value.X / Game1.tileSize ), ( int ) ( position.Value.Y / Game1.tileSize ) );
                if ( newTile != prevTile.Value )
                {
                    var dummy = new Farmer();
                    location.falseExplode( prevTile.Value, 0, dummy, damage_amount: 3 );
                    prevTile.Value = newTile;
                }
            }

            return base.update( time, location );
        }

        public override void draw( SpriteBatch b )
        {
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

        public override void updatePosition( GameTime time )
        {
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }
    }
}
