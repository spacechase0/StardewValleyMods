using System;
using Magic.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace Magic.Game
{
    internal class SpellProjectile : Projectile
    {
        private readonly Farmer source;
        private readonly ProjectileSpell spell;
        private readonly NetInt damage = new();
        private readonly NetFloat dir = new();
        private readonly NetFloat vel = new();
        private readonly NetBool seeking = new();

        private Texture2D tex;
        private readonly NetString texId = new();

        private Monster seekTarget;

        public SpellProjectile()
        {
            this.NetFields.AddFields(this.damage, this.dir, this.vel, this.seeking, this.texId);
        }

        public SpellProjectile(Farmer theSource, ProjectileSpell theSpell, int dmg, float theDir, float theVel, bool theSeeking)
            : this()
        {

            this.source = theSource;
            this.spell = theSpell;
            this.damage.Value = dmg;
            this.dir.Value = theDir;
            this.vel.Value = theVel;
            this.seeking.Value = theSeeking;

            this.theOneWhoFiredMe.Set(theSource.currentLocation, this.source);
            this.position.Value = this.source.getStandingPosition();
            this.position.X += this.source.GetBoundingBox().Width;
            this.position.Y += this.source.GetBoundingBox().Height;
            this.rotation = theDir;
            this.xVelocity.Value = (float)Math.Cos(this.dir) * this.vel;
            this.yVelocity.Value = (float)Math.Sin(this.dir) * this.vel;
            this.damagesMonsters.Value = true;

            this.tex = Content.loadTexture("magic/" + this.spell.ParentSchoolId + "/" + this.spell.Id + "/projectile.png");
            this.texId.Value = Content.loadTextureKey("magic/" + this.spell.ParentSchoolId + "/" + this.spell.Id + "/projectile.png");

            if (this.seeking)
            {
                float nearestDist = float.MaxValue;
                Monster nearestMob = null;
                foreach (var character in theSource.currentLocation.characters)
                {
                    if (character is Monster mob)
                    {
                        float dist = Utility.distance(mob.Position.X, this.position.X, mob.Position.Y, this.position.Y);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearestMob = mob;
                        }
                    }
                }

                this.seekTarget = nearestMob;
            }
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
            //disappear(loc);
        }

        public override void behaviorOnCollisionWithMonster(NPC npc, GameLocation loc)
        {
            if (!(npc is Monster))
                return;

            bool didDmg = loc.damageMonster(npc.GetBoundingBox(), this.damage, this.damage + 1, false, this.source);
            if (this.source != null && didDmg)
                this.source.AddCustomSkillExperience(Magic.Skill, this.damage / ((this.theOneWhoFiredMe.Get(loc) as Farmer).CombatLevel + 1));
            this.disappear(loc);
        }

        public override void behaviorOnCollisionWithOther(GameLocation loc)
        {
            if (!this.seeking)
                this.disappear(loc);
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation loc, Farmer farmer)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation loc)
        {
            if (!this.seeking)
                this.disappear(loc);
        }

        public override bool isColliding(GameLocation location)
        {
            if (this.seeking)
            {
                return location.doesPositionCollideWithCharacter(this.getBoundingBox(), false) != null;
            }
            else return base.isColliding(location);
        }

        public override Rectangle getBoundingBox()
        {
            return new((int)(this.position.X - Game1.tileSize), (int)(this.position.Y - Game1.tileSize), Game1.tileSize / 2, Game1.tileSize / 2);
        }

        public override bool update(GameTime time, GameLocation location)
        {
            if (this.seeking)
            {
                if (this.seekTarget == null || this.seekTarget.Health <= 0 || this.seekTarget.currentLocation == null)
                {
                    this.disappear(location);
                    return true;
                }
                else
                {
                    Vector2 unit = new Vector2(this.seekTarget.GetBoundingBox().Center.X + 32, this.seekTarget.GetBoundingBox().Center.Y + 32) - this.position;
                    unit.Normalize();

                    this.xVelocity.Value = unit.X * this.vel;
                    this.yVelocity.Value = unit.Y * this.vel;
                }
            }

            return base.update(time, location);
        }

        public override void updatePosition(GameTime time)
        {
            //if (true) return;
            this.position.X += this.xVelocity;
            this.position.Y += this.yVelocity;
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
            if (this.tex == null)
                this.tex = Game1.content.Load<Texture2D>(this.texId.Value);
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(this.getBoundingBox().X + this.getBoundingBox().Width / 2, this.getBoundingBox().Y + this.getBoundingBox().Height / 2));
            b.Draw(this.tex, drawPos, new Rectangle(0, 0, this.tex.Width, this.tex.Height), Color.White, this.dir, new Vector2(this.tex.Width / 2, this.tex.Height / 2), 2, SpriteEffects.None, (float)(((double)this.position.Y + (double)(Game1.tileSize * 3 / 2)) / 10000.0));
            //Vector2 bdp = Game1.GlobalToLocal(new Vector2(getBoundingBox().X, getBoundingBox().Y));
            //b.Draw(Mod.instance.manaFg, new Rectangle((int)bdp.X, (int)bdp.Y, getBoundingBox().Width, getBoundingBox().Height), Color.White);
        }

        private static Random rand = new();
        private void disappear(GameLocation loc)
        {
            if (this.spell.SoundHit != null)
                Game1.playSound(this.spell.SoundHit);
            //Game1.createRadialDebris(loc, 22 + rand.Next( 2 ), ( int ) position.X / Game1.tileSize, ( int ) position.Y / Game1.tileSize, 3 + rand.Next(5), false);
            Game1.createRadialDebris(loc, this.texId, Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 0), 4, (int)this.position.X, (int)this.position.Y, 6 + SpellProjectile.rand.Next(10), (int)((double)this.position.Y / (double)Game1.tileSize) + 1, new Color(255, 255, 255, 8 + SpellProjectile.rand.Next(64)), 2.0f);
            //Game1.createRadialDebris(loc, tex, new Rectangle(0, 0, tex.Width, tex.Height), 0, ( int ) position.X, ( int ) position.Y, 3 + rand.Next(5), ( int ) position.Y / Game1.tileSize, Color.White, 5.0f);
            this.destroyMe = true;
        }
    }
}
