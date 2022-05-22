using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace AQualityMod
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Texture2D wonderfulTex, poorTex;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            wonderfulTex = Helper.ModContent.Load<Texture2D>("assets/wonderful.png");
            poorTex = Helper.ModContent.Load<Texture2D>("assets/poor.png");

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Cask), nameof(Cask.performObjectDropInAction))]
    public static class CaskObjectDropInPatch
    {
        public static IEnumerable<CodeInstruction> Transpile(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4_4)
                {
                    insn.opcode = OpCodes.Ldc_I4_6;
                }
                ret.Add(insn);
            }
            return ret;
        }
    }

    [HarmonyPatch(typeof(Cask), nameof(Cask.checkForMaturity))]
    public static class CaskCheckMaturityPatch
    {
        public static IEnumerable<CodeInstruction> Transpile(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4_4)
                {
                    insn.opcode = OpCodes.Ldc_I4_6;
                }
                ret.Add(insn);
            }
            return ret;
        }
    }

    [HarmonyPatch(typeof(Cask), nameof(Cask.GetDaysForQuality))]
    public static class CaskDaysForQualityPatch
    {
        public static bool Prefix(int quality, ref float __result)
        {
            if (quality == 6)
            {
                __result = 14;
                return false;
            }
            if (quality == -2)
            {
                __result = 70;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Cask), nameof(Cask.GetNextQuality))]
    public static class CaskNextQualityPatch
    {
        public static bool Prefix(int quality, ref int __result)
        {
            if (quality == 6)
            {
                __result = 28; // Should this be 14 to match the pattern of vanilla?
                return false;
            }
            if (quality == -2)
            {
                __result = 70;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), "_PopulateContextTags")]
    public static class ObjectContextTagPatch
    {
        public static void Postfix(StardewValley.Object __instance, HashSet<string> tags)
        {
            if (__instance.Quality == 6)
                tags.Add("quality_wonderful");
            else if (__instance.Quality == -2)
                tags.Add("quality_poor");
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.drawInMenu))]
    public static class ObjectDrawMenuQualityPatch
    {
        public static void Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (__instance.Quality != 6 && __instance.Quality != -2)
                return;

            if (drawStackNumber != 0)
            {
                Microsoft.Xna.Framework.Rectangle quality_rect = new( 0, 0, 8, 8 );
                Texture2D quality_sheet = __instance.Quality == 6 ? Mod.wonderfulTex : Mod.poorTex;
                float yOffset = (((int)__instance.quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
                spriteBatch.Draw(quality_sheet, location + new Vector2(12f, 52f + yOffset), quality_rect, color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + yOffset), SpriteEffects.None, layerDepth);
            }
        }
    }

    [HarmonyPatch(typeof(ColoredObject), nameof(ColoredObject.drawInMenu))]
    public static class ColoredObjectDrawMenuQualityPatch
    {
        public static void Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
        {
            if (__instance.Quality != 6 && __instance.Quality != -2)
                return;

            if (drawStackNumber != 0)
            {
                Microsoft.Xna.Framework.Rectangle quality_rect = new(0, 0, 8, 8);
                Texture2D quality_sheet = __instance.Quality == 6 ? Mod.wonderfulTex : Mod.poorTex;
                float yOffset = (((int)__instance.quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
                spriteBatch.Draw(quality_sheet, location + new Vector2(12f, 52f + yOffset), quality_rect, Color.White * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + yOffset), SpriteEffects.None, layerDepth);
            }
        }
    }

    [HarmonyPatch(typeof(Cask), nameof(Cask.draw))]
    public static class CaskDrawQualityPatch
    {
        public static void Postfix(Cask __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (__instance.heldObject.Value != null && (int)__instance.heldObject.Value.quality > 0)
            {
                if (__instance.heldObject.Value.Quality != 6 && __instance.heldObject.Value.Quality != -2)
                    return;

                Vector2 scaleFactor = (((int)__instance.minutesUntilReady > 0) ? new Vector2(Math.Abs(__instance.scale.X - 5f), Math.Abs(__instance.scale.Y - 5f)) : Vector2.Zero);
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X + 32f - 8f - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y + 64f + 8f - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(16f + scaleFactor.X), (int)(16f + scaleFactor.Y / 2f));
                Microsoft.Xna.Framework.Rectangle quality_rect = new(0, 0, 8, 8);
                Texture2D quality_sheet = __instance.Quality == 10 ? Mod.wonderfulTex : Mod.poorTex;
                spriteBatch.Draw(quality_sheet, destination, quality_rect, Color.White * 0.95f, 0f, Vector2.Zero, SpriteEffects.None, (float)((y + 1) * 64) / 10000f);
            }
        }
    }
}
