using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BlahajBlast;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;

namespace StardewValley.Tools
{
    [XmlType("Mods_spacechase0_BlahajBlast_LaserGun")]
    public class SharknadoGun : Tool
    {
        private double lastUse;

        public SharknadoGun()
        {
            this.Name = this.BaseName = "BlahajBlast";
            this.InstantUse = true;
        }

        protected override string loadDisplayName()
        {
            return I18n.Tool_BlahajBlast_Name();
        }

        protected override string loadDescription()
        {
            return I18n.Tool_BlahajBlast_Description();
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override void DoFunction( GameLocation location, int x, int y, int power, Farmer who )
        {
            who.CanMove = true;
            who.UsingTool = false;
            who.canReleaseTool = true;

            if (Game1.currentGameTime.TotalGameTime.TotalSeconds - lastUse <= 0.25f)
                return;

            lastUse = Game1.currentGameTime.TotalGameTime.TotalSeconds;

            var spot = Mod.instance.Helper.Input.GetCursorPosition();

            Vector2 vel = spot.AbsolutePixels - Game1.player.getStandingPosition();
            vel.Normalize();
            vel *= 10;
            for (int i = Game1.random.Next( 8, 17 ); i >= 0; i--)
            {
                var proj = new SharknadoPellet(Game1.player.getStandingPosition(), vel, who);
                location.projectiles.Add(proj);
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(Mod.sharkTex, location + new Vector2(32f, 32f), Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, IndexOfMenuItemView), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            var ret = new SharknadoGun();
            ret._GetOneFrom(this);
            return ret;
        }
    }
}
