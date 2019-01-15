using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Magic.Schools;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;

namespace Magic.Spells
{
    // TODO: Change into trap?
    class TendrilsSpell : Spell
    {
        public TendrilsSpell() : base( SchoolId.Nature, "tendrils" )
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            TendrilGroup tendrils = new TendrilGroup();
            foreach ( var npc in player.currentLocation.characters )
            {
                if ( npc is Monster mob )
                {
                    float rad = (7 - level * 2) * Game1.tileSize / 2;
                    int dur = ( level * 2 + 5 ) * 60;
                    if ( Vector2.Distance(mob.position, new Vector2( targetX, targetY ) ) <= rad )
                    {
                        tendrils.Add(new Tendril(mob, new Vector2(targetX, targetY), rad, dur ));
                        player.addMagicExp(3);
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
            private readonly Monster mob;
            private readonly Vector2 pos;
            private readonly float radius;
            private readonly Texture2D tex;
            private int duration;

            public Tendril( Monster theMob, Vector2 pos, float rad, int dur )
            {
                mob = theMob;
                this.pos = pos;
                radius = rad;
                duration = dur;
                tex = Content.loadTexture("magic/nature/tendrils/tendril.png");
            }

            /// <summary>Update the effect state if needed.</summary>
            /// <param name="e">The update tick event args.</param>
            /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
            public bool Update(UpdateTickedEventArgs e)
            {
                Vector2 mobPos = new Vector2(mob.GetBoundingBox().Center.X, mob.GetBoundingBox().Center.Y);
                if (Vector2.Distance(mobPos, pos) >= radius)
                {
                    Vector2 offset = mob.position - pos;
                    offset.Normalize();
                    offset *= radius;
                    mob.position.Value = pos + offset;
                }

                return --duration > 0;
            }

            /// <summary>Draw the effect to the screen if needed.</summary>
            /// <param name="spriteBatch">The sprite batch being drawn.</param>
            public void Draw(SpriteBatch spriteBatch)
            {
                Vector2 mobPos = new Vector2(mob.GetBoundingBox().Center.X, mob.GetBoundingBox().Center.Y);
                var dist = Vector2.Distance(mobPos, pos);
                Rectangle r = new Rectangle((int)pos.X, (int)pos.Y, 10, (int)dist);
                r = Game1.GlobalToLocal(Game1.viewport, r);
                float rot = (float)-Math.Atan2(pos.Y - mobPos.Y, mobPos.X - pos.X);
                spriteBatch.Draw(tex, r, new Rectangle(0, 0, 10, 12), Color.White, rot - 3.14f / 2, new Vector2(5, 0), SpriteEffects.None, 1);
            }
        }
    }
}
