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
        public string HeightmapPath { get; set; }
        public int HeightmapMin { get; set; }
        public int HeightmapMax { get; set; }

        [JsonIgnore]
        public MonoGameDeviceContent<MonoGameModelTemplate> Model;

        [JsonIgnore]
        public Texture2D HeightmapTex { get; set; }
        public Color[] Heightmap { get; set; }
        public Color HeightmapMinColor { get; set; } = Color.White;
        public Color HeightmapMaxColor { get; set; } = Color.Black;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            if (ModelPath != null)
            {
                Model = MonoGameModelTemplate.LoadDeviceModel(Game1.game1.GraphicsDevice, Util.FetchFullPath(Mod.instance.Helper.ModRegistry, ModelPath));
            }
            if (HeightmapPath != null)
            {
                HeightmapTex = Util.FetchTexture(Mod.instance.Helper.ModRegistry, HeightmapPath);
                Heightmap = new Color[HeightmapTex.Width * HeightmapTex.Height ];
                HeightmapTex.GetData(Heightmap);

                foreach (var col in Heightmap)
                {
                    if (col.R < HeightmapMaxColor.R)
                        HeightmapMinColor = col;
                    if (col.R > HeightmapMaxColor.R)
                        HeightmapMaxColor = col;
                }
            }
        }

        public void Dispose()
        {
            Model?.Dispose();
        }
    }
}
