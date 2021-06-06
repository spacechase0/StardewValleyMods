﻿[← back to readme](README.md)

# Release notes
## 1.7.6
Released 02 April 2021 for Stardew Valley 1.5.

* Json Assets now loads stuff earlier (for bundles) and saves earlier.
* Added QiGemShop support (thanks to Digus!).
* Added `ja_unfix` console command.
* Fixed `ja_summary` console command for farmhands.
* Fixed furniture contents not ID-fixed outside decoratable locations.
* Fixed equipped rings.

## 1.7.5
Released 15 January 2021 for Stardew Valley 1.5.

* Fixed double-ID-fixing TMXL locations.

## 1.7.4
Released 14 January 2021 for Stardew Valley 1.5.

* Shifted custom object IDs to start at 3000 (instead of 2000) to avoid vanilla conflicts in Stardew Valley 1.5.
* Fixed non-shop menus causing an error in the console.
* Fixed chests not being ID-fixed.

## 1.7.3
Released 06 January 2021 for Stardew Valley 1.5.

* Fixed adding bad shop data.
* Fixed corrupting bundle data (retroactive).

## 1.7.2
Released 05 January 2021 for Stardew Valley 1.5.

* Added `GetAllBootsFromContentPack` to API (thanks to CMAlbrecht!).
* Fixed purchase requirement stuff and recipe purchasing.

## 1.7.1
Released 21 December 2020 for Stardew Valley 1.5.

* Fixed Linux/macOS issue.

## 1.7.0
Released 21 December 2020 for Stardew Valley 1.5.

* Now requires [Expanded Preconditions Utility](https://www.nexusmods.com/stardewvalley/mods/6529).
* Added `ja summary` console command to list IDs.
* Added support for...
  * Forge recipes;
  * custom fences;
  * universal gift tastes.
* Added fields:
  * `HideFromShippingCollection` for objects;
  * `CanPurchase` for hats;
  * `AdditionalPurchaseData` field for everything.
* Removed limits on the number of objects, hats, and big craftables.
* Improved performance in asset patching (thanks to aaron-cooper!).
* Fixed bugs.

## 1.6.2
Released 01 March 2020 for Stardew Valley 1.4.

* Added `ReserveExtraIndexCount`.
* Added Artifact category.
* Fixed for gift tastes conflict with Content Patcher.

## 1.6.1
Released 02 February 2020 for Stardew Valley 1.4.

* Added [`SpriteTilesheet` and `SpriteX`/`Y` custom tokens for Content Patcher](author-guide.md#tokens-in-fields).
* Fixed boots stats being reset.
* Fixed unshippable objects being in the collection.

## 1.6.0
Released 17 January 2020 for Stardew Valley 1.4.

* Added support for boots.
* Added support for giant crops.
* Made API content packs not random.
* More IDs to skip for big craftables.
* Fixed sign contents not being ID-fixed.

## 1.5.6
Released 01 January 2020 for Stardew Valley 1.4.

* Fixed `Object.preserveParentSheetIndex`.

## 1.5.5
Released 01 January 2020 for Stardew Valley 1.4.

* Fixed `Crop.netSeedIndex`.


## 1.5.4
Released 31 December 2019 for Stardew Valley 1.4.

* Skipped problematic IDs.
* Fixed big craftables with light sources showing "true" for their name.
* Added to API:
  * fix IDs;
  * get all things for a content pack.

## 1.5.3
Released 25 December 2019 for Stardew Valley 1.4.

* Fixed furniture not being ID fixed.
* Added new IDs fixed event.
* Added attributes: `EnableWithMod`, `CanSell`, `CanTrash`, `CanBeGifted`.

## 1.5.2
Released 23 December 2019 for Stardew Valley 1.4.

* Added greens category.
* Added `ReserveNextIndex` for a second texture for big craftables.
* Added coordinate tokens for CP (not yet usable).
* Added skill unlock support for recipes.
* Block actions only for "Chair" big craftables.
* Improved error handling in Harmony patches (thanks to Pathoschild!).
* Fixed recipe result count not being used.
* Fixed an issue with removed items in collections.
* Fixed non-localized recipes.

## 1.5.1
Released 30 November 2019 for Stardew Valley 1.4.

* Fixed empty ponds being loaded breaking the game.

## 1.5.0
Released 29 November 2019 for Stardew Valley 1.4.

* More API stuff.
* Added category text/color override.
* Removed limits on the number of shirts and pants.

## 1.4.0
Released 26 November 2019 for Stardew Valley 1.4.

* Updated for Stardew Valley 1.4.
* Added support for...
  * clothing/tailoring;
  * hat metadata;
  * object context tags;
  * indoor/paddy crops;
  * separate seed sell price.
* Added `DisableWithMod` field.
* Removed limits on the number of crops and fruit trees.
* Fixed bugs.

## 1.3.7
Released 14 June 2019 for Stardew Valley 1.3.36.

* Fixed crops not working for all seasons.
* Fixed Harvey not being a valid shop location.
* Added [custom tokens for Content Patcher](author-guide.md#tokens-in-fields) (`ObjectId`, `BigCraftableId`, `CropId`, `FruitTreeId`, `HatId`, and `WeaponId`).


## 1.3.6
Released 18 May 2019 for Stardew Valley 1.3.36.

* Fixed vanilla collection entries disappearing.

## 1.3.5
Released 18 May 2019 for Stardew Valley 1.3.36.

* Fixed multiplayer saving issue.
* Fixed API null issue.
* Fixed certain weird loading issue.
* Fixed collections not being ID-fixed.


## 1.3.4
Released 09 May 2019 for Stardew Valley 1.3.36.

* Fixed farmhands with custom rings.

## 1.3.3
Released 09 March 2019 for Stardew Valley 1.3.36.

* Added more logging.
* Fixed bugs.

## 1.3.2
Released 04 March 2019 for Stardew Valley 1.3.36.

* Added ability to get all assigned IDs from the API.
* Added clearer warning when duplicate items exist.
* Fixed loading items on new save.
* Fixed multiplayer client loading items.

## 1.3.1
Released 05 February 2019 for Stardew Valley 1.3.36.

* Fixed descriptions.
* Fixed loading saves with ring objects (probably).

## 1.3.0
Released 29 January 2019 for Stardew Valley 1.3.33.

* Added localization support.
* Added weapons and hats to API.
* Added animal goods category.

## 1.2.0
Released 25 January 2019 for Stardew Valley 1.3.33.

* Added support for weapons.

## 1.1.3
Released 17 January 2019 for Stardew Valley 1.3.33.

* Updated for SMAPI 3.0 (thanks to Pathoschild!).
* Dropped support for legacy ID mapping files in the mod folder (thanks to Pathoschild!).

## 1.1.2
Released 15 January 2019 for Stardew Valley 1.3.32.

* Fixed chairs cycling.
* Fixed rings not shown in collection menu.
* Maybe more?

## 1.1.0
Released 24 February 2018 for Stardew Valley 1.2.

* Added support for [standard content packs](https://stardewvalleywiki.com/Modding:Content_packs) (thanks to Pathoschild!).
* Added API.
* Added ring category.
* Fixed some problems with default recipes. 


## 1.0.0
Released 28 December 2017 for Stardew Valley 1.2.

* Initial release.
* Updated for SMAPI 2.0 before release (thanks to Pathoschild!).
