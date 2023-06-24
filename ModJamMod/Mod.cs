using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ModJamMod
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }
}
