using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>A live wrapper around a Content Patcher patch.</summary>
    internal class PatchData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying Content Patcher patch object.</summary>
        public object PatchObj { get; set; }

        /// <summary>Get whether the patch is applied.</summary>
        public Func<bool> IsActive { get; set; }

        /// <summary>Get the texture to which the patch applies.</summary>
        public Func<Texture2D> TargetFunc { get; set; }

        /// <summary>The texture to which the patch applies.</summary>
        public Texture2D Target { get; set; }

        /// <summary>Get the source texture loaded by the patch.</summary>
        public Func<Texture2D> SourceFunc { get; set; }

        /// <summary>The source texture loaded by the patch.</summary>
        public Texture2D Source { get; set; }

        /// <summary>Get the source rectangle in the <see cref="Source"/>, if any.</summary>
        public Func<Rectangle> FromAreaFunc { get; set; }

        /// <summary>Get the source rectangle in the <see cref="Target"/>, if any.</summary>
        public Func<Rectangle> ToAreaFunc { get; set; }

        /// <summary>The current animation frame.</summary>
        public int CurrentFrame { get; set; }
    }
}
