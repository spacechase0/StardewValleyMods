using MoreRings.Other;
using SpaceCore.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Magic
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static SaveData Data { get; private set; }
        public static Configuration Config { get; private set; }
        
        private static JsonAssetsApi ja;

        public Mod()
        {
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;

            Config = Helper.ReadConfig<Configuration>();

            Monitor.Log("HELLO?", LogLevel.Alert);

            GameEvents.FirstUpdateTick += firstUpdate;
            SaveEvents.AfterLoad += afterLoad;
            SaveEvents.AfterSave += afterSave;

            Magic.init();
        }

        private void firstUpdate(object sender, EventArgs args)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.error("No Json Assets API???");
                return;
            }
            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void afterLoad(object sender, EventArgs args)
        {
            try
            {
                Log.info("Loading save data (\"" + SaveData.FilePath + "\")...");
                Data = Helper.ReadJsonFile<SaveData>(SaveData.FilePath) ?? new SaveData();
            }
            catch ( Exception e )
            {
                Log.warn("Exception loading save data: " + e);
                Log.warn("Using default");
                Data = new SaveData();
            }
        }
        
        private void afterSave(object sender, EventArgs args)
        {
            Log.info("Saving save data (\"" + SaveData.FilePath + "\")...");
            Helper.WriteJsonFile(SaveData.FilePath, Data);
        }
    }
}
