using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using DynamicGameAssets.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using SpaceCore;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

// TODO: Objects: Light?
// TODO: Objects (or general): Deconstructor output patch?
// TODO: Objects: Fish tank display?
// TODO: Objects: Stuff on tables
// TODO: Objects: Preserve overrides?
// TODO: Objects: warp totems?
// TODO: Objects&Crops&GiantCrops(?)&ItemAbstraction: colors?
// TODO: Crops: Can grow in IndoorPot field
// TODO: Crops: Can grow in greenhouse?
// TODO: Crops: getRandomWildCropForSeason support?
// TODO: General validation, optimization (cache Data in IDGAItem's)not crashing when an item is missing, etc.
// TODO: General: Extension data
// TODO: Look into Gourmand requests?
/* TODO:
 * Big craftables
 * Boots
 * Clothing (pants, shirt)
 * Fences
 * Forge recipes
 * Fruit trees
 * Hats
 * ? walls/floors
 * Custom Ore Nodes & Custom Resource Clumps
 * Basic machine recipes
 * ? paths
 * ? buildings
 * NOT farm animals (FAVR)
 * NOT NPCs (covered by CP indirectly)
 * farm types????
 * NOT critters (needs AI stuff, can be its own mod)
 * NOT quests
 * NOT mail (MFM)
 * secret notes?
 * NOT trees (BURT)
 */
// TODO: API
// Stretch: In-game editor

namespace DynamicGameAssets
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public static Mod instance;
        internal ContentPatcher.IContentPatcherAPI cp;
        private Harmony harmony;

        public static readonly int BaseFakeObjectId = 1720;

        internal static Dictionary<string, ContentPack> contentPacks = new Dictionary<string, ContentPack>();

        internal static Dictionary<int, string> itemLookup = new Dictionary<int, string>();

        internal static List<DGACustomRecipe> customRecipes = new List<DGACustomRecipe>();

        private static readonly PerScreen<StateData> _state = new PerScreen<StateData>( () => new StateData() );
        internal static StateData State => _state.Value;

        public static CommonPackData Find( string fullId )
        {
            int slash = fullId.IndexOf( '/' );
            string pack = fullId.Substring( 0, slash );
            string item = fullId.Substring( slash + 1 );
            return contentPacks.ContainsKey( pack ) ? contentPacks[ pack ].Find( item ) : null;
        }
        
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.UpdateTicked += ( s, e ) => State.AnimationFrames++;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            helper.ConsoleCommands.Add( "dga_list", "...", OnListCommand );
            helper.ConsoleCommands.Add( "dga_add", "...", OnAddCommand, AddCommandAutoComplete );
            helper.ConsoleCommands.Add( "dga_force", "Do not use", OnForceCommand );
            helper.ConsoleCommands.Add( "dga_reload", "...", OnReloadCommand/*, ReloadCommandAutoComplete*/ );

            harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
            harmony.Patch( typeof( IClickableMenu ).GetMethod( "drawHoverText", new[] { typeof( SpriteBatch ), typeof( StringBuilder ), typeof( SpriteFont ), typeof( int ), typeof( int ), typeof( int ), typeof( string ), typeof( int ), typeof( string[] ), typeof( Item ), typeof( int ), typeof( int ), typeof( int ), typeof( int ), typeof( int ), typeof( int ),typeof( CraftingRecipe ), typeof( IList<Item> ) } ), transpiler: new HarmonyMethod( typeof( DrawHoverTextPatch ).GetMethod( "Transpiler" ) ) );
            harmony.Patch( typeof( SpriteBatch ).GetMethod( "Draw", new[] { typeof( Texture2D ), typeof( Rectangle ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ), typeof( SpriteEffects ), typeof( float ) } ), prefix: new HarmonyMethod( typeof( SpriteBatchTileSheetAdjustments ).GetMethod( nameof( SpriteBatchTileSheetAdjustments.Prefix1 ) ) ) );
            harmony.Patch( typeof( SpriteBatch ).GetMethod( "Draw", new[] { typeof( Texture2D ), typeof( Rectangle ), typeof( Rectangle? ), typeof( Color ), } ), prefix: new HarmonyMethod( typeof( SpriteBatchTileSheetAdjustments ).GetMethod( nameof( SpriteBatchTileSheetAdjustments.Prefix2 ) ) ) );
            harmony.Patch( typeof( SpriteBatch ).GetMethod( "Draw", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ), typeof( Vector2 ), typeof( SpriteEffects ), typeof( float ) } ), prefix: new HarmonyMethod( typeof( SpriteBatchTileSheetAdjustments ).GetMethod( nameof( SpriteBatchTileSheetAdjustments.Prefix3 ) ) ) );
            harmony.Patch( typeof( SpriteBatch ).GetMethod( "Draw", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( Rectangle? ), typeof( Color ), typeof( float ), typeof( Vector2 ), typeof( float ), typeof( SpriteEffects ), typeof( float ) } ), prefix: new HarmonyMethod( typeof( SpriteBatchTileSheetAdjustments ).GetMethod( nameof( SpriteBatchTileSheetAdjustments.Prefix4 ) ) ) );
            harmony.Patch( typeof( SpriteBatch ).GetMethod( "Draw", new[] { typeof( Texture2D ), typeof( Vector2 ), typeof( Rectangle? ), typeof( Color ) } ), prefix: new HarmonyMethod( typeof( SpriteBatchTileSheetAdjustments ).GetMethod( nameof( SpriteBatchTileSheetAdjustments.Prefix5 ) ) ) );

            LoadContentPacks();

            RefreshSpritebatchCache();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            cp = Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>( "Pathoschild.ContentPatcher" );

            var spacecore = Helper.ModRegistry.GetApi<ISpaceCoreApi>( "spacechase0.SpaceCore" );
            spacecore.RegisterSerializerType( typeof( CustomObject ) );
            spacecore.RegisterSerializerType(typeof(CustomCraftingRecipe));
            spacecore.RegisterSerializerType(typeof(CustomBasicFurniture));
            spacecore.RegisterSerializerType(typeof(CustomBedFurniture));
            spacecore.RegisterSerializerType(typeof(CustomTVFurniture));
            spacecore.RegisterSerializerType(typeof(CustomFishTankFurniture));
            spacecore.RegisterSerializerType(typeof(CustomStorageFurniture));
            spacecore.RegisterSerializerType(typeof(CustomCrop));
            spacecore.RegisterSerializerType(typeof(CustomGiantCrop));
            spacecore.RegisterSerializerType(typeof(CustomMeleeWeapon));

            foreach ( var pack in contentPacks )
            {
                foreach (var recipe in pack.Value.items.Values.OfType<CraftingPackData>())
                {
                    var crecipe = new DGACustomRecipe(recipe);
                    customRecipes.Add(crecipe);
                    (recipe.IsCooking ? CustomRecipe.CookingRecipes : CustomRecipe.CraftingRecipes).Add(recipe.CraftingDataKey, crecipe);
                }
            }
        }
        
        private void OnDayStarted( object sender, DayStartedEventArgs e )
        {
            // Enabled/disabled
            foreach ( var cp in contentPacks )
            {
                foreach ( var data in cp.Value.items )
                {
                    if ( data.Value.EnableConditionsObject == null )
                        data.Value.EnableConditionsObject = Mod.instance.cp.ParseConditions( Mod.instance.ModManifest,
                                                                                             data.Value.EnableConditions,
                                                                                             cp.Value.conditionVersion,
                                                                                             cp.Value.smapiPack.Manifest.Dependencies?.Select( ( d ) => d.UniqueID )?.ToArray() ?? new string[0] );

                    bool wasEnabled = data.Value.Enabled;
                    data.Value.Enabled = data.Value.EnableConditionsObject.IsMatch;
                    
                    if ( !data.Value.Enabled && wasEnabled )
                    {
                        data.Value.OnDisabled();
                    }
                }
                foreach ( var data in cp.Value.others )
                {
                    if ( data.EnableConditionsObject == null )
                        data.EnableConditionsObject = Mod.instance.cp.ParseConditions( Mod.instance.ModManifest,
                                                                                       data.EnableConditions,
                                                                                       cp.Value.conditionVersion,
                                                                                       cp.Value.smapiPack.Manifest.Dependencies?.Select( ( d ) => d.UniqueID )?.ToArray() ?? new string[ 0 ] );

                    data.Enabled = data.EnableConditionsObject.IsMatch;
                }
            }

            // Dynamic fields
            foreach ( var cp in contentPacks )
            {
                var newItems = new Dictionary<string, CommonPackData>();
                foreach ( var data in cp.Value.items )
                {
                    var newItem = ( CommonPackData ) data.Value.original.Clone();
                    newItem.ApplyDynamicFields();
                    newItems.Add( data.Key, newItem );
                }
                cp.Value.items = newItems;

                var newOthers = new List<BasePackData>();
                foreach ( var data in cp.Value.others )
                {
                    var newOther = ( BasePackData ) data.original.Clone();
                    newOther.ApplyDynamicFields();
                    newOthers.Add( newOther );
                }
                cp.Value.others = newOthers;
            }

            RefreshShopEntries();

            if ( Context.ScreenId == 0 )
            {
                RefreshSpritebatchCache();
            }

            Helper.Content.InvalidateCache("Data\\CraftingRecipes");
            Helper.Content.InvalidateCache("Data\\CookingRecipes");
        }

        private void OnMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( e.NewMenu is ShopMenu shop )
            {
                if ( shop.storeContext == "ResortBar" || shop.storeContext == "VolcanoShop" )
                {
                    PatchCommon.DoShop( shop.storeContext, shop );
                }
            }
        }

        private void OnListCommand( string cmd, string[] args )
        {
            string output = "";
            foreach ( var cp in contentPacks )
            {
                output += cp.Key + ":\n";
                foreach ( var entry in cp.Value.items )
                {
                    output += "\t" + entry.Key + "\n";
                }
                output += "\n";
            }

            Log.Info( output );
        }

        private void OnAddCommand( string cmd, string[] args )
        {
            if ( args.Length < 1 )
            {
                Log.Info( "Usage: dga_add <mod.id/ItemId> [amount]" );
                return;
            }

            var data = Find( args[ 0 ] );
            if ( data == null )
            {
                Log.Error( $"Item '{args[ 0 ]}' not found." );
                return;
            }

            var item = data.ToItem();
            if ( item == null )
            {
                Log.Error( $"The item '{args[ 0 ]}' has no inventory form." );
                return;
            }
            if ( args.Length >= 2 )
            {
                item.Stack = int.Parse( args[ 1 ] );
            }

            Game1.player.addItemByMenuIfNecessary( item );
        }

        private string[] AddCommandAutoComplete( string cmd, string input )
        {
            if ( input.Contains( ' ' ) )
                return null;

            var ret = new List<string>();

            int slash = input.IndexOf( '/' );
            if ( slash == -1 )
            {
                foreach ( string packId in contentPacks.Keys )
                {
                    if ( packId.StartsWith( input ) )
                        ret.Add( packId );
                }
            }
            else
            {
                string packId = input.Substring( 0, slash );
                string itemInPack = input.Substring( slash + 1 );

                if ( !contentPacks.ContainsKey( packId ) )
                    return null;

                var pack = contentPacks[ packId ];
                foreach ( string itemId in pack.items.Keys )
                {
                    if ( itemId.StartsWith( itemInPack ) )
                        ret.Add( packId + "/" + itemId.Replace( " ", "\" \"" ) );
                }
            }

            return ret.ToArray();
        }

        private void OnForceCommand( string cmd, string[] args )
        {
            OnDayStarted( this, null );
        }

        private void OnReloadCommand( string cmd, string[] args )
        {
            contentPacks.Clear();
            itemLookup.Clear();
            customRecipes.Clear();
            foreach ( var state in _state.GetActiveValues() )
            {
                state.Value.TodaysShopEntries.Clear();
            }
            LoadContentPacks();
            OnDayStarted( this, null );
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

        private void LoadContentPacks()
        {
            foreach ( var cp in Helper.ContentPacks.GetOwned() )
            {
                Log.Debug( $"Loading content pack \"{cp.Manifest.Name}\"..." );
                if ( cp.Manifest.ExtraFields == null ||
                     !cp.Manifest.ExtraFields.ContainsKey( "DGA.FormatVersion" ) ||
                     !int.TryParse( cp.Manifest.ExtraFields[ "DGA.FormatVersion" ].ToString(), out int ver ) )
                {
                    Log.Error("Must specify a DGA.FormatVersion as an integer! (See documentation.)");
                    continue;
                }
                if ( ver != 1 )
                {
                    Log.Error( "Unsupported format version!" );
                    continue;
                }   
                if ( !cp.Manifest.ExtraFields.ContainsKey( "DGA.ConditionsFormatVersion" ) ||
                    !SemanticVersion.TryParse( cp.Manifest.ExtraFields[ "DGA.ConditionsFormatVersion" ].ToString(), out ISemanticVersion condVer ) )
                {
                    Log.Error( "Must specify a DGA.ConditionsFormatVersion as a semantic version! (See documentation.)" );
                    continue;
                }
                var pack = new ContentPack( cp );
                contentPacks.Add( cp.Manifest.UniqueID, pack );
            }
        }

        public bool CanEdit<T>( IAssetInfo asset )
        {
            if (asset.AssetNameEquals("Data\\CookingRecipes"))
                return true;
            if (asset.AssetNameEquals("Data\\CraftingRecipes"))
                return true;
            if (asset.AssetNameEquals("Data\\ObjectInformation"))
                return true;
            return false;
        }

        public void Edit<T>( IAssetData asset )
        {
            if (asset.AssetNameEquals("Data\\CookingRecipes"))
            {
                var dict = asset.AsDictionary<string, string>().Data;
                int i = 0;
                foreach (var crecipe in customRecipes)
                {
                    if (crecipe.data.Enabled && crecipe.data.IsCooking)
                    {
                        dict.Add(crecipe.data.CraftingDataKey, crecipe.data.CraftingDataValue);
                        ++i;
                    }
                }
                Log.Trace("Added " + i + "/" + customRecipes.Count + " entries to cooking recipes");
            }
            else if (asset.AssetNameEquals("Data\\CraftingRecipes"))
            {
                var dict = asset.AsDictionary<string, string>().Data;
                int i = 0;
                foreach (var crecipe in customRecipes)
                {
                    if (crecipe.data.Enabled && !crecipe.data.IsCooking)
                    {
                        dict.Add(crecipe.data.CraftingDataKey, crecipe.data.CraftingDataValue);
                        ++i;
                    }
                }
                Log.Trace("Added " + i + "/" + customRecipes.Count + " entries to crafting recipes");
            }
            else if (asset.AssetNameEquals("Data\\ObjectInformation"))
            {
                asset.AsDictionary<int, string>().Data.Add(BaseFakeObjectId, "DGA Dummy Object/0/0/Basic -20/DGA Dummy Object/You shouldn't have this./food/0 0 0 0 0 0 0 0 0 0 0 0/0");
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

        private void RefreshShopEntries()
        {
            State.TodaysShopEntries.Clear();
            foreach ( var cp in contentPacks )
            {
                foreach ( var shopEntry in cp.Value.others.OfType< ShopPackData >() )
                {
                    if ( shopEntry.Enabled )
                    {
                        if ( !State.TodaysShopEntries.ContainsKey( shopEntry.ShopId ) )
                            State.TodaysShopEntries.Add( shopEntry.ShopId, new List<ShopEntry>() );
                        State.TodaysShopEntries[ shopEntry.ShopId ].Add( new ShopEntry()
                        {
                            Item = shopEntry.Item.Create(),//MakeItemFrom( shopEntry.Item, cp.Value ),
                            Quantity = shopEntry.MaxSold,
                            Price = shopEntry.Cost,
                            Currency = shopEntry.Currency == null ? null : (shopEntry.Currency.Contains( '/' ) ? shopEntry.Currency : $"{cp.Key}/{shopEntry.Currency}")
                        } );
                    }
                }
            }
        }

        internal void RefreshSpritebatchCache()
        {
            if ( Game1.objectSpriteSheet == null )
                Game1.objectSpriteSheet = Game1.content.Load< Texture2D >( "Maps\\springobjects" );

            SpriteBatchTileSheetAdjustments.objectOverrides.Clear();
            foreach ( var cp in contentPacks )
            {
                foreach ( var obj in cp.Value.items.Values.OfType<ObjectPackData>() )
                {
                    var tex = cp.Value.GetTexture( obj.Texture, 16, 16 );
                    string fullId = $"{cp.Key}/{obj.ID}";
                    SpriteBatchTileSheetAdjustments.objectOverrides.Add( Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, fullId.GetHashCode(), 16, 16 ), tex );
                }
            }
        }
    }
}
