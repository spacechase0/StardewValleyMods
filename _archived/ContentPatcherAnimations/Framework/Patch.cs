namespace ContentPatcherAnimations.Framework
{
    /// <summary>Raw patch fields loaded from a Content Patcher content pack.</summary>
    internal class Patch
    {
        /*********
        ** Accessors
        *********/
        //
        // Target and FromFile are taken from CP since it handles tokens
        // Same for FromArea and ToArea
        //

        /// <summary>The unique patch log name.</summary>
        /// <remarks>This is used to identify the patch and check if it's active.</remarks>
        public string LogName { get; set; }

        /// <summary>The patch action.</summary>
        /// <remarks>This should always be <c>EditImage</c> for animated patches.</remarks>
        public string Action { get; set; }

        /// <summary>The number of game ticks between each frame, defined by the content pack author.</summary>
        public int AnimationFrameTime { get; set; } = -1;

        /// <summary>The number of animation frames, defined by the content pack author.</summary>
        public int AnimationFrameCount { get; set; } = -1;
    }
}
