using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Objects;
using StardewValley;

namespace PyromancersJourney.Projectiles
{
    public class PlayerFireball : BaseProjectile
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load<Texture2D>("assets/fireball.png");

        public int Level = 0;
        public Vector2 Speed;

        private static VertexBuffer buffer;

        public override bool HurtsPlayer => false;
        public override int Damage => this.Level + 1;

        public PlayerFireball(World world)
            : base(world)
        {
            if (PlayerFireball.buffer == null)
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

                PlayerFireball.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
                PlayerFireball.buffer.SetData(vertices.ToArray());
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

            if (this.World.map.IsAirSolid(this.Position.X, this.Position.Z))
            {
                this.Dead = true;
            }
        }

        public override void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            base.Render(device, projection, cam);
            var camForward = (cam.pos - cam.target);
            camForward.Normalize();
            BaseProjectile.effect.World = Matrix.CreateConstrainedBillboard(this.Position, cam.pos, cam.up, null, null);
            BaseProjectile.effect.TextureEnabled = true;
            BaseProjectile.effect.Texture = PlayerFireball.tex;
            for (int e = 0; e < BaseProjectile.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseProjectile.effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(PlayerFireball.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 6 * this.Level, 2);
            }
        }
    }
}
