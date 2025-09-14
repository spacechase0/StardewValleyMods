using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace RaftRestoration.Patches
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
                original: this.RequireMethod<Farmer>(nameof(Farmer.moveRaft)),
                postfix: this.GetHarmonyMethod(nameof(After_MoveRaft))
            );

            harmony.Patch(
                original: this.RequireMethod<Farmer>(nameof(Farmer.draw), new[] { typeof(SpriteBatch) }),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Farmer.moveRaft"/>.</summary>
        public static void After_MoveRaft(Farmer __instance, GameLocation currentLocation, GameTime time)
        {
            __instance.position.X += __instance.xVelocity;
            __instance.position.Y += __instance.yVelocity;
            Rectangle r = new Rectangle((int)(__instance.Position.X), (int)(__instance.Position.Y + 16), 64, 64);

            // 2 - down, 0 - up, 3 - left, 1 - right
            //Log.trace( "meow: " + ( __instance.movementDirections.Count > 0 ?__instance.movementDirections[0]:-1));
            if (__instance.movementDirections.Contains(2) || __instance.movementDirections.Contains(0))
            {
                Vector2 pos = new Vector2(r.X + r.Width / 2, r.Y + r.Height);
                var xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                xTile.ObjectModel.PropertyValue propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        //Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }

                pos = new Vector2(r.X + r.Width / 2, r.Y);
                xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }
            }

            if (__instance.movementDirections.Contains(3))
            {
                Vector2 pos = new Vector2(r.X, r.Y + r.Height / 2);
                var xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                xTile.ObjectModel.PropertyValue propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }

                pos = new Vector2(r.X + r.Width, r.Y + r.Height / 2);
                xPos = new xTile.Dimensions.Location((int)pos.X, (int)pos.Y);
                propVal = null;
                currentLocation.map.GetLayer("Back").PickTile(xPos, Game1.viewport.Size)?.TileIndexProperties.TryGetValue("Water", out propVal);
                if (propVal == null)
                {
                    if (currentLocation.isTileLocationOpen(xPos))
                    {
                        Game1.player.isRafting = false;
                        Game1.player.Position = pos;
                        Game1.player.position.Y -= 64 + 16;
                        Game1.player.setTrajectory(0, 0);
                    }
                    return;
                }
            }
        }

        /// <summary>The method which transpiles <see cref="Farmer.draw(SpriteBatch)"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_Draw(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var ret = new List<CodeInstruction>();
            bool foundIt = false;

            var getSourceRectForStandardTileSheet = PatchHelper.RequireMethod<Game1>(nameof(Game1.getSourceRectForStandardTileSheet));
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo meth && meth == getSourceRectForStandardTileSheet)
                {
                    foundIt = true;
                    ret.RemoveRange(ret.Count - 4, 4);
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 0));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    ret.Add(new CodeInstruction(OpCodes.Ldc_I4, 16));
                    insn.operand = PatchHelper.RequireMethod<FarmerPatcher>(nameof(MakeRectangleHelper));
                }
                if (foundIt && insn.opcode == OpCodes.Ldc_R4 && ((float)insn.operand) == 1f)
                {
                    insn.operand = 4f;
                    foundIt = false;
                }
                ret.Add(insn);
            }
            return ret;
        }

        public static Rectangle MakeRectangleHelper(int x, int y, int w, int h)
        {
            return new(x, y, w, h);
        }
    }
}
