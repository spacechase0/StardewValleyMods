using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Models;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Stardew3D.Data
{
    [JsonConverter( typeof( ModelDataCreationConverter ) )]
    public class ModelData : IModelMapping
    {
        public virtual string Type => $"{Mod.Instance.ModManifest.UniqueID}/Model";

        public string ModelFilePath { get; set; }
        public string SubModelPath { get; set; } // If not unique, picks a random one of the matching ones

        public class OtherModelReference : IModelMapping
        {
            public string ModelId { get; set; }

            public Vector3 Scale { get; set; }
            public Vector3 Rotation { get; set; }
            public Vector3 Translation { get; set; }

            public Dictionary<string, string> TextureMap { get; set; } = new();
        }
        public List<OtherModelReference> OtherModels { get; set; } = new();

        public Dictionary<string, string> TextureMap { get; set; } = new();

        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;

        // Objects
        public Vector3? HeldObjectOffset { get; set; }
        public float HeldObjectScale { get; set; } = 1;
        public BoundingBox? InteractBox { get; set; }

        // Characters
        // ...

        // Farmer
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
}
