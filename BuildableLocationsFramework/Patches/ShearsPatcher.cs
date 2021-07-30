using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace BuildableLocationsFramework.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Shears"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ShearsPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Shears>(nameof(Shears.beginUsing)),
                prefix: this.GetHarmonyMethod(nameof(Before_BeginUsing))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="Shears.beginUsing"/>.</summary>
        private static bool Before_BeginUsing(Shears __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            x = (int)who.GetToolLocation().X;
            y = (int)who.GetToolLocation().Y;
            Rectangle toolRect = new Rectangle(x - 32, y - 32, 64, 64);
            if (location is IAnimalLocation animalLoc)
                Mod.Instance.Helper.Reflection.GetField<FarmAnimal>(__instance, "animal").SetValue(Utility.GetBestHarvestableFarmAnimal(animalLoc.Animals.Values, __instance, toolRect));
            who.Halt();
            int currentFrame = who.FarmerSprite.CurrentFrame;
            who.FarmerSprite.animateOnce(283 + who.FacingDirection, 50f, 4);
            who.FarmerSprite.oldFrame = currentFrame;
            who.UsingTool = true;
            who.CanMove = false;
            __result = true;
            return false;
        }
    }
}
