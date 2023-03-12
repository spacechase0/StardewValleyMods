using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace RealtimeMinimap.Framework
{
    internal class State
    {
        public bool ShowMinimap { get; set; }
        public RenderTarget2D MinimapTarget { get; set; }
        public RenderTarget2D MinimapLightmap { get; set; }

        public bool DoRenderThisTick { get; set; } = false;
    }
}
