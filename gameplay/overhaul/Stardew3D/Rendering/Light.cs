using Microsoft.Xna.Framework;

namespace Stardew3D.Rendering;

public class Light
{
    public enum LightType
    {
        Direction,
        Point,
        Spot,
    }

    public LightType Type { get; set; } = LightType.Point;
    public Vector3 Position { get; set; } = Vector3.Forward;
    public Vector3 Direction { get; set; } = Vector3.Forward;
    public float Range { get; set; } = 4;
    public float Falloff { get; set; } = 1;
    public Color Color { get; set; } = Color.White;
    public float Intensity { get; set; } = 1;

    public bool CastsShadows { get; set; } = false;
}
