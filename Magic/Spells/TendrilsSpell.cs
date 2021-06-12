using System;
using System.Collections.Generic;
using System.Linq;
using Magic.Schools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Spells
{
    // TODO: Change into trap?
    internal class TendrilsSpell : Spell
    {
        public TendrilsSpell()
            : base(SchoolId.Nature, "tendrils") { }

        public override int GetManaCost(Farmer player, int level)
        {
            return 10;
        }

        public override int GetMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY)
        {
            TendrilGroup tendrils = new TendrilGroup();
            foreach (var npc in player.currentLocation.characters)
            {
                if (npc is Monster mob)
                {
                    float rad = Game1.tileSize;
                    int dur = 11 * 60;
                    if (Vector2.Distance(mob.position, new Vector2(targetX, targetY)) <= rad)
                    {
                        tendrils.Add(new Tendril(mob, new Vector2(targetX, targetY), rad, dur));
                        player.AddCustomSkillExperience(Magic.Skill, 3);
                    }
                }
            }

            return tendrils.Any()
                ? tendrils
                : null;
        }

        private class TendrilGroup : List<Tendril>, IActiveEffect
        {
            /// <summary>Update the effect state if needed.</summary>
            /// <param name="e">The update tick event args.</param>
            /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
            public bool Update(UpdateTickedEventArgs e)
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    Tendril tendril = this[i];
                    if (!tendril.Update(e))
                        this.RemoveAt(i);
                }

                return this.Any();
            }

            /// <summary>Draw the effect to the screen if needed.</summary>
            /// <param name="spriteBatch">The sprite batch being drawn.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                foreach (Tendril tendril in this)
                    tendril.Draw(spriteBatch);
            }
        }

        private class Tendril : IActiveEffect
        {
            private readonly Monster Mob;
            private readonly Vector2 Pos;
            private readonly float Radius;
            private readonly Texture2D Tex;
            private int Duration;

            public Tendril(Monster theMob, Vector2 pos, float rad, int dur)
            {
                this.Mob = theMob;
                this.Pos = pos;
                this.Radius = rad;
                this.Duration = dur;
                this.Tex = Content.LoadTexture("magic/nature/tendrils/tendril.png");
            }

            /// <summary>Update the effect state if needed.</summary>
            /// <param name="e">The update tick event args.</param>
            /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
            public bool Update(UpdateTickedEventArgs e)
            {
                Vector2 mobPos = new Vector2(this.Mob.GetBoundingBox().Center.X, this.Mob.GetBoundingBox().Center.Y);
                if (Vector2.Distance(mobPos, this.Pos) >= this.Radius)
                {
                    Vector2 offset = this.Mob.position - this.Pos;
                    offset.Normalize();
                    offset *= this.Radius;
                    this.Mob.position.Value = this.Pos + offset;
                }

                return --this.Duration > 0;
            }

            /// <summary>Draw the effect to the screen if needed.</summary>
            /// <param name="spriteBatch">The sprite batch being drawn.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                Vector2 mobPos = new Vector2(this.Mob.GetBoundingBox().Center.X, this.Mob.GetBoundingBox().Center.Y);
                float dist = Vector2.Distance(mobPos, this.Pos);
                Rectangle r = new Rectangle((int)this.Pos.X, (int)this.Pos.Y, 10, (int)dist);
                r = Game1.GlobalToLocal(Game1.viewport, r);
                float rot = (float)-Math.Atan2(this.Pos.Y - mobPos.Y, mobPos.X - this.Pos.X);
                spriteBatch.Draw(this.Tex, r, new Rectangle(0, 0, 10, 12), Color.White, rot - 3.14f / 2, new Vector2(5, 0), SpriteEffects.None, 1);
            }
        }
    }
}
