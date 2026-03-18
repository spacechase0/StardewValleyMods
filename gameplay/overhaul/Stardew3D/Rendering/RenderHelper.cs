using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Utilities;
using StardewValley;

namespace Stardew3D.Rendering;
public static class RenderHelper
{
    public static GenericModelEffect GenericEffect => Mod.State.GenericModelEffect;
    public static RasterizerState RasterizerState = RasterizerState.CullClockwise;
    public static DepthStencilState DepthState = new()
    {
        DepthBufferEnable = true,
        DepthBufferWriteEnable = true,
    };

    internal static VertexBuffer quadVbo;

    public static void GenerateQuad(ICollection<SimpleVertex> toAddTo, Vector3 pos, Vector2 vp00, Vector2 vp10, Vector2 vp01, Vector2 vp11, float tx, float ty, float twidth, float theight, Vector3 facingDir, Color? col_ = null, Vector3? upOverride = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        Color col = col_.HasValue ? col_.Value : Color.White;

        var up = Vector3.Zero;
        if (facingDir == Vector3.Up || facingDir == Vector3.Down)
        {
            up = facingDir == Vector3.Up ? Vector3.Forward : Vector3.Backward;
        }
        else
        {
            var right = Vector3.Cross(Vector3.Up, facingDir).Normalized();
            up = Vector3.Cross(facingDir, right).Normalized();
        }
        if (upOverride.HasValue)
            up = upOverride.Value;
        //up = -up; // Flips the texture vertically. Don't know why we need this.

        SimpleVertex v00 = new(new(vp00.X, vp00.Y, 0), new Vector2(tx + twidth, ty + theight));
        SimpleVertex v10 = new(new(vp10.X, vp10.Y, 0), new Vector2(tx, ty + theight));
        SimpleVertex v11 = new(new(vp11.X, vp11.Y, 0), new Vector2(tx, ty));
        SimpleVertex v01 = new(new(vp01.X, vp01.Y, 0), new Vector2(tx + twidth, ty));
        v00.Color = v10.Color = v11.Color = v01.Color = col;
        if (texCoordEffect.HasFlag(SpriteEffects.FlipHorizontally))
        {
            Util.Swap(ref v00.TexCoord, ref v10.TexCoord);
            Util.Swap(ref v01.TexCoord, ref v11.TexCoord);
        }
        if (texCoordEffect.HasFlag(SpriteEffects.FlipVertically))
        {
            Util.Swap(ref v00.TexCoord, ref v01.TexCoord);
            Util.Swap(ref v10.TexCoord, ref v11.TexCoord);
        }

        var transform = Matrix.CreateBillboard(pos, pos + facingDir, up, -facingDir);
        v00.Position = Vector3.Transform(v00.Position, transform);
        v10.Position = Vector3.Transform(v10.Position, transform);
        v11.Position = Vector3.Transform(v11.Position, transform);
        v01.Position = Vector3.Transform(v01.Position, transform);

        toAddTo.Add(v00);
        toAddTo.Add(v01);
        toAddTo.Add(v10);
        toAddTo.Add(v11);
        toAddTo.Add(v10);
        toAddTo.Add(v01);
    }

    public static void GenerateQuad(ICollection<SimpleVertex> toAddTo, Vector3 pos, Vector2 displaySize, float tx, float ty, float twidth, float theight, Vector3 facingDir, Color? col_ = null, Vector3? upOverride = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        GenerateQuad(toAddTo, pos, new(-displaySize.X / 2, -displaySize.Y / 2), new(displaySize.X / 2, -displaySize.Y / 2), new(-displaySize.X / 2, displaySize.Y / 2), new(displaySize.X / 2, displaySize.Y / 2), tx, ty, twidth, theight, facingDir, col_, upOverride, texCoordEffect: texCoordEffect);
    }

    public static void GenerateQuad(ICollection<SimpleVertex> toAddTo, Texture2D tex, Vector3 pos, Vector2 displaySize, Rectangle texCoords, Vector3 facingDir, Color? col = null, Vector3? upOverride = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        float tx = texCoords.X / (float)tex.Width;
        float ty = texCoords.Y / (float)tex.Height;
        float txi = texCoords.Width / (float)tex.Width;
        float tyi = texCoords.Height / (float)tex.Height;

        GenerateQuad(toAddTo, pos, displaySize, tx, ty, txi, tyi, facingDir, col, upOverride, texCoordEffect: texCoordEffect);
    }

    public static void DrawQuad(Texture2D tex, Vector3 pos, Vector2 displaySize, Rectangle texCoords, Vector3 facingDir, Color? col = null, Vector3? upOverride = null, Matrix? additionalTransform = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        Matrix additionalTransform_ = additionalTransform ?? Matrix.Identity;

        float tx = texCoords.X / (float)tex.Width;
        float ty = texCoords.Y / (float)tex.Height;
        float txi = texCoords.Width / (float)tex.Width;
        float tyi = texCoords.Height / (float)tex.Height;

        List<SimpleVertex> vertices = new(6);
        GenerateQuad( vertices, pos, displaySize, tx, ty, txi, tyi, facingDir, col, upOverride, texCoordEffect: texCoordEffect );
        quadVbo.SetData(vertices.ToArray());
        Game1.graphics.GraphicsDevice.SetVertexBuffer(quadVbo);

        GenericEffect.Texture = tex;
        GenericEffect.World = additionalTransform_;
        var oldDepth = Game1.graphics.GraphicsDevice.DepthStencilState;
        var oldRaster = Game1.graphics.GraphicsDevice.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = DepthState;
        Game1.graphics.GraphicsDevice.RasterizerState = RasterizerState;
        {
            GenericEffect.CurrentTechnique = GenericEffect.Techniques["SingleDrawing_Transparent_1"];
            foreach (var pass in GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
        {
            GenericEffect.CurrentTechnique = GenericEffect.Techniques["SingleDrawing_Transparent_2"];
            foreach (var pass in GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
        Game1.graphics.GraphicsDevice.DepthStencilState = oldDepth;
        Game1.graphics.GraphicsDevice.RasterizerState = oldRaster;
    }

    public static void DrawBillboard(ICamera camera, Texture2D tex, Vector3 pos, Vector2 displaySize, Rectangle texCoords, Color? col = null, Matrix? additionalTransform = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        DrawQuad(tex, pos, displaySize, texCoords, (Vector3.Transform(camera.Position, additionalTransform?.Inverted() ?? Matrix.Identity ) - pos).Normalized(), col: col, upOverride: Vector3.TransformNormal(camera.Up, additionalTransform?.Inverted() ?? Matrix.Identity), additionalTransform: additionalTransform, texCoordEffect: texCoordEffect);
    }

    public static void DrawBillboard(Vector3 cameraPos, Vector3 cameraUp, Texture2D tex, Vector3 pos, Vector2 displaySize, Rectangle texCoords, Color? col = null, Matrix? additionalTransform = null, SpriteEffects texCoordEffect = SpriteEffects.None)
    {
        DrawQuad(tex, pos, displaySize, texCoords, (Vector3.Transform(cameraPos, additionalTransform?.Inverted() ?? Matrix.Identity) - pos).Normalized(), col: col, upOverride: Vector3.TransformNormal(cameraUp, additionalTransform?.Inverted() ?? Matrix.Identity), additionalTransform: additionalTransform, texCoordEffect: texCoordEffect);
    }

    public static void DebugRenderGrid()
    {
        for (int ix = -10; ix < 10; ++ix)
        {
            for (int iy = -10; iy < 10; ++iy)
            {
                Color col = new(128, 0, 128);
                if (ix == 0) col.R += 64;
                else if (ix == -1) col.R -= 64;
                else if (ix < 0) col.R -= 128;
                else col.R += 127;
                if (iy == 0) col.B += 64;
                else if (iy == -1) col.B -= 64;
                else if (iy < 0) col.B -= 128;
                else col.B += 127;
                DrawQuad(Game1.staminaRect, new(ix + 0.5f, 0, iy + 0.5f), Vector2.One * (15f / 16), Game1.staminaRect.Bounds, Vector3.Up, col);
                col.G = 255;
                DrawQuad(Game1.staminaRect, new(ix + 0.5f, 10, iy + 0.5f), Vector2.One * (15f / 16), Game1.staminaRect.Bounds, Vector3.Down, col);
            }
        }
    }

    public static void DebugRender(ICamera camera)
    {
        float dist = 4f;
        float scale = 3;

        float rat = Game1.objectSpriteSheet.Width / (float)Game1.objectSpriteSheet.Height;
        // Static billboards
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(1, 0, 0) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(-1, 0, 0) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(0, 1, 0) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(0, -1, 0) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(0, 0, 1) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);
        DrawBillboard(camera, Game1.objectSpriteSheet, camera.Position + new Vector3(0, 0, -1) * dist * 1.35f, new Vector2(rat / 2, 1) * scale, Game1.objectSpriteSheet.Bounds);

        // Static quads
        DrawQuad(Game1.mouseCursors, Vector3.Right * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Left);
        DrawQuad(Game1.mouseCursors, Vector3.Left * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Right);
        DrawQuad(Game1.mouseCursors, Vector3.Up * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Down);
        DrawQuad(Game1.mouseCursors, Vector3.Down * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Up);
        DrawQuad(Game1.mouseCursors, Vector3.Backward * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Forward);
        DrawQuad(Game1.mouseCursors, Vector3.Forward * dist * 2, new(3), Game1.mouseCursors.Bounds, Vector3.Backward);

        // Billboard that follows camera
        DrawBillboard(camera, Game1.content.Load<Texture2D>("Portraits/Penny"), camera.Position + camera.Forward * 5, Vector2.One, new(0, 0, 64, 64));
    }
}
