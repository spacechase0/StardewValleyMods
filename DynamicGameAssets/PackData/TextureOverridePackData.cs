using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
            return pack.GetTexture( SourceTexture, TargetRect.Width, TargetRect.Height );
        }

        [OnDeserialized]
        public void OnDeserialized( StreamingContext ctx )
        {
            // This is important because the paths need to match exactly.
            // Starting in SDV 1.5.5, these are always '/', not OS-dependent.
            TargetTexture = TargetTexture.Replace( '\\', '/' );
        }
    }
}
