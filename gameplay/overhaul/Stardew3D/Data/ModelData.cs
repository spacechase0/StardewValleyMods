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
    public class ModelData : IModelMapping
    {
        public string ModelFilePath { get; set; }
        public string SubModelPath { get; set; } // If not unique, picks a random one of the matching ones

        public Dictionary<string, string> TextureMap { get; set; } = new();

        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;

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
