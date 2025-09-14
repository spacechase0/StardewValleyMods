using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Framework.Objects;
using StardewValley;

namespace PyromancersJourney.Framework.Projectiles
{
    internal abstract class BaseProjectile
    {
        protected internal static BasicEffect Effect = BaseProjectile.GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

        public virtual RectangleF BoundingBox { get; } = new(0, 0, 0.5f, 0.5f);

        public virtual bool HurtsPlayer => true;
        public virtual int Damage => 1;

        public virtual void Trigger(BaseObject target) { }

        public virtual void Update() { }

        public virtual void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            BaseProjectile.Effect.Projection = projection;
            BaseProjectile.Effect.View = cam.CreateViewMatrix();
            BaseProjectile.Effect.World = this.MakeWorldMatrix();
        }

        protected BaseProjectile(World world)
        {
            this.World = world;
        }

        protected virtual Matrix MakeWorldMatrix()
        {
            return Matrix.CreateWorld(this.Position, Vector3.Forward, Vector3.Up);
        }

        private static BasicEffect GetBasicEffect()
        {
            return new(Game1.game1.GraphicsDevice)
            {
                Alpha = 1,
                VertexColorEnabled = true,
                LightingEnabled = false,
                AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f)
            };
        }
    }
}
