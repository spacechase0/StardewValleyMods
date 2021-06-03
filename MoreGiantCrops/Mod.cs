using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MoreGiantCrops
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Dictionary<int, Texture2D> sprites = new Dictionary<int, Texture2D>();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            SpaceShared.Log.Monitor = Monitor;

            Log.trace("Finding giant crop images");
            foreach ( var path in Directory.EnumerateFiles(Path.Combine(Helper.DirectoryPath, "assets"), "*.png") )
            {
                string filename = Path.GetFileName(path);
                if (!int.TryParse(filename.Split('.')[0], out int id))
                {
                    Log.error("Bad PNG: " + filename);
                    continue;
                }
                Log.trace("Found PNG: " + filename);
                var tex = helper.Content.Load<Texture2D>($"assets/{filename}");
                sprites.Add(id, tex);
            }

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.newDay)),
                transpiler: new HarmonyMethod(typeof(CropPatches), nameof(CropPatches.NewDay_Transpiler))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.draw)),
                prefix: new HarmonyMethod(typeof(GiantCropPatches), nameof(GiantCropPatches.Draw_Prefix))
            );
        }
    }
}
