using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MoreGiantCrops.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;

namespace MoreGiantCrops
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Dictionary<int, Texture2D> sprites = new();

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            Directory.CreateDirectory(Path.Combine(this.Helper.DirectoryPath, "assets"));

            Log.trace("Finding giant crop images");
            foreach (string path in Directory.EnumerateFiles(Path.Combine(this.Helper.DirectoryPath, "assets"), "*.png"))
            {
                string filename = Path.GetFileName(path);
                if (!int.TryParse(filename.Split('.')[0], out int id))
                {
                    Log.error("Bad PNG: " + filename);
                    continue;
                }
                Log.trace("Found PNG: " + filename);
                var tex = helper.Content.Load<Texture2D>($"assets/{filename}");
                Mod.sprites.Add(id, tex);
            }

            if (!Mod.sprites.Any())
            {
                Log.error("You must install an asset pack to use this mod.");
                return;
            }

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new GiantCropPatcher()
            );
        }
    }
}
