﻿[← back to readme](README.md)

# Release notes
## 1.10.3
Released 09 January 2022 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Internal optimizations.

## 1.10.2
Released 24 December 2021 for SMAPI 3.13.0 or later. Updated by Pathoschild.

* Updated for Stardew Valley 1.5.5.

## 1.10.1
Released 27 November 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Fixed error when an indoor pot contains a custom crop that no longer exists.
* Reduced some log messages to `TRACE` level.

## 1.10.0
Released 15 October 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild.

* Added support for storing item translations in standard `i18n` translation files ([see docs](author-guide.md#localization)).
* Added support for using `default` as a locale in `NameLocalization` and `DescriptionLocalization` fields, which sets the default display name in any language (including English) which doesn't have a translation.
* Added validation error if an item doesn't have the required name field.
* Fixed objects/seeds in shops using `PurchasePrice` instead of `Price` as the sell price.

## 1.9.1
Released 11 September 2021 for SMAPI 3.12.5 or later. Updated by Pathoschild and spacechase0.

* Fixed error if an item is initialized before Json Assets is initialized.
* Internal refactoring.

## 1.9.0
Released 04 September 2021 for SMAPI 3.12.6 or later. Updated by Pathoschild.

* Expanded Preconditions Utility (EPU) is now optional.  
  _Json Assets now handles vanilla conditions itself. If a content pack uses EPU-specific conditions
  and you don't have it installed, Json Assets will log an error and treat those conditions as
  always failed._
* Fixed resource clumps outside the farm not ID-fixed.
* Fixed error when ID-fixing bundles with invalid item IDs.

**Update note for mod authors:**  
If your content pack uses EPU-specific conditions in a `PurchaseRequirements` field, adding it as a
[manifest dependency](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Dependencies)
and to your mod description is recommended to avoid confusion.

These conditions are EPU-specific: `!` (to reverse a condition), `CommunityCenterComplete`,
`FarmHouseUpgradeLevel`, `HasCookingRecipe`, `HasCraftingRecipe`, `HasItem`, `HasMod`,
`JojaMartComplete`, `NPCAt`, `SeededRandom`, and `SkillLevel`.

## 1.8.3
Released 01 August 2021 for SMAPI 3.12.0 or later. Updated by Pathoschild.

* Updated for Harmony upgrade in SMAPI 3.12.0.
* Fixed duplicate items added to shop menu when it's hidden and restored by mods like Lookup Anything.

## 1.8.2
Released 24 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed items added to Krobus' shop not added when he's not there.

## 1.8.1
Released 18 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed error patching the forge menu.

## 1.8.0
Released 17 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* You can now add items to any shop in the game, including custom shops. See [_shops_ in the author guide](author-guide.md#shops).
* Shop IDs for `PurchaseFrom` fields are no longer case-sensitive.
* Fixed "couldn't find destroyCrop call in the HoeDirt.dayUpdate method" error for some players.
* Internal refactoring.

## 1.7.8
Released 10 July 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Updated for SpaceCore 1.5.8.
* Fixed `AdditionalData` for saplings adding recipe instead of sapling to the shop (thanks to lshtech!).
* Fixed crash in some cases when two custom items have the same name and type.
* Fixed many cases that would cause `NullReferenceException` errors or crashes.
* Internal refactoring.

## 1.7.7
Released 19 June 2021 for SMAPI 3.9.5 or later. Updated by Pathoschild.

* Fixed compatibility with [unofficial 64-bit mode](https://stardewvalleywiki.com/Modding:Migrate_to_64-bit_on_Windows). (Thanks to pepoluan for a related winter crop fix!)
* Fixed cask contents not ID-fixed.¹
* Fixed combined rings not ID-fixed in some cases (thanks to SlivaStari!).
* Internal refactoring.
* Improved documentation. (Thanks to 6480k and ParadigmNomad for some updates in the author guide!)

<sup>¹ Unreleased changes by spacechase0.</sup>

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
