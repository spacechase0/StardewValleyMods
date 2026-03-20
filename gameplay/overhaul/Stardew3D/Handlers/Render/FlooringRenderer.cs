using Microsoft.Xna.Framework;
using Stardew3D.DataModels;
using StardewValley.GameData.FloorsAndPaths;
using StardewValley.TerrainFeatures;

namespace Stardew3D.Handlers.Render;
public class FlooringRenderer : RendererWithPlaceholder<ModelData, Flooring>
{
    private PlaceholderData[] placeholders;
    public override PlaceholderData[] Placeholders => placeholders;

    public FlooringRenderer(Flooring obj)
        : base(obj)
    {
        var data = Object.GetData();
        var textureCorner = Object.GetTextureCorner();
        byte key = (byte)(Object.neighborMask & 0xFu);
        int num2 = Flooring.drawGuide[key];
        if (data.ConnectType == FloorPathConnectType.Random)
        {
            num2 = Flooring.drawGuideList[Object.whichView.Value];
        }

        PlaceholderData placeholder = new();
        placeholder.Texture = Object.GetTexture();
        placeholder.TextureRegion = new Rectangle(textureCorner.X + num2 * 16 % 256, num2 / 16 * 16 + textureCorner.Y, 16, 16);
        placeholder.Billboard = false;
        placeholder.OrientationIfNotBillboard = Matrix.CreateLookAt(Vector3.Zero, Vector3.Up, Vector3.Forward);
        placeholder.OrientationIfNotBillboard *= Matrix.CreateTranslation(0, 0.0025f, 0.5f);
        placeholders = [placeholder];
    }
}
