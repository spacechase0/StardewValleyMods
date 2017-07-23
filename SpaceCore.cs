using Harmony;
using Microsoft.Xna.Framework;
using SpaceCore.Events;
using SpaceCore.Overrides;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SpaceCore
{
    public class SpaceCore : Mod
    {
        internal static SpaceCore instance;

        public SpaceCore()
        {
        }

        public override void Entry(IModHelper helper)
        {
            base.Entry(helper);
            instance = this;

            GameEvents.UpdateTick += onUpdate;

            SaveEvents.AfterLoad += onLoad;
            SaveEvents.AfterSave += onSave;

            Commands.register();

            var harmony = HarmonyInstance.Create("spacechase0.SpaceCore");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            try
            {
                Log.debug("test");
                NewGame1.hijack(harmony);
                NewMeleeWeapon.hijack(harmony);
                NewUtility.hijack(harmony);
            }
            catch (Exception e)
            {
                Log.error("Exception hijacking methods: " + e);
            }
        }

        private int prevLoaderNum = 0;
        private void onUpdate( object sender, EventArgs args )
        {
            if (Game1.currentLoader != null)
            {
                if (Game1.currentLoader.Current == 25 && prevLoaderNum != 25)
                {
                    SpaceEvents.InvokeOnBlankSave();
                }
                prevLoaderNum = Game1.currentLoader.Current;
            }
            //Log.debug("L:" + (Game1.currentLoader != null ? Game1.currentLoader.Current:-1));
        }

        private void onLoad(object sender, EventArgs args)
        {
            var data = Helper.ReadJsonFile<Sleep.Data>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"));
            if (data == null || data.Year != Game1.year || data.Season != Game1.currentSeason || data.Day != Game1.dayOfMonth)
                return;

            Log.debug("Previously slept in a tent, replacing player position.");

            var loc = Game1.getLocationFromName(data.Location);
            if (loc == null || loc.name == festivalLocation())
            {
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));
                return;
            }

            if (loc is MineShaft)
            {
                Log.trace("Slept in a mine.");
                var pos = (loc as MineShaft).enterMine(Game1.player, data.MineLevel, false);
                data.X = pos.X * Game1.tileSize;
                data.Y = pos.Y * Game1.tileSize;
            }

            Game1.player.currentLocation = Game1.currentLocation = loc;
            Game1.player.position = new Vector2(data.X, data.Y);
        }

        private void onSave(object sender, EventArgs args)
        {
            if (!Sleep.SaveLocation)
                return;

            Log.debug("Saving tent sleep data");

            if (Game1.player.currentLocation.name == festivalLocation())
            {
                Log.trace("There'll be a festival here tomorrow, canceling");
                Game1.addHUDMessage(new HUDMessage("You camped out where the festival was, so you have returned home."));

                var house = Game1.getLocationFromName("FarmHouse") as FarmHouse;
                Game1.player.currentLocation = Game1.currentLocation = house;
                Game1.player.position = new Vector2(house.getBedSpot().X * Game1.tileSize, house.getBedSpot().Y * Game1.tileSize);
                Sleep.SaveLocation = false;
                return;
            }

            var data = new Sleep.Data();
            data.Location = Game1.currentLocation.name;
            data.X = Game1.player.position.X;
            data.Y = Game1.player.position.Y;

            data.Year = Game1.year;
            data.Season = Game1.currentSeason;
            data.Day = Game1.dayOfMonth;

            if (Game1.currentLocation is MineShaft)
            {
                data.MineLevel = (Game1.currentLocation as MineShaft).mineLevel;
            }

            Helper.WriteJsonFile<Sleep.Data>(Path.Combine(Constants.CurrentSavePath, "sleepy-eye.json"), data);
            Sleep.SaveLocation = false;
        }

        // TODO: Move somewhere more sensible (and make public)?
        internal string festivalLocation()
        {
            try
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + (object)Game1.dayOfMonth)["conditions"].Split('/')[0];
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
