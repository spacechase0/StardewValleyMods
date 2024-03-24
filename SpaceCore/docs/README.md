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

Provided functionality for content pack authors:
* New GameStateQuery queries:
    * Every custom skill registered through the C# API automatically registers a `PLAYER_<SKILLID_IN_CAPS>_LEVEL` query matching the vanilla ones (such as PLAYER_FARMING_LEVEL).
    * `NEARBY_CROPS radius cropId` - Only usable in CropExtensionData YieldOverrides PerItemCondition entries. Checks for fully grown crops of a particular type in a certain radius.
* New tile action - `spacechase0.SpaceCore_TriggerAction triggerActionId` - for running a trigger action, set the Trigger to "Manual"
* New touch action - `spacechase0.SpaceCore_TriggerAction triggerActionId` - for running a trigger action, set the Trigger to "Manual"
* New trigger actions
    * `spacechase0.SpaceCore_OnItemUsed` - use item GSQ conditions to check the right item
    * `spacechase0.SpaceCore_OnItemEaten` - use item GSQ conditions to check the right item
* New trigger action actions
    * `spacechase0.SpaceCore_PlaySound sound local` - `sound` = the cue ID, `local` = `true` if everyone near the player should hear it, `false` otherwise
    * `spacechase0.SpaceCore_ShowHudMessage "message goes here"`
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
        * `UseForTriggerAction` - True to run a trigger action upon use, false otherwise. Default false.
        * `GiftableToNpcDisallowList` - A dictionary of NPC names to messages that should show when you try to gift the item to them, instead of them receiving the gift.
        * `GiftableToNpcAllowList` - A dictionary of NPC names to `true` for if you want that NPC allowed. If set, any NPCs not listed here will not be able to receive the gift, and instead will show the message from the folowing field.
        * `GiftedToNotOnAllowListMessage` - The message to show for when the item is gifted to someone not on the allow list. Required if `GiftableToNpcAllowList` is set. (The disallow list is checked first, so you can still allow specific responses by certain NPCs.)
    * Crops - These are in `spacechase0.SpaceCore/CropExtensionData`
        * `YieldOverrides` - A little complex, but you can override each crop phase's harvestability with experience gained, the new phase it goes to, and the drops it has (including conditional drops). Example [here](https://gist.github.com/spacechase0/79f95bcd46160da9e52f5bc0c71329f4).
    * Weapons - Stored in the `CustomFields` on the weapon data asset object:
        * `CanBeTrashed` - true/false, also prevents dropping, default true
    * Furniture - Stored in the asset `"spacechase0.SpaceCore/FurnitureExtensionData"`, which is a dictionary with the key being the furniture ID, and the value being an object containing the following fields:
        * `TileProperties` - A dictionary of tile coordinates to a dictionary of layers to a dictionary of tile properties. Just look at [this example](https://gist.github.com/spacechase0/ea6db01284157d408d9f359f141a0d65).
        * `DescriptionOverride` - Description override for the furniture.
    * Shops - Stored in the asset `"spacechase0.SpaceCore/ShopExtensionData"`, which is a dictionary with the key being the shop ID, and the value being an object containing the following fields:
        * `Tabs` - The options are `"None"`, `"Catalogue"`, `"FurnitureCatalogue"`, and `"Custom"`. For custom, see the next field.
        * `CustomTabs` - A list consisting of the following object (see [this example](https://gist.github.com/spacechase0/8a80b22655f624d9854486bfbe5abc7e)):
            * `Id` - The ID of this tab, must be unique (used by Content Patcher)
            * `IconTexture` - The path to the texture to use for the tab. Use the `{{InternalAssetKey: }}` token for your own assets.
            * `IconRect` - The subrect of the texture to use for the tab. An object containing `X`, `Y`, `Width`, `Height` (all in pixels).
            * `FilterCondition` - A GameStateQuery condition used for filtering the items. Use `TRUE` to just get everything (useful for your first tab). Example to get all seeds: `"ITEM_CATEGORY Input -74"`
    * NPCs - Stored in the asset `"spacechase0.SpaceCore/NpcExtensionData"`, which is a dictionary with the key being an NPC name, and the value being an object containing the following fields:
        * `GiftEventTriggers` - A dictionary with the keys being an object, and the values being an event to trigger when that item is given to the NPC.
            * The "event to trigger" needs to be the full event key (ID and preconditions) used in the events data file, so that SpaceCore can find the event.
            * The preconditions are ignored.
            * It will only check the current location, so make sure to only have the event in this data file when in the right location.
                * For Content Patcher users: You can do this using `"When": { "LocationName": "Town" }, "Update": "OnLocationChange"` on your patch.
            * The event can reoccur if the item is given again.
                * For Content Patcher users: If you don't want this behavior, make sure to add a `HasSeenEvent` event condition to your `"When"` block for the patch.
        * `IgnoreMarriageSchedule` - true/false, defaults to false
    * Crafting/Cooking Recipes - Stored in `spacechase0.SpaceCore/CraftingRecipeOverrides` and `spacechase0.SpaceCore/CookingRecipeOverrides`, these assets are both a dictionary, with the key being the ID of the corresponding recipe, and the value being an object with the following:
        * `ProductQualifiedId` - The qualified ID of the product
        * `ProductAmount` - How many of the product should be made
        * `Ingredients` - an array of objects containing the following
            * `Type` - either `"Item"` or `"ContextTag"`.
            * `Value` - Different depending on `Type`:
                * For `Item` type ingredients, the qualified ID of the ingredient
                * For `ContextTag` type ingredients, the context tags. Multiple can be specified separated by commas, which will mean any context tag in the list means the item works as this ingredient.
            * `Amount` - The amount of this ingredient should be required.
            * `OverrideText` - You can override the text shown for the ingredient by specifying this. Required for `Type`=`"ContextTag"`
            * `OverrideTexturePath` - The path to texture to use for this ingredient. You can use a vanilla texture path, or one from your mod using the `{{InternalAssetKey}}` token. Required for `Type`=`"ContextTag"`
            * `OverrideTextureRect` - If using `OverrideTexturePath`, where on the texture should be displayed for this ingredient. Required for `Type`=`"ContextTag"`
    * Farm Types - Stored in `spacechase0.SpaceCore/FarmExtensionData`:
        * This lets you place buildings (with or without animals) and fences on the farm at creation. Example [here](https://gist.github.com/spacechase0/063505cabbed28dfa94b802b28857885).
* Animations - You can animate textures by editing `"spacechase0.SpaceCore/TextureOverrides"`, which is a dictionary with the key being the ID of your animation, and the following information:
    * `TargetTexture` - The path to the file you want to animate.
    * `TargetRect` - The rectangle in the target file you want to animate. Example: `{ "X": 32, "Y": 48, "Width": 16, "Height": 16 }`
    * `SourceTexture` - The texture and frames you want to pull from for the animation, in the old DGA format. (The texture name followed by a colon, followed by a comma separated list of frame indices. Frame indices can optionally include a frame duration with @.)
        * Example (for Content Patcher): `"{{InternalAssetKey: assets/prismatic.png}}:0@5,1@5,2@5,3@5,4@5,5@5`
        * Another way of doing the above is using `..` to specify a sequence of frames that all get the same duration: `{{InternalAssetKey: assets/prismatic.png}}:0..5@5`
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

The rest of the features assume you understand C# and the game code a little bit (and are only accessible via C#):
* In the API provided through SMAPI's mod registry (see mod source for interface you can copy):
    * `string[] GetCustomSkills()` - Returns an array of skill IDs, one for each registered skill.
    * `int GetLevelForCustomSkill(Farmer farmer, string skill)` - Gets the level of the given `skill` for the given `farmer`.
    * `void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt)` - Adds `amt` experience to the given `skill` for the given `farmer`.
    * `int GetProfessionId(string skill, string profession)` - Gets the integer ID of the given `profession` (for `Farmer.professions`) for the given skill.
    * `void RegisterSerializerType(Type type)` - Register a `type` as being valid for the vanilla serializer. Must have the attribute `XmlType` applied, with the parameter starting with `"Mods_"`, ie. `[XmlType("Mods_AuthorName_MyCustomObject")]`.
    * `void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter)` - Register a virtual property, attaching itself to a vanilla object for serialization.
        * `declaringType` is the type to attach to
        * `name` is the name of the property.
        * `propType` is the type of the property you're adding.
        * `getter` is a `MethodInfo` pointing to your static function acting as a getter. It take an instance of the type corresponding to `declaringType`, and return a value of the type corresponding to `propType`.
        * `setter` is a `MethodInfo` pointing to your static function acting as a setter. It take an instance of the type corresponding to `declaringType` and a value of the type corresponding to `propType`.
    * An event: `AdvancedInteractionStarted`, which passes the NPC as the `object sender` and an `Action<string, Action>` as the event argument, which you call with a string for what string to show for you choice, and an Action for what to happen when it is chosen. (See [Backstory Questions Framework](https://www.nexusmods.com/stardewvalley/mods/14451), a mod now integrated into SpaceCore, for an example on usage).
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
    * *Also hard to document, go read the source ([here](https://github.com/spacechase0/StardewValleyMods/tree/develop/SpaceShared/Content)) or look at Moon Misadventures Redux for an example.
* Some other things that will remain undocumented because they will be removed soon.

## Compatibility
Compatible with Stardew Valley 1.6.0+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
