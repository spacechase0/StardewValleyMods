using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceCore.VanillaAssetExpansion
{
    // This class is just for editing from content packs - editing things
    // like the source texture afterwards won't do anything
    // TODO: Make it usable from C# mods by taking out ondeserialized and changing to use setter overrides
    public class TextureOverridePackData
    {
        public string TargetTexture { get; set; }
        public Rectangle TargetRect { get; set; }

        private string _sourcetex;
        public string SourceTexture
        {
            get { return _sourcetex; }
            set { _sourcetex = value; animation = TextureAnimation.ParseFrom(SourceTexture); }
        }

        public Texture2D sourceTex;
        internal TextureAnimation animation;
        internal int currFrame = 0;
        internal int currFrameTick = 0;
        internal Rectangle sourceRectCache;

        public override bool Equals(object obj)
        {
            if (obj is not TextureOverridePackData other)
                return false;
            return TargetTexture == other.TargetTexture && TargetRect == other.TargetRect && SourceTexture == other.SourceTexture;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetTexture, TargetRect, SourceTexture);
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
