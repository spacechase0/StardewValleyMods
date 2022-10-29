using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace ManaBar.Interactions.GMCM
{
    internal static class Initializer
    {
        public static void InitilizeModMenu(IModHelper helper)
        {
            // Get Generic Mod Config Menu's API (if it's installed).
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
                return;

            // Register mod.
            configMenu.Register(
                mod: Mod.Instance.ModManifest,
                reset: () => Mod.Config = new ModConfig(),
                save: () => helper.WriteConfig(Mod.Config)
            );

            #region Main Setting.

            configMenu.AddSectionTitle(
                mod: Mod.Instance.ModManifest,
                text: () => helper.Translation.Get("main-setting")
            );

            // Render ManaBar?
            configMenu.AddBoolOption(
                mod: Mod.Instance.ModManifest,
                name: () => helper.Translation.Get("render-setting"),
                tooltip: () => helper.Translation.Get("render-setting-des"),
                getValue: () => Mod.Config.RenderManaBar,
                setValue: value => Mod.Config.RenderManaBar = value
            );
            #endregion

            #region Positions Settings.

            configMenu.AddSectionTitle(
                mod: Mod.Instance.ModManifest,
                text: () => helper.Translation.Get("position-settings")
            );

            // X Position Offset.
            configMenu.AddTextOption(
                mod: Mod.Instance.ModManifest,
                name: () => helper.Translation.Get("bar-x-position"),
                tooltip: () => helper.Translation.Get("bar-x-position-des"),
                getValue: () => Mod.Config.XManaBarOffset.ToString(),
                setValue: value =>
                {
                    if (int.TryParse(value, out int result))
                        Mod.Config.XManaBarOffset = result;
                }
            );

            // Y Position Offset.
            configMenu.AddTextOption(
                mod: Mod.Instance.ModManifest,
                name: () => helper.Translation.Get("bar-y-position"),
                tooltip: () => helper.Translation.Get("bar-y-position-des"),
                getValue: () => Mod.Config.YManaBarOffset.ToString(),
                setValue: value =>
                {
                    if (int.TryParse(value, out int result))
                        Mod.Config.YManaBarOffset = result;
                }
            );
            #endregion

            #region Additional Settings.

            configMenu.AddSectionTitle(
                mod: Mod.Instance.ModManifest,
                text: () => helper.Translation.Get("additional-settings")
            );

            // Bar Size Multiplier.
            configMenu.AddNumberOption(
                mod: Mod.Instance.ModManifest,
                name: () => helper.Translation.Get("size-multiplier"),
                tooltip: () =>  helper.Translation.Get("size-multiplier-des"),
                getValue: () => Mod.Config.SizeMultiplier,
                setValue: value => Mod.Config.SizeMultiplier = value,
                interval: 0.5f,
                min: 5f,
                max: 35f
            );
            #endregion
        }
    }
}
