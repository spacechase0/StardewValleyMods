using System;
using StardewModdingAPI;
using System.Reflection;
using Harmony;
using StardewValley;

namespace ExtendedReach
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private HarmonyInstance harmony;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            try
            {
                harmony = HarmonyInstance.Create("spacechase0.ExtendedReach");
                doTranspiler(typeof(Utility), nameof(Utility.canGrabSomethingFromHere));
                doTranspiler(typeof(Utility), nameof(Utility.checkForCharacterInteractionAtTile));
                doTranspiler(typeof(Game1), nameof(Game1.pressActionButton));
                doTranspiler(typeof(Game1), nameof(Game1.pressUseToolButton));
                doTranspiler(typeof(Game1), nameof(Game1.tryToCheckAt));
                doTranspiler(typeof(GameLocation), nameof(GameLocation.isActionableTile));
            }
            catch ( Exception e )
            {
                Log.error("Exception patching: ");
                Log.error(e.ToString());
            }
        }

        private void doTranspiler(Type origType, string origMethod)
        {
            doTranspiler(origType.GetMethod(origMethod), typeof(TileRadiusFix).GetMethod(nameof(TileRadiusFix.IncreaseRadiusChecks)));
        }
        private void doTranspiler(MethodInfo orig, MethodInfo transpiler)
        {
            try
            {
                Log.trace($"Doing transpiler patch {orig}:{transpiler}...");
                harmony.Patch(orig, null, null, new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing transpiler patch {orig}:{transpiler}: {e}");
            }
        }
    }
}
