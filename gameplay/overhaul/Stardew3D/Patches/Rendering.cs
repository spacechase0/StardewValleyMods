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

[HarmonyPatch(typeof(Game1), nameof(Game1.GlobalToLocal), typeof(Vector2))]
public static class NullifyGlobalToLocalPatch1
{
    public static void Postfix(Vector2 globalPosition, ref Vector2 __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = globalPosition;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.GlobalToLocal), typeof(xTile.Dimensions.Rectangle), typeof(Vector2))]
public static class NullifyGlobalToLocalPatch2
{
    public static void Postfix(Vector2 globalPosition, ref Vector2 __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = globalPosition;
    }
}

[HarmonyPatch(typeof(Game1), nameof(Game1.GlobalToLocal), typeof(xTile.Dimensions.Rectangle), typeof(Rectangle))]
public static class NullifyGlobalToLocalPatch3
{
    public static void Postfix(Rectangle globalPosition, ref Rectangle __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = globalPosition;
    }
}

[HarmonyPatch(typeof(Character), nameof(Character.getLocalPosition))]
public static class NullifyGlobalToLocalPatch4
{
    public static void Postfix(Character __instance, ref Vector2 __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = __instance.Position + new Vector2(0, __instance.yJumpOffset);
    }
}

[HarmonyPatch(typeof(Utility), nameof(Utility.isOnScreen), typeof(Vector2), typeof(int))]
public static class NullifyGlobalToLocalPatch5
{
    public static void Postfix(ref bool __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = true;
    }
}

[HarmonyPatch(typeof(Utility), nameof(Utility.isOnScreen), typeof(Point), typeof(int), typeof(GameLocation))]
public static class NullifyGlobalToLocalPatch6
{
    public static void Postfix(ref bool __result)
    {
        if (Mod.State.ActiveMode != null)
            __result = true;
    }
}
