using Microsoft.Xna.Framework;

namespace Stardew3D.Models;

public interface IModelMapping
{
    public Vector3 Scale { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Translation { get; set; }

    public Dictionary<string, string> TextureMap { get; set; }
}
