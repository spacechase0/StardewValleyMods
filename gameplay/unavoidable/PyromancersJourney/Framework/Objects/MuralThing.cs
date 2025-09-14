using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class MuralThing : BaseObject
    {
        private readonly Texture2D Tex = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/caldera-mural-assembled.png");
        private readonly VertexBuffer Buffer;

        public MuralThing(World world)
            : base(world)
        {
            VertexPositionColorTexture[] test = {
                new(new Vector3(-3, 0, 0), Color.White, new Vector2(0, 1)),
                new(new Vector3(-3, 5, 0), Color.White, new Vector2(0, 0)),
                new(new Vector3(4, 5, 0), Color.White, new Vector2(1, 0)),
                new(new Vector3(-3f, 0, 0), Color.White, new Vector2(0, 1)),
                new(new Vector3(4, 0, 0), Color.White, new Vector2(1, 1)),
                new(new Vector3(4, 5, 0), Color.White, new Vector2(1, 0))
            };
            this.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            this.Buffer.SetData(test);
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
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            this.Buffer?.Dispose();
        }
    }
}
