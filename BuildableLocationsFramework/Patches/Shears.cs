using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace BuildableLocationsFramework.Patches
{
    [HarmonyPatch(typeof(Shears), nameof(Shears.beginUsing))]
    public static class ShearsBeginUsingFix
    {
        public static bool Prefix(Shears __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            x = (int)who.GetToolLocation(false).X;
            y = (int)who.GetToolLocation(false).Y;
            Rectangle toolRect = new Rectangle(x - 32, y - 32, 64, 64);
            if (location is IAnimalLocation animalLoc)
                Mod.instance.Helper.Reflection.GetField<FarmAnimal>(__instance, "animal").SetValue(Utility.GetBestHarvestableFarmAnimal(animalLoc.Animals.Values, __instance, toolRect));
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
