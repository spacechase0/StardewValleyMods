﻿[← back to readme](README.md)

# Release notes
## Upcoming release
* Redesigned API:
  * Added full translation support for display text.
  * Simplified usage and merged methods.
  * Options are now available in-game by default when using the new API (unless you set `editableInGame: false`).
* The generic config UI is now translatable.
* Fixed `api.SubscribeToChange` only tracking fields on the page that was active when it was called.
* Fixed sliders with an `interval` sometimes snapping to a value outside the `min`/`max` range.

**Migration guide for mod authors:**  
<details>
  <summary>Click to expand</summary>

The previous API still works, but it's now deprecated and will eventually be removed. To migrate
your mod code to the new API:

1. Replace `IGenericModConfigMenuApi` with [the latest version](../IGenericModConfigMenuApi.cs).
2. Arguments like `name` and `tooltip` let you get text from your mod's translations now:

   ```c#
   name: () => this.Helper.Translation.Get("example.name"),
   tooltip: () => this.Helper.Translation.Get("example.tooltip")
   ```

   If you config text isn't translatable, you can just return literal text instead:

   ```c#
   name: () => "Example Option",
   tooltip: () => "This is just an example option."
   ```
3. Update code which calls the old methods:

   old method | migration notes
   :--------- | :--------------
   `RegisterModConfig` | Use `Register`.<br />**Note:** config fields will be enabled both on the title screen and in-game. To keep the previous behavior, set the `editableInGame: false` argument.
   `UnregisterModConfig` | Use `Unregister`.
   `SetDefaultIngameOptinValue` | To change the default for all fields, set `editableInGame` in the `Register` call. To only change it for some fields, use `SetEditableInGameForNextOptions` which works just like the old method.
   `StartNewPage` | Use `AddPage`.
   `OverridePageDisplayName` | Use `AddPage` with the `pageTitle` argument.
   `RegisterLabel` | Use `AddSectionTitle`.
   `RegisterPageLabel` | Use `AddPageLink`.
   `RegisterParagraph` | Use `AddParagraph`.
   `RegisterImage` | Use `AddImage`.<br />**Note A:** You now need to pass a `Texture2D` instance instead of an asset path. This avoids needing to provide the image through the game's content pipeline. To keep the previous logic, change `RegisterImage(mod, "texture path")` to `AddImage(mod, () => Game1.content.Load<Texture2D>("texture path"))`.<br />**Note B:** the texture is now cached while the menu is open. If it changes, the change will only be visible in-game when the mod's menu is reopened.
   `RegisterSimpleOption` | Use `AddOption`.
   `RegisterClampedOption` | Use `AddOption`.
   `RegisterChoiceOption` | Use `AddOption`.
   `RegisterComplexOption` | Use `AddComplexOption`.
   `SubscribeToChange` | Use `OnFieldChanged`.

4. Delete any methods you don't need in your copy of `IGenericModConfigMenuApi`.

If you need help migrating your code, feel free to [ask in #making-mods on the Stardew Valley
Discord](https://stardewvalleywiki.com/Modding:Community#Discord)!

</details>

## 1.4.2
Released 11 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild and spacechase0.

* Fixed error when a mod removes an already-removed mod page.
* Internal refactoring.

## 1.4.1
Released 12 July 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Improved mod manifest validation.
* Fixed intermittent error with Custom Music.

## 1.4.0
Released 10 July 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* You can now press `ESC` to exit a mod config menu (thanks to pepoluan!).
* Added API method to get info about the currently displayed mod config menu (thanks to pepoluan!).
* Improved API validation (in collaboration with pepoluan).
* Removed example mod config (thanks to pepoluan!).
* Fixed UI scale not handled correctly (thanks to pepoluan!).
* Fixed cancel from a subpage returning to the mod list instead of the parent menu.
* Fixed paragraph rendering:
  * fixed quirky line wrapping;
  * fixed duplicated paragraph text;
  * fixed paragraph break after every second line.

## 1.3.4
Released 19 June 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).
* Fixed error changing a keybind list option which is currently unbound.
* Improved documentation.
* Internal refactoring.

## 1.3.3
Released 27 March 2021 for Stardew Valley 1.5.

* Added support for editing options after loading the save. This is only enabled for mods which opt in.
* Fixed error with unbound keys.
* Fixed escape being bindable.

## 1.3.2
Released 16 March 2021 for Stardew Valley 1.5.

* Added partial support for `KeybindingList` options.

## 1.3.1
Released 04 March 2021 for Stardew Valley 1.5.

* Added ability to override page display name.
* Added ability to unregister mod menus (thanks to Digus!).

## 1.3.0
Released 03 March 2021 for Stardew Valley 1.5.

* Added support for paragraphs, pages, and images.

## 1.2.1
Released 05 January 2021 for Stardew Valley 1.5.

* Fixed click detection in Stardew Valley 1.5.

## 1.2.0
Released 03 August 2020 for Stardew Valley 1.4.

* Improved API (thanks to Platonymous!):
  * Added interval option for float/int sliders.
  * Added methods to react to arbitrary option changes by ID.
  * Dropdowns can now be scrolled if needed.
* Added Android improvements (thanks to kdau!).
* Polished UI (thanks to kdau!).

## 1.1.0
Released 17 January 2020 for Stardew Valley 1.4.

* Added support for label.

## 1.0.0
Released 26 November 2019 for Stardew Valley 1.4.

* Initial release.
