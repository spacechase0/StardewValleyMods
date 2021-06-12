using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Framework.Projectiles;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal class GolemEnemy : Enemy
    {
        public static Texture2D Tex = Mod.Instance.Helper.Content.Load<Texture2D>("assets/golem.png");

        public override RectangleF BoundingBox { get; } = new(-0.5f, -0.5f, 1, 1);

        private enum AnimState
        {
            Glow,
            Shoot,
            Immune,
            Summon,
        }
        private AnimState State = AnimState.Glow;
        private int Frame;
        private float FrameAccum;

        private VertexBuffer Buffer;

        public GolemEnemy(World world)
            : base(world)
        {
            this.Health = 50;

            var vertices = new List<VertexPositionColorTexture>();
            for (int f = 0; f < 12 * 9; ++f)
            {
                int fx = f % 12;
                int fy = f / 12;

                float xa = (80f / GolemEnemy.Tex.Width) * fx;
                float xb = (80f / GolemEnemy.Tex.Width) * (fx + 1);
                float ya = (80f / GolemEnemy.Tex.Height) * (fy + 1);
                float yb = (80f / GolemEnemy.Tex.Height) * fy;
                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 0, 0), Color.White, new Vector2(xa, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 0, 0), Color.White, new Vector2(xb, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 3, 0), Color.White, new Vector2(xb, yb)));

                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 0, 0), Color.White, new Vector2(xa, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 3, 0), Color.White, new Vector2(xa, yb)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 3, 0), Color.White, new Vector2(xb, yb)));
            }

            this.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
            this.Buffer.SetData(vertices.ToArray());
        }

        public override void Hurt(int amt)
        {
            if (this.State == AnimState.Immune && this.Frame > 3 && this.Frame < 11)
            {
                Game1.playSound("crit");
                return;
            }

            base.Hurt(amt);
            if (this.Dead)
            {
                Game1.playSound("explosion");
            }
            else
            {
                Game1.playSound("stoneCrack");
            }
        }

        public override void DoMovement()
        {
        }

        public override void Update()
        {
            base.Update();

            this.FrameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.FrameAccum >= 0.2f)
            {
                this.FrameAccum = 0;

                switch (this.State)
                {
                    case AnimState.Glow:
                        if (++this.Frame > 7)
                        {
                            this.GoToNextState();
                        }
                        break;
                    case AnimState.Shoot:
                        if (++this.Frame == 8)
                        {
                            var player = this.World.Player;
                            var speed = player.Position - this.Position;
                            speed.Y = 0;
                            speed.Normalize();
                            speed /= 10;

                            this.World.Projectiles.Add(new GolemArm(this.World)
                            {
                                Position = this.Position + new Vector3(0, 0.5f, 0),
                                Speed = new Vector2(speed.X, speed.Z)
                            });
                        }
                        else if (this.Frame > 16)
                        {
                            this.GoToNextState();
                        }
                        break;
                    case AnimState.Immune:
                        if (this.Frame < 7)
                            ++this.Frame;
                        if (this.World.Objects.OfType<Enemy>().Count() <= 1)
                        {
                            if (++this.Frame > 14)
                            {
                                this.GoToNextState();
                            }
                        }
                        break;
                    case AnimState.Summon:
                        if (++this.Frame > 6)
                        {
                            this.GoToNextState();
                        }
                        break;
                }
            }
        }

        private void GoToNextState()
        {
            this.Frame = 0;
            if (this.State == AnimState.Summon)
            {
                int amt = 2;
                for (int i = 0; i < amt; ++i)
                {
                    for (int t = 0; t < 10; ++t)
                    {
                        Vector2 pos = new Vector2(1 + Game1.random.Next((int)this.World.Map.Size.X - 2), 1 + Game1.random.Next((int)this.World.Map.Size.Y - 2));
                        if (this.World.Map.IsAirSolid(pos.X, pos.Y))
                        {
                            continue;
                        }

                        this.World.QueueObject(new BatEnemy(this.World) { Position = new Vector3(pos.X + 0.5f, 0.5f, pos.Y + 0.5f) });
                        break;
                    }
                }
                Game1.playSound("debuffHit");
                this.State = AnimState.Immune;
            }
            else if (this.State != AnimState.Glow)
            {
                this.State = AnimState.Glow;
            }
            else
            {
                switch (Game1.random.Next(4))
                {
                    case 0:
                    case 1:
                        this.State = AnimState.Glow;
                        break;
                    case 2:
                        this.State = AnimState.Shoot;
                        break;
                    case 3:
                        this.State = AnimState.Summon;
                        break;
                }
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int fx = this.Frame;
            int fy = 0;
            switch (this.State)
            {
                case AnimState.Glow:
                    fy = 1;
                    break;
                case AnimState.Shoot:
                    fy = 2;
                    if (fx > 8)
                        fx = 8 - (fx - 8);
                    break;
                case AnimState.Immune:
                    fy = 3;
                    if (fx > 7)
                        fx = 7 - (fx - 7);
                    break;
                case AnimState.Summon:
                    fy = 5;
                    if (fx > 6)
                        fx = 6 - (fx - 6);
                    break;
            }
            int frame = fy * 12 + fx;

            var oldStencil = device.DepthStencilState;
            var newStencil = new DepthStencilState();
            newStencil.DepthBufferWriteEnable = false;
            device.DepthStencilState = newStencil;

            BaseObject.Effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.Pos, Vector3.Up, null, null);
            BaseObject.Effect.TextureEnabled = true;
            BaseObject.Effect.Texture = GolemEnemy.Tex;
            for (int e = 0; e < BaseObject.Effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.Effect.CurrentTechnique.Passes[e];
                pass.Apply();

                device.SetVertexBuffer(this.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
