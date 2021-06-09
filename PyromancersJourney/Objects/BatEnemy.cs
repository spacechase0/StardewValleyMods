using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class BatEnemy : Enemy
    {
        public static Texture2D tex = Game1.content.Load<Texture2D>("Characters\\Monsters\\Lava Bat");

        public override bool Floats => true;

        private int frame = Game1.random.Next(4);
        private float frameAccum = 0;

        private static VertexBuffer mainBuffer;

        public BatEnemy(World world)
            : base(world)
        {
            this.Health = 2;

            if (BatEnemy.mainBuffer == null)
            {
                float s = 0.75f;

                var vertices = new List<VertexPositionColorTexture>();
                for (int i = 0; i < 4; ++i)
                {
                    float xa = (16f / BatEnemy.tex.Width) * i;
                    float xb = (16f / BatEnemy.tex.Width) * (i + 1);
                    float ya = (24f / BatEnemy.tex.Height) * (0 + 1);
                    float yb = (24f / BatEnemy.tex.Height) * 0;
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, 0, 0), Color.White, new Vector2(xb, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(xa, ya)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, s, 0), Color.White, new Vector2(xa, yb)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(s, s, 0), Color.White, new Vector2(xb, yb)));
                }

                BatEnemy.mainBuffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
                BatEnemy.mainBuffer.SetData(vertices.ToArray());
            }
        }

        public override void Hurt(int amt)
        {
            base.Hurt(amt);
            if (this.Dead)
            {
                Game1.playSound("batScreech");
            }
            else
            {
                Game1.playSound("hitEnemy");
            }
        }

        public override void DoMovement()
        {
            var player = this.World.player;
            var diff = player.Position - this.Position;
            diff.Y = 0;
            diff.Normalize();

            this.Position += diff / 50 * 1.5f;
        }

        public override void Update()
        {
            base.Update();

            this.frameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.frameAccum >= 0.15f)
            {
                this.frameAccum = 0;
                if (++this.frame >= 4)
                    this.frame = 0;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int frame = this.frame;

            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState();
            newStencil.DepthBufferWriteEnable = false;
            device.DepthStencilState = newStencil;

            var camForward = (cam.pos - cam.target);
            camForward.Normalize();
            BaseObject.effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.pos, cam.up, null, null);
            BaseObject.effect.TextureEnabled = true;
            BaseObject.effect.Texture = BatEnemy.tex;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();

                device.SetVertexBuffer(BatEnemy.mainBuffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
