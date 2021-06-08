using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace FireArcadeGame.Objects
{
    public abstract class BaseObject
    {
        protected internal static BasicEffect effect = GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

        public BaseObject(World world)
        {
            World = world;
        }

        public virtual void Update() { }

        public virtual void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            effect.Projection = projection;
            effect.View = cam.CreateViewMatrix();
            effect.World = MakeWorldMatrix();
        }

        public virtual void RenderOver(GraphicsDevice device, Matrix projection, Camera cam)
        {
            effect.Projection = projection;
            effect.View = cam.CreateViewMatrix();
            effect.World = MakeWorldMatrix();
        }

        public virtual void RenderUi(SpriteBatch b)
        {
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
