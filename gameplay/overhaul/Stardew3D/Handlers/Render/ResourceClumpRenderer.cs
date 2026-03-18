using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class ResourceClumpRenderer : RendererWithPlaceholder<ModelData, ResourceClump>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public ResourceClumpRenderer(ResourceClump obj)
        : base(obj)
    {
        obj.loadSprite();

        PlaceholderData placeholder = new();
        if (Object is GiantCrop gc)
        {
            if (gc.GetData() is { } data)
            {
                placeholder.Texture = Game1.content.Load<Texture2D>(data.Texture);
                placeholder.TextureRegion = new(data.TexturePosition, new(data.TileSize.X * 16, data.TileSize.Y * 16));
            }
            else
            {
                placeholder.Texture = ItemRegistry.RequireTypeDefinition("(O)").GetErrorTexture();
                placeholder.TextureRegion = ItemRegistry.RequireTypeDefinition("(O)").GetErrorSourceRect();
            }
        }
        else
        {
            placeholder.Texture = Object.texture;
            Rectangle rect = Game1.getSourceRectForStandardTileSheet(Object.texture, Object.parentSheetIndex.Value, 16, 16);
            rect.Width *= Object.width.Value;
            rect.Height *= Object.height.Value;
            placeholder.TextureRegion = rect;
        }
        placeholders = [placeholder];
    }
}
