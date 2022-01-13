﻿[← back to readme](README.md)

# Release notes
## 1.2.3
Released 12 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Fixed version check for Generic Mod Config Menu not working with some older versions

## 1.2.2
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Added min-version check for integration with Generic Mod Config Menu.
* Rewrote migration logic for older saves containing tents saved through PyTK.
* Improved translations. Thanks to wally232 (added Korean)!

## 1.2.1
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.
* Improved tent icon and textures (thanks to Illuzio!).
* Improved translations. Thanks to ellipszist (added Thai) and Evexyron (added Spanish)!

## 1.2.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added full [translation](https://stardewvalleywiki.com/Modding:Translations) support.
* Improved integration with Generic Mod Config Menu:
  * Updated for Generic Mod Config Menu 1.5.0.
  * Enabled config UI from the in-game options menu after the save is loaded.

## 1.1.2
Released 18 July 2021 for Stardew Valley 1.5. Updated by Pathoschild.

* Fixed error saving with tent tool.

## 1.1.1
Released 17 July 2021 for Stardew Valley 1.5. Updated by Pathoschild.

* Migrated from PyTK to SpaceCore.
* Fixed error when opening some shops.

**Update note:**  
You no longer need PyTK to use Sleepy Eye. Existing tent items in your save should be migrated
automatically. If you find a broken item named `PyTK|Item|SleepyEye.TentTool,  SleepyEye|`, you can
fix it by moving it into your inventory, then saving and reloading the save to migrate it.

## 1.1.0
Released 19 June 2021 for Stardew Valley 1.5. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.
* The place-tent delay is now configurable.
* Holding the tent button now triggers a save after the delay without waiting for you to release the button.
* SpaceCore is no longer required.
* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows).
* Improved documentation.
* Internal refactoring and optimizations.

**Update note:**  
If you slept in a tent before updating the mod, you'll start the day at home. The tent location
will be saved correctly thereafter.

## 1.0.4
Released 26 November 2019 for Stardew Valley 1.4.

* Updated for Stardew Valley 1.4.

## 1.0.3
Released 18 January 2019 for Stardew Valley 1.3.33.

* Updated for SMAPI 3.0 (thanks to Pathoschild!).

## 1.0.2
Released 18 August 2018 for Stardew Valley 1.3.28.

* Update for Stardew Valley 1.3.28.

## 1.0.1
Released 01 August 2017 for Stardew Valley 1.2.

* Fixed tent not trashable.

## 1.0.0
Released 10 May 2017 for Stardew Valley 1.2.

* Initial release.
