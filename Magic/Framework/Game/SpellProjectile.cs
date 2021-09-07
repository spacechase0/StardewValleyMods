using System;
using Magic.Framework.Spells;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceCore;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace Magic.Framework.Game
{
    internal class SpellProjectile : Projectile
    {
        /*********
        ** Fields
        *********/
        private readonly Farmer Source;
        private readonly ProjectileSpell Spell;
        private readonly NetInt Damage = new();
        private readonly NetFloat Direction = new();
        private readonly NetFloat Velocity = new();
        private readonly NetBool IsSeeking = new();

        private Texture2D Tex;
        private readonly NetString TexId = new();
        private readonly Monster SeekTarget;

        private static readonly Random Rand = new();


        /*********
        ** Public methods
        *********/
        public SpellProjectile()
        {
            this.NetFields.AddFields(this.Damage, this.Direction, this.Velocity, this.IsSeeking, this.TexId);
        }

        public SpellProjectile(Farmer source, ProjectileSpell spell, int damage, float direction, float velocity, bool isSeeking)
            : this()
        {
            this.Source = source;
            this.Spell = spell;
            this.Damage.Value = damage;
            this.Direction.Value = direction;
            this.Velocity.Value = velocity;
            this.IsSeeking.Value = isSeeking;

            this.theOneWhoFiredMe.Set(source.currentLocation, this.Source);
            this.position.Value = this.Source.getStandingPosition();
            this.position.X += this.Source.GetBoundingBox().Width;
            this.position.Y += this.Source.GetBoundingBox().Height;
            this.rotation = direction;
            this.xVelocity.Value = (float)Math.Cos(this.Direction.Value) * this.Velocity.Value;
            this.yVelocity.Value = (float)Math.Sin(this.Direction.Value) * this.Velocity.Value;
            this.damagesMonsters.Value = true;

            this.Tex = Content.LoadTexture($"magic/{this.Spell.ParentSchoolId}/{this.Spell.Id}/projectile.png");
            this.TexId.Value = Content.LoadTextureKey($"magic/{this.Spell.ParentSchoolId}/{this.Spell.Id}/projectile.png");

            if (this.IsSeeking.Value)
            {
                float nearestDist = float.MaxValue;
                Monster nearestMob = null;
                foreach (var character in source.currentLocation.characters)
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

                this.SeekTarget = nearestMob;
            }
        }

        public override void behaviorOnCollisionWithMineWall(int tileX, int tileY)
        {
            //disappear(loc);
        }

        public override void behaviorOnCollisionWithMonster(NPC npc, GameLocation loc)
        {
            if (npc is not Monster)
                return;

            bool didDmg = loc.damageMonster(npc.GetBoundingBox(), this.Damage.Value, this.Damage.Value + 1, false, this.Source);
            if (this.Source != null && didDmg)
                this.Source.AddCustomSkillExperience(Magic.Skill, this.Damage.Value / ((this.theOneWhoFiredMe.Get(loc) as Farmer).CombatLevel + 1));
            this.Disappear(loc);
        }

        public override void behaviorOnCollisionWithOther(GameLocation loc)
        {
            if (!this.IsSeeking.Value)
                this.Disappear(loc);
        }

        public override void behaviorOnCollisionWithPlayer(GameLocation loc, Farmer farmer)
        {
        }

        public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation loc)
        {
            if (!this.IsSeeking.Value)
                this.Disappear(loc);
        }

        public override bool isColliding(GameLocation location)
        {
            if (this.IsSeeking.Value)
            {
                return location.doesPositionCollideWithCharacter(this.getBoundingBox()) != null;
            }
            else return base.isColliding(location);
        }

        public override Rectangle getBoundingBox()
        {
            return new((int)(this.position.X - Game1.tileSize), (int)(this.position.Y - Game1.tileSize), Game1.tileSize / 2, Game1.tileSize / 2);
        }

        public override bool update(GameTime time, GameLocation location)
        {
            if (this.IsSeeking.Value)
            {
                if (this.SeekTarget is not { Health: > 0 } || this.SeekTarget.currentLocation == null)
                {
                    this.Disappear(location);
                    return true;
                }
                else
                {
                    Vector2 unit = new Vector2(this.SeekTarget.GetBoundingBox().Center.X + 32, this.SeekTarget.GetBoundingBox().Center.Y + 32) - this.position;
                    unit.Normalize();

                    this.xVelocity.Value = unit.X * this.Velocity.Value;
                    this.yVelocity.Value = unit.Y * this.Velocity.Value;
                }
            }

            return base.update(time, location);
        }

        public override void updatePosition(GameTime time)
        {
            //if (true) return;
            this.position.X += this.xVelocity.Value;
            this.position.Y += this.yVelocity.Value;
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
            this.Tex ??= Game1.content.Load<Texture2D>(this.TexId.Value);
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(this.getBoundingBox().X + this.getBoundingBox().Width / 2, this.getBoundingBox().Y + this.getBoundingBox().Height / 2));
            b.Draw(this.Tex, drawPos, new Rectangle(0, 0, this.Tex.Width, this.Tex.Height), Color.White, this.Direction.Value, new Vector2(this.Tex.Width / 2, this.Tex.Height / 2), 2, SpriteEffects.None, (float)((this.position.Y + (double)(Game1.tileSize * 3 / 2)) / 10000.0));
            //Vector2 bdp = Game1.GlobalToLocal(new Vector2(getBoundingBox().X, getBoundingBox().Y));
            //b.Draw(Mod.instance.manaFg, new Rectangle((int)bdp.X, (int)bdp.Y, getBoundingBox().Width, getBoundingBox().Height), Color.White);
        }


        /*********
        ** Private methods
        *********/
        private void Disappear(GameLocation loc)
        {
            if (this.Spell?.SoundHit != null)
                loc.LocalSoundAtPixel(this.Spell.SoundHit, this.position.Value);

            //Game1.createRadialDebris(loc, 22 + rand.Next( 2 ), ( int ) position.X / Game1.tileSize, ( int ) position.Y / Game1.tileSize, 3 + rand.Next(5), false);
            Game1.createRadialDebris(loc, this.TexId.Value, Game1.getSourceRectForStandardTileSheet(Projectile.projectileSheet, 0), 4, (int)this.position.X, (int)this.position.Y, 6 + SpellProjectile.Rand.Next(10), (int)(this.position.Y / (double)Game1.tileSize) + 1, new Color(255, 255, 255, 8 + SpellProjectile.Rand.Next(64)), 2.0f);
            //Game1.createRadialDebris(loc, tex, new Rectangle(0, 0, tex.Width, tex.Height), 0, ( int ) position.X, ( int ) position.Y, 3 + rand.Next(5), ( int ) position.Y / Game1.tileSize, Color.White, 5.0f);
            this.destroyMe = true;
        }
    }
}
