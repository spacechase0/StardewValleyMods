using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;

namespace Magic.Framework.Game
{
    internal class CloudMount : Horse
    {
        /*********
        ** Fields
        *********/
        private readonly Texture2D Tex = Content.LoadTexture("entities/cloud.png");


        /*********
        ** Public methods
        *********/
        public CloudMount()
        {
            this.Name = this.displayName = "";
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            if (this.rider == null || this.rider.mount != this)
            {
                /*if (!dismountedOnce)
                {
                    rider = prevRider;
                    checkAction(prevRider, currentLocation);
                    dismountedOnce = true;
                }
                else*/
                if (!this.dismounting.Value)
                    this.currentLocation.characters.Remove(this);
                return;
            }

            if (!location.IsOutdoors)
            {
                this.checkAction(this.rider, location);
            }

            this.rider.speed = 10;
            this.rider.TemporaryPassableTiles.Add(new Rectangle((int)this.rider.position.X - Game1.tileSize, (int)this.rider.position.Y - Game1.tileSize, Game1.tileSize * 3, Game1.tileSize * 3));
        }

        public override void draw(SpriteBatch b)
        {
            //Game1.player.draw(b);
            b.Draw(this.Tex, this.getLocalPosition(Game1.viewport) + new Vector2(-Game1.tileSize * 0.90f, -Game1.tileSize * 0.75f), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
        }

        public override bool checkAction(Farmer who, GameLocation l)
        {
            if (this.rider == null)
                return false;

            this.dismounting.Value = true;
            this.farmerPassesThrough = false;
            this.rider.TemporaryPassableTiles.Clear();
            Vector2 tileForCharacter = Utility.recursiveFindOpenTileForCharacter(this.rider, this.rider.currentLocation, this.rider.Tile, 9 * 9);
            this.dismounting.Value = false;
            this.Halt();
            if (!tileForCharacter.Equals(Vector2.Zero) /*&& (double)Vector2.Distance(tileForCharacter, this.rider.getTileLocation()) < 2.0*/)
            {
                this.rider.yJumpVelocity = 6f;
                this.rider.yJumpOffset = -1;
                this.rider.LocalSound("dwop");
                this.rider.freezePause = 5000;
                this.rider.Halt();
                this.rider.xOffset = 0.0f;
                this.dismounting.Value = true;
                Mod.Instance.Helper.Reflection.GetField<Vector2>(this, "dismountTile").SetValue(tileForCharacter);
                //Log.trace("dismount tile: " + tileForCharacter.ToString());
            }
            else
                this.dismount();
            return true;
        }

        public override Rectangle GetBoundingBox()
        {
            return new((int)this.position.X, (int)this.position.Y, 0, 0);
        }
    }
}
