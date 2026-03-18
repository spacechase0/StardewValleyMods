using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley;
using StardewValley.TerrainFeatures;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;
public class HoeDirtRenderer : RendererWithPlaceholder<ModelData, HoeDirt>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public HoeDirtRenderer(HoeDirt obj)
        : base(obj)
    {
        obj.UpdateDrawSums();

        List<PlaceholderData> placeholders = new();
        PlaceholderData placeholder = new();
        placeholder.Texture = Object.texture ?? ((obj.Location?.GetSeason() != Season.Winter || (obj.Location?.SeedsIgnoreSeasonsHere() ?? false)) ? HoeDirt.lightTexture : HoeDirt.snowTexture );
        placeholder.TextureRegion = new Rectangle(Object.sourceRectPosition % 4 * 16, Object.sourceRectPosition / 4 * 16, 16, 16);
        placeholder.Billboard = false;
        placeholder.OrientationIfNotBillboard = Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward);
        placeholder.OrientationIfNotBillboard *= Matrix.CreateTranslation(Vector3.UnitY * 0.0025f);
        placeholders.Add(placeholder);
        if (Object.state.Value == HoeDirt.watered)
        {
            placeholder.TextureRegion = new Rectangle(Object.wateredRectPosition % 4 * 16 + (Object.paddyWaterCheck() ? 128 : 64), Object.wateredRectPosition / 4 * 16, 16, 16);
            placeholder.OrientationIfNotBillboard *= Matrix.CreateTranslation(Vector3.UnitY * 0.0025f);
            placeholders.Add(placeholder);
        }
        if (Object.HasFertilizer())
        {
            placeholder.Texture = Game1.mouseCursors;
            placeholder.TextureRegion = Object.GetFertilizerSourceRect();
            placeholder.OrientationIfNotBillboard *= Matrix.CreateTranslation(Vector3.UnitY * 0.0025f);
            placeholders.Add(placeholder);
        }
        this.placeholders = placeholders.ToArray();
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new HoeDirtRenderData(ctx, this);
    }
}
