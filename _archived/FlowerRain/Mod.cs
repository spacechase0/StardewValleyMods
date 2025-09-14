using System;
using System.Collections.Generic;
using System.Buffers;
using FlowerRain.Framework;
using FlowerRain.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace FlowerRain
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Config Config;
        private readonly Dictionary<string, List<FlowerData>> Fd = new();
        private Texture2D InvisibleRain;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.Config = helper.ReadConfig<Config>();

            int pixelCount = 256 * 64;
            Color[] transparent = ArrayPool<Color>.Shared.Rent(pixelCount);
            try
            {
                Array.Fill(transparent, Color.Transparent, 0, pixelCount);
                this.InvisibleRain = new Texture2D(Game1.graphics.GraphicsDevice, 256, 64);
                this.InvisibleRain.SetData(transparent, 0, pixelCount);
            }
            finally
            {
                ArrayPool<Color>.Shared.Return(transparent);
            }

            this.BuildFlowerData(useWhitelist: true);

            helper.Events.GameLoop.GameLaunched += this.GameLaunched;
            helper.Events.Content.AssetRequested += (_, e) =>
            {
                if (e.NameWithoutLocale.IsEquivalentTo("TileSheets\\rain"))
                    e.LoadFrom(() => this.InvisibleRain, AssetLoadPriority.Exclusive);
            };

            HarmonyPatcher.Apply(this,
                new Game1Patcher(this.Fd)
            );
        }

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.Config = new Config(),
                    save: () => this.Helper.WriteConfig(Mod.Config),
                    titleScreenOnly: true
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_VanillaFlowersOnly_Name,
                    tooltip: I18n.Config_VanillaFlowersOnly_Tooltip,
                    getValue: () => Mod.Config.VanillaFlowersOnly,
                    setValue: value =>
                    {
                        Mod.Config.VanillaFlowersOnly = value;
                        if (Mod.Config.VanillaFlowersOnly)
                            this.BuildFlowerData(useWhitelist: true);
                    }
                );
            }

            var jsonAssets = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (jsonAssets != null)
                jsonAssets.IdsAssigned += this.JaIdsAssigned;
        }

        private void JaIdsAssigned(object sender, EventArgs e)
        {
            if (!Mod.Config.VanillaFlowersOnly)
            {
                this.BuildFlowerData(useWhitelist: false);
            }
        }

        private void BuildFlowerData(bool useWhitelist)
        {
            var objData = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            var spring = new List<FlowerData>();
            var summer = new List<FlowerData>();
            var fall = new List<FlowerData>();
            var winter = new List<FlowerData>();

            var whitelist = new List<int>(new[] { 591, 593, 595, 597, 376, 402, 418 });

            foreach (var crop in cropData)
            {
                string[] toks = crop.Value.Split('/');
                string[] seasons = toks[1].Split(' ');
                int product = int.Parse(toks[3]);

                int category = int.Parse(objData[product].Split('/')[3].Split(' ')[1]);

                if (category != SObject.flowersCategory || useWhitelist && !whitelist.Contains(product))
                    continue;

                List<Color> cols = new List<Color>(new[] { Color.White });
                if (toks[8].StartsWith("true "))
                {
                    cols.Clear();
                    string[] colToks = toks[8].Split(' ');
                    for (int i = 1; i < colToks.Length; i += 3)
                    {
                        int r = int.Parse(colToks[i + 0]);
                        int g = int.Parse(colToks[i + 1]);
                        int b = int.Parse(colToks[i + 2]);
                        var col = new Color(r, g, b);
                        cols.Add(col);
                    }
                }

                foreach (var col in cols)
                {
                    FlowerData fd = new FlowerData
                    {
                        Index = product,
                        Color = col
                    };

                    foreach (string season in seasons)
                    {
                        switch (season)
                        {
                            case "spring": spring.Add(fd); break;
                            case "summer": summer.Add(fd); break;
                            case "fall": fall.Add(fd); break;
                            case "winter": winter.Add(fd); break;
                        }
                    }
                }
            }

            this.Fd.Clear();
            this.Fd.Add("spring", spring);
            this.Fd.Add("summer", summer);
            this.Fd.Add("fall", fall);
            this.Fd.Add("winter", winter);
        }
    }
}
