using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace BetterMeteorites
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            SpaceShared.Log.Monitor = this.Monitor;

            helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            Game1.getFarm().resourceClumps.OnValueRemoved += this.OnClumpRemoved;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Game1.getFarm().resourceClumps.OnValueRemoved += this.OnClumpRemoved;
        }

        private void OnClumpRemoved(ResourceClump value)
        {
            if (value.parentSheetIndex.Value == ResourceClump.meteoriteIndex)
            {
                Random r = new Random((int)value.Tile.X * 1000 + (int)value.Tile.Y);
                Game1.createMultipleObjectDebris(SObject.stone.ToString(), (int)value.Tile.X, (int)value.Tile.Y, 75 + r.Next(175));
                Game1.createMultipleObjectDebris(SObject.coal.ToString(), (int)value.Tile.X, (int)value.Tile.Y, 20 + r.Next(55));
                Game1.createMultipleObjectDebris(SObject.iridium.ToString(), (int)value.Tile.X, (int)value.Tile.Y, 50 + r.Next(100));
                Game1.createMultipleObjectDebris("535", (int)value.Tile.X, (int)value.Tile.Y, 7 + r.Next(15));
                Game1.createMultipleObjectDebris("536", (int)value.Tile.X, (int)value.Tile.Y, 7 + r.Next(15));
                Game1.createMultipleObjectDebris("537", (int)value.Tile.X, (int)value.Tile.Y, 7 + r.Next(15));
                Game1.createMultipleObjectDebris("749", (int)value.Tile.X, (int)value.Tile.Y, 3 + r.Next(9));
            }
        }
    }
}
