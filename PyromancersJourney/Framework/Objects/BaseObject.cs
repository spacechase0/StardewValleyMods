using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal abstract class BaseObject
    {
        protected internal static BasicEffect Effect = BaseObject.GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

        public BaseObject(World world)
        {
            this.World = world;
        }

        public virtual void Update() { }

        public virtual void Render(GraphicsDevice device, Matrix projection, Camera cam)
        {
            BaseObject.Effect.Projection = projection;
            BaseObject.Effect.View = cam.CreateViewMatrix();
            BaseObject.Effect.World = this.MakeWorldMatrix();
        }

        public virtual void RenderOver(GraphicsDevice device, Matrix projection, Camera cam)
        {
            BaseObject.Effect.Projection = projection;
            BaseObject.Effect.View = cam.CreateViewMatrix();
            BaseObject.Effect.World = this.MakeWorldMatrix();
        }

        public virtual void RenderUi(SpriteBatch b)
        {
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
