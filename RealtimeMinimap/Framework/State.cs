using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace RealtimeMinimap.Framework
{
    internal class State
    {
        public bool ShowMinimap { get; set; }
        public RenderTarget2D MinimapTarget { get; set; }
        public RenderTarget2D MinimapLightmap { get; set; }

        public Dictionary<string, Texture2D> Locations { get; set; } = new();
        public List<string> RenderQueue { get; set; } = new();
    }
}
