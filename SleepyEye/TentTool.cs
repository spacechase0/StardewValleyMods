using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using SpaceCore;
using StardewModdingAPI;
using StardewValley;
using SFarmer = StardewValley.Farmer;

namespace SleepyEye
{
    public class TentTool : Tool, ISaveElement
    {
        private DateTime? StartedUsing;

        public TentTool()
        {
            this.Name = "Tent";
            this.description = this.loadDescription();

            this.numAttachmentSlots.Value = 0;
            this.IndexOfMenuItemView = 0;
            this.Stack = 1;
        }

        public override int salePrice()
        {
            return 5000;
        }

        public override bool beginUsing(GameLocation location, int x, int y, SFarmer who)
        {
            this.StartedUsing = DateTime.Now;
            who.canMove = false;
            return true;
        }

        public override bool onRelease(GameLocation location, int x, int y, SFarmer who)
        {
            if (this.StartedUsing == null)
                return true;

            TimeSpan useTime = DateTime.Now - (DateTime)this.StartedUsing;
            if (useTime > TimeSpan.FromSeconds(7))
            {
                Sleep.SaveLocation = true;
                Game1.NewDay(0);
            }

            this.StartedUsing = null;
            who.canMove = true;

            return true;
        }

        public override void tickUpdate(GameTime time, SFarmer who)
        {
            if (this.StartedUsing == null)
                return;

            FarmerSprite sprite = (FarmerSprite)who.Sprite;
            switch (who.FacingDirection)
            {
                case Game1.up:
                    sprite.animate(112, time);
                    break;

                case Game1.right:
                    sprite.animate(104, time);
                    break;

                case Game1.down:
                    sprite.animate(96, time);
                    break;

                case Game1.left:
                    sprite.animate(120, time);
                    break;
            }
        }

        public override void drawInMenu(SpriteBatch b, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            b.Draw(Mod.Instance.Helper.Content.Load<Texture2D>("Maps/" + Game1.currentSeason + "_outdoorsTileSheet", ContentSource.GameContent), new Vector2(location.X + Game1.tileSize / 2, location.Y + Game1.tileSize / 2), new Rectangle(224, 96, 48, 80), Color.White, 0, new Vector2(24, 40), scaleSize * 0.8f, SpriteEffects.None, 0);
        }

        public override void draw(SpriteBatch b)
        {
            this.CurrentParentTileIndex = this.IndexOfMenuItemView = -999;
            if (this.StartedUsing == null)
                return;

            TimeSpan useTime = DateTime.Now - (DateTime)this.StartedUsing;

            //if ( useTime > TimeSpan.FromSeconds( 7 ) )
            {
                Color col = Color.White * (0.2f + (float)useTime.TotalSeconds / 7f * 0.6f);
                if (useTime > TimeSpan.FromSeconds(7))
                    col = Color.White;

                Vector2 pos = Game1.GlobalToLocal(Game1.player.getStandingPosition());
                pos.Y -= Game1.tileSize * 2;
                b.Draw(Mod.Instance.Helper.Content.Load<Texture2D>("Maps/" + Game1.currentSeason + "_outdoorsTileSheet", ContentSource.GameContent), pos, new Rectangle(224, 96 + 80 - 16, 48, 16), col, 0, new Vector2(24, 40 - 80 + 16), 4, SpriteEffects.None, 0);
                b.Draw(Mod.Instance.Helper.Content.Load<Texture2D>("Maps/" + Game1.currentSeason + "_outdoorsTileSheet", ContentSource.GameContent), pos, new Rectangle(224, 96, 48, 80 - 16), col, 0, new Vector2(24, 40), 4, SpriteEffects.None, 0.999999f);
            }
        }

        protected override string loadDescription()
        {
            return "Sleep here. Sleep there. Sleep everywhere!";
        }

        protected override string loadDisplayName()
        {
            return "Tent";
        }

        public override bool canBeDropped()
        {
            return true;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override Item getOne()
        {
            return new TentTool();
        }

        public object getReplacement()
        {
            return new StardewValley.Object();
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            this.Name = "Tent";
            this.description = this.loadDescription();

            this.numAttachmentSlots.Value = 0;
            this.IndexOfMenuItemView = 0;
        }
    }
}
