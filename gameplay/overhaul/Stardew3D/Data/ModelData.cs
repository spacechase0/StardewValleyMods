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

        public class AnimationMetadata
        {
            public enum LoopFinishMode
            {
                Reset,
                Hold,
                Replay,
            }
            public LoopFinishMode Loop { get; set; } = LoopFinishMode.Reset;

            public class AnimationActions
            {
                public float Time { get; set; }
                public bool? ForReverse { get; set; } = null;
                public List<string> Actions { get; set; } = new();
            }
            public List<AnimationActions> Actions { get; set; } = new();

            [OnDeserialized]
            private void OnDeserialized(StreamingContext ctx)
            {
                Actions.Sort((a, b) => a.Time.CompareTo(b.Time));
            }
        }
        public Dictionary<string, AnimationMetadata> AdditionalAnimationData { get; set; } = new();

        public class OtherModelReference : IModelMapping
        {
            public string ModelId { get; set; }

            public Vector3 Scale { get; set; } = Vector3.One;
            public Vector3 Rotation { get; set; } = Vector3.Zero;
            public Vector3 Translation { get; set; } = Vector3.Zero;

            public Dictionary<string, string> TextureMap { get; set; } = new();

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
        public List<OtherModelReference> OtherModels { get; set; } = new();

        public Dictionary<string, string> TextureMap { get; set; } = new();
        public List<string> ForceTransparency { get; set; } = new();

        public int UseExistingTransformHierarchy { get; set; } = 0;
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Translation { get; set; } = Vector3.Zero;

        public static ModelData Get(string id)
        {
            id = id.Replace('\\', '/');
            var dict = Game1.content.Load<Dictionary<string, ModelData>>($"{Mod.Instance.ModManifest.UniqueID}/Models");
            if (!Mod.Instance.ModelDataDict.TryGetValue(id, out var data))
                return null;
            return data;
        }

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
