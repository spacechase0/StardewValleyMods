using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;

namespace MayoRain
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private IJsonAssetsApi ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            helper.Events.GameLoop.TimeChanged += this.GameLoop_TimeChanged;
            helper.Events.Content.AssetRequested += this.Content_AssetRequested;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "json-assets"));
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (!Game1.player.currentLocation.IsOutdoors || !Game1.IsRainingHere())
                return;


            if (Game1.player.ActiveObject?.ItemId == "Empty Mayo Jar")
            {
                int id = 306;
                if (Game1.random.NextDouble() < 0.1)
                    id = 307;
                else if ( Game1.random.NextDouble() < 0.01)
                    id = 308;
                else if (Game1.random.NextDouble() < 0.01)
                    id = 807;

                if (Game1.player.addItemToInventoryBool(new StardewValley.Object(id.ToString(), 1)))
                    Game1.player.reduceActiveItemByOne();
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/rain"))
                e.LoadFromModFile< Texture2D >("assets/rain.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
        }
    }
}
