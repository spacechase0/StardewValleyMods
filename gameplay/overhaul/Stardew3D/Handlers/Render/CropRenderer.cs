using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;

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
