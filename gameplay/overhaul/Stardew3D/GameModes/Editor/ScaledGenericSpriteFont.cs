using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace Stardew3D.GameModes.Editor;
public class ScaledGenericSpriteFont : GenericSpriteFont
{
    // TODO: Better implementation of everything
    public float Scale { get; }

    private int origLineSpacing;

    public ScaledGenericSpriteFont(float scale, SpriteFont font, SpriteFont bold = null, SpriteFont italic = null)
        : base(font, bold, italic)
    {
        Scale = scale;

        origLineSpacing = Font.LineSpacing;
        Font.LineSpacing = (int)(Font.LineSpacing * Scale);
    }

    protected override float MeasureCharacter(int codePoint)
    {
        return base.MeasureCharacter(codePoint) * Scale;
    }

    public override void DrawCharacter(SpriteBatch batch, int codePoint, string character, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        scale *= Scale;

        int oldLineSpacing = Font.LineSpacing;
        Font.LineSpacing = origLineSpacing;
        base.DrawCharacter(batch, codePoint, character, position, color, rotation, scale, effects, layerDepth);
        Font.LineSpacing = oldLineSpacing;
    }
}
