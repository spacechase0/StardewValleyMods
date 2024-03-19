using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JsonAssets.Data;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace JsonAssets.Patches
{
    // TODO when custom fence support is added
    // Remember to also uncomment patcher instance creation in Mod.cs
#if false
    /// <summary>Applies Harmony patches to <see cref="Fence"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class FencePatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireConstructor<Fence>(typeof(Vector2), typeof(int), typeof(bool)),
                postfix: this.GetHarmonyMethod(nameof(After_Constructor))
            );

            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.repair)),
                prefix: this.GetHarmonyMethod(nameof(Before_Repair))
            );

            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.dropItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_DropItem))
            );

            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.performToolAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformToolAction))
            );

            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.performObjectDropInAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformObjectDropInAction))
            );

            harmony.Patch(
                original: this.RequireMethod<Fence>(nameof(Fence.CanRepairWithThisItem)),
                prefix: this.GetHarmonyMethod(nameof(Before_CanRepairWithThisItem))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after the <see cref="Fence"/> constructor.</summary>
        private static void After_Constructor(Fence __instance, Vector2 tileLocation, int whichType, bool isGate)
        {
            foreach (var fence in Mod.instance.Fences)
            {
                if (whichType == fence.CorrespondingObject.GetObjectId())
                {
                    __instance.health.Value = (float)(fence.MaxHealth + Game1.random.Next(-100, 101) / 100.0);
                    __instance.name = fence.Name;
                    __instance.ParentSheetIndex = -whichType;

                    __instance.health.Value *= 2;
                    __instance.maxHealth.Value = __instance.health.Value;
                    return;
                }
            }
        }

        /// <summary>The method to call before <see cref="Fence.repair"/>.</summary>
        private static bool Before_Repair(Fence __instance)
        {
            foreach (var fence in Mod.instance.Fences)
            {
                if (__instance.whichType.Value == fence.CorrespondingObject.GetObjectId())
                {
                    __instance.health.Value = (float)(fence.MaxHealth + Game1.random.Next(-100, 101) / 100.0);
                    __instance.name = fence.Name;
                    __instance.ParentSheetIndex = -__instance.whichType.Value;
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Fence.dropItem"/>.</summary>
        private static bool Before_DropItem(Fence __instance, GameLocation location, Vector2 origin, Vector2 destination)
        {
            if (__instance.isGate.Value)
                return true;

            foreach (var fence in Mod.instance.Fences)
            {
                if (__instance.whichType.Value == fence.CorrespondingObject.GetObjectId())
                {
                    location.debris.Add(new Debris(fence.CorrespondingObject.GetObjectId(), origin, destination));
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Fence.performToolAction"/>.</summary>
        private static bool Before_PerformToolAction(Fence __instance, Tool t, GameLocation location, ref bool __result)
        {
            if (__instance.heldObject.Value != null && t is not (null or MeleeWeapon) && t.isHeavyHitter())
                return true;
            else if (__instance.isGate.Value && t is Axe or Pickaxe)
                return true;

            foreach (var fence in Mod.instance.Fences)
            {
                if (__instance.whichType.Value == fence.CorrespondingObject.GetObjectId())
                {
                    __result = false;

                    if (fence.BreakTool == FenceBreakToolType.Pickaxe && t is Pickaxe ||
                         fence.BreakTool == FenceBreakToolType.Axe && t is Axe)
                    {
                    }
                    else return false;

                    location.playSound(t is Axe ? "axchop" : "hammer");
                    location.objects.Remove(__instance.TileLocation);
                    for (int i = 0; i < 4; ++i)
                    {
                        location.temporarySprites.Add(new CosmeticDebris(
                            __instance.fenceTexture.Value,
                            new Vector2(__instance.TileLocation.X * 64 + 32, __instance.TileLocation.Y * 64 + 32),
                            Game1.random.Next(-5, 5) / 100f,
                            Game1.random.Next(-64, 64) / 30f,
                            Game1.random.Next(-800, -100) / 100f,
                            (int)((__instance.TileLocation.Y + 1) * 64),
                            new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8),
                            Color.White,
                            Game1.soundBank != null ? Game1.soundBank.GetCue("shiny4") : null,
                            null,
                            0,
                            200
                        ));
                    }
                    Game1.createRadialDebris(location, t is Axe ? 12 : 14, (int)__instance.TileLocation.X, (int)__instance.TileLocation.Y, 6, false);
                    Mod.instance.Helper.Reflection
                        .GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue()
                        .broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(__instance.TileLocation.X * 64, __instance.TileLocation.Y * 64), Color.White, 8, Game1.random.NextDouble() < 0.5, 50));

                    if (__instance.maxHealth.Value - __instance.health.Value < 0.5)
                        location.debris.Add(new Debris(new SObject(fence.CorrespondingObject.GetObjectId(), 1), __instance.TileLocation * 64 + new Vector2(32, 32)));
                    return false;
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Fence.performObjectDropInAction"/>.</summary>
        private static bool Before_PerformObjectDropInAction(Fence __instance, Item dropIn, bool probe, Farmer who, ref bool __result)
        {
            if (__instance.health.Value > 1 || !__instance.CanRepairWithThisItem(dropIn))
                return true;

            foreach (var fence in Mod.instance.Fences)
            {
                if (__instance.whichType.Value == fence.CorrespondingObject.GetObjectId())
                {
                    if (probe)
                    {
                        __result = true;
                        return false;
                    }

                    if (dropIn.ParentSheetIndex == fence.CorrespondingObject.GetObjectId())
                    {
                        __instance.health.Value = fence.MaxHealth + Game1.random.Next(-1000, 1000) / 100f; // Technically I should add a field to the json to make this changeable, but meh.
                        who.currentLocation.playSound(fence.RepairSound);
                        __result = true;
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>The method to call before <see cref="Fence.CanRepairWithThisItem"/>.</summary>
        private static bool Before_CanRepairWithThisItem(Fence __instance, Item item, ref bool __result)
        {
            if (__instance.health.Value > 1 || item is not SObject)
                return true;

            foreach (var fence in Mod.instance.Fences)
            {
                if (__instance.whichType.Value == fence.CorrespondingObject.GetObjectId())
                {
                    __result = Utility.IsNormalObjectAtParentSheetIndex(item, fence.CorrespondingObject.GetObjectId());
                    return false;
                }
            }

            return true;
        }
    }
#endif
}
