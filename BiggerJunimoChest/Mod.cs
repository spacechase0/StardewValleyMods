using BiggerJunimoChest.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;

namespace BiggerJunimoChest
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            HarmonyPatcher.Apply(this,
                new ChestPatcher()
            );
        }
    }
}
