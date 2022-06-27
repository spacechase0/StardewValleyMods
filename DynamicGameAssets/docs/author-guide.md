﻿[← back to readme](README.md)

# Dynamic Game Assets

This documentation is for making mods; for using Dynamic Game Assets as a user, please check the [Nexus](https://www.nexusmods.com/stardewvalley/mods/9365) page.

Contents
* [Differences from Json Assets](#differences-from-json-assets)
* [Useful Commands](#useful-commands)
* [manifest.json](#manifestjson)
* [Localization](#localization)
* [Common Field Types](#common-field-types)
    * [Texture](#texture)
    * [Texture Animation](#texture-animation)
    * [MultiTexture](#multitexture)
    * [Item](#item)
    * [WeightedItem](#weighteditem)
    * [DynamicField](#dynamicfield)
* [Pack Data](#pack-data)
    * [Common](#common)
    * [Config Schema](#config-schema)
        * [Label](#label)
        * [Paragraph](#paragraph)
        * [Image](#image)
        * [Config Option](#config-option)
    * [Content Index](#content-index)
    * [Big Craftables](#big-craftables)
    * [Boots](#boots)
    * [Crafting Recipes](#crafting-recipes)
        * [Special Types](#special-types)
            * [Ingredient](#ingredient)
    * [Crops](#crops)
        * [Special Types](#special-types)
            * [Phase](#phase)
            * [HarvestedDrop](#harvesteddrop)
    * [Fences](#fences)
    * [Forge Recipe](#forge-recipe)
    * [Fruit Trees](#fruit-trees)
    * [Furniture](#furniture)
        * [Special Types](#special-types)
            * [FurnitureConfiguration](#furnitureconfiguration)
    * [Hats](#hats)
    * [Machine Recipes](#machine-recipes)
    * [Melee Weapons](#melee-weapons)
    * [Objects](#objects)
        * [Special Types](#special-types)
            * [FoodBuffs](#foodbuffs)
    * [Gift Tastes](#gift-tastes)
    * [Pants](#pants)
    * [Shirts](#shirts)
    * [Shop Entries](#shop-entries)
    * [Tailoring Recipes](#tailoring-recipes)
    * [Texture Overrides](#texture-overrides)
    * [Extra Information](#extra-information)
        * [Vaild Shop IDs for Vanilla](#valid-shop-ids-for-vanilla)
* [Dynamic Fields](#dynamic-fields)

## Differences from Json Assets
This is a list of differences *as pertains to making content packs*. For a full list of differences, see the [Nexus](https://www.nexusmods.com/stardewvalley/mods/9365) page.

* JA crops that regrow work on the amount of days until they can be harvested (yes, really, I just double-checked and tested). However, in DGA, you can only revert to a different phase. Because of this, the converter will only revert regrow crops to their previous phase. You can reconfigure this to a different phase if you want.
* Shop entries are calculated at the beginning of the day instead of when the shop is opened. This means certain conditions (such as time based ones) might not work, or behave oddly.
* Crops and fruit trees do not have seeds/saplings created automatically; instead, create an object with specify the crop/fruit tree using the `Plants` field.
* Crops and fruit trees do not have a `"Season"` field anymore; instead the `"CanGrowNow"` field must be set using dynamic fields. An example is shown in the (Crop)[#] section.
* Crops can have as many phases are you want.
* Crops do not have `"MaxIncreasePerFarmLevel"`. I think this can be done using [Dynamic Fields](#dynamicfield) (though I haven't tested it yet).
* Internationalization is done through the `i18n` folder now.
* Tilesheets are supported. See the [Texture field type](#texture) section.
* Everything goes in or is referenced by `content.json` at the root of the content pack; this is to make it so overwriting folders does not result in items that should no longer be present. (In Json Assets, you had to delete the mod and reinstall it to avoid this behavior.)
* Rings are not supported.
 * This is due to the fact that you need SMAPI code to implement their effects anyways; at that point, you can just use a custom subclass of `Ring` and the SpaceCore serialization API like this mod does.
* Similarly, there is no longer a code API for dynamically registering items. You can just subclass things and register it with the SpaceCore serializer like this mod does, or subclass `ContentPack`, add your items to it, and then register your content pack directly with the mod.
* Maybe more I forgot.

## Useful commands
* `dga_list` - List the items in all content packs.
* `dga_add <mod.id/ItemId> [quantity]` - Add the specified item to your inventory (in the specified amount, if possible for the given item type).
* `dga_reload` - Reload all content packs.
* `dga_store [mod.id]` - Get a store containing all currently enabled items (optionally from a speicfic pack).
* `dga_force` - Force-update enable conditions and dynamic fields. Useful if you used debug commands to change the season and want to test fruit trees, for example.

## manifest.json
For your content pack, you need a normal SMAPI content pack manifest.json. However, you need two additional fields, `"DGA.FormatVersion"` (currently 2) and `"DGA.ConditionsFormatVersion"` (matching the Content Patcher version for the conditions you want to use). Example:

```json
{
    "Name": "Dynamic Game Assets Example Content Pack",
    "Author": "spacechase0",
    "Version": "1.0.0",
    "Description": "Example pack for DGA",
    "UniqueID": "spacechase0.DynamicGameAssets.Example",
    "ContentPackFor": { "UniqueID": "spacechase0.DynamicGameAssets" },
    "UpdateKeys": [],

    "DGA.FormatVersion": 2,
    "DGA.ConditionsFormatVersion": "1.23.0"
  }
  ```

## Localization
DGA uses the standard SMAPI localization format. That is, in the `i18n` folder, create a `default.json` for english, and then `lang.json` for other languages (replacing `lang` with your language code). Then, use the key-value format. Here is an example:

```json
{
  "object.Mysterious Circle.name": "A mysterious circle",
  "object.Mysterious Circle.description": "A circle with mysterious qualities",
}
```

## Common Field Types
(Note: `bool` means `true` or `false`.)
(Note: `Vector2` is an `X` and `Y` coordinate, or `"X, Y"`.)

A few of the fields have special types with subfields. Here they are.

### Texture
This field describes the texture something will be using. Typically, it will include a size, such as `Texture[16, 16]` or `Texture[16, 32]`. Here is the format for the value of this field:

```json
"file_name"
OR
"file_name:index"
```

`file_name` is pretty obvious; it's the filename of the texture. `index` is where in the file it is, based on the size of the object searching the texture. If the texture size is (16, 32), then it will search in (16, 32) sized blocks to the right, until it reaches the right side of the texture. Then, it will go down a row and repeat, until it reaches the end of the texture. (This means you can't have two different sized objects in the same file, such as a horizontal and vertical rug.) If the `index` is not specified, the top left of the file will be used.

### Texture Animation
You can optionally animate any `Texture` field. A basic animation consists of a comma-separated list of frames, each in the form `<image source>[:<frame>][@<duration>]` (like `objects.png:0@5`):

field          | effect
-------------- | ------
`image source` | The name of the image file which contains the animation frame; see [_Texture_](#texture).
`frame`        | The sprite index within the image source; see [_Texture_](#texture). If omitted, defaults to `0`.
`duration`     | how long the animation frame should be drawn before switching to the next frame, measured in game ticks. Each tick is 1/60th of a second. If omitted, defaults to one tick.

For example:
```js
// draw index 0 for 5 ticks, index 1 for 10 ticks, index 2 for 5 ticks, then repeat
"objects.png:0@5, objects.png:1@10, objects.png:2@5"
```

As a shortcut, you can specify a sequence of animated frames:
```js
// draw indexes 0 to 5 (inclusive) for 5 ticks each
"objects.png:0..5@5"
```

### MultiTexture
Sometimes, you can specify multiple textures instead of one. This will change which texture is used based on in-game conditions. An example of this being used in the vanilla game is with the seed phase of crops; they all have two seed sprites, but only one is used on each tile, depending on where the crop is.

This is simply an array of `Texture`, ie.:

```json
[ "crops.png:0", "crops.png:1" ]
```

### Item
There are multiple types of items in the game, and specifying them can be tricky. Sometimes, you may want a vanilla weapon or big craftable, or other times you'll want one of your custom items. For `Item`s, you'll need to specify a `Type`, and `Value` (which depends on the `Type`). Here are a few examples:

```jsonc
{
    "Type": "VanillaObject",
    "Value": 74, // ID of prismatic shard
},
{
    "Type": "VanillaObject",
    "Value": "Bat Wing", // Name of the item also works
},
{
    "Type": "DGAItem",
    "Value": "mod.id/YourItemID",
}
```

Here is the full list of supported `Type` values:

| Type | Description | Accepted Values |
| --- | --- | --- |
| `DGAItem` | A custom item from any content pack for Dynamic Game Assets. | Specify the content pack's ID, followed by a `/`, followed by the item ID, ie. `mod.id/YourItemID` |
| `DGARecipe` | A custom recipe from any content pack for Dynamic Game Assets. | Specify the content pack's ID, followed by a `/`, followed by the item ID, ie. `mod.id/YourCraftingRecipeID` |
| `VanillaObject` | A vanilla game object, from `Data/ObjectInformation`. | Either the object numeric ID, or the object name. |
| `VanillaObjectColored` | A vanilla game object, from `Data/ObjectInformation`. This should be used instead of `VanillaObject` for objects that use a color texture overlay as well, such as flowers. | Either the object numeric ID, or the object name. |
| `VanillaBigCraftable` | A vanilla game big craftable, from `Data/BigCraftablesInformation`. | Either the big craftable numeric ID, or the big craftable name. |
| `VanillaWeapon` | A vanilla melee weapon, from `Data/weapons`. | Either the weapon numeric ID, or the weapon name. |
| `VanillaHat` | A vanilla hat, from `Data/hats`. | Either the hat numeric ID, or the hat name. |
| `VanillaClothing` | A vanilla clothing item, from `Data/ClothingInformation`. | Either the clothing numeric ID, or the clothing name. |
| `VanillaBoots` | A vanilla boots item, from `Data/Boots`. | Either the boots numeric ID, or the boots name. |
| `VanillaFurniture` | A vanilla furniture item, from `Data/Furniture`. | Either the furniture numeric ID, or the furniture name. |
| `ContextTag` | Items matching a context tag. This is only usable in recipe ingredients; Using it as something to be actually created will result in an error. | The context tag. |
| `Custom` | An item of a custom C# class type. See note 3. | The full type namespace and name, followed by a slash, followed a string for the constructor, ie. package.name.ClassName/arg |

If an object has a color overlay, such as a DGA Object with a `TextureColor` set, or a `VanillaObjectColored`, you can set the `Color` field in the format of `"R, G, B, A"`.Sure

Notes:
1. While most of the fields say "Vanilla" , they will detect anything from their corresponding data file, such as from Json Assets.
2. The default value for `Type` is `DGAItem`. So, if you are specifying that for your item type, you can omit it.
3. For the `Custom` type:
   * When used in a crafting recipe, it must have a `NameOverride` and `IconOverride` (see crafting recipes section).
   * When used as an ingredient, must have a static function with the following signature: `bool IngredientMatches(Item, string)`
   * When used as a product, must have a constructor taking a single string (even if that string is empty).

### WeightedItem[]
In some cases (primarily recipe output) you may specify multiple `Item`s, with a weight. The mod will then choose one based on the weight values. Higher weights are more likely to be chosen. Here is an example:

```jsonc
[
    {
        "Weight": 10,
        "Value": /* Item object here, such as: */ {
            "Type": "VanillaObject",
            "Value": "Copper Ore"
        }
    },
    {
        "Weight": 5,
        "Value": {
            "Type": "VanillaObject",
            "Value": "Iron Ore"
        }
    },
    {
        "Weight": 1,
        "Value": {
            "Type": "VanillaObject",
            "Value": "Gold Ore"
        }
    }
]
```

### DynamicField[]
A little complex, but very powerful. See the [Dynamic Fields](#dynamic-fields) section.

## Pack data

All pack data comes in a single json file, `content.json` in the root of the content pack. Each entry has a `"$ItemType"` field, indicating what type of item it is. For example, all objects have `"$ItemType": "Object"`, and all melee weapons have `"$ItemType": "MeleeWeapon"`. Most pack data has an ID as well. This is unique *to your pack* (except crafting recipes, which are global to the game, including vanilla recipes), meaning multiple mods can create an item with the ID `"Cherry"`. Internally, IDs are prefixed with the content pack ID.

### Common
These fields are common to every type of pack data.
| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `$ItemType` | `string` | Required | The type of this pack data. Listed at the beginning of each pack data section. | `false` |
| `Enabled` | `bool` | Default: `true` | Whether or not the item is currently enabled. Useful with dynamic fields. | `true` |
| `EnableConditions` | `Dictionary<string, string>` | Default: `null` | Checked at the beginning of each day, these can changed the `Enabled` field dynamically. If disabled, instances of this item will be removed from the game world. Some data might linger; if a pack data type has a "Traces" section, it will describe what will linger. These can be removed, too, by setting `RemoveAllTracesWhenDisabled` to `true` for those pack data types. | `false` |
| `DynamicFields` | `DynamicField[]` | Default: `null` | See the [Dynamic Fields](#dynamic-fields) section. | `false` |

### Content Index
`$ItemType` - `"ContentIndex"`

These allow you to specify additional json files providing content. They also work with `EnableConditions`, meaning you could disable a bunch of entries referenced by one of these at once, without specifying it on every item.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `FilePath` | `string` | Required | The path to the json file for this content. | `true` |

### Config Schema
Unlike most pack data, config schema entries live in `config-schema.json`.

DGA supports custom config files for packs, integrated with GMCM. You make a list of entries to show in Generic Mod Config Menu (GMCM) (or in the config file, although labels, paragraphcs, images, and pages don't work there), and it works automatically. Everything shows up in the order they show in the config schema.

Config fields are usable in dynamic field conditions and enable conditions. (Remember, these are only applied at the beginning of each day.) They are very useful with Content Patcher's [Query token](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-tokens-guide.md#query-expressions). To learn how to use config options in your dynamic fields, see the [Dynamic Fields](#dynamic-fields) section.

Every config schema entry has two fields, plus more depending on the type.
| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `OnPage` | `string` | Default: `""` (the main page) | The page that this entry will show up on. |
| `ElementType` | `ConfigElementType` | Default: `"ConfigOption"` | The type of element this is. | 

There are four `ConfigElementType`s for config schema entries. Each one has a section below.

#### Label
A label is a large piece of text that shows up on the page, and can have a tooltip when hovered. This shows up in GMCM only.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `Name` | `string` | Required | The text for this label. |
| `Description` | `string` | Default: `""` (no tooltip) | The tooltip for this label. |
| `PageToGoTo` | `string` | Default: `null` (doesn't go anywhere) | The name of the page to go when this label is clicked. |

#### Paragraph
A paragraph is a block of text that shows on the page. This shows up in GMCM only.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `Name` | `string` | Required | The text for this paragraph. |

#### Image
An image entry is just a centered image. This shows up in GMCM only.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `ImagePath` | `string` | Required | The path to the image in your content pack. NOTE: This is not a `Texture`, meaning it does not support indexing or animations. This is just a file path, to display a full image (unless you specify `ImageRect`, see below). |
| `ImageRect` | `Rectangle` | Default: `null` (show full image) | The subrect of the image to show. |
| `ImageScale` | `int` | Default: `4` (matches vanilla image scale) | How scaled up the image should be. |

#### Config Option
A config option is an actual configurable option for the user. This showsi n both config.json and GMCM.

Every config schema entry has three fields, plus more depending on the type.
| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `Name` | `string` | Required | The name for this config option. |
| `Description` | `string` | Default: `""` (no tooltip) | The tooltip for this config option. |
| `ValueType` | `ConfigValueType` | Default: `"ConfigOption"` | The type of element this is. | 

There are four `ConfigValueType`s for config option entries. Each one has a section below.

##### Boolean
A boolean is just a true or false value. In GMCM, this is a checkbox.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `DefaultValue` | `bool` | Required | The default value for this option. |

##### Integer
An integer is a whole number. In GMCM, this is a slider if `ValidValues` is specified; otherwise, it is a textbox.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `DefaultValue` | `int` | Required | The default value for this option. |
| `ValidValues` | `int, int[, int]` | Default: `null` (anything is valid) | The valid values for this option. If specified, this creates a slider in GMCM. The first integer is the minimum value, and the second integer is the maximum value. The third, if specified, is the "step size" of the slider (the increments in which the value changes). |

##### Float
An float is a number that can have a value after the decimal point. In GMCM, this is a slider if `ValidValues` is specified; otherwise, it is a textbox.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `DefaultValue` | `int` | Required | The default value for this option. |
| `ValidValues` | `int, int[, int]` | Default: `null` (anything is valid) | The valid values for this option. If specified, this creates a slider in GMCM. The first float is the minimum value, and the second float is the maximum value. The third, if specified, is the "step size" of the slider (the increments in which the value changes). |

##### String
An string is just some text. In GMCM, this is a dropdown if `ValidValues` is specified; otherwise, it is a textbox.

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `DefaultValue` | `string` | Required | The default value for this option. |
| `ValidValues` | `string, string[, string[, string...]]` | Default: `null` (anything is valid) | The valid values for this option. If specified, this creates a dropdown in GMCM. The valid values must be comma separated. |

### Big Craftables
`$ItemType` - `"BigCraftable"`

Big craftables can be localized in the following keys: `"big-craftable.YourBigCraftable.name"` and `"big-craftable.YourBigCraftable.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this big craftable. | `false` |
| `Texture` | `Texture[16, 32]` | Required | The texture of this big craftable. | `true` |
| `SellPrice` | `int` | Default: `null` | The price if sold to a shop. | `true` |
| `ForcePriceOnAllInstances` | `bool` | Default: `false` | Should the price be used globally for all big craftables of this type (true)? Or should it be set at object creation and left at that (false)? | `true` |
| `ProvidesLight` | `bool` | Default: `false` | Whether or not this big craftable provides light, like a lamp. | `true` |

### Boots
`$ItemType` - `"Boots"`

Boots can be localized in the following keys: `"boots.YourBoots.name"` and `"boots.YourBoots.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of these boots. | `false` |
| `Texture` | `Texture[16, 16]` | Required | The texture of these boots. | `true` |
| `FarmerColors` | `Texture[4, 1]` | Required | The color palette for these boots, applied to the farmer's shoes when equipped. | `true` |
| `Defense` | `int` | Default: `0` | The defense buff from these boots. | `true` |
| `Immunity` | `int` | Default: `0` | The immunity buff from these boots. | `true` |
| `SellPrice` | `int` | Default: `0` | The sell price of these boots when sold to a shop. | `true` |

### Crafting Recipes
`$ItemType` - `"CraftingRecipe"`

Note: Unlike everything else, the `ID` field must be unique *across the game* (including vanilla recipes). Also, the name cannot contain the word `Recipe` or it will not be purchaseable (and might cause other problems). 

Crafting recipes can be localized in the following keys: `"crafting.YourCraftingRecipe.name"` and `"crafting.YourCraftingRecipe.description"`.

If `RemoveAllTraceswhenDisabled` is set, then the player will not remember the recipe when it is re-enabled.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of these boots. | `false` |
| `IsCooking` | `bool` | Required | Whether this is a cooking or crafting recipe. | (unknown, untested) |
| `KnownByDefault` | `bool` | Default: `false` | Whether or not this is known by default. If not known, added at the beginning of the day. | `true` |
| `SkillUnlockName` | `string` | Default: `null` | What skill will unlock this recipe. If past the level in the skill, added at the beginning of the day. | `true` |
| `SkillUnlockLevel` | `string` | Default: `null` | What skill level will unlock this recipe. If past the level in the skill, added at the beginning of the day. | `true` |
| `Ingredients` | `Ingredient[]` | Required | The ingredients for this crafting recipe. | (unknown, untested) |
| `Result` | `WeightedItem[]` | Required | The result of this crafting recipe. If multiple are listed, the first will be shown for the icon/name. | (unknown, untested) |

#### Special types
These are special types relating to just crafting recipes, used in the above table.

##### Ingredient[]
This should be an array of an an object called `Ingredient`. An `Ingredient` is just an `Item` with the following extra fields:

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `NameOverride` | `string` | Required/Default: `null` | Override the name showing for this ingredient. Required when using `ContextTag` for `Type`.  |
| `IconOverride` | `Texture[16, 16]` | Required/Default: `null` | Override the icon showing for this ingredient. Required when using `ContextTag` for `Type`. |

Example:

```json
"Ingredients": [
    {
        "Type": "VanillaObject",
        "Value": 74
    },
    {
        "Type": "ContextTag",
        "Value": "color_white",
        "NameOverride": "Something white",
        "IconOverride": "objects.png:0"
    }
]
```
### Crops
`$ItemType` - `"Crop"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this crop. | `false` |
| `Type` | `Enum[Normal, Indoors, Paddy]` | Default: `"Normal"` | The crop type: a "normal" one, one that must be indoors, or one that grows faster near water. | `true` |
| `CanGrowNow` | `bool` | Default: `false` | Whether or not the crop can grow. Use with [Dynamic Fields](#dynamicfield) to make a crop that doesn't either always or never grow. | `true` |
| `Colors` | `Color[]` | Default: `null` | The potential colors for the color layer of a phase to use. Harvested products will also use this color if they have a color layer. | (unknown, untested) |
| `Phases` | `Phase[]` | Required | The phases for this crop. | (unknown, untested) |
| `GiantChance` | `float` | Default: `0.01` | The chance for a giant crop to grow, if `GiantTextureChoices` is set. | `true` |
| `GiantTexturesChoices` | `MultiTexture[48, 63]` | Default: `null` | If set, enables giant crop growth. This will determine the texture for it. | (unknown, untested) |
| `GianDrops` | `HarvestDrop[]` | Default: `null` | The drops when the giant crop is broken. | (unknown, untested) |

Example `DynamicFields` for a spring-only `CanGrowNow`:

```json
"DynamicFields": [
    {
        "Conditions": { "Season |contains=spring": true },
        "CanGrowNow": true
    }
]
```

#### Special types
These are special types relating to just crops, used in the above table.

##### Phase[]
This should be an array of an an object called `Phase`. A `Phase` is as follows:

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `TextureChoices` | `MultiTexture[16, 32]` | Required | The choices between textures. A texture is chosen based on the tile location of the crop. |
| `TextureColorChoices` | `MultiTexture[16, 32]` | Default: null | The choices between textures for the color layer. A texture is chosen based on the tile location of the crop. |
| `Length` | `int` | Required | The length of this phase, in days. |
| `Scythable` | `bool` | Default: `false` | If the crop is scythable this phase, assuming it can be harvested. |
| `Trellis` | `bool` | Default: `false` | If the crop is solid this phase. |
| `HarvestedDrops` | `HarvestDrop[]` | Default: `null` | The drops from harvesting this phase. `null` means it can't be harvested. |
| `HarvestedExperience` | `int` | Default: `0` | How much experience to grant when harvesting. The game normally uses this formula for this (with `price` being the price of the object harvested): `16 * log_ee(0.018 * price + 1)` |
| `HarvestedNewPhase` | `int` | Default: -1 | The index of the phase to revert to after harvesting, or -1 if the crop should disappear. |

Example seed phase:

```json
{
    "TextureChoices": [ "crops.png:0", "crops.png:1" ],
    "Length": 1
}
```

##### HarvestedDrop[]
This should be an array of an an object called `HarvestedDrop`. A `HarvestedDrop` is as follows:

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `MininumHarvestedQuantity` | `int` | Default: `1` | The minimum amount of this item to drop. |
| `MaximumHarvestedQuantity` | `int` | Default: `1` | The maximum amount of this item to drop. |
| `ExtraQuantityChance` | `float` | Default: `0` | A chance (between `0` and `0.9`, with `0.5` being 50%) for the quantity to increase. If increased, the check will run again, and again, until the check fails. |
| `Item` | `WeightedItem[]` | Required | The items to choose between for when dropping this. NOTE: The item value for a WeightedItem can be null in this case, which means nothing will drop. This can be used to make a chance to drop something, or nothing at all. |

Example:

```json
{
    "MinimumHarvestedQuantity": 3,
    "MaximumHarvestedQuantity": 5,
    "ExtraChance": 0.5,
    "Item": [
        {
            "Weight": 1,
            "Value": {
                "Value": "spacechase0.DynamicGameAssets.Example/Mysterious Blue Circle"
            }
        },
        {
            "Weight": 1,
            "Value": {
                "Value": "spacechase0.DynamicGameAssets.Example/Mysterious Circle"
            }
        }
    ]
}
```

### Fences 
`$ItemType` - `"Fence"`

Fences can be localized in the following keys: `"fence.YourFence.name"` and `"fence.YourFence.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this fence. | `false` |
| `ObjectTexture` | `Texture[16, 16]` | Required | The texture for when the fence is in your inventory. | `true` |
| `PlacedTilesheet` | `Texture[48, 352]` | Required | The tilesheet for this fence, matching the format of vnailla fences. | `true` |
| `MaxHealth` | `int` | Required | The maximum health of the fence (vs decaying). | `true` (applies to new fences placed/repaired only) |
| `RepairMaterial` | `Item` | Required | The material to repair this fence. | (unknown, untested) |
| `BreakTool` | `Enum(Axe, Pickaxe)` | Default: `Axe` | The tool to break this fence. | `true` |
| `PlacementSound` | `string` | Required | The sound cue ID of what to play when placing this fence. | `true` |
| `RepairSound` | `string` | Required | The sound cue ID of what to play when repairing this fence. | `true` |

### Forge Recipe
`$ItemType` - `"ForgeRecipe"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `Result` | `WeightedItem[]` | Required | The result from crafting this forge recipe. | (unknown, untested) |
| `BaseItem` | `Item` | Required | The base item (left side) for this recipe. | (unknown, untested) |
| `IngredientItem` | `Item` | Required | The ingredient item (right side) for this recipe. | (unknown, untested) | 
| `CinderShardCost` | `int` | Default: `0` | The cost in cinder shards to forge this recipe. | `true` |

### Fruit Trees
`$ItemType` - `"FruitTree"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this fruit tree. | `false` |
| `Texture` | `Texture[432, 80]` | Required | The texture used for this fruit tree, matching the vanilla format. | `true` |
| `CanGrowNow` | `bool` | Default: `false` | Whether or not the fruit tree can produce fruit. Use with [Dynamic Fields](#dynamic-fields) to make a fruit tree that doesn't either always or never produce fruit. | `true` |
| `Product` | `WeightedItem[]` | Required | The product to grow on the tree. Once something rows on the tree, it will stay there until shaken (or struck by lightning). Max of 3 per tree. (NOTE: For technical purposes, this must be a `VanillaObject` or an object pack data type!) |

Example `DynamicFields` for a spring-only `CanGrowNow`:

```json
"DynamicFields":
[
    {
        "Conditions": { "Season |contains=spring": true },
        "CanGrowNow": true
    }
]
```

## Furniture
`$ItemType` - `"Furniture"`

Furniture can be localized in the following keys: `"furniture.YourFurniture.name"` and `"furniture.YourFurniture.description"`. Additionally, `"furniture.YourFurniture.category"` can be specified for a category text override.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this furniture. | `false` |
| `Type` | `Enum[Bed, Decoration, Dresser, Fireplace, FishTank, Lamp, Painting, Rug, Table, Sconce, TV, Window, Chair, Bench, Couch, Armchair]` | Required | The type of this furniture. | `false` |
| `Configurations` | `FurnitureConfiguration[]` | Required | The configurations for this funriture. It uses the first configuration by default, and the others after being rotated. (NOTE: Fish tanks, beds, and TVs may only support one configuration! Chairs, benches, couches, and armchairs may only support 4 configurations.) | (unknown, untested) |
| `ShowInCatalogue` | `bool` | Default: `true` | Whether the furniture shows up in the furniture catalogue. | (unknown, untested) |

Certain furniture types have additional fields:

| Applicable for | Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- | --- |
| `Bed` | `BedType` | `Enum[Single, Double, Child]` | `"Single"` | The bed type. | (unknown, untested) |
| `TV` | `ScreenPosition` | `Vector2` | Default: `"0, 0"` | The offset for the screen to render, in pixels, from the texture. | (unknown, untested) |
| `TV` | `ScreenSize` | `float` | Required | A multiplier for the screen size, in relation to the size of the graphics in the game files. | `true` |
| `FishTank` | `TankSwimmingCapacity` | `int` | Default: `-1` | The max amount of "swimming fish" in the fish tank, or -1 for unlimited. | `true` |
| `FishTank` | `TankGroundCapacity` | `int` | Default: `-1` | The max amount of "ground fish" in the fish tank, or -1 for unlimited. | `true` |
| `FishTank` | `TankDecorationCapacity` | `int` | Default: `-1` | The max amount of "decorations" in the fish tank, or -1 for unlimited. (NOTE: Only one of each type of decoration is supported.) | `true` |

#### Special types
These are special types relating to just furniture, used in the above tables.

##### FurnitureConfiguration[]
This should be an array of an an object called `FurnitureConfiguration`. A `FurnitureConfiguration` is as follows:

| Field | Type | Required or Default value | Description |
| --- | --- | --- | --- |
| `Texture` | `Texture` | Required | The texture for this configuration. |
| `FrontTexture` | `Texture` | Optional* (Default: `null`) | (* Required for beds/fish tanks/furniture with seats to look right.) The texture for this configuration to render on top. |
| `NightTexture` | `Texture` | Optional* (Default: `null`) | (* Required for windows, sconces, and lamps to look right.) The texture for this configuration to render when it's nighttime. |
| `DisplaySize` | `Vector2` | Required | The display size of this furniture, in tiles. |
| `CollisionHeight` | `int` | Required | How high from the bottom this furniture is solid. |
| `Flipped` | `bool` | Default: `false` | If the texture is flipped or not. |
| `Seats` | `Vector2[]` | Default: `null` (Except for Chair, Bench, Couch, Armchair, where the default is the game defaults for those furniture types.) | The list of seat positions, in tiles, if any. |
| `SittingDirection` | `Enum[Any, Up, Down, Left, Right]` | Default: `"Any"` (Except for Chair, Bench, Couch, Armchair, where the default is the game defaults for those furniture types.) | The direction the seats face. |
| `TileProperties` | `Dictionary<Vector2, Dictionary<string, Dictionary<string, string>>>` | The tile properties to emulate for this furniture. It goes position -> layer -> property = value. (See example.)

`Texture` and `FrontTexture` (if any) should have the same size: `DisplaySize` multiplied by 16 for both `X` and `Y`.

Example furniture configuration:

```json
{
    "Texture": "test_decoration.png",
    "DisplaySize": { "X": 1, "Y": 1 },
    "CollisionHeight": 1,
    "TileProperties": {
        "0, 0": {
            "Buildings": {
                "Action": "Warp 5 7 FarmHouse"
            }
        }
    }
}
```

### Hats
`$ItemType` - `"Hat"`

Hats can be localized in the following keys: `"hat.YourHat.name"` and `"hat.YourHat.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this hat. | `false` |
| `Texture` | `Texture` | Required | The texture of this hat. | `true` |
| `HairStyle` | `Enum[Full, Obscured, Hide]` | Default: `"Full"` | How hair should be shown with this hat equipped. | `true` |
| `IgnoreHairstyleOffset` | `bool` | Default: `false` | If this hat should ignore where hair is placed when being positioned. | `true` |

### Machine Recipes
`$ItemType` - `"MachineRecipe"`

Only basic machine recipes are supported; for advanced machines, use Producer Framework Mod.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `MachineId` | `string` | Required | The ID of the machine this reicpe is for. | `true` |
| `Result` | `WeightedItem[]` | Required | The result of this recipe. | `true` |
| `Ingredients` | `Item[]` | Required | The ingredients for this recipe. | `true` |
| `MinutesToProcess` | `int` | Required | How many minutes for this recipe to process. Minimum 10 minutes. | `true` |
| `StartWorkingSound` | `int` | Default: `"furnace"` | The sound to make when work has started. | `true` |
| `WorkingLightOverride` | `bool?` | Default: `null` | An override for if the machine provides light. | (unknown, untested) |
| `MachineWorkingTextureOverride` | `Texture[16, 32]` | Default: `null` | An override for the texture while the machine is working. This texture must be in the machine's content pack. | `true` |
| `MachineFinishedTextureOverride` | `Texture[16, 32]` | Default: `null` | An override for the texture when the machine is finished. This texture must be in the machine's content pack. | `true` |
| `MachinePulseWhileworking` | `bool` | Default: `true` | Should the machine pulse (scale up and down) while it is working? | `true` |

Note that the first ingredient must be being held for the recipe to activate.

### Melee Weapons
`$ItemType` - `"MeleeWeapon"`

Melee weapons can be localized in the following keys: `"melee-weapon.YourMeleeWeapon.name"` and `"melee-weapon.YourMeleeWeapon.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this melee weapon. | `false` |
| `Texture` | `Texture[16, 16]` | Required | The texture of this melee weapon. | `true` |
| `Type` | `Enum[Dagger, Club, Sword]` | Default: `"Dagger"` | The type of this melee weapon. | `false` |
| `MinimumDamage` | `int` | Required | The minimum damage for this weapon. | `true` |
| `MaximumDamage` | `int` | Required | The maximum damage for this weapon. | `true` |
| `Knockback` | `float` | Default: `0` | The knockback bonus for this weapon. | `true` |
| `Speed` | `int` | Default: `0` | The speed bonus for this weapon. (TODO: Swing speed or movement speed?) | `true` |
| `Accuracy` | `int` | Default: `0` | The accuracy bonus for tthis wepaon. (TODO: What?) | `true` |
| `Defense` | `int` | Default: `0` | The defense bonus while holding this wepaon. | `true` |
| `ExtraSwingArea` | `int` | Default: `0` | The extra swing area over the default for this weapon type. | `true` |
| `CritChance` | `float` | Default: `0` | The extra crit chance for this weapon. | `true` |
| `CritMultiplier` | `float` | Default: `0` | The crit multiplier for this weapon. | `true` |
| `CanTrash` | `bool` | Default: `true` | Whether or not this weapon can be trashed. | `true` |

### Objects
`$ItemType` - `"Object"`

Objects can be localized in the following keys: `"object.YourObject.name"` and `"object.YourObject.description"`. Additionally, `"object.YourObject.category"` can be specified for a category text override.

If `RemoveAllTraceswhenDisabled` is set, then the player will lose their shipped count upon being disabled.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of this object. | `false` |
| `Texture` | `Texture[16, 16]` | Required | The texture of this object. | `true` |
| `Plants` | `string` | Default: `null` | The ID of the crop to plant. Must be the full ID of the DGA crop. | `true` |
| `Category` | `Enum[Vegetable, Fruit, Flower, Gem, Fish, Egg, Milk, Cooking, Crafting, Mineral, Meat, Metal, Junk, Syrup, MonsterLoot, ArtisanGoods, Seeds, AnimalGoods, Greens]` | Default: `"Junk"` | The vanilla category for this object. | (unknown, untested) |
| `CategoryColorOverride` | `Color?` | Default: `null` | The color override for the category name. | (unknown, untested) |
| `Edibility` | `int` | Default: `-300` (inedible) | The edibility value for thsi object; auto-calculates stamina and health restored based on the standard game formula. | `true` |
| `EatenHealthRestoredOverride` | `int?` | Default: `null` | An override for how much health is restored upon eating. | `true` |
| `EatenStaminaRestoredOverride` | `int?` | Default: `null` | An override for how much stamina is restored upon eating. | `true` |
| `EdibleIsDrink` | `bool` | Default: `false` | Whether this object, if edible, is shown as a drink, or as a food. | `true` |
| `EdibleBuffs` | `FoodBuffs` | Default values | The buffs this provides upon being eaten. | `true` |
| `SellPrice` | `int?` | Default: `0` | The sell price of this object, or null if it can't be sold. | `true` |
| `ForcePriceOnAllInstances` | `bool` | Default: `false` | Should the price be used globally for all objects of this type (true)? Or should it be set at object creation and left at that (false)? | `true` |
| `CanTrash` | `bool` | Default: `true` | Whether or not this object can be trashed. | `true` |
| `HideFromShippingCollection` | `bool` | Default: `false` | Whether or not to hide this object from the shipping collection. | `true` |
| `IsGiftable` | `bool` | Default: `true` | Whether or not this object is giftable. | `true` |
| `UniversalGiftTaste` | `int` | Default: `20` | The universal gift taste for this object. Dialogue is chosen based on this value. 1 heart is 250 points. (Check wiki for vanilla point values for gifts.) | `true` |
| `Placeable` | `bool` | Default: `false` | Whether or not this object is placeable. | `true` |
| `SprinklerTiles` | `Vector2[]` | Default: `null` | What tiles to water when this object is placed. | (unknown, untested) |
| `UpgradedSprinklerTiles` | `Vector2[]` | Default: `null` | What tiles to water when this object is placed and upgraded. | (unknown, untested) |
| `ContextTags` | `string[]` | Default: `null` | What additional context tags this object has. Note that some are auto-generated anyways, based on name and category, for example. |

#### Special types
These are special types relating to just objects, used in the above table.

##### FoodBuffs
A `FoodBuffs` is as follows. (All fields are `int` and default to `0`.)

| Field | Description |
| --- | --- |
| `Farming` | The farming level buff amount. |
| `Fishing` | The fishing level buff amount. |
| `Mining` | The mining level buff amount. |
| `Luck` | The luck level buff amount. |
| `Foraging` | The foraging level buff amount. |
| `MaxStamina` | The max stamina buff amount. |
| `MagnetRadius` | The additional magnet radius buff amount. |
| `Speed` | The speed buff amount. |
| `Defense` | The defense buff amount. |
| `Attack` | The attack buff amount. |
| `Duration` | The duration of the buff, in in-game minutes. |

Here is an example `FoodBuffs` inside an object:

```json
"EdibleBuffs": {
    "Speed": 5,
    "Duration": 30
},
```

### Gift Tastes
`$ItemType` - `"GiftTaste"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ObjectId` | `string` | Required | The full object ID of what we are overriding the gift taste for. This can be a comma-separated list. | `false` |
| `Npc` | `string` | Required | The NPC of who we are overriding the gift taste for. This can be a comma-separated list. | `false` |
| `Amount` | `int` | Required | The amount of friendship points to give or take. | `true` |
| `NormalTextTranslationKey` | `string` | Default: `null` | The translation key of the text normally shown for this NPC when they receive this object. If null, the vanilla response is shown instead. | `true` |
| `BirthdayTextTranslationKey` | `string` | Default: `null` | The translation key of the text for this NPC when they receive this object on their birthday. If null, the vanilla response is shown instead. | `true` |
| `EmoteId` | `int?` | Default: `null` | The emote ID override for when they receive this gift, if any. | `true` |

### Pants
`$ItemType` - `"Pants"`

Pants can be localized in the following keys: `"pants.YourPants.name"` and `"pants.YourPants.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of these pants. | `false` |
| `Texture` | `Texture[192, 188]` | Required | The texture tilesheet for these pants, matching the vanilla format. | `true` |
| `DefaultColor` | `Color` | Default: White (TODO: how to format?) | The default color of these pants on creation. | (unknown, untested) |
| `Dyeable` | `bool` | Default: `false` | Whether or not these pants are dyeable. | (unknown, untested) |

### Shirts
`$ItemType` - `"Shirts"`

Shirts can be localized in the following keys: `"shirt.YourShirt.name"` and `"shirt.YourShirt.description"`.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ID` | `string` | Required | The ID of these pants. | `false` |
| `TextureMale` | `Texture[8, 32]` | Required | The texture tilesheet for this shirt for male players, matching the vanilla format. | `true` |
| `TextureMaleColor` | `Texture[8, 32]` | Required | The texture tilesheet for this shirt for male players, for the dye layer, matching the vanilla format. | `true` |
| `TextureFemale` | `Texture[8, 32]` | Required | The texture tilesheet for this shirt for female players, matching the vanilla format. | `true` |
| `TextureFemaleColor` | `Texture[8, 32]` | Required | The texture tilesheet for this shirt for female players, for the dye layer, matching the vanilla format. | `true` |
| `DefaultColor` | `Color` | Default: White (TODO: how to format?) | The default color of these pants on creation. | (unknown, untested) |
| `Dyeable` | `bool` | Default: `false` | Whether or not these pants are dyeable. | (unknown, untested) |
| `Sleeveless` | `bool` | Default: `false` | Whether this shirt is sleeveless or not. | (unknown, untested) |

### Shop Entries
`$ItemType` - `"ShopEntry"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `ShopId` | `string` | Required | The shop ID that this entry will go in. | `true` |
| `Item` | `Item` | Required | The item to put in the shop. | `true` |
| `MaxSold` | `int` | Required | The maximum amount of `Item` that the shop will day today. | `true` |
| `Cost` | `int` | Required | The cost of this shop entry. | `true` |
| `Currency` | `string` | Default: `null` | Custom currency override for this shop entry. Must be either a vanilla item ID, OR a full ID of a DGA *object*. | `true` |

### Tailoring Recipes
`$ItemType` - `"TailoringRecipe"`

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `FirstItemTags` | `string[]` | Default: `[ "item_cloth" ]` | The item tags the first item must have. | (unknown, untested) |
| `SecondItemTags` | `string[]` | Required | The item tags the second item must have. | (unknown, untested) |
| `ConsumeSecondItem` | `bool` | Default: `true` | Whether or not the second item should be consumed. | `true` |
| `CraftedItem` | `WeightedItem[]` | Required | The item to craft. | (unknown, untested) |

### Texture Overrides
`$ItemType` - `"TextureOverride"`

These were implemented for a more optimal solution than Content Patcher Animations for some cases. (There's a chance it works better with mods like SpriteMaster, too.)

NOTE: These can be a little tricky, since `TargetRect` needs to be the exact rectangle the game uses to draw it. Overlapping rectangles, like drawing half the target area, will not be animated. For some cases, such as the tools tilesheet, you may need to look (or get someone else to look) in the game's source code to figure out what to use for `TargetRect`.

You don't need to use these to animate DGA objects from your pack; you can just use the corresponding item's `Texture` property to animate it directly.

| Field | Type | Required or Default value | Description | Dynamic |
| --- | --- | --- | --- | --- |
| `TargetTexture` | `string` | Required | The target asset path of the texture to override. If you are overriding another DGA pack, then prefix it with `DGA/{MOD_ID}/` (with `{MOD_ID}` being the mod ID of the mod whose texture you want to override). | (unknown, untested) |
| `TargetRect` | `Rectangle` | Required | The target rectangle on `TargetTexture` to override. See the note above this table. | (unknown, untested) |
| `SourceTexture` | `Texture` | Required | The source texture from your pack to use. This supports texture indexing (such as `texture.png:5`, with 5 being the index) and animations as well, just like other fields of type `Texture`. For the case of indexing, the size of the tilesheet's squares should be the same as `TargetRect`. | `true` |

## Extra Information

### Valid Shop IDs for Vanilla
Here are the shop IDs for vanilla shops:

world area | shop                                                                                       | shop ID
---------- | ------------------------------------------------------------------------------------------ | -------
Beach      | [Night Market decoration boat](https://stardewvalleywiki.com/Night_Market#Decoration_Boat) | `BlueBoat`
Beach      | [Night Market magic shop boat](https://stardewvalleywiki.com/Night_Market#Magic_Shop_Boat) | `GeMagic`
Beach      | [Willy](https://stardewvalleywiki.com/Fish_Shop)                                           | `FishShop`
Desert     | [Casino](https://stardewvalleywiki.com/Casino)                                             | `Club`
Desert     | [Desert trader](https://stardewvalleywiki.com/Desert_Trader)                               | `DesertMerchant`
Desert     | [Sandy](https://stardewvalleywiki.com/Oasis)                                               | `Sandy`
Forest     | [Hat mouse](https://stardewvalleywiki.com/Abandoned_House)                                 | `HatMouse`
Forest     | [Marnie's supplies](https://stardewvalleywiki.com/Marnie%27s_Ranch)                        | `AnimalSupplies`
Forest     | [Traveling cart](https://stardewvalleywiki.com/Traveling_Cart)                             | `TravelingMerchant`
Island     | [Island trader](https://stardewvalleywiki.com/Island_Trader)                               | `IslandMerchant`
Island     | [Qi walnut room](https://stardewvalleywiki.com/Qi%27s_Walnut_Room)                         | `QiGemShop`
Island     | [Resort bar](https://stardewvalleywiki.com/Ginger_Island#Beach_Resort)                     | `ResortBar`
Island     | [Volcano shop](https://stardewvalleywiki.com/Volcano_Dungeon#Shop)                         | `VolcanoShop`
Mountain   | [Adventurer's guild](https://stardewvalleywiki.com/Adventurer%27s_Guild)                   | `AdventurerGuild`
Mountain   | [Dwarf](https://stardewvalleywiki.com/Dwarf)                                               | `Dwarf`
Mountain   | [Robin's carpentry](https://stardewvalleywiki.com/Carpenter%27s_Shop)                      | `Carpenter`
Town       | [Clint's blacksmithery](https://stardewvalleywiki.com/Blacksmith)                          | `Blacksmith`
Town       | [Harvey's clinic](https://stardewvalleywiki.com/Harvey%27s_Clinic)                         | `Hospital`
Town       | [Ice cream stand](https://stardewvalleywiki.com/Ice_Cream_Stand)                           | `IceCreamStand`
Town       | [JojaMart](https://stardewvalleywiki.com/JojaMart)                                         | `Joja`
Town       | [Krobus](https://stardewvalleywiki.com/Krobus)                                             | `Krobus`
Town       | [Movie theater ticket office](https://stardewvalleywiki.com/Movie_Theater)                 | `Theater_BoxOffice`
Town       | [Pierre](https://stardewvalleywiki.com/Pierre%27s_General_Store)                           | `SeedShop`
Town       | [Saloon](https://stardewvalleywiki.com/The_Stardrop_Saloon)                                | `Saloon`
_festival_ | Festival shop                                                                              | `Festival.<date>` (e.g. `Festival.summer5`)

## Dynamic Fields
Dynamic fields let you change a field of an item at the beginning of the day based on Content Patcher conditions, as well as values from config.json (if you have a config schema).

Values from the config file are specified in the format of `{{My Option}}`, or `{{My Page/My Option}}` if the option was on another page.

First, you specify the conditions in a "Conditions" field, in the standard Content Patcher format. Then, the rest of the fields specified will override fields in the item.

This is done in the following order:
1. An item is checked to see if it is enabled through `EnableConditions`. If not, the item is removed, and the rest is skipped.
2. An item is reset to its original values.
3. The dynamic fields are gone through in order.
 * If one matches, its fields are applied to the object.
 * The list keeps being processed, meaning more multiple sets of dynamic fields can be applied!

 Here is an example with an Object:

 ```jsonc
  {
    "ID": "Mysterious Circle",
    "Category": "Vegetable",
    "Texture": "items16.png:1@20, items16.png:2@20, items16.png:3@20",
    "TextureColor": "items16.png:4",
    "Edibility": 9,
    "EdibileIsDrink": true,
    "EdibleBuffs": {
      "Speed": 5,
      "Duration": 30
    },
    "SellPrice": 5,
    "ForcePriceOnAllInstances": true,
    "UniversalGiftTaste": 100,
    "DynamicFields": [
      {
        "Conditions": { "Season |contains=spring": false },
        "SellPrice": 500,
        "Texture": "items16.png:0",
        "EdibleIsDrink": false,
        "EdibleBuffs.Speed": 500
      },
      {
        // No `Conditions` means always true. We just want to use our config values
        "UniversalGiftTaste": "{{Actual Settings/Mysterious Circle Universal Gift Taste}}",
        "DummyField": false // Test for extension data, you can ignore this
      },
      {
        "Conditions": { "Query: {{Actual Settings/Animated Mysterious Circle}} = false and {{Actual Settings/Static Mysterious Circle Color}} = 'White'": true },
        "Texture": "items16.png:0"
      },
      {
        "Conditions": { "Query: {{Actual Settings/Animated Mysterious Circle}} = false and {{Actual Settings/Static Mysterious Circle Color}} = 'Red'": true },
        "Texture": "items16.png:1"
      },
      {
        "Conditions": { "Query: {{Actual Settings/Animated Mysterious Circle}} = false and {{Actual Settings/Static Mysterious Circle Color}} = 'Green'": true },
        "Texture": "items16.png:2"
      },
      {
        "Conditions": { "Query: {{Actual Settings/Animated Mysterious Circle}} = false and {{Actual Settings/Static Mysterious Circle Color}} = 'Blue'": true },
        "Texture": "items16.png:3"
      }
    ]
  },
  ```
