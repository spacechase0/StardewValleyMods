using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class BatEnemy : Enemy
    {
        public static Texture2D Tex = Game1.content.Load<Texture2D>("Characters\\Monsters\\Lava Bat");

        public override bool Floats => true;

        private int Frame = Game1.random.Next(4);
        private float FrameAccum;

        private static VertexBuffer MainBuffer;

        public BatEnemy(World world)
            : base(world)
        {
            this.Health = 2;

            if (BatEnemy.MainBuffer == null)
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for (int i = 0; i < 4; ++i)
                {
                    float xa = (16f / BatEnemy.Tex.Width) * i;
                    float xb = (16f / BatEnemy.Tex.Width) * (i + 1);
                    float ya = (24f / BatEnemy.Tex.Height) * (0 + 1);
                    float yb = (24f / BatEnemy.Tex.Height) * 0;
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                }

                BatEnemy.MainBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
                BatEnemy.MainBuffer.SetData(vertices.ToArray());
            }
        }

        public override void Hurt(int amt)
        {
            base.Hurt(amt);
            Game1.playSound(this.Dead ? "batScreech" : "hitEnemy");
        }

        public override void DoMovement()
        {
            var player = this.World.Player;
            var diff = player.Position - this.Position;
            diff.Y = 0;
            diff.Normalize();

            this.Position += diff / 50 * 1.5f;
        }

        public override void Update()
        {
            base.Update();

            this.FrameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.FrameAccum >= 0.15f)
            {
                this.FrameAccum = 0;
                if (++this.Frame >= 4)
                    this.Frame = 0;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int frame = this.Frame;

            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState
            {
                DepthBufferWriteEnable = false
            };
            device.DepthStencilState = newStencil;

            var camForward = (cam.Pos - cam.Target);
            camForward.Normalize();
            BaseObject.Effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.Pos, cam.Up, null, null);
            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = BatEnemy.Tex;
            foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.SetVertexBuffer(BatEnemy.MainBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
