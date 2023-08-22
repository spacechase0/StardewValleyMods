**SpaceCore** is a [Stardew Valley](http://stardewvalley.net/) framework mod used by some of my
other mods.

## Install
1. Install the latest version of [SMAPI](https://smapi.io).
2. Install [this mod from Nexus Mods](http://www.nexusmods.com/stardewvalley/mods/1348).
3. Run the game using SMAPI.

## Use
* **For players:** just install the mod normally, and it'll work for the mods that need it.
* **For mod authors:** SpaceCore's functionality may change without warning (but probably won't).

Provided functionality (this assumes you understand C# and the game code a little bit):
* Some new event commands:
    * `damageFarmer amount`
    * `setDating npc`
    * `totemWarpEffect tileX tileY totemSpriteSheetPatch sourceRectX,SourceRectY,sourceRectWidth,sourceRectHeight` - will need to delay after this command if you want to wait for the animation to be done
    * `setActorScale actor factorX factorY` - reset with factorX/Y = 1 - this will probably not be positioned right after scaling and will need a position offset afterwards
    * `cycleActorColors actor durationInSeconds color [color...]` color=`R,G,B` - specify 1 or more. Specifying 1 will just mean it stays that color instead of cycling
    * `flash durationInSeconds`
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
        * Use `ObjectIngredientMatcher(int index, int quantity)` if you want to match `StardewValley.Object` instances.
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
* ExtEngine - scripting in content packs
    * TODO: Document this once it is more complete
* Vanilla Asset Expansions
    * Objects - These are in the asset `spacechase0.SpaceCore/ObjectExtensionData`, which is a dictionary with the key being an object's unqualified item ID, and the value being an object containing the following fields:
        * `CategoryTextOverride` - string, default null
        * `CategoryColorOverride` - same format as Json Assets colors, default null
        * `HideFromShippingCollection` - true/false, default false
        * `CanBeTrashed` - true/false, also prevents dropping, default true
        * `CanBeGifted` - true/false, default true
        * `CanBeShipped` - true/false, default true
        * `EatenHealthRestoredOverride` - integer, override how much health is restored on eating this item, default null (use vanilla method of calculation)
        * `EatenStaminaRestoredOverride` - integer, override how much stamina is restored on eating this item, default null (use vanilla method of calculation)
        * `MaxStackSizeOverride` - integer, override the max stack size of your item, default null (use vanilla amount)
    * Weapons - Stored in the `CustomFields` on the weapon data asset object:
        * `CanBeTrashed` - true/false, also prevents dropping, default true
    * Wallet items - You can now add items to the player wallet if they have a specified mail flag. Example [here](https://gist.github.com/spacechase0/a8f52196965ff630fc5bbcc6528bd9e5). It's a dictionary with the key being the mail flag, and the value being an object that contains:
        * `HoverText` - The text to show on hover
        * `TexturePath` - The texture to use (you can use Content Patcher's `{{InternalAssetKey}}` token)
        * `SpriteIndex` - The index in the texture to use for the sprite, default 0
    * NPCs - Stored in the asset `"spacechase0.SpaceCore/NpcExtensionData"`, which is a dictionary with the key being an NPC name, and the value being an object containing the followingfields:
        * `GiftEventTriggers` - A dictionary with the keys being an object, and the values being an event to trigger when that item is given to the NPC.
            * The "event to trigger" needs to be the full event key (ID and preconditions) used in the events data file, so that SpaceCore can find the event.
            * The preconditions are ignored.
            * It will only check the current location, so make sure to only have the event in this data file when in the right location.
                * For Content Patcher users: You can do this using `"When": { "LocationName": "Town" }, "Update": "OnLocationChange"` on your patch.
            * The event can reoccur if the item is given again.
                * For Content Patcher users: If you don't want this behavior, make sure to add a `HasSeenEvent` event condition to your `"When"` block for the patch.
* Some other things that will remain undocumented because they will be removed soon.

## Compatibility
Compatible with Stardew Valley 1.5.5+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
