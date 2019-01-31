[Json Assets](https://github.com/spacechase0/JsonAssets) is a [Stardew Valley](http://stardewvalley.net/) mod which allows custom objects to be added to the game.
                                                                                                           
**This documentation is for modders. If you're a player, see the [Nexus page](https://www.nexusmods.com/stardewvalley/mods/1720) instead.**
                                                                                                           
## Contents
* [Install](#install)
* [Introduction](#introduction)
* [Basic Features](#basic-features)
  * [Overview](#overview)
  * [Big Craftables](#bigcraftables)
  * [Crops](#crops)
  * [Fruit Trees](#fruittrees)
  * [Objects](#objects)
    * [Crop and Fruit Tree Objects](#crop-and-fruit-tree-objects)
    * [Recipes](#recipes)
  * [Hats](#hats)
  * [Weapons](#weapons)
* [Gift Tastes](#gift-tastes)
* [Localization](#localization)
* [Converting From Legacy Format](#converting-from-legacy-format)
* [Releasing A Content Pack](#releaseing-a-content-pack)
* [Troubleshooting](#troubleshooting)
  * [Target Out of Range](#target-out-of-range)
  * [Exception Injecting Given Key](#exception-injecting-given-key)
  * [Exception Injecting Duplicate Key](#exception-injecting-duplicate-key)
* [See Also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. Install [this mod from Nexus mods](https://www.nexusmods.com/stardewvalley/mods/1720).
3. Unzip any Json Assets content packs into `Mods` to install them.
4. Run the game using SMAPI.

## Introduction
### What is Json Assets?
Json Assets allows you to add custom objects to the game without having to create a SMAPI mod or altering vanilla files. Currently, Json Assets supports the following types of items:
* Crops
* Fruit Trees
* Recipes
* Craftables (16x16)
* Big-Craftables (16x32)
* Hats (20x80)
* Weapons

Examples of how to set up all types of objects can be found in the [Blank JSON Assets Template](https://www.nexusmods.com/stardewvalley/mods/1746). I also highly recommend looking up preexisting content packs for further examples:

* [Farmer to Florist](https://www.nexusmods.com/stardewvalley/mods/2075) contains examples of big craftables.
* [Starbrew Valley](https://www.nexusmods.com/stardewvalley/mods/1764) contains examples using all valid EdibleBuff fields.
* [Fantasy Crops](https://www.nexusmods.com/stardewvalley/mods/1610) contains examples of crops producing vanilla items.
* [PPJA Home of Abandoned Mods](https://www.nexusmods.com/stardewvalley/mods/3374) contains examples of hats & weapons.

### Companion Mods
Json Assets is a great tool if you want to add one of the above objects, but there are other frameworks out there that pair well with Json Assets:

 * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) to add machines.

## Basic Features
### Overview
There are six main folders you are likely to see when downloading Json Asset content packs:
* BigCraftables
* Crops
* FruitTrees
* Objects 
* Hats
* Weapons

You will also see a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:SMAPI_APIs#Manifest) on the wiki).
Each of these folders contains subfolders that at minimum contains a `json` and a `png`. 

### BigCraftables
Big craftables are objects like scarecrows that are 16x32.

A big craftable subfolder is a folder with these files:
* a `big-craftable.json`;
* a `big-craftable.png`. Size: 16x32

The `big-craftable.json` contains these fields:

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                | How much your item sells for.
`Description`          | Description for what this does. Note if it does anything special like provide light.
`ProvidesLight`        | On/Off switch for if it provides light or not. Set to `true` or `false`.
`Recipe`               | Begins the recipe block.
`ResultCount`          | How many of the product does the recipe produce.
`Ingredients`          | If using a vanilla object, you will have to use the [objects ID number](https://pastebin.com/TBsGu6Em). If using a custom object added by Json Assets, you will have to use the name. Ex. "Honeysuckle".
`Object` & `Count`     | Fields that are part of `Ingredients`. You can add up to five different ingredients to a recipe. `Object` fields that contain a negative value are the generic ID. Example: Rather than using a specific milk, -6 allows for any milk to be used.
`IsDefault`            | _(optional)_ Setting this to `true` will have the recipe already unlocked. Setting this to `false` (or excluding this field) will require additional fields specifiying how to obtain the recipe:
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor.
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.

### Crops

A crop subfolder is a folder with these files:
* a `crop.json`;
* a `crop.png`; Size: 128x32
* a `seeds.png`. Size: 16x16

field                      | purpose
-------------------------- | -------
`Name`                     | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                    | How much your item sells for.
`Product`                  | Determines what the crop produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the [objects ID number](https://pastebin.com/TBsGu6Em) and not include a corresponding `Objects` folder.
`SeedName`                 | The seed name of the crop. Typically crop name + seeds or starter.
`SeedDescription`          | Describe what season you plant these in. Also note if it continues to grow after first harvest and how many days it takes to regrow.
`Type`                     | Available types are `Flower`, `Fruit`, `Vegetable`, `Gem`, `Fish`, `Egg`, `Milk`, `Cooking`, `Crafting`, `Mineral`, `Meat`, `Metal`, `Junk`, `Syrup`, `MonsterLoot`, `ArtisanGoods`, and `Seeds`.
`Season`                   | Seasons must be in lowercase and in quotation marks, so if you want to make your crop last all year, you'd put in "spring", "summer", "fall", "winter". If you want to make winter plants, you will have to require [SpaceCore](http://www.nexusmods.com/stardewvalley/mods/1348) for your content pack.
`Phases`                   | Determines how long each phase lasts. Crops can have 2-5 phases, and the numbers in phases refer to how many days a plant spends in that phase. Seeds **do not** count as a phase. If your crop has regrowth, the last number in this set corresponds to how many days it takes for the crop to regrow. Ex. [1, 2, 3, 4, 3] This crop takes 10 days to grow and 3 days to regrow.
`RegrowthPhase`            | If your plant is a one time harvest set this to `-1`. If it does, this determines which sprite the regrowth starts at. I typically recommend the sprite right before the harvest. *Requires additional sprite at the end of the crop.png*
`HarvestWithScythe`        | Set to `true` or `false`.
`TrellisCrop`              | Set to `true` or `false`. Determines if you can pass through a crop or not. Flowers cannot grow on trellises and have colors.
`Colors`                   | Colors use RGBA for color picking, set to `null` if your plant does not have colors.
`Bonus`                    | This block determines the chance to get multiple crops.
`MinimumPerHarvest`        | Minimum number of crops you will get per harvest. Must be one or greater.
`MaximumPerHarvest`        | Maximum number of crops you will get per harvest. Must be one or greater. *Recommended not to exceed 10*.
`MaxIncreasePerFarmLevel`  | How many farming skill experience points you get from harvesting.
`ExtraChance`              | Value between 0 and 1.
`SeedPurchasePrice`        | How much you can purchase seeds for.
`SeedPurchaseFrom`         | Who you can purchase seeds from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor.
`SeedPurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `SeedPurchaseRequirements` set this to `null`.

### FruitTrees
Fruit trees added by Json Assets work a bit differently than vanilla fruit trees as you have to till/hoe the ground before planting them. This means they cannot be placed on the edges of the greenhouse like vanilla trees. (7/4/18) There is a proposed fix using Harmony to treat custom trees like vanilla trees.

A fruit trees subfolder is a folder with these files:
* a `tree.json`;
* a `tree.png`; Size: 432x80
* a `sapling.png`. Size: 16x16

field                         | purpose
----------------------------- | -------
`Name`                        | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                       | How much your item sells for.
`Product`                     | Determines what the fruit tree produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the [objects ID number](https://pastebin.com/TBsGu6Em) and not include a corresponding `Objects` folder.
`SaplingName`                 | The name of the sapling, typically product + sapling.
`SaplingDescription`          | The description of the sapling, often sticks to vanilla format: Takes 28 days to produce a mature `product` tree. Bears `type` in the summer. Only grows if the 8 surrounding \"tiles\" are empty.
`Season`                      | Season must be in lowercase and in quotation marks. Fruit trees can support only one season. If you want to make winter fruit trees, you will have to require [SpaceCore]
`SaplingPurchasePrice`        | Determines how much the sapling can be purchased for.
`SaplingPurchaseFrom`         | Who you can purchase saplings from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor.
`SaplingPurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `SaplingPurchaseRequirements` set this to `null`.

### Objects
#### Crop and Fruit Tree Objects
Unless your crop or fruit tree is producing a vanilla item, it will need to have a corresponding folder in `Objects`

An object subfolder for crops & fruit trees is a folder that contains these files:

* an `object.json`;
* an `object.png`; Size: 16x16
* _(optional)_ a `color.png`. Size: 16x16, this will be a grayscale version of the part you want colored. *[See Mizu's Flowers](https://www.nexusmods.com/stardewvalley/mods/2028) for an example*.

field                         | purpose
----------------------------- | -------
`Name`                        | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                       | How much your item sells for.
`Description`                 | Description of the product.
`Category`                    | This should match the `crop.json` `Type` or for fruit trees should be one of the following: `Flower`, `Fruit`, `Vegetable`, `Gem`, `Fish`, `Egg`, `Milk`, `Cooking`, `Crafting`, `Mineral`, `Meat`, `Metal`, `Junk`, `Syrup`, `MonsterLoot`, `ArtisanGoods`, and `Seeds`.
`Edibility`                   | Edibility is for health, energy is calculated by the game. For inedibile items, set to -300.
`IsColored`                   | _(optional)_ Set this value to `true` if your product is colored.
`Recipe`                      | Set to `null`.

#### Recipes
Recipes and craftables (16x16) can be added via Json Assets through the `Objects` folder.

An object subfolder for a recipe is a folder that contains these files:

* an `object.json`;
* an `object.png`;

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                | How much your item sells for.
`Description`          | Description of the product.
`Category`             | Set to either `Crafting` or `Cooking` depending on the menu you want it to appear in.
`Edibility`            | Edibility is for health, energy is calculated by the game. For inedibile items, set to -300.
`EdibleIsDrink`        | Set to `true` or `false`.
`EdibleBuffs`          | Either set to `null` or include **all** required valid fields. It will not work if you only use the needed fields. Set unused fields to 0. Supports negative values. Required valid fields: `Farming`, `Fishing`, `Mining`, `Luck`, `Duration`. Optional valid fields: `Foraging`, `MaxStamina`, `MagnetRadius`, `Speed`, `Defense`, `Attack`.
`IsColored`            | Set to `false`.
`Recipe`               | Begins the recipe block.
`ResultCount`          | How many of the product does the recipe produce.
`Ingredients`          | If using a vanilla object, you will have to use the [objects ID number](https://pastebin.com/TBsGu6Em). If using a custom object added by Json Assets, you will have to use the name. Ex. "Honeysuckle".
`Object` & `Count`     | Fields that are part of `Ingredients`. You can add up to five different ingredients to a recipe. `Object` fields that contain a negative value are the generic ID. Example: Rather than using a specific milk, -6 allows for any milk to be used.
`IsDefault`            | _(optional)_ Setting this to `true` will have the recipe already unlocked. Setting this to `false` (or excluding this field) will require additional fields specifiying how to obtain the recipe:
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor.
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.

### Hats
Hats are 20x80 and can be added through a `Hats` folder. All hats are purchaseable through [hat mouse](https://stardewvalleywiki.com/Abandoned_House). There is a limit of 87 custom hats. 

A hats subfolder for a hat is a folder that contains these files:

* a `hat.json`;
* a `hat.png`;

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your object to have, this should be identical to the subfolder name.
`Description`          | Description of the product.
`PurchasePrice`        | How much you can purchase the hat for.
`ShowHair`             | Set this to `true` or `false` depending on if you want the players' hair to be visible or not. Setting this to `false` is a good idea for masks.
`IgnoreHairstyleOffset`| Set this to `true` or `false`. When set to `true` the hat will ignore any hairstyle offset.

### Weapons
Weapons are 16x16 and can be added via Json Assets through the `Weapons` folder. 

A weapon subfolder is a folder that contains these files:

* a `weapon.json`;
* a `weapon.png`;

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your object to have, this should be identical to the subfolder name.
`Description`          | Description of the product.
`Category`             | Depending on the weapon set this to one of the following: `sword`, `dagger`, or `club`. `Slingshot` is untested.
`MinimumDamage`        | The minimum number of damage points an enemy hit with this weapon will receive.
`MaximumDamage`        | The maximum number of damage points an enemy hit with this weapon will receive.
`Knockback`            | How far the enemy will be pushed back from the player after being hit with this weapon.
`Speed`                | How fast the swing of the weapon is.
`Accurary`             | How accurate the weapon is.
`Defense`              | When blocking, how much protection it provides. * This could be a
`MineDropVar`          | * I'm honestly not sure what this one is
`MineDropMinimumLevel` | The first level the weapon can drop when in the mines.
`ExtraSwingArea`       |
`CritChance`           | The chance the weapon will land a critical hit.
`CritMultiplier`       | Damage multiplied by this number is how much damage a critical hit does.
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor. For weapons, `Marlon` is recommended.
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.

Weapons do not support gift taste.

## Gift Tastes

You can add gift taste support to any pre-existing content pack by adding the following to the respective `.json` file. It does not matter where you put it. I tend to place it at the bottom of the `.json` but it is personal preferance. 

If it can be gifted to an NPC it has gift taste support built in. This means `hats` and `big-craftables` do not have gift taste support. If you exclude an NPC from the gift taste, their reaction will default to `Neutral`.

```
 "GiftTastes":
    {
        "Love": [],
        "Like": [],
        "Neutral": [],
        "Dislike": [],
        "Hat": [],
    },
```
An example of a filled out gift taste can be found [here](https://pastebin.com/9K3t2SLL). You can delete unused fields within `GiftTastes`.

## Localization

JsonAssets supports name localization without the need for a seperate or different download. These lines can be added to the bottom of their respective `json` files. Most localization is the same except "Crops have their loc. fields prefixed with"Seed", fruit trees prefixed with "Sapling"."

Examples:
```
 "NameLocalization": { "es": "spanish weapon (name)" },
    "DescriptionLocalization": { "es": "spanish weapon (desc)" }
```

For Crops:
```
"SeedNameLocalization": { "es": "spanish seed (name)" },
    "SeedDescriptionLocalization": { "es": "spanish seed (desc)" }
```

For Saplings:
```
"SaplingNameLocalization": { "es": "spanish ftree (name)" },
    "SaplingDescriptionLocalization": { "es": "spanish ftree (desc)" }
```

## Converting From Legacy Format
Before the release of SMAPI 2.5, Json Assets content packs previously needed a `content-pack.json` and had to be installed directly in the Json Assets folder. This is an outdated method and the more current `manifest.json` method should be used.

To learn how to set up a `manifest.json` please visit the [wiki page](https://stardewvalleywiki.com/Modding:SMAPI_APIs#Manifest). An example `manifest.json` specifically for Json Assets is included below:

```
{
   "Name": "Mizu's Flowers for JsonAssets",
   "Author": "ParadigmNomad & Eemie (Port) & Mizu (Sprites)",
   "Description": "A port of Mizu's sprites for JsonAssets.",
   "Version": "1.4",
   "UniqueID": "Mizu Flowers",

   "ContentPackFor": {
       "UniqueID": "spacechase0.JsonAssets"
    },
   "UpdateKeys": [ "Nexus:2028" ],
}
```
## Releasing a content pack
See [content packs](https://stardewvalleywiki.com/Modding:Content_packs) on the wiki for general
info. Suggestions:

1. Add specific install steps in your mod description to help players:
   ```
   [size=5]Install[/size]
   [list=1]
   [*][url=https://smapi.io]Install the latest version of SMAPI[/url].
   [*][url=https://www.nexusmods.com/stardewvalley/mods/1720]Install Json Assets[/url].
   [*]Download this mod and unzip it into [font=Courier New]Stardew Valley/Mods[/font].
   [*]Run the game using SMAPI.
   [/list]
   ```
2. When editing the Nexus page, add Json Assets under 'Requirements'. Besides reminding players to install it first, it'll also add your content pack to the list on the Json Asset page.

## Troubleshooting

There are some common errors with easy solutions. Your error may look slightly different but the general principal is the same. For a more in depth FAQ visit [this](https://github.com/paradigmnomad/ppjajsonassetsfaq/blob/master/README.md) link. FAQ is a work in progress.

### Target Out of Range
```
Exception injecting crop sprite for Blue_Mist: System.ArgumentOutOfRangeException: The target area is outside the bounds of the target texture.
Parameter name: targetArea
   at StardewModdingAPI.Framework.Content.AssetDataForImage.PatchImage(Texture2D source, Nullable`1 sourceArea, Nullable`1 targetArea, PatchMode patchMode) in C:\source\_Stardew\SMAPI\src\SMAPI\Framework\Content\AssetDataForImage.cs:line 44
   at JsonAssets.ContentInjector.Edit[T](IAssetData asset) in G:\StardewValley\Mods\JsonAssets\ContentInjector.cs:line 194
```
Solution: The sprite is too big. Double check what size the image needs to be for that specific type of item and crop your image accordingly. If you're trying to load tree crops, this error occurs when you have reached the maximum amount of trees the game can handle (a hardcoded number). 

### Exception Injecting Given Key
```
Exception injecting cooking recipe for Bulgogi: System.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
   at JsonAssets.Mod.ResolveObjectId(Object data) in G:\StardewValley\Mods\JsonAssets\Mod.cs:line 336
   at JsonAssets.Data.ObjectData.Recipe_.GetRecipeString(ObjectData parent) in G:\StardewValley\Mods\JsonAssets\Data\ObjectData.cs:line 60
   at JsonAssets.ContentInjector.Edit[T](IAssetData asset) in G:\StardewValley\Mods\JsonAssets\ContentInjector.cs:line 98
 ```
Solution: There is something missing from the recipe. This is caused by not installing a dependency or typing in an item ID/Name wrong. Install the dependencies (often listed on the download page) or open up the `.json` file and see if you typed something wrong.
 
### Exception Injecting Duplicate Key
```
Exception injecting cooking recipe for Bacon: System.ArgumentException: An item with the same key has already been added.
   at System.ThrowHelper.ThrowArgumentException(ExceptionResource resource)
   at System.Collections.Generic.Dictionary`2.Insert(TKey key, TValue value, Boolean add)
   at System.Collections.Generic.Dictionary`2.Add(TKey key, TValue value)
   at JsonAssets.ContentInjector.Edit[T](IAssetData asset) in G:\StardewValley\Mods\JsonAssets\ContentInjector.cs:line 99Exception i
 ```
 Solution: There is already an item with that name. This can happen when: using mods that have the same items, having two of the same file in different locations, or accidently naming something with the same name. Double check all folders and rename accordingly. 
 
## See Also

* [Nexus Page](https://www.nexusmods.com/stardewvalley/mods/1720)
* [FAQ](https://github.com/paradigmnomad/ppjajsonassetsfaq/blob/master/README.md)
