using CapstoneProfessions.Framework;
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
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;

        public static readonly int ProfessionTime = 1000;
        public static readonly int ProfessionProfit = 1001;

        internal static Texture2D ClockTex;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.Player.Warped += this.OnWarped;

            SpaceEvents.ShowNightEndMenus += this.OnNightMenus;

            Mod.ClockTex = this.Helper.Content.Load<Texture2D>("assets/clock.png");

            HarmonyPatcher.Apply(this,
                new Game1Patcher(),
                new ObjectPatcher()
            );
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && this.Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
            {
                if (e.Player.professions.Contains(Mod.ProfessionTime) && !e.Player.professions.Contains(Mod.ProfessionProfit))
                    e.Player.professions.Add(Mod.ProfessionProfit);
                if (!e.Player.professions.Contains(Mod.ProfessionTime) && e.Player.professions.Contains(Mod.ProfessionProfit))
                    e.Player.professions.Add(Mod.ProfessionTime);
            }
        }

        private void OnNightMenus(object sender, EventArgsShowNightEndMenus e)
        {
            if (Game1.player.farmingLevel.Value == 10 && Game1.player.foragingLevel.Value == 10 &&
                 Game1.player.fishingLevel.Value == 10 && Game1.player.miningLevel.Value == 10 &&
                 Game1.player.combatLevel.Value == 10)
            {
                if (Game1.player.professions.Contains(Mod.ProfessionTime) || Game1.player.professions.Contains(Mod.ProfessionProfit))
                    return;

                Log.Debug("Doing profession menu");

                if (Game1.endOfNightMenus.Count == 0)
                    Game1.endOfNightMenus.Push(new SaveGameMenu());

                Game1.endOfNightMenus.Push(new CapstoneProfessionMenu());
            }
        }
    }
}
