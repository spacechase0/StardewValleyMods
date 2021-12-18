using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonMisadventures.Game;
using MoonMisadventures.Game.Locations;
using MoonMisadventures.Game.Monsters;
using MoonMisadventures.Game.Projectiles;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

/* Art:
 *  paradigmnomad (most art)
 *  finalbossblues https://finalbossblues.itch.io/dark-dimension-tileset (recolored by paradigmnomad)
 */

namespace MoonMisadventures
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        public override void Entry( IModHelper helper )
        {
            Log.Monitor = Monitor;
            instance = this;

            Assets.Load( helper.Content );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            Helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
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

            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>( "spacechase0.DynamicGameAssets" );
            dga.AddEmbeddedPack( this.ModManifest, Path.Combine( Helper.DirectoryPath, "assets", "dga" ) );
            dgaPack = DynamicGameAssets.Mod.GetPacks().First( cp => cp.GetManifest().UniqueID == ModManifest.UniqueID );

            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
        }

        private void OnTimeChanged( object sender, TimeChangedEventArgs e )
        {
            AsteroidsDungeon.UpdateLevels10Minutes( e.NewTime );
        }

        private void OnLoadStageChanged( object sender, LoadStageChangedEventArgs e )
        {
            if ( e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations )
            {
                Game1.locations.Add( new MountainTop( Helper.Content ) );
                Game1.locations.Add( new MoonLandingArea( Helper.Content ) );
                Game1.locations.Add( new AsteroidsEntrance( Helper.Content ) );
            }
        }

        private void OnMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( !( e.NewMenu is ShopMenu shop ) )
                return;

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

        private void OnReturnedToTitle( object sender, ReturnedToTitleEventArgs e )
        {
            AsteroidsDungeon.ClearAllLevels();
        }
    }
}
