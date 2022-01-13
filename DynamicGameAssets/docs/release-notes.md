﻿[← back to readme](README.md)

# Release notes
## 1.4.1
Released 12 January 2022 for SMAPI 3.13.0 or later.

* Added support for giant crops outside the farm.

## 1.4.0
Release 11 January 2022 for SMAPI 3.13.0 or later.

* Added `Custom` field for item which use a custom class.
* Fixed cloning custom hats.
* Fixed SpaceCore's `AfterGiftGiven` event not raised when gifting custom items.

## 1.3.4
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Improved errors when a content pack...
  * adds an invalid recipe entry to shops;
  * adds a duplicate texture override.
* Improved translations. Thanks to burunduk (added Ukrainian) and wally232 (added Korean)!

## 1.3.3
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.
* Fixed issues with custom forge recipes (thanks to Esca-MMC!):
  * Fixed error when an entire item stack is consumed.
  * Fixed wrong result item shown if any ingredients require multiple.
* Improved translations. Thanks to ellipszist (added Thai) and Evexyron (added Spanish)!

## 1.3.2
Released 27 November 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed error when giving a custom item as a gift when the content pack doesn't specify response translation keys.

## 1.3.1
Released 29 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Improved mod-provided API (thanks to Digus!):
  * Added support for creating a colored item.
  * Fixed error when requesting an unknown/invalid item ID.
* Improved example content pack:
  * Standardized file structure.
  * Added update key.
* Fixed error in some cases when a texture being drawn is null.

## 1.3.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added full [translation](https://stardewvalleywiki.com/Modding:Translations) support.
* Added three new furniture types: `Lamp`, `Sconce`, and `Window` (thanks to echangda!).
* Updated for Generic Mod Config Menu 1.5.0.
* Fixed various animation parsing issues.
* Fixed shop ID for Krobus (thanks to echangda!).
* Fixed error when crafting in some cases.

## 1.2.1
Released 19 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Reverted recipe changes in 1.2.0 which broke some content packs.
* Fixed breaking bigcraftables with an axe (thanks to ImJustMatt!).

## 1.2.0
Released 19 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild and spacechase0.

* Added animation range syntax (thanks to ImJustMatt!).
* Added support for custom fruit trees with a chance to produce nothing on a given day.
* Fixed edge cases for custom items with " Recipe" in the name.
* Fixed error parsing rectangle fields in some cases.

## 1.1.0
Released 11 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild and spacechase0.

* Backported to Stardew Valley 1.5.4.
* Added mod-provided API method to spawn DGA items by their ID.
* Fixed bigcraftable textures being cut off in some places.
* Fixed some shops not identified correctly.
* Fixed game unable to remove vanilla items from the player inventory in some cases.
* Fixed DGA items not placeable in some cases.
* Internal refactoring.

## 1.0.0
Released 06 September 2021 for SMAPI 3.13.0-beta or later. Updated by spacechase0.

* Initial release.
