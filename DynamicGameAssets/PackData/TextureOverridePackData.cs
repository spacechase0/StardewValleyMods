using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace DynamicGameAssets.PackData
{
    public class TextureOverridePackData : BasePackData
    {
        public string TargetTexture { get; set; }
        public Rectangle TargetRect { get; set; }
        public string SourceTexture { get; set; }

        public TexturedRect GetCurrentTexture()
        {
            return this.pack.GetTexture(this.SourceTexture, this.TargetRect.Width, this.TargetRect.Height);
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext ctx)
        {
            // This is important because the paths need to match exactly.
            // Starting in SDV 1.5.5, these are always '/', not OS-dependent.
            this.TargetTexture = this.TargetTexture.Replace('\\', '/');
        }
    }
}
