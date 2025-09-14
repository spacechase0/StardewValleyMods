using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Game1"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class Game1Patcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.eventFinished)),
                prefix: this.GetHarmonyMethod(nameof(Before_EventFinished))
            );

            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.loadForNewGame)),
                postfix: this.GetHarmonyMethod(nameof(After_LoadForNewGame))
            );

            if (this.TryGetEndOfNightMethod(monitor, out MethodInfo endOfNightMethod))
            {
                harmony.Patch(
                    original: endOfNightMethod,
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_ShowEndOfNightStuff))
                );
            }

            harmony.Patch(
                original: Constants.TargetPlatform != GamePlatform.Android
                    ? this.RequireMethod<Game1>(nameof(Game1.warpFarmer), new[] { typeof(LocationRequest), typeof(int), typeof(int), typeof(int) })
                    : this.RequireMethod<Game1>(nameof(Game1.warpFarmer), new[] { typeof(LocationRequest), typeof(int), typeof(int), typeof(int), typeof(bool)}),
                prefix: this.GetHarmonyMethod(nameof(Before_WarpFarmer))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Game1.eventFinished"/>.</summary>
        private static void Before_EventFinished()
        {
            if (Game1.CurrentEvent != null)
                SpaceEvents.InvokeOnEventFinished();
        }

        /// <summary>The method to call after <see cref="Game1.loadForNewGame"/>.</summary>
        private static void After_LoadForNewGame(bool loadedGame)
        {
            SpaceEvents.InvokeOnBlankSave();
        }

        /// <summary>The method which transpiles <see cref="Game1.showEndOfNightStuff"/>.</summary>
        private static IEnumerable<CodeInstruction> Transpile_ShowEndOfNightStuff(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            // TODO: Learn how to use ILGenerator

            FieldInfo farmingLevel = typeof(Farmer).GetField("farmingLevel");

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.LoadsField(farmingLevel))
                {
                    newInsns.Insert(newInsns.Count - 2, new CodeInstruction(OpCodes.Call, PatchHelper.RequireMethod<Game1Patcher>(nameof(ShowEndOfNightStuffLogic))));
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method to call before <see cref="Game1.warpFarmer(LocationRequest,int,int,int)"/>.</summary>
        private static bool Before_WarpFarmer(ref LocationRequest locationRequest, ref int tileX, ref int tileY, ref int facingDirectionAfterWarp)
        {
            return !SpaceEvents.InvokeBeforeWarp(ref locationRequest, ref tileX, ref tileY, ref facingDirectionAfterWarp);
        }

        private static void ShowEndOfNightStuffLogic()
        {
            var ev = new EventArgsShowNightEndMenus();
            SpaceEvents.InvokeShowNightEndMenus(ev);
        }

        /// <summary>Get the <see cref="Game1.showEndOfNightStuff"/> method with support for both Linux/macOS and Windows.</summary>
        /// <param name="monitor">The monitor with which to log any errors.</param>
        /// <param name="method">The method reference, if found.</param>
        /// <returns>Returns whether the method was successfully found.</returns>
        private bool TryGetEndOfNightMethod(IMonitor monitor, out MethodInfo method)
        {
            try
            {
                Type game1CompilerType = null;
                foreach (var t in typeof(Game1).Assembly.GetTypes())
                {
                    if (t.FullName == "StardewValley.Game1+<>c")
                        game1CompilerType = t;
                }

                foreach (var m in game1CompilerType.GetRuntimeMethods())
                {
                    if (m.FullDescription().Contains(nameof(Game1.showEndOfNightStuff)))
                    {
                        method = m;
                        return true;
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                monitor.Log($"Weird exception doing finding Windows {nameof(Game1.showEndOfNightStuff)}: {ex}", LogLevel.Error);
                foreach (var le in ex.LoaderExceptions)
                    monitor.Log($"LE: {le}", LogLevel.Error);

                method = null;
                return false;
            }
            catch (Exception ex)
            {
                monitor.Log($"Failed to find Windows {nameof(Game1.showEndOfNightStuff)} lambda: {ex}");
                try
                {
                    Type game1CompilerType = typeof(Game1);
                    foreach (var m in game1CompilerType.GetRuntimeMethods())
                    {
                        if (m.FullDescription().Contains($"<{nameof(Game1.showEndOfNightStuff)}>m__"))
                        {
                            method = m;
                            return true;
                        }
                    }
                }
                catch (Exception e2)
                {
                    monitor.Log($"Failed to find Mac/Linux {nameof(Game1.showEndOfNightStuff)} lambda: {e2}", LogLevel.Error);

                    method = null;
                    return false;
                }
            }

            monitor.Log($"Failed to locate {nameof(Game1.showEndOfNightStuff)} method for patching.", LogLevel.Error);
            method = null;
            return false;
        }
    }
}
