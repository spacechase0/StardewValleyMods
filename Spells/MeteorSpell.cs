using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using StardewValley.Monsters;
using System;
using SObject = StardewValley.Object;
using SpaceCore;

namespace Magic.Spells
{
    class MeteorSpell : Spell
    {
        public MeteorSpell() 
            : base( SchoolId.Eldritch, "meteor" )
        {
        }

        public override int getManaCost(Farmer player, int level)
        {
            return 0;
        }

        public override bool canCast(Farmer player, int level)
        {
            return base.canCast(player, level) && player.hasItemInInventory(SObject.iridium, 1);
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override IActiveEffect onCast(Farmer player, int level, int targetX, int targetY)
        {
            player.consumeObject(SObject.iridium, 1);
            return new Meteor(player, targetX, targetY);
        }
    }
    
    internal class Meteor : IActiveEffect
    {
        private readonly GameLocation loc;
        private readonly Farmer source;
        private static readonly Random rand = new Random();
        private readonly Vector2 position;
        private readonly float yVelocity;
        private float height = 1000;

        public Meteor(Farmer theSource, int tx, int ty)
        {
            loc = theSource.currentLocation;
            source = theSource;

            position.X = tx;
            position.Y = ty;
            yVelocity = 64;
        }

        /// <summary>Update the effect state if needed.</summary>
        /// <param name="e">The update tick event args.</param>
        /// <returns>Returns true if the effect is still active, or false if it can be discarded.</returns>
        public bool Update(UpdateTickedEventArgs e)
        {
            // decrease height until zero
            height -= (int)yVelocity;
            if (height > 0)
                return true;

            // trigger explosion
            {
                Game1.playSound("explosion");
                for (int i = 0; i < 10; ++i)
                {
                    for (int ix = -i; ix <= i; ++ix)
                    for (int iy = -i; iy <= i; ++iy)
                        Game1.createRadialDebris(loc, Game1.objectSpriteSheetName, new Rectangle(352, 400, 32, 32), 4, (int)this.position.X + ix * 20, (int)this.position.Y + iy * 20, 15 - 14 + rand.Next(15 - 14), (int)((double)this.position.Y / (double)Game1.tileSize) + 1, new Color(255, 255, 255, 255), 4.0f);
                }
                foreach (var npc in source.currentLocation.characters)
                {
                    if (npc is Monster mob)
                    {
                        float rad = 8 * 64;
                        if (Vector2.Distance(mob.position, new Vector2(position.X, position.Y)) <= rad)
                        {
                            mob.takeDamage(300, 0, 0, false, 0, source);
                            source.AddCustomSkillExperience(Magic.Skill, 5);
                        }
                    }
                }
                loc.explode(new Vector2((int)position.X / Game1.tileSize, (int)position.Y / Game1.tileSize), 4 + 2, source);
                return false;
            }
        }

        /// <summary>Draw the effect to the screen if needed.</summary>
        /// <param name="spriteBatch">The sprite batch being drawn.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(position.X, position.Y - height));
            spriteBatch.Draw(Game1.objectSpriteSheet, drawPos, new Rectangle(352, 400, 32, 32), Color.White, 0, new Vector2(16, 16), 2 + 8, SpriteEffects.None, (float)(((double)this.position.Y - height + (double)(Game1.tileSize * 3 / 2)) / 10000.0));
        }
    }
}
