using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class MuralThing : BaseObject
    {
        private Texture2D tex = Mod.instance.Helper.Content.Load<Texture2D>("assets/caldera-mural-assembled.png");
        private VertexBuffer buffer;

        public MuralThing(World world)
            : base(world)
        {
            VertexPositionColorTexture[] test = new VertexPositionColorTexture[6]
            {
                new VertexPositionColorTexture( new Vector3( -3, 0, 0 ), Color.White, new Vector2( 0, 1 ) ),
                new VertexPositionColorTexture( new Vector3( -3, 5, 0 ), Color.White, new Vector2( 0, 0 ) ),
                new VertexPositionColorTexture( new Vector3( 4, 5, 0 ), Color.White, new Vector2( 1, 0 ) ),
                new VertexPositionColorTexture( new Vector3( -3f, 0, 0 ), Color.White, new Vector2( 0, 1 ) ),
                new VertexPositionColorTexture( new Vector3( 4, 0, 0 ), Color.White, new Vector2( 1, 1 ) ),
                new VertexPositionColorTexture( new Vector3( 4, 5, 0 ), Color.White, new Vector2( 1, 0 ) ),
            };
            this.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            this.buffer.SetData(test);
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            effect.TextureEnabled = true;
            effect.Texture = this.tex;
            for (int e = 0; e < effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(this.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
    }
}
