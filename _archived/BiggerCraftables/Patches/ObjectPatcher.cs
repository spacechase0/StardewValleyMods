using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BiggerCraftables.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace BiggerCraftables.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.placementAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PlacementAction))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw1))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: this.GetHarmonyMethod(nameof(Before_Draw2))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.drawWhenHeld)),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawWhenHeld))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawInMenu))
            );

            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.drawPlacementBounds)),
                prefix: this.GetHarmonyMethod(nameof(Before_DrawPlacementBounds))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="SObject.placementAction"/>.</summary>
        private static bool Before_PlacementAction(SObject __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;

            string placementSound = "hammer";

            Vector2 baseIndex = new Vector2(x / 64, y / 64);
            __instance.setHealth(10);
            __instance.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

            for (int ix = 0; ix < entry.Width; ++ix)
            {
                for (int iy = 0; iy < entry.Length; ++iy)
                {
                    Vector2 index = baseIndex + new Vector2(ix, iy);
                    if (location.objects.ContainsKey(index))
                    {
                        __result = false;
                        return false;
                    }
                }
            }

            for (int ix = 0; ix < entry.Width; ++ix)
            {
                for (int iy = 0; iy < entry.Length; ++iy)
                {
                    int iOffset = ix + (entry.Length - 1 - iy) * entry.Width;
                    Vector2 index = baseIndex + new Vector2(ix, iy);
                    var obj = new SObject(index, __instance.ParentSheetIndex);
                    obj.SetBiggerIndex(iOffset);
                    location.objects.Add(index, obj);
                }
            }

            location.playSound(placementSound);

            __result = true;
            return false;
        }

        /// <summary>The method to call before <see cref="SObject.draw(SpriteBatch,int,int,float,float)"/>.</summary>
        private static bool Before_Draw1(SObject __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;
            int hdiff = entry.Texture.Height - entry.Length * 16;

            if (__instance.GetBiggerIndex() == 0)
            {
                if (__instance.isTemporarilyInvisible)
                    return false;

                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
                spriteBatch.Draw(entry.Texture, pos - new Vector2(0, hdiff * 4 * 2) + scaleFactor / 2f + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), null, Color.White * alpha, 0, Vector2.Zero, 4, SpriteEffects.None, layerDepth);
            }
            return false;
        }

        /// <summary>The method to call before <see cref="SObject.draw(SpriteBatch,int,int,float)"/>.</summary>
        private static bool Before_Draw2(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;
            int hdiff = entry.Texture.Height;

            if (__instance.GetBiggerIndex() == 0)
            {
                if (__instance.isTemporarilyInvisible)
                    return false;

                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64));
                float drawLayer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;
                spriteBatch.Draw(entry.Texture, pos - new Vector2(0, hdiff * 4 - 64) + scaleFactor / 2f + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), null, Color.White * alpha, 0, Vector2.Zero, 4, SpriteEffects.None, drawLayer);
            }
            return false;
        }

        /// <summary>The method to call before <see cref="SObject.drawWhenHeld"/>.</summary>
        private static bool Before_DrawWhenHeld(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;

            spriteBatch.Draw(entry.Texture, objectPosition + new Vector2(32, 32), null, Color.White, 0, new Vector2(entry.Texture.Width / 2, entry.Texture.Height / 2), 4, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 3) / 10000f));

            return false;
        }

        /// <summary>The method to call before <see cref="SObject.drawInMenu(SpriteBatch,Vector2,float,float,float,StackDrawType,Color,bool)"/>.</summary>
        private static bool Before_DrawInMenu(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;

            if (__instance.IsRecipe)
            {
                transparency = 0.5f;
                scaleSize *= 0.75f;
            }
            bool flag = (drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1 || drawStackNumber == StackDrawType.Draw_OneInclusive) && scaleSize > 0.3 && __instance.Stack != int.MaxValue;
            if (__instance.IsRecipe)
                flag = false;

            float scale = Math.Min(4f * 16 / entry.Texture.Width, 4f * 16 / entry.Texture.Height);
            spriteBatch.Draw(entry.Texture, location + new Vector2(32, 32), null, color * transparency, 0, new Vector2(entry.Texture.Width / 2, entry.Texture.Height / 2), scale, SpriteEffects.None, layerDepth);
            if (flag)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) + 3f * scaleSize, (float)(64.0 - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, color);

            if (!__instance.IsRecipe)
                return false;
            spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), color, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);

            return false;
        }

        private static bool Before_DrawPlacementBounds(SObject __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (!__instance.bigCraftable.Value)
                return true;
            if (!Mod.Entries.TryGetValue(__instance.Name, out Entry entry))
                return true;

            int x = (int)Game1.GetPlacementGrabTile().X * 64;
            int y = (int)Game1.GetPlacementGrabTile().Y * 64;
            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            if (Game1.isCheckingNonMousePlacement)
            {
                Vector2 placementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
                x = (int)placementPosition.X;
                y = (int)placementPosition.Y;
            }
            bool flag = Utility.playerCanPlaceItemHere(location, __instance, x, y, Game1.player) || Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y) && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
            Game1.isCheckingNonMousePlacement = false;
            for (int ix = 0; ix < entry.Width; ++ix)
            {
                for (int iy = 0; iy < entry.Length; ++iy)
                {
                    spriteBatch.Draw(Game1.mouseCursors, new Vector2(x / 64 * 64 + ix * 64 - Game1.viewport.X, y / 64 * 64 + iy * 64 - Game1.viewport.Y), new Rectangle(flag ? 194 : 210, 388, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                    if (ix == 0 && iy == entry.Length - 1)
                        __instance.draw(spriteBatch, x / 64 + ix, y / 64 + iy, 0.5f);
                }
            }

            return false;
        }
    }
}
