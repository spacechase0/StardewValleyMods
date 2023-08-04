using System;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace BlahajBlast
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static Texture2D sharkTex;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            sharkTex = Helper.ModContent.Load<Texture2D>("assets/shark.png");

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
            Helper.Events.Display.MenuChanged += this.Display_MenuChanged;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

            Helper.ConsoleCommands.Add("player_giveblahajblast", "...", (cmd, args) => Game1.player.addItemByMenuIfNecessary(new SharknadoGun()));
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(SharknadoGun));
            sc.RegisterSerializerType(typeof(SharknadoPellet));
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            lastGender = null;
        }

        bool? lastGender = null;
        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (Game1.player.IsMale != lastGender)
            {
                if (lastGender.HasValue)
                    Game1.player.addItemToInventory(new SharknadoGun());
                lastGender = Game1.player.IsMale;
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                if (shop.storeContext == "FishShop")
                {
                    var sg = new SharknadoGun();
                    shop.itemPriceAndStock.Add(sg, new int[] { 10000, int.MaxValue });
                    shop.forSale.Add(sg);
                }
            }
        }
    }
}
