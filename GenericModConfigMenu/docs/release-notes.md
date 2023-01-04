﻿[← back to readme](README.md)

# Release notes
# 1.9.1
* Fix crash when hovering over mod with null tooltip.

## 1.9.0
* Added keybindings menu (icon thanks to mistyspring!).
* Added description tooltip for mods on mod list menu.

## 1.8.2
* The mod list now remembers your scroll position when returning from a config view (thanks to Pathoschild!).
* Complex Options can now have Height that changes, rather than being fixed when the menu opens (thanks to jltaylor-us).

## 1.8.1
Released 12 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Fixed keybind overlay drawn off-screen if UI scale doesn't match zoom level.
* Improved translations. Thanks to Scartiana (added German)!

## 1.8.0
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* For players:
  * You can now disable a keybind from the keybind overlay.
  * You can now cancel or click outside the keybind overlay to close it without changes.
  * When viewing the list of configurable mods in-game, mods that are only editable on the title screen are now listed separately instead of hidden entirely.
  * Fixed clicks sometimes handled by elements that are scrolled out of view.
  * Fixed multi-row elements sometimes disappearing before they're fully off-screen (thanks to jltaylor-us!).
  * Fixed multi-row element heights in some cases (thanks to jltaylor-us!).
  * Fixed mouse-wheel-scroll sound playing even after the menu is closed.
  * Improved translations. Thanks to BuslaevLegat (added Russian), ChulkyBow (added Ukrainian), Evexyron (updated Spanish), Lake1059 (added Chinese), wally232 (updated Korean), and Zangorr (added Polish)!
* For mod authors:
  * You can now format slider labels.
  * Improved `AddComplexOption` with new menu opened/closed callbacks (thanks to jltaylor-us!).
  * Fixed API `TryGetCurrentMenu` method not working when on the title screen.

## 1.7.0
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.
* Added a config UI for Generic Mod Config Menu itself.
* Added optional keybind to open the config UI.
* Adjusted save/reset/cancel buttons:
  * The buttons now auto-align to fit their text for better localization support.
  * The reset button is no longer shown on subpages (since it resets the entire config).
  * Increased button area width.
* Improved `AddComplexOption` with more granular callbacks.
* Improved translations. Thanks to ellipszist (added Thai) and Evexyron (updated Spanish)!

## 1.6.0
Released 29 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added support for translating dropdown values (thanks to ImJustMatt!).
* Improved translations. Thanks to wally232 (added Korean)!

## 1.5.1
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed checkboxes not selectable in the 1.5.0 update.

## 1.5.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added full [translation](https://stardewvalleywiki.com/Modding:Translations) support.
* Redesigned mod API:
  * Added support for translating mod options.
  * Simplified usage and merged methods.
  * Options are now available in-game by default when using the new API (unless you set `titleScreenOnly: true`).
  * Complex options can now set a height to support multi-row content.
* Fixed sliders for integer values not showing the value label.
* Fixed sliders with an `interval` sometimes snapping to a value outside the `min`/`max` range.
* Fixed long paragraphs sometimes overlapping the fields below them.
* Fixed long paragraphs or images clipped before the end of the content area.
* Fixed `api.SubscribeToChange` only tracking fields on the page that was active when it was called.
* Improved translations. Thanks to Evelyon (added Spanish)!

**Migration guide for mod authors:**  
See the [1.5.0 migration guide](author-migration-guide.md#150).

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
