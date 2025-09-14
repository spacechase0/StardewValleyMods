using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using DynamicGameAssets.Framework;
using DynamicGameAssets.Game;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Farmer"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FarmerPatcher : BasePatcher
    {

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.getItemCountInList)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetItemCountInList))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.removeItemsFromInventory)),
                prefix: this.GetHarmonyMethod(nameof(Before_RemoveItemsFromInventory))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.eatObject)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_EatObject))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.doneEating)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_DoneEating))
            );
            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.showEatingItem)),
                postfix: this.GetHarmonyMethod(nameof(Postfix_showEatingItem))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Farmer.getItemCountInList"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_GetItemCountInList(Farmer __instance, IList<Item> list, int item_index,
            int min_price, ref int __result)
        {
            if (Mod.itemLookup.ContainsKey(item_index))
            {
                __result = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    var item = list[i];
                    if (item is CustomObject obj && obj.FullId.GetDeterministicHashCode() == item_index)
                        __result += obj.Stack;
                }

                return false;
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Farmer.removeItemsFromInventory"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_RemoveItemsFromInventory(Farmer __instance, int index, int stack, ref bool __result)
        {
            if (Mod.itemLookup.ContainsKey(index))
            {
                if (__instance.hasItemInInventory(index, stack))
                {
                    for (int i = 0; i < __instance.items.Count; ++i)
                    {
                        var item = __instance.items[i];
                        if (item is not CustomObject obj || obj.FullId.GetDeterministicHashCode() != index)
                            continue;

                        if (item.Stack > stack)
                        {
                            item.Stack -= stack;
                            break;
                        }
                        else
                        {
                            stack -= item.Stack;
                            __instance.items[i] = null;

                            if (stack == 0)
                                break;
                        }
                    }

                    __result = true;
                }
                else
                {
                    __result = false;
                }

                return false;
            }

            return true;
        }

        /// <summary>The method which transpiles <see cref="Farmer.eatObject"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_EatObject(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler2(gen, original, instructions);
        }

        /// <summary>The method which transpiles <see cref="Farmer.doneEating"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_DoneEating(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            return PatchCommon.RedirectForFakeObjectInformationTranspiler2(gen, original, instructions);
        }

        /// <summary>
        /// Postfixes <see cref="Farmer.showEatingItem(Farmer)" /> to add back in the TAS for DGA.
        /// </summary>
        /// <param name="who">Farmer.</param>
        private static void Postfix_showEatingItem(Farmer who)
        {
            if (who.itemToEat is CustomObject obj)
            {
                PackData.TextureAnimationFrame animationFrame = obj.Data.pack.GetTextureFrame(obj.Data.Texture);
                TexturedRect tex = obj.Data.pack.GetTexture(obj.Data.Texture, 16, 16);
                string textureName = obj.Data.pack.smapiPack.GetActualAssetKey(animationFrame.FilePath);

                if (tex.Rect is null)
                {
                    return;
                }

                switch (who.FarmerSprite.currentAnimationIndex)
                {
                    case 1:
                    {
                        TemporaryAnimatedSprite tas = new(
                            textureName: textureName,
                            sourceRect: tex.Rect.Value,
                            animationInterval: 254f,
                            animationLength: 1,
                            numberOfLoops: 0,
                            position: who.Position + new Vector2(-21f, -112f),
                            flicker: false,
                            flipped: false,
                            layerDepth: who.getStandingY() / 10000f + 0.01f,
                            alphaFade: 0f,
                            color: Color.White,
                            scale: Game1.pixelZoom,
                            scaleChange: 0f,
                            rotation: 0f,
                            rotationChange: 0f);
                        who.currentLocation.temporarySprites.Add(tas);
                        return;
                    }
                    case 2:
                    {
                        TemporaryAnimatedSprite tas = new(
                            textureName: textureName,
                            sourceRect: tex.Rect.Value,
                            animationInterval: 650f,
                            animationLength: 1,
                            numberOfLoops: 0,
                            position: who.Position + new Vector2(-21f, -108f),
                            flicker: false,
                            flipped: false,
                            layerDepth: who.getStandingY() / 10000f + 0.01f,
                            alphaFade: 0f,
                            color: Color.White,
                            scale: Game1.pixelZoom,
                            scaleChange: -0.01f,
                            rotation: 0f,
                            rotationChange: 0f)
                        {
                            motion = new Vector2(0.8f, -11f),
                            acceleration = new Vector2(0f, 0.5f)
                        };
                        who.currentLocation.temporarySprites.Add(tas);
                        return;
                    }
                    case 4:
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Rectangle r = tex.Rect.Value;
                            r.X += 8;
                            r.Y += 8;
                            r.Width = 4;
                            r.Height = 4;
                            TemporaryAnimatedSprite tas = new(
                                textureName: textureName,
                                sourceRect: r,
                                animationInterval: 400f,
                                animationLength: 1,
                                numberOfLoops: 0,
                                position: who.Position + new Vector2(24f, -48f),
                                flicker: false,
                                flipped: false,
                                layerDepth: who.getStandingY() / 10000f + 0.01f,
                                alphaFade: 0f,
                                color: Color.White,
                                scale: Game1.pixelZoom,
                                scaleChange: 0f,
                                rotation: 0f,
                                rotationChange: 0f)
                            {
                                motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-6, -3)),
                                acceleration = new Vector2(0f, 0.5f)
                            };
                            who.currentLocation.temporarySprites.Add(tas);
                        }
                        return;
                    }
                    default:
                        return;
                }
            }
        }
    }
}
