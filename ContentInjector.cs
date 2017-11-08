using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets
{
    public class ContentInjector : IAssetEditor
    {
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data\\ObjectInformation"))
                return true;
            if (asset.AssetNameEquals("Data\\Crops"))
                return true;
            if (asset.AssetNameEquals("Maps\\springobjects"))
                return true;
            if (asset.AssetNameEquals("TileSheets\\crops"))
                return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data\\ObjectInformation"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var obj in Mod.instance.objects)
                {
                    Log.trace($"Injecting to objects: {obj.GetObjectId()}: {obj.GetObjectInformation()}");
                    data.Add(obj.GetObjectId(), obj.GetObjectInformation());
                }
            }
            else if (asset.AssetNameEquals("Data\\Crops"))
            {
                var data = asset.AsDictionary<int, string>().Data;
                foreach (var crop in Mod.instance.crops)
                {
                    Log.trace($"Injecting to crops: {crop.GetSeedId()}: {crop.GetCropInformation()}");
                    data.Add(crop.GetSeedId(), crop.GetCropInformation());
                }
            }
            else if (asset.AssetNameEquals("Maps\\springobjects"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);

                foreach (var obj in Mod.instance.objects)
                {
                    try {
                    Log.trace($"Injecting {obj.Name} sprites");
                    asset.AsImage().PatchImage(Mod.instance.Helper.Content.Load<Texture2D>($"{obj.directory}/{obj.imageName}.png"), null, objectRect(obj.GetObjectId()));
                    if (obj.IsColored)
                        asset.AsImage().PatchImage(Mod.instance.Helper.Content.Load<Texture2D>($"{obj.directory}/color.png"), null, objectRect(obj.GetObjectId() + 1));
                    }
                    catch ( Exception e ){Log.error("EXCEPTION:" + e);}
                }
            }
            else if (asset.AssetNameEquals("TileSheets\\crops"))
            {
                var oldTex = asset.AsImage().Data;
                Texture2D newTex = new Texture2D(Game1.graphics.GraphicsDevice, oldTex.Width, Math.Max(oldTex.Height, 4096));
                asset.ReplaceWith(newTex);
                asset.AsImage().PatchImage(oldTex);

                foreach (var crop in Mod.instance.crops)
                {
                    Log.trace($"Injecting {crop.Name} crop images");
                    asset.AsImage().PatchImage(Mod.instance.Helper.Content.Load<Texture2D>($"{crop.directory}/crop.png"), null, cropRect(crop.GetCropSpriteIndex()));
                }
            }
        }
        private Rectangle objectRect(int index)
        {
            return new Rectangle(index % 24 * 16, index / 24 * 16, 16, 16);
        }
        private Rectangle cropRect(int index)
        {
            return new Rectangle(index % 2 * 128, index / 2 * 32, 128, 32);
        }
    }
}
