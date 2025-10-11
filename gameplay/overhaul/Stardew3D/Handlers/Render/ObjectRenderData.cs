using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;
using StardewValley.Network.NetEvents;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.Handlers.Render;

public class ObjectRenderData : RenderData<ObjectRenderer>
{
    private int nonInstanced = -1;

    private ICamera lastCamera;

    public ObjectRenderData(RenderContext ctx, ObjectRenderer parent)
        : base( ctx, parent,
            parent.Object.Location == null ? 0 : parent.Object.TileLocation.ToPoint().X + parent.Object.TileLocation.ToPoint().Y * parent.Object.Location.Map.Layers[0].LayerWidth)
    {
        if (Model.Matches.Count == 0)
        {
            if (!Batch.HasGenericData(Parent.Object.QualifiedItemId))
            {
                ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(Parent.Object.QualifiedItemId);
                List<SimpleVertex> vertices = new();
                RenderHelper.GenerateQuad(vertices, draw.GetTexture(), new Vector3(0, Parent.Object.bigCraftable.Value ? 1 : 0.5f, 0), new Vector2(1, Parent.Object.bigCraftable.Value ? 2 : 1), draw.GetSourceRect(Parent.Object.showNextIndex.Value ? 1 : 0), Vector3.Forward);
                RenderBatcher.GenericRenderData data = new()
                {
                    Vertices = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), vertices.Count, BufferUsage.WriteOnly),
                    Indices = new(Game1.graphics.GraphicsDevice, IndexElementSize.SixteenBits, vertices.Count, BufferUsage.WriteOnly),
                    Effect = Mod.State.GenericModelEffect.Clone(),
                    Blend = BlendState.AlphaBlend,
                };
                data.Vertices.SetData(vertices.ToArray());
                data.Indices.SetData(Enumerable.Range(0, vertices.Count).Select(i => (short)i).ToArray());
                (data.Effect as GenericModelEffect).Texture = draw.GetTexture();
                Batch.AddGenericData(Parent.Object.QualifiedItemId, [data]);
            }

            int instance = Batch.AddInstanced(Parent.Object.QualifiedItemId, Matrix.Identity, Color.White);
            Instances.Add(instance);
        }

        if (Parent.Object.Location != null)
        {
            nonInstanced = Batch.AddNonInstanced((env, color, world, view, proj) =>
            {
                if (Parent.Object.readyForHarvest.Value)
                {
                    var pos = new Vector3(0, 2.5f, 0);
                    var held = Parent.Object.heldObject.Value;
                    var draw = ItemRegistry.GetDataOrErrorItem(held.QualifiedItemId);
                    RenderHelper.DrawBillboard(lastCamera, Game1.mouseCursors, pos, new Vector2(1, 1), new Rectangle(141, 465, 20, 24), col: color, additionalTransform: world);
                    RenderHelper.DrawBillboard(lastCamera, draw.GetTexture(), pos + new Vector3(0, 0.1f, 0), new Vector2(0.9f, 0.9f), draw.GetSourceRect(0), col: color, additionalTransform: world);
                }
            }, Matrix.Identity, hasTransparency: true);
        }
    }

    public override void Update(RenderContext ctx)
    {
        lastCamera = ctx.WorldCamera;

        base.Update(ctx);
        if (Model.Matches.Count == 0)
        {
            Batch.UpdateInstanced(Instances[0], Matrix.CreateConstrainedBillboard(Vector3.Zero, lastCamera.Position - ctx.WorldTransform.Translation, Vector3.Up, lastCamera.Forward, Vector3.Forward) * ctx.WorldTransform, Color.White);
        }

        if (nonInstanced != -1)
        {
            Batch.UpdateNonInstanced(nonInstanced, ctx.WorldTransform);
        }
    }
}
