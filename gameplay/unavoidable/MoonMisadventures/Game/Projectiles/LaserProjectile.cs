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
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace MoonMisadventures.Game.Projectiles
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_LaserProjectile" )]
    public class LaserProjectile : Projectile
    {
        public LaserProjectile()
        {
            damagesMonsters.Value = true;
        }

        public LaserProjectile( Vector2 pos, Vector2 vel, Farmer who )
        :   this()
        {
            ignoreLocationCollision.Value = true;
            position.Value = pos;
            xVelocity.Value = vel.X;
            yVelocity.Value = vel.Y;
            theOneWhoFiredMe.Set(who.currentLocation, who);
        }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle( ( int ) position.X - 16 / 2, ( int ) position.Y - 16 / 2, 16, 16 ); 
        }


        public override void draw( SpriteBatch b )
        {
            float rot = MathF.Atan2(yVelocity.Value, xVelocity.Value);
            //b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, getBoundingBox()), Color.Red);
            b.Draw( Assets.Laser, Game1.GlobalToLocal( Game1.viewport, position.Value ), null, Color.White, rot, new Vector2( 8, 8 ), 1, SpriteEffects.None, 2 );
        }

        public override void behaviorOnCollisionWithPlayer( GameLocation location, Farmer player )
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
            if (n is not Monster monster)
                return;

            float xvel = monster.xVelocity;
            float yvel = monster.yVelocity;
            location.damageMonster(n.GetBoundingBox(), 45, 60, false, this.theOneWhoFiredMe.Get( location ) as Farmer );
            monster.xVelocity = xvel;
            monster.yVelocity = yvel;
        }

        public override void updatePosition( GameTime time )
        {
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }
    }
}
