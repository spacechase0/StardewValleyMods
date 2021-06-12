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
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Dictionary<int, Texture2D> Sprites = new();

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Directory.CreateDirectory(Path.Combine(this.Helper.DirectoryPath, "assets"));

            Log.Trace("Finding giant crop images");
            foreach (string path in Directory.EnumerateFiles(Path.Combine(this.Helper.DirectoryPath, "assets"), "*.png"))
            {
                string filename = Path.GetFileName(path);
                if (!int.TryParse(filename.Split('.')[0], out int id))
                {
                    Log.Error("Bad PNG: " + filename);
                    continue;
                }
                Log.Trace("Found PNG: " + filename);
                var tex = helper.Content.Load<Texture2D>($"assets/{filename}");
                Mod.Sprites.Add(id, tex);
            }

            if (!Mod.Sprites.Any())
            {
                Log.Error("You must install an asset pack to use this mod.");
                return;
            }

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new GiantCropPatcher()
            );
        }
    }
}
