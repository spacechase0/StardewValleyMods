using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BlahajBlast;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace BlahajBlast
{
    [XmlType( "Mods_spacechase0_BlahajBlast_LaserProjectile" )]
    public class SharknadoPellet : Projectile
    {
        float dist;
        float crot;
        float spd = 7;
        bool dir = false;

        public SharknadoPellet()
        {
            damagesMonsters.Value = true;
            dist = (float)Game1.random.NextDouble() * 128 + 48;
            crot = (float)Game1.random.NextDouble() * 3.14f * 2;
            spd += (float)Game1.random.NextDouble() * 5;
            dir = (float)Game1.random.NextDouble() < 0.5;
        }

        public SharknadoPellet( Vector2 pos, Vector2 vel, Farmer who )
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
            var pos = position.Value;
            pos += new Vector2(MathF.Sin(crot) * dist, MathF.Cos(crot) * dist);

            return new Rectangle( ( int )pos.X - 16 / 2, ( int )pos.Y - 16 / 2, 16, 16 ); 
        }


        public override void draw( SpriteBatch b )
        {
            float rot = MathF.Atan2(yVelocity.Value, xVelocity.Value);
            rot += crot * 2;
            crot += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds * spd;

            var pos = position.Value;
            pos += new Vector2(MathF.Sin(crot) * dist, MathF.Cos(crot) * dist);

            //b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, getBoundingBox()), Color.Red);
            b.Draw( Mod.sharkTex, Game1.GlobalToLocal( Game1.viewport, pos ), null, Color.White, rot, new Vector2( 8, 8 ), 4, SpriteEffects.None, 2 );
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
            location.damageMonster(n.GetBoundingBox(), 7, 22, false, this.theOneWhoFiredMe.Get( location ) as Farmer );
            monster.xVelocity = xvel;
            monster.yVelocity = yvel;
        }

        public override void updatePosition( GameTime time )
        {
            position.X += xVelocity.Value;
            position.Y += yVelocity.Value;
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
        }
    }
}
