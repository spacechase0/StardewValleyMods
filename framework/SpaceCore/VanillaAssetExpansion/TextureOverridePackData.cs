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
        public Rectangle TargetRect { get; set; } = Rectangle.Empty;

        private string _sourcetex;
        public string SourceTexture
        {
            get { return _sourcetex; }
            set { _sourcetex = value; animation = TextureAnimation.ParseFrom(SourceTexture); }
        }

        public Point? SourceSizeOverride { get; set; } = null;

        public double? ChancePerTick { get; set; } = null;

        public bool FullSheetMode { get; set; } = false;

        public float FullSheetModeScaleModifier { get; set; } = 1f;

        internal Texture2D sourceTex;
        internal TextureAnimation animation;
        internal int currFrame = 0;
        internal int currFrameTick = 0;
        internal Rectangle sourceRectCache;
        internal Texture2D? targetTex;
        internal Point texturePosCache;

        internal Rectangle GetDrawOverrideSourceRect(Rectangle originalRect, Rectangle targetRect)
        {
            return GetDrawOverrideSourceRect(originalRect, targetRect.Width / (float)originalRect.Width, targetRect.Height / (float)originalRect.Height);
        }

        internal Rectangle GetDrawOverrideSourceRect(Rectangle originalRect, float originalScaleX, float originalScaleY)
        {
            return new Rectangle(
                (int)((texturePosCache.X + originalRect.X) / FullSheetModeScaleModifier),
                texturePosCache.Y + (int)(originalRect.Y / FullSheetModeScaleModifier),
                (int)(originalRect.Width / FullSheetModeScaleModifier),
                (int)(originalRect.Height / FullSheetModeScaleModifier)
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is not TextureOverridePackData other)
                return false;
            return TargetTexture == other.TargetTexture && TargetRect == other.TargetRect && SourceTexture == other.SourceTexture && FullSheetMode == other.FullSheetMode && FullSheetModeScaleModifier == other.FullSheetModeScaleModifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetTexture, TargetRect, SourceTexture, FullSheetMode, FullSheetModeScaleModifier);
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
