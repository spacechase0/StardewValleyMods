using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Data;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class BuildingRenderer : RendererWithPlaceholder<ModelData, Building>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public BuildingRenderer(Building obj)
        : base(obj)
    {
        List<PlaceholderData> placeholders = new();
        PlaceholderData placeholder = new();
        placeholder.Texture = Object.texture.Value;
        placeholder.TextureRegion = Object.getSourceRect();
        //placeholder.OffsetOverride = placeholder.DefaultOffset + new Vector3(0, placeholder.TextureRegion.Height / 16 / 2, 0);
        placeholders.Add(placeholder);
        BuildingData data = obj.GetData();
        if (data?.DrawLayers != null)
        {
            foreach (var layer in data.DrawLayers)
            {
                if (layer.DrawInBackground || layer.OnlyDrawIfChestHasContents != null)
                    continue;

                PlaceholderData layerPlaceholder = new();
                layerPlaceholder.Texture = layer.Texture != null ? Game1.content.Load<Texture2D>(layer.Texture) : Object.texture.Value;
                layerPlaceholder.TextureRegion = Object.ApplySourceRectOffsets(layer.GetSourceRect(0));
                layerPlaceholder.OffsetOverride = placeholder.DefaultOffset + new Vector3(0, layerPlaceholder.TextureRegion.Height / 16 / 2, 0);
                layerPlaceholder.OffsetOverride -= new Vector3(layer.DrawPosition.X / 16 / 2, layer.DrawPosition.Y / 16 / 2, 0); // TODO: This is wrong
                placeholders.Add(layerPlaceholder);
            }
        }
        this.placeholders = placeholders.ToArray();
    }
}
