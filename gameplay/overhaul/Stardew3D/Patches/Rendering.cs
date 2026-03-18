using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Stardew3D.GameModes.FirstPersonVR;
using StardewValley;

namespace Stardew3D.Patches;

[HarmonyPatch(typeof(Game1), "renderScreenBuffer")]
internal static class SuppressDrawingUiAfterFramePatch
{
    public static bool Prefix(Game1 __instance, RenderTarget2D target_screen)
    {
        if (Stardew3D.Mod.State.ActiveMode is FirstPersonVRGameMode)
        {
            renderScreenBuffer(__instance, target_screen);
            return false;
        }

        return true;
    }

    private static void renderScreenBuffer(Game1 __instance, RenderTarget2D target_screen)
    {
        Game1.graphics.GraphicsDevice.SetRenderTarget(null);
        if (!__instance.takingMapScreenshot && !LocalMultiplayer.IsLocalMultiplayer() && (target_screen == null || !target_screen.IsContentLost))
        {
            if (__instance.ShouldDrawOnBuffer() && target_screen != null)
            {
                __instance.GraphicsDevice.Clear(Game1.bgColor);
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                Game1.spriteBatch.Draw(target_screen, new Vector2(0f, 0f), target_screen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                Game1.spriteBatch.End();
                /*
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                Game1.spriteBatch.Draw(__instance.uiScreen, new Vector2(0f, 0f), __instance.uiScreen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
                Game1.spriteBatch.End();
                */
            }
            else
            {
                /*
                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                Game1.spriteBatch.Draw(__instance.uiScreen, new Vector2(0f, 0f), __instance.uiScreen.Bounds, Color.White, 0f, Vector2.Zero, Game1.options.uiScale, SpriteEffects.None, 1f);
                Game1.spriteBatch.End();
                */
            }
        }
    }
}
