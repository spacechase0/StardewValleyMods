using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcherAnimations.Framework
{
    /// <summary>A live wrapper around a Content Patcher patch.</summary>
    internal class PatchData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The underlying Content Patcher patch object.</summary>
        public object PatchObj { get; init; }

        /// <summary>Get whether the patch is applied.</summary>
        public Func<bool> IsActive { get; init; }

        /// <summary>Get the texture to which the patch applies.</summary>
        public Func<Texture2D> TargetFunc { get; init; }

        /// <summary>The texture to which the patch applies.</summary>
        public Texture2D Target { get; protected set; }

        /// <summary>Get the source texture loaded by the patch.</summary>
        public Func<Texture2D> SourceFunc { get; init; }

        /// <summary>The source texture loaded by the patch.</summary>
        public Texture2D Source { get; protected set; }

        /// <summary>Get the source rectangle in the <see cref="Source"/>, if any.</summary>
        public Func<Rectangle> FromAreaFunc { get; init; }

        /// <summary>Get the source rectangle in the <see cref="Target"/>, if any.</summary>
        public Func<Rectangle> ToAreaFunc { get; init; }

        /// <summary>The current animation frame.</summary>
        public int CurrentFrame { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct patch data for a loaded patch.</summary>
        /// <param name="contentPack">The content pack from which the patch was loaded.</param>
        /// <param name="patch">The patch instance from Content Patcher.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public static PatchData ReadPatchData(IContentPack contentPack, object patch, IReflectionHelper reflection)
        {
            IReflectedProperty<bool> appliedProp = reflection.GetProperty<bool>(patch, "IsApplied");
            IReflectedProperty<string> sourceProp = reflection.GetProperty<string>(patch, "FromAsset");
            IReflectedProperty<string> targetProp = reflection.GetProperty<string>(patch, "TargetAsset");

            Rectangle FromAreaFunc() => PatchData.GetRectangleFromPatch(reflection, patch, "FromArea");

            return new PatchData
            {
                PatchObj = patch,
                IsActive = appliedProp.GetValue,
                SourceFunc = () => contentPack.LoadAsset<Texture2D>(sourceProp.GetValue()),
                TargetFunc = () => PatchData.FindTargetTexture(reflection, targetProp.GetValue()),
                FromAreaFunc = FromAreaFunc,
                ToAreaFunc = () =>
                {
                    var fromArea = FromAreaFunc();
                    return PatchData.GetRectangleFromPatch(reflection, patch, "ToArea", new Rectangle(0, 0, fromArea.Width, fromArea.Height));
                }
            };
        }

        /// <summary>Reload the <see cref="Source"/> and <see cref="Target"/> textures.</summary>
        public void Reload()
        {
            this.Source = this.SourceFunc();
            this.Target = this.TargetFunc();
        }

        /// <summary>Clear the <see cref="Target"/> value.</summary>
        public void ClearTarget()
        {
            this.Target = null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the texture for a given asset name.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="target">The asset name to match.</param>
        private static Texture2D FindTargetTexture(IReflectionHelper reflection, string target)
        {
            if (PathUtilities.NormalizeAssetName(target) == PathUtilities.NormalizeAssetName("TileSheets/tools"))
                return reflection.GetField<Texture2D>(typeof(Game1), "_toolSpriteSheet").GetValue();

            var tex = Game1.content.Load<Texture2D>(target);
            if (tex.GetType().Name == "ScaledTexture2D")
            {
                Log.Trace($"Found ScaledTexture2D from PyTK: {target}");
                tex = reflection.GetProperty<Texture2D>(tex, "STexture").GetValue();
            }
            return tex;
        }

        /// <summary>Get the source rectangle for a Content Patcher patch.</summary>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="targetPatch">The Content Patcher patch.</param>
        /// <param name="rectName">The rectangle field name.</param>
        /// <param name="defaultTo">The default rectangle value if the field isn't defined.</param>
        private static Rectangle GetRectangleFromPatch(IReflectionHelper reflection, object targetPatch, string rectName, Rectangle defaultTo = default)
        {
            object tokenRect = reflection.GetField<object>(targetPatch, rectName).GetValue();
            if (tokenRect == null)
                return defaultTo;

            object[] args = { null, null }; // out Rectangle rectangle, out string error
            return reflection.GetMethod(tokenRect, "TryGetRectangle").Invoke<bool>(args)
                ? (Rectangle)args[0]
                : Rectangle.Empty;
        }
    }
}
