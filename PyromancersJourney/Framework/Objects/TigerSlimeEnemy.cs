using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class TigerSlimeEnemy : Enemy
    {
        public static Texture2D Tex = Game1.content.Load<Texture2D>("Characters\\Monsters\\Tiger Slime");

        private int Frame = Game1.random.Next(4);
        private float FrameAccum;
        public int EyeType = Game1.random.Next(4);

        private static VertexBuffer MainBuffer;
        private static VertexBuffer EyesBuffer;

        public TigerSlimeEnemy(World world)
            : base(world)
        {
            this.Health = this.EyeType + 1;

            if (TigerSlimeEnemy.MainBuffer == null)
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for (int f = 0; f < 4; ++f)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        float xa = (16f / TigerSlimeEnemy.Tex.Width) * i;
                        float xb = (16f / TigerSlimeEnemy.Tex.Width) * (i + 1);
                        float ya = (24f / TigerSlimeEnemy.Tex.Height) * (f + 1);
                        float yb = (24f / TigerSlimeEnemy.Tex.Height) * f;
                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                    }
                }

                TigerSlimeEnemy.MainBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
                TigerSlimeEnemy.MainBuffer.SetData(vertices.ToArray());

                vertices.Clear();
                for (int i = 0; i < 4; ++i)
                {
                    int x = 32 + i % 2 * 16;
                    int y = 120 + i / 2 * 24;
                    float xa = (x / (float)TigerSlimeEnemy.Tex.Width);
                    float xb = ((x + 16) / (float)TigerSlimeEnemy.Tex.Width);
                    float ya = ((y + 24) / (float)TigerSlimeEnemy.Tex.Height);
                    float yb = (y / (float)TigerSlimeEnemy.Tex.Height);

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                }

                TigerSlimeEnemy.EyesBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
                TigerSlimeEnemy.EyesBuffer.SetData(vertices.ToArray());
            }
        }

        public override void Hurt(int amt)
        {
            base.Hurt(amt);
            Game1.playSound(this.Dead ? "slimedead" : "slimeHit");
        }

        public override void DoMovement()
        {
            var player = this.World.Player;
            var diff = player.Position - this.Position;
            diff.Y = 0;
            diff.Normalize();

            this.Position += diff / 50;
        }

        public override void Update()
        {
            base.Update();

            this.FrameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.FrameAccum >= 0.2f)
            {
                this.FrameAccum = 0;
                if (++this.Frame >= 4)
                    this.Frame = 0;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int facing = 0;
            int frame = this.Frame;
            frame += facing * 4;


            var oldStencil = device.DepthStencilState;
            device.DepthStencilState = new DepthStencilState
            {
                DepthBufferWriteEnable = false
            };

            var camForward = (cam.Pos - cam.Target);
            camForward.Normalize();
            BaseObject.Effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.Pos, cam.Up, null, null);
            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = TigerSlimeEnemy.Tex;
            foreach (var pass in BaseObject.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.SetVertexBuffer(TigerSlimeEnemy.MainBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            if (facing != 3)
            {
                var eyePos = new Vector3(0, 0.1f, 0);
                //if ( facing == 1 ) eyePos.X = 4 / 16f;
                //if ( facing == 2 ) eyePos.X = -4 / 16f;
                switch (this.Frame)
                {
                    case 0: break;
                    case 1: eyePos.Y += 0.05f; break;
                    case 2: eyePos.Y += 0.1f; break;
                    case 3: eyePos.Y += 0.05f; break;
                }
                eyePos.Y *= 0.75f;

                BaseObject.Effect.World = Matrix.CreateConstrainedBillboard(this.Position + eyePos, cam.Pos, cam.Up, null, null);
                foreach (EffectPass pass in BaseObject.Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.SetVertexBuffer(TigerSlimeEnemy.EyesBuffer);
                    device.DrawPrimitives(PrimitiveType.TriangleList, this.EyeType * 6, 2);
                }
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
