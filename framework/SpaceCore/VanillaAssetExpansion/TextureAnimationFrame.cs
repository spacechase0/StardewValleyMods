namespace SpaceCore.VanillaAssetExpansion
{
    /// <summary>A single frame in a texture animation.</summary>
    internal class TextureAnimationFrame
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The texture's relative file path in the content pack.</summary>
        public string FilePath { get; }

        /// <summary>The index within the texture to display.</summary>
        public int SpriteIndex { get; }

        /// <summary>The frame duration in game ticks.</summary>
        public int Duration { get; }

        /// <summary>A standardized descriptor for the frame.</summary>
        public string Descriptor { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="filePath">The texture's relative file path in the content pack.</param>
        /// <param name="spriteIndex">The index within the texture to display.</param>
        /// <param name="duration">The frame duration in game ticks.</param>
        public TextureAnimationFrame(string filePath, int spriteIndex, int duration)
        {
            this.FilePath = filePath;
            this.SpriteIndex = spriteIndex;
            this.Duration = duration;
            this.Descriptor = $"{this.FilePath}:{this.SpriteIndex}@{this.Duration}";
        }
    }
}
