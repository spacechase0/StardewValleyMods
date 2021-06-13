using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class LevelWarp : BaseObject
    {
        private readonly Texture2D Tex;
        private readonly VertexBuffer Buffer;
        private readonly int TriCount;

        public LevelWarp(World world)
            : base(world)
        {
            this.Position = new Vector3(world.Map.Size.X / 2f, 0, world.Map.Size.Y / 2f);
            this.Tex = Game1.content.Load<Texture2D>("Maps\\Mines\\mine_dark");

            Vector2 tb = new Vector2(224f / this.Tex.Width, 224f / this.Tex.Height);
            float tx = 16f / this.Tex.Width, ty = 16f / this.Tex.Height;

            var vertices = new VertexPositionColorTexture[]
            {
                new(new Vector3(0, 0.01f, 0), Color.White, tb + new Vector2(0, 0)),
                new(new Vector3(1, 0.01f, 0), Color.White, tb + new Vector2(tx, 0)),
                new(new Vector3(1, 0.01f, 1), Color.White, tb + new Vector2(tx, ty)),
                new(new Vector3(0, 0.01f, 0), Color.White, tb + new Vector2(0, 0)),
                new(new Vector3(1, 0.01f, 1), Color.White, tb + new Vector2(tx, ty)),
                new(new Vector3(0, 0.01f, 1), Color.White, tb + new Vector2(0, ty))
            };

            this.TriCount += 2;

            this.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
            this.Buffer.SetData(vertices);
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = this.Tex;
            foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(this.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, this.TriCount);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Buffer?.Dispose();
        }
    }
}
