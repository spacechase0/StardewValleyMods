#nullable enable
using System;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

namespace MoreGiantCrops;
public interface IMoreGiantCropsAPI
{
    public Texture2D? GetTexture(int productIndex);

    public int[] RegisteredCrops();
}

public sealed class MoreGiantCropsAPI: IMoreGiantCropsAPI
{
    public Texture2D? GetTexture(int productIndex)
        => Mod.Sprites.TryGetValue(productIndex, out Lazy<Texture2D>? tex) ? tex.Value : null;

    public int[] RegisteredCrops() => Mod.Sprites.Keys.ToArray();
}
