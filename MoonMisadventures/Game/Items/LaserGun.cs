using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures;
using MoonMisadventures.Game.Locations;
using MoonMisadventures.Game.Projectiles;
using Netcode;
using StardewValley;

namespace StardewValley.Tools
{
    [XmlType( "Mods_spacechase0_MoonMisadventures_LaserGun" )]
    public class LaserGun : Tool
    {
        public LaserGun()
        {
            this.Name = this.BaseName = "LaserGun";
            this.InstantUse = true;
        }

        protected override string loadDisplayName()
        {
            return I18n.Tool_LaserGun_Name();
        }

        protected override string loadDescription()
        {
            return I18n.Tool_LaserGun_Description();
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

            var spot = Mod.instance.Helper.Input.GetCursorPosition();

            Vector2 vel = spot.AbsolutePixels - Game1.player.StandingPixel.ToVector2();
            vel.Normalize();
            vel *= 10;

            var proj = new LaserProjectile(Game1.player.StandingPixel.ToVector2(), vel, who);
            location.projectiles.Add(proj);

            location.playSound("mm_laser", Game1.player.Tile);
        }


        protected override Item GetOneNew()
        {
            return new LaserGun();
        }
    }
}
