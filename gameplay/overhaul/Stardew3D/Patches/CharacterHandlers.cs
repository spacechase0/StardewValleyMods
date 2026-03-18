using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Handlers;
using StardewValley;

namespace Stardew3D.Patches;

internal static class CharacterHandlers
{
    internal static void ManualBootstrap(Harmony harmony)
    {
        foreach (var orig in UpdateTargetMethods())
        {
            Type concreteType = typeof(Impl<>).MakeGenericType(orig.DeclaringType);
            harmony.CreateReversePatcher(orig, new HarmonyMethod(concreteType.GetMethod("OriginalUpdateMethod")) { priority = Priority.VeryLow, reversePatchType = HarmonyReversePatchType.Snapshot });
            harmony.Patch(orig, prefix: new HarmonyMethod(concreteType.GetMethod(orig.DeclaringType == typeof(Farmer) ? "UpdatePrefix_Farmer" : "UpdatePrefix")) { priority = Priority.Last });
        }
#if false
        foreach (var orig in DrawTargetMethods())
        {
            Type concreteType = typeof(Impl<>).MakeGenericType(orig.DeclaringType);
            harmony.CreateReversePatcher(orig, new HarmonyMethod(concreteType.GetMethod($"OriginalDrawMethod_{orig.GetParameters().Length}")) { priority = Priority.VeryLow, reversePatchType = HarmonyReversePatchType.Snapshot });
            harmony.Patch(orig, prefix: new HarmonyMethod(concreteType.GetMethod($"DrawPrefix_{orig.GetParameters().Length}")) { priority = Priority.Last });
        }
#endif
    }
    public static IEnumerable<MethodBase> UpdateTargetMethods()
    {
        var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Steamworks.NET") && !a.IsDynamic)
                         from type in asm.GetExportedTypes()
                         where type.IsSubclassOf(typeof(Character))
                         select type;

        var ps = new Type[] { typeof(GameTime), typeof(GameLocation), typeof(long), typeof(bool) };

        yield return AccessTools.Method(typeof(Character), nameof(Character.update), ps);
        yield return AccessTools.Method(typeof(Farmer), nameof(Farmer.Update));
        foreach (var subclass in subclasses)
        {
            var meth = subclass.GetMethod(nameof(Character.update), ps);
            if (meth != null && meth.DeclaringType == subclass)
                yield return meth;
        }
    }

    public static IEnumerable<MethodBase> DrawTargetMethods()
    {
        var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Steamworks.NET") && !a.IsDynamic)
                         from type in asm.GetExportedTypes()
                         where type.IsSubclassOf(typeof(Character))
                         select type;

        var ps1 = new Type[] { typeof(SpriteBatch) };
        var ps2 = new Type[] { typeof(SpriteBatch), typeof( int ), typeof( float ) };

        yield return AccessTools.Method(typeof(Character), nameof(Character.draw), ps1);
        yield return AccessTools.Method(typeof(Character), nameof(Character.draw), ps2);
        foreach (var subclass in subclasses)
        {
            var meth = subclass.GetMethod(nameof(Character.draw), ps1);
            if (meth != null && meth.DeclaringType == subclass)
                yield return meth;
            meth = subclass.GetMethod(nameof(Character.draw), ps2);
            if (meth != null && meth.DeclaringType == subclass)
                yield return meth;
        }
    }

    internal static class Impl<T>
        where T : Character
    {
        public static bool UpdatePrefix_Farmer(T __instance, GameTime __0, GameLocation __1) => UpdatePrefix(__instance, __0, __1, 0, true);
        public static bool UpdatePrefix(T __instance, GameTime __0, GameLocation __1, long __2, bool __3)
        {

            bool didUpdateOnce = false;
            void forceUpdateIfNotAlreadyRun(IUpdateHandler.UpdateContext ctx)
            {
                if (didUpdateOnce)
                    return;

                OriginalUpdateMethod(__instance, __0, __1, __2, __3);
                didUpdateOnce = true;
            }

            IUpdateHandler.UpdateContext ctx = new()
            {
                Time = __0,

                ForceUpdateIfNotAlreadyRun = forceUpdateIfNotAlreadyRun,
            };

            var currentHandlers = Mod.State.GetUpdateHandlersFor(__instance);
            foreach (var handler in currentHandlers)
            {
                handler?.Update(ctx);
            }

            if (currentHandlers[0] != null)
            {
                return false;
            }

            return true;
        }

        public static void OriginalUpdateMethod(T __instance, GameTime __0, GameLocation __1, long __2, bool __3)
        {
            Log.Error("MEOW! This should never happen");
        }

        public static bool DrawPrefix_1(T __instance, SpriteBatch __0)
        {
            bool didRenderOnce = false;
            void forceRenderIfNotAlreadyRun(IRenderHandler.RenderContext ctx)
            {
                if (didRenderOnce)
                    return;

                OriginalDrawMethod_1(__instance, __0);
                didRenderOnce = true;
            }

            IRenderHandler.RenderContext ctx = new()
            {
                Time = Game1.currentGameTime,
                TargetScreen = Mod.State.ActiveMode?.CurrentTargetScreen,

                WorldBatch = null,
                WorldEnvironment = Mod.State.ActiveMode?.GetCurrentEnvironmentFor( __instance.currentLocation ),
                WorldCamera = Mod.State.ActiveMode?.Camera,
                WorldTransform = Mod.State.ActiveMode?.GetCurrentTransformFor( __instance.currentLocation ) ?? Matrix.Identity,

                ForceRenderIfNotAlreadyRun = forceRenderIfNotAlreadyRun,
            };

            var currentHandlers = Mod.State.GetRenderHandlersFor(__instance);
            foreach (var handler in currentHandlers)
            {
                handler?.Render(ctx);
            }

            if (currentHandlers[0] != null)
            {
                return false;
            }

            return true;
        }

        public static bool DrawPrefix_3(T __instance, SpriteBatch __0, int __1, float __2)
        {
            bool didRenderOnce = false;
            void forceRenderIfNotAlreadyRun(IRenderHandler.RenderContext ctx)
            {
                if (didRenderOnce)
                    return;

                OriginalDrawMethod_3(__instance, __0, __1, __2);
                didRenderOnce = true;
            }

            IRenderHandler.RenderContext ctx = new()
            {
                Time = Game1.currentGameTime,
                TargetScreen = Mod.State.ActiveMode?.CurrentTargetScreen,

                WorldBatch = null,
                WorldEnvironment = Mod.State.ActiveMode?.GetCurrentEnvironmentFor(__instance.currentLocation),
                WorldCamera = Mod.State.ActiveMode?.Camera,
                WorldTransform = Mod.State.ActiveMode?.GetCurrentTransformFor(__instance.currentLocation) ?? Matrix.Identity,

                ForceRenderIfNotAlreadyRun = forceRenderIfNotAlreadyRun,
            };

            var currentHandlers = Mod.State.GetRenderHandlersFor(__instance);
            foreach (var handler in currentHandlers)
            {
                handler?.Render(ctx);
            }

            if (currentHandlers[0] != null)
            {
                return false;
            }

            return true;
        }

        public static void OriginalDrawMethod_1(T __instance, SpriteBatch __0)
        {
            Log.Error("MEOW! This should never happen");
        }

        public static void OriginalDrawMethod_3(T __instance, SpriteBatch __0, int __1, float __2)
        {
            Log.Error("MEOW! This should never happen");
        }
    }
}
