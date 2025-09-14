using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using static StardewValley.FarmerSprite;

namespace SpaceShared.APIs
{
    public interface IFashionSenseApi
    {
        public enum Type
        {
            Unknown,
            Hair,
            Accessory,
            [Obsolete("No longer maintained. Use Accessory instead.")]
            AccessorySecondary,
            [Obsolete("No longer maintained. Use Accessory instead.")]
            AccessoryTertiary,
            Hat,
            Shirt,
            Pants,
            Sleeves,
            Shoes,
            Player
        }

        public record RawTextureData(int Width, int Height, Color[] Data) : IRawTextureData;

        KeyValuePair<bool, string> SetAppearance(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessorySlot(string accessoryId, int accessorySlot);
        KeyValuePair<bool, string> SetAppearanceColor(Type appearanceType, Color color, IManifest callerManifest);
        KeyValuePair<bool, string> SetAccessoryColor(Color color, int accessorySlot);

        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetHatAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetHairAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessoryPrimaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessorySecondaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAccessorySlot.")]
        KeyValuePair<bool, string> SetAccessoryTertiaryAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetShirtAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetSleevesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetPantsAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use SetAppearance.")]
        KeyValuePair<bool, string> SetShoesAppearance(string targetPackId, string targetAppearanceName, IManifest callerManifest);

        KeyValuePair<bool, string> ClearAppearance(Type appearanceType, IManifest callerManifest);
        KeyValuePair<bool, string> ClearAccessorySlot(int accessorySlot, IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearHatAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearHairAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessoryPrimaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessorySecondaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance (for all accessories) or ClearAccessorySlot.")]
        KeyValuePair<bool, string> ClearAccessoryTertiaryAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearShirtAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearSleevesAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearPantsAppearance(IManifest callerManifest);
        [Obsolete("No longer maintained as of Fashion Sense v5. Use ClearAppearance.")]
        KeyValuePair<bool, string> ClearShoesAppearance(IManifest callerManifest);

        KeyValuePair<bool, string> GetCurrentAppearanceId(Type appearanceType, Farmer target = null);
        KeyValuePair<bool, Color> GetAppearanceColor(Type appearanceType, Farmer target = null);
        KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, bool getOriginalTexture = false);
        KeyValuePair<bool, IRawTextureData> GetAppearanceTexture(string appearanceId, bool getOriginalTexture = false);
        KeyValuePair<bool, string> SetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);
        KeyValuePair<bool, string> SetAppearanceTexture(string appearanceId, IRawTextureData textureData, IManifest callerManifest, bool shouldOverridePersist = false);
        KeyValuePair<bool, string> ResetAppearanceTexture(Type appearanceType, string targetPackId, string targetAppearanceName, IManifest callerManifest);
        KeyValuePair<bool, string> ResetAppearanceTexture(string appearanceId, IManifest callerManifest);

        KeyValuePair<bool, List<string>> GetOutfitIds();
        KeyValuePair<bool, string> GetCurrentOutfitId();
        KeyValuePair<bool, string> SetCurrentOutfitId(string outfitId, IManifest callerManifest);

        KeyValuePair<bool, string> SetAppearanceLockState(Type appearanceType, string targetPackId, string targetAppearanceName, bool isLocked, IManifest callerManifest);

        public interface IDrawTool
        {
            public Farmer Farmer { get; init; }
            public SpriteBatch SpriteBatch { get; init; }
            public FarmerRenderer FarmerRenderer { get; init; }
            public Texture2D BaseTexture { get; init; }
            public Rectangle FarmerSourceRectangle { get; init; }
            public AnimationFrame AnimationFrame { get; init; }
            public bool IsDrawingForUI { get; init; }
            public Color OverrideColor { get; init; }
            public Color AppearanceColor { get; set; }
            public Vector2 Position { get; init; }
            public Vector2 Origin { get; init; }
            public Vector2 PositionOffset { get; init; }
            public int FacingDirection { get; init; }
            public int CurrentFrame { get; init; }
            public float Scale { get; init; }
            public float Rotation { get; init; }
            public float LayerDepthSnapshot { get; set; }
        }
        KeyValuePair<bool, string> RegisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest, Func<IDrawTool, bool> appearanceDrawOverride);
        KeyValuePair<bool, string> UnregisterAppearanceDrawOverride(Type appearanceType, IManifest callerManifest);
        KeyValuePair<bool, string> IsDrawOverrideActive(Type appearanceType, IManifest callerManifest);

        // Events
        event EventHandler SetSpriteDirtyTriggered;

        /*
         * Example usages (using the Fashion Sense example pack)
         * 
         * var api = Helper.ModRegistry.GetApi<IApi>("PeacefulEnd.FashionSense");
         * var response = api.SetHatAppearance("ExampleAuthor.ExampleFashionSensePack", "Animated Pumpkin Head", this.ModManifest);
         * if (response.Key is true)
         * {
         *     // Setting was successful!
         * }
         * 
         * // Attempt to get and brighten the current hair texture (note that counter is a variable outside of this snippet)
         * response = api.GetCurrentAppearanceId(IFashionSenseApi.Type.Hair);
         * if (response.Key is true)
         * {
         *    var appearanceId = response.Value;
         *    var dataResponse = api.GetAppearanceTexture(appearanceId);
         *    if (dataResponse.Key is true)
         *    {
         *       var textureData = dataResponse.Value;
         *       Color[] colors = new Color[textureData.Width * textureData.Height];
         *
         *       bool shouldReset = false;
         *       for (int i = 0; i < textureData.Data.Length; i++)
         *       {
         *          var pixel = textureData.Data[i];
         *          if (pixel != Color.Transparent)
         *          {
         *              var red = pixel.R + 10 > 255 ? 255 : pixel.R + 10;
         *              var green = pixel.G + 10 > 255 ? 255 : pixel.G + 10;
         *              var blue = pixel.B + 10 > 255 ? 255 : pixel.B + 10;
         *
         *              colors[i] = new Color(red, green, blue);
         *              if (red == 255 && green == 255 && blue == 255)
         *              {
         *                  shouldReset = true;
         *              }
         *          }
         *          else
         *          {
         *              colors[i] = pixel;
         *          }
         *       }
         *       
         *       // Reset the texture after 15 brighten cycles
         *       if (shouldReset && counter >= 15)
         *       {
         *          response = api.ResetAppearanceTexture(appearanceId, this.ModManifest);
         *          counter = 0;
         *          
         *          Monitor.Log(response.Value, LogLevel.Debug);
         *       }
         *       else
         *       {
         *          response = api.SetAppearanceTexture(appearanceId, new IFashionSenseApi.RawTextureData(textureData.Width, textureData.Height, colors), this.ModManifest);
         *          counter += 1;
         *
         *          Monitor.Log(response.Value, LogLevel.Debug);
         *       }
         *    }
         * }
         */
    }
}
