using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Objects;
using StardewValley;

namespace PyromancersJourney.Projectiles
{
    public class GolemArm : BaseProjectile
    {
        public static Texture2D tex = Mod.instance.Helper.Content.Load<Texture2D>("assets/golem_arm.png");

        public Vector2 Speed;

        private static VertexBuffer buffer;

        public override bool HurtsPlayer => true;
        public override int Damage => 1;

        public GolemArm(World world)
            : base(world)
        {
            if (GolemArm.buffer == null)
            {
                float a = 0;
                float b = 1;
                var vertices = new List<VertexPositionColorTexture>();
                vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(0.5f, 0, 0), Color.White, new Vector2(b, 0)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(0.5f, 1, 0), Color.White, new Vector2(b, 1)));

                vertices.Add(new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(a, 1)));
                vertices.Add(new VertexPositionColorTexture(new Vector3(0.5f, 1, 0), Color.White, new Vector2(b, 1)));

                GolemArm.buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Count(), BufferUsage.WriteOnly);
                GolemArm.buffer.SetData(vertices.ToArray());
            }
        }

        public override void Trigger(BaseObject target)
        {
            if (target is Player player)
            {
                player.Hurt(this.Damage);
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
            BaseProjectile.effect.Texture = GolemArm.tex;
            for (int e = 0; e < BaseProjectile.effect.CurrentTechnique.Passes.Count; ++e)
            {
                var pass = BaseProjectile.effect.CurrentTechnique.Passes[e];
                pass.Apply();
                device.SetVertexBuffer(GolemArm.buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
    }
}