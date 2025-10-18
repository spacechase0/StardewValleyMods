**SpaceCore** is a [Stardew Valley](http://stardewvalley.net/) framework mod used by some of my
other mods.

## Install
1. Install the latest version of [SMAPI](https://smapi.io).
2. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/1348).
3. Run the game using SMAPI.

## Use
* **For players:** just install the mod normally, and it'll work for the mods that need it.
* **For mod authors:** SpaceCore's functionality may change without warning (but probably won't).

Provided functionality for players:
* Hold left control when right clicking an NPC to open a dialogue box with choices for interactions.
    * By default this only includes chatting (and gifting if you are holding something), but mods can add new entries.
    * Content pack authors can let you ask an NPC questions by editing an asset from Content Patcher (see "NPC Questions" in the content pack authors). You can ask one question per day.
    * You can set this menu to always show up when interacting with an NPC in the config, in case you want to (for example) prevent accidentally gifting an item to somebody.
* If any mods add equipment slots through SpaceCore, a "+" button will show to the left of your boots slot. This can access the new equipment slots.
    * The image can be edited by content pack authors for recolors. The asset path to this texture is: `spacechase0.SpaceCore/ExtraEquipmentIcon`

Provided functionality for content pack authors:
* Fixes NPCs taking the longer path when there are circular routes. Note that the "length" of the path is determined by amount of warps, not by amount of tiles travelled, so if there are two paths to a location with the same amount of warps but one map is much larger, they might still take the larger map path.
* New map properties:
    * `spacechase0.SpaceCore_ColoredLights [x y type radiusMultiplier colorR colorG colorB]+` - A list of light sources that can be colored or use custom light textures.
        * `x` and `y` - the tile coordinates
        * `type` - same as vanilla `Light` property light types, or you can use the path to a custom light texture (must be loaded with Action: Load)
        * `radiusMultiplier` - Make the light bigger or smaller
        * `colorR`, `colorG`, `colorB` - Make the lights a specific color, using RGB values. Each value should be in the range of [0, 255].
    * A few dungeon-specific ones, listed in the Dungeons section.
* New GameStateQuery queries:
    * Every custom skill registered through the C# API automatically registers a `PLAYER_<SKILLID_IN_CAPS>_LEVEL` query matching the vanilla ones (such as PLAYER_FARMING_LEVEL).
    * `NEARBY_CROPS radius cropId` - Only usable in CropExtensionData YieldOverrides PerItemCondition entries. Checks for fully grown crops of a particular type in a certain radius.
    * `PLAYER_SEEN_CONVERSATION_TOPIC targetPlayer topicId` - If the player has seen the conversation topic in the past (but is no longer active). For targetPlayer, see [here](https://stardewvalleywiki.com/Modding:Game_state_queries#Target_player).
* New tile actions
    * `spacechase0.SpaceCore_TriggerAction triggerActionId` - for running a trigger action, set the Trigger to "Manual"
    * `spacechase0.SpaceCore_OpenGlobalInventory inventoryId` - open a global inventory with the given ID
    * A few dungeon-specific ones, listed in Dungeons section.
* New touch actions
    * `spacechase0.SpaceCore_TriggerAction triggerActionId` - for running a trigger action, set the Trigger to "Manual"
* New trigger actions
    * `spacechase0.SpaceCore_OnItemUsed` - use item GSQ conditions to check the right item, make sure to set `UseForTriggerAction` in ObjectExtensionData (see further below) for that item to true
    * `spacechase0.SpaceCore_OnItemEaten` - use item GSQ conditions to check the right item
* New trigger action actions
    * `spacechase0.SpaceCore_PlaySound sound local` - `sound` = the cue ID, `local` = `true` if everyone near the player should hear it, `false` otherwise
    * `spacechase0.SpaceCore_ShowHudMessage "message goes here" optionalQualifiedItemIdForIcon`
    * `spacechase0.SpaceCore_PlayEvent eventid ifNotSeen` - `ifNotSeen` is optional (defaults to false) - if true, the event won't play if it has been seen before
    * `spacechase0.SpaceCore_DamageCurrentFarmer amount`
    * `spacechase0.SpaceCore_ApplyBuff buffId "Source to show on the buff"` - Applies the buff `buffId` (from the vanilla file `Data/Buffs`), with the source in the tooltip being shown as what you provide.
    * `spacechase0.SpaceCore_TriggerSpawnGroup spawnGroupId location includeRegions excludeRegions` - Trigger a spawn group (see spawnables documentation).
        * `location` can be a static location name, or `Target` for triggering inside the current dungeon level (see the dungeons documentation).
        * `includeRegions` and `excludeRegions` are a slash separated list of rectangles, with each component of the rectangle separated by commas.
            * Ex. `2,2,10,10/25,30,15,10` would be two rectangles:
                * One at X=2 Y=2 with a width of 10 and a height of 10
                * One at X=25 Y=30 with a width of 15 and a height of 10
    * `spacechase0.SpaceCore_ClearSetPiecesFromSpawnable spawnDefinitionId location` - Clear all set pieces spawned from a particular spawn definition in the specified location (see the spawnables documentation for what a spawn definition is).
* Custom event commands
    * `damageFarmer amount`
    * `setDating npc [true/false]` - default true
    * `totemWarpEffect tileX tileY totemSpriteSheetPatch sourceRectX,SourceRectY,sourceRectWidth,sourceRectHeight` - will need to delay after this command if you want to wait for the animation to be done
    * `setActorScale actor factorX factorY` - reset with factorX/Y = 1 - this will probably not be positioned right after scaling and will need a position offset afterwards
    * `cycleActorColors actor durationInSeconds color [color...]` color=`R,G,B` - specify 1 or more. Specifying 1 will just mean it stays that color instead of cycling
    * `flash durationInSeconds`
    * `setRaining locationContext true/false` - Sets a location context as raining (or not). In vanilla, valid location contexts are "Default", "Island", and "Desert" (case sensitive).
    * `screenshake intensity durationInSeconds` - Shake the screen for a certain amount of time. For intensity, `1` will be basically nothing. It's in pixels difference from the base screen position, so try something like 5 or 10 to start with.
    * `setZoom factor` (factor of 4 is zoomed in 400% - don't zoom out (ie. less than 1), the game doesn't like it)
    * `smoothZoom targetZoomFactor durationInSeconds`
    * `setEngaged npc asRoommate weddingOffset` - `npc` = NPC internal ID, `asRoommate` = true if this is a roommate, false otherwise, `weddingOffset` = how many days away the wedding should be, minimum of one (pushed back for invalid wedding dates like festivals).
* Vanilla Asset Expansions
    * Objects - These are in the asset `spacechase0.SpaceCore/ObjectExtensionData`, which is a dictionary with the key being an object's unqualified item ID, and the value being an object containing the following fields:
        * `CategoryTextOverride` - string, default null (no override)
        * `CategoryColorOverride` - same format as Json Assets colors, default null (no override)
        * `CanBeTrashed` - true/false, also prevents dropping, default true
        * `CanBeShipped` - true/false, default true
        * `EatenHealthRestoredOverride` - integer, override how much health is restored on eating this item, default null (use vanilla method of calculation)
        * `EatenStaminaRestoredOverride` - integer, override how much stamina is restored on eating this item, default null (use vanilla method of calculation)
        * `MaxStackSizeOverride` - integer, override the max stack size of your item, default null (use vanilla amount)
        * `TotemWarp` - allow a custom object to act as a warp totem, an object containing the following properties:
            * `Location` - string, the location to warp to - ex. `"CommunityCenter"`
            * `Position` - Vector2, the tile to warp to - ex. `"25, 15"`
            * `Color` - Color, the color the screen should flash - ex. `{ "R": 0, "G": 0, "B": 255, "A": 255 }`
            * `ConsumedOnUse` - bool, default true
        * `UseForTriggerAction` - True to run a trigger action upon use, false otherwise. Default false.
        * `ConsumeForTriggerAction` - If the above field is true, this will control if the item is consumed on use. Default false for backwards compatibility.
        * `GiftableToNpcDisallowList` - A dictionary of NPC names to messages that should show when you try to gift the item to them, instead of them receiving the gift.
        * `GiftableToNpcAllowList` - A dictionary of NPC names to `true` for if you want that NPC allowed. If set, any NPCs not listed here will not be able to receive the gift, and instead will show the message from the following field.
        * `GiftedToNotOnAllowListMessage` - The message to show for when the item is gifted to someone not on the allow list. Required if `GiftableToNpcAllowList` is set. (The disallow list is checked first, so you can still allow specific responses by certain NPCs.)
    * Food buffs - Put these in the `CustomFields` inside the entry in `Buffs`
        * `"spacechase0.SpaceCore/HealthRegeneration"` - Add a buff that adds an amount of health regeneration per second.
        * `"spacechase0.SpaceCore/StaminaRegeneration"` - Add a buff that adds an amount of stamina regeneration per second.
    * `Data/Buffs` extensions - Put these in `CustomFields`.
        * `"spacechase0.SpaceCore/HealthRegeneration"` - Add a buff that adds an amount of health regeneration per second.
        * `"spacechase0.SpaceCore/StaminaRegeneration"` - Add a buff that adds an amount of stamina regeneration per second.
    * Buffs from worn items  - These are in asset in `spacechase0.SpaceCore/WearableData`, which is a dictionary with the key being the qualified item ID of the wearable, and the value being an object containing the following fields:
        * `BuffIdToApply` - The ID of the buff to apply, pulled from the vanilla file `Data/Buffs`.
    * Crops - These are in `spacechase0.SpaceCore/CropExtensionData`
        * `YieldOverrides` - A little complex, but you can override each crop phase's harvestability with experience gained, the new phase it goes to, and the drops it has (including conditional drops). Example [here](https://gist.github.com/spacechase0/79f95bcd46160da9e52f5bc0c71329f4).
    * Fruit trees - These are in `spacechase0.SpaceCore/FruitTreeExtensionData`
        * `FruitLocations` - A list of tile coordinates to use for the offset of each fruit grown, instead of what vanilla does by default.
    * Weapons - Stored in the `CustomFields` on the weapon data asset object:
        * `CanBeTrashed` - true/false, also prevents dropping, default true
    * Furniture - Stored in the asset `"spacechase0.SpaceCore/FurnitureExtensionData"`, which is a dictionary with the key being the furniture ID, and the value being an object containing the following fields:
        * `TileProperties` - A dictionary of tile coordinates to a dictionary of layers to a dictionary of tile properties. Just look at [this example](https://gist.github.com/spacechase0/ea6db01284157d408d9f359f141a0d65).
        * `DescriptionOverride` - Description override for the furniture.
        * `SeatLocations` - A dictionary for overriding where seats are on the location. The key is an integer corresponding to the rotation the seats are for, and the value is a list of coordinates (in pixels relative to the furniture).
    * Shops - Stored in the asset `"spacechase0.SpaceCore/ShopExtensionData"`, which is a dictionary with the key being the shop ID, and the value being an object containing the following fields:
        * `Tabs` - The options are `"None"`, `"Catalogue"`, `"FurnitureCatalogue"`, and `"Custom"`. For custom, see the next field.
        * `CustomTabs` - A list consisting of the following object (see [this example](https://gist.github.com/spacechase0/8a80b22655f624d9854486bfbe5abc7e)):
            * `Id` - The ID of this tab, must be unique (used by Content Patcher)
            * `IconTexture` - The path to the texture to use for the tab. Use the `{{InternalAssetKey: }}` token for your own assets.
            * `IconRect` - The subrect of the texture to use for the tab. An object containing `X`, `Y`, `Width`, `Height` (all in pixels).
            * `FilterCondition` - A GameStateQuery condition used for filtering the items. Use `TRUE` to just get everything (useful for your first tab). Example to get all seeds: `"ITEM_CATEGORY Input -74"`
    * Virtual Currencies (like qi gems and golden walnuts) - Stored in the asset `"spacechase0.SpaceCore/VirtualCurrencyData"`, which is a dictionary with the key being the object ID of the currency, and the value being an object containing the following fields:
        * `TeamWide` - True if the currency is shared across all farmers, false if it is per farmer.
        * `ObtainSound` - The sound to play on getting more. (Known bug: Doesn't seem to fire the first time.)
        * If you want to use the currency as the `Currency` field in your shop, run the following command with your currency ID: `spacecore_getcurrencyid MyCurrencyId`. The resulting number (including the negative sign, if there is one) is what you should use for the `Currency` field.
        * If you want to use the currency as a trade item for a specific shop entry, just set the `TradeItemId` to the currency item ID and the set the proper `TradeItemAmount`.
    * NPCs - Stored in the asset `"spacechase0.SpaceCore/NpcExtensionData"`, which is a dictionary with the key being an NPC name, and the value being an object containing the following fields:
        * `GiftEventTriggers` - A dictionary with the keys being an object, and the values being an event to trigger when that item is given to the NPC.
            * The "event to trigger" needs to be the full event key (ID and preconditions) used in the events data file, so that SpaceCore can find the event.
            * The preconditions are ignored.
            * It will only check the current location, so make sure to only have the event in this data file when in the right location.
                * For Content Patcher users: You can do this using `"When": { "LocationName": "Town" }, "Update": "OnLocationChange"` on your patch.
            * The event can reoccur if the item is given again.
                * For Content Patcher users: If you don't want this behavior, make sure to add a `HasSeenEvent` event condition to your `"When"` block for the patch.
        * `IgnoreMarriageSchedule` - true/false, defaults to false
        * `SeparateDatability` - If datability is tracked separately in multiplayer for this NPC. (ie. if the NPC can be datable for one player but not the other) - Default false.
    * Schedule Animations - Stored in the asset `spacechase0.SpaceCore/ScheduleAnimationExtensionData`, which is a dictionary with the key being the animation ID from `Data/animationDescriptions` and the value containing the data for the animation override.
        * You can make bigger animations and make the NPC emote or play a sound at certain points in the animation. Example [here](https://gist.github.com/spacechase0/55f5b8b75a47b5d4d6f790609f48d20c).
    * Crafting/Cooking Recipes - Stored in `spacechase0.SpaceCore/CraftingRecipeOverrides` and `spacechase0.SpaceCore/CookingRecipeOverrides`, these assets are both a dictionary, with the key being the ID of the corresponding recipe, and the value being an object with the following:
        * `ProductQualifiedId` - The qualified ID of the product
        * `ProductAmount` - How many of the product should be made
        * `Ingredients` - an array of objects containing the following
            * `Type` - either `"Item"` or `"ContextTag"`.
            * `Value` - Different depending on `Type`:
                * For `Item` type ingredients, the qualified ID of the ingredient
                * For `ContextTag` type ingredients, the context tags. Multiple can be specified separated by commas, which will mean any context tag in the list means the item works as this ingredient.
            * `ContextTagsRequireAll` - (default false) - if true and `Type` is `"ContextTag"`, then all context tags must be had for a valid ingredient. If false, then only one needs to be had for the ingredient to be considered valid.
            * `Amount` - The amount of this ingredient should be required.
            * `OverrideText` - You can override the text shown for the ingredient by specifying this. Required for `Type`=`"ContextTag"`
            * `OverrideTexturePath` - The path to texture to use for this ingredient. You can use a vanilla texture path, or one from your mod using the `{{InternalAssetKey}}` token. Required for `Type`=`"ContextTag"`
            * `OverrideTextureRect` - If using `OverrideTexturePath`, where on the texture should be displayed for this ingredient. Required for `Type`=`"ContextTag"`
    * Farm Types - Stored in `spacechase0.SpaceCore/FarmExtensionData`:
        * This lets you place buildings (with or without animals) and fences on the farm at creation. Example [here](https://gist.github.com/spacechase0/063505cabbed28dfa94b802b28857885).
    * Trigger Actions - Stored in `spacechase0.SpaceCore/TriggerActionExtensionData`, a dictionary with the key being the trigger action ID and the value being an object with the following fields.
        * `Times` - An list of the times the trigger action should be triggered. Example: `"Times": [ 630 ]`
            * The `Trigger` for this trigger action must be set to "Manual" for it to work.
* Spawnables - Spawn things on a trigger action. The trigger action actions to use are listed in the trigger action section further above.
    * First, an example with everything: https://gist.github.com/spacechase0/35453e1e7a0593d8d4f00246c1dd3990
    * Spawnables consist of spawnable definitions (the spawns themselves) and spawning groups (a group of spawn definitions that are spawned together, specified with a trigger action).
    * Spawnable definitions live in `spacechase0.SpaceCore/SpawnableDefinitions`. It's a dictionary with the key being the ID (referred to in spawn groups), and the value consisting of a `Type` field, with more fields depending on the type. (There is also a `Condition` GSQ on each one.) The types (with their additional fields listed in sub bullet points) are:
        * `SetPiece` - Place a set piece (randomized map patch). Set pieces are random sections from a map file that are then applied onto an existing location. They persist until cleared with the relevant trigger action. If applied to the location you are in, they won't show up until you leave and re-enter the location. (The vanilla volcano dungeon uses something similar to set pieces - it's even called set pieces internally).
            * `SetPiecesMap` - The map file to pull set pieces from. Use the `{{InternalAssetKey}}` token when specifying your custom maps.
            * `SetPieceSizeX` - The width of each set piece in the map file.
            * `SetPieceSizeY` - The height of each set piece in the map file.
            * `SetPieceCount` - The amount of set pieces in the map file.
            * Additionally, set pieces can use tile properties to trigger another spawn group. Place a tile on the `Paths` layer, and spsecify a tile property named `spacechase0.SpaceCore/TriggerSpawnGroup`. The value should be the spawn group to trigger at this tile.
        * `Forageable` - Spawn a forageable (either pick-up or artifact spot)
            * `ForageableItemData` - A list of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). A random one will be chosen according to the weights. (See the example from before for how to format weighted item spawn fields.) These must be objects (unless `ForageableIsTillSpot` is true).
            * `ForageableExpiresWeekly` - If the forageable should disappear at the end of Saturday like vanila forageables do. Default true.
            * `ForageableIsTillSpot` - If it should make an artifact spot and have to be dug with a hoe instead of picked up normally. Default false.
        * `Minable` - Spawn a minable, to be broken with a pickaxe or axe.
            * `MinableTool` - Either `Pickaxe` or `Axe`.
            * `MinableObjectId` - The object ID of the object this minable should look like.
            * `MinableHealth` - The amount of health the minable has. Each hit takes away an amount of health equal to the tool tier.
            * `MinableDrops` - A list of lists of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). How this works is it will pick one weighted item spawn from each list in the outer list. (See the example from before for how to format these.)
            * `MinableExperienceGranted` - How much skill experience to grant upon being broken. This will be mining experience for pickaxe minables, and foraging experience for axe minables.
        * `LargeMinable` - Spawn a larger-than-one-tile minable, to be broken with a pickaxe or axe.
            * `LargeMinableTool` - Either `Pickaxe` or `Axe`.
            * `LargeMinableRequiredToolTier` - The required tool tier to be able to break this large minable.
            * `LargeMinableHealth` - The amount of health the minable has.
            * `LargeMinableSizeX` - How many tiles wide this minable should be.
            * `LargeMinableSizeY` - How many tiles tall this minable should be.
            * `LargeMinableTexture` - The texture to use for the minable, or null for the vanilla springobjects tilesheet.
            * `LargeMinableSpriteIndex` - The inedx of the sprite in the texture. The indices are counted by 16x16 tiles, not by the size of the minable.
            * `LargeMinableDrops` - A list of lists of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). How this works is it will pick one weighted item spawn from each list in the outer list. (See the example from before for how to format these.)
            * `LargeMinableShavingDrops` - A list of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields) to use if someone hits the minable with a tool with the Shaving enchantment. A random one will be chosen according to the weights. (See the example from before for how to format weighted item spawn fields.)
            * `LargeMinableExperienceGranted` - How much skill experience to grant upon being broken. This will be mining experience for pickaxe minables, and foraging experience for axe minables.
        * `Breakable` - A breakable container, like the barrels or crates in the mines.
            * `BreakableBigCraftableId` - The ID of the big craftable this breakable should look like.
            * `BreakableHealth` - How much health this breakable should have.
            * `BreakbleHitSound` - The sound to make when hit. Default `woodWhack`.
            * `BreakbleBrokenSound` - The sound to make when broken. Default `barrelBreak`.
            * `BreakableDrops` - A list of lists of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). How this works is it will pick one weighted item spawn from each list in the outer list. (See the example from before for how to format these.)
        * `LootChest` - A chest that will drop items when opened, similar to the chests in the volcano dungeon.
            * `LootChestBigCraftableId` - The ID of the big craftable this loot chest should look like. Note that the sprite indices after the chest are used for the lid - check a vanilla chest for an example.
            * `LootChestDrops` - A list of lists of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). How this works is it will pick one weighted item spawn from each list in the outer list. (See the example from before for how to format these.)
        * `Furniture` - A furniture item, placed into the world.
            * `FurnitureQualifiedId` - The qualified item ID of the furniture to spawn.
            * `FurnitureRotation` - The amount of times it should be "rotated" (as if a player were placing it).
            * `FurnitureIsOn` - For lamps, if they should be on.
            * `FurnitureHeldObject` - A list of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields) to use for a single item to be sitting on the furniture (if it's a table). A random one will be chosen according to the weights. (See the example from before for how to format weighted item spawn fields.)
            * `FurnitureCanPickUp` - If the furniture should be able to be picked up or not. Default true. (Note that any held object WILL be able to be taken regardless of this field.)
        * `Monster` - A monster. This can be a vanilla type, or  a custom one if registered through the SpaceCore API.
            * `MonsterType` - The type of monster to spawn. Valid types are any of the following: `Bat`, `BigSlime`, `BlueSquid`, `Bug`, `DinoMonster`, `Duggy`, `DustSpirit`, `DwarvishSentry`, `Fly`, `Ghost`, `GreenSlime`, `Grub`, `HotHead`, `LavaLurk`, `Leaper`, `MetalHead`, `Mummy`, `RockCrab`, `RockGolem`, `Serpent`, `ShadowBrute`, `ShadowGirl`, `ShadowGuy`, `ShadowShaman`, `Shooter`, `Skeleton`, `Spiker`, `SquidKid` (C# mods can register additional types with the SpaceCore API.)
            * `MonsterName` - If specified, will reload the monster with stats from specified entry in `Data/Monsters`.
            * `MonsterTextureOverride` - If specified, override the spritesheet of the monster.
            * `MonsterDropOverride` - If specified, will replace the monster's drops with these. A list of lists of weighted [item spawn fields](https://stardewvalleywiki.com/Modding:Item_queries#Item_spawn_fields). How this works is it will pick one weighted item spawn from each list in the outer list. (See the example from before for how to format these.)
            * `MonsterAdditionalData` - Some monsters can have additional data to configure them:
                * `BigSlime`
                    * `Color` - The color of the slime, specified with `{ "R": 255, "G": 255, "B": 255 }`
                    * `HeldItemQualifiedId` - Qualified item ID of the item inside the slime
                * `Bug` - `IsArmored` can be set to true
                * `Ghost` - `IsPutrid` can be set to true
                * `GreenSlime` - `Color` can be set just like with BigSlime
                * `RockCrab` - `IsStickBug` can be set to true (preventing the armor from breaking)
                * `Serpent` - `SegmentCount` can be set to an integer
                * `Skeleton` - `IsMage` can be set to true
                * `Spiker` - `Direction` must be specified - it determines the direction it will move when a player approaches
        * `WildTree` - Spawn a fully grown wild tree.
            * `WildTreeType` - the ID of the wild tree, as specified in `Data/WildTrees`.
        * `FruitTree` - Spawn a fully grown fruit tree (with fruit if the season is correct in the location). This fruit tree will not drop a sapling when cut down.
            * `FruitTreeType` - the ID of the fruit tree, as specified in `Data/fruitTrees`.
    * Spawn groups live in `spacechase0.SpaceCore/SpawningGroups`. This is a dictionary with the key being the ID (referred to from trigger actions), and the value being an object containing the following fields:
        * `SpawnablesToSpawn` - A list of objects with the following fields:
            * `SpawnableIds` - A weighted list of IDs of spawnable definitions to chose from spawning.
            * `Minimum` - The minimum amount to spawn. (Each attempt will have 10 tries - if there's no room, that attempt is skipped.)
            * `Maximum` - The maximum amount to spawn.
* Dungeons - You can define custom dungeons like the mines or skull caverns.
    * There is an example [here](https://gist.github.com/spacechase0/8502b78aed4c5d84a41881e3b2e1b42b).
    * You can control entering and exiting the dungeon via tile actions and map properties.
        * Tile actions
            * `spacechase0.SpaceCore_DungeonEntrance dungeonId` - Has you enter the first floor of the dungeon. Works anywhere.
            * `spacechase0.SpaceCore_DungeonElevatorMenu dungeonId` - Opens the elevator for the dungeon dungeonId. The floors available are based on how deep you go, and defined in the dungeon data. Works anywhere.
            * `spacechase0.SpaceCore_DungeonLadderExit` - A normal exit for the dungeon - you will be taken to where is defined in the dungeon data. Works in a dungeon.
            * `spacechase0.SpaceCore_DungeonLadder` - Acts like a ladder in the mines, descending one floor. Works in a dungeon.
            * `spacechase0.SpaceCore_DungeonMineshaft` - Acts like a mineshaft in the skull cavern, descending a random amount of floors. Works in a dungeon.
        * Map properties
            * `spacechase0.SpaceCore_DungeonLadderEntrance x y` - Where the palyer is placed when entering the location via ladder or entering the dungeon.
            * `spacechase0.SpaceCore_DungeonElevatorEntrance x y` - Where the player is placed when entering the location via an elevator.
    * The dungeon data itself lives in `spacechase0.SpaceCore/Dungeons`, which is a dictionary with the dungeon ID being the key, and a model containing the following values:
        * `ViewportFollowsPlayer` - Default true. If true, the viewport will follow the player no matter where they are in the level (lines the mines). If false, the viewport will be clamped to the edges of the map.
        * `LadderExitLocation` - The location name of the place to be taken to when exiting through `spacechase0.SpaceCore_DungeonLadderExit`.
        * `LadderExitTile` - The position in the location above to to be taken to.
        * `ElevatorExitLocation` - The location name of the place to be taken to when exiting through floor 0 of `spacechase0.SpaceCore_DungeonElevatorMenu dungeonId`.
        * `ElevatorExitTile` - The position in the location above to be taken to.
        * `FloorsWithElevators` - A list of floors that the elevator will list as choices (if someone on that farm has been that deep before).
        * `SpawnLadders` - If this dungeon should spawn ladders when rocks are broken. Default true.
        * `SpawnMineshafts` - If this dungeon should spawn mineshafts when rocks are broken. Default false.
        * `LadderTileSheet` - The tilesheet dungeon ladders should show from. Default `"Maps/Mines/mine_desert"`.
        * `LadderTileIndex` - The tile index dungeon ladders should show. Default 173.
        * `MineshaftTileSheet` - The tilesheet dungeon mineshafts should show from. Default `"Maps/Mines/mine_desert"`.
        * `MineshaftTileIndex` - The tile index dungeon mineshafts should show. Default 174.
        * `AdditionalTimeMilliseconds` - How much time should slow, in milliseconds per ten minutes of in-game time, when in this dungeon in singleplayer. Default 2000.
        * `Regions` - Also a dictionary, with the string being the region ID (unique to this dungeon), and the values being a model containing the following:
            * `LevelRange` - An object with a `Begin` and `End` for which levels this region defines.
            * `MapPool` - A list of weighted strings containing map paths to choose from. Choices with higher weight are more likely to be chosen, proportionate to the weight of other choices.
                * For how to format these, check out the example.
            * `TriggerActionsOnEntry` - A list of trigger actions to run when entering the location.
                * If using SpaceCore's Spawnables system to populate the level, make sure to mark those trigger actions as `"HostOnly": true` (to prevent levels from getting repopulated when a new player enters) and `"MarkActionApplied": false` (to make sure it works more than once per save).
            * `LocationDataEntry` - What location to emulate from the `Data/Locations` asset. This defines things like location context, music, how fish get chosen, etc.
* Animations - You can animate textures by editing `"spacechase0.SpaceCore/TextureOverrides"`, which is a dictionary with the key being the ID of your animation, and the following information:
    * `TargetTexture` - The path to the file you want to animate.
    * `TargetRect` - The rectangle in the target file you want to animate. Example: `{ "X": 32, "Y": 48, "Width": 16, "Height": 16 }`
    * `SourceTexture` - The texture and frames you want to pull from for the animation, in the old DGA format. (The texture name followed by a colon, followed by a comma separated list of frame indices. Frame indices can optionally include a frame duration with @.)
        * Example (for Content Patcher): `"{{InternalAssetKey: assets/prismatic.png}}:0@5,1@5,2@5,3@5,4@5,5@5`
        * Another way of doing the above is using `..` to specify a sequence of frames that all get the same duration: `{{InternalAssetKey: assets/prismatic.png}}:0..5@5`
    * `SourceSizeOverride` - A Vector2 size. If specified, the sprite size to use for frames in `SourceTexture` (normally the size is the same as the size in `TargetRect`). This allows you to fit a larger image in place of a smaller vanilla image. (For example, fitting a 256x256 portrait into the default 64x64 spaces.)
    * `ChancePerTick` - The percent chance for an animation, in a range of 0 to 1 (ex. `0.10` = 10% chance per tick). This is the chance for the animation to *start when no animation is active*. If an animation has already been triggered, this value is ignored until the animation is finished.
* NPC Questions - Previously part of [Backstory Questions Framework](https://www.nexusmods.com/stardewvalley/mods/14451):
    * To edit the questions you can ask someone, edit the `spacechase0.SpaceCore/Questions` asset, which is a dictionary with the NPC name for the key, and the value being a list of objects with the following values:
        * `ID` - Required, must be unique, doesn't show to end users
        * `Weight` - How likely the question will show up compared to other questions. Higher values means more likely likely. Default 1.
        * `QuestionText` - The text for the question.
        * `AnswerText` - The text for the answer.
        * `CanRepeatQuestion` - If the question can be repeated. Default false.
        * `FriendshipModifier` - How much friendship the player gets from asking this question. Default 10.
        * `Condition` - The GameStateQuery condition for if this question will be a valid choice.
        * If this seems confusing, see the example pack on Backstory Questions Framework's mod page.
            * The asset name in there is different, since it was made when the feature was in the separate mod.
* New CP tokens -
    * `spacechase0.SpaceCore/CurrentlyInEvent` - true or false
    * `spacechase0.SpaceCore/CurrentEventId` - the current event ID, if in an event
    * `spacechase0.SpaceCore/QuestionsAsked` - a token which takes in an NPC name, and returns the questions asked (not including repeatable questions). (For use with the Backstory Questions feature.)
    * `spacechase0.SpaceCore/BooksellerInTown` - true or false
* New dialogue keys:
    * `HitBySlingshot_(O)ItemId` for if they get hit with a slingshot shot of the `(O)ItemId` item.
    * `HitBySlingshot_context_tag` for if they get hit with a slingshot shot of an item with the context tag `context_tag`.
* Guidebooks - You can add guidebooks with dynamic contents, openable via trigger action.
    * The trigger action to open them is `spacechase0.SpaceCore_OpenGuidebook guidebookId optionalChpaterId optionalPageId`
    * The guidebooks are stored in `spacechase0.SpaceCore/Guidebooks`. You can find example data [here](https://gist.github.com/spacechase0/18743e1ead1c33fadc807040fdb3626c).
    * Each entry must have the following:
        * `Title` - The title of the guidebook, shown at the top. You should use `{{i18n}}` here.
        * `PageTexture` - Optional - if specified (use `{{InternalAssetKey}}`), use a specific texture for the background of your pages in the book. The size of the image will be the size of the entire page, without scaling.
        * `PagePadding` - Optional, a vector2 that defaults to "28, 28" - The distance from the edges of the page texture the contents will show up.
        * `PageSize` - Optional, defaults to "600, 500" - If there is no `PageTexture`, this size will be used for each page.
        * `DefaultChapter` - The default chapter to open when one isn't specified in the trigger action.
        * `Chapters` - A dictionary of chapter ID to object containing the following fields:
            * `Name` - The chapter name, shown just below the book title, and when hovering over the tab icon. You should use `{{i18n}}` here.
            * `TabIconTexture` - The texture to use for the tab icon (use `{{InternalAssetKey}}`) on the side of the book if the chapter is unlocked.
            * `TabIconRect` - A rectangle for the part of the texture to use for the tab icon. If not specified, the whole texture is used.
            * `TabIconScale` - Optional, default 4 - A scale factor for how big the tab icon should be.
            * `Condition` - Optional, default `TRUE` - A GameStateQuery to use for if this chapter should be unlocked (and have a visible tab).
            * `Pages` - A list of pages, which are objects containing the following values:
                * `Id` - Optional, only needed if you want to link to a specific page. The ID to use for the `[link]` tag.
                * `Contents` - The contents of the page (see further below for formatting). You should use `{{i18n}}` here.
                * `Condition` - Optional, default `TRUE` - A GameStateQuery for if this page should be available.
     * Page contents formatting - You should probably use newlines in this field. (In VSCode, set the format of your file to "JSON Lines" to hide errors from this - though this will cause comments to error). You can use the following tags inside the content, and you can nest these tags as well (formatted like BBCode - see the example gist's i18n for examples).
        * `[center]centered text[/center]` - Puts the text `centered text` centered horizontally on the page. May not work when more than one line of text.
        * `[center=width]centered text[/center]` - Puts the text `centered text` centered horizontally within the next `width` pixels (manually useful for when manually positioning things). May not work when more than one line of text.
        * `[font=FontName]text[/font]` - Puts the text `text` on the page in the `FontName` font. Valid fonts are: `default`, `dialogue`, `tiny`
        * `[color=Color]text[/color]` - Puts the text `text` on the page in the `Color` color. Valid color syntax can be found [on the Stardew wiki](https://stardewvalleywiki.com/Modding:Common_data_field_types#Color).
        * `[image=PathToImageAsset:SourceRect:Scale]` - Adds a centered image to the page. There are three parameters, separated by `:` (though the second and third parameters are optional).
            * `PathToImageAsset` - This is the asset path to the image you want to use. Normally you'd use `{{InternalAssetKey}}` here.
                * If you're doing this in your i18n and don't have access to tokens, you can use the undocumented syntax SMAPI uses internally: `SMAPI/mod.id/path_in_mod` - `mod.id` being your mod ID, and `path_in_mod` should be the path to your image file. (Note that everything except the initial `SMAPI` should be lowercase, even if it isn't normally lowercase.)
            * `SourceRect` - If specified, four values separated by commas indicating the X, Y, Width, and Height of the image to use. (You can also just put `null` to use the whole thing.)
            * `Scale` - The scale of the image to use. Defaults to 4 times the normal size, like most game assets.
        * `[icon=PathToImageAsset:SourceRect:Scale]` - Adds a inline image to the page. There are three parameters, separated by `:` (though the second and third parameters are optional).
            * `PathToImageAsset` - This is the asset path to the image you want to use. Normally you'd use `{{InternalAssetKey}}` here.
                * If you're doing this in your i18n and don't have access to tokens, you can use the undocumented syntax SMAPI uses internally: `SMAPI/mod.id/path_in_mod` - `mod.id` being your mod ID, and `path_in_mod` should be the path to your image file. (Note that everything except the initial `SMAPI` should be lowercase, even if it isn't normally lowercase.)
            * `SourceRect` - If specified, four values separated by commas indicating the X, Y, Width, and Height of the image to use. (You can also just put `null` to use the whole thing.)
            * `Scale` - The scale of the image to use. Defaults to 4 times the normal size, like most game assets.
        * `[link=ChapterID/PageID]Click here[/link]` - Puts the text `Click here` that, when clicked, will link to the specified page in the specified chapter.
            * You can also use this to put HTTP links in that will open in the default browser of the user when clicked (this will force a tooltip of the url the link will go to).
        * `[action=TriggerActionActionHere]Click here[/action]` - Puts the text `Click here` that, when clicked, will run the specified trigger action when clicked.
        * `[onceaction=TriggerActionActionHere]Click here[/action]` - Same as `[action]`, but will make the clicked elements disappear after they are used. (This will only affect the clicked element, not everything inside the `[onceaction]` block.)
        * `[texttooltip=Title:Description]meow[/texttooltip]` - Make the specified elements have a tooltip when hovered over with the specified values. If you don't want a title for the tooltip, just make it empty, ie. `[texttooltip=:Description]`.
        * `[itemtooltip=QualifiedItemId]meow[/itemtooltip]` - Make the specified elements have a tooltip when hovered, matching the tooltip of the specified item. `QualifiedItemId` needs to be a qualified item ID, such as `(O)74`.
        * `[imagetooltip=Title:PathToImageAsset:SourceRect:Scale]` - Make the specified elements have a tooltip when hovered showing an image with a title.
            * `Title` - If specified, the title above the image in the tooltip. If you don't want a title for the tooltip, just make it empty, ie. `[imagetooltip=:PathToImageAsset:SourceRect:Scale]`.
            * `PathToImageAsset` - This is the asset path to the image you want to use. Normally you'd use `{{InternalAssetKey}}` here.
                * If you're doing this in your i18n and don't have access to tokens, you can use the undocumented syntax SMAPI uses internally: `SMAPI/mod.id/path_in_mod` - `mod.id` being your mod ID, and `path_in_mod` should be the path to your image file. (Note that everything except the initial `SMAPI` should be lowercase, even if it isn't normally lowercase.)
            * `SourceRect` - If specified, four values separated by commas indicating the X, Y, Width, and Height of the image to use. (You can also just put `null` to use the whole thing.)
            * `Scale` - The scale of the image to use. Defaults to 4 times the normal size, like most game assets.
        * `[if=Condition]meow[/if]` - Puts the text `meow` in the page, but only if the game state query `Condition` passes.
        * `[else]meow2[/else]` - If the most recent `[if]` that ended failed, put the text `meow2` on the page.
        * `[nospacing]...[/nospacing]` - Anything inside a `[nospacing]` will not move the "cursor" around when placing elements.
            * This means `[image]`s will no longer be automatically centered.
            * It also means any text and `[icon]` elements will not cause subsequent elements to be moved after it.
            * This is useful for manually positioning things on a page (see next two tags).
        * `[nextposition=X,Y]` - Will set the position of the next element placed to the specified value. If `X` or `Y` is set to `null`, then it won't override that coordinate (useful if you want to only override one coordinate but not the other).
        * `[offset=X,Y]` - Moves the next element placed from where it normally would be by the specified amount.

The rest of the features assume you understand C# and the game code a little bit (and are only accessible via C#):
* In the API provided through SMAPI's mod registry (see mod source for interface you can copy):
    * `string[] GetCustomSkills()` - Returns an array of skill IDs, one for each registered skill.
    * `int GetLevelForCustomSkill(Farmer farmer, string skill)` - Gets the base level of the given `skill` for the given `farmer`.
    * `int GetBuffLevelForCustomSkill(Farmer farmer, string skill)` - Gets the buff level of the given `skill` for the given `farmer`.
    * `int GetTotalLevelForCustomSkill(Farmer farmer, string skill)` - Gets the base + buff level of the given `skill` for the given `farmer`.
    * `void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt)` - Adds `amt` experience to the given `skill` for the given `farmer`.
    * `Texture2D GetSkillPageIconForCustomSkill(string skill)` - Gets the skill page icon for the given `skill`.
    * `Texture2D GetSkillIconForCustomSkill(string skill)` - Gets the skill icon for the given `skill`.
    * `int GetProfessionId(string skill, string profession)` - Gets the integer ID of the given `profession` (for `Farmer.professions`) for the given skill.
    * `void RegisterSerializerType(Type type)` - Register a `type` as being valid for the vanilla serializer. Must have the attribute `XmlType` applied, with the parameter starting with `"Mods_"`, ie. `[XmlType("Mods_AuthorName_MyCustomObject")]`.
    * `void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter)` - Register a virtual property, attaching itself to a vanilla object for serialization.
        * `declaringType` is the type to attach to
        * `name` is the name of the property.
        * `propType` is the type of the property you're adding.
        * `getter` is a `MethodInfo` pointing to your static function acting as a getter. It take an instance of the type corresponding to `declaringType`, and return a value of the type corresponding to `propType`.
        * `setter` is a `MethodInfo` pointing to your static function acting as a setter. It take an instance of the type corresponding to `declaringType` and a value of the type corresponding to `propType`.
    * An event: `AdvancedInteractionStarted`, which passes the NPC as the `object sender` and an `Action<string, Action>` as the event argument, which you call with a string for what string to show for you choice, and an Action for what to happen when it is chosen. (See [Backstory Questions Framework](https://www.nexusmods.com/stardewvalley/mods/14451), a mod now integrated into SpaceCore, for an example on usage).
    * `void RegisterSpawnableMonster(string id, Func<Vector2, Dictionary<string, object>, Monster> monsterSpawner)` - Register a spawnable monster with the spawnables system.
    * `List<int> SpaceCore.GetLocalIndexForMethod(MethodBase meth, string local)` - gets the indices of all variables in a method using a given name. Used for transpilers.
    * Virtual currency manipulation functions (corresponding to the content pack feature):
        * `List<string> GetVirtualCurrencyList()` - Get a list of virtual currency IDs that are currently active.
        * `bool IsVirtualCurrencyTeamWide(string currency)` - Check if the virtual currency is a team-wide currency or not.
        * `int GetVirtualCurrencyAmount(Farmer who, string currency)` - Get the current amount of virtual currency the player has. (You still need to pass in a player for team wide currencies.)
        * `void AddToVirtualCurrency(Farmer who, string currency, int amount)` - Add an amount of virtual currency to the player. (You still need to pass in a player for team wide currencies.) You can pass in a negative value to consume some - the amount the player has will never be less than zero.
    * Custom equipment slot functions:
        * Note: The IDs are global, across all mods. Please include your mod unique ID in your slot ID.
        * `void RegisterEquipmentSlot(IManifest modManifest, string globalId, Func<Item, bool> slotValidator, Func<string> slotDisplayName, Texture2D bgTex, Rectangle? bgRect = null)` - Register an equipment slot to show on the additional equipment menu. (See player features section.)
            * `IManifest modManifest` - Your mod's `ModManifest` field from your ModEntry class. Currently unused, might be used at some point to show what mod a slot comes from.
            * `string globalId` - The id of the slot, must be unique across all mods.
            * `Func<Item, bool> slotValidator` - The function to validate if an item will be accepted in the slot. Make sure it accepts `null`.
            * `Func<string> slotDisplayName` - The display name of the slot, shown on the extra equipment menu.
            * `Texture2D bgTex` - The texture containing the background iamge of the slot.
            * `Rectangle? bgRect` - The rectangle of the texture for the slot, or null to use the whole texture.
        * `Item GetItemInEquipmentSlot(Farmer farmer, string globalId)` - Get the item in the specified equipment slot for the specified farmer.
        * `void SetItemInEquipmentSlot(Farmer farmer, string globalId, Item item)` - Set the item in the specified equipment slot for the specified farmer. Note: This only works for your local farmer or offline ones, not other online players.
        * `bool CanItemGoInEquipmentSlot(string globalId, Item item)` - If the item can go in the requested slot.
* Events, located in SpaceCore.Events.SpaceEvents:
    * `OnBlankSave` - Occurs before loading starts. Custom locations can be added here so that they retain player data.
    * `ShowNightEndMenus` - Right before the shipping menu, level up menus, etc. pop up so you can add your own.
        * `EventArgsShowNightEndMenus` - not sure why this uses a custom argument tbh, it has nothing in it
    * `ChooseNightlyFarmEvent` - This is specifically for the 'shared' farm events. You can put your own here.
        * `EventArgsChooseNightlyFarmEvent`
            * `FarmEvent NightEvent` - The event that will be used. Can be modified to change which one runs.
    * `OnItemEaten` - Check player.itemToEat for what they just ate.
        * `sender` - The farmer that ate.
    * `BeforeGiftGiven` - Called before a gift is given. Can be used to cancel the default gift given behavior and do your own stuff.
        * `sender` - The farmer that gave the gift.
        * `EventArgsBeforeReceiveObject`
            * `bool Cancel` - Set to true to cancel default behavior.
            * `NPC Npc` - The NPC the gift is given to.
            * `StardewValley.Object Gift` - The gift that was given.
    * `AfterGiftGiven` - Called after a gift is given. Can be used if you want special behavior after it is received.
        * `sender` - The farmer that gave the gift.
        * `EventArgsGiftGiven`
            * `NPC Npc` - The NPC the gift is given to.
            * `StardewValley.Object Gift` - The gift that was given.
    * `BeforeWarp` - Called before a warp is done. Can be used to cancel or modify it.
        * `sender` - The player that warped. (Always Game1.player, for now)
        * `EventArgsBeforeWarp`
            * `bool Cancel` - Set to true to cancel default behavior.
            * `LocationRequest WarpTargetLocation` - Vanilla class passed around when warping, has information on where we're going.
            * `int WarpTargetX` - The x position of the tile to warp to.
            * `int WarpTargetY` - The y position of the tile to warp to.
            * `int WarpTargetFacing` - The direction the farmer will be facing after warp.
    * `BombExploded` - When a bomb explodes in a location. Useful for zelda-like puzzle walls.
        * `sender` - The farmer who placed the bomb, if any,
        * `EventArgsBombExploded`
            * `Vector2 Position` - The tile position of the bomb that exploded.
            * `int Radius` - The radius of the explosion.
    * `OnEventFinished` - When an event cutscene finishes. Use `Game1.CurrentEvent` to check which one.
    * `AddWalletItems` Event for adding wallet items to `NewSkillsPage` before controller logic is built. USeful for adding custom wallet items.
        * `sender` - The `NewSkillsPage` instance.
        * This one is a little different. See [here](https://github.com/spacechase0/StardewValleyMods/blob/bbedf45195732812d6a6483fc631a4d4121a6094/MoonMisadventures/Mod.cs#L400) for an example.
* The skill API. You subclass `Skill` and implement its functions to register a custom skill that shows in the skills tab on the game pause menu, and will show with Experience Bars. See Skills.cs for more details.
    * Register a skill by calling `SkillAs.RegisterSkill(Skill skill)`, with `skill` being your `Skill` instance.
    * `string GetName()` - Returns the localized name of your skill.
    * `Texture2D Icon` - the icon for use with the skill level up menu, and Experience Bars.
    * `Texture2D SkillsPageIcon` - the i con for the skills page.
    * `IList<Profession> Professions` - add your implementations of `Profession` here.
        * Subclasses of `Profession` need to implement:
            * `Texture2D Icon` - set this to the icon for your profession.
            * `string GetName()` - returns the localized name of your profession.
            * `string GetDescription()` - returns the localized description of your profession.
            * `void DoImmediateProfessionPerk()` - optional, apply an effect immediately upon being chosen.
            * `void UndoImmediateProfessionPerk()` - optional, undo an effect from above for when the player uses the sewer statue to change professions.
    * `int[] ExperienceCurve` - The experience curve for levels 1 through 10.
        * You can put more or less, but it might look funny on the skills page.
    * `IList<ProfessionPair> ProfessionsForLevels` - Put profession choices here.
        * A `ProfessionPair` takes the `int level` requirement, then `Profession first` and `Profession second` for its choices, and an optional `Profession req` that is required for this branch.
    * `Color ExperienceBarColor`
    * `List<string> GetExtraLevelUpInfo(int level)` - optional, extra text to show upon leveling
    * `string GetSkillPageHoverText(int level)` - optional, extra text to show when hovering on the skills page
    * `void DoLevelPerk(int level)` - optional, apply a some code immediately upon leveling
    * `bool ShouldShowOnSkillsPage`
    * Custom buffs for your skill you can have by adding `"spacechase0.SpaceCore.SkillBuff.<skill_ID_here>": "<value>"` as a custom field to the buff for food or drink.
    * Your custom skill can have level up crafting and cooking recipes by just adding your skill ID to where the vanilla skill ID would be.
    * By adding the skill id to the context tags of a book object, players can read the book to gain exp in the custom skill.
* Custom crafting recipes, for when you want more flexibility (like using non-Object item types).
    * You subclass `CustomCraftingRecipe` and register it by doing `CustomCraftingRecipe.CraftingRecipes.Add( key, new MyCustomCraftingRecipeSubclass() )`.
        * If it is a cooking recipe, you use `CustomCraftingRecipe.CookingRecipes` instead.
        * An entry in the corresponding vanilla data file is still needed. Example for crafting recipes: `("Test Recipe", "0 1//0 1/false//Test Recipe")`
    * `string Name { get; }` - the display name of the crafting recipe, or null if it should use what is in the data file.
    * `string Description { get; }` - the description of the crafting recipe.
    * `Texture2D IconTexture { get; }` - the texture of the icon for the recipe
    * `Rectangle? IconSubrect { get; }` - the subrect of the texture for the recipe icon, if any
    * `IngredientMatcher[] Ingredients`
        * Use `ObjectIngredientMatcher(string index, int quantity)` if you want to match `StardewValley.Object` instances.
        * Implement `IngredientMatcher` for other things:
            * `string DisplayName { get; }` - Display name of this ingredient.
            * `Texture2D IconTexture { get; }` - the texture of the icon for the ingredient
            * `Rectangle? IconSubrect { get; }` - the subrect of the texture for the ingredient icon, if any
            * `int Quantity { get; }` - the amount of this ingredient needed
            * `int GetAmountInList(IList<Item> items)` - return the amount of matching ingredients in the given list
            * `void Consume(IList<Chest> additionalIngredients)` - Consume the ingredients from the player inventory and from `additionalIngredients`
* Custom forge recipes
    * Subclass `CustomForgeRecipe` and register it by doing `CustomForgeRecipe.Recipes.Add( new MyCustomForgeRecipeSubclass() )`.
        * `IngredientMatcher` - different from the `CustomCraftingRecipe` on, because it only needs two (slightly different) functions:
            `bool HasEnoughFor(Item item)` - if the item in the slot is enough for the craft
            `void Consume(ref Item item)` - Consume the proper amount from the corresponding slot. Set to null if you use it all up.
    * `IngredientMatcher BaseItem { get; }` - for the left slot
    * `IngredientMatcher IngredientItem { get; }` - for the right slot
    * `int CinderShardCost { get; }` - for how many cinder shards the recipe costs
    * `Item CreateResult(Item baseItem, Item ingredItem)` - for creating the resulting item from the base and ingredient items
* Custom tool class will work in the vanilla `Data/Tools` asset, if they are added to the SpaceCore serializer API.
* UI Framework
    * This one is hard to document thoroughly, so your best bet is to look through the C# source code.
        * It's stored [here](https://github.com/spacechase0/StardewValleyMods/tree/develop/SpaceShared/UI).
    * Despite being in SpaceShared, this will be in the public code for referencing in SpaceCore (hence the #if stuff).
    * Generally, you create a `RootElement` that you put everything under, and set the `RootElement`s local position to where your window starts.
    * Every element has a local position, which offsets it from its parent's global position.
    * This is the same UI framework that GMCM uses.
    * SpaceCore also includes a way to load these from an XML file, using the `UiDeserializer` class.
        * It takes four functions in the constructor:
            * `Func<string, string>` - loads text files, example: `(path) => File.ReadAllText( Path.Combine( Helper.DirectoryPath, path ) )`
            * `Func<string, Texture2D>` - loads image files, example: `(path) => Helper.ModContent.Load<Texture2D>( path )`
            * `Func<string, string>` - optional, preprocesses the loaded text file before deserializing, example that loads from your i18n file like Content Patcher's `i18n` token (with no arguments): `(s) => Regex.Replace( s, @"\{\{i18n:([a-zA-Z0-9_\-]+)\}\}", (m) => Helper.Translation.Get( m.Groups[0].Value.Trim() ) )`
            * `Func<string, bool>` - optional, processes a conditon string, used for "When" as an attribute on an element.
        * You then call the `LoadFromFile` function with the path to the XML file.
            * There is an overload of this function that contains an additional `out List<Element>` parameter which contains all elements created during deserialization.
        * It supports any type in the UI framework, and custom ones that you add to your `UiDeserializer` instance's `Types` property.
        * It supports a special element called "Include", which will include elements from another file in its place, example: `<Include File="assets/container-of-cats.xml">`
        * It supports two special attributes "CenterH" and "CenterV", which will center the element horizontally or vertically in its parent container when set, example: `<Label LocalPosition="0, 50" String="Meow?" CenterH="true" />`
        * Another special attribute is "When", which will call your condition processor (fourth argument in the constructor) and exclude the element when it returns false.
        * Every `Element` made will have its `UserData` field filled with an instance of the `UiExtraData` class, which contains the following:
            * `Id` - a `string` that contains what was in the "Id" attribute of the element (useful with the overload of `LoadFromFile` that gives you a list containing all elements made)
            * `ExtraFields` - a `Dictionary<string, string>` that contains all the attributes you put on the element in the XML that weren't used in deserialition.
            * `UserData` - an `object` field for you to store whatever you want in
* Content Engine
    * Also hard to document, go read the source ([here](https://github.com/spacechase0/StardewValleyMods/tree/develop/SpaceShared/Content)) or look at Moon Misadventures Redux for an example.
* Some other things that will remain undocumented because they will be removed soon.

## Compatibility
Compatible with Stardew Valley 1.6.0+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
