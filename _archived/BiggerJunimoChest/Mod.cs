using BiggerJunimoChest.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;

namespace BiggerJunimoChest
{
    internal class Mod : StardewModdingAPI.Mod
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
