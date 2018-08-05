using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using Magic.Schools;
using StardewValley;
using StardewValley.Monsters;
using System;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace Magic.Spells
{
    class MeteorSpell : Spell
    {
        public MeteorSpell() : base( SchoolId.Eldritch, "meteor" )
        {
        }

        public override int getManaCost(StardewValley.Farmer player, int level)
        {
            return 0;
        }

        public override bool canCast(StardewValley.Farmer player, int level)
        {
            return base.canCast(player, level) && player.hasItemInInventory(SObject.iridium, 1);
        }

        public override int getMaxCastingLevel()
        {
            return 1;
        }

        public override void onCast(StardewValley.Farmer player, int level, int targetX, int targetY)
        {
            new Meteor(player, targetX, targetY);
            player.consumeObject(SObject.iridium, 1);
        }
    }
    
    internal class Meteor
    {
        private readonly GameLocation loc;
        private readonly SFarmer source;

        private Vector2 position = new Vector2();
        private float yVelocity;
        private float height = 1000;

        public Meteor(SFarmer theSource, int tx, int ty)
        {
            loc = theSource.currentLocation;
            source = theSource;
                
            position.X = tx;
            position.Y = ty;
            yVelocity = 64;

            GameEvents.UpdateTick += update;
            GraphicsEvents.OnPreRenderHudEvent += render;
        }

        private static Random rand = new Random();
        public void update(object sender, EventArgs args)
        {
            height -= (int)yVelocity;
            if (height <= 0)
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
                            source.addMagicExp(5);
                        }
                    }
                }
                loc.explode(new Vector2((int)position.X / Game1.tileSize, (int)position.Y / Game1.tileSize), 4 + 2, source);

                GameEvents.UpdateTick -= update;
                GraphicsEvents.OnPreRenderHudEvent -= render;
            }
        }

        public void render(object sender, EventArgs args)
        {
            SpriteBatch b = Game1.spriteBatch;
            Vector2 drawPos = Game1.GlobalToLocal(new Vector2(position.X, position.Y - height));
            b.Draw(Game1.objectSpriteSheet, drawPos, new Rectangle(352, 400, 32, 32), Color.White, 0, new Vector2(16, 16), 2 + 8, SpriteEffects.None, (float)(((double)this.position.Y - height + (double)(Game1.tileSize * 3 / 2)) / 10000.0));
        }
    }
}
