using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyromancersJourney.Objects;
using StardewValley;

namespace PyromancersJourney.Projectiles
{
    public abstract class BaseProjectile
    {
        protected internal static BasicEffect effect = BaseProjectile.GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

        public virtual RectangleF BoundingBox { get; } = new(0, 0, 0.5f, 0.5f);

        public virtual bool HurtsPlayer => true;
        public virtual int Damage => 1;

        public BaseProjectile(World world)
        {
            this.World = world;
        }

        public virtual void Trigger(BaseObject target) { }

        public virtual void Update() { }

        public virtual void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            BaseProjectile.effect.Projection = projection;
            BaseProjectile.effect.View = cam.CreateViewMatrix();
            BaseProjectile.effect.World = this.MakeWorldMatrix();
        }

        protected virtual Matrix MakeWorldMatrix()
        {
            return Matrix.CreateWorld(this.Position, Vector3.Forward, Vector3.Up);
        }

        private static BasicEffect GetBasicEffect()
        {
            var ret = new BasicEffect(Game1.game1.GraphicsDevice)
            {
                Alpha = 1,
                VertexColorEnabled = true,
                LightingEnabled = false,
            };

            //ret.EnableDefaultLighting();
            ret.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            return ret;
        }
    }
}
