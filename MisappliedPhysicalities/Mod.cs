using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace MisappliedPhysicalities
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static IDynamicGameAssetsApi dga;

        internal static Texture2D toolsRadioactive;
        internal static Texture2D toolsMythicite;

        public override void Entry( IModHelper helper )
        {
            Log.Monitor = Monitor;
            instance = this;

            toolsRadioactive = Helper.Content.Load<Texture2D>( "assets/tools-radioactive.png" );
            toolsMythicite = Helper.Content.Load<Texture2D>( "assets/tools-mythicite.png" );

            Helper.ConsoleCommands.Add( "mp_items", "...", OnItemsCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Display.MenuChanged += OnMenuChanged;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
        }

        private void OnItemsCommand( string cmd, string[] args )
        {
            Dictionary<ISalable, int[]> stock = new();
            {
                // ...
            }
            Game1.activeClickableMenu = new ShopMenu( stock );
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var sc = Helper.ModRegistry.GetApi< ISpaceCoreApi >( "spacechase0.SpaceCore" );

            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>( "spacechase0.DynamicGameAssets" );

            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
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
    }
}
