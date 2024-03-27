using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace Magic.Framework.Spells.Effects
{
    internal class Meteor : IActiveEffect
    {
        /*********
        ** Fields
        *********/
        private readonly GameLocation Loc;
        private readonly Farmer Source;
        private static readonly Random Rand = new();
        private readonly Vector2 Position;
        private readonly float YVelocity;
        private float Height = 1000;


        /*********
        ** Public methods
        *********/
        public Meteor(Farmer theSource, int tx, int ty)
        {
            this.Loc = theSource.currentLocation;
            this.Source = theSource;

            this.Position.X = tx;
            this.Position.Y = ty;
            this.YVelocity = 64;
        }

        /// <summary>Update the effect state if needed.</summary>
        /// <param name="e">The update tick event args.</param>
        /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
        public bool Update(UpdateTickedEventArgs e)
        {
            // decrease height until zero
            this.Height -= (int)this.YVelocity;
            if (this.Height > 0)
                return true;

            // trigger explosion
            {
                this.Loc.LocalSoundAtPixel("explosion", this.Position);
                for (int i = 0; i < 10; ++i)
                {
                    for (int x = -i; x <= i; ++x)
                    {
                        for (int y = -i; y <= i; ++y)
                            Game1.createRadialDebris(this.Loc, Game1.objectSpriteSheetName, new Rectangle(352, 400, 32, 32), 4, (int)this.Position.X + x * 20, (int)this.Position.Y + y * 20, 15 - 14 + Meteor.Rand.Next(15 - 14), (int)(this.Position.Y / (double)Game1.tileSize) + 1, new Color(255, 255, 255, 255), 4.0f);
                    }
                }
                foreach (var npc in this.Source.currentLocation.characters)
                {
                    if (npc is Monster mob)
                    {
                        float rad = 8 * 64;
                        if (Vector2.Distance(mob.position.Value, new Vector2(this.Position.X, this.Position.Y)) <= rad)
                        {
                            // TODO: Use location damage method for xp and quest progress
                            mob.takeDamage(300, 0, 0, false, 0, this.Source);
                            this.Source.AddCustomSkillExperience(Magic.Skill, 5);
                        }
                    }
                }
                this.Loc.explode(new Vector2((int)this.Position.X / Game1.tileSize, (int)this.Position.Y / Game1.tileSize), 4 + 2, this.Source);
                return false;
            }
        }

        /// <summary>Draw the effect to the screen if needed.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(this.Position.X, this.Position.Y - this.Height));
            spriteBatch.Draw(Game1.objectSpriteSheet, drawPos, new Rectangle(352, 400, 32, 32), Color.White, 0, new Vector2(16, 16), 2 + 8, SpriteEffects.None, (float)(((double)this.Position.Y - this.Height + Game1.tileSize * 3 / 2) / 10000.0));
        }
    }
}
