using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FlowerRain.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The flower metadata for each season.</summary>
        private static IDictionary<string, List<Mod.FlowerData>> FlowerData;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="flowerData">The flower metadata for each season.</param>
        public Game1Patcher(IDictionary<string, List<Mod.FlowerData>> flowerData)
        {
            Game1Patcher.FlowerData = flowerData;
        }

        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.updateWeather)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_UpdateWeather))
            );

            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.drawWeather)),
                postfix: this.GetHarmonyMethod(nameof(After_DrawWeather))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Game1.updateWeather"/>.</summary>
        /// <remarks>This slows down rain.</remarks>
        public static IEnumerable<CodeInstruction> Transpile_UpdateWeather(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == -16)
                {
                    insn.operand = (sbyte)-8;
                }
                else if (insn.opcode == OpCodes.Ldc_I4_S && (sbyte)insn.operand == 32)
                {
                    insn.operand = (sbyte)16;
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method to call before <see cref="Game1.updateWeather"/>.</summary>
        /// <remarks>This draws flowers.</remarks>
        public static void After_DrawWeather(Game1 __instance, GameTime time, RenderTarget2D target_screen)
        {
            if (!Game1.isRaining || !Game1.currentLocation.IsOutdoors || (Game1.currentLocation.Name.Equals("Desert") || Game1.currentLocation is Summit))
                return;

            var currFlowers = Game1Patcher.FlowerData[Game1.currentSeason];
            if (currFlowers.Count == 0)
                return;
            Random rand = new Random(0);

            if (__instance.takingMapScreenshot)
            {
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    Vector2 position = new Vector2((float)Game1.random.Next(Game1.viewport.Width - 64), (float)Game1.random.Next(Game1.viewport.Height - 64));
                    var rd = Game1.rainDrops[index];
                    var fd = currFlowers[index % currFlowers.Count];
                    float r = (float)(rand.NextDouble() * 3.14);
                    float s = 2f;
                    s -= rd.frame * 0.5f;
                    if (s <= 0)
                        continue;
                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, getRect(fd.index + 1), fd.color, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                }
            }
            else
            {
                if (Game1.eventUp && !Game1.currentLocation.isTileOnMap(new Vector2((float)(Game1.viewport.X / 64), (float)(Game1.viewport.Y / 64))))
                    return;
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    var rd = Game1.rainDrops[index];
                    var fd = currFlowers[index % currFlowers.Count];
                    float r = (float)(rand.NextDouble() * 3.14);
                    float s = 2f;
                    s -= rd.frame * 0.5f;
                    if (s <= 0)
                        continue;
                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, rd.position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, rd.position, getRect(fd.index + 1), fd.color, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                }
            }
        }

        private static Rectangle getRect(int index)
        {
            return new Rectangle(index % 24 * 16, index / 24 * 16, 16, 16);
        }
    }
}
