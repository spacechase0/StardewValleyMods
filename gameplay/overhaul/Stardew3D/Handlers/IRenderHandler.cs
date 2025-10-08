using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.Rendering;
using StardewValley.Mods;

namespace Stardew3D.Handlers;
public interface IRenderHandler
{
    public struct RenderContext
    {
        public GameTime Time;
        public RenderTarget2D TargetScreen;

        public SpriteBatch MenuSpriteBatch;

        public RenderBatcher WorldBatch;
        public PBREnvironment WorldEnvironment;
        public ICamera WorldCamera;
        public Matrix WorldTransform;

        public Action<RenderContext> ForceRenderIfNotAlreadyRun = static (_) => { };

        public RenderContext()
        {
        }
    }

    public void Render(RenderContext ctx);
}
