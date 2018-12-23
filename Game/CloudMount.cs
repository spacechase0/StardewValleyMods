using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;

namespace Magic.Game
{
    public class CloudMount : Horse
    {
        private Texture2D tex = Content.loadTexture("entities/cloud.png");

        public CloudMount()
        {
            Name = displayName = "";
        }

        //private bool dismountedOnce = false;
        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            if (rider == null || rider.mount != this)
            {
                /*if (!dismountedOnce)
                {
                    rider = prevRider;
                    checkAction(prevRider, currentLocation);
                    dismountedOnce = true;
                }
                else*/ if ( !dismounting )
                    currentLocation.characters.Remove(this);
                return;
            }

            if ( !location.IsOutdoors )
            {
                checkAction(rider, location);
            }

            rider.speed = 10;
            rider.temporaryImpassableTile = new Rectangle((int)rider.position.X - Game1.tileSize, (int)rider.position.Y - Game1.tileSize, Game1.tileSize * 3, Game1.tileSize * 3);
        }

        public override void draw(SpriteBatch b)
        {
            //Game1.player.draw(b);
            b.Draw(tex, getLocalPosition(Game1.viewport) + new Vector2( -Game1.tileSize * 0.90f, -Game1.tileSize * 0.75f ), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
        }

        public override bool checkAction(Farmer who, GameLocation l)
        {
            if (rider == null)
                return false;

            this.dismounting.Value = true;
            this.farmerPassesThrough = false;
            this.rider.temporaryImpassableTile = Rectangle.Empty;
            Vector2 tileForCharacter = Utility.recursiveFindOpenTileForCharacter((Character)this.rider, this.rider.currentLocation, this.rider.getTileLocation(), 9*9);
            this.dismounting.Value = false;
            this.Halt();
            if (!tileForCharacter.Equals(Vector2.Zero) /*&& (double)Vector2.Distance(tileForCharacter, this.rider.getTileLocation()) < 2.0*/)
            {
                this.rider.yJumpVelocity = 6f;
                this.rider.yJumpOffset = -1;
                Game1.playSound("dwop");
                this.rider.freezePause = 5000;
                this.rider.Halt();
                this.rider.xOffset = 0.0f;
                this.dismounting.Value = true;
                Mod.instance.Helper.Reflection.GetField<Vector2>(this, "dismountTile").SetValue(tileForCharacter);
                //Log.trace("dismount tile: " + tileForCharacter.ToString());
            }
            else
                this.dismount();
            return true;
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle((int)position.X, (int)position.Y, 0, 0);
        }
    }
}
