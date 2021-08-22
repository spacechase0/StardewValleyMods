using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace SpaceCore.Interface
{
    // Edited version of ForgeMenu
    // Patching what we needed was getting tricky (and I was getting lazy, so TODO: port PATCH_NEEDS_PORTING as transpiler)
    public class NewForgeMenu : MenuWithInventory
    {
        //<MINE>
        private CustomForgeRecipe justCrafted = null;
        private bool IsLeftCraftIngredient( Item item )
        {
            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( item ) )
                    return true;
            }

            return false;
        }
        private bool IsRightCraftIngredient( Item item )
        {
            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.IngredientItem.HasEnoughFor( item ) )
                    return true;
            }

            return false;
        }
        //</MINE>

        public enum CraftState
        {
            MissingIngredients,
            MissingShards,
            Valid,
            InvalidRecipe
        }

        protected int _timeUntilCraft;

        protected int _clankEffectTimer;

        protected int _sparklingTimer;

        public const int region_leftIngredient = 998;

        public const int region_rightIngredient = 997;

        public const int region_startButton = 996;

        public const int region_resultItem = 995;

        public const int region_unforgeButton = 994;

        public ClickableTextureComponent craftResultDisplay;

        public ClickableTextureComponent leftIngredientSpot;

        public ClickableTextureComponent rightIngredientSpot;

        public ClickableTextureComponent startTailoringButton;

        public ClickableComponent unforgeButton;

        public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

        public const int region_ring_1 = 110;

        public const int region_ring_2 = 111;

        public const int CRAFT_TIME = 1600;

        public Texture2D forgeTextures;

        protected Dictionary<Item, bool> _highlightDictionary;

        protected Dictionary<string, Item> _lastValidEquippedItems;

        protected List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

        private bool unforging;

        protected string displayedDescription = "";

        protected CraftState _craftState;

        public Vector2 questionMarkOffset;

        public NewForgeMenu()
            : base( null, okButton: true, trashCan: true, 12, 132 )
        {
            Game1.playSound( "bigSelect" );
            if ( base.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder )
            {
                base.movePosition( 0, -IClickableMenu.spaceToClearTopBorder );
            }
            base.inventory.highlightMethod = HighlightItems;
            this.forgeTextures = Game1.temporaryContent.Load<Texture2D>( "LooseSprites\\ForgeMenu" );
            this._CreateButtons();
            if ( base.trashCan != null )
            {
                base.trashCan.myID = 106;
            }
            if ( base.okButton != null )
            {
                base.okButton.leftNeighborID = 11;
            }
            if ( Game1.options.SnappyMenus )
            {
                base.populateClickableComponentList();
                this.snapToDefaultClickableComponent();
            }
            this._ValidateCraft();
        }

        protected void _CreateButtons()
        {
            this.leftIngredientSpot = new ClickableTextureComponent( new Rectangle( base.xPositionOnScreen + 204, base.yPositionOnScreen + 212, 64, 64 ), this.forgeTextures, new Rectangle( 142, 0, 16, 16 ), 4f )
            {
                myID = 998,
                downNeighborID = -99998,
                leftNeighborID = 110,
                rightNeighborID = 997,
                item = ( ( this.leftIngredientSpot != null ) ? this.leftIngredientSpot.item : null ),
                fullyImmutable = true
            };
            this.rightIngredientSpot = new ClickableTextureComponent( new Rectangle( base.xPositionOnScreen + 348, base.yPositionOnScreen + 212, 64, 64 ), this.forgeTextures, new Rectangle( 142, 0, 16, 16 ), 4f )
            {
                myID = 997,
                downNeighborID = 996,
                leftNeighborID = 998,
                rightNeighborID = 994,
                item = ( ( this.rightIngredientSpot != null ) ? this.rightIngredientSpot.item : null ),
                fullyImmutable = true
            };
            this.startTailoringButton = new ClickableTextureComponent( new Rectangle( base.xPositionOnScreen + 204, base.yPositionOnScreen + 308, 52, 56 ), this.forgeTextures, new Rectangle( 0, 80, 13, 14 ), 4f )
            {
                myID = 996,
                downNeighborID = -99998,
                leftNeighborID = 111,
                rightNeighborID = 994,
                upNeighborID = 998,
                item = ( ( this.startTailoringButton != null ) ? this.startTailoringButton.item : null ),
                fullyImmutable = true
            };
            this.unforgeButton = new ClickableComponent( new Rectangle( base.xPositionOnScreen + 484, base.yPositionOnScreen + 312, 40, 44 ), "Unforge" )
            {
                myID = 994,
                downNeighborID = -99998,
                leftNeighborID = 996,
                rightNeighborID = 995,
                upNeighborID = 997,
                fullyImmutable = true
            };
            if ( base.inventory.inventory != null && base.inventory.inventory.Count >= 12 )
            {
                for ( int j = 0; j < 12; j++ )
                {
                    if ( base.inventory.inventory[ j ] != null )
                    {
                        base.inventory.inventory[ j ].upNeighborID = -99998;
                    }
                }
            }
            this.craftResultDisplay = new ClickableTextureComponent( new Rectangle( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64 ), this.forgeTextures, new Rectangle( 0, 208, 16, 16 ), 4f )
            {
                myID = 995,
                downNeighborID = -99998,
                leftNeighborID = 996,
                upNeighborID = 997,
                item = ( ( this.craftResultDisplay != null ) ? this.craftResultDisplay.item : null )
            };
            this.equipmentIcons = new List<ClickableComponent>();
            this.equipmentIcons.Add( new ClickableComponent( new Rectangle( 0, 0, 64, 64 ), "Ring1" )
            {
                myID = 110,
                leftNeighborID = -99998,
                downNeighborID = -99998,
                upNeighborID = -99998,
                rightNeighborID = -99998
            } );
            this.equipmentIcons.Add( new ClickableComponent( new Rectangle( 0, 0, 64, 64 ), "Ring2" )
            {
                myID = 111,
                upNeighborID = -99998,
                downNeighborID = -99998,
                rightNeighborID = -99998,
                leftNeighborID = -99998
            } );
            for ( int i = 0; i < this.equipmentIcons.Count; i++ )
            {
                this.equipmentIcons[ i ].bounds.X = base.xPositionOnScreen - 64 + 9;
                this.equipmentIcons[ i ].bounds.Y = base.yPositionOnScreen + 192 + i * 64;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID( 0 );
            this.snapCursorToCurrentSnappedComponent();
        }

        public bool IsBusy()
        {
            if ( this._timeUntilCraft <= 0 )
            {
                return this._sparklingTimer > 0;
            }
            return true;
        }

        public override bool readyToClose()
        {
            if ( base.readyToClose() && base.heldItem == null )
            {
                return !this.IsBusy();
            }
            return false;
        }

        public bool HighlightItems( Item i )
        {
            if ( i == null )
            {
                return false;
            }
            if ( i != null && !this.IsValidCraftIngredient( i ) )
            {
                return false;
            }
            if ( this._highlightDictionary == null )
            {
                this.GenerateHighlightDictionary();
            }
            if ( !this._highlightDictionary.ContainsKey( i ) )
            {
                this._highlightDictionary = null;
                this.GenerateHighlightDictionary();
            }
            return this._highlightDictionary[ i ];
        }

        public void GenerateHighlightDictionary()
        {
            this._highlightDictionary = new Dictionary<Item, bool>();
            List<Item> item_list = new List<Item>(base.inventory.actualInventory);
            if ( Game1.player.leftRing.Value != null )
            {
                item_list.Add( Game1.player.leftRing.Value );
            }
            if ( Game1.player.rightRing.Value != null )
            {
                item_list.Add( Game1.player.rightRing.Value );
            }
            foreach ( Item item in item_list )
            {
                if ( item == null )
                {
                    continue;
                }
                if ( Utility.IsNormalObjectAtParentSheetIndex( item, 848 ) )
                {
                    this._highlightDictionary[ item ] = true;
                }
                else if ( this.leftIngredientSpot.item == null && this.rightIngredientSpot.item == null )
                {
                    bool valid = false;
                    if ( item is Ring )
                    {
                        valid = true;
                    }
                    if ( item is Tool && BaseEnchantment.GetAvailableEnchantmentsForItem( item as Tool ).Count > 0 )
                    {
                        valid = true;
                    }
                    if ( BaseEnchantment.GetEnchantmentFromItem( null, item ) != null )
                    {
                        valid = true;
                    }
                    this._highlightDictionary[ item ] = valid;
                }
                else if ( this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null )
                {
                    this._highlightDictionary[ item ] = false;
                }
                else if ( this.leftIngredientSpot.item != null )
                {
                    this._highlightDictionary[ item ] = this.IsValidCraft( this.leftIngredientSpot.item, item );
                }
                else
                {
                    this._highlightDictionary[ item ] = this.IsValidCraft( item, this.rightIngredientSpot.item );
                }
            }
        }

        private void _leftIngredientSpotClicked()
        {
            Item old_item = this.leftIngredientSpot.item;
            if ( ( base.heldItem == null || this.IsValidCraftIngredient( base.heldItem ) ) && ( base.heldItem == null || base.heldItem is Tool || base.heldItem is Ring ) || IsLeftCraftIngredient( base.heldItem ) )
            {
                Game1.playSound( "stoneStep" );
                this.leftIngredientSpot.item = base.heldItem;
                base.heldItem = old_item;
                this._highlightDictionary = null;
                this._ValidateCraft();
            }
        }

        public bool IsValidCraftIngredient( Item item )
        {
            if ( !item.canBeTrashed() && ( !( item is Tool ) || BaseEnchantment.GetAvailableEnchantmentsForItem( item as Tool ).Count <= 0 ) )
            {
                return false;
            }
            return true;
        }

        private void _rightIngredientSpotClicked()
        {
            Item old_item = this.rightIngredientSpot.item;
            if ( ( base.heldItem == null || this.IsValidCraftIngredient( base.heldItem ) ) && ( base.heldItem == null || ( int ) base.heldItem.parentSheetIndex != 848 ) )
            {
                Game1.playSound( "stoneStep" );
                this.rightIngredientSpot.item = base.heldItem;
                base.heldItem = old_item;
                this._highlightDictionary = null;
                this._ValidateCraft();
            }
        }

        public override void receiveKeyPress( Keys key )
        {
            if ( key == Keys.Delete )
            {
                if ( base.heldItem != null && this.IsValidCraftIngredient( base.heldItem ) )
                {
                    Utility.trashItem( base.heldItem );
                    base.heldItem = null;
                }
            }
            else
            {
                base.receiveKeyPress( key );
            }
        }

        public bool IsHoldingEquippedItem()
        {
            if ( base.heldItem == null )
            {
                return false;
            }
            if ( !Game1.player.IsEquippedItem( base.heldItem ) )
            {
                return Game1.player.IsEquippedItem( Utility.PerformSpecialItemGrabReplacement( base.heldItem ) );
            }
            return true;
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true )
        {
            Item old_held_item = base.heldItem;
            Game1.player.IsEquippedItem( old_held_item );
            base.receiveLeftClick( x, y );
            foreach ( ClickableComponent c in this.equipmentIcons )
            {
                if ( !c.containsPoint( x, y ) )
                {
                    continue;
                }
                string name = c.name;
                if ( !( name == "Ring1" ) )
                {
                    if ( !( name == "Ring2" ) || ( !this.HighlightItems( Game1.player.rightRing.Value ) && Game1.player.rightRing.Value != null ) )
                    {
                        return;
                    }
                    Item item_to_place2 = base.heldItem;
                    Item old_item2 = Game1.player.rightRing.Value;
                    if ( old_item2 != base.heldItem && ( item_to_place2 == null || item_to_place2 is Ring ) )
                    {
                        if ( Game1.player.rightRing.Value != null )
                        {
                            Game1.player.rightRing.Value.onUnequip( Game1.player, Game1.currentLocation );
                        }
                        Game1.player.rightRing.Value = ( item_to_place2 as Ring );
                        base.heldItem = old_item2;
                        if ( Game1.player.rightRing.Value != null )
                        {
                            Game1.player.rightRing.Value.onEquip( Game1.player, Game1.currentLocation );
                            Game1.playSound( "crit" );
                        }
                        else if ( base.heldItem != null )
                        {
                            Game1.playSound( "dwop" );
                        }
                        this._highlightDictionary = null;
                        this._ValidateCraft();
                    }
                }
                else
                {
                    if ( !this.HighlightItems( Game1.player.leftRing.Value ) && Game1.player.leftRing.Value != null )
                    {
                        return;
                    }
                    Item item_to_place = base.heldItem;
                    Item old_item = Game1.player.leftRing.Value;
                    if ( old_item != base.heldItem && ( item_to_place == null || item_to_place is Ring ) )
                    {
                        if ( Game1.player.leftRing.Value != null )
                        {
                            Game1.player.leftRing.Value.onUnequip( Game1.player, Game1.currentLocation );
                        }
                        Game1.player.leftRing.Value = ( item_to_place as Ring );
                        base.heldItem = old_item;
                        if ( Game1.player.leftRing.Value != null )
                        {
                            Game1.player.leftRing.Value.onEquip( Game1.player, Game1.currentLocation );
                            Game1.playSound( "crit" );
                        }
                        else if ( base.heldItem != null )
                        {
                            Game1.playSound( "dwop" );
                        }
                        this._highlightDictionary = null;
                        this._ValidateCraft();
                    }
                }
                return;
            }
            if ( Game1.GetKeyboardState().IsKeyDown( Keys.LeftShift ) && old_held_item != base.heldItem && base.heldItem != null )
            {
                //if ( base.heldItem is Tool || ( base.heldItem is Ring && this.leftIngredientSpot.item == null ) )
                if ( ( base.heldItem is Tool || base.heldItem is Ring || this.IsLeftCraftIngredient( base.heldItem ) ) && this.leftIngredientSpot.item == null )
                {
                    this._leftIngredientSpotClicked();
                }
                else
                {
                    this._rightIngredientSpotClicked();
                }
            }
            if ( this.IsBusy() )
            {
                return;
            }
            if ( this.leftIngredientSpot.containsPoint( x, y ) )
            {
                this._leftIngredientSpotClicked();
                if ( Game1.GetKeyboardState().IsKeyDown( Keys.LeftShift ) && base.heldItem != null )
                {
                    if ( Game1.player.IsEquippedItem( base.heldItem ) )
                    {
                        base.heldItem = null;
                    }
                    else
                    {
                        base.heldItem = base.inventory.tryToAddItem( base.heldItem, "" );
                    }
                }
            }
            else if ( this.rightIngredientSpot.containsPoint( x, y ) )
            {
                this._rightIngredientSpotClicked();
                if ( Game1.GetKeyboardState().IsKeyDown( Keys.LeftShift ) && base.heldItem != null )
                {
                    if ( Game1.player.IsEquippedItem( base.heldItem ) )
                    {
                        base.heldItem = null;
                    }
                    else
                    {
                        base.heldItem = base.inventory.tryToAddItem( base.heldItem, "" );
                    }
                }
            }
            else if ( this.startTailoringButton.containsPoint( x, y ) )
            {
                if ( base.heldItem == null )
                {
                    bool fail = false;
                    if ( !this.CanFitCraftedItem() )
                    {
                        Game1.playSound( "cancel" );
                        Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:Crop.cs.588" ) );
                        this._timeUntilCraft = 0;
                        fail = true;
                    }
                    if ( !fail && this.IsValidCraft( this.leftIngredientSpot.item, this.rightIngredientSpot.item ) && Game1.player.hasItemInInventory( 848, this.GetForgeCost( this.leftIngredientSpot.item, this.rightIngredientSpot.item ) ) )
                    {
                        Game1.playSound( "bigSelect" );
                        this.startTailoringButton.scale = this.startTailoringButton.baseScale;
                        this._timeUntilCraft = 1600;
                        this._clankEffectTimer = 300;
                        this._UpdateDescriptionText();
                        int crystals2 = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item);
                        for ( int k = 0; k < crystals2; k++ )
                        {
                            this.tempSprites.Add( new TemporaryAnimatedSprite( "", new Rectangle( 143, 17, 14, 15 ), new Vector2( base.xPositionOnScreen + 276, base.yPositionOnScreen + 300 ), flipped: false, 0.1f, Color.White )
                            {
                                texture = this.forgeTextures,
                                motion = new Vector2( -4f, -4f ),
                                scale = 4f,
                                layerDepth = 1f,
                                startSound = "boulderCrack",
                                delayBeforeAnimationStart = 1400 / crystals2 * k
                            } );
                        }
                        if ( this.rightIngredientSpot.item != null && ( int ) this.rightIngredientSpot.item.parentSheetIndex == 74 )
                        {
                            this._sparklingTimer = 900;
                            Rectangle r = this.leftIngredientSpot.bounds;
                            r.Offset( -32, -32 );
                            List<TemporaryAnimatedSprite> sparkles = Utility.sparkleWithinArea(r, 6, Color.White, 80, 1600);
                            sparkles.First().startSound = "discoverMineral";
                            this.tempSprites.AddRange( sparkles );
                            r = this.rightIngredientSpot.bounds;
                            r.Inflate( -16, -16 );
                            Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
                            int num = 30;
                            for ( int j = 0; j < num; j++ )
                            {
                                position = Utility.getRandomPositionInThisRectangle( r, Game1.random );
                                this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 48, 2, 2 ), position, flipped: false, 0f, Color.White )
                                {
                                    motion = new Vector2( -4f, 0f ),
                                    yPeriodic = true,
                                    yPeriodicRange = 16f,
                                    yPeriodicLoopTime = 1200f,
                                    scale = 4f,
                                    layerDepth = 1f,
                                    animationLength = 12,
                                    interval = Game1.random.Next( 20, 40 ),
                                    totalNumberOfLoops = 1,
                                    delayBeforeAnimationStart = this._clankEffectTimer / num * j
                                } );
                            }
                        }
                    }
                    else
                    {
                        Game1.playSound( "sell" );
                    }
                }
                else
                {
                    Game1.playSound( "sell" );
                }
            }
            else if ( this.unforgeButton.containsPoint( x, y ) )
            {
                if ( this.rightIngredientSpot.item == null )
                {
                    if ( this.IsValidUnforge() )
                    {
                        if ( this.leftIngredientSpot.item is MeleeWeapon && !Game1.player.couldInventoryAcceptThisObject( 848, ( this.leftIngredientSpot.item as MeleeWeapon ).GetTotalForgeLevels() * 5 + ( ( this.leftIngredientSpot.item as MeleeWeapon ).GetTotalForgeLevels() - 1 ) * 2 ) )
                        {
                            this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_noroom" );
                            Game1.playSound( "cancel" );
                        }
                        else if ( this.leftIngredientSpot.item is CombinedRing && Game1.player.freeSpotsInInventory() < 2 )
                        {
                            this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_noroom" );
                            Game1.playSound( "cancel" );
                        }
                        else
                        {
                            this.unforging = true;
                            this._timeUntilCraft = 1600;
                            int crystals = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item) / 2;
                            for ( int i = 0; i < crystals; i++ )
                            {
                                Vector2 motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
                                if ( motion.X == 0f && motion.Y == 0f )
                                {
                                    motion = new Vector2( -4f, -4f );
                                }
                                this.tempSprites.Add( new TemporaryAnimatedSprite( "", new Rectangle( 143, 17, 14, 15 ), new Vector2( this.leftIngredientSpot.bounds.X, this.leftIngredientSpot.bounds.Y ), flipped: false, 0.1f, Color.White )
                                {
                                    alpha = 0.01f,
                                    alphaFade = -0.1f,
                                    alphaFadeFade = -0.005f,
                                    texture = this.forgeTextures,
                                    motion = motion,
                                    scale = 4f,
                                    layerDepth = 1f,
                                    startSound = "boulderCrack",
                                    delayBeforeAnimationStart = 1100 / crystals * i
                                } );
                            }
                            Game1.playSound( "debuffHit" );
                        }
                    }
                    else
                    {
                        this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_unforge_invalid" );
                        Game1.playSound( "cancel" );
                    }
                }
                else
                {
                    if ( this.IsValidUnforge( ignore_right_slot_occupancy: true ) )
                    {
                        this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_unforge_right_slot" );
                    }
                    else
                    {
                        this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_unforge_invalid" );
                    }
                    Game1.playSound( "cancel" );
                }
            }
            if ( base.heldItem == null || this.isWithinBounds( x, y ) || !base.heldItem.canBeTrashed() )
            {
                return;
            }
            if ( Game1.player.IsEquippedItem( base.heldItem ) )
            {
                if ( base.heldItem == Game1.player.hat.Value )
                {
                    Game1.player.hat.Value = null;
                }
                else if ( base.heldItem == Game1.player.shirtItem.Value )
                {
                    Game1.player.shirtItem.Value = null;
                }
                else if ( base.heldItem == Game1.player.pantsItem.Value )
                {
                    Game1.player.pantsItem.Value = null;
                }
            }
            Game1.playSound( "throwDownITem" );
            Game1.createItemDebris( base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection );
            base.heldItem = null;
        }

        protected virtual bool CheckHeldItem( Func<Item, bool> f = null )
        {
            return f?.Invoke( base.heldItem ) ?? ( base.heldItem != null );
        }

        public virtual int GetForgeCostAtLevel( int level )
        {
            return 10 + level * 5;
        }

        public virtual int GetForgeCost( Item left_item, Item right_item )
        {
            if ( right_item != null && ( int ) right_item.parentSheetIndex == 896 )
            {
                return 20;
            }
            if ( right_item != null && ( int ) right_item.parentSheetIndex == 74 )
            {
                return 20;
            }
            if ( right_item != null && ( int ) right_item.parentSheetIndex == 72 )
            {
                return 10;
            }
            if ( left_item is MeleeWeapon && right_item is MeleeWeapon )
            {
                return 10;
            }
            if ( left_item != null && left_item is Tool )
            {
                return this.GetForgeCostAtLevel( ( left_item as Tool ).GetTotalForgeLevels() );
            }
            if ( left_item != null && left_item is Ring && right_item != null && right_item is Ring )
            {
                return 20;
            }

            //<MINE>
            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
                {
                    return recipe.CinderShardCost;
                }
            }
            //</MINE>

            return 1;
        }

        protected void _ValidateCraft()
        {
            Item left_item = this.leftIngredientSpot.item;
            Item right_item = this.rightIngredientSpot.item;
            if ( left_item == null || right_item == null )
            {
                this._craftState = CraftState.MissingIngredients;
            }
            else if ( this.IsValidCraft( left_item, right_item ) )
            {
                this._craftState = CraftState.Valid;
                Item left_item_clone = left_item.getOne();
                if ( right_item != null && Utility.IsNormalObjectAtParentSheetIndex( right_item, 72 ) )
                {
                    ( left_item_clone as Tool ).AddEnchantment( new DiamondEnchantment() );
                    this.craftResultDisplay.item = left_item_clone;
                }
                else
                {
                    this.craftResultDisplay.item = this.CraftItem( left_item_clone, right_item.getOne() );
                }
            }
            else
            {
                this._craftState = CraftState.InvalidRecipe;
            }
            this._UpdateDescriptionText();
        }

        protected void _UpdateDescriptionText()
        {
            if ( this.IsBusy() )
            {
                if ( this.rightIngredientSpot.item != null && ( int ) this.rightIngredientSpot.item.parentSheetIndex == 74 )
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_enchanting" );
                }
                else
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_forging" );
                }
            }
            else if ( this._craftState == CraftState.MissingIngredients )
            {
                this.displayedDescription = ( this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_description1" ) + Environment.NewLine + Environment.NewLine + Game1.content.LoadString( "Strings\\UI:Forge_description2" ) );
            }
            else if ( this._craftState == CraftState.MissingShards )
            {
                if ( base.heldItem != null && base.heldItem.ParentSheetIndex == 848 )
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_shards" );
                }
                else
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_notenoughshards" );
                }
            }
            else if ( this._craftState == CraftState.Valid )
            {
                if ( !this.CanFitCraftedItem() )
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\StringsFromCSFiles:Crop.cs.588" );
                }
                else
                {
                    this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_valid" );
                }
            }
            else if ( this._craftState == CraftState.InvalidRecipe )
            {
                this.displayedDescription = Game1.content.LoadString( "Strings\\UI:Forge_wrongorder" );
            }
            else
            {
                this.displayedDescription = "";
            }
        }

        public bool IsValidCraft( Item left_item, Item right_item )
        {
            if ( left_item == null || right_item == null )
            {
                return false;
            }
            if ( left_item is Tool && ( left_item as Tool ).CanForge( right_item ) )
            {
                return true;
            }
            if ( left_item is Ring && right_item is Ring && ( left_item as Ring ).CanCombine( right_item as Ring ) )
            {
                return true;
            }

            //<MINE>
            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
                {
                    return true;
                }
            }
            //</MINE>

            return false;
        }

        public Item CraftItem( Item left_item, Item right_item, bool forReal = false )
        {
            if ( left_item == null || right_item == null )
            {
                return null;
            }
            if ( left_item is Tool && !( left_item as Tool ).Forge( right_item, forReal ) )
            {
                return null;
            }
            if ( left_item is Ring && right_item is Ring )
            {
                left_item = ( left_item as Ring ).Combine( right_item as Ring );
            }

            //<MINE>
            foreach ( var recipe in CustomForgeRecipe.Recipes )
            {
                if ( recipe.BaseItem.HasEnoughFor( left_item ) && recipe.IngredientItem.HasEnoughFor( right_item ) )
                {
                    if ( forReal )
                        justCrafted = recipe;
                    return recipe.CreateResult( left_item, right_item );
                }
            }
            //</MINE>

            return left_item;
        }

        public void SpendRightItem()
        {
            //<MINE>
            if ( justCrafted != null )
            {
                justCrafted.IngredientItem.Consume( ref this.rightIngredientSpot.item );
                justCrafted = null;
                return;
            }
            //</MINE>

            if ( this.rightIngredientSpot.item != null )
            {
                this.rightIngredientSpot.item.Stack--;
                if ( this.rightIngredientSpot.item.Stack <= 0 || this.rightIngredientSpot.item.maximumStackSize() == 1 )
                {
                    this.rightIngredientSpot.item = null;
                }
            }
        }

        public void SpendLeftItem()
        {
            //<MINE>
            if ( justCrafted != null )
            {
                justCrafted.BaseItem.Consume( ref this.leftIngredientSpot.item );
                return;
            }
            //</MINE>

            if ( this.leftIngredientSpot.item != null )
            {
                this.leftIngredientSpot.item.Stack--;
                if ( this.leftIngredientSpot.item.Stack <= 0 || this.leftIngredientSpot.item.maximumStackSize() == 1 )
                {
                    this.leftIngredientSpot.item = null;
                }
            }
        }

        public override void receiveRightClick( int x, int y, bool playSound = true )
        {
            if ( !this.IsBusy() )
            {
                base.receiveRightClick( x, y );
            }
        }

        public override void performHoverAction( int x, int y )
        {
            if ( this.IsBusy() )
            {
                return;
            }
            base.hoveredItem = null;
            base.performHoverAction( x, y );
            base.hoverText = "";
            for ( int i = 0; i < this.equipmentIcons.Count; i++ )
            {
                if ( this.equipmentIcons[ i ].containsPoint( x, y ) )
                {
                    if ( this.equipmentIcons[ i ].name == "Ring1" )
                    {
                        base.hoveredItem = Game1.player.leftRing.Value;
                    }
                    else if ( this.equipmentIcons[ i ].name == "Ring2" )
                    {
                        base.hoveredItem = Game1.player.rightRing.Value;
                    }
                }
            }
            if ( this.craftResultDisplay.visible && this.craftResultDisplay.containsPoint( x, y ) && this.craftResultDisplay.item != null )
            {
                base.hoveredItem = this.craftResultDisplay.item;
            }
            if ( this.leftIngredientSpot.containsPoint( x, y ) && this.leftIngredientSpot.item != null )
            {
                base.hoveredItem = this.leftIngredientSpot.item;
            }
            if ( this.rightIngredientSpot.containsPoint( x, y ) && this.rightIngredientSpot.item != null )
            {
                base.hoveredItem = this.rightIngredientSpot.item;
            }
            if ( this.unforgeButton.containsPoint( x, y ) )
            {
                base.hoverText = Game1.content.LoadString( "Strings\\UI:Forge_Unforge" );
            }
            if ( this._craftState == CraftState.Valid && this.CanFitCraftedItem() )
            {
                this.startTailoringButton.tryHover( x, y, 0.33f );
            }
            else
            {
                this.startTailoringButton.tryHover( -999, -999 );
            }
        }

        public bool CanFitCraftedItem()
        {
            if ( this.craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList( this.craftResultDisplay.item, base.inventory.actualInventory ) )
            {
                return false;
            }
            return true;
        }

        public override void gameWindowSizeChanged( Rectangle oldBounds, Rectangle newBounds )
        {
            base.gameWindowSizeChanged( oldBounds, newBounds );
            int yPositionForInventory = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
            base.inventory = new InventoryMenu( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, base.inventory.highlightMethod );
            this._CreateButtons();
        }

        public override void emergencyShutDown()
        {
            this._OnCloseMenu();
            base.emergencyShutDown();
        }

        public override void update( GameTime time )
        {
            base.update( time );
            for ( int l = this.tempSprites.Count - 1; l >= 0; l-- )
            {
                if ( this.tempSprites[ l ].update( time ) )
                {
                    this.tempSprites.RemoveAt( l );
                }
            }
            if ( this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null && !Game1.player.hasItemInInventory( 848, this.GetForgeCost( this.leftIngredientSpot.item, this.rightIngredientSpot.item ) ) )
            {
                if ( this._craftState != CraftState.MissingShards )
                {
                    this._craftState = CraftState.MissingShards;
                    this.craftResultDisplay.item = null;
                    this._UpdateDescriptionText();
                }
            }
            else if ( this._craftState == CraftState.MissingShards )
            {
                this._ValidateCraft();
            }
            base.descriptionText = this.displayedDescription;
            this.questionMarkOffset.X = ( float ) Math.Sin( time.TotalGameTime.TotalSeconds * 2.5 ) * 4f;
            this.questionMarkOffset.Y = ( float ) Math.Cos( time.TotalGameTime.TotalSeconds * 5.0 ) * -4f;
            bool can_fit_crafted_item = this.CanFitCraftedItem();
            if ( this._craftState == CraftState.Valid && !this.IsBusy() && can_fit_crafted_item )
            {
                this.craftResultDisplay.visible = true;
            }
            else
            {
                this.craftResultDisplay.visible = false;
            }
            if ( this._timeUntilCraft <= 0 && this._sparklingTimer <= 0 )
            {
                return;
            }
            this.startTailoringButton.tryHover( this.startTailoringButton.bounds.Center.X, this.startTailoringButton.bounds.Center.Y, 0.33f );
            this._timeUntilCraft -= ( int ) time.ElapsedGameTime.TotalMilliseconds;
            this._clankEffectTimer -= ( int ) time.ElapsedGameTime.TotalMilliseconds;
            if ( this._timeUntilCraft <= 0 && this._sparklingTimer > 0 )
            {
                this._sparklingTimer -= ( int ) time.ElapsedGameTime.TotalMilliseconds;
            }
            else if ( this._clankEffectTimer <= 0 && !this.unforging )
            {
                this._clankEffectTimer = 450;
                if ( this.rightIngredientSpot.item != null && ( int ) this.rightIngredientSpot.item.parentSheetIndex == 74 )
                {
                    Rectangle r2 = this.rightIngredientSpot.bounds;
                    r2.Inflate( -16, -16 );
                    Vector2 position2 = Utility.getRandomPositionInThisRectangle(r2, Game1.random);
                    int num = 30;
                    for ( int k = 0; k < num; k++ )
                    {
                        position2 = Utility.getRandomPositionInThisRectangle( r2, Game1.random );
                        this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 48, 2, 2 ), position2, flipped: false, 0f, Color.White )
                        {
                            motion = new Vector2( -4f, 0f ),
                            yPeriodic = true,
                            yPeriodicRange = 16f,
                            yPeriodicLoopTime = 1200f,
                            scale = 4f,
                            layerDepth = 1f,
                            animationLength = 12,
                            interval = Game1.random.Next( 20, 40 ),
                            totalNumberOfLoops = 1,
                            delayBeforeAnimationStart = this._clankEffectTimer / num * k
                        } );
                    }
                }
                else
                {
                    Game1.playSound( "crafting" );
                    Game1.playSound( "clank" );
                    Rectangle r = this.leftIngredientSpot.bounds;
                    r.Inflate( -21, -21 );
                    Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
                    this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 46, 2, 2 ), position, flipped: false, 0.015f, Color.White )
                    {
                        motion = new Vector2( -1f, -10f ),
                        acceleration = new Vector2( 0f, 0.6f ),
                        scale = 4f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = 30f,
                        totalNumberOfLoops = 1
                    } );
                    this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 46, 2, 2 ), position, flipped: false, 0.015f, Color.White )
                    {
                        motion = new Vector2( 0f, -8f ),
                        acceleration = new Vector2( 0f, 0.48f ),
                        scale = 4f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = 30f,
                        totalNumberOfLoops = 1
                    } );
                    this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 46, 2, 2 ), position, flipped: false, 0.015f, Color.White )
                    {
                        motion = new Vector2( 1f, -10f ),
                        acceleration = new Vector2( 0f, 0.6f ),
                        scale = 4f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = 30f,
                        totalNumberOfLoops = 1
                    } );
                    this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 46, 2, 2 ), position, flipped: false, 0.015f, Color.White )
                    {
                        motion = new Vector2( -2f, -8f ),
                        acceleration = new Vector2( 0f, 0.6f ),
                        scale = 2f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = 30f,
                        totalNumberOfLoops = 1
                    } );
                    this.tempSprites.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors2", new Rectangle( 114, 46, 2, 2 ), position, flipped: false, 0.015f, Color.White )
                    {
                        motion = new Vector2( 2f, -8f ),
                        acceleration = new Vector2( 0f, 0.6f ),
                        scale = 2f,
                        layerDepth = 1f,
                        animationLength = 12,
                        interval = 30f,
                        totalNumberOfLoops = 1
                    } );
                }
            }
            if ( this._timeUntilCraft > 0 || this._sparklingTimer > 0 )
            {
                return;
            }
            if ( this.unforging )
            {
                if ( this.leftIngredientSpot.item is MeleeWeapon )
                {
                    MeleeWeapon weapon = this.leftIngredientSpot.item as MeleeWeapon;
                    int cost = 0;
                    if ( weapon != null )
                    {
                        int weapon_forge_levels = weapon.GetTotalForgeLevels(for_unforge: true);
                        for ( int j = 0; j < weapon_forge_levels; j++ )
                        {
                            cost += this.GetForgeCostAtLevel( j );
                        }
                        if ( weapon.hasEnchantmentOfType<DiamondEnchantment>() )
                        {
                            cost += this.GetForgeCost( this.leftIngredientSpot.item, new StardewValley.Object( 72, 1 ) );
                        }
                        for ( int i = weapon.enchantments.Count - 1; i >= 0; i-- )
                        {
                            if ( weapon.enchantments[ i ].IsForge() )
                            {
                                weapon.RemoveEnchantment( weapon.enchantments[ i ] );
                            }
                        }
                        if ( weapon.appearance.Value >= 0 )
                        {
                            weapon.appearance.Value = -1;
                            weapon.IndexOfMenuItemView = weapon.getDrawnItemIndex();
                            cost += 10;
                        }
                        this.leftIngredientSpot.item = null;
                        Game1.playSound( "coin" );
                        base.heldItem = weapon;
                    }
                    Utility.CollectOrDrop( new StardewValley.Object( 848, cost / 2 ) );
                }
                else if ( this.leftIngredientSpot.item is CombinedRing )
                {
                    CombinedRing ring = this.leftIngredientSpot.item as CombinedRing;
                    if ( ring != null )
                    {
                        List<Ring> rings = new List<Ring>(ring.combinedRings);
                        ring.combinedRings.Clear();
                        foreach ( Ring item in rings )
                        {
                            Utility.CollectOrDrop( item );
                        }
                        this.leftIngredientSpot.item = null;
                        Game1.playSound( "coin" );
                    }
                    Utility.CollectOrDrop( new StardewValley.Object( 848, 10 ) );
                }
                this.unforging = false;
                this._timeUntilCraft = 0;
                this._ValidateCraft();
                return;
            }
            Game1.player.removeItemsFromInventory( 848, this.GetForgeCost( this.leftIngredientSpot.item, this.rightIngredientSpot.item ) );
            Item crafted_item = this.CraftItem(this.leftIngredientSpot.item, this.rightIngredientSpot.item, forReal: true);
            if ( crafted_item != null && !Utility.canItemBeAddedToThisInventoryList( crafted_item, base.inventory.actualInventory ) )
            {
                Game1.playSound( "cancel" );
                Game1.showRedMessage( Game1.content.LoadString( "Strings\\StringsFromCSFiles:Crop.cs.588" ) );
                this._timeUntilCraft = 0;
                return;
            }
            if ( this.leftIngredientSpot.item == crafted_item )
            {
                this.leftIngredientSpot.item = null;
            }
            else
            {
                this.SpendLeftItem();
            }
            this.SpendRightItem();
            Game1.playSound( "coin" );
            base.heldItem = crafted_item;
            this._timeUntilCraft = 0;
            this._ValidateCraft();
        }

        public virtual bool IsValidUnforge( bool ignore_right_slot_occupancy = false )
        {
            if ( !ignore_right_slot_occupancy && this.rightIngredientSpot.item != null )
            {
                return false;
            }
            if ( this.leftIngredientSpot.item != null && this.leftIngredientSpot.item is MeleeWeapon && ( ( this.leftIngredientSpot.item as MeleeWeapon ).GetTotalForgeLevels() > 0 || ( this.leftIngredientSpot.item as MeleeWeapon ).appearance.Value >= 0 ) )
            {
                return true;
            }
            if ( this.leftIngredientSpot.item != null && this.leftIngredientSpot.item is CombinedRing )
            {
                return true;
            }
            return false;
        }

        public override void draw( SpriteBatch b )
        {
            b.Draw( Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f );
            Game1.DrawBox( base.xPositionOnScreen - 64, base.yPositionOnScreen + 128, 128, 201, new Color( 116, 11, 3 ) );
            Game1.player.FarmerRenderer.drawMiniPortrat( b, new Vector2( ( float ) ( base.xPositionOnScreen - 64 ) + 9.6f, base.yPositionOnScreen + 128 ), 0.87f, 4f, 2, Game1.player );
            base.draw( b, drawUpperPortion: true, drawDescriptionArea: true, 116, 11, 3 );
            b.Draw( this.forgeTextures, new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder ), new Rectangle( 0, 0, 142, 80 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f );
            Color draw_color = Color.White;
            if ( this._craftState == CraftState.MissingShards )
            {
                draw_color = Color.Gray * 0.75f;
            }
            b.Draw( this.forgeTextures, new Vector2( base.xPositionOnScreen + 276, base.yPositionOnScreen + 300 ), new Rectangle( 142, 16, 17, 17 ), draw_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f );
            if ( this.leftIngredientSpot.item != null && this.rightIngredientSpot.item != null && this.IsValidCraft( this.leftIngredientSpot.item, this.rightIngredientSpot.item ) )
            {
                /*
                int source_offset = (this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item) - 10) / 5;
                if ( source_offset >= 0 && source_offset <= 2 )
                {
                    b.Draw( this.forgeTextures, new Vector2( base.xPositionOnScreen + 344, base.yPositionOnScreen + 320 ), new Rectangle( 142, 38 + source_offset * 10, 17, 10 ), Color.White * ( ( this._craftState == CraftState.MissingShards ) ? 0.5f : 1f ), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f );
                }
                */
                //<MINE>
                int cost = this.GetForgeCost(this.leftIngredientSpot.item, this.rightIngredientSpot.item);

                if ( cost is not ( 10 or 15 or 20 ) )
                    Game1.spriteBatch.DrawString( Game1.dialogueFont, "x" + cost, new Vector2( this.xPositionOnScreen + 345, this.yPositionOnScreen + 320 ), new Color( 226, 124, 65 ) );
                else
                {
                    int source_offset = (cost - 10) / 5;
                    if ( source_offset >= 0 && source_offset <= 2 )
                    {
                        b.Draw( this.forgeTextures, new Vector2( base.xPositionOnScreen + 344, base.yPositionOnScreen + 320 ), new Rectangle( 142, 38 + source_offset * 10, 17, 10 ), Color.White * ( ( this._craftState == CraftState.MissingShards ) ? 0.5f : 1f ), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f );
                    }
                }
                //</MINE>
            }
            if ( this.IsValidUnforge() )
            {
                b.Draw( this.forgeTextures, new Vector2( this.unforgeButton.bounds.X, this.unforgeButton.bounds.Y ), new Rectangle( 143, 69, 11, 10 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f );
            }
            if ( this._craftState == CraftState.Valid )
            {
                this.startTailoringButton.draw( b, Color.White, 0.96f, ( int ) Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 200 % 12 );
                this.startTailoringButton.drawItem( b, 16, 16 );
            }
            Point random_shaking = new Point(0, 0);
            bool left_slot_accepts_this_item = false;
            bool right_slot_accepts_this_item = false;
            Item highlight_item = base.hoveredItem;
            if ( base.heldItem != null )
            {
                highlight_item = base.heldItem;
            }
            if ( highlight_item != null && highlight_item != this.leftIngredientSpot.item && highlight_item != this.rightIngredientSpot.item && highlight_item != this.craftResultDisplay.item )
            {
                if ( highlight_item is Tool )
                {
                    if ( this.leftIngredientSpot.item is Tool )
                    {
                        right_slot_accepts_this_item = true;
                    }
                    else
                    {
                        left_slot_accepts_this_item = true;
                    }
                }
                if ( BaseEnchantment.GetEnchantmentFromItem( this.leftIngredientSpot.item, highlight_item ) != null )
                {
                    right_slot_accepts_this_item = true;
                }
                if ( highlight_item is Ring && !( highlight_item is CombinedRing ) && ( this.leftIngredientSpot.item == null || this.leftIngredientSpot.item is Ring ) && ( this.rightIngredientSpot.item == null || this.rightIngredientSpot.item is Ring ) )
                {
                    left_slot_accepts_this_item = true;
                    right_slot_accepts_this_item = true;
                }
                //<MINE> PATCH_NEEDS_PORTING
                if ( IsLeftCraftIngredient( highlight_item ) )
                    left_slot_accepts_this_item = true;
                if ( IsRightCraftIngredient( highlight_item ) )
                    right_slot_accepts_this_item = true;
                //</MINE>
            }
            foreach ( ClickableComponent c in this.equipmentIcons )
            {
                string name = c.name;
                if ( !( name == "Ring1" ) )
                {
                    if ( !( name == "Ring2" ) )
                    {
                        continue;
                    }
                    if ( Game1.player.rightRing.Value != null )
                    {
                        b.Draw( this.forgeTextures, c.bounds, new Rectangle( 0, 96, 16, 16 ), Color.White );
                        float transparency2 = 1f;
                        if ( !this.HighlightItems( ( Ring ) Game1.player.rightRing ) )
                        {
                            transparency2 = 0.5f;
                        }
                        if ( Game1.player.rightRing.Value == base.heldItem )
                        {
                            transparency2 = 0.5f;
                        }
                        Game1.player.rightRing.Value.drawInMenu( b, new Vector2( c.bounds.X, c.bounds.Y ), c.scale, transparency2, 0.866f, StackDrawType.Hide );
                    }
                    else
                    {
                        b.Draw( this.forgeTextures, c.bounds, new Rectangle( 16, 96, 16, 16 ), Color.White );
                    }
                }
                else if ( Game1.player.leftRing.Value != null )
                {
                    b.Draw( this.forgeTextures, c.bounds, new Rectangle( 0, 96, 16, 16 ), Color.White );
                    float transparency = 1f;
                    if ( !this.HighlightItems( ( Ring ) Game1.player.leftRing ) )
                    {
                        transparency = 0.5f;
                    }
                    if ( Game1.player.leftRing.Value == base.heldItem )
                    {
                        transparency = 0.5f;
                    }
                    Game1.player.leftRing.Value.drawInMenu( b, new Vector2( c.bounds.X, c.bounds.Y ), c.scale, transparency, 0.866f, StackDrawType.Hide );
                }
                else
                {
                    b.Draw( this.forgeTextures, c.bounds, new Rectangle( 16, 96, 16, 16 ), Color.White );
                }
            }
            if ( !this.IsBusy() )
            {
                if ( left_slot_accepts_this_item )
                {
                    this.leftIngredientSpot.draw( b, Color.White, 0.87f );
                }
            }
            else if ( this._clankEffectTimer > 300 || ( this._timeUntilCraft > 0 && this.unforging ) )
            {
                random_shaking.X = Game1.random.Next( -1, 2 );
                random_shaking.Y = Game1.random.Next( -1, 2 );
            }
            this.leftIngredientSpot.drawItem( b, random_shaking.X * 4, random_shaking.Y * 4 );
            if ( this.craftResultDisplay.visible )
            {
                string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
                Utility.drawTextWithColoredShadow( position: new Vector2( ( float ) this.craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString( make_result_text ).X / 2f, ( float ) this.craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString( make_result_text ).Y ), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f );
                if ( this.craftResultDisplay.item != null )
                {
                    this.craftResultDisplay.drawItem( b );
                }
            }
            if ( !this.IsBusy() && right_slot_accepts_this_item )
            {
                this.rightIngredientSpot.draw( b, Color.White, 0.87f );
            }
            this.rightIngredientSpot.drawItem( b );
            foreach ( TemporaryAnimatedSprite tempSprite in this.tempSprites )
            {
                tempSprite.draw( b, localPosition: true );
            }
            if ( !base.hoverText.Equals( "" ) )
            {
                IClickableMenu.drawHoverText( b, base.hoverText, Game1.smallFont, ( base.heldItem != null ) ? 32 : 0, ( base.heldItem != null ) ? 32 : 0 );
            }
            else if ( base.hoveredItem != null )
            {
                if ( base.hoveredItem == this.craftResultDisplay.item && Utility.IsNormalObjectAtParentSheetIndex( this.rightIngredientSpot.item, 74 ) )
                {
                    BaseEnchantment.hideEnchantmentName = true;
                }
                IClickableMenu.drawToolTip( b, base.hoveredItem.getDescription(), base.hoveredItem.DisplayName, base.hoveredItem, base.heldItem != null );
                BaseEnchantment.hideEnchantmentName = false;
            }
            if ( base.heldItem != null )
            {
                base.heldItem.drawInMenu( b, new Vector2( Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8 ), 1f );
            }
            if ( !Game1.options.hardwareCursor )
            {
                base.drawMouse( b );
            }
        }

        protected override void cleanupBeforeExit()
        {
            this._OnCloseMenu();
        }

        protected void _OnCloseMenu()
        {
            if ( !Game1.player.IsEquippedItem( base.heldItem ) )
            {
                Utility.CollectOrDrop( base.heldItem, 2 );
            }
            if ( !Game1.player.IsEquippedItem( this.leftIngredientSpot.item ) )
            {
                Utility.CollectOrDrop( this.leftIngredientSpot.item, 2 );
            }
            if ( !Game1.player.IsEquippedItem( this.rightIngredientSpot.item ) )
            {
                Utility.CollectOrDrop( this.rightIngredientSpot.item, 2 );
            }
            if ( !Game1.player.IsEquippedItem( this.startTailoringButton.item ) )
            {
                Utility.CollectOrDrop( this.startTailoringButton.item, 2 );
            }
            base.heldItem = null;
            this.leftIngredientSpot.item = null;
            this.rightIngredientSpot.item = null;
            this.startTailoringButton.item = null;
        }
    }
}
