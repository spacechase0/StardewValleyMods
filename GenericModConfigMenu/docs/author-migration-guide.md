﻿← [README](README.md)

This document helps mod authors update their mods for newer versions of Generic Mod Config Menu. If
a version isn't listed here, no migration is needed for it.

**This is for mod authors only. See the [main README](README.md) for other info**.

## Migration guides
### 1.5.0
Released 15 October 2021 (see [release notes](release-notes.md#150)).

1.5.0 adds a redesigned mod API. The previous API still works, but it's deprecated and will
eventually be removed. To migrate your mod code to the new API:

1. Replace `IGenericModConfigMenuApi` with [the latest version](../IGenericModConfigMenuApi.cs).
2. Arguments like `name` and `tooltip` now let you get text from your mod's translations:

   ```c#
   name: () => this.Helper.Translation.Get("example.name"),
   tooltip: () => this.Helper.Translation.Get("example.tooltip")
   ```

   If your config text isn't translatable, you can just return literal text instead:

   ```c#
   name: () => "Example Option",
   tooltip: () => "This is an example option."
   ```
3. Update code which calls the old methods:

   old method | migration notes
   :--------- | :--------------
   `RegisterModConfig` | Use `Register`.<br />**Note:** config fields will be enabled both on the title screen and in-game. To keep the previous behavior, set the `titleScreenOnly: true` argument.
   `UnregisterModConfig` | Use `Unregister`.
   `SetDefaultIngameOptinValue` | To change the default for all fields, set `titleScreenOnly` in the `Register` call. To only change it for some fields, use `SetTitleScreenOnlyForNextOptions` which works just like the old method if you invert the value (e.g. `SetDefaultIngameOptinValue(false)` → `SetTitleScreenOnlyForNextOptions(true)`).
   `StartNewPage` | Use `AddPage`.
   `OverridePageDisplayName` | Use `AddPage` with the `pageTitle` argument.
   `RegisterLabel` | Use `AddSectionTitle`.
   `RegisterPageLabel` | Use `AddPageLink`.
   `RegisterParagraph` | Use `AddParagraph`.
   `RegisterImage` | Use `AddImage`.<br />**Note A:** You now need to pass a `Texture2D` instance instead of an asset path. This avoids needing to provide the image through the game's content pipeline. To keep the previous logic, change `RegisterImage(mod, "texture path")` to `AddImage(mod, () => Game1.content.Load<Texture2D>("texture path"))`.<br />**Note B:** the texture is now cached while the menu is open. If it changes, the change will only be visible in-game when the mod's menu is reopened.
   `RegisterSimpleOption` | Use `AddBoolOption`, `AddKeybind`, `AddKeybindList`, `AddNumberOption`, or `AddTextOption` depending on the option type.
   `RegisterClampedOption` | Use `AddNumberOption`.
   `RegisterChoiceOption` | Use `AddTextOption`.
   `RegisterComplexOption` | Use `AddComplexOption`.
   `SubscribeToChange` | Use `OnFieldChanged`.

4. Delete any methods you don't need in your copy of `IGenericModConfigMenuApi`.

If you need help migrating your code, feel free to [ask in #making-mods on the Stardew Valley
Discord](https://stardewvalleywiki.com/Modding:Community#Discord)!

## See also
* [README](README.md) for other info
* [Ask for help](https://stardewvalleywiki.com/Modding:Help)
