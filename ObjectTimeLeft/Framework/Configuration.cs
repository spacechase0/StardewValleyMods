using StardewModdingAPI;

namespace ObjectTimeLeft.Framework
{
    internal class Configuration
    {
        public SButton ToggleKey { get; set; } = SButton.L;
        public float TextScale { get; set; } = 1.0f;
        public bool ShowOnStart { get; set; } = true;
    }
}
