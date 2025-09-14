﻿[← back to readme](README.md)

# Release notes
## 1.8.1
Released 12 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Fixed version check for Generic Mod Config Menu not working with some older versions.
* Improved translations. Thanks to Scartiana (added German)!

## 1.8.0
Release 11 January 2022 for SMAPI 3.13.0 or later.

* Added support for...
  * custom location contexts;
  * custom wallet items;
  * custom properties (to attach data to existing classes so they're persisted in the save file).
* Made custom UI framework public.  
  _This is the UI framework used to build Generic Mod Config Menu._

## 1.7.3
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Added min-version check for integration with Generic Mod Config Menu.

## 1.7.2
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.
* Added error handling for invalid tilesheets.
* Removed unneeded warning log.
* Improved translations. Thanks to Evexyron (added Spanish)!

## 1.7.1
Released 29 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Improved translations. Thanks to wally232 (added Korean)!

## 1.7.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added full [translation](https://stardewvalleywiki.com/Modding:Translations) support.
* Added `dump_spacecore_skills` command to list custom skills/professions registered through SpaceCore, including their IDs for save editing.
* Added option to disable All Professions integration for custom professions.
* Changed custom profession IDs for compatibility with the upcoming Stardew Valley 1.5.5. Existing saves will be migrated automatically.
* Improved integration with Generic Mod Config Menu:
  * Updated for Generic Mod Config Menu 1.5.0.
  * Enabled config UI from the in-game options menu after the save is loaded.

## 1.6.2
Released 19 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Removed custom map/tilesheet editing API.  
  _This was previously used by Magic and Theft of the Winter Star, which now use the built-in SMAPI APIs._
* Removed bundled `TiledNet.dll` and `TiledNet.pdb` files.

## 1.6.1
Released 11 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Backported 1.6 changes to Stardew Valley 1.5.4.
* Internal refactoring.

## 1.6.0
Released 06 September 2021 for SMAPI 3.13.0-beta or later. Updated by spacechase0.

* Added custom crafting recipes and forge recipes.
* Added a new `OnEventFinished` event.

## 1.5.11
Released 04 September 2021 for SMAPI 3.12.6 or later. Updated by Pathoschild.

* Fixed conflict with PyTK items if no custom SpaceCore items are registered.

## 1.5.10
Released 01 August 2021 for SMAPI 3.12.0 or later. Updated by Pathoschild.

* Updated for Harmony upgrade in SMAPI 3.12.0.

## 1.5.9
Released 17 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed serialization API in 1.5.8...
  * not fully compatible with PyTK;
  * not restoring custom items within custom items;
  * reordering custom items when there are several in the same list.

## 1.5.8
Released 10 July 2021  for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Improved performance and reduced memory usage when read/writing saves.
* SpaceCore no longer disables crop withering on day update.  
  _This should have no effect on players, since crops wither separately if needed. This was to
  support Json Assets, which now applies the change itself._

## 1.5.7
Released 19 June 2021  for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).
* Reduced performance impact (thanks to pepoluan!).
* Internal refactoring.
* Improved documentation.

## 1.5.6
Released 18 May 2021 for Stardew Valley 1.5.

* Overhauled custom skills page and improved gamepad support (thanks to b-b-blueberry!).
* Fixed game launched event not triggering, causing GMCM entry to not show up.
* Fixed serializer conflict with Entoarox Framework.

## 1.5.5
Released 23 February 2021 for Stardew Valley 1.5.

* Disabled serializer patches if the serialization API isn't used.

## 1.5.4
Released 28 January 2021 for Stardew Valley 1.5.

* Fixed potential memory issue.

## 1.5.3
Released 16 January 2021 for Stardew Valley 1.5.

* Added new UI framework elements: `StaticContainer` and `Image`.

## 1.5.2
Released 06 January 2021 for Stardew Valley 1.5.

* Updated for SMAPI 3.8.2.

## 1.5.1
Released 26 December 2020 for Stardew Valley 1.5.

* Fixed serializer issues.
* Fixed UI issues.
* Fixed winter crop issues.

## 1.5.0
Released 21 December 2020 for Stardew Valley 1.5.

* Added serialization API.

## 1.4.0
Released 16 August 2020 for Stardew Valley 1.4.

* Added UI changes from Generic Mod Config Menu.
* Added custom event command API.
* Added 'before receive gift' event.
* Hid farmhand error.
* Reduced tilesheet expansion logging.
* Fixed `ActionActivated` event not activating during festivals.

## 1.3.5
Released 14 March 2020 for Stardew Valley 1.4.

* Fixed Android issues (thanks to ZaneYork!).
* Fixed API issues.

## 1.3.4
Released 17 January 2020 for Stardew Valley 1.4.

* Fixed Linux/macOS tilesheet expansion issues.

## 1.3.3
Released 31 December 2019 for Stardew Valley 1.4.

* Added new event.

## 1.3.2
Released 23 December 2019 for Stardew Valley 1.4.

* Made `player_giveexp` console command case-insensitive.
* Fixed level up menu to work like 1.4 (fixes some bugs).
* Extended tilesheets are now loaded through SMAPI's content API so Content Patcher packs can edit them.

## 1.3.1
Released 28 November 2019 for Stardew Valley 1.4.

* Fixed rice paddies not working.

## 1.3.0
Released 26 November 2019 for Stardew Valley 1.4.

* Updated for Stardew Valley 1.4.
* Custom skills page is now disabled if no custom skills are registered.
* Added asset invalidate command.
* Added tilesheet expansion feature.
* Added support for [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).
* Fixed Linux/macOS support.

## 1.2.7
Released 18 May 2019 for Stardew Valley 1.3.36.

* Added an API for skills.
* Fixed no immediate level perks for levels 5 and 10.

## 1.2.6
Released 29 April 2019 for Stardew Valley 1.3.36.

* Fixed nightly farm event not being modifiable.

## 1.2.5
Released 18 January 2019 for Stardew Valley 1.3.33 and 1.3.36.

* Added `GameMenu` tab API.

## 1.2.4
Released 18 January 2019 for Stardew Valley 1.3.33.

* Updated for SMAPI 3.0 (thanks to Pathoschild!).
* Save data is now stored in the save instead of the mod folder, and existing data will be migrated automatically (thanks to Pathoschild!).

## 1.2.3
Released 03 December 2018 for Stardew Valley 1.3.32.

* Updated for Stardew Valley 1.3.32.
* Added new event.

## 1.2.2
Released 31 August 2018 for Stardew Valley 1.3.28.

* Fix support for [All Professions](https://www.nexusmods.com/stardewvalley/mods/174).
* Fixed skill API skills at level 10 with experience bars (hopefully).

## 1.2.1
Released 21 August 2018 for Stardew Valley 1.3.28.

* Fixed `OnBlankSave` event and experience bars.

## 1.2.0
Released 18 August 2018 for Stardew Valley 1.3.28.

* Updated for Stardew Valley 1.3.28.
* Added some new events.
* Added networking API.
* Added skill API.  
  _(Thanks to MercuriusXeno for some prerelease bug fixes!)_

## 1.1.1
Released 15 April 2018 for Stardew Valley 1.2.

* Fixed `CustomDecoratableLocation` issue.

## 1.1.0
Released 25 March 2018 for Stardew Valley 1.2.

* Added item eaten event.
* Fixed bugs.

## 1.0.5
Released 09 November 2017 for Stardew Valley 1.2.

* Fixed issue with custom farm types.

## 1.0.4
Released 08 November 2017 for Stardew Valley 1.2.

* Custom crops can now grow in winter.

## 1.0.3
Released 01 August 2017 for Stardew Valley 1.2.

* Changed black magic to dark gray magic.

## 1.0.2
Released 22 July 2017 for Stardew Valley 1.2.

* Fixed controller errors.
* Fixed sleeping crash.

## 1.0.1
Released 20 July 2017 for Stardew Valley 1.2.

* Fixed hijacking virtual methods.

## 1.0.0
Released 19 July 2017 for Stardew Valley 1.2.

* Initial release.
