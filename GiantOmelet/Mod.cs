using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

// Frying pan sound effect is (supposedly) from TF2: https://www.youtube.com/watch?v=08EqQPIvHOU

namespace GiantOmelet
{
    public class Mod : StardewModdingAPI.Mod
    {
        internal static Mod instance;
        internal bool got = false;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            SoundEffect panhit = SoundEffect.FromFile(Path.Combine(Helper.DirectoryPath, "assets", "hitsound.wav"));
            Game1.soundBank.AddCue(new CueDefinition("panhit", panhit, 3));

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Player.Warped += OnWarped;

            GameLocation.RegisterTileAction("GiantOmelet", OnActionActivated);
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            got = false;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name != "Desert")
                return;

            string imgPath = Helper.ModContent.GetInternalAssetName("assets/janky_omelet.png").Name;
            if (e.NewLocation.Map.TileSheets.FirstOrDefault(ts => ts.ImageSource == imgPath) != null)
                return;

            var ts = new xTile.Tiles.TileSheet(e.NewLocation.Map, imgPath, new(8, 6), new(16, 16));
            e.NewLocation.Map.AddTileSheet(ts);

            int x = 28, y = 2;
            for (int ix = 0; ix < 8; ++ix)
            {
                for (int iy = 0; iy < 6; ++iy)
                {
                    string layerName = iy < 3 ? "Front" : "Buildings";
                    var layer = e.NewLocation.Map.Layers.First(l => l.Id == layerName);
                    layer.Tiles[x + ix, y + iy] = new xTile.Tiles.StaticTile(layer, ts, xTile.Tiles.BlendMode.Alpha, ix + iy * 8);
                    if (iy >= 3)
                        Game1.currentLocation.setTileProperty(x + ix, y + iy, "Buildings", "Action", "GiantOmelet");
                }
            }

            e.NewLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
        }

        private bool OnActionActivated(GameLocation loc, string[] args, Farmer farmer, Point tile )
        {
            if (got)
                WrathOfGus();
            else
            {
                got = true;

                int qual = Game1.random.Next(4);
                if (qual == 3) qual = 4;
                var yum = new StardewValley.Object("195", Game1.random.Next(3) + 1, quality: qual);
                Game1.player.addItemByMenuIfNecessary( yum );
                Game1.player.holdUpItemThenMessage(yum, true);
            }

            return true;
        }

        private void WrathOfGus()
        {
            var gus = new TemporaryAnimatedSprite("Characters\\Gus", new Rectangle(32, 192, 16, 32), Game1.player.GetBoundingBox().Center.ToVector2() - new Vector2( 32, 192 ), false, 0.025f, Color.White)
            {
                scale = 4,
                layerDepth = 1,
            };
            // I'd like to add some puffs of smoke that appear in front, but lazy
            Game1.currentLocation.TemporarySprites.Add(gus);
            Game1.currentLocation.playSound("panhit");
            Game1.player.takeDamage(5, true, null);
        }
    }
}
