[Json Assets](https://github.com/spacechase0/JsonAssets) is a [Stardew Valley](http://stardewvalley.net/) mod which allows custom objects to be added to the game.
                                                                                                           
**This documentation is for modders. If you're a player, see the [Nexus page](https://www.nexusmods.com/stardewvalley/mods/1720) instead.**
                                                                                                           
## Contents 
* [Install](#install)
* [Introduction](#introduction)
* [Basic Features](#basic-features)
  * [Overview](#overview)
  * [Big Craftables](#bigcraftables)
    * [Machine Animations](#machine-animations)
  * [Crops](#crops)
    * [Giant Crops](#giant-crops)
  * [Fruit Trees](#fruittrees)
  * [Objects](#objects)
    * [Crop and Fruit Tree Objects](#crop-and-fruit-tree-objects)
    * [Recipes](#recipes)
  * [Hats](#hats)
  * [Weapons](#weapons)
  * [Shirts & Pants](#shirts-and-pants)
    * [Shirts](#shirts)
    * [Pants](#pants)
  * [Boots](#boots)
  * [Tailoring](#tailoring)
* [Gift Tastes](#gift-tastes)
* [Context Tags](#context-tags)
* [Localization](#localization)
* [Content Patcher API](#content-patcher-api)
* [Tokens in Fields](tokens-in-fields)
* [Converting From Legacy Format](#converting-from-legacy-format)
* [Releasing A Content Pack](#releasing-a-content-pack)
* [Troubleshooting](#troubleshooting)
  * [Target Out of Range](#target-out-of-range)
  * [Exception Injecting Given Key](#exception-injecting-given-key)
  * [Exception Injecting Duplicate Key](#exception-injecting-duplicate-key)
  * [Previous Clothing Items Gone](#previous-clothing-items-gone)
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
* Weapons (16x16)
* Shirts & Pants
* Boots (16x16)
* Tailoring Recipes

Examples of how to set up all types of objects can be found in the [PPJA Resource Collection](https://www.nexusmods.com/stardewvalley/mods/4590). I also highly recommend looking up preexisting content packs for further examples:

* [Farmer to Florist](https://www.nexusmods.com/stardewvalley/mods/2075) contains examples of big craftables.
* [Starbrew Valley](https://www.nexusmods.com/stardewvalley/mods/1764) contains examples using all valid EdibleBuff fields.
* [Fantasy Crops](https://www.nexusmods.com/stardewvalley/mods/1610) contains examples of crops producing vanilla items.
* [PPJA Home of Abandoned Mods](https://www.nexusmods.com/stardewvalley/mods/3374) contains examples of hats, weapons, and clothing.

### Companion Mods
Json Assets is a great tool if you want to add one of the above objects, but there are other frameworks out there that pair well with Json Assets:

 * [Producer Framework Mod](https://www.nexusmods.com/stardewvalley/mods/4970) to add machines.
 * [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915).
 * [Better Artisan Good Icons](https://www.nexusmods.com/stardewvalley/mods/2080) to customize the appearance of artisan products.
 * [Mail Framework Mod](https://www.nexusmods.com/stardewvalley/mods/1536) to send objects & cooking/crafting recipes.
 * [Shop Tile Framework](https://www.nexusmods.com/stardewvalley/mods/5005) to add shops easier with full JA pack support.

## Basic Features
### Overview
There are nine main folders you are likely to see when downloading Json Asset content packs:
* BigCraftables
* Crops
* FruitTrees
* Objects 
* Hats
* Weapons
* Shirts
* Pants
* Boots
* Tailoring

You will also see a `manifest.json` for SMAPI to read (see [content packs](https://stardewvalleywiki.com/Modding:SMAPI_APIs#Manifest) on the wiki).
Each of these folders contains subfolders that at minimum contains a `json` and a `png`. 

### BigCraftables
Big craftables are objects like scarecrows that are 16x32.

A big craftable subfolder is a folder with these files:
* a `big-craftable.json`;
* a `big-craftable.png`; Size: 16x32

The `big-craftable.json` contains these fields:

field                    | purpose
-------------------------| -------
`Name`                   | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                  | How much your item sells for.
`Description`            | Description for what this does. Note if it does anything special like provide light.
`ProvidesLight`          | On/Off switch for if it provides light or not. Set to `true` or `false`.
`Recipe`                 | Begins the recipe block.
`ResultCount`            | How many of the product does the recipe produce.
`Ingredients`            | If using a vanilla object, you will have to use the objects ID number. If using a custom object added by Json Assets, you will have to use the name. Ex. "Honeysuckle".
`Object` & `Count`       | Fields that are part of `Ingredients`. You can add up to five different ingredients to a recipe. `Object` fields that contain a negative value are the generic ID. Example: Rather than using a specific milk, -6 allows for any milk to be used.
`IsDefault`              | _(optional)_ Setting this to `true` will have the recipe already unlocked. Setting this to `false` (or excluding this field) will require additional fields specifiying how to obtain the recipe:
`CanPurchase`            | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`           | Who you can purchase the recipe from. Valid entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. If an NPC isn't listed here they can't be used. `Pierre` is the default vendor.
`PurchasePrice`          | How much you can purchase the recipe for.
`PurchaseRequirements`   | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.
`SkillUnlockName`        | The name of the [skill](https://stardewvalleywiki.com/Skills) required for unlock.
`SkillUnlockLevel`       | The level, 1 - 10, required to unlock.
`ReserveNextIndex`       | _(optional)_ Used for animations with PFM. Set to `true` or `false`. Reserves 1 index. Useful for machines that work like the Charcoal Kiln. Cannot be used with `ReserveExtraIndexCount`.
`ReserveExtraIndexCount` | _(optional)_ Used for animations with PFM. Set to the number of additional frames needed. See [Machine Animations](#machine-animations) for more information. Cannot be used with `ReserveNextIndex`.
`EnableWithMod`          | _(optional)_ Enables the craftable when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`         | _(optional)_ Disables the craftable when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Big Craftables do not support gift tastes.

#### Machine Animations

`ReserveExtraIndexCount` is used primarily for big-craftable machines. It may also be useful for a SMPAI mod that utilizes chest animation. Unlike CFR, each frame of the machine will need to be it's own image. Starting with `big-craftble`, `big-craftable-2` `big-craftable-3` and so on. `big-craftable` (no numbers) is considered to be 0 in the index. So for our example of the Alembic, there is the starting frame and then 7 additional frames afterwards for the animation.

Here is a preview of the folder contents
[Imgur](https://i.imgur.com/Du4WNM5.png)

Example:
```
{
    "Name": "Alembic",
    "Description": "Distills flowers, fruits, herbs, and vegetables into essential oils.",
    "Price": 1,
    "ProvidesLight": false,
    "ReserveExtraIndexCount": 7,
    "Recipe":
    {
        "ResultCount": 1,
        "Ingredients": [
        {
            "Object": 334,
            "Count": 5,
        },
        {
            "Object": 766,
            "Count": 50,
        },
        {
            "Object": 709,
            "Count": 10,
        }, ],
        "CanPurchase": false,
    },
}
```

If you want an image to constantly animate you will need to use the [Content Patcher API](#content-patcher-api).

<details>
  <summary> <b>Expand for more information on PFM useage </b> </summary>

When using with PFM in the `producersConfig.json` this information would translate to:

```
{
    "ProducerName": "Alembic", 
    "AlternateFrameProducing": false, 
    "AlternateFrameWhenReady": false, 
    "DisableBouncingAnimationWhileWorking": true, // Disables defualt bouncing animation
    "ProducingAnimation": { 
            "RelativeFrameIndex": [1,2,3,4,5,6], //big-craftable-2 through big-craftable-7
            "FrameInterval": 10 
        },
        "ReadyAnimation": 
        {
          "RelativeFrameIndex": [7], // big-craftable-8
      },
  },
```

This is mentioned because JA & PFM indexs are one off of each other. `big-craftable` is your idle animation. `big-craftable-2` through `big-craftable-7` are your `ProducingAnimation` `RelativeFrameIndex`. Finally `big-craftable-8` is your `ReadyAnimation` `RelativeFrameIndex`. You can have less or more than 8 `big-craftable` just keep in mind to bump each number down one.

</details>

### Crops

A crop subfolder is a folder with these files:
* a `crop.json`;
* a `crop.png`; Size: 128x32
* a `seeds.png`; Size: 16x16
* _(optional)_ a `giant.png`; Size: 48x63 See [Giant Crops](#giant-crops) for more information.

field                      | purpose
-------------------------- | -------
`Name`                     | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                    | How much your item sells for.
`Product`                  | Determines what the crop produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the objects ID number and not include a corresponding `Objects` folder.
`SeedName`                 | The seed name of the crop. Typically crop name + seeds or starter.
`SeedDescription`          | Describe what season you plant these in. Also note if it continues to grow after first harvest and how many days it takes to regrow.
`Type`                     | Vanilla types are `Flower`, `Fruit`, `Vegetable`, `Gem`, `Fish`, `Egg`, `Milk`, `Cooking`, `Crafting`, `Mineral`, `Meat`, `Metal`, `Junk`, `Syrup`, `MonsterLoot`, `ArtisanGoods`, `AnimalGoods`, `Greens`, and `Seeds`. 
`CropType`                 | Available types are `Normal`, `IndoorsOnly`, and `Paddy`. If no `CropType` is specified (largely affecting pre-SDV1.4 crops) `Normal` is the default. `IndoorsOnly` means it can only grow when inside (greenhouse or garden pot). `Paddy` means it follows the same rules as rice (SDV1.4) and does not need watered if planted around a water source.
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
`SeedPurchaseFrom`         | Who you can purchase seeds from. Valid vanilla entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. You can also use a custom NPC as a vendor. `Pierre` is the default vendor.
`SeedPurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `SeedPurchaseRequirements` set this to `null`.
`EnableWithMod`            | _(optional)_ Enables the crop when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`           | _(optional)_ Disables the crop when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

**Facts about Custom Crops**:
* Sprites are 32px tall and there are 2 per row. Vanilla `Tilesheets\crops` is 256 x 672 px
* JA starts numbering crops at ID 100, and the first sprites are placed at 0,1600.
* The crop limit has been removed as of v.1.4.0.

#### Giant Crops

Giant crops work the same way as vanilla giant crops. It is not recommended to make regrowable crops have a giant variant as once they become giant and are harvested they will not replant themselves. This is not a bug and is intended behavior. Mods that include giant regrowable crops should include a disclaimer so users are aware that they may lose their regrowing crops. Below is a sample disclaimer created by SpringsSong:

"Giant Crops were never meant to be regrown, they were meant to be a one-off of the crop when the proper conditions were met. If you use the regrowing crops variant of these giant crops, you will lose your crops when you harvest them. This is intentional, not a bug, and will not be fixed."

Giant crops are 48x63. Custom giant crops need to be placed inside the corresponding `Crops` folder and named `giant.png`.


### FruitTrees

A fruit trees subfolder is a folder with these files:
* a `tree.json`;
* a `tree.png`; Size: 432x80
* a `sapling.png`; Size: 16x16

field                         | purpose
----------------------------- | -------
`Name`                        | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                       | How much your item sells for.
`Product`                     | Determines what the fruit tree produces. This will correspond to a folder with the same name in `Objects` (ex. Both folders will be named "Honeysuckle"). _(optional)_ You can produce vanilla items. Instead of a named object you will use the objects ID number and not include a corresponding `Objects` folder.
`SaplingName`                 | The name of the sapling, typically product + sapling.
`SaplingDescription`          | The description of the sapling, often sticks to vanilla format: Takes 28 days to produce a mature `product` tree. Bears `type` in the summer. Only grows if the 8 surrounding \"tiles\" are empty.
`Season`                      | Season must be in lowercase and in quotation marks. Fruit trees can support only one season. If you want to make winter fruit trees, you will have to require [SpaceCore]
`SaplingPurchasePrice`        | Determines how much the sapling can be purchased for.
`SaplingPurchaseFrom`         | Who you can purchase saplings from. Valid vanilla entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. You can also use a custom NPC as a vendor.`Pierre` is the default vendor.
`SaplingPurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `SaplingPurchaseRequirements` set this to `null`.
`EnableWithMod`               | _(optional)_ Enables the fruit tree when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`              | _(optional)_ Disables the fruit tree when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

**Facts about Custom Trees**:
* Sprites are 80px tall and there is only 1 tree per row. Vanilla `Tilesheets\fruitTrees` has partial sprites for a 7th tree and is 432 x 560 px
* JA starts numbering its trees at ID 10, and the first sprites are placed at 0,800.
* The tree limit has been removed as of v.1.4.0.

### Objects
#### Crop and Fruit Tree Objects
Unless your crop or fruit tree is producing a vanilla item, it will need to have a corresponding folder in `Objects`

An object subfolder for crops & fruit trees is a folder that contains these files:

* an `object.json`;
* an `object.png`; Size: 16x16
* _(optional)_ a `color.png`; Size: 16x16, this will be a grayscale version of the part you want colored. *[See Mizu's Flowers](https://www.nexusmods.com/stardewvalley/mods/2028) for an example*.

field                         | purpose
----------------------------- | -------
`Name`                        | The name you would like your object to have, this should be identical to the subfolder name.
`Price`                       | How much your item sells for.
`Description`                 | Description of the product.
`Category`                    | This should match the `crop.json` `Type` or for fruit trees use one of the following categories: `Flower`, `Fruit`, `Vegetable`, `Gem`, `Fish`, `Egg`, `Milk`, `Cooking`, `Crafting`, `Mineral`, `Meat`, `Metal`, `Junk`, `Syrup`, `MonsterLoot`, `ArtisanGoods`, `Greens`, `AnimalGoods` and `Seeds`.
`CategoryTextOverride`        | _(optional_) Visually allows you to alter what category the item appears as. Examples include: `herb`, `spice`, `hybrid`.
`CategoryColorOverride`       | _(optional)_ Works the same as `Colors` field using RGBA, but only allows one input. Alters the text color of the category.
`Edibility`                   | Edibility is for health, energy is calculated by the game. For inedibile items, set to -300.
`IsColored`                   | _(optional)_ Set this value to `true` if your product is colored.
`Recipe`                      | Set to `null`.
`EnableWithMod`               | _(optional)_ Enables the object when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`              | _(optional)_ Disables the object when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

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
`Ingredients`          | If using a vanilla object, you will have to use the objects ID number. If using a custom object added by Json Assets, you will have to use the name. Ex. "Honeysuckle".
`Object` & `Count`     | Fields that are part of `Ingredients`. You can add up to five different ingredients to a recipe. `Object` fields that contain a negative value are the generic ID. Example: Rather than using a specific milk, -6 allows for any milk to be used.
`IsDefault`            | _(optional)_ Setting this to `true` will have the recipe already unlocked. Setting this to `false` (or excluding this field) will require additional fields specifiying how to obtain the recipe:
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the recipe from. Valid vanilla entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. You can also use a custom NPC as a vendor. `Pierre` is the default vendor.
`PurchasePrice`        | How much you can purchase the recipe for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.
`SkillUnlockName`      | The name of the [skill](https://stardewvalleywiki.com/Skills) required for unlock.
`SkillUnlockLevel`     | The level, 1 - 10, required to unlock.
`EnableWithMod`        | _(optional)_ Enables the recipe when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the recipe when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

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
`EnableWithMod`        | _(optional)_ Enables the hat when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the hat when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Hats do not support gift tastes.

### Weapons
Weapons are 16x16 and can be added via Json Assets through the `Weapons` folder. 

A weapon subfolder is a folder that contains these files:

* a `weapon.json`;
* a `weapon.png`;

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your object to have, this should be identical to the subfolder name.
`Description`          | Description of the product.
`Type`                 | Depending on the weapon set this to one of the following: `sword`, `dagger`, or `club`. `Slingshot` is untested.
`MinimumDamage`        | The minimum number of damage points an enemy hit with this weapon will receive.
`MaximumDamage`        | The maximum number of damage points an enemy hit with this weapon will receive.
`Knockback`            | How far the enemy will be pushed back from the player after being hit with this weapon.
`Speed`                | How fast the swing of the weapon is.
`Accurary`             | How accurate the weapon is.
`Defense`              | When blocking, how much protection it provides.
`MineDropVar`          | 
`MineDropMinimumLevel` | The first level the weapon can drop when in the mines.
`ExtraSwingArea`       |
`CritChance`           | The chance the weapon will land a critical hit.
`CritMultiplier`       | Damage multiplied by this number is how much damage a critical hit does.
`CanPurchase`          | Set this to `true` if `IsDefault` is set to `false` or excluded from the `json`.
`PurchaseFrom`         | Who you can purchase the weapon from. Valid vanilla entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. You can also use a custom NPC as a vendor. `Pierre` is the default vendor. For weapons, `Marlon` is recommended.
`PurchasePrice`        | How much you can purchase the weapon for.
`PurchaseRequirements` | See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.
`EnableWithMod`        | _(optional)_ Enables the weapon when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the weapon when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Weapons do not support gift taste.

### Shirts and Pants

"Shirts and pants simply exist right now without recipes." {spacechase0) As of JA v1.4, shirts & pants added will have to be spawned in using [CJB Item Spawner](https://www.nexusmods.com/stardewvalley/mods/93). You can use [Shop Tile Framework](https://www.nexusmods.com/stardewvalley/mods/5005) or [TMXLoader](https://www.nexusmods.com/stardewvalley/mods/1820) to create a custom shop to sell clothing.

#### Shirts
Shirts are 8x32 and can be added via Json Assets through the `Shirts` folder.

A shirt subfolder is a folder that contains these files:

* _(optional)_ a `female.png`; 
* a `male.png`;
* _(optional)_ a `male-color.png`. Size: 16x16, this will be a grayscale version of the part you want colored. *[See Mizu's Flowers](https://www.nexusmods.com/stardewvalley/mods/2028) for an example*.
* _(optional)_ a `female-color.png`. Size: 16x16, this will be a grayscale version of the part you want colored. *[See Mizu's Flowers](https://www.nexusmods.com/stardewvalley/mods/2028) for an example*.
* a `shirt.json`;

`female.png` and `male.png` can be identical sprites.

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your shirt to have, this should be identical to the subfolder name. Shirts have a standard naming format. [PackName-Shirt(Number)] ex. `ParadigmNomadClothing-Shirt1`
`Description`          | Description of the product.
`HasFemaleVariant`     | Select `true` or `false`.
`Price`                | How much the item sells for.
`Dyable`               | Can the clothing item be dyed. Set to `true` or `false`.
`DefaultColor`         | Colors use RGBA for color picking. Remove if not being used. Can only have one color option.
`EnableWithMod`        | _(optional)_ Enables the shirt when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the shirt when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs. 

Shirts do not support gift tastes. Shirts do not support context tags. Shirts added this way will also not show up in the character creation screen.

#### Pants
Pants are 192x688 and can be added via Json Assets through the `Pants` folder.

The left portion of the image (96x672) is for male characters. The right portion of the image (96x672) is for female characters. You will need both filled out even if it is the same for both male and female.
Underneath the male portion of the image, there is a 16x16 square in the bottom left corner. This is the preview image of the pants that appears in the players inventory.

A pants subfolder is a folder that contains these files:

* a `pants.json`;
* a `pants.png`;

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your shirt to have, this should be identical to the subfolder name.
`Description`          | Description of the product.
`Price`                | How much the item sells for.
`Dyable`               | Can the clothing item be dyed. Set to `true` or `false`.
`DefaultColor`         | Colors use RGBA for color picking. Remove if not being used. Can only have one color option. Default color for pants is `255, 235, 203, 255`
`EnableWithMod`        | _(optional)_ Enables the pants when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the pants when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Pants to do not support gift tastes. Pants do not support context tags. Pants added this way will also not show up in the character creation screen.

### Boots

Boots are 16x16 and can be added via Json Assets through the `Boots` folder.

A boots subfolder is a folder that contains these files:

* a `boots.json`;
* a `boots.png`;
* a `color.png`; 

The `color.png` is a horizonal strip that is 1px high, that contains all the colors used in the `boots.png`.

field                  | purpose
---------------------- | -------
`Name`                 | The name you would like your boots to have, this should be identical to the subfolder name.
`Description`          | Description of the shoes.
`Price`                | How much the item sells for.
`Defense`              | How much resistance the boots provide.
`Immunity`             | How much immunity the boots provide.
`PurchaseFrom`         | _(optional)_ Who you can purchase the weapon from. Valid vanilla entries are: `Willy`, `Pierre`, `Robin`, `Sandy`, `Krobus`, `Clint`, `Harvey`, `Marlon`, and `Dwarf`. You can also use a custom NPC as a vendor. `Marlon` is the default vendor.
`PurchasePrice`        | _(optional)_ How much you can purchase the boots for.
`PurchaseRequirements` | _(optional)_ See [Event Preconditions](https://stardewvalleywiki.com/Modding:Event_data#Event_preconditions). If you do not want to have any `PurchaseRequirements` set this to `null`.
`EnableWithMod`        | _(optional)_ Enables the boots when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the boots when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Boots do not support gift tastes. 

### Tailoring

Tailoring is the recipe used to craft (tailor) a shirt or pants.

A tailoring subfolder is a folder that contains these files:

* a `recipe.json`;

field                  | purpose
---------------------- | -------
`FirstItemTags`        | Prefix'd with `item_` Specifys the name of the first item to be used.
`SecondItemTags`       | Prefix'd with `item_` Specifys the name of the second item to be used.
`ConsumeSecondItem`    | Removes the `SecondItemTags` item from the players inventory. Can be set to `true` or `false`.
`CraftedItems`         | The name of the shirt/pants being produced. 
`EnableWithMod`        | _(optional)_ Enables the tailoring recipe when a specific mod is installed. Example: `"EnableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.
`DisableWithMod`       | _(optional)_ Disables the tailoring recipe when a specific mod is installed. Example: `"DisableWithMod": "ppja.moretrees"`. Does not support multiple uniqueIDs.

Tailoring does not support localization.
Below is a bit more about item tag names from Mr. Podunkian.

"It's `item_itemname` where `itemname` is the item's name in all lowercase, with spaces replaced with \_'s and ' (apostrophe) removed. So mermaid's pendant would be `item_mermaids_pendant`. They have an alternative id, which is `id_(o for normal objects)_(id number)`. Typing in `debug listtags` into SMAPI [will] print out all of the context tags for that item. You need to use the alt ID for any items that might have name collisions."

## Gift Tastes

You can add gift taste support to any pre-existing content pack by adding the following to the respective `.json` file. It does not matter where you put it. I tend to place it at the bottom of the `.json` but it is personal preferance. 

If it can be gifted to an NPC it has gift taste support built in. This means `hats`, `big-craftables`, `weapons`, `shirts`, `pants`, `boots` and `tailoring` do not have gift taste support. If you exclude an NPC from the gift taste, their reaction will default to `Neutral`.

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

## Context Tags

"Context tags are an array in the item "ContextTags", injected into Data\ObjectContextTags". It allows mods like [Better Shop Menu](https://www.nexusmods.com/stardewvalley/mods/2012) to categorize your items better. This is an optional feature and not required for a content pack to work. 

Example:

```
"ContextTags": 
        [
        "season_summer",
        "color_yellow",
        "fruit_tree_item",
        "fruit_item"
        ],
```

You can include as much or as little information you want to with context tags.
Common information in context tags are: season, main color, what produces the item, and what type the item is.

Here is a [link](https://pastebin.com/5F66hZh1) to 1.3.36 context tags. An alternative way to check a pre-exisiting items context tags is "Typing in `debug listtags` into SMAPI [will] print out all of the context tags for that item." (Mr. Podunkian) You aren't limited to those context tags, but it gives you an idea of the vanilla context tags.

## Localization

JsonAssets supports name localization without the need for a seperate or different download. These lines can be added to the bottom of their respective `json` files. Most localization is the same except "Crops have their localization fields prefixed with `Seed`, fruit trees prefixed with `Sapling`."

Examples:

For Anything not a crop/sapling:
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
    "SaplingNameLocalization": { "es": "spanish tree (name)" },
    "SaplingDescriptionLocalization": { "es": "spanish tree (desc)" }
```

PPJA has put together some [translation templates](https://github.com/paradigmnomad/PPJA/wiki/Submitting-a-Translation#translation-guide) that we strongly encourage users to use as a way to standardize how translations are done.

## Content Patcher API

As of [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) 1.12 we can now target assets created by JA. Currently supported categories are:

* Object;
* Crop;
* FruitTree;
* BigCraftable;
* Hat;
* Weapon;

These tokens will now have a `___SpriteTilesheet` and `___Sprite(X|Y)`. "You should always use the `__SpriteTilesheet` tokens for `Target` because of the expanded tilesheet stuff.

Example:
```
        {
            "LogName": "Test JA rectangle tokens",
            "Action": "EditImage",
            "Target": "{{spacechase0.JsonAssets/CropSpriteTilesheet:Honeysuckle}}",
            "FromFile": "Penny_Spring_Indoor.png",
            "FromArea": { "X": "0", "Y": "0", "Width": "16", "Height": "32" },
            "ToArea": { "X": "{{spacechase0.JsonAssets/CropSpriteX:Honeysuckle}}", "Y": "{{spacechase0.JsonAssets/CropSpriteY:Honeysuckle}}", "Width": "16", "Height": "32" },
            "AnimationFrameTime": 4,
            "AnimationFrameCount": 4
        },
```

Below is some more information on the newly added fields.

field                  | purpose
---------------------- | -------
`AnimationFrameTime`   | _(optional)_ Frames per second. For machine animations, 1-3 appears to work the best.
`AnimationFrameCount`  | _(optional)_ How many frames the image had.


## Tokens in Fields

[Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915) can use Json Assets as tokens. An example of this would be sending an `object` through a mail. Note: You cannot send cooking recipes via Content Patcher. You will need to use the [Mail Framework Mod](https://www.nexusmods.com/stardewvalley/mods/1536) to send cooking recipes. Mail Framework Mod is recommended if you're sending multiple types of objects as users will only have to install one additional dependency.

Example:

```
        {
            "LogName": "Letters - Mizu's Flowers",
            "Action": "EditData",
            "Target": "Data/Mail",
            "Entries":
            {
                    "[{{UNIQUEID}}]": "Dear @,^^ Here's some seeds from the little garden I keep out back. You probably already have some of these but they make a great tea.^^  -Caroline %item object {{spacechase0.JsonAssets/ObjectId:[{{OBJECT NAME}}] [{{QUANTITY}}] %%",
            },
        },
```

Make sure to list the Json Assets pack as a dependency in your `manifest`.

## Converting From Legacy Format
Before the release of SMAPI 2.5, Json Assets content packs previously needed a `content-pack.json` and had to be installed directly in the Json Assets folder. This is an outdated method and the more current `manifest.json` method should be used.

To learn how to set up a `manifest.json` please visit the [wiki page](https://stardewvalleywiki.com/Modding:SMAPI_APIs#Manifest). An example `manifest.json` specifically for Json Assets is included below:

```
{
    "Name": "Mizu's Flowers for JsonAssets",
    "Author": "ParadigmNomad & Eemie (Port) & Mizu (Sprites)",
    "Description": "A port of Mizu's sprites for JsonAssets.",
    "Version": "1.4",
    "UniqueID": "Mizu.Flowers",
    "ContentPackFor":
    {
        "UniqueID": "spacechase0.JsonAssets"
    },
    "UpdateKeys": ["Nexus:2028"],
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

There are some common errors with easy solutions. Your error may look slightly different but the general principal is the same. For a more in depth FAQ visit [this](https://github.com/paradigmnomad/PPJA/wiki/Troubleshooting) link. FAQ is a work in progress.

### Target Out of Range
```
   Exception injecting crop sprite for Blue_Mist: System.ArgumentOutOfRangeException: The target area is outside the bounds of the target texture.
   Parameter name: targetArea
   at StardewModdingAPI.Framework.Content.AssetDataForImage.PatchImage(Texture2D source, Nullable`1 sourceArea, Nullable`1 targetArea, PatchMode patchMode) in C:\source\_Stardew\SMAPI\src\SMAPI\Framework\Content\AssetDataForImage.cs:line 44
   at JsonAssets.ContentInjector.Edit[T](IAssetData asset) in G:\StardewValley\Mods\JsonAssets\ContentInjector.cs:line 194
```
Solution: The sprite is too big. Double check what size the image needs to be for that specific type of item and crop your image accordingly. 

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

 ### Previous Clothing Items Gone

 Solution: If you have previously used clothing added via Content Patcher it will show as a blank object. Clicking on this item will make it disappear but your menu keys may lock up. Clicking `X` close to the dresser on screen works to close the menu. (Courtesy of minervamaga)

 It is recommended you remove any Content Patcher mods that are now being handled by Json Assets before adding in the Json Assets version to avoid this.
 
## See Also

* [Nexus Page](https://www.nexusmods.com/stardewvalley/mods/1720)
* [FAQ](https://github.com/paradigmnomad/PPJA/wiki/Troubleshooting)
* [PPJA Resource Collection](https://www.nexusmods.com/stardewvalley/mods/4590)
* Facts courtesy of MouseyPounds & Mr. Podunkian
