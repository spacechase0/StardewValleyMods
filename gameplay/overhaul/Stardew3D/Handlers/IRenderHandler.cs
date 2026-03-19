using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Rendering;

namespace Stardew3D.Handlers;
public interface IRenderHandler
{
    public struct RenderContext
    {
        public GameTime Time;
        public RenderTarget2D TargetScreen;

        public SpriteBatch MenuSpriteBatch;
        public SpriteBatchProxy WorldSpriteBatch;

        public RenderBatcher WorldBatch;
        public PBREnvironment WorldEnvironment;
        public ICamera WorldCamera;
        public Matrix ParentWorldTransform = Matrix.Identity;
        public Matrix WorldTransform = Matrix.Identity;
        public bool CanBillboard = true;

        public bool Reset = false;
        public Action<RenderContext> ForceRenderIfNotAlreadyRun = static (_) => { };

        public RenderContext()
        {
        }
    }

    public void Render(RenderContext ctx);
}
