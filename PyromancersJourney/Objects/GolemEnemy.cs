using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Projectiles;
using StardewValley;

namespace PyromancersJourney.Objects
{
    public class GolemEnemy : Enemy
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load<Texture2D>("assets/golem.png");

        public override RectangleF BoundingBox { get; } = new RectangleF(-0.5f, -0.5f, 1, 1);

        private enum AnimState
        {
            Glow,
            Shoot,
            Immune,
            Summon,
        }
        private AnimState state = AnimState.Glow;
        private int frame = 0;
        private float frameAccum = 0;

        private VertexBuffer buffer;

        public GolemEnemy(World world)
            : base(world)
        {
            this.Health = 50;

            var vertices = new List<VertexPositionColorTexture>();
            for (int f = 0; f < 12 * 9; ++f)
            {
                int fx = f % 12;
                int fy = f / 12;

                float xa = (80f / GolemEnemy.tex.Width) * fx;
                float xb = (80f / GolemEnemy.tex.Width) * (fx + 1);
                float ya = (80f / GolemEnemy.tex.Height) * (fy + 1);
                float yb = (80f / GolemEnemy.tex.Height) * fy;
                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 0, 0), Color.White, new Vector2(xa, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 0, 0), Color.White, new Vector2(xb, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 3, 0), Color.White, new Vector2(xb, yb)));

                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 0, 0), Color.White, new Vector2(xa, ya)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(-1.5f, 3, 0), Color.White, new Vector2(xa, yb)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(1.5f, 3, 0), Color.White, new Vector2(xb, yb)));
            }

            this.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
            this.buffer.SetData(vertices.ToArray());
        }

        public override void Hurt(int amt)
        {
            if (this.state == AnimState.Immune && this.frame > 3 && this.frame < 11)
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

            this.frameAccum += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (this.frameAccum >= 0.2f)
            {
                this.frameAccum = 0;

                switch (this.state)
                {
                    case AnimState.Glow:
                        if (++this.frame > 7)
                        {
                            this.GoToNextState();
                        }
                        break;
                    case AnimState.Shoot:
                        if (++this.frame == 8)
                        {
                            var player = this.World.player;
                            var speed = player.Position - this.Position;
                            speed.Y = 0;
                            speed.Normalize();
                            speed /= 10;

                            this.World.projectiles.Add(new GolemArm(this.World)
                            {
                                Position = this.Position + new Vector3(0, 0.5f, 0),
                                Speed = new Vector2(speed.X, speed.Z)
                            });
                        }
                        else if (this.frame > 16)
                        {
                            this.GoToNextState();
                        }
                        break;
                    case AnimState.Immune:
                        if (this.frame < 7)
                            ++this.frame;
                        if (this.World.objects.OfType<Enemy>().Count() <= 1)
                        {
                            if (++this.frame > 14)
                            {
                                this.GoToNextState();
                            }
                        }
                        break;
                    case AnimState.Summon:
                        if (++this.frame > 6)
                        {
                            this.GoToNextState();
                        }
                        break;
                }
            }
        }

        private void GoToNextState()
        {
            this.frame = 0;
            if (this.state == AnimState.Summon)
            {
                int amt = 2;
                for (int i = 0; i < amt; ++i)
                {
                    for (int t = 0; t < 10; ++t)
                    {
                        Vector2 pos = new Vector2(1 + Game1.random.Next((int)this.World.map.Size.X - 2), 1 + Game1.random.Next((int)this.World.map.Size.Y - 2));
                        if (this.World.map.IsAirSolid(pos.X, pos.Y))
                        {
                            continue;
                        }

                        this.World.QueueObject(new BatEnemy(this.World) { Position = new Vector3(pos.X + 0.5f, 0.5f, pos.Y + 0.5f) });
                        break;
                    }
                }
                Game1.playSound("debuffHit");
                this.state = AnimState.Immune;
            }
            else if (this.state != AnimState.Glow)
            {
                this.state = AnimState.Glow;
            }
            else
            {
                switch (Game1.random.Next(4))
                {
                    case 0:
                    case 1:
                        this.state = AnimState.Glow;
                        break;
                    case 2:
                        this.state = AnimState.Shoot;
                        break;
                    case 3:
                        this.state = AnimState.Summon;
                        break;
                }
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);

            int fx = this.frame;
            int fy = 0;
            switch (this.state)
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

            BaseObject.effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.pos, Vector3.Up, null, null);
            BaseObject.effect.TextureEnabled = true;
            BaseObject.effect.Texture = GolemEnemy.tex;
            for (int e = 0; e < BaseObject.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseObject.effect.CurrentTechnique.Passes[e];
                pass.Apply();

                device.SetVertexBuffer(this.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, frame * 6, 2);
            }

            device.DepthStencilState = oldStencil;
        }
    }
}
