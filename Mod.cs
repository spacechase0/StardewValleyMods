using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepyEye
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            base.Entry(helper);
            instance = this;

            SaveEvents.AfterLoad += onLoad;
            SaveEvents.AfterSave += onSave;
            MenuEvents.MenuChanged += onMenuChange;

            Entoarox.Framework.EntoFramework.GetTypeRegistry().RegisterType<TentTool>();
        }

        private void onLoad(object sender, EventArgs args)
        {
            var data = Helper.ReadJsonFile<SleepData>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
            if (data == null || data.Year != Game1.year || data.Season != Game1.currentSeason || data.Day != Game1.dayOfMonth )
                return;

            Log.debug("Previously slept in a tent, replacing player position.");

            var loc = Game1.getLocationFromName(data.Location);
            if (loc == null || loc.name == festivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));
                return;
            }

            if ( loc is MineShaft )
            {
                Log.trace("Slept in a mine.");
                var pos = (loc as MineShaft).enterMine(Game1.player, data.MineLevel, false);
                data.X = pos.X * Game1.tileSize;
                data.Y = pos.Y * Game1.tileSize;
            }

            Game1.player.currentLocation = Game1.currentLocation = loc;
            Game1.player.position = new Vector2(data.X, data.Y);
        }

        public bool saveSleepLocation = false;
        private void onSave(object sender, EventArgs args)
        {
            if (!saveSleepLocation)
                return;

            Log.debug("Saving tent sleep data");

            if ( Game1.player.currentLocation.name == festivalLocation())
            {
                Log.trace("There'll be a festival here tomorrow, canceling");
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));

                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                Game1.player.currentLocation = Game1.currentLocation = house;
                Game1.player.position = new Vector2(house.getBedSpot().X * Game1.tileSize, house.getBedSpot().Y * Game1.tileSize);
                saveSleepLocation = false;
                return;
            }

            var data = new SleepData();
            data.Location = Game1.currentLocation.name;
            data.X = Game1.player.position.X;
            data.Y = Game1.player.position.Y;

            data.Year = Game1.year;
            data.Season = Game1.currentSeason;
            data.Day = Game1.dayOfMonth;

            if ( Game1.currentLocation is MineShaft )
            {
                data.MineLevel = (Game1.currentLocation as MineShaft).mineLevel;
            }

            Helper.WriteJsonFile<SleepData>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"), data);
            saveSleepLocation = false;
        }

        private void onMenuChange( object sender, EventArgsClickableMenuChanged args )
        {
            var menu = args.NewMenu as ShopMenu;
            if (menu == null)
                return;

            if ( menu.portraitPerson.name == "Pierre" )
            {
                Log.debug("Adding tent to shop");

                var forSale = Helper.Reflection.GetPrivateValue<List<Item>>(menu, "forSale");
                var itemPriceAndStock = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(menu, "itemPriceAndStock");

                var item = new TentTool();
                forSale.Add(item);
                itemPriceAndStock.Add(item, new int[] { item.salePrice(), item.Stack });
            } 
        }

        private string festivalLocation()
        {
            try
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (object)Game1.dayOfMonth)["conditions"].Split('/')[0];
            }
            catch ( Exception e )
            {
                return null;
            }
        }
    }
}
