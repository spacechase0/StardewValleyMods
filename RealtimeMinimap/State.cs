using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace RealtimeMinimap
{
    public class State
    {
        public bool ShowMinimap { get; set; }
        public RenderTarget2D MinimapTarget { get; set; }
        public RenderTarget2D MinimapLightmap { get; set; }

        public Dictionary<string, Texture2D> Locations { get; set; } = new();
        public List<string> RenderQueue { get; set; } = new();
    }
}
