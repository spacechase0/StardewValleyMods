using FireArcadeGame.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FireArcadeGame.Projectiles
{
    public abstract class BaseProjectile
    {
        protected internal static BasicEffect effect = GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

        public virtual RectangleF BoundingBox { get; } = new RectangleF(0, 0, 0.5f, 0.5f);

        public virtual bool HurtsPlayer => true;
        public virtual int Damage => 1;

        public BaseProjectile(World world)
        {
            World = world;
        }

        public virtual void Trigger(BaseObject target) { }

        public virtual void Update() { }

        public virtual void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            effect.Projection = projection;
            effect.View = cam.CreateViewMatrix();
            effect.World = MakeWorldMatrix();
        }

        protected virtual Matrix MakeWorldMatrix()
        {
            return Matrix.CreateWorld(Position, Vector3.Forward, Vector3.Up);
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
