[Json Assets](https://github.com/spacechase0/JsonAssets) is a [Stardew Valley](http://stardewvalley.net/) mod which allows custom objects to be added to the game.
                                                                                                           
**This documentation is for modders. If you're a player, see the [Nexus page](https://www.nexusmods.com/stardewvalley/mods/1720) instead.**
                                                                                                           
## Contents
* [Install](#install)
* [Introduction](#introduction)
* [Basic Features](#basic-features)
  * [Overview](#overview)
  * [Common Fields](#common-fields)
  * [Big Craftables](#bigcraftables)
  * [Crops](#crops)
  * [Fruit Trees](#fruittrees)
  * [Objects](#objects)
    * [Crop and Fruit Tree Objects](#crop-and-fruit-tree-objects)
    * [Recipes](#recipes)
* [Converting From Legacy Format](#converting-from-legacy-format)
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

Examples of how to set up all types of objects can be found in the [Blank JSON Assets Template](https://www.nexusmods.com/stardewvalley/mods/1746). I also highly recommend looking up preexisting content packs for further examples:

* [Farmer to Florist](https://www.nexusmods.com/stardewvalley/mods/2075) contains examples of big craftables.
* [Starbrew Valley](https://www.nexusmods.com/stardewvalley/mods/1764) contains examples using all valid EdibleBuff fields.
* [Fantasy Crops](https://www.nexusmods.com/stardewvalley/mods/1610) contains examples of crops producing vanilla items.

### Companion Mods
Json Assets is a great tool if you want to add one of the above objects, but there are other frameworks out there that pair well with Json Assets:

 * [Custom Farming Redux](https://www.nexusmods.com/stardewvalley/mods/991) to add machines.

## Basic Features
### Overview
There are four main folders you are likely to see when downloading Json Asset content packs:
* BigCraftables
* Crops
* FruitTrees
* Objects

You will also see a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:SMAPI_APIs#Manifest) on the wiki).
Each of these folders contains subfolders that at minimum contains a `json` and a `png`. 

### Common Fields
All `json` files (excluding `manifest.json`) support these common fields:

field         | purpose
------------- | -------
`Name`        | The name you would like your object to have, this should be identical to the subfolder name.
`Price`       | How much your item sells for.

### BigCraftables
Big craftables are objects like scarecrows that are 16x32.

A big craftable subfolder is a folder with these files:
* a `big-craftable.json`;
* a `big-craftable.png`. Size: 16x32

The `big-craftable.json` contains these fields:

field                  | purpose
---------------------- | -------
  &nbsp;               | See _common fields_ above.
  `Description`        | Description for what this does. Note if it does anything special like provide light.
`ProvidesLight`        | On/Off switch for if it provides light or not. Set to `true` or `false`.
`Recipe`               | Begins the recipe block.
`ResultCount`          | How many of the product does the recipe produce.
`Ingredients`          | If using a vanilla object, you will have to use the [objects ID number](https://pastebin.com/TBsGu6Em). If using a custom object added by Json Assets, you will have to use the name. Ex. "Honeysuckle".
`Object` & `Count`     | Fields that are part of `Ingredients`. You can add up to five different ingredients to a recipe. `Object` fields that contain a negative value are the generic ID. Example: Rather than using a specific milk, -6 allows for any milk to be used.
`IsDefault`            | _(optional)_ Setting this to `true` will have the recipe already unlocked. Setting this to `false` (or excluding this field) will require additional fields specifiying how to obtain the recipe:
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. 
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.

### Crops

A crop subfolder is a folder with these files:
* a `crop.json`;
* a `crop.png`; Size: 128x32
* a `seeds.png`. Size: 16x16

field                      | purpose
-------------------------- | -------
  &nbsp;                   | See _common fields_ above.
`Product`                  | Determines what the crop produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the [objects ID number](https://pastebin.com/TBsGu6Em) and not include a corresponding `Objects` folder.
`SeedName`                 | The seed name of the crop. Typically crop name + seeds or starter.
`SeedDescription`          | Describe what season you plant these in. Also note if it continues to grow after first harvest and how many days it takes to regrow.
`Type`                     | Available types are `Flower`, `Fruit`, `Vegetable`, `Gem`, `Fish`, `Egg`, `Milk`, `Cooking`, `Crafting`, `Mineral`, `Meat`, `Metal`, `Junk`, `Syrup`, `MonsterLoot`, `ArtisanGoods`, and `Seeds`.
`Season`                   | Seasons must be in lowercase and in quotation marks, so if you want to make your crop last all year, you'd put in "spring", "summer", "fall", "winter". If you want to make winter plants, you will have to require [SpaceCore](http://www.nexusmods.com/stardewvalley/mods/1348) for your content pack.
`Phases`                   | Determines how long each phase lasts. Crops can have 2-5 phases, and the numbers in phases refer to how many days a plant spends in that phase. Seeds **do not** count as a phase.
`RegrowthPhase`            | If your plant is a one time harvest set this to `-1`. If it does, this determines how many days it takes for it to regrow. *Requires additional sprite at the end of the crop.png*
`HarvestWithScythe`        | Set to `true` or `false`.
`TrellisCrop`              | Set to `true` or `false`. Determines if you can pass through a crop or not. Flowers cannot grow on trellises and have colors.
`Colors`                   | Colors use RGBA for color picking, set to `null` if your plant does not have colors.
`Bonus`                    | This block determines the chance to get multiple crops.
`MinimumPerHarvest`        | Minimum number of crops you will get per harvest. Must be one or greater.
`MaximumPerHarvest`        | Maximum number of crops you will get per harvest. Must be one or greater. *Recommended not to exceed 10*.
`MaxIncreasePerFarmLevel`  | How many farming skill experience points you get from harvesting.
`ExtraChance`              | Value between 0 and 1.
`SeedPurchasePrice`        | How much you can purchase seeds for.
`SeedPurchaseFrom`         | Who you can purchase seeds from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. 
`SeedPurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `SeedPurchaseRequirements` set this to `null`.

### FruitTrees
Fruit trees added by Json Assets work a bit differently than vanilla fruit trees as you have to till/hoe the ground before planting them. This means they cannot be placed on the edges of the greenhouse like vanilla trees.

A fruit trees subfolder is a folder with these files:
* a `tree.json`;
* a `tree.png`; Size: 432x80
* a `sapling.png`. Size: 16x16

field                         | purpose
----------------------------- | -------
  &nbsp;                      | See _common fields_ above.
`Product`                     | Determines what the fruit tree produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the [objects ID number](https://pastebin.com/TBsGu6Em) and not include a corresponding `Objects` folder.
`SaplingName`                 | The name of the sapling, typically product + sapling.
`SaplingDescription`          | The description of the sapling, often sticks to vanilla format: Takes 28 days to produce a mature `product` tree. Bears `type` in the summer. Only grows if the 8 surrounding \"tiles\" are empty.
`Season`                      | Season must be in lowercase and in quotation marks. Fruit trees can support only one season. If you want to make winter fruit trees, you will have to require [SpaceCore]
`SaplingPurchasePrice`        | Determines how much the sapling can be purchased for.
`SaplingPurchaseFrom`         | Who you can purchase saplings from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. 
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
  &nbsp;                      | See _common fields_ above.
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
  &nbsp;               | See _common fields_ above.
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
`PurchaseFrom`         | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. 
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.

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

## See Also

* [Nexus Page](https://www.nexusmods.com/stardewvalley/mods/1720)
