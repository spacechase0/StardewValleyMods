using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RealtimeMinimap.Framework
{
    internal class Configuration
    {
        public bool ShowByDefault { get; set; } = true;
        public KeybindList ToggleShowKey { get; set; } = new KeybindList(new Keybind(SButton.M, SButton.LeftControl), new Keybind(SButton.M, SButton.RightControl));
        //public KeybindList MapKey { get; set; } = new KeybindList( new Keybind( SButton.M ) );

        public int UpdateInterval = -1;

        public float MinimapAnchorX { get; set; } = 0;
        public float MinimapAnchorY { get; set; } = 0;

        public int MinimapOffsetX { get; set; } = 24;
        public int MinimapOffsetY { get; set; } = 24;

        public int MinimapSize { get; set; } = 250;

        public int RenderHeads { get; set; } = 2;
        public int RenderNpcs { get; set; } = 2;
        public int RenderWoodSigns { get; set; } = 2;
        public int RenderStoneSigns { get; set; } = 2;
        public int RenderDarkSigns { get; set; } = 3;
    }
}
