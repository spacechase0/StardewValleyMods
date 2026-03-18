using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Objects;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class TelevisionRenderData : RenderDataWithPlaceholder<ModelData, TV>
{
    private int nonInstanced = -1;
    private MeshPart screenPart;
    private MeshPart screenOverlayPart;

    private Holder<Texture2D> screen1 = new();
    private Holder<Texture2D> screen2 = new();

    public TelevisionRenderData(RenderContext ctx, TelevisionRenderer parent)
        : base( ctx, parent )
    {
    }

    public override void Update(RenderContext ctx)
    {
        if (instance == null || instance.WhichMatch >= Model?.Matches.Count)
        {
            base.Update(ctx);
            return;
        }

        var mesh = Model.Matches[instance.WhichMatch];
        if (screenPart == null)
        {
            var screenEntry = mesh.FirstOrDefault(kvp => kvp.Key.EndsWith("/SCREEN_REPLACE"));
            if (!string.IsNullOrEmpty(screenEntry.Key))
                screenPart = screenEntry.Value.SelectMany(m => m.Mesh).FirstOrDefault();
        }

        if (screenOverlayPart == null)
        {
            var overlayEntry = mesh.FirstOrDefault(kvp => kvp.Key.EndsWith("/SCREEN_REPLACE_OVERLAY"));
            if (!string.IsNullOrEmpty(overlayEntry.Key))
                screenOverlayPart = overlayEntry.Value.SelectMany(m => m.Mesh).FirstOrDefault();
        }

        if (screenPart != null && screenOverlayPart != null)
        {
            (TemporaryAnimatedSprite Sprite, MeshPart Mesh, Holder<Texture2D> TmpScreen)[] parts =
            [
                new( Parent.Object.screen, screenPart, screen1 ),
                new( Parent.Object.screenOverlay, screenOverlayPart, screen2 ),
            ];
            foreach (var part in parts)
            {
                var effect = part.Mesh.Effect as GenericModelEffect;
                effect.Color = part.Sprite == null ? Color.Transparent : Color.White;
                if (part.Sprite != null)
                {
                    part.Sprite.update(ctx.Time);
                    if (part.TmpScreen.Value?.Bounds.Size != Parent.Object.screen.sourceRect.Size)
                    {
                        part.TmpScreen.Value?.Dispose();
                        part.TmpScreen.Value = new(Game1.graphics.GraphicsDevice, Parent.Object.screen.sourceRect.Width, Parent.Object.screen.sourceRect.Height);
                        part.TmpScreen.Value.Name = effect.Texture.Name;
                    }
                    Color[] col = new Color[part.TmpScreen.Value.Width * part.TmpScreen.Value.Height];
                    part.TmpScreen.Value.SetData(col);
                    col = new Color[part.Sprite.sourceRect.Width * part.Sprite.sourceRect.Height];
                    part.Sprite.Texture.GetData(0, part.Sprite.sourceRect, col, 0, col.Length);
                    Vector2 screenPos = part.Sprite.Position - Parent.Object.getScreenPosition();
                    screenPos /= Parent.Object.getScreenSizeModifier();
                    part.TmpScreen.Value.SetData(0, new( (int)screenPos.X, (int)screenPos.Y, part.Sprite.sourceRect.Width, part.Sprite.sourceRect.Height), col, 0, col.Length);
                    effect.Texture = part.TmpScreen.Value;
                }
            }
        }

        base.Update(ctx);
    }
}
