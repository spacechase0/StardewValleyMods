using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Magic.Spells;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using System;

namespace Magic.Game
{
    class SpellProjectile : Projectile
    {
        private readonly Farmer source;
        private readonly ProjectileSpell spell;
        private readonly int damage;
        private readonly float dir;
        private readonly float vel;

        private Texture2D tex;
        private string texId;

        public SpellProjectile(Farmer theSource, ProjectileSpell theSpell, int dmg, float theDir, float theVel )
        {
            source = theSource;
            spell = theSpell;
            damage = dmg;
            dir = theDir;
            vel = theVel;

            theOneWhoFiredMe.Set(theSource.currentLocation, source );
            position.Value = source.getStandingPosition();
            position.X += source.GetBoundingBox().Width;
            position.Y += source.GetBoundingBox().Height;
            rotation = theDir;
            xVelocity.Value = (float) Math.Cos(dir) * vel;
            yVelocity.Value = (float) Math.Sin(dir) * vel;
            damagesMonsters.Value = true;

            tex = Content.loadTexture("magic/" + spell.ParentSchoolId + "/" + spell.Id + "/projectile.png");
            texId = Content.loadTextureKey("magic/" + spell.ParentSchoolId + "/" + spell.Id + "/projectile.png");
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
            //disappear(loc);
        }

        public override void behaviorOnCollisionWithMonster(NPC npc, GameLocation loc)
        {
            if (!(npc is Monster))
                return;

            loc.damageMonster(npc.GetBoundingBox(), damage, damage + 1, false, source);
            if (source != null)
                source.addMagicExp(damage);
            disappear(loc);
        }

        public override void behaviorOnCollisionWithOther(GameLocation loc)
        {
            disappear(loc);
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation loc, Farmer farmer)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation loc)
        {
            disappear( loc );
        }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle(( int )(position.X - Game1.tileSize), (int)(position.Y - Game1.tileSize), Game1.tileSize / 2, Game1.tileSize / 2);
        }

        public override void updatePosition(GameTime time)
        {
            //if (true) return;
            position.X += xVelocity;
            position.Y += yVelocity;
        }
        /*
        public override bool isColliding(GameLocation location)
        {
            Log.trace("iscoll");
            return false;
            if (!location.isTileOnMap(this.position / (float)Game1.tileSize) || !this.ignoreLocationCollision && location.isCollidingPosition(this.getBoundingBox(), Game1.viewport, false, 0, true, this.theOneWhoFiredMe, false, true, false) || !this.damagesMonsters && Game1.player.GetBoundingBox().Intersects(this.getBoundingBox()))
                return true;
            if (this.damagesMonsters)
                return location.doesPositionCollideWithCharacter(this.getBoundingBox(), false) != null;
            return false;
        }*/

        public override void draw(SpriteBatch b)
        {
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(getBoundingBox().X + getBoundingBox().Width / 2, getBoundingBox().Y + getBoundingBox().Height / 2));
            b.Draw(tex, drawPos, new Rectangle( 0, 0, tex.Width, tex.Height ), Color.White, dir, new Vector2( tex.Width / 2, tex.Height / 2 ), 2, SpriteEffects.None, (float)(((double)this.position.Y + (double)(Game1.tileSize * 3 / 2)) / 10000.0));
            //Vector2 bdp = Game1.GlobalToLocal(new Vector2(getBoundingBox().X, getBoundingBox().Y));
            //b.Draw(Mod.instance.manaFg, new Rectangle((int)bdp.X, (int)bdp.Y, getBoundingBox().Width, getBoundingBox().Height), Color.White);
        }

        private static Random rand = new Random();
        private void disappear( GameLocation loc )
        {
            if ( spell.SoundHit != null )
                Game1.playSound( spell.SoundHit );
            //Game1.createRadialDebris(loc, 22 + rand.Next( 2 ), ( int ) position.X / Game1.tileSize, ( int ) position.Y / Game1.tileSize, 3 + rand.Next(5), false);
            Game1.createRadialDebris(loc, texId, Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet,0), 4, (int)this.position.X, (int)this.position.Y, 6 + rand.Next( 10 ), (int)((double)this.position.Y / (double)Game1.tileSize) + 1, new Color( 255, 255, 255, 8 + rand.Next( 64 ) ), 2.0f);
            //Game1.createRadialDebris(loc, tex, new Rectangle(0, 0, tex.Width, tex.Height), 0, ( int ) position.X, ( int ) position.Y, 3 + rand.Next(5), ( int ) position.Y / Game1.tileSize, Color.White, 5.0f);
            destroyMe = true;
        }
    }
}
