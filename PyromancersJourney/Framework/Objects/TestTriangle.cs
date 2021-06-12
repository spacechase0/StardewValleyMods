using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class TestTriangle : BaseObject
    {
        private readonly VertexBuffer Buffer = new(Game1.game1.GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);

        public TestTriangle(World world)
            : base(world)
        {
            VertexPositionColor[] test = new VertexPositionColor[3]
            {
                new(new Vector3(0, 0, 0), Color.Red),
                new(new Vector3(1, 2, 0), Color.Green),
                new(new Vector3(2, 0, 0), Color.Blue),
            };
            this.Buffer.SetData(test);
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            BaseObject.Effect.TextureEnabled = false;
            for (int e = 0; e < BaseObject.Effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.Effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
            }
        }
    }
}
