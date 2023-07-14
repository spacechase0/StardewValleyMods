using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MageDelve
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            AlchemyRecipes.Init();

            SoundEffect alchemyParticlize = SoundEffect.FromFile(Path.Combine(Helper.DirectoryPath, "assets", "alchemy-particlize.wav"));
            Game1.soundBank.AddCue(new CueDefinition("spacechase0.MageDelve_alchemy_particlize", alchemyParticlize, 3));
            SoundEffect alchemySynthesize = SoundEffect.FromFile(Path.Combine(Helper.DirectoryPath, "assets", "alchemy-synthesize.wav"));
            Game1.soundBank.AddCue(new CueDefinition("spacechase0.MageDelve_alchemy_synthesize", alchemySynthesize, 3));

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;

            Helper.ConsoleCommands.Add("magedelve_alchemy", "...", OnAlchemyCommand);

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit((asset) =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict.Add("spacechase0.MageDelve_EarthEssence", $"Earth Essence/25/-300/Basic -28/{I18n.Item_EarthEssence_Name()}/{I18n.Item_EarthEssence_Description()}////0/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_AirEssence", $"Air Essence/25/-300/Basic -28/{I18n.Item_AirEssence_Name()}/{I18n.Item_AirEssence_Description()}////1/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_WaterEssence", $"Water Essence/25/-300/Basic -28/{I18n.Item_WaterEssence_Name()}/{I18n.Item_WaterEssence_Description()}////2/spacechase0.MageDelve\\assets\\essences.png");
                    dict.Add("spacechase0.MageDelve_FireEssence", $"Fire Essence/25/-300/Basic -28/{I18n.Item_FireEssence_Name()}/{I18n.Item_FireEssence_Description()}////3/spacechase0.MageDelve\\assets\\essences.png");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MageDelve/assets/essences.png"))
                e.LoadFromModFile<Texture2D>("assets/essences.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
        }

        private void OnAlchemyCommand(string arg1, string[] arg2)
        {
            if (!Context.IsPlayerFree)
                return;

            Game1.activeClickableMenu = new AlchemyMenu();
        }
    }
}
