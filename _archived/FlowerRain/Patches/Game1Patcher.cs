using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using FlowerRain.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace FlowerRain.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>The flower metadata for each season.</summary>
        private static IDictionary<string, List<FlowerData>> FlowerData;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="flowerData">The flower metadata for each season.</param>
        public Game1Patcher(IDictionary<string, List<FlowerData>> flowerData)
        {
            Game1Patcher.FlowerData = flowerData;
        }

        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
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
        public static IEnumerable<CodeInstruction> Transpile_UpdateWeather(IEnumerable<CodeInstruction> instructions)
        {
            var result = new List<CodeInstruction>();

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == -16)
                    instruction.operand = (sbyte)-8;
                else if (instruction.opcode == OpCodes.Ldc_I4_S && (sbyte)instruction.operand == 32)
                    instruction.operand = (sbyte)16;

                result.Add(instruction);
            }

            return result;
        }

        /// <summary>The method to call before <see cref="Game1.drawWeather"/>.</summary>
        /// <remarks>This draws flowers.</remarks>
        public static void After_DrawWeather(Game1 __instance)
        {
            // get location
            GameLocation location = Game1.currentLocation;
            if (location?.IsOutdoors != true || location is Desert || !Game1.IsRainingHere(location))
                return;

            // get flower data
            string season = Game1.GetSeasonForLocation(location);
            var curFlowers = Game1Patcher.FlowerData[season];
            if (curFlowers.Count == 0)
                return;

            // draw flowers
            Random rand = new Random(0);
            if (__instance.takingMapScreenshot)
            {
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    Vector2 position = new Vector2(Game1.random.Next(Game1.viewport.Width - Game1.tileSize), Game1.random.Next(Game1.viewport.Height - Game1.tileSize));
                    var drop = Game1.rainDrops[index];
                    var flower = curFlowers[index % curFlowers.Count];
                    float rotation = (float)rand.NextDouble() * 3.14f;
                    float scale = 2f - drop.frame * 0.5f;
                    if (scale <= 0)
                        continue;

                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, position, Game1Patcher.GetRect(flower.Index + 1), flower.Color, rotation, new Vector2(8, 8), scale, SpriteEffects.None, 1);
                }
            }
            else
            {
                if (Game1.eventUp && !Game1.currentLocation.isTileOnMap(new Vector2(Game1.viewport.X / Game1.tileSize, Game1.viewport.Y / Game1.tileSize)))
                    return;
                for (int index = 0; index < Game1.rainDrops.Length; ++index)
                {
                    var drop = Game1.rainDrops[index];
                    var flower = curFlowers[index % curFlowers.Count];
                    float rotation = (float)rand.NextDouble() * 3.14f;
                    float scale = 2f - drop.frame * 0.5f;
                    if (scale <= 0)
                        continue;

                    //Game1.spriteBatch.Draw(Game1.objectSpriteSheet, rd.position, getRect(fd.index), Color.White, r, new Vector2(8, 8), s, SpriteEffects.None, 1);
                    Game1.spriteBatch.Draw(Game1.objectSpriteSheet, drop.position, Game1Patcher.GetRect(flower.Index + 1), flower.Color, rotation, new Vector2(8, 8), scale, SpriteEffects.None, 1);
                }
            }
        }

        /// <summary>Get the source rectangle for a flower.</summary>
        /// <param name="index">The flower index.</param>
        private static Rectangle GetRect(int index)
        {
            return new(index % 24 * 16, index / 24 * 16, 16, 16);
        }
    }
}
