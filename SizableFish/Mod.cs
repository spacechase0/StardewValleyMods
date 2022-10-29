using System;
using System.Collections.Generic;
using HarmonyLib;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace SizableFish
{
    /*
    public class Configuration
    {
        public bool RandomizeFishWithoutStoredSize { get; set; } = false;
    }
    */

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        //public static Configuration Config { get; set; }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            //Config = Helper.ReadConfig<Configuration>();

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            /*
            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));
                gmcm.AddBoolOption(ModManifest, () => Config.RandomizeFishWithoutStoredSize, (b) => Config.RandomizeFishWithoutStoredSize = b, () => I18n.Config_RandomizeUnstored_Name(), () => I18n.Config_RandomizeUnstored_Description());
            }
            */
        }
    }

    [HarmonyPatch(typeof(TankFish), MethodType.Constructor, new Type[] { typeof(FishTankFurniture), typeof(Item) })]
    public class TankFishScalePatch
    {
        public static void Postfix(TankFish __instance, Item item)
        {
            var dataDict = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            if (!dataDict.ContainsKey(item.ParentSheetIndex))
                return;

            string[] data = dataDict[item.ParentSheetIndex].Split('/');
            if (data[1] == "trap")
                return;

            int min = int.Parse(data[3]);
            int max = int.Parse(data[4]);
            float size = (max + min) / 2;
            if (true)//Mod.Config.RandomizeFishWithoutStoredSize)
            {
                size = Utility.RandomFloat(min, max);
            }
            __instance.fishScale = 0.3f + size / 40;
        }
    }
}
