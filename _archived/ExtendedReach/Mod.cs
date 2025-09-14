using ExtendedReach.Framework;
using ExtendedReach.Patches;
using Spacechase.Shared.Patching;
using SpaceShared;
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
            I18n.Init(helper.Translation);
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
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => this.Config = new Configuration(),
                    save: () => this.Helper.WriteConfig(this.Config)
                );
                configMenu.AddBoolOption(
                    mod: this.ModManifest,
                    name: I18n.Config_WigglyArms_Name,
                    tooltip: I18n.Config_WigglyArms_Tooltip,
                    getValue: () => this.Config.WigglyArms,
                    setValue: value => this.Config.WigglyArms = value
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
