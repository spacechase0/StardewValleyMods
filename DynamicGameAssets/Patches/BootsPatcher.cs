using DynamicGameAssets.Game;
using HarmonyLib;
using StardewValley.Objects;

namespace DynamicGameAssets.Patches
{
    [HarmonyPatch(typeof(Boots), "loadDisplayFields")]
    public static class BootsLoadDisplayFieldsPatch
    {
        public static bool Prefix(Boots __instance, ref bool __result)
        {
            if (__instance is CustomBoots cb)
            {
                __result = cb.LoadDisplayFields();
                return false;
            }
            return true;
        }
    }
}
