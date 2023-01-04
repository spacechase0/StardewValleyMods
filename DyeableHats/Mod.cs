using System.Collections.Generic;

using HarmonyLib;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceShared;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;

namespace DyeableHats
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = Monitor;

            helper.ConsoleCommands.Add("hat_dye", "hat_dye <color>", OnCommand);

            helper.Events.Content.AssetRequested += (_, e) =>
            {
                if (e.NameWithoutLocale.IsEquivalentTo(this.ModManifest.UniqueID))
                    e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            };

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void OnCommand(string cmd, string[] args)
        {
            var hat = Game1.player.hat.Value;

            var f = Helper.Reflection.GetProperty<Color>(typeof(Color), args[0], false);
            if (f != null)
            {
                if (hat.modData.ContainsKey("color"))
                    hat.modData["color"] = f.GetValue().PackedValue.ToString();
                else
                    hat.modData.Add("color", f.GetValue().PackedValue.ToString());
            }
            else
            {
                if (hat.modData.ContainsKey("color"))
                    hat.modData["color"] = uint.Parse(args[0]).ToString();
                else
                    hat.modData.Add("color", uint.Parse(args[0]).ToString());
            }
        }

        internal static bool GetDataForHat(Hat hat, out Color col, out Texture2D tex)
        {
            if (!hat.modData.ContainsKey("color"))
            {
                col = default;
                tex = default;
                return false;
            }

            var hats = Game1.content.Load<Dictionary<string, string>>(Mod.instance.ModManifest.UniqueID);
            string texPath = null;
            if (hats.ContainsKey(hat.which.Value.ToString()))
                texPath = hats[hat.which.Value.ToString()];
            else if (hats.ContainsKey(hat.Name))
                texPath = hats[hat.Name];
            if (texPath == null)
            {
                col = default;
                tex = default;
                return false;
            }

            tex = Util.FetchTexture(Mod.instance.Helper.ModRegistry, texPath);
            if (tex == Game1.staminaRect)
            {
                Log.Monitor.LogOnce("Hat " + hat.Name + " has invalid color texture path! " + texPath, LogLevel.Error);
                col = default;
                return false;
            }

            col = new Color(uint.Parse(hat.modData["color"]));

            return true;
        }
        }

    [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
    public static class FarmerRendererDrawHatColorPatch
    {
        public static void Postfix(FarmerRenderer __instance,
                                   SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth,
                                   Rectangle ___hatSourceRect, Vector2 ___positionOffset)
        {
            if (who.hat.Value != null && !who.bathingClothes.Value)
            {

                var hat = who.hat.Value;
                if (!Mod.GetDataForHat(hat, out Color col, out Texture2D tex))
                    return;

                var hsr = ___hatSourceRect;
                hsr.X = 0;
                hsr.Y %= 80;

                // Vanilla code
                bool flip = who.FarmerSprite.CurrentAnimationFrame.flip;
                float layer_offset = 3.9E-05f + 1E-06f;
                if (who.hat.Value.isMask && facingDirection == 0)
                {
                    Rectangle mask_draw_rect = hsr;
                    mask_draw_rect.Height -= 11;
                    mask_draw_rect.Y += 11;
                    //b.Draw(tex, position + origin + ___positionOffset + new Vector2(0f, 44f) + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), mask_draw_rect, Color.White, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                    mask_draw_rect = ___hatSourceRect;
                    mask_draw_rect.Height = 11;
                    layer_offset = +1E-06f;
                    b.Draw(tex, position + origin + ___positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), mask_draw_rect, col, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
                else
                {
                    b.Draw(tex, position + origin + ___positionOffset + new Vector2(-8 + ((!flip) ? 1 : (-1)) * FarmerRenderer.featureXOffsetPerFrame[currentFrame] * 4, -16 + FarmerRenderer.featureYOffsetPerFrame[currentFrame] * 4 + ((!who.hat.Value.ignoreHairstyleOffset) ? FarmerRenderer.hairstyleHatOffset[(int)who.hair % 16] : 0) + 4 + (int)__instance.heightOffset), hsr, col, rotation, origin, 4f * scale, SpriteEffects.None, layerDepth + layer_offset);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Hat), nameof(Hat.drawInMenu))]
    public static class HatDrawColorInMenuPatch
    {
        public static void Postfix(Hat __instance,
                                   SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            if (!Mod.GetDataForHat(__instance, out Color col, out Texture2D tex))
                return;

            // Vanilla code
            spriteBatch.Draw(tex, location + new Vector2(32f, 32f), new Rectangle((int)0 * 20 % FarmerRenderer.hatsTexture.Width, (int)0 * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), (col * transparency), 0f, new Vector2(10f, 10f), 4f * scaleSize, SpriteEffects.None, layerDepth);

        }
    }

    // technically another patch but lazy
}
