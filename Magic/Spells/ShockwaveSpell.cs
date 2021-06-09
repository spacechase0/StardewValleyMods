using System;
using Magic.Schools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Spells
{
    internal class ShockwaveSpell : Spell
    {
        public ShockwaveSpell()
            : base(SchoolId.Nature, "shockwave") { }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.yJumpVelocity == 0;
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            player.jump();
            return new Shockwave(player, level);
        }

        private class Shockwave : IActiveEffect
        {
            private readonly Farmer player;
            private readonly int level;

            public Shockwave(Farmer player, int level)
            {
                this.player = player;
                this.level = level;
            }

            private bool jumping = true;
            private float prevJumpVel = 0;
            private float landX, landY;
            private float timer = 0;
            private int currRad = 0;

            /// <summary>Update the effect state if needed.</summary>
            /// <param name="e">The update tick event args.</param>
            /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
            public bool Update(UpdateTickedEventArgs e)
            {
                if (this.jumping)
                {
                    if (this.player.yJumpVelocity == 0 && this.prevJumpVel < 0)
                    {
                        this.landX = this.player.position.X;
                        this.landY = this.player.position.Y;
                        this.jumping = false;
                    }
                    this.prevJumpVel = this.player.yJumpVelocity;
                }
                if (!this.jumping)
                {
                    if (--this.timer > 0)
                    {
                        return true;
                    }
                    this.timer = 10;

                    int spotsForCurrRadius = 1 + this.currRad * 7;
                    for (int i = 0; i < spotsForCurrRadius; ++i)
                    {
                        Game1.playSound("hoeHit");
                        float ix = this.landX + (float)Math.Cos(Math.PI * 2 / spotsForCurrRadius * i) * this.currRad * Game1.tileSize;
                        float iy = this.landY + (float)Math.Sin(Math.PI * 2 / spotsForCurrRadius * i) * this.currRad * Game1.tileSize;
                        this.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(ix, iy), Color.White, 8, Game1.random.NextDouble() < 0.5, 30, 0, -1, -1f, -1, 0));
                        this.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(12, new Vector2(ix, iy), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, -1, 0));
                    }
                    ++this.currRad;

                    foreach (var character in this.player.currentLocation.characters)
                    {
                        if (character is Monster mob)
                        {
                            if (Vector2.Distance(new Vector2(this.landX, this.landY), mob.position) < this.currRad * Game1.tileSize)
                            {
                                // TODO: Use location damage method for xp and quest progress
                                mob.takeDamage((this.level + 1) * 5 * (this.player.CombatLevel + 1), 0, 0, false, 0, this.player);
                                this.player.AddCustomSkillExperience(Magic.Skill, 3);
                            }
                        }
                    }

                    if (this.currRad >= 1 + (this.level + 1) * 2)
                        return false;
                }

                return true;
            }

            /// <summary>Draw the effect to the screen if needed.</summary>
            /// <param name="spriteBatch">The sprite batch being drawn.</param>
            public void Draw(SpriteBatch spriteBatch) { }
        }
    }
}
