using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Framework.Objects;
using StardewValley;

namespace PyromancersJourney.Framework.Projectiles
{
    internal class PlayerFireball : BaseProjectile
    {
        public static Texture2D Tex = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/fireball.png");

        public int Level = 0;
        public Vector2 Speed;

        private static VertexBuffer Buffer;

        public override bool HurtsPlayer => false;
        public override int Damage => this.Level + 1;

        public PlayerFireball(World world)
            : base(world)
        {
            if (PlayerFireball.Buffer == null)
            {
                var vertices = new List<VertexPositionColorTexture>();
                for (int i = 0; i < 4; ++i)
                {
                    float a = 0.25f * i;
                    float b = 0.25f * (i + 1);
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(b, 0)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(b, 1)));

                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(a, 1)));
                    vertices.Add(new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(b, 1)));
                }

                PlayerFireball.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count, BufferUsage.WriteOnly);
                PlayerFireball.Buffer.SetData(vertices.ToArray());
            }
        }

        public override void Trigger(BaseObject target)
        {
            if (target is Enemy enemy)
            {
                enemy.Hurt(this.Damage);
                if (this.Level == 3)
                {
                    // explode
                }
                this.Dead = true;
            }
        }

        public override void Update()
        {
            base.Update();
            this.Position += new Vector3(this.Speed.X, 0, this.Speed.Y);

            if (this.World.Map.IsAirSolid(this.Position.X, this.Position.Z))
            {
                this.Dead = true;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            var camForward = (cam.Pos - cam.Target);
            camForward.Normalize();
            BaseProjectile.Effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.Pos, cam.Up, null, null);
            BaseProjectile.Effect.TextureEnabled = true;
            BaseProjectile.Effect.Texture = PlayerFireball.Tex;
            foreach (EffectPass pass in BaseProjectile.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(PlayerFireball.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 6 * this.Level, 2);
            }
        }
    }
}
