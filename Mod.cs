using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using System.Reflection;
using Harmony;
using StardewValley;
using System.Reflection.Emit;

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
                doTranspiler(typeof(Utility), "canGrabSomethingFromHere");
                doTranspiler(typeof(Utility), "checkForCharacterInteractionAtTile");
                doTranspiler(typeof(Game1), "pressActionButton");
                doTranspiler(typeof(Game1), "pressUseToolButton");
                doTranspiler(typeof(Game1), "tryToCheckAt");
                doTranspiler(typeof(GameLocation), "isActionableTile");
            }
            catch ( Exception e )
            {
                Log.error("Exception patching: ");
                Log.error(e.ToString());
            }
        }

        private void doTranspiler(Type origType, string origMethod)
        {
            doTranspiler(origType.GetMethod(origMethod), typeof(TileRadiusFix).GetMethod("IncreaseRadiusChecks"));
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
