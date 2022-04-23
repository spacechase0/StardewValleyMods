using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SharpGLTF.Runtime;
using SpaceShared;
using StardewValley;

namespace Stardew3D
{
    public class ModelData : IDisposable
    {
        public string ID { get; set; }
        public string ModelPath { get; set; }

        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 Translation { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;

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

        [JsonIgnore]
        public MonoGameDeviceContent<MonoGameModelTemplate> Model;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (ModelPath != null)
            {
                Model = MonoGameModelTemplate.LoadDeviceModel(Game1.game1.GraphicsDevice, Util.FetchFullPath(Mod.instance.Helper.ModRegistry, ModelPath));
            }
        }

        public void Dispose()
        {
            Model?.Dispose();
        }
    }
}
