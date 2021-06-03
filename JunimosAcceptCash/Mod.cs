using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace JunimosAcceptCash
{
    public class Mod : StardewModdingAPI.Mod
    {
        public Mod instance;
        public Configuration Config;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Config = helper.ReadConfig<Configuration>();

            helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.Display.MenuChanged += onMenuChanged;
        }

        private void onGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var gmcm = Helper.ModRegistry.GetApi< GenericModConfigMenuAPI >( "spacechase0.GenericModConfigMenu" );
            if ( gmcm != null )
            {
                gmcm.RegisterModConfig( ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig( Config ) );
                gmcm.RegisterSimpleOption( ModManifest, "Cost Multiplier", "The multiplier for the cost of the items to charge.", () => Config.CostMultiplier, ( i ) => Config.CostMultiplier = i );
            }
        }

        private JunimoNoteMenu activeMenu = null;
        private void onMenuChanged( object sender, MenuChangedEventArgs e )
        {
            if ( e.NewMenu is JunimoNoteMenu menu )
            {
                activeMenu = menu;
                Helper.Events.Display.RenderedActiveMenu += onRenderMenu;
                Helper.Events.GameLoop.UpdateTicked += onUpdated;
            }
            else if ( e.OldMenu is JunimoNoteMenu )
            {
                activeMenu = null;
                Helper.Events.Display.RenderedActiveMenu -= onRenderMenu;
                Helper.Events.GameLoop.UpdateTicked -= onUpdated;
            }
        }

        private void onUpdated( object sender, UpdateTickedEventArgs e )
        {
            if ( activeMenu == null )
                return;

            var currentPageBundle = Helper.Reflection.GetField<Bundle>( activeMenu, "currentPageBundle" ).GetValue();
            if ( currentPageBundle == null || !Helper.Reflection.GetField<bool>( activeMenu, "specificBundlePage" ).GetValue() )
                return;

            if ( purchaseButton == null )
                purchaseButton = new ClickableTextureComponent( new Rectangle( activeMenu.xPositionOnScreen + 800, activeMenu.yPositionOnScreen + 504, 260, 72 ), activeMenu.noteTexture, new Rectangle( 517, 286, 65, 20 ), 4f, false );
            else
            {
                purchaseButton.bounds.Y = activeMenu.yPositionOnScreen;
            }

            purchaseButton.tryHover( Game1.getOldMouseX(), Game1.getOldMouseY() );
            
            if ( purchaseButton.containsPoint( Game1.getOldMouseX(), Game1.getOldMouseY() ) && Helper.Input.IsDown( SButton.MouseLeft ) )
            {
                Helper.Input.Suppress( SButton.MouseLeft );

                int whichArea = Helper.Reflection.GetField< int >( activeMenu, "whichArea" ).GetValue();

                // Copied from JunimoNoteMenu, modified
                int stack = calculateActiveBundleCost();
                if ( Game1.player.Money >= stack )
                {
                    Game1.player.Money -= stack;
                    Game1.playSound( "select" );
                    if ( this.purchaseButton != null )
                        this.purchaseButton.scale = this.purchaseButton.baseScale * 0.75f;
                    for ( int i = 0; i < currentPageBundle.numberOfIngredientSlots; ++i )
                    {
                        activeMenu.ingredientSlots[ i ].item = new StardewValley.Object( currentPageBundle.ingredients[ i ].index, currentPageBundle.ingredients[ i ].stack, false, -1, currentPageBundle.ingredients[ i ].quality );
                    }
                    Helper.Reflection.GetMethod( activeMenu, "checkIfBundleIsComplete" ).Invoke();

                    Helper.Reflection.GetMethod( activeMenu, "closeBundlePage" ).Invoke();
                }
                else
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
            }
        }

        private ClickableTextureComponent purchaseButton;
        private void onRenderMenu( object sender, RenderedActiveMenuEventArgs e )
        {
            if ( activeMenu == null )
                return;

            var currentPageBundle = Helper.Reflection.GetField<Bundle>( activeMenu, "currentPageBundle" ).GetValue();
            if ( currentPageBundle == null || !Helper.Reflection.GetField<bool>( activeMenu, "specificBundlePage" ).GetValue() )
                return;
            
            if ( purchaseButton == null )
                return;
            
            if ( Helper.Reflection.GetField<ClickableTextureComponent>( activeMenu, "purchaseButton" ).GetValue() != null )
                return;

            StardewValley.BellsAndWhistles.SpriteText.drawString( e.SpriteBatch, $"{calculateActiveBundleCost()}g", purchaseButton.bounds.X - 150, purchaseButton.bounds.Y + 10 );
            purchaseButton.draw( e.SpriteBatch );
            Game1.dayTimeMoneyBox.drawMoneyBox( e.SpriteBatch, -1, -1 );
            activeMenu.drawMouse( e.SpriteBatch );
        }

        // TODO: Cache this value
        private int calculateActiveBundleCost()
        {
            var bundle = Helper.Reflection.GetField<Bundle>( activeMenu, "currentPageBundle" ).GetValue();

            int cost = 0;
            List<int> used = new List<int>();
            for ( int i = 0; i < bundle.numberOfIngredientSlots; ++i )
            {
                if ( activeMenu.ingredientSlots[ i ].item != null )
                    continue;

                int mostExpensiveSlot = 0;
                int mostExpensiveCost = new StardewValley.Object( bundle.ingredients[ 0 ].index, bundle.ingredients[ 0 ].stack, false, -1, bundle.ingredients[ 0 ].quality ).sellToStorePrice() * bundle.ingredients[ 0 ].stack;
                for ( int j = 1; j < bundle.ingredients.Count; ++j )
                {
                    if ( used.Contains( j ) )
                        continue;

                    int itemCost = new StardewValley.Object( bundle.ingredients[ j ].index, bundle.ingredients[ j ].stack, false, -1, bundle.ingredients[ j ].quality ).sellToStorePrice() * bundle.ingredients[ j ].stack;
                    if ( cost > mostExpensiveCost )
                    {
                        mostExpensiveSlot = j;
                        mostExpensiveCost = itemCost;
                    }
                }
                used.Add( mostExpensiveSlot );
                cost += mostExpensiveCost;
            }

            return cost * Config.CostMultiplier;
        }
    }
}
