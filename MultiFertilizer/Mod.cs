using MultiFertilizer.Patches;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;

namespace MultiFertilizer
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static string KEY_FERT => $"{Mod.instance.ModManifest.UniqueID}/FertilizerLevel";
        public static string KEY_RETAIN => $"{Mod.instance.ModManifest.UniqueID}/WaterRetainLevel";
        public static string KEY_SPEED => $"{Mod.instance.ModManifest.UniqueID}/SpeedGrowLevel";

        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            HarmonyPatcher.Apply(this,
                new CropPatcher(),
                new GameLocationPatcher(),
                new HoeDirtPatcher(),
                new ObjectPatcher(),
                new UtilityPatcher()
            );
        }
    }
}
