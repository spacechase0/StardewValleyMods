using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI.Enums;
using StardewValley;
using StardewValley.Locations;
using xTile.Tiles;

namespace ConstellationVoyage
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.Specialized.LoadStageChanged += this.Specialized_LoadStageChanged;
            Helper.Events.Player.Warped += this.Player_Warped;

            SpaceEvents.ActionActivated += this.SpaceEvents_ActionActivated;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Specialized_LoadStageChanged(object sender, StardewModdingAPI.Events.LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.CreatedLocations || e.NewStage == LoadStage.SaveAddedLocations)
            {
                Game1.locations.Add(new VoyageLocation(Helper.ModContent));
            }
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e.NewLocation is BoatTunnel bt && true /* TODO: has flag */ )
            {
                var ts = new xTile.Tiles.TileSheet("zz_springoutdoors", bt.Map, "Maps/spring_outdoorsTileSheet", new(25, 69), new(16, 16));
                bt.Map.AddTileSheet(ts);
                var layer = bt.Map.GetLayer("Buildings");
                var tile = new StaticTile(layer, ts, BlendMode.Alpha, 1866);
                tile.Properties.Add("Action", "EnchantBoat");
                layer.Tiles[new(10, 10)] = tile;
                Game1.mapDisplayDevice.LoadTileSheet(ts);
            }
        }

        private void SpaceEvents_ActionActivated(object sender, EventArgsAction e)
        {
            if (e.Action == "EnchantBoat" && Game1.player.currentLocation is BoatTunnel bt)
            {
                Helper.Reflection.GetField<Texture2D>(bt, "boatTexture").SetValue(Helper.ModContent.Load<Texture2D>("assets/magic-boat.png"));
                Game1.playSound("wand");

                Game1.player.CanMove = false;

                Vector2 boatPos = bt.GetBoatPosition();
                var mp = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                for (int i = 0; i < 50; i++)
                {
                    mp.broadcastSprites(bt, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)boatPos.X, (int)boatPos.X + 156 * 4), Game1.random.Next((int)boatPos.Y, (int)boatPos.Y + 118 * 4)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false)
                    {
                        layerDepth = 1,
                    });
                }

                DelayedAction.functionAfterDelay(() =>
                {
                    Game1.playSound("doorClose");
                    NPC willy = Game1.getCharacterFromName("Willy");
                    GameLocation willyOldLoc = willy.currentLocation;
                    Vector2 willyOldPos = willy.getTileLocation();
                    Game1.warpCharacter(willy, bt, new Vector2(6, 11));
                    willy.showTextAboveHead(I18n.WillyBoatExclamation());
                    Helper.Reflection.GetField<float>(willy, "textAboveHeadAlpha").SetValue(1);

                    DelayedAction.fadeAfterDelay(() =>
                    {
                        Game1.warpCharacter(willy, willyOldLoc, willyOldPos);
                        willy.warpToPathControllerDestination();
                        Game1.warpFarmer("Custom_ConstellationVoyageLocation", 53, 54, false);
                    }, 3000);
                }, 1000);
            }
        }
    }
}
