using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace PyromancersJourney.Framework.Objects
{
    internal abstract class BaseObject : IDisposable
    {
        protected internal static BasicEffect Effect = BaseObject.GetBasicEffect();

        public World World { get; }

        public Vector3 Position;

        public bool Dead = false;

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

        public virtual void RenderUi(SpriteBatch b) { }

        /// <inheritdoc />
        public abstract void Dispose();

        protected BaseObject(World world)
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
