using Microsoft.Xna.Framework;

namespace Stardew3D.Rendering;
public class WorldEnvironment
{
    public Color AmbientLight { get; set; } = Color.White;
    public Light[] Lights { get; set; } = new Light[GenericModelEffect.MaxLights];
}
