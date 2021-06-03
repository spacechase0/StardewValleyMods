using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace FlowerRain
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;
        public static Config config;
        private Texture2D invisibleRain;

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("TileSheets\\rain");
        }
        
        public T Load<T>(IAssetInfo asset)
        {
            return (T) (object) invisibleRain;
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
            
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.Patch(typeof(Game1).GetMethod("updateWeather", BindingFlags.Public | BindingFlags.Static), transpiler: new HarmonyMethod(this.GetType().GetMethod("UpdateWeatherTranspiler", BindingFlags.Public | BindingFlags.Static)));
            harmony.Patch(typeof(Game1).GetMethod("drawWeather", BindingFlags.Public | BindingFlags.Instance), postfix: new HarmonyMethod(this.GetType().GetMethod("DrawWeatherPostfix", BindingFlags.Public | BindingFlags.Static)));
        }

        private void gameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if ( gmcm != null )
            {
                gmcm.RegisterModConfig(ModManifest, () => config = new Config(), () => Helper.WriteConfig(config));
                gmcm.RegisterSimpleOption(ModManifest, "Use Vanilla Flowers Only", "Only use vanilla flowers in the flower rain",
                                          () => config.VanillaFlowersOnly,
                                          (b) =>
                                          {
                                              config.VanillaFlowersOnly = b;
                                              if (config.VanillaFlowersOnly)
                                                  BuildFlowerData(useWhitelist: true);
                                          });
            }

            var ja = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if ( ja != null )
            {
                ja.IdsAssigned += jaIdsAssigned;
            }
        }

        private void jaIdsAssigned(object sender, EventArgs e)
        {
            if ( !config.VanillaFlowersOnly )
            {
                BuildFlowerData(useWhitelist: false);
            }
        }

        private struct FlowerData
        {
            public int index;
            public Color color;
        }

        private static Dictionary<string, List<FlowerData>> fd = new Dictionary<string, List<FlowerData>>();

        private void BuildFlowerData(bool useWhitelist)
        {
            var objData = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            var spring = new List<FlowerData>();
            var summer = new List<FlowerData>();
            var fall = new List<FlowerData>();
            var winter = new List<FlowerData>();

            var whitelist = new List<int>(new int[] { 591, 593, 595, 597, 376, 402, 418 });

            foreach ( var crop in cropData )
            {
                string[] toks = crop.Value.Split('/');
                string[] seasons = toks[1].Split(' ');
                int product = int.Parse(toks[3]);

                int category = int.Parse(objData[product].Split('/')[3].Split(' ')[1]);

                if (category != StardewValley.Object.flowersCategory || useWhitelist && !whitelist.Contains(product))
                    continue;

                List<Color> cols = new List<Color>(new Color[] { Color.White });
                if ( toks[8].StartsWith( "true " ) )
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

                foreach ( var col in cols )
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
                            case "fall":   fall.Add(fd); break;
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

        // Slow down rain
        public static IEnumerable<CodeInstruction> UpdateWeatherTranspiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == -16)
                {
                    insn.operand = (sbyte) -8;
                }
                else if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == 32)
                {
                    insn.operand = (sbyte) 16;
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        // Draw flowers
        public static void DrawWeatherPostfix(Game1 __instance, GameTime time, RenderTarget2D target_screen)
        {
            if (!Game1.isRaining || !Game1.currentLocation.IsOutdoors || (Game1.currentLocation.Name.Equals("Desert") || Game1.currentLocation is Summit))
                return;
            
            var currFlowers = fd[Game1.currentSeason];
            if (currFlowers.Count == 0)
                return;
            Random rand = new Random(0);

            if (__instance.takingMapScreenshot)
            {
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    Vector2 position = new Vector2((float)Game1.random.Next(Game1.viewport.Width - 64), (float)Game1.random.Next(Game1.viewport.Height - 64));
                    var rd = Game1.rainDrops[index];
                    var fd = currFlowers[index % currFlowers.Count];
                    float r = (float)(rand.NextDouble() * 3.14);
                    float s = 2f;
                    s -= rd.frame * 0.5f;
                    if (s <= 0)
                        continue;
                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, getRect(fd.index + 1), fd.color, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                }
            }
            else
            {
                if (Game1.eventUp && !Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64))))
                    return;
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    var rd = Game1.rainDrops[index];
                    var fd = currFlowers[index % currFlowers.Count];
                    float r = (float)(rand.NextDouble() * 3.14);
                    float s = 2f;
                    s -= rd.frame * 0.5f;
                    if (s <= 0)
                        continue;
                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, rd.position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, rd.position, getRect(fd.index + 1), fd.color, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                }
            }
        }

        private static Rectangle getRect(int index)
        {
            return new Rectangle(index % 24 * 16, index / 24 * 16, 16, 16);
        }
    }
}
