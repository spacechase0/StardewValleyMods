using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class LevelWarp : BaseObject
    {
        private Texture2D tex;
        private VertexBuffer buffer;
        private int triCount;

        public LevelWarp(World world)
            : base(world)
        {
            this.Position = new Vector3(world.map.Size.X / 2f, 0, world.map.Size.Y / 2f);
            this.tex = Game1.content.Load<Texture2D>("Maps\\Mines\\mine_dark");

            Vector2 tb = new Vector2(224f / this.tex.Width, 224f / this.tex.Height);
            float tx = 16f / this.tex.Width, ty = 16f / this.tex.Height;

            var vertices = new List<VertexPositionColorTexture>();
            vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0.01f, 0), Color.White, tb + new Vector2(0, 0)));
            vertices.Add(new VertexPositionColorTexture(new Vector3(1, 0.01f, 0), Color.White, tb + new Vector2(tx, 0)));
            vertices.Add(new VertexPositionColorTexture(new Vector3(1, 0.01f, 1), Color.White, tb + new Vector2(tx, ty)));
            ++this.triCount;

            vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0.01f, 0), Color.White, tb + new Vector2(0, 0)));
            vertices.Add(new VertexPositionColorTexture(new Vector3(1, 0.01f, 1), Color.White, tb + new Vector2(tx, ty)));
            vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0.01f, 1), Color.White, tb + new Vector2(0, ty)));
            ++this.triCount;

            this.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
            this.buffer.SetData(vertices.ToArray());
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            BaseObject.effect.TextureEnabled = true;
            BaseObject.effect.Texture = this.tex;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.triCount);
            }
        }
    }
}
