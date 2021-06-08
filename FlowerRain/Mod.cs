using System;
using System.Collections.Generic;
using System.Linq;
using FlowerRain.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace FlowerRain
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;
        public static Config config;
        private readonly Dictionary<string, List<FlowerData>> fd = new();
        private Texture2D invisibleRain;

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("TileSheets\\rain");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)invisibleRain;
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            config = helper.ReadConfig<Config>();

            helper.Events.GameLoop.GameLaunched += gameLaunched;

            // https://stackoverflow.com/a/9664937/1687492
            Color[] transparent = Enumerable.Range(0, 256 * 64).Select(p => Color.Transparent).ToArray();
            invisibleRain = new Texture2D(Game1.graphics.GraphicsDevice, 256, 64);
            invisibleRain.SetData(transparent);

            BuildFlowerData(useWhitelist: true);

            HarmonyPatcher.Apply(this,
                new Game1Patcher(this.fd)
            );
        }

        private void gameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.RegisterModConfig(ModManifest, () => config = new Config(), () => Helper.WriteConfig(config));
                gmcm.RegisterSimpleOption(
                    ModManifest,
                    "Use Vanilla Flowers Only",
                    "Only use vanilla flowers in the flower rain",
                    () => config.VanillaFlowersOnly,
                    b =>
                    {
                        config.VanillaFlowersOnly = b;
                        if (config.VanillaFlowersOnly)
                            BuildFlowerData(useWhitelist: true);
                    });
            }

            var ja = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if (ja != null)
            {
                ja.IdsAssigned += jaIdsAssigned;
            }
        }

        private void jaIdsAssigned(object sender, EventArgs e)
        {
            if (!config.VanillaFlowersOnly)
            {
                BuildFlowerData(useWhitelist: false);
            }
        }

        internal struct FlowerData
        {
            public int index;
            public Color color;
        }

        private void BuildFlowerData(bool useWhitelist)
        {
            var objData = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            var spring = new List<FlowerData>();
            var summer = new List<FlowerData>();
            var fall = new List<FlowerData>();
            var winter = new List<FlowerData>();

            var whitelist = new List<int>(new int[] { 591, 593, 595, 597, 376, 402, 418 });

            foreach (var crop in cropData)
            {
                string[] toks = crop.Value.Split('/');
                string[] seasons = toks[1].Split(' ');
                int product = int.Parse(toks[3]);

                int category = int.Parse(objData[product].Split('/')[3].Split(' ')[1]);

                if (category != StardewValley.Object.flowersCategory || useWhitelist && !whitelist.Contains(product))
                    continue;

                List<Color> cols = new List<Color>(new Color[] { Color.White });
                if (toks[8].StartsWith("true "))
                {
                    cols.Clear();
                    var colToks = toks[8].Split(' ');
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
                    FlowerData fd = new FlowerData()
                    {
                        index = product,
                        color = col,
                    };

                    foreach (var season in seasons)
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

            fd.Clear();
            fd.Add("spring", spring);
            fd.Add("summer", summer);
            fd.Add("fall", fall);
            fd.Add("winter", winter);
        }
    }
}
