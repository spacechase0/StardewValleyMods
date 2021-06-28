**Generic Mod Config Menu** is a [Stardew Valley](http://stardewvalley.net/) mod which adds an
in-game UI to edit other mods' config options. This only works for mods designed to support it.

![](screenshot.png)

## Install
1. Install the latest version of [SMAPI](https://smapi.io).
2. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/5098).
3. Run the game using SMAPI.

## Use
### For players
You can edit settings from the title screen ([via the cog button](screenshot-title.png)) or in-game
([at the bottom of the in-game options menu](screenshot-in-game-options.png)). This only works for
mods which were designed to support Generic Mod Config Menu, and some mods may only allow editing
their config from the title screen.

Changes are live after saving, no need to restart the game.

### For C# mod authors
You can use [SMAPI's mod API feature](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs)
to add a config UI. Your mod will always work, even if a player doesn't have Generic Mod Config Menu
installed (they just won't see the config UI).

To use it:

1. Copy [`IGenericModConfigMenuApi`](../IGenericModConfigMenuApi.cs) into your code.
2. In the [`GameLaunched` event](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Events#Game_loop),
   call the API and register your config fields.
3. Delete any methods in `IGenericModConfigMenuApi` you're not using (for best compatibility with
   future versions).

For example, this adds a very simple config UI (assuming you [created a config
model](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Config) named `ModConfig` and saved
it to a `Config` field in your entry class):

```c#
private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
{
    // get Generic Mod Config Menu API (if it's installed)
    var api = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
    if (api is null)
        return;

    // register mod configuration
    api.RegisterModConfig(
        mod: this.ModManifest,
        revertToDefault: () => this.Config = new ModConfig(),
        saveToFile: () => this.Helper.WriteConfig(this.Config)
    );

    // let players configure your mod in-game (instead of just from the title screen)
    api.SetDefaultIngameOptinValue(this.ModManifest, true);

    // add some config options
    api.RegisterSimpleOption(
        mod: this.ModManifest,
        optionName: "Example checkbox",
        optionDesc: "An optional description shown as a tooltip to the player.",
        optionGet: () => this.Config.ExampleCheckbox,
        optionSet: value => this.Config.ExampleCheckbox = value
    );
    api.RegisterSimpleOption(
        mod: this.ModManifest,
        optionName: "Example string",
        optionDesc: "...",
        optionGet: () => this.Config.ExampleString,
        optionSet: value => this.Config.ExampleString = value
    );
    api.RegisterChoiceOption(
        mod: this.ModManifest,
        optionName: "Example dropdown",
        optionDesc: "...",
        optionGet: () => this.Config.ExampleDropdown,
        optionSet: value => this.Config.ExampleDropdown = value,
        choices: new string[] { "choice A", "choice B", "choice C" }
    );
}
```

See the `IGenericModConfigMenuApi` for more options.

### For Content Patcher pack authors
You don't need to do anything! Content Patcher will add the config UI automatically for you.

## Compatibility
Compatible with Stardew Valley 1.5+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
