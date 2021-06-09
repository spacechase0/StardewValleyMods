using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class TigerSlimeEnemy : Enemy
    {
        public static Texture2D tex = Game1.content.Load<Texture2D>("Characters\\Monsters\\Tiger Slime");

        private int frame = Game1.random.Next(4);
        private float frameAccum = 0;
        public int eyeType = Game1.random.Next(4);

        private static VertexBuffer mainBuffer;
        private static VertexBuffer eyesBuffer;

        public TigerSlimeEnemy(World world)
            : base(world)
        {
            this.Health = this.eyeType + 1;

            if (TigerSlimeEnemy.mainBuffer == null)
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for (int f = 0; f < 4; ++f)
                {
                    for (int i = 0; i < 4; ++i)
                    {
                        float xa = (16f / TigerSlimeEnemy.tex.Width) * i;
                        float xb = (16f / TigerSlimeEnemy.tex.Width) * (i + 1);
                        float ya = (24f / TigerSlimeEnemy.tex.Height) * (f + 1);
                        float yb = (24f / TigerSlimeEnemy.tex.Height) * f;
                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                        vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                    }
                }

                TigerSlimeEnemy.mainBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
                TigerSlimeEnemy.mainBuffer.SetData(vertices.ToArray());

                vertices.Clear();
                for (int i = 0; i < 4; ++i)
                {
                    int x = 32 + i % 2 * 16;
                    int y = 120 + i / 2 * 24;
                    float xa = (x / (float)TigerSlimeEnemy.tex.Width);
                    float xb = ((x + 16) / (float)TigerSlimeEnemy.tex.Width);
                    float ya = ((y + 24) / (float)TigerSlimeEnemy.tex.Height);
                    float yb = (y / (float)TigerSlimeEnemy.tex.Height);

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                }

                TigerSlimeEnemy.eyesBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
                TigerSlimeEnemy.eyesBuffer.SetData(vertices.ToArray());
            }
        }

        public override void Hurt(int amt)
        {
            base.Hurt(amt);
            if (this.Dead)
            {
                Game1.playSound("slimedead");
            }
            else
            {
                Game1.playSound("slimeHit");
            }
        }

        public override void DoMovement()
        {
            var player = this.World.player;
            var diff = player.Position - this.Position;
            diff.Y = 0;
            diff.Normalize();

            this.Position += diff / 50;
        }

        public override void Update()
        {
            base.Update();

            this.frameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.frameAccum >= 0.2f)
            {
                this.frameAccum = 0;
                if (++this.frame >= 4)
                    this.frame = 0;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int facing = 0;
            int frame = this.frame;
            frame += facing * 4;


            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState();
            newStencil.DepthBufferWriteEnable = false;
            device.DepthStencilState = newStencil;

            var camForward = (cam.pos - cam.target);
            camForward.Normalize();
            BaseObject.effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.pos, cam.up, null, null);
            BaseObject.effect.TextureEnabled = true;
            BaseObject.effect.Texture = TigerSlimeEnemy.tex;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();

                device.SetVertexBuffer(TigerSlimeEnemy.mainBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            if (facing != 3)
            {
                var eyePos = new Vector3(0, 0.1f, 0);
                //if ( facing == 1 ) eyePos.X = 4 / 16f;
                //if ( facing == 2 ) eyePos.X = -4 / 16f;
                switch (this.frame)
                {
                    case 0: break;
                    case 1: eyePos.Y += 0.05f; break;
                    case 2: eyePos.Y += 0.1f; break;
                    case 3: eyePos.Y += 0.05f; break;
                }
                eyePos.Y *= 0.75f;

                BaseObject.effect.World = Matrix.CreateConstrainedBillboard(this.Position + eyePos, cam.pos, cam.up, null, null);
                for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
                {
                    var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                    pass.Apply();

                    device.SetVertexBuffer(TigerSlimeEnemy.eyesBuffer);
                    device.DrawPrimitives(PrimitiveType.TriangleList, this.eyeType * 6, 2);
                }
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
