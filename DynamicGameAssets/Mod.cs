using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DynamicGameAssets.Framework;
using DynamicGameAssets.Framework.ContentPacks;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using DynamicGameAssets.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceCore;
using SpaceCore.Framework.Extensions;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

// TODO: Shirts don't work properly if JA is installed? (Might look funny, might make you run out of GPU memory thanks to SpaceCore tilesheet extensions)
// TODO: Cooking recipes show when crafting, but not in the collection.
// TODO: Recipe items (ie. in stores) don't show the correct ingredients
// TODO: Big craftables recipes don't take two vertical slots on the crafting page.

// TODO: Dynamic tokens equivalent
// TODO: Converter & Migration
// TODO: Objects: Donatable to museum? Then need to do museum rewards...
// TODO: Objects (or general): Deconstructor output patch?
// TODO: Objects: Fish tank display?
// TODO: Objects: Stuff on tables, while eating?
// TODO: Objects: Preserve overrides?
// TODO: Objects: warp totems?
// TODO: Crops: Can grow in IndoorPot field (like ancient seeds)
// TODO: Crops: Can grow in greenhouse?
// TODO: Crops: getRandomWildCropForSeason support?
// TODO: General validation, optimization (cache Data in IDGAItem's), not crashing when an item is missing, etc.
// TODO: Look into Gourmand requests?
/* TODO:
 * ? mail
 * ? quests
 * ? adding through events/dialogue
 * ? bundles
 * ? quests
 * ? fishing
 * ? walls/floors
 * Custom Ore Nodes & Custom Resource Clumps (with permission from aedenthorn)
 * ? paths
 * ? buildings (I have a working unreleased framework for this already)
 * NOT farm animals (FAVR)
 * NOT NPCs (covered by CP indirectly)
 * ????farm types????
 * NOT critters (needs AI stuff, can be its own mod)
 * NOT quests
 * NOT mail (MFM)
 * secret notes?
 * NOT trees (BURT)
 * ??? grass + grass starters?
 */
// TODO: API
// TODO: Converter (packs) and converter (items)
// Stretch: In-game editor

namespace DynamicGameAssets
{
    public class Mod : StardewModdingAPI.Mod
    {
        private static readonly string AssetPrefix = "DGA" + PathUtilities.PreferredAssetSeparator;
        public static Mod instance;
        internal ContentPatcher.IContentPatcherAPI cp;

        public static readonly int BaseFakeObjectId = 1720;
        public static ContentPack DummyContentPack;

        internal static Dictionary<string, ContentPack> contentPacks = new(StringComparer.OrdinalIgnoreCase);

        internal static Dictionary<int, string> itemLookup = new();

        internal static Dictionary<string, Dictionary<string, GiftTastePackData>> giftTastes = new();

        // TODO: Should these and SpriteBatchPatcher.packOverrides (and similar overrides) go into State? For splitscreen
        internal static List<DGACustomCraftingRecipe> customCraftingRecipes = new();
        internal static List<DGACustomForgeRecipe> customForgeRecipes = new();
        internal static readonly Dictionary<string, List<MachineRecipePackData>> customMachineRecipes = new();
        internal static List<TailoringRecipePackData> customTailoringRecipes = new();

        private static readonly PerScreen<StateData> _state = new(() => new StateData());
        internal static StateData State => Mod._state.Value;

        private PerScreen<ShopMenu?> _lastShopMenu = new();
        private ShopMenu? LastShopMenu
        {
            get => this._lastShopMenu.Value;
            set => this._lastShopMenu.Value = value;
        }

        public static CommonPackData Find(string fullId)
        {
            int slash = fullId.IndexOf('/');
            if (slash < 0) return null;
            string pack = fullId[..slash];
            string item = fullId[(slash + 1)..];
            return Mod.contentPacks.ContainsKey(pack) ? Mod.contentPacks[pack].Find(item) : null;
        }

        public static List<ContentPack> GetPacks()
        {
            return new List<ContentPack>(Mod.contentPacks.Values);
        }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            //nullPack.Manifest.ExtraFields.Add( "DGA.FormatVersion", -1 );
            //nullPack.Manifest.ExtraFields.Add( "DGA.ConditionsVersion", "1.0.0" );
            Mod.DummyContentPack = new ContentPack(new NullContentPack());

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;

            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            helper.ConsoleCommands.Add("dga_list", "List all items.", this.OnListCommand);
            helper.ConsoleCommands.Add("dga_add", "`dga_add <mod.id/ItemId> [amount] - Add an item to your inventory.", this.OnAddCommand/*, AddCommandAutoComplete*/ );
            helper.ConsoleCommands.Add("dga_force", "Do not use", this.OnForceCommand);
            helper.ConsoleCommands.Add("dga_reload", "Reload all content packs.", this.OnReloadCommand/*, ReloadCommandAutoComplete*/ );
            helper.ConsoleCommands.Add("dga_clean", "Remove all invalid items from the currently loaded save.", this.OnCleanCommand);
            helper.ConsoleCommands.Add("dga_store", "`dga_store [mod.id] - Get a store containing everything for free (optionally from a specific content pack).", this.OnStoreCommand);

            HarmonyPatcher.Apply(this,
                new BootsPatcher(),
                new CollectionsPagePatcher(),
                new CropPatcher(),
                new FarmerPatcher(),
                new FarmerRendererPatcher(),
                new FencePatcher(),
                new FishTankFurniturePatcher(),
                new FruitTreePatcher(),
                new FurniturePatcher(),
                new Game1Patcher(),
                new GameLocationPatcher(),
                //new HatPatcher(),
                new IClickableMenuPatcher(),
                new IndoorPotPatcher(),
                new MeleeWeaponPatcher(),
                new NpcPatcher(),
                new ObjectPatcher(),
                new ShippingMenuPatcher(),
                new ShopPatcher(),
                new SpriteBatchPatcher(),
                new TailoringMenuPatcher(),
                new UtilityPatcher()
            );
        }

        public override object GetApi()
        {
            return new Api();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.cp = this.Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");

            var spacecore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spacecore.RegisterSerializerType(typeof(CustomObject));
            spacecore.RegisterSerializerType(typeof(Game.CustomCraftingRecipe));
            spacecore.RegisterSerializerType(typeof(CustomBasicFurniture));
            spacecore.RegisterSerializerType(typeof(CustomBedFurniture));
            spacecore.RegisterSerializerType(typeof(CustomTVFurniture));
            spacecore.RegisterSerializerType(typeof(CustomFishTankFurniture));
            spacecore.RegisterSerializerType(typeof(CustomStorageFurniture));
            spacecore.RegisterSerializerType(typeof(CustomCrop));
            spacecore.RegisterSerializerType(typeof(CustomGiantCrop));
            spacecore.RegisterSerializerType(typeof(CustomMeleeWeapon));
            spacecore.RegisterSerializerType(typeof(CustomBoots));
            spacecore.RegisterSerializerType(typeof(CustomHat));
            spacecore.RegisterSerializerType(typeof(CustomFence));
            spacecore.RegisterSerializerType(typeof(CustomBigCraftable));
            spacecore.RegisterSerializerType(typeof(CustomFruitTree));
            spacecore.RegisterSerializerType(typeof(CustomShirt));
            spacecore.RegisterSerializerType(typeof(CustomPants));

            this.LoadContentPacks();

            this.RefreshSpritebatchCache();
        }

        private readonly ConditionalWeakTable<Farmer, Holder<string>> prevBootsFrame = new();

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            Mod.State.AnimationFrames++;

            // Support animated boots colors
            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer.boots.Value is CustomBoots boots)
                {
                    TextureAnimationFrame frame = boots.Data.pack.GetTextureFrame(boots.Data.FarmerColors);
                    if (frame is not null && this.prevBootsFrame.GetOrCreateValue(farmer).Value != frame.Descriptor)
                    {
                        if (this.prevBootsFrame.TryGetValue(farmer, out var holder))
                            holder.Value = frame.Descriptor;
                        else
                            this.prevBootsFrame.Add(farmer, new Holder<string>(frame.Descriptor));

                        farmer.FarmerRenderer.MarkSpriteDirty();
                    }
                }
            }
        }

        [EventPriority(EventPriority.Low)]
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var recipe in Mod.customCraftingRecipes)
                (recipe.data.IsCooking ? SpaceCore.CustomCraftingRecipe.CookingRecipes : SpaceCore.CustomCraftingRecipe.CraftingRecipes).Remove(recipe.data.CraftingDataKey);
            foreach (var recipe in Mod.customForgeRecipes)
                CustomForgeRecipe.Recipes.Remove(recipe);

            Mod.giftTastes.Clear();
            Mod.customCraftingRecipes.Clear();
            Mod.customForgeRecipes.Clear();
            Mod.customMachineRecipes.Clear();
            Mod.customTailoringRecipes.Clear();
            SpriteBatchPatcher.packOverrides.Clear();

            // Enabled/disabled
            foreach (var cp in Mod.contentPacks)
            {
                void DoDisable(ContentIndexPackData parent)
                {
                    foreach (var data in cp.Value.enableIndex[parent])
                    {
                        bool wasEnabled = data.Enabled;
                        data.Enabled = data.original.Enabled = false;

                        if (data is CommonPackData cdata && !cdata.Enabled && wasEnabled)
                        {
                            cdata.OnDisabled();
                        }
                        else if (data is ContentIndexPackData cidata)
                        {
                            DoDisable(cidata);
                        }
                    }
                }

                void DoEnableDisable(ContentIndexPackData parent)
                {
                    foreach (var data in cp.Value.enableIndex[parent])
                    {
                        bool shouldreject = false;
                        var conds = new Dictionary<string, string>();
                        if (data.EnableConditions != null)
                        {
                            foreach (var cond in data.EnableConditions)
                            {
                                string key = cond.Key, value = cond.Value;
                                foreach (var opt in parent.pack.configIndex)
                                {
                                    string val = parent.pack.currConfig.Values[opt.Key].ToString();

                                    if (key == opt.Key)
                                    {// this one we should handle ourselves
                                        Log.Trace($"Evaluating pack config option {key} in EnableCondition");
                                        if (!val.Equals(value, StringComparison.OrdinalIgnoreCase))
                                        { // fail the pack, we get to skip the rest of the work too.
                                            shouldreject = true;
                                            Log.Trace($"Rejecting condition {key} with value {val} because it's not equal to {value}");
                                            goto BreakBreak;
                                        }
                                        goto DontAdd;
                                    }

                                    if (parent.pack.configIndex[opt.Key].ValueType == ConfigPackData.ConfigValueType.String)
                                        val = "'" + val + "'";

                                    key = key.Replace("{{" + opt.Key + "}}", val);
                                    value = value.Replace("{{" + opt.Key + "}}", val);
                                }
                                conds.Add(key, value);
DontAdd:;
                            }
                        }

BreakBreak:;
                        data.EnableConditionsObject = Mod.instance.cp.ParseConditions(
                            Mod.instance.ModManifest,
                            conds,
                            cp.Value.conditionVersion,
                            cp.Value.smapiPack.Manifest.Dependencies?.Select((d) => d.UniqueID)?.ToArray() ?? Array.Empty<string>()
                        );
                        if (!data.EnableConditionsObject.IsValid)
                            Log.Warn("Invalid enable conditions for " + data + " " + data.pack.smapiPack.Manifest.Name + "! " + data.EnableConditionsObject.ValidationError);

                        bool wasEnabled = data.Enabled;
                        data.Enabled = data.original.Enabled = (data.EnableConditionsObject.IsMatch && !shouldreject);

                        if (data is CommonPackData cdata && !cdata.Enabled && wasEnabled)
                        {
                            Log.Trace($"Disabling item {cdata.ID}");
                            cdata.OnDisabled();
                        }
                        else if (data is ContentIndexPackData cidata)
                        {
                            if (!cidata.Enabled && wasEnabled)
                            {
                                Log.Trace($"Disabling content index {cidata.FilePath}");
                                DoDisable(cidata);
                            }
                            else
                            {
                                Log.Trace($"Enabling content index {cidata.FilePath}");
                                DoEnableDisable(cidata);
                            }
                        }
                    }
                }

                foreach (var contentIndex in cp.Value.enableIndex.Keys.Where(ci => ci.parent == null))
                    DoEnableDisable(contentIndex);
            }

            // Get active recipes
            foreach (var cp in Mod.contentPacks)
            {
                var pack = cp.Value;
                foreach (var recipe in pack.items.Values.OfType<CraftingRecipePackData>())
                {
                    if (!recipe.Enabled)
                        continue;
                    try
                    {
                        var crecipe = new DGACustomCraftingRecipe(recipe);
                        Mod.customCraftingRecipes.Add(crecipe);
                        (recipe.IsCooking ? SpaceCore.CustomCraftingRecipe.CookingRecipes : SpaceCore.CustomCraftingRecipe.CraftingRecipes).Add(recipe.CraftingDataKey, crecipe);
                    }
                    catch (Exception e2)
                    {
                        Log.Error("Failed when creating crafting recipe implementation for " + recipe.ID + "! " + e2);
                    }
                }

                foreach (var recipe in pack.others.OfType<ForgeRecipePackData>())
                {
                    if (!recipe.Enabled)
                        continue;
                    try
                    {
                        var crecipe = new DGACustomForgeRecipe(recipe);
                        Mod.customForgeRecipes.Add(crecipe);
                        CustomForgeRecipe.Recipes.Add(crecipe);
                    }
                    catch (Exception e2)
                    {
                        Log.Error("Failed when creating forge recipe implementation! " + e2);
                    }
                }
            }

            // Dynamic fields
            foreach (var cp in Mod.contentPacks)
            {
                var newItems = new Dictionary<string, CommonPackData>();
                foreach (var data in cp.Value.items)
                {
                    var newItem = (CommonPackData)data.Value.original.Clone();
                    newItem.ApplyDynamicFields();
                    newItems.Add(data.Key, newItem);
                }
                cp.Value.items = newItems;

                var newOthers = new List<BasePackData>();
                foreach (var data in cp.Value.others)
                {
                    var newOther = (BasePackData)data.original.Clone();
                    newOther.ApplyDynamicFields();
                    newOthers.Add(newOther);

                    if (newOther is MachineRecipePackData machineRecipe)
                    {
                        if (!Mod.customMachineRecipes.ContainsKey(machineRecipe.MachineId))
                            Mod.customMachineRecipes.Add(machineRecipe.MachineId, new List<MachineRecipePackData>());
                        if (machineRecipe.Enabled)
                            Mod.customMachineRecipes[machineRecipe.MachineId].Add(machineRecipe);
                    }
                    else if (newOther is TailoringRecipePackData tailoringRecipe)
                    {
                        if (tailoringRecipe.Enabled)
                            Mod.customTailoringRecipes.Add(tailoringRecipe);
                    }
                    else if (newOther is GiftTastePackData giftTaste)
                    {
                        if (giftTaste.Enabled)
                        {
                            string[] npcs = giftTaste.Npc.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            foreach (string npc in npcs)
                            {
                                if (!Mod.giftTastes.ContainsKey(npc))
                                    Mod.giftTastes.Add(npc, new());

                                string[] objects = giftTaste.ObjectId.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                                foreach (string obj in objects)
                                    if (!Mod.giftTastes[npc].ContainsKey(obj))
                                        Mod.giftTastes[npc].Add(obj, giftTaste);
                            }
                        }
                    }
                }
                cp.Value.others = newOthers;
            }

            foreach (var player in Game1.getAllFarmers())
            {
                foreach (var recipe in Mod.customCraftingRecipes)
                {
                    bool learn = recipe.data.KnownByDefault;
                    if (recipe.data.SkillUnlockName != null && recipe.data.SkillUnlockLevel > 0)
                    {
                        int level = recipe.data.SkillUnlockName switch
                        {
                            "Farming" => Game1.player.farmingLevel.Value,
                            "Fishing" => Game1.player.fishingLevel.Value,
                            "Foraging" => Game1.player.foragingLevel.Value,
                            "Mining" => Game1.player.miningLevel.Value,
                            "Combat" => Game1.player.combatLevel.Value,
                            "Luck" => Game1.player.luckLevel.Value,
                            _ => Game1.player.GetCustomSkillLevel(recipe.data.SkillUnlockName)
                        };

                        if (level >= recipe.data.SkillUnlockLevel)
                            learn = true;
                    }

                    if (learn)
                    {
                        if (!recipe.data.IsCooking && !player.craftingRecipes.Keys.Contains(recipe.data.CraftingDataKey))
                            player.craftingRecipes.Add(recipe.data.CraftingDataKey, 0);
                        else if (!recipe.data.IsCooking && !player.cookingRecipes.Keys.Contains(recipe.data.CraftingDataKey))
                            player.cookingRecipes.Add(recipe.data.CraftingDataKey, 0);
                    }
                }
            }

            this.RefreshRecipes();
            this.RefreshShopEntries();

            if (Context.ScreenId == 0)
            {
                this.RefreshSpritebatchCache();
            }

            this.Helper.GameContent.InvalidateCache("Data\\CraftingRecipes");
            this.Helper.GameContent.InvalidateCache("Data\\CookingRecipes");
            if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
            {
                this.Helper.GameContent.InvalidateCache($@"Data\CraftingRecipes.{this.Helper.GameContent.CurrentLocale}");
                this.Helper.GameContent.InvalidateCache($@"Data\CookingRecipes.{this.Helper.GameContent.CurrentLocale}");
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu shop)
            {
                // Log.Trace($"We have found a shop: {shop.portraitPerson?.Name}");
                if (shop.storeContext is "ResortBar" or "VolcanoShop")
                {
                    PatchCommon.DoShop(shop.storeContext, shop);
                }

                // handle STF shop menu -- this is copied from JA
                if (!object.ReferenceEquals(e.NewMenu, this.LastShopMenu))
                {
                    this.LastShopMenu = shop;

                    string? id = shop.portraitPerson?.Name;
                    if (id is null || !id.StartsWith("STF", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    Log.Trace($"Adding objects for STF shop '{id}'.");
                    PatchCommon.DoShop(id, shop);
                }
            }
        }

        private void OnListCommand(string cmd, string[] args)
        {
            string output = "";
            foreach (var cp in Mod.contentPacks)
            {
                output += cp.Key + ":\n";
                foreach (var entry in cp.Value.items)
                {
                    if (entry.Value.Enabled)
                        output += "\t" + entry.Key + "\n";
                }
                output += "\n";
            }

            Log.Info(output);
        }

        private void OnAddCommand(string cmd, string[] args)
        {
            if (args.Length < 1)
            {
                Log.Info("Usage: dga_add <mod.id/ItemId> [amount]");
                return;
            }

            var data = Mod.Find(args[0]);
            if (data == null)
            {
                Log.Error($"Item '{args[0]}' not found.");
                return;
            }

            var item = data.ToItem();
            if (item == null)
            {
                Log.Error($"The item '{args[0]}' has no inventory form.");
                return;
            }
            if (args.Length >= 2)
            {
                item.Stack = int.Parse(args[1]);
            }

            Game1.player.addItemByMenuIfNecessary(item);
        }

        private string[] AddCommandAutoComplete(string cmd, string input)
        {
            if (input.Contains(' '))
                return null;

            var ret = new List<string>();

            int slash = input.IndexOf('/');
            if (slash == -1)
            {
                foreach (string packId in Mod.contentPacks.Keys)
                {
                    if (packId.StartsWith(input))
                        ret.Add(packId);
                }
            }
            else
            {
                string packId = input.Substring(0, slash);
                string itemInPack = input.Substring(slash + 1);

                if (!Mod.contentPacks.ContainsKey(packId))
                    return null;

                var pack = Mod.contentPacks[packId];
                foreach (string itemId in pack.items.Keys)
                {
                    if (itemId.StartsWith(itemInPack))
                        ret.Add(packId + "/" + itemId.Replace(" ", "\" \""));
                }
            }

            return ret.ToArray();
        }

        private void OnForceCommand(string cmd, string[] args)
        {
            this.OnDayStarted(this, null);
        }

        private void OnReloadCommand(string cmd, string[] args)
        {
            Mod.contentPacks.Clear();
            Mod.itemLookup.Clear();
            foreach (var recipe in Mod.customCraftingRecipes)
                (recipe.data.IsCooking ? SpaceCore.CustomCraftingRecipe.CookingRecipes : SpaceCore.CustomCraftingRecipe.CraftingRecipes).Remove(recipe.data.CraftingDataKey);
            foreach (var recipe in Mod.customForgeRecipes)
                CustomForgeRecipe.Recipes.Remove(recipe);
            Mod.customCraftingRecipes.Clear();
            Mod.customForgeRecipes.Clear();
            SpriteBatchPatcher.packOverrides.Clear();
            foreach (var state in Mod._state.GetActiveValues())
            {
                state.Value.TodaysShopEntries.Clear();
            }

            this.LoadContentPacks();
            this.OnDayStarted(this, null);
        }
        /*
        private string[] ReloadCommandAutoComplete( string cmd, string input )
        {
            if ( input.Contains( ' ' ) )
                return null;

            var ret = new List<string>();

            foreach ( string packId in contentPacks.Keys )
            {
                if ( packId.StartsWith( input ) )
                    ret.Add( packId );
            }

            return ret.ToArray();
        }*/

        public void OnCleanCommand(string cmd, string[] args)
        {
            SpaceUtility.iterateAllItems((item) =>
            {
                if (item is IDGAItem citem && Mod.Find(citem.FullId) == null)
                {
                    return null;
                }
                return item;
            });
            SpaceUtility.iterateAllTerrainFeatures((tf) =>
            {
                if (tf is IDGAItem citem && Mod.Find(citem.FullId) == null)
                {
                    return null;
                }
                else if (tf is HoeDirt hd && hd.crop is IDGAItem citem2 && Mod.Find(citem2.FullId) == null)
                {
                    hd.crop = null;
                }
                return tf;
            });
        }

        private void OnStoreCommand(string cmd, string[] args)
        {
            if (args.Length > 1)
            {
                Log.Error("Too many arguments");
                return;
            }
            if (args.Length == 0)
            {
                Dictionary<ISalable, int[]> stuff = new();
                foreach (var pack in Mod.contentPacks)
                {
                    foreach (var data in pack.Value.items.Values)
                    {
                        if (!data.Enabled)
                            continue;

                        var item = data.ToItem();
                        if (item != null)
                            stuff.Add(item, new[] { 0, item is DynamicGameAssets.Game.CustomCraftingRecipe ? 1 : int.MaxValue });
                    }
                }
                Game1.activeClickableMenu = new ShopMenu(stuff);
            }
            else
            {
                if (!Mod.contentPacks.ContainsKey(args[0]))
                {
                    Log.Error("Invalid pack ID");
                    return;
                }
                var pack = Mod.contentPacks[args[0]];

                Dictionary<ISalable, int[]> stuff = new();
                foreach (var data in pack.items.Values)
                {
                    if (!data.Enabled)
                        continue;

                    var item = data.ToItem();
                    if (item != null)
                        stuff.Add(item, new[] { 0, item is DynamicGameAssets.Game.CustomCraftingRecipe ? 1 : int.MaxValue });
                }
                Game1.activeClickableMenu = new ShopMenu(stuff);
            }
        }

        public static void AddContentPack(ContentPack pack)
        {
            Mod.contentPacks.Add(pack.smapiPack.Manifest.UniqueID, pack);
        }

        internal static void AddEmbeddedContentPack(IManifest manifest, string dir)
        {
            Log.Trace($"Loading embedded content pack for \"{manifest.Name}\"...");
            if (manifest.ExtraFields == null ||
                 !manifest.ExtraFields.ContainsKey("DGA.FormatVersion") ||
                 !int.TryParse(manifest.ExtraFields["DGA.FormatVersion"].ToString(), out int formatVer))
            {
                Log.Error("Must specify a DGA.FormatVersion as an integer! (See documentation.)");
                return;
            }
            if (formatVer is < 1 or > 2)
            {
                Log.Error("Unsupported format version!");
                return;
            }
            if (!manifest.ExtraFields.ContainsKey("DGA.ConditionsFormatVersion") ||
                 !SemanticVersion.TryParse(manifest.ExtraFields["DGA.ConditionsFormatVersion"].ToString(), out ISemanticVersion condVer))
            {
                Log.Error("Must specify a DGA.ConditionsFormatVersion as a semantic version! (See documentation.)");
                return;
            }

            var cp = Mod.instance.Helper.ContentPacks.CreateTemporary(dir, manifest.UniqueID, manifest.Name, manifest.Description, manifest.Author, manifest.Version);
            var pack = new ContentPack(cp, formatVer, condVer);
            Mod.contentPacks.Add(manifest.UniqueID, pack);
        }

        private void LoadContentPacks()
        {
            foreach (var cp in this.Helper.ContentPacks.GetOwned())
            {
                Log.Trace($"Loading content pack \"{cp.Manifest.Name}\"...");
                if (cp.Manifest.ExtraFields == null ||
                     !cp.Manifest.ExtraFields.ContainsKey("DGA.FormatVersion") ||
                     !int.TryParse(cp.Manifest.ExtraFields["DGA.FormatVersion"].ToString(), out int formatVer))
                {
                    Log.Error("Must specify a DGA.FormatVersion as an integer! (See documentation.)");
                    continue;
                }
                if (formatVer is < 1 or > 2)
                {
                    Log.Error("Unsupported format version!");
                    continue;
                }
                if (!cp.Manifest.ExtraFields.ContainsKey("DGA.ConditionsFormatVersion") ||
                     !SemanticVersion.TryParse(cp.Manifest.ExtraFields["DGA.ConditionsFormatVersion"].ToString(), out ISemanticVersion condVer))
                {
                    Log.Error("Must specify a DGA.ConditionsFormatVersion as a semantic version! (See documentation.)");
                    continue;
                }
                try
                {
                    var pack = new ContentPack(cp);
                    Mod.contentPacks.Add(cp.Manifest.UniqueID, pack);
                }
                catch (Exception e)
                {
                    Log.Error("Exception loading content pack \"" + cp.Manifest.Name + "\": " + e);
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith(AssetPrefix, false, true)
                && e.NameWithoutLocale.BaseName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                string id = e.NameWithoutLocale.BaseName.GetNthChunk(new[] { '/' , '\\'}, 1).ToString();
                if (Mod.contentPacks.TryGetValue(id, out var pack))
                {
                    _ = pack.TryLoad(e);
                }
            }
            else if (Mod.customCraftingRecipes.Count > 0 && e.NameWithoutLocale.IsEquivalentTo("Data\\CookingRecipes"))
            {
                e.Edit(static (asset) =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    int i = 0;
                    foreach (var crecipe in Mod.customCraftingRecipes)
                    {
                        if (crecipe.data.Enabled && crecipe.data.IsCooking)
                        {
                            dict.Add(crecipe.data.CraftingDataKey, crecipe.data.CraftingDataValue);
                            ++i;
                        }
                    }
                    Log.Trace($"Added {i}/{Mod.customCraftingRecipes.Count} entries to cooking recipes");
                });
            }
            else if (Mod.customCraftingRecipes.Count > 0 && e.NameWithoutLocale.IsEquivalentTo("Data\\CraftingRecipes"))
            {
                e.Edit(static (asset) =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    int i = 0;
                    foreach (var crecipe in Mod.customCraftingRecipes)
                    {
                        if (crecipe.data.Enabled && !crecipe.data.IsCooking)
                        {
                            dict.Add(crecipe.data.CraftingDataKey, crecipe.data.CraftingDataValue);
                            ++i;
                        }
                    }
                    Log.Trace($"Added {i}/{Mod.customCraftingRecipes.Count} entries to crafting recipes");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\ObjectInformation"))
            {
                e.Edit(static (asset) =>
                {
                    asset.AsDictionary<int, string>().Data.Add(Mod.BaseFakeObjectId, "DGA Dummy Object/0/0/Basic -20/DGA Dummy Object/You shouldn't have this./food/0 0 0 0 0 0 0 0 0 0 0 0/0");
                });
            }
        }

        /*
        private Item MakeItemFrom( string name, ContentPack context = null )
        {
            if ( context != null )
            {
                foreach ( var item in context.items )
                {
                    if ( name == item.Key )
                    {
                        var retCtx = item.Value.ToItem();
                        if ( retCtx != null )
                            return retCtx;
                    }
                }
            }

            int slash = name.IndexOf( '/' );
            if ( slash != -1 )
            {
                string pack = name.Substring( 0, slash );
                string item = name.Substring( slash + 1 );
                if ( contentPacks.ContainsKey( pack ) && contentPacks[ pack ].items.ContainsKey( item ) )
                {
                    var retCp = contentPacks[ pack ].items[ item ].ToItem();
                    if ( retCp != null )
                        return retCp;
                }

                Log.Error( $"Failed to find item \"{name}\" from context {context?.smapiPack?.Manifest?.UniqueID}" );
                return null;
            }

            var ret = Utility.getItemFromStandardTextDescription( name, Game1.player );
            if ( ret == null )
            {
                Log.Error( $"Failed to find item \"{name}\" from context {context?.smapiPack?.Manifest?.UniqueID}" );

            }
            return ret;
        }
        */

        private void RefreshRecipes()
        {
            foreach (var recipe in Mod.customCraftingRecipes)
                recipe.Refresh();
            foreach (var recipe in Mod.customForgeRecipes)
                recipe.Refresh();
        }

        private void RefreshShopEntries()
        {
            Mod.State.TodaysShopEntries.Clear();
            foreach (var cp in Mod.contentPacks)
            {
                foreach (var shopEntry in cp.Value.others.OfType<ShopEntryPackData>())
                {
                    try
                    {
                        if (shopEntry.Enabled)
                        {
                            if (!Mod.State.TodaysShopEntries.ContainsKey(shopEntry.ShopId))
                                Mod.State.TodaysShopEntries.Add(shopEntry.ShopId, new List<ShopEntry>());
                            int shopEntryPrice = shopEntry.Cost;
                            var salable = shopEntry.Item.Create();
                            if (salable is StardewValley.Object obj && (obj.Category == StardewValley.Object.SeedsCategory || obj.isSapling()))
                            {
                                shopEntryPrice = (int)((float)shopEntryPrice * Game1.MasterPlayer.difficultyModifier);
                            }
                            Mod.State.TodaysShopEntries[shopEntry.ShopId].Add(new ShopEntry()
                            {
                                Item = salable,//MakeItemFrom( shopEntry.Item, cp.Value ),
                                Quantity = shopEntry.MaxSold,
                                Price = shopEntryPrice,
                                CurrencyId = shopEntry.Currency == null ? null : (int.TryParse(shopEntry.Currency, out int intCurr) ? intCurr : $"{cp.Key}/{shopEntry.Currency}".GetDeterministicHashCode())
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error making shop entry from " + cp.Value.smapiPack.Manifest.Name + ": " + e);
                    }
                }
            }
        }

        internal void RefreshSpritebatchCache()
        {
            if (Game1.objectSpriteSheet == null)
                Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");

            SpriteBatchPatcher.objectOverrides.Clear();
            SpriteBatchPatcher.weaponOverrides.Clear();
            SpriteBatchPatcher.hatOverrides.Clear();
            SpriteBatchPatcher.shirtOverrides.Clear();
            SpriteBatchPatcher.pantsOverrides.Clear();
            SpriteBatchPatcher.packOverrides.Clear();
            foreach (var cp in Mod.contentPacks)
            {
                foreach (var item in cp.Value.items.Values)
                {
                    if (item is ObjectPackData obj)
                    {
                        var tex = cp.Value.GetTexture(obj.Texture, 16, 16);
                        string fullId = $"{cp.Key}/{obj.ID}";
                        SpriteBatchPatcher.objectOverrides.Add(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, fullId.GetDeterministicHashCode(), 16, 16), tex);
                    }
                    else if (item is MeleeWeaponPackData weapon)
                    {
                        var tex = cp.Value.GetTexture(weapon.Texture, 16, 16);
                        string fullId = $"{cp.Key}/{weapon.ID}";
                        SpriteBatchPatcher.weaponOverrides.Add(Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, fullId.GetDeterministicHashCode(), 16, 16), tex);
                    }
                    else if (item is HatPackData hat)
                    {
                        var tex = hat.GetTexture();
                        if (!tex.Rect.HasValue)
                            tex.Rect = new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);

                        string fullId = $"{cp.Key}/{hat.ID}";
                        int which = fullId.GetDeterministicHashCode();

                        var rect = new Rectangle(20 * (int)which % FarmerRenderer.hatsTexture.Width, 20 * (int)which / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20);
                        SpriteBatchPatcher.hatOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 0, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 20);
                        SpriteBatchPatcher.hatOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 1, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 20);
                        SpriteBatchPatcher.hatOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 2, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 20);
                        SpriteBatchPatcher.hatOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 3, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                    }
                    else if (item is ShirtPackData shirt)
                    {
                        string fullId = $"{cp.Key}/{shirt.ID}";
                        int which = fullId.GetDeterministicHashCode();

                        var tex = cp.Value.GetTexture(shirt.TextureMale, 8, 32);
                        tex.Rect ??= new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);
                        var rect = new Rectangle(which * 8 % 128, which * 8 / 128 * 32, 8, 8);
                        SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 0, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 8);
                        SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 1, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 8);
                        SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 2, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        rect.Offset(0, 8);
                        SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 3, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });

                        if (shirt.TextureMaleColor != null)
                        {
                            tex = cp.Value.GetTexture(shirt.TextureMaleColor, 8, 32);
                            tex.Rect ??= new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);
                            rect.Offset(128, -24);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 0, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 1, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 2, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 3, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        }

                        if (shirt.TextureFemale != null)
                        {
                            tex = cp.Value.GetTexture(shirt.TextureFemale, 8, 32);
                            tex.Rect ??= new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);
                            which += 1;
                            rect = new Rectangle(which * 8 % 128, which * 8 / 128 * 32, 8, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 0, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 1, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 2, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 3, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        }

                        if (shirt.TextureFemaleColor != null)
                        {
                            tex = cp.Value.GetTexture(shirt.TextureFemaleColor, 8, 32);
                            tex.Rect ??= new Rectangle(0, 0, tex.Texture.Width, tex.Texture.Height);
                            rect.Offset(128, -24);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 0, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 1, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 2, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                            rect.Offset(0, 8);
                            SpriteBatchPatcher.shirtOverrides.Add(rect, new TexturedRect() { Texture = tex.Texture, Rect = new Rectangle(tex.Rect.Value.X, tex.Rect.Value.Y + tex.Rect.Value.Height / 4 * 3, tex.Rect.Value.Width, tex.Rect.Value.Height / 4) });
                        }
                    }
                    else if (item is PantsPackData pants)
                    {
                        var tex = cp.Value.GetTexture(pants.Texture, 192, 688);
                        string fullId = $"{cp.Key}/{pants.ID}";
                        int which = fullId.GetDeterministicHashCode();
                        SpriteBatchPatcher.pantsOverrides.Add(new Rectangle(which % 10 * 192, which / 10 * 688, 192, 688), tex);
                    }
                }
                foreach (BasePackData other in cp.Value.others)
                {
                    if (other is TextureOverridePackData textureOverride)
                    {
                        if (!SpriteBatchPatcher.packOverrides.TryGetValue(textureOverride.TargetTexture, out var packOverrides))
                            SpriteBatchPatcher.packOverrides[textureOverride.TargetTexture] = packOverrides = new Dictionary<Rectangle, TextureOverridePackData>();

                        packOverrides[textureOverride.TargetRect] = textureOverride;
                    }
                }
            }
        }
    }
}
