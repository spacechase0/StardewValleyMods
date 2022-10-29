using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using MoreGiantCrops.Patches;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

using StardewValley;

namespace MoreGiantCrops
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Dictionary<int, Lazy<Texture2D>> Sprites = new();

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnLaunched;

            var dir = Directory.CreateDirectory(Path.Combine(this.Helper.DirectoryPath, "assets"));

            Log.Trace("Finding giant crop images");
            foreach (string path in Directory.EnumerateFiles(dir.FullName, "*.png"))
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                if (!int.TryParse(filename, out int id))
                {
                    Log.Error("Bad PNG: " + filename);
                    continue;
                }
                Mod.Sprites.Add(id, new(() => helper.ModContent.Load<Texture2D>($"assets/{filename}")));
            }

            Log.Trace($"{Sprites.Keys.Count} loaded from assets, {string.Join(", ", Sprites.Keys.Select(k => k.ToString()))}");

            helper.ConsoleCommands.Add("force_load_giant_crops", "Forces MoreGiantCrops to load all textures", this.ForceLoad);

            var harmony = HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new GiantCropPatcher()
            );

            if (new Version(1, 6) > new Version(Game1.version))
            {
                this.Monitor.Log("Applying patch to restore giant crops from save");
                var patch = new FixLocationsPatcher();
                patch.Apply(harmony, this.Monitor);
            }
        }

        private void OnLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            foreach (var pack in this.Helper.ContentPacks.GetOwned())
            {
                Log.Info($"Loading pack {pack.Manifest}");
                try
                {
                    List<int> loaded = new();
                    var data = pack.ReadJsonFile<List<CropModel>>("giantcrops.json");
                    foreach (var datum in data)
                    {
                        if (datum.Index != -1 && datum.Path is not null)
                        {
                            if (!Mod.Sprites.TryAdd(datum.Index, new(() => pack.ModContent.Load<Texture2D>(datum.Path))))
                            {
                                Log.Warn($"Duplicate crop {datum.Index} not added");
                            }
                            else
                            {
                                loaded.Add(datum.Index);
                            }
                        }
                    }

                    Log.Trace($"Loaded {loaded.Count} giant crops: {string.Join(", ", loaded.Select((a) => a.ToString()))}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to load from {pack.Manifest}:\n\n{ex}");
                }
            }

            if (!Mod.Sprites.Any())
            {
                Log.Error("You must install an asset pack to use this mod.");
                return;
            }
        }

        public override object GetApi() => new MoreGiantCropsAPI();

        private void ForceLoad(string command, string[] param)
        {
            List<int> ToLoad;
            if (param.Length == 0)
            {
                ToLoad = Sprites.Keys.ToList();
            }
            else
            {
                ToLoad = new(param.Length);
                foreach (string val in param)
                {
                    if (int.TryParse(val, out int num))
                    {
                        ToLoad.Add(num);
                    }
                    else
                    {
                        this.Monitor.Log($"{val} is not a valid index.", LogLevel.Warn);
                    }
                }
            }

            foreach (int idx in ToLoad)
            {
                try
                {
                    if (Mod.Sprites.TryGetValue(idx, out var tex))
                    {
                        _ = tex.Value;
                    }
                    else
                    {
                        Log.Warn($"{idx} not found?");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"{idx} did not load correctly: {ex}");
                }
            }
        }
    }
}
