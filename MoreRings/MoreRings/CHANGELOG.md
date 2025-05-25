## About  
I am chiccen, and I enjoy updating Stardew Valley mods for fun. Such as Fruit Tree Tweaks by aedenthorn and this mod.  
I personally only play vanilla/vanilla+, thus seldom really have any ideas for mods myself. However, I enjoy programming and I enjoy Stardew Valley.  
So as an exercise, I try to update mods that broke with Stardew Valley 1.6.  
I am not very good at programming, but I can usually get the job done. I hope you find the work I have done with this mod satisfactory.  
Below, I will compile a list of changes I made. Most should just be to bring the mod up to date, but some will be due to my own limited experience or preference.  
If you'd like to view my other work, feel free to check out [the rest of this repository](https://github.com/chiccenDev/StardewValleyMods) or my [Nexus Mods](https://next.nexusmods.com/profile/chiccenSDV/mods) page.

## Changes

### Mod.cs

1) Class  
* Omitted JsonAssets instance to avoid dependency on JsonAssets  
* Updated WearMoreRings API  
* Omitted string assignments for ring names, opting instead for hardcoded strings for each application  
* `internal class` => `public partial class` to compartmentalize Methods and CodePatches while still accessing ModEntry members  

2) Entry  
* Omitted `SpaceEvents.OnItemEaten` to avoid dependency on SpaceCore  
* Condensed `HarmonyPatcher.Apply()` => `harmony.PatchAll()` due to preference  
* Omitted `ConsoleCommandHelper.RegisterCommandsInAssembly(this)` due to lack of knowledge of this API  

3) OnGameLaunched  
* Updated WearMoreRings API registration  
* Updated GenericModConfigMenu API registration  
* Omitted registration of JsonAssets to avoid dependency  

4) OnMenuChanged  
* None

5) OnRenderedWorld  
* Moved preliminary checks from `TrueSight.DrawOverWorld()` here to maintain utilization of `HasRingsEquipped()`

6) OnUpdateTicked  
* None

7) OnItemeaten  
* Moved to CodePatches.cs  
* Replaced iterative logic check with native `Farmer.hasBuff()`  
* Replaced outdate remove function with native `Game1.player.buffs.Remove(Buff.tipsy)`

8) HasRingEquipped  
* Moved to Methods.cs due to preference for compartmentalization

9) CountRingsEquipped  
* Moved to Methods.cs due to preference for compartmentalization
* Put all logic on one line because I am lazy and a little bit of preference

10) Log & LogOnce  
* Added tweaked Log functions that I find makes reading logs for a little easier. I recommend removing this if I haven't already.

### Framework.ModConfig.cs

1) Class  
* `internal class` => `public class` probably due to laziness if I am being honest
* Added `bool Debug { get; set; } = false` for use with `ModEntry.Log` and `ModEntry.LogOnce`

### Framework.TrueSight

1) DrawOverWorld  
* Moved preliminary checks. See Mod.cs paragraph 4.  
* `obj.ItemId` => `obj.itemId.Value`  
	* Line 43 (now 40)
	* Line 61 (now 58)
	* Line 64 (now 61)
* `SObject(new Vector2(0, 0), doDraw, 1)` => `SObject(new Vector2(0, 0), doDraw)` Line 78 (now 75)
* `FoundBuriedNuts.ContainsKey()` => `FoundBuriedNuts.Contains` Line 93 (now 90)
* `SObject(new Vector2(0, 0), "73", 1)` => `SObject(new Vector2(0, 0), "73")` on Line 97 (now 94)

2) GetMineStroneDrop  
* Swapped reflection call assignment to local variable in favor of native `int MineShaft.stonesLeftOnThisLevel` public getter Line 118 (now 115)
* Omitted reflection call assignment to local variable in favor of single use of native public variable `bool MineShaft.ladderHasSpawned` on Line 119
* `mine.getOreIndexForLevel()` => `mine.getOreIdForLevel()` on Line 177 (now 173)

3) GetBaseMineStoneDrop  
* `unseenSecretNote.ItemID` => `unseenSecretNote.itemId.Value` on Line 350 (now 346)

4) GetArtifactSpotDrop  
* `string` => `StardewValley.GameData.Objects.ObjectData`
	* Line 364 (now 360)
	* Line 434 (now 430)
* `Game1.objectInformation` => `Game1.objectData`  
	* Line 364 (now 360)
	* Line 434 (now 430)
* `keyValuePair.Value.Split('/')` => `keyValuePair.Value.Name.Split('/')` on Line 366 (now 362)
* `Game1.currentLocation.name` => `Game1.currentLocation.Name` on Line 423 (now 419)
* `objData.Split('/')` => `objData.Name.Split('/')` on Line 436 (now 432)
* `Game1.netWorldState.Value.LostBooksFound.Value` => `Game1.netWorldState.Value.LostBooksFound` on Line 438 (now 434)

### (Axe/Hoe/Pickaxe/WateringCan)Patcher.cs  

1) Class  
* Moved to CodePatches.cs due to preference
* Slightly altered patching methods due to preference

### CropPatcher.cs

1) Class  
* Moved to CodePatches.cs due to preference 

2) Transpile_Harvest
* Altered patching method because I can read IL but I can't read the clear documentation provided by Harmony lol

3) Game_CreateItemDebris
* Moved to Methods.cs due to preference
* Renamed `Game1_createItemDebris` for consistency

4) Farmer_AddItemToInventoryBool
* Moved to Methods.cs due to preference
* Renamed `Farmer_addItemToInventoryBool` for consistency

5) ModifyCropQuality
* Moved to Methods.cs due to preference

### Game1Patcher

1) Class
* Moved to CodePatches.cs due to preference

2) Transpile_PressUseToolButton
* See CropPatcher paragraph 2

3) toolRangeHook
* Moved to Methods.cs due to preference
* Renamed `Utility_withinRadiusOfPlayer` for consistency even though it technically wasn't patching that at all

### i18n

1) Names & Descriptions  
* Moved to Content Patcher folder
* Renamed for brevity

2) Descriptions
* Renamed for brevity

### Ring JSONAssets Data/Assets

1) JSONs
* Moved to Content Patcher for the following reasons:
	* I am more familiar with it
	* It is more popular
	* Hopefully one less thing to break in the future

2) PNGs
* Moved to content patcher
* Reduced to a single PNG with assigned SpriteIndex for each ring