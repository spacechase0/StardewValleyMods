using ExtendedReach.Framework;
using ExtendedReach.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ExtendedReach
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private Configuration Config;

        /// <summary>Handles the logic for rendering wiggly arms.</summary>
        private WigglyArmsRenderer WigglyArmsRenderer;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            this.Config = helper.ReadConfig<Configuration>();
            this.WigglyArmsRenderer = new(helper.Input, helper.Reflection);

            helper.Events.Display.RenderedWorld += this.OnRenderWorld;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            HarmonyPatcher.Apply(this,
                new TileRadiusPatcher()
            );
        }


        /*********
        ** Private methods
        *********/
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                configMenu.RegisterModConfig(
                    mod: this.ModManifest,
                    revertToDefault: () => this.Config = new Configuration(),
                    saveToFile: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.RegisterSimpleOption(
                    mod: this.ModManifest,
                    optionName: "Wiggly Arms",
                    optionDesc: "Show wiggly arms reaching out to your cursor.",
                    optionGet: () => this.Config.WigglyArms,
                    optionSet: value => this.Config.WigglyArms = value
                );
            }
        }

        private void OnRenderWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Context.IsPlayerFree && this.Config.WigglyArms)
                this.WigglyArmsRenderer.Render(e.SpriteBatch);
        }
    }
}
