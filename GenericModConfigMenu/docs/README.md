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

### For mod authors
* **For C# mods:** you can use [SMAPI's mod API feature](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Integrations#Mod-provided_APIs)
  to add a config UI. Your mod will still work even if players don't have Generic Mod Config Menu
  installed (they just won't see the config UI). See [example usage](https://gist.github.com/spacechase0/2d8d4dbffe5f2ce9457d2c891a8b99e3)
  with the API at the bottom.

* **For Content Patcher packs:** you don't need to do anything! Content Patcher will add the config
  UI automatically for you.

## Compatibility
Compatible with Stardew Valley 1.5+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
