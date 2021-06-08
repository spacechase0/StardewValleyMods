using CapstoneProfessions.Patches;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CapstoneProfessions
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static readonly int PROFESSION_TIME = 1000;
        public static readonly int PROFESSION_PROFIT = 1001;

        internal static Texture2D clockTex;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.Player.Warped += OnWarped;

            SpaceEvents.ShowNightEndMenus += OnNightMenus;

            clockTex = Helper.Content.Load<Texture2D>("assets/clock.png");

            HarmonyPatcher.Apply(this,
                new Game1Patcher(),
                new ObjectPatcher()
            );
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
            {
                if (e.Player.professions.Contains(PROFESSION_TIME) && !e.Player.professions.Contains(PROFESSION_PROFIT))
                    e.Player.professions.Add(PROFESSION_PROFIT);
                if (!e.Player.professions.Contains(PROFESSION_TIME) && e.Player.professions.Contains(PROFESSION_PROFIT))
                    e.Player.professions.Add(PROFESSION_TIME);
            }
        }

        private void OnNightMenus(object sender, EventArgsShowNightEndMenus e)
        {
            if (Game1.player.farmingLevel.Value == 10 && Game1.player.foragingLevel.Value == 10 &&
                 Game1.player.fishingLevel.Value == 10 && Game1.player.miningLevel.Value == 10 &&
                 Game1.player.combatLevel.Value == 10)
            {
                if (Game1.player.professions.Contains(PROFESSION_TIME) || Game1.player.professions.Contains(PROFESSION_PROFIT))
                    return;

                Log.debug("Doing profession menu");

                if (Game1.endOfNightMenus.Count == 0)
                    Game1.endOfNightMenus.Push(new SaveGameMenu());

                Game1.endOfNightMenus.Push(new CapstoneProfessionMenu());
            }
        }
    }
}
