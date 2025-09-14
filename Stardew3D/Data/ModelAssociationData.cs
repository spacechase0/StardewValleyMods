using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Stardew3D.Models;
using StardewModdingAPI.Utilities;

namespace Stardew3D.Data;
public class ModelAssociationData : IModelMapping
{
    public SingleModelAssociationData[] ModelsUsed { get; set; }

    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Translation { get; set; } = Vector3.Zero;

    public Dictionary<string, string> TextureMap { get; set; } = new();

    // Objects
    public Vector3? HeldObjectOffset { get; set; }
    public float HeldObjectScale { get; set; } = 1;
    public BoundingBox? InteractBox { get; set; }

    // Characters
    // ...

    // Farmer
    // ...

    // Locations
    // ...

    [OnDeserialized]
    private void OnDeserialized(StreamingContext ctx)
    {
        Dictionary<string, string> newTexMap = new();
        foreach (var entry in TextureMap)
        {
            newTexMap.Add(PathUtilities.NormalizePath(entry.Key), PathUtilities.NormalizePath(entry.Value));
        }
        TextureMap = newTexMap;
    }
}
