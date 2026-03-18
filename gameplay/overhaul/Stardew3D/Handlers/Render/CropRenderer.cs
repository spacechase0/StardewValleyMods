using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class CropRenderer : RendererWithPlaceholder<ModelData, Crop>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public CropRenderer(Crop obj)
        : base(obj)
    {
        List<PlaceholderData> placeholders = new();
        PlaceholderData placeholder = new();
        placeholder.Texture = Object.DrawnCropTexture;
        placeholder.TextureRegion = Object.sourceRect;
        placeholders.Add(placeholder);
        if (Object.tintColor.Value != Color.White)
        {
            placeholder.TextureRegion = Object.coloredSourceRect;
            placeholder.Color = Object.tintColor.Value;
            placeholders.Add(placeholder);
        }
        this.placeholders = placeholders.ToArray();
    }
}
