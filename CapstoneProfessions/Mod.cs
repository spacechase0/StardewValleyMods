using System.Diagnostics.CodeAnalysis;
using CapstoneProfessions.Framework;
using CapstoneProfessions.Patches;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
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
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;


            SpaceEvents.ShowNightEndMenus += this.OnNightMenus;

            Mod.ClockTex = this.Helper.ModContent.Load<Texture2D>("assets/clock.png");

            HarmonyPatcher.Apply(this,
                new Game1Patcher(),
                new ObjectPatcher()
            );
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (this.Helper.ModRegistry.IsLoaded("cantorsdust.AllProfessions"))
                this.Helper.Events.Player.Warped += this.OnWarped;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.Player.professions.Contains(Mod.ProfessionTime) && !e.Player.professions.Contains(Mod.ProfessionProfit))
                    e.Player.professions.Add(Mod.ProfessionProfit);
                if (!e.Player.professions.Contains(Mod.ProfessionTime) && e.Player.professions.Contains(Mod.ProfessionProfit))
                    e.Player.professions.Add(Mod.ProfessionTime);
            }
        }

        [SuppressMessage("Reliability", "CA2000", Justification = DiagnosticMessages.DisposableOutlivesScope)]
        private void OnNightMenus(object sender, EventArgsShowNightEndMenus e)
        {
            if (!this.HasMaxedSkills() || Game1.player.professions.Contains(Mod.ProfessionTime) || Game1.player.professions.Contains(Mod.ProfessionProfit))
                return;

            Log.Debug("Doing profession menu");

            if (Game1.endOfNightMenus.Count == 0)
                Game1.endOfNightMenus.Push(new SaveGameMenu());

            Game1.endOfNightMenus.Push(new CapstoneProfessionMenu());
        }

        /// <summary>Get whether the player has maxed out all their skills.</summary>
        private bool HasMaxedSkills()
        {
            return
                Game1.player.farmingLevel.Value >= 10
                && Game1.player.foragingLevel.Value >= 10
                && Game1.player.fishingLevel.Value >= 10
                && Game1.player.miningLevel.Value >= 10
                && Game1.player.combatLevel.Value >= 10;
        }
    }
}
