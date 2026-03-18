using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Stardew3D;

public class Configuration
{
    public int FieldOfViewDegrees { get; set; } = 70;
    public int MultisampleCount { get; set; } = 0;

    public KeybindList ToggleThirdDimension { get; set; } = new(SButton.Home);
    public KeybindList ToggleVirtualReality { get; set; } = new(SButton.End);
    public KeybindList ToggleEditor { get; set; } = new(SButton.PageUp);
    public KeybindList ToggleShowInteractionShapes { get; set; } = new(SButton.Insert);
}
