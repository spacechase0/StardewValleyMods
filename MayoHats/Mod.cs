using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace MayoHats
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static IJsonAssetsApi ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "json-assets"));
        }
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
    public static class InventoryPageHatPatch
    {
        public static void Prefix(InventoryPage __instance, int x, int y, bool playSound)
        {
            var c = __instance.equipmentIcons.First(c => c.myID == 101);
            if (!c.containsPoint(x, y))
            {
                return;
            }

            if (Game1.player.CursorSlotItem is StardewValley.Object obj && obj.Stack == 1)
            {
                if (obj.ParentSheetIndex == 306)
                    Game1.player.CursorSlotItem = new Hat("Mayo");
                if (obj.ParentSheetIndex == 307)
                    Game1.player.CursorSlotItem = new Hat("Duck Mayo");
                if (obj.ParentSheetIndex == 308)
                    Game1.player.CursorSlotItem = new Hat("Void Mayo");
                if (obj.ParentSheetIndex == 807)
                    Game1.player.CursorSlotItem = new Hat("Dino Mayo");
            }
        }
    }
}
