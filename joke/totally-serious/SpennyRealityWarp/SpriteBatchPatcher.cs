using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace SpennyRealityWarp;

/// <summary>Applies Harmony patches to <see cref="SpriteBatch"/>.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
internal class SpriteBatchPatcher : BasePatcher
{
    /*********
    ** Public methods
    *********/
    /// <inheritdoc />
    public override void Apply(Harmony harmony, IMonitor monitor)
    {
        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw1), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Rectangle?), typeof(Color) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw2), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(Vector2), typeof(SpriteEffects), typeof(float) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw3), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw4), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw5), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Vector2), typeof(Color) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw6), priority: Priority.VeryLow)
        );

        harmony.Patch(
            original: this.RequireMethod<SpriteBatch>(nameof(SpriteBatch.Draw), new[] { typeof(Texture2D), typeof(Rectangle), typeof(Color) }),
            prefix: this.GetHarmonyMethod(nameof(Before_Draw7), priority: Priority.VeryLow)
        );
    }


    /*********
    ** Private methods
    *********/
    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>.</summary>
    private static void Before_Draw1(SpriteBatch __instance, Texture2D texture, ref Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, SpriteEffects effects, float layerDepth, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return;
        if (!__runOriginal)
            return;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return;

        Vector2 originalOrigin = destinationRectangle.Location.ToVector2() + origin;
        Vector2 newOrigin = Mod.instance.Transform(originalOrigin);
        Vector2 diff = newOrigin - originalOrigin;

        destinationRectangle = new((int)(destinationRectangle.X + diff.X), (int)(destinationRectangle.Y + diff.Y), destinationRectangle.Width, destinationRectangle.Height);
        rotation += Mod.instance.worldRotation.Value;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color)"/>.</summary>
    private static bool Before_Draw2(SpriteBatch __instance, Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return true;
        if (!__runOriginal)
            return true;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return true;

        __instance.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 1);
        return false;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,Vector2,SpriteEffects,float)"/>.</summary>
    private static void Before_Draw3(SpriteBatch __instance, Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, ref Vector2 scale, SpriteEffects effects, float layerDepth, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return;
        if (!__runOriginal)
            return;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return;

        float rfactor = 1f;
        if (texture.Name?.StartsWith("Maps/") ?? false)
            rfactor = 0; // why????
        else if (texture.Name != "TileSheets/Craftables")
            rfactor = 0; // why?????????

        Vector2 originalOrigin = position + origin;
        Vector2 newOrigin = Mod.instance.Transform(originalOrigin, rfactor: rfactor);
        Vector2 diff = newOrigin - originalOrigin;
        
        position += diff;
        rotation += Mod.instance.worldRotation.Value * rfactor;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color,float,Vector2,float,SpriteEffects,float)"/>.</summary>
    private static void Before_Draw4(SpriteBatch __instance, Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, ref float rotation, ref Vector2 origin, float scale, SpriteEffects effects, float layerDepth, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return;
        if (!__runOriginal)
            return;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return;

        Vector2 originalOrigin = position + origin;
        Vector2 newOrigin = Mod.instance.Transform(originalOrigin);
        Vector2 diff = newOrigin - originalOrigin;

        position += diff;
        rotation += Mod.instance.worldRotation.Value;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Rectangle?,Color)"/>.</summary>
    private static bool Before_Draw5(SpriteBatch __instance, ref Texture2D texture, ref Vector2 position, Rectangle? sourceRectangle, Color color, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return true;
        if (!__runOriginal)
            return true;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return true;

        __instance.Draw(texture, position, sourceRectangle, color, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        return false;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Vector2,Color)"/>.</summary>
    private static bool Before_Draw6(SpriteBatch __instance, ref Texture2D texture, ref Vector2 position, Color color, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return true;
        if (!__runOriginal)
            return true;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return true;

        __instance.Draw(texture, position, null, color, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
        return false;
    }

    /// <summary>The method to call before <see cref="SpriteBatch.Draw(Texture2D,Rectangle,Color)"/>.</summary>
    private static bool Before_Draw7(SpriteBatch __instance, ref Texture2D texture, ref Rectangle destinationRectangle, Color color, bool __runOriginal)
    {
        if (!Mod.instance.inWorldRender || !Mod.instance.worldRotation.HasValue)
            return true;
        if (!__runOriginal)
            return true;
        if (texture.Name is "Characters/Penny" or "Characters/Penny_Beach")
            return true;

        __instance.Draw(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 1);
        return false;
    }
}
