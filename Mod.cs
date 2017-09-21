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
        public override void Entry(IModHelper helper)
        {
            base.Entry(helper);
            instance = this;

            try
            {
                Log.debug("Patching...");
                var harmony = HarmonyInstance.Create("spacechase0.ExtendedReach");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.debug("Done!");
            }
            catch ( Exception e )
            {
                Log.error("Exception patching: ");
                Log.error(e.ToString());
            }
        }
    }
}
