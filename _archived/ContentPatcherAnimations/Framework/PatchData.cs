using System;
using System.Collections.Generic;
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
        ** Fields
        *********/
        /// <summary>Simplifies access to private code.</summary>
        private readonly IReflectionHelper Reflection;

        /// <summary>The underlying patch's <c>LastChangedTick</c> property.</summary>
        private readonly IReflectedProperty<int> LastChangedTickProperty;

        /// <summary>The underlying patch's <c>IsReady</c> property.</summary>
        private readonly IReflectedProperty<bool> IsReadyProperty;

        /// <summary>The underlying patch's <c>IsApplied</c> property.</summary>
        private readonly IReflectedProperty<bool> IsAppliedProperty;

        /// <summary>The underlying patch's <c>FromAsset</c> property.</summary>
        private readonly IReflectedProperty<string> FromAssetProperty;

        /// <summary>The underlying patch's <c>TargetAsset</c> property.</summary>
        private readonly IReflectedProperty<IAssetName> TargetAssetProperty;

        /// <summary>The underlying patch's <c>FromArea</c> field.</summary>
        private readonly IReflectedField<object> FromAreaProperty;

        /// <summary>The underlying patch's <c>ToArea</c> field.</summary>
        private readonly IReflectedField<object> ToAreaProperty;

        /// <summary>The raw patch name to display in error messages.</summary>
        private readonly string Name;

        /// <summary>The last <see cref="Game1.ticks"/> value when the underlying patch data was last changed.</summary>
        private int LastChangedTick;

        /// <summary>The cached source frame data.</summary>
        private readonly IDictionary<int, Color[]> AnimationFrames = new Dictionary<int, Color[]>();


        /*********
        ** Accessors
        *********/
        /// <summary>The content pack from which the patch was loaded.</summary>
        public IContentPack ContentPack { get; }

        /// <summary>Get whether the patch is ready.</summary>
        public bool IsReady { get; protected set; }

        /// <summary>Get whether the patch is applied.</summary>
        public bool IsActive { get; protected set; }

        /// <summary>The normalized target asset name.</summary>
        public IAssetName TargetName { get; protected set; }

        /// <summary>The texture to which the patch applies.</summary>
        public Texture2D Target { get; protected set; }

        /// <summary>The source texture loaded by the patch.</summary>
        public Texture2D Source { get; protected set; }

        /// <summary>Get the source rectangle in the <see cref="Source"/>, if any.</summary>
        public Rectangle FromArea { get; protected set; }

        /// <summary>Get the source rectangle in the <see cref="Target"/>, if any.</summary>
        public Rectangle ToArea { get; protected set; }

        /// <summary>Whether to refresh the patch data on the next update, even if the underlying patch hasn't changed.</summary>
        public bool ForceNextRefresh { get; set; } = true;

        /// <summary>The current animation frame.</summary>
        public int CurrentFrame { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentPack">The content pack from which the patch was loaded.</param>
        /// <param name="name">The raw patch name to display in error messages.</param>
        /// <param name="patch">The patch instance from Content Patcher.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        /// <param name="reflection">Simplifies access to game content.</param>
        public PatchData(IContentPack contentPack, string name, object patch, IReflectionHelper reflection)
        {
            this.ContentPack = contentPack;
            this.Name = name;
            this.Reflection = reflection;

            this.LastChangedTickProperty = reflection.GetProperty<int>(patch, "LastChangedTick");
            this.IsReadyProperty = reflection.GetProperty<bool>(patch, "IsReady");
            this.IsAppliedProperty = reflection.GetProperty<bool>(patch, "IsApplied");
            this.FromAssetProperty = reflection.GetProperty<string>(patch, "FromAsset");
            this.TargetAssetProperty = reflection.GetProperty<IAssetName>(patch, "TargetAsset");
            this.FromAreaProperty = reflection.GetField<object>(patch, "FromArea");
            this.ToAreaProperty = reflection.GetField<object>(patch, "ToArea");

            this.RefreshIfNeeded();
        }

        /// <summary>Refresh the patch data if the underlying patch changed.</summary>
        public void RefreshIfNeeded()
        {
            // update IsActive (doesn't change LastChangedTick value)
            this.IsActive = this.IsAppliedProperty.GetValue();

            // refresh if patch data changed
            int lastChangedTick = this.LastChangedTickProperty.GetValue();
            if (lastChangedTick > this.LastChangedTick || this.ForceNextRefresh)
            {
                this.LastChangedTick = lastChangedTick;
                this.ForceNextRefresh = false;
                this.AnimationFrames.Clear();

                try
                {
                    this.IsReady = this.IsReadyProperty.GetValue();
                    if (this.IsReady && this.TryLoadSource(out Texture2D sourceTexture) && this.TryLoadTarget(out Texture2D targetTexture))
                    {
                        this.IsActive = this.IsAppliedProperty.GetValue();
                        this.Source = sourceTexture;
                        this.Target = targetTexture;
                        this.TargetName = this.TargetAssetProperty.GetValue();
                        this.FromArea = this.GetRectangleFromPatch(this.FromAreaProperty) ?? Rectangle.Empty;
                        this.ToArea = this.GetRectangleFromPatch(this.ToAreaProperty) ?? new Rectangle(0, 0, this.FromArea.Width, this.FromArea.Height);
                    }
                    else
                        this.Clear();
                }
                catch (Exception ex)
                {
                    Log.Trace($"Exception refreshing patch '{this.Name}', ignoring patch until its next update.\n{ex}");
                    this.Clear();
                }
            }
        }

        /// <summary>Get the texture pixels for a given animation frame.</summary>
        /// <param name="index">The frame index.</param>
        public Color[] GetAnimationFrame(int index)
        {
            if (this.Source == null)
                return Array.Empty<Color>();

            if (!this.AnimationFrames.TryGetValue(index, out Color[] pixels))
            {
                Rectangle sourceRect = this.FromArea;
                sourceRect.X += index * sourceRect.Width;

                pixels = new Color[sourceRect.Width * sourceRect.Height];
                this.Source.GetData(0, sourceRect, pixels, 0, pixels.Length);

                this.AnimationFrames[index] = pixels;
            }

            return pixels;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reset the underlying patch data.</summary>
        private void Clear()
        {
            this.IsReady = false;
            this.IsActive = false;
            this.Source = null;
            this.Target = null;
            this.FromArea = Rectangle.Empty;
            this.ToArea = Rectangle.Empty;
        }

        /// <summary>Get the source rectangle for a Content Patcher patch.</summary>
        /// <param name="field">The raw token rectangle value from the Content Patcher patch.</param>
        private Rectangle? GetRectangleFromPatch(IReflectedField<object> field)
        {
            object tokenRect = field.GetValue();
            if (tokenRect == null)
                return null;

            object[] args = { null, null }; // out Rectangle rectangle, out string error
            return this.Reflection.GetMethod(tokenRect, "TryGetRectangle").Invoke<bool>(args)
                ? (Rectangle)args[0]
                : null;
        }

        /// <summary>Load the <see cref="FromAssetProperty"/> asset if it's available.</summary>
        /// <param name="texture">The loaded texture.</param>
        private bool TryLoadSource(out Texture2D texture)
        {
            try
            {
                string path = this.FromAssetProperty.GetValue();
                texture = this.ContentPack.ModContent.Load<Texture2D>(path);
                return true;
            }
            catch
            {
                texture = null;
                return false;
            }
        }

        /// <summary>Load the <see cref="TargetAssetProperty"/> asset if it's available.</summary>
        /// <param name="texture">The loaded texture.</param>
        private bool TryLoadTarget(out Texture2D texture)
        {
            try
            {
                IAssetName assetName = this.TargetAssetProperty.GetValue();

                if (assetName.IsEquivalentTo("TileSheets/tools"))
                {
                    texture = Game1.toolSpriteSheet;
                    return true;
                }

                texture = Game1.content.Load<Texture2D>(assetName.Name);
                if (texture.GetType().Name == "ScaledTexture2D")
                    texture = this.Reflection.GetProperty<Texture2D>(texture, "STexture").GetValue();

                return true;
            }
            catch
            {
                texture = null;
                return false;
            }
        }
    }
}
