using System;
using CombatOverhaulMod.Buffs;
using CombatOverhaulMod.Combat;
using CombatOverhaulMod.Elements;
using CombatOverhaulMod.FightStamina;
using HarmonyLib;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatOverhaulMod
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Configuration Config { get; set; }
        internal ISpaceCoreApi SpaceCore;

        internal event EventHandler Ready;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            Config = Helper.ReadConfig<Configuration>();
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;

            new ElementsEngine();
            new BuffEngine();
            new CombatEngine();
            new FightStaminaEngine();

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            SpaceCore = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");

            // TODO: GMCM

            Ready?.Invoke(this, new());
        }
    }
}
