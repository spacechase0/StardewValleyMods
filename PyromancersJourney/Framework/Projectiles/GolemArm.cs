using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Framework.Objects;
using StardewValley;

namespace PyromancersJourney.Framework.Projectiles
{
    internal class GolemArm : BaseProjectile
    {
        public static Texture2D Tex = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/golem_arm.png");

        public Vector2 Speed;

        private static VertexBuffer Buffer;

        public override bool HurtsPlayer => true;
        public override int Damage => 1;

        public GolemArm(World world)
            : base(world)
        {
            if (GolemArm.Buffer == null)
            {
                float a = 0;
                float b = 1;
                var vertices = new VertexPositionColorTexture[]
                {
                    new(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)),
                    new(new Vector3(0.5f, 0, 0), Color.White, new Vector2(b, 0)),
                    new(new Vector3(0.5f, 1, 0), Color.White, new Vector2(b, 1)),
                    new(new Vector3(0, 0, 0), Color.White, new Vector2(a, 0)),
                    new(new Vector3(0, 1, 0), Color.White, new Vector2(a, 1)),
                    new(new Vector3(0.5f, 1, 0), Color.White, new Vector2(b, 1))
                };


                GolemArm.Buffer = new VertexBuffer(Game1.game1.GraphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
                GolemArm.Buffer.SetData(vertices);
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
            BaseProjectile.Effect.Texture = GolemArm.Tex;
            foreach (EffectPass pass in BaseProjectile.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.SetVertexBuffer(GolemArm.Buffer);
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }
        }
    }
}
