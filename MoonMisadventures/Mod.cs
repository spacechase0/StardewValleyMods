using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game;
using MoonMisadventures.Game.Items;
using MoonMisadventures.Game.Locations;
using MoonMisadventures.Game.Monsters;
using MoonMisadventures.Game.Projectiles;
using MoonMisadventures.VirtualProperties;
using Netcode;
using SpaceCore.Events;
using SpaceCore.Interface;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

/* Art:
 *  paradigmnomad (most art)
 *  finalbossblues https://finalbossblues.itch.io/dark-dimension-tileset (recolored by paradigmnomad)
 *  ... more ...
 * Music:
 *  https://lowenergygirl.itch.io/space-journey (Into the Spaceship)
 */

namespace MoonMisadventures
{
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;
        internal static IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        internal static DepthStencilState DefaultStencilOverride = null;
        internal static DepthStencilState StencilBrighten = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilDarken = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 0,
            DepthBufferEnable = false,
        };
        internal static DepthStencilState StencilRenderOnDark = new()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.NotEqual,
            StencilPass = StencilOperation.Keep,
            ReferenceStencil = 1,
            DepthBufferEnable = false,
        };

        public override void Entry( IModHelper helper )
        {
            Log.Monitor = Monitor;
            instance = this;

            Assets.Load( helper.Content );
            SoundEffect mainMusic = SoundEffect.FromFile( Path.Combine( Helper.DirectoryPath, "assets", "into-the-spaceship.wav" ) );
            Game1.soundBank.AddCue( new CueDefinition( "into-the-spaceship", mainMusic, 2, loop: true ) );

            Helper.ConsoleCommands.Add( "mm_items", "View all items added by this mod.", OnItemsCommand );
            Helper.ConsoleCommands.Add( "mm_key", "Gives you the lunar key.", OnKeyCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.Display.RenderingWorld += OnRenderingWorld;
            Helper.Events.Display.RenderedWorld += OnRenderedWorld;
            Helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            Helper.Events.Player.Warped += OnWarped;

            SpaceEvents.AddWalletItems += AddWalletItems;
            SpaceEvents.AfterGiftGiven += AfterGiftGiven;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
            harmony.Patch( AccessTools.Method( "StardewModdingAPI.Framework.SGame:DrawImpl" ), transpiler: new HarmonyMethod( typeof( Patches.Game1CatchLightingRenderPatch ).GetMethod( "Transpiler" ) ) );
        }

        public bool CanLoad<T>( IAssetInfo asset )
        {
            if ( Game1.currentLocation is LunarLocation )
            {
                return asset.AssetNameEquals( "TerrainFeatures/hoeDirt" );
            }
            return false;
        }

        public T Load<T>( IAssetInfo asset )
        {
            if ( Game1.currentLocation is LunarLocation )
            {
                if ( asset.AssetNameEquals( "TerrainFeatures/hoeDirt" ) )
                    return ( T ) ( object ) Assets.HoeDirt;
            }
            return default( T );
        }

        private void OnItemsCommand( string cmd, string[] args )
        {
            Dictionary<ISalable, int[]> stock = new();
            {
                stock.Add( new AnimalGauntlets(), new int[] { 0, int.MaxValue } );
                {
                    var mp = Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
                    var ag = new AnimalGauntlets();
                    ag.holding.Value = new LunarAnimal( LunarAnimalType.Cow, Vector2.Zero, mp.getNewID() );
                    stock.Add( ag, new int[] { 0, int.MaxValue } );
                }
                {
                    var mp = Helper.Reflection.GetField< Multiplayer >( typeof( Game1 ), "multiplayer" ).GetValue();
                    var ag = new AnimalGauntlets();
                    ag.holding.Value = new LunarAnimal( LunarAnimalType.Chicken, Vector2.Zero, mp.getNewID() );
                    stock.Add( ag, new int[] { 0, int.MaxValue } );
                }
                foreach ( var type in Enum.GetValues<Necklace.Type>() )
                    stock.Add( new Necklace( type ), new int[] { 0, int.MaxValue } );
                foreach ( var data in dgaPack.GetItems() )
                {
                    var item = data.ToItem();
                    if ( item == null )
                        continue;
                    stock.Add( item, new int[] { 0, int.MaxValue } );
                }
            }
            Game1.activeClickableMenu = new ShopMenu( stock );
        }

        private void OnKeyCommand( string arg1, string[] arg2 )
        {
            Game1.player.team.get_hasLunarKey().Value = true;
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var sc = Helper.ModRegistry.GetApi< ISpaceCoreApi >( "spacechase0.SpaceCore" );
            sc.RegisterSerializerType( typeof( MountainTop ) );
            sc.RegisterSerializerType( typeof( LunarLocation ) );
            sc.RegisterSerializerType( typeof( MoonLandingArea ) );
            sc.RegisterSerializerType( typeof( AsteroidsEntrance ) );
            sc.RegisterSerializerType( typeof( AsteroidsDungeon ) );
            sc.RegisterSerializerType( typeof( BoomEye ) );
            sc.RegisterSerializerType( typeof( BoomProjectile ) );
            sc.RegisterSerializerType( typeof( AsteroidProjectile ) );
            sc.RegisterSerializerType( typeof( LunarFarm ) );
            sc.RegisterSerializerType( typeof( LunarFarmCave ) );
            sc.RegisterSerializerType( typeof( LunarAnimal ) );
            sc.RegisterSerializerType( typeof( AnimalGauntlets ) );
            sc.RegisterSerializerType( typeof( Necklace ) );
            sc.RegisterSerializerType( typeof( MoonPlanetOverlook ) );
            sc.RegisterSerializerType( typeof( UfoInterior ) );
            sc.RegisterSerializerType( typeof( LunarFarmHouse ) );
            sc.RegisterCustomLocationContext( "Moon",
                getLocationWeatherForTomorrowFunc: ( r ) =>
                {
                    LocationWeather lw = new();
                    lw.weatherForTomorrow.Value = 0;
                    lw.isRaining.Value = false;
                    return lw;
                }/*,
                passoutWakeupLocationFunc: ( who ) => "Custom_MM_MoonLandingArea",
                passoutWakeupPointFunc: ( who ) => new Point( 9, 30 ) // TODO: Inside farm house if unlocked
                */
            );
            sc.RegisterCustomProperty( typeof( FarmerTeam ), "hasLunarKey", typeof( NetBool ), AccessTools.Method( typeof( FarmerTeam_LunarKey ), nameof( FarmerTeam_LunarKey.get_hasLunarKey ) ), AccessTools.Method( typeof( FarmerTeam_LunarKey ), nameof( FarmerTeam_LunarKey.set_hasLunarKey ) ) );
            sc.RegisterCustomProperty( typeof( Farmer ), "necklaceItem", typeof( NetRef< Item > ), AccessTools.Method( typeof( Farmer_Necklace ), nameof( Farmer_Necklace.get_necklaceItem ) ), AccessTools.Method( typeof( Farmer_Necklace ), nameof( Farmer_Necklace.set_necklaceItem ) ) );

            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>( "spacechase0.DynamicGameAssets" );
            dga.AddEmbeddedPack( this.ModManifest, Path.Combine( Helper.DirectoryPath, "assets", "dga" ) );
            dgaPack = DynamicGameAssets.Mod.GetPacks().First( cp => cp.GetManifest().UniqueID == ModManifest.UniqueID );

            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
        }

        private void OnTimeChanged( object sender, TimeChangedEventArgs e )
        {
            AsteroidsDungeon.UpdateLevels10Minutes( e.NewTime );
        }

        private void OnUpdateTicked( object sender, UpdateTickedEventArgs e )
        {
            var necklace = Game1.player.get_necklaceItem().Value as Necklace;
            if ( necklace != null )
            {
                switch ( necklace.necklaceType.Value )
                {
                    case Necklace.Type.Speed:
                        {
                            var buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault( b => b.source == "necklace" );
                            if ( buff == null )
                            {
                                buff = new Buff( 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, 00, 0, 10, "necklace", "necklace" );
                                Game1.buffsDisplay.addOtherBuff( buff );
                            }
                            buff.millisecondsDuration = 1000;
                        }
                        break;
                    case Necklace.Type.Cooling:
                        {
                            if ( Game1.player.currentLocation is VolcanoDungeon volcano )
                            {
                                for ( int ix = -1; ix <= 1; ++ix )
                                {
                                    for ( int iy = -1; iy <= 1; ++iy )
                                    {
                                        var spot = Game1.player.getTileLocation() + new Vector2( ix, iy );
                                        if ( volcano.isTileOnMap( spot ) && volcano.waterTiles[ ( int ) spot.X, ( int ) spot.Y ] && !volcano.cooledLavaTiles.ContainsKey( spot ) )
                                            volcano.coolLavaEvent.Fire( new Point( ( int ) spot.X, ( int ) spot.Y ) );
                                    }
                                }
                            }
                        }
                        break;
                    case Necklace.Type.Sea:
                        {
                            if ( Game1.player.CurrentTool is FishingRod fr )
                            {
                                if ( fr.timeUntilFishingBite != -1 )
                                {
                                    fr.fishingBiteAccumulator += (int)(Game1.currentGameTime.ElapsedGameTime.Milliseconds * 1.5);
                                }
                                else if ( Game1.activeClickableMenu is BobberBar bb )
                                {
                                    if ( Helper.Reflection.GetField< bool >( bb, "bobberInBar" ).GetValue() )
                                    {
                                        var distCatchField = Helper.Reflection.GetField<float>( bb, "distanceFromCatching" );
                                        distCatchField.SetValue( distCatchField.GetValue() + 0.003f );
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        private void OnLoadStageChanged( object sender, LoadStageChangedEventArgs e )
        {
            if ( e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations )
            {
                Game1.locations.Add( new MountainTop( Helper.Content ) );
                Game1.locations.Add( new MoonLandingArea( Helper.Content ) );
                Game1.locations.Add( new AsteroidsEntrance( Helper.Content ) );
                Game1.locations.Add( new LunarFarm( Helper.Content ) );
                Game1.locations.Add( new LunarFarmCave( Helper.Content ) );
                Game1.locations.Add( new MoonPlanetOverlook( Helper.Content ) );
                Game1.locations.Add( new UfoInterior( Helper.Content ) );
                Game1.locations.Add( new LunarFarmHouse( Helper.Content ) );
            }
        }

        private void OnMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( e.NewMenu is ShopMenu shop )
            {
                if ( shop.storeContext != "ClintUpgrade" )
                    return;

                Tool orig = Game1.player.getToolFromName( "Axe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Axe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
                }

                orig = Game1.player.getToolFromName( "Watering Can" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new WateringCan() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
                }

                orig = Game1.player.getToolFromName( "Pickaxe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Pickaxe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
                }

                orig = Game1.player.getToolFromName( "Hoe" );
                if ( orig != null && ( orig.UpgradeLevel == 4 || orig.UpgradeLevel == 5 ) )
                {
                    Tool tool = new Hoe() { UpgradeLevel = orig.UpgradeLevel + 1 };
                    shop.forSale.Add( tool );
                    shop.itemPriceAndStock.Add( tool, new[] { tool.UpgradeLevel == 5 ? 100000 : 250000 } );
                }
            }
            else if ( e.NewMenu is AnimalQueryMenu aquery )
            {
                // We don't want the move animal button at all.
                // Hide it off screen, make it unreachable with controllers
                aquery.moveHomeButton.bounds = new Rectangle( 99999, 99999, 1, 1 ); 
                aquery.textBoxCC.downNeighborID = aquery.sellButton.myID;
                aquery.sellButton.upNeighborID = aquery.textBoxCC.myID;
            }
        }

        private void OnRenderingWorld( object sender, RenderingWorldEventArgs e )
        {
            if ( Game1.background is SpaceBackground )
            {
                // This part doesn't do anything normally (https://github.com/MonoGame/MonoGame/issues/5441),
                // but SpriteMaster makes it work. So need this for compatibility.
                if ( Game1.graphics.PreferredDepthStencilFormat != DepthFormat.Depth24Stencil8 )
                {
                    Game1.graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
                    Game1.graphics.ApplyChanges();
                }

                DefaultStencilOverride = StencilDarken;
                Game1.graphics.GraphicsDevice.Clear( ClearOptions.Stencil, Color.Transparent, 0, 0 );
            }
        }

        private void OnRenderedWorld( object sender, RenderedWorldEventArgs e )
        {
            DefaultStencilOverride = null;
        }

        private void OnReturnedToTitle( object sender, ReturnedToTitleEventArgs e )
        {
            AsteroidsDungeon.ClearAllLevels();
        }

        private void OnWarped( object sender, WarpedEventArgs e )
        {
            if ( e.OldLocation is LunarLocation ^ e.NewLocation is LunarLocation )
                Helper.Content.InvalidateCache( "TerrainFeatures/hoeDirt" );
        }

        private void AddWalletItems( object sender, EventArgs e )
        {
            var page = sender as NewSkillsPage;
            if ( Game1.player.team.get_hasLunarKey().Value )
                page.specialItems.Add( new ClickableTextureComponent(
                    name: "", bounds: new Rectangle( -1, -1, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom ),
                    label: null, hoverText: Helper.Translation.Get( "item.lunar-key.name" ),
                    texture: Assets.LunarKey, sourceRect: new Rectangle( 0, 0, 16, 16 ), scale: 4f, drawShadow: true ) );
        }

        private void AfterGiftGiven( object sender, EventArgsGiftGiven e )
        {
            if ( e.Gift is CustomObject cobj && cobj.FullId == ItemIds.SoulSapphire )
            {
                // NOTE: DGA doesn't currently support its items showing here anyways (I think).
                // 
                var farmer = sender as Farmer;
                foreach ( int key in Game1.objectInformation.Keys )
                {
                    var obj = new StardewValley.Object(key, 1);
                    if ( !obj.canBeGivenAsGift() || obj.questItem || obj.ParentSheetIndex == 809 )
                        continue;
                    if ( !farmer.giftedItems[ e.Npc.Name ].ContainsKey( key ) && ( !( obj.Name == "Stone" ) || key == 390 ) )
                        farmer.giftedItems[ e.Npc.Name ].Add( key, 0 );
                }
            }
        }
    }
}
