using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MoreGiantCrops
{
    // Copied/modified from JA
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming convention is set by Harmony.")]
    public static class GiantCropPatches
    {
        public static bool Draw_Prefix(GiantCrop __instance, SpriteBatch spriteBatch, Vector2 tileLocation)
        {
            try
            {
                if ( Mod.sprites.ContainsKey( __instance.parentSheetIndex.Value ) )
                {
                    Texture2D tex = Mod.sprites[__instance.parentSheetIndex.Value];
                    double shakeTimer = Mod.instance.Helper.Reflection.GetField<float>(__instance, "shakeTimer").GetValue();
                    spriteBatch.Draw(tex, Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f - new Vector2(shakeTimer > 0.0 ? (float)(Math.Sin(2.0 * Math.PI / shakeTimer) * 2.0) : 0.0f, 64f)), null, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((double)tileLocation.Y + 2.0) * 64.0 / 10000.0));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.error($"Failed in {nameof(GiantCropPatches)}.{nameof(Draw_Prefix)}:\n{ex}");
                return true;
            }
        }
    }
}
