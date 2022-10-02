using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using MoreGiantCrops.Patches;

using Spacechase.Shared.Patching;

using SpaceShared;

using StardewModdingAPI;

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
                Mod.Sprites.Add(id, new (() => helper.ModContent.Load<Texture2D>($"assets/{filename}")));
            }

            Log.Trace($"{Sprites.Keys.Count} loaded, {string.Join(", ", Sprites.Keys.Select(k => k.ToString()))}");

            if (!Mod.Sprites.Any())
            {
                Log.Error("You must install an asset pack to use this mod.");
                return;
            }

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new GiantCropPatcher(),
                new FixLocationsPatcher()
            );
        }

        public override object GetApi() => new MoreGiantCropsAPI();
    }
}
