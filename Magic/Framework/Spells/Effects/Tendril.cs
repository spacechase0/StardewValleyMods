using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Framework.Spells.Effects
{
    internal class Tendril : IActiveEffect
    {
        /*********
        ** Fields
        *********/
        private readonly Monster Mob;
        private readonly Vector2 Pos;
        private readonly float Radius;
        private readonly Texture2D Tex;
        private int Duration;


        /*********
        ** Public methods
        *********/
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
                Vector2 offset = this.Mob.position.Value - this.Pos;
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
