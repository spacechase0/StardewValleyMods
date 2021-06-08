using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Tiles;

namespace ArcadeRoom
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private bool patchedMap = false;

        public Queue<Vector2> machineSpots = new Queue<Vector2>();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.Player.Warped += onWarped;

            SpaceEvents.OnBlankSave += onBlankSave;
        }

        internal Vector2 ReserveNextMachineSpot()
        {
            return machineSpots.Dequeue();
        }

        private Api api;
        public override object GetApi()
        {
            return (api = new Api());
        }

        private void onBlankSave(object sender, EventArgs e)
        {
            patchedMap = false;

            var arcade = new GameLocation(Helper.Content.GetActualAssetKey("assets/Arcade.tbin"), "Arcade");
            Game1.locations.Add(arcade);
            for (int ix = 0; ix < arcade.Map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < arcade.Map.Layers[0].LayerHeight; ++iy)
                {
                    if (!string.IsNullOrEmpty(arcade.doesTileHaveProperty(ix, iy, "ArcadeSpot", "Back")))
                        machineSpots.Enqueue(new Vector2(ix, iy));
                }
            }
            api.InvokeOnRoomSetup();
        }

        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name != "Saloon" /*|| patchedMap*/ )
                return;
            patchedMap = true;

            var map = e.NewLocation.Map;

            var ts = new TileSheet(e.NewLocation.Map, Helper.Content.GetActualAssetKey("assets/tiles.png"), new xTile.Dimensions.Size(8, 8), new xTile.Dimensions.Size(16, 16));
            ts.Id = "\u03A9" + ts.Id;
            e.NewLocation.Map.AddTileSheet(ts);
            e.NewLocation.Map.LoadTileSheets(Game1.mapDisplayDevice);

            map.GetLayer("Front").Tiles[33, 14] = map.GetLayer("Buildings").Tiles[33, 14];
            map.GetLayer("Front").Tiles[34, 14] = map.GetLayer("Buildings").Tiles[34, 14];
            map.GetLayer("Front").Tiles[35, 14] = map.GetLayer("Buildings").Tiles[35, 14];
            map.GetLayer("Buildings").Tiles[33, 14] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 24);
            map.GetLayer("Buildings").Tiles[34, 14] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 25);
            map.GetLayer("Buildings").Tiles[35, 14] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 26);
            map.GetLayer("Buildings").Tiles[33, 15] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 32);
            map.GetLayer("Buildings").Tiles[34, 15] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 33);
            map.GetLayer("Buildings").Tiles[35, 15] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 34);
            map.GetLayer("Buildings").Tiles[33, 16] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 40);
            map.GetLayer("Buildings").Tiles[34, 16] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 41);
            map.GetLayer("Buildings").Tiles[35, 16] = new StaticTile(map.GetLayer("Buildings"), ts, BlendMode.Alpha, 42);
            map.GetLayer("Buildings").Tiles[34, 16].Properties.Add("Action", new xTile.ObjectModel.PropertyValue("Warp 8 11 Arcade"));

            map.GetLayer("Front").Tiles[33, 16] = null;
            map.GetLayer("Buildings").Tiles[33, 17] = null;
            map.GetLayer("Front").Tiles[35, 16] = null;
            map.GetLayer("Buildings").Tiles[35, 17] = null;
        }
    }
}
