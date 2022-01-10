using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SleepyEye.Framework;
using StardewValley;

namespace SleepyEye
{
    [XmlType("Mods_spacechase0_SleepyEye")]
    public class TentTool : Tool
    {
        /*********
        ** Fields
        *********/
        /// <summary>When the player started using the tent, if they're currently using it.</summary>
        private long? StartedUsing;

        /// <summary>When the tool triggered a save, if it's in progress.</summary>
        private long? StartedSaving;

        /// <summary>Whether the player is currently using the tent.</summary>
        private bool IsUsing => this.StartedUsing != null;

        /// <summary>Whether the tent triggered an ongoing save.</summary>
        private bool IsSaving => this.StartedSaving != null;

        /// <summary>How long after a save is triggered before resetting the tool's use and save flags.</summary>
        private readonly TimeSpan ResetDelay = TimeSpan.FromSeconds(3);

        /// <summary>The texture loaded for the item in inventories.</summary>
        private Texture2D ItemTexture;

        /// <summary>The last texture key loaded for the tent.</summary>
        private string TentTextureKey;

        /// <summary>The last texture loaded for the tent.</summary>
        private Texture2D TentTexture;


        /*********
        ** Accessors
        *********/
        /// <summary>How long the tent must be used before a save is triggered.</summary>
        internal static TimeSpan UseDelay { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public TentTool()
        {
            this.Name = "Tent";
            this.description = this.loadDescription();

            this.numAttachmentSlots.Value = 0;
            this.IndexOfMenuItemView = 0;
            this.Stack = 1;
        }

        /// <inheritdoc />
        public override int salePrice()
        {
            return 5000;
        }

        /// <inheritdoc />
        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            if (!this.IsSaving)
            {
                this.StartedUsing = this.GetTicks();
                who.canMove = false;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool onRelease(GameLocation location, int x, int y, Farmer who)
        {
            if (this.IsUsing && !this.IsSaving)
                this.CancelUse(who);

            return true;
        }

        public override void tickUpdate(GameTime time, Farmer who)
        {
            // cancel save mode
            if (this.IsSaving)
            {
                TimeSpan savingTime = this.GetTimeSince(this.StartedSaving!.Value);
                if (savingTime > this.ResetDelay)
                {
                    this.CancelUse(who);
                    this.StartedSaving = null;
                    return;
                }
            }

            // handle use mode
            if (this.IsUsing)
            {
                // animate
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

                // save if done
                TimeSpan useTime = this.GetTimeSince(this.StartedUsing!.Value);
                if (useTime > TentTool.UseDelay)
                {
                    this.CancelUse(who);

                    this.StartedSaving = this.GetTicks();
                    Mod.Instance.RememberLocation();
                    Game1.player.isInBed.Value = true;
                    Game1.NewDay(0);
                }
            }
        }

        public override void drawInMenu(SpriteBatch b, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            this.ItemTexture ??= Mod.Instance.Helper.Content.Load<Texture2D>("assets/tent-item.png");

            b.Draw(this.ItemTexture, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 16), color * transparency, 0, new Vector2(8f, 8f), Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch b)
        {
            this.CurrentParentTileIndex = this.IndexOfMenuItemView = -999;

            if (this.IsSaving || this.IsUsing)
            {
                // get transparency
                Color color = Color.White;
                if (!this.IsSaving && this.IsUsing && this.GetTimeSince(this.StartedUsing!.Value) < TentTool.UseDelay)
                {
                    var useTime = this.GetTimeSince(this.StartedUsing!.Value);
                    color *= (0.2f + (float)useTime.TotalSeconds / (float)TentTool.UseDelay.TotalSeconds * 0.6f);
                }

                // draw
                Vector2 pos = Game1.GlobalToLocal(Game1.player.getStandingPosition());
                pos.Y -= Game1.tileSize * 2;

                Texture2D texture = this.GetOrLoadTexture();
                b.Draw(texture, pos, new Rectangle(0, 0, 48, 80), color, 0, new Vector2(24, 40), Game1.pixelZoom, SpriteEffects.None, 0.999999f);
            }
        }

        /// <inheritdoc />
        protected override string loadDescription()
        {
            return I18n.Tent_Description();
        }

        /// <inheritdoc />
        protected override string loadDisplayName()
        {
            return I18n.Tent_Name();
        }

        /// <inheritdoc />
        public override bool canBeDropped()
        {
            return true;
        }

        /// <inheritdoc />
        public override bool canBeTrashed()
        {
            return true;
        }

        /// <inheritdoc />
        public override Item getOne()
        {
            return new TentTool();
        }


        /*********
        ** Public methods
        *********/
        /// <summary>Clean up when the player stops using the tool.</summary>
        /// <param name="player">The player who was using it.</param>
        private void CancelUse(Farmer player)
        {
            this.StartedUsing = null;
            player.canMove = true;
        }

        /// <summary>Get the current timestamp in ticks.</summary>
        private long GetTicks()
        {
            return DateTime.UtcNow.Ticks;
        }

        /// <summary>Get the time elapsed since the given timestamp.</summary>
        /// <param name="prevTicks">The previous timestamp in ticks.</param>
        private TimeSpan GetTimeSince(long prevTicks)
        {
            return TimeSpan.FromTicks(this.GetTicks() - prevTicks);
        }

        /// <summary>Get the cached tent texture, loading it if needed.</summary>
        private Texture2D GetOrLoadTexture()
        {
            string key = $"assets/{Game1.currentSeason}_tent";

            if (this.TentTextureKey != key || this.TentTexture == null || this.TentTexture.IsDisposed)
            {
                this.TentTextureKey = key;
                this.TentTexture = Mod.Instance.Helper.Content.Load<Texture2D>(key);
            }

            return this.TentTexture;
        }
    }
}
