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
* In the API provided through SMAPI's mod registry (see mod source for interface you can copy):
    * `string[] GetCustomSkills()` - Returns an array of skill IDs, one for each registered skill.
    * `int GetLevelForCustomSkill(Farmer farmer, string skill)` - Gets the level of the given `skill` for the given `farmer`.
    * `void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt)` - Adds `amt` experience to the given `skill` for the given `farmer`.
    * `int GetProfessionId(string skill, string profession)` - Gets the integer ID of the given `profession` (for `Farmer.professions`) for the given skill.
    * `void AddEventCommand(string command, MethodInfo info)` - Adds a custom event command `command` that directs to the method `info`.
        * Note that the method must take the following parameters: `(Event, GameLocation, GameTime, string[])`
        * (The corresponding event, where it takes place, the delta game time, and the parameters to the command.)
    * `void RegisterSerializerType(Type type)` - Register a `type` as being valid for the vanilla serializer. Must have the attribute `XmlType` applied, with the parameter starting with `"Mods_"`, ie. `[XmlType("Mods_AuthorName_MyCustomObject")]`.
    * `void RegisterCustomProperty(Type declaringType, string name, Type propType, MethodInfo getter, MethodInfo setter)` - Register a virtual property, attaching itself to a vanilla object for serialization.
        * `declaringType` is the type to attach to
        * `name` is the name of the property.
        * `propType` is the type of the property you're adding.
        * `getter` is a `MethodInfo` pointing to your static function acting as a getter. It take an instance of the type corresponding to `declaringType`, and return a value of the type corresponding to `propType`.
        * `setter` is a `MethodInfo` pointing to your static function acting as a setter. It take an instance of the type corresponding to `declaringType` and a value of the type corresponding to `propType`.
    * `void RegisterCustomLocationContext( string name, Func<Random, LocationWeather> getLocationWeatherForTomorrowFunc)` - Register a custom location context for user with maps. `name` is the name of it, while `getLocationWeatherForTomorrowFunc` is, you guessed it, the function for getting its weather for tomorrow.
* Events, located in SpaceCore.Events.SpaceEvents:
    * `OnBlankSave` - Occurs before loading starts. Custom locations can be added here so that they retain player data.
    * `ShowNightEndMenus` - Right before the shipping menu, level up menus, etc. pop up so you can add your own.
        * `EventArgsShowNightEndMenus` - not sure why this uses a custom argument tbh, it has nothing in it
    * `ChooseNightlyFarmEvent` - This is specifically for the 'shared' farm events. You can put your own here.
        * `EventArgsChooseNightlyFarmEvent`
            * `FarmEvent NightEvent` - The event that will be used. Can be modified to change which one runs.
    * `OnItemEaten` - Check player.itemToEat for what they just ate.
        * `sender` - The farmer that ate.
    * `ActionActivated` - When a tile with 'Action' property has been activated.
        * `sender` - The farmer that activated it.
        * `EventArgsAction`
            * `bool Cancel` - Set to true to cancel default behavior.
            * `bool TouchAction` - false in this case, true for `TouchActionActivated`
            * `string Action` - the action name
            * `string ActionString` - the full action string
            * `Location Position` - the position of the action, only valid for actions, not touch actions.
    * `TouchActionActivated` - When a tile with 'TouchAction' property has been activated.
        * `sender` The farmer that activated it.
        * `EventArgsAction` - see above
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
            `bool HasEnoughFor(Item item)` - if the item in the slot is enough for the crat
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
* Some other things that will remain undocumented because they will be removed soon.

## Compatibility
Compatible with Stardew Valley 1.5.5+ on Linux/macOS/Windows, both single-player and multiplayer.

## See also
* [Release notes](release-notes.md)
