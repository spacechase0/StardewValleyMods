using System.IO;
using Harmony;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace StatueOfGenerosity
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private static JsonAssetsAPI ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += onGameLaunched;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            harmony.Patch(AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DayUpdate)), new HarmonyMethod(this.GetType().GetMethod(nameof(DayUpdatePostfix))));
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ja = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        public static void DayUpdatePostfix(StardewValley.Object __instance, GameLocation location)
        {
            if (!__instance.bigCraftable.Value || __instance.ParentSheetIndex != ja.GetBigCraftableId("Statue of Generosity"))
                return;

            NPC npc = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
            if (npc == null)
                npc = Utility.getRandomTownNPC();

            Game1.NPCGiftTastes.TryGetValue(npc.Name, out string str);
            string[] favs = str.Split('/')[1].Split(' ');

            __instance.MinutesUntilReady = 1;
            __instance.heldObject.Value = new StardewValley.Object(int.Parse(favs[Game1.random.Next(favs.Length)]), 1, false, -1, 0);
        }
    }
}
