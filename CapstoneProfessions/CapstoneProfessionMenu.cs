using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace CapstoneProfessions
{
    public class CapstoneProfessionMenu : IClickableMenu
    {
        private readonly Rectangle cursorsGoldIcon = new Rectangle( 280, 411, 16, 16 );

        public const int region_okButton = 101;

        public const int region_leftProfession = 102;

        public const int region_rightProfession = 103;

        public const int basewidth = 768;

        public const int baseheight = 512;

        public bool informationUp;

        public bool isActive;

        public bool hasUpdatedProfessions;

        private int timerBeforeStart;

        private float scale;

        private Color leftProfessionColor = Game1.textColor;

        private Color rightProfessionColor = Game1.textColor;

        private MouseState oldMouseState;

        public ClickableTextureComponent starIcon;

        public ClickableTextureComponent okButton;

        public ClickableComponent leftProfession;

        public ClickableComponent rightProfession;

        private List<string> extraInfoForLevel = new List<string>();

        private List<string> leftProfessionDescription = new List<string>();

        private List<string> rightProfessionDescription = new List<string>();

        private Rectangle sourceRectForLevelIcon;

        private string title;

        private List<int> professionsToChoose = new List<int>();

        private List<TemporaryAnimatedSprite> littleStars = new List<TemporaryAnimatedSprite>();

        public bool hasMovedSelection;

        public CapstoneProfessionMenu()
            : base( Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512 )
        {
            Game1.player.team.endOfNightStatus.UpdateState( "level" );
            this.timerBeforeStart = 250;
            this.isActive = true;
            base.width = 960;
            base.height = 512;
            this.okButton = new ClickableTextureComponent( new Rectangle( base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64 ), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet( Game1.mouseCursors, 46 ), 1f )
            {
                myID = 101
            };
            this.extraInfoForLevel.Clear();
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.informationUp = true;
            this.title = Mod.instance.Helper.Translation.Get( "menu.title" );
            this.extraInfoForLevel = this.getExtraInfoForLevel();
            switch ( Game1.whichFarm )
            {
                case Farm.default_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 0, 324, 22, 20 );
                    break;
                case Farm.riverlands_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 22, 324, 22, 20 );
                    break;
                case Farm.forest_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 44, 324, 22, 20 );
                    break;
                case Farm.mountains_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 66, 324, 22, 20 );
                    break;
                case Farm.combat_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 88, 324, 22, 20 );
                    break;
                case Farm.fourCorners_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 0, 344, 22, 20 );
                    break;
                case Farm.beach_layout:
                    this.sourceRectForLevelIcon = new Rectangle( 22, 344, 22, 20 );
                    break;
            }
            int newHeight = 0;
            base.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
            Game1.player.freezePause = 100;
            this.gameWindowSizeChanged( Rectangle.Empty, Rectangle.Empty );
            //if ( this.isProfessionChooser )
            {
                this.leftProfession = new ClickableComponent( new Rectangle( base.xPositionOnScreen, base.yPositionOnScreen + 128, base.width / 2, base.height ), "" )
                {
                    myID = 102,
                    rightNeighborID = 103
                };
                this.rightProfession = new ClickableComponent( new Rectangle( base.width / 2 + base.xPositionOnScreen, base.yPositionOnScreen + 128, base.width / 2, base.height ), "" )
                {
                    myID = 103,
                    leftNeighborID = 102
                };
            }
            base.populateClickableComponentList();
        }

        public bool CanReceiveInput()
        {
            if ( !this.informationUp )
            {
                return false;
            }
            if ( this.timerBeforeStart > 0 )
            {
                return false;
            }
            return true;
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID( 103 );
            Game1.setMousePosition( base.xPositionOnScreen + base.width + 64, base.yPositionOnScreen + base.height + 64 );
        }

        public override void applyMovementKey( int direction )
        {
            if ( this.CanReceiveInput() )
            {
                if ( direction == 3 || direction == 1 )
                {
                    this.hasMovedSelection = true;
                }
                base.applyMovementKey( direction );
            }
        }

        public override void gameWindowSizeChanged( Rectangle oldBounds, Rectangle newBounds )
        {
            base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
            base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2;
            this.okButton.bounds = new Rectangle( base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 64 - IClickableMenu.borderWidth, 64, 64 );
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true )
        {
        }

        public List<string> getExtraInfoForLevel()
        {
            List<string> extraInfo = new List<string>();
            extraInfo.Add( Mod.instance.Helper.Translation.Get( "menu.extra" ) );
            return extraInfo;
        }

        public override void receiveRightClick( int x, int y, bool playSound = true )
        {
        }

        public override void performHoverAction( int x, int y )
        {
        }

        public override void update( GameTime time )
        {
            if ( !this.isActive )
            {
                base.exitThisMenu();
                return;
            }
            if ( !this.hasUpdatedProfessions )
            {
                professionsToChoose.Add( Mod.PROFESSION_TIME );
                professionsToChoose.Add( Mod.PROFESSION_PROFIT );
                this.leftProfessionDescription = new List<string>( new string[] { Mod.instance.Helper.Translation.Get( "profession.time.name" ), Mod.instance.Helper.Translation.Get( "profession.time.description" ) } );
                this.rightProfessionDescription = new List<string>( new string[] { Mod.instance.Helper.Translation.Get( "profession.profit.name" ), Mod.instance.Helper.Translation.Get( "profession.profit.description" ) } );
                this.hasUpdatedProfessions = true;
            }
            for ( int i = this.littleStars.Count - 1; i >= 0; i-- )
            {
                if ( this.littleStars[ i ].update( time ) )
                {
                    this.littleStars.RemoveAt( i );
                }
            }
            if ( Game1.random.NextDouble() < 0.03 )
            {
                Vector2 position = new Vector2(0f, Game1.random.Next(base.yPositionOnScreen - 128, base.yPositionOnScreen - 4) / 20 * 4 * 5 + 32);
                if ( Game1.random.NextDouble() < 0.5 )
                {
                    position.X = Game1.random.Next( base.xPositionOnScreen + base.width / 2 - 228, base.xPositionOnScreen + base.width / 2 - 132 );
                }
                else
                {
                    position.X = Game1.random.Next( base.xPositionOnScreen + base.width / 2 + 116, base.xPositionOnScreen + base.width - 160 );
                }
                if ( position.Y < ( float ) ( base.yPositionOnScreen - 64 - 8 ) )
                {
                    position.X = Game1.random.Next( base.xPositionOnScreen + base.width / 2 - 116, base.xPositionOnScreen + base.width / 2 + 116 );
                }
                position.X = position.X / 20f * 4f * 5f;
                this.littleStars.Add( new TemporaryAnimatedSprite( "LooseSprites\\Cursors", new Rectangle( 364, 79, 5, 5 ), 80f, 7, 1, position, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f )
                {
                    local = true
                } );
            }
            if ( this.timerBeforeStart > 0 )
            {
                this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
                if ( this.timerBeforeStart <= 0 && Game1.options.SnappyMenus )
                {
                    base.populateClickableComponentList();
                    this.snapToDefaultClickableComponent();
                }
                return;
            }
            if ( this.isActive )
            {
                this.leftProfessionColor = Game1.textColor;
                this.rightProfessionColor = Game1.textColor;
                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.player.freezePause = 100;
                if ( Game1.getMouseY() > base.yPositionOnScreen + 192 && Game1.getMouseY() < base.yPositionOnScreen + base.height )
                {
                    if ( Game1.getMouseX() > base.xPositionOnScreen && Game1.getMouseX() < base.xPositionOnScreen + base.width / 2 )
                    {
                        this.leftProfessionColor = Color.Green;
                        if ( ( ( Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released ) || ( Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown( Buttons.A ) && !Game1.oldPadState.IsButtonDown( Buttons.A ) ) ) && this.readyToClose() )
                        {
                            Game1.player.professions.Add( this.professionsToChoose[ 0 ] );
                            this.isActive = false;
                            this.informationUp = false;
                        }
                    }
                    else if ( Game1.getMouseX() > base.xPositionOnScreen + base.width / 2 && Game1.getMouseX() < base.xPositionOnScreen + base.width )
                    {
                        this.rightProfessionColor = Color.Green;
                        if ( ( ( Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released ) || ( Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown( Buttons.A ) && !Game1.oldPadState.IsButtonDown( Buttons.A ) ) ) && this.readyToClose() )
                        {
                            Game1.player.professions.Add( this.professionsToChoose[ 1 ] );
                            this.isActive = false;
                            this.informationUp = false;
                        }
                    }
                }
                base.height = 512;
            }
            this.oldMouseState = Game1.input.GetMouseState();
            if ( this.isActive && !this.informationUp && this.starIcon != null )
            {
                if ( this.starIcon.containsPoint( Game1.getOldMouseX(), Game1.getOldMouseY() ) )
                {
                    this.starIcon.sourceRect.X = 294;
                }
                else
                {
                    this.starIcon.sourceRect.X = 310;
                }
            }
            if ( !this.isActive || !this.informationUp )
            {
                return;
            }
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.okButton.scale = Math.Max( 1f, this.okButton.scale - 0.05f );
            Game1.player.freezePause = 100;
        }

        public void okButtonClicked()
        {
            this.isActive = false;
            this.informationUp = false;
        }

        public override void receiveKeyPress( Keys key )
        {
            if ( Game1.options.SnappyMenus && ( ( !Game1.options.doesInputListContain( Game1.options.cancelButton, key ) && !Game1.options.doesInputListContain( Game1.options.menuButton, key ) ) ) )
            {
                base.receiveKeyPress( key );
            }
        }

        public override void draw( SpriteBatch b )
        {
            if ( this.timerBeforeStart > 0 )
            {
                return;
            }
            b.Draw( Game1.fadeToBlackRect, new Rectangle( 0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height ), Color.Black * 0.5f );
            foreach ( TemporaryAnimatedSprite littleStar in this.littleStars )
            {
                littleStar.draw( b );
            }
            b.Draw( Game1.mouseCursors, new Vector2( base.xPositionOnScreen + base.width / 2 - 116, base.yPositionOnScreen - 32 + 12 ), new Rectangle( 363, 87, 58, 22 ), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f );
            if ( !this.informationUp && this.isActive && this.starIcon != null )
            {
                this.starIcon.draw( b );
            }
            else
            {
                if ( !this.informationUp )
                {
                    return;
                }
                //if ( this.isProfessionChooser )
                {
                    if ( this.professionsToChoose.Count() == 0 )
                    {
                        return;
                    }
                    Game1.drawDialogueBox( base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true );
                    base.drawHorizontalPartition( b, base.yPositionOnScreen + 192 );
                    base.drawVerticalIntersectingPartition( b, base.xPositionOnScreen + base.width / 2 - 32, base.yPositionOnScreen + 192 );
                    Utility.drawWithShadow( b, Game1.mouseCursors, new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 ), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f );
                    b.DrawString( Game1.dialogueFont, this.title, new Vector2( ( float ) ( base.xPositionOnScreen + base.width / 2 ) - Game1.dialogueFont.MeasureString( this.title ).X / 2f, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 ), Game1.textColor );
                    Utility.drawWithShadow( b, Game1.mouseCursors, new Vector2( base.xPositionOnScreen + base.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 ), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f );
                    string chooseProfession = Mod.instance.Helper.Translation.Get( "menu.extra" );
                    b.DrawString( Game1.smallFont, chooseProfession, new Vector2( ( float ) ( base.xPositionOnScreen + base.width / 2 ) - Game1.smallFont.MeasureString( chooseProfession ).X / 2f, base.yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder ), Game1.textColor );
                    b.DrawString( Game1.dialogueFont, this.leftProfessionDescription[ 0 ], new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 ), this.leftProfessionColor );
                    b.Draw( Mod.clockTex, new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2 - 112, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16 ), null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f );
                    for ( int j = 1; j < this.leftProfessionDescription.Count; j++ )
                    {
                        b.DrawString( Game1.smallFont, Game1.parseText( this.leftProfessionDescription[ j ], Game1.smallFont, base.width / 2 - 64 ), new Vector2( -4 + base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * ( j + 1 ) ), this.leftProfessionColor );
                    }
                    b.DrawString( Game1.dialogueFont, this.rightProfessionDescription[ 0 ], new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 ), this.rightProfessionColor );
                    b.Draw( Game1.mouseCursors, new Vector2( base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width - 128, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16 ), cursorsGoldIcon, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f );
                    for ( int i = 1; i < this.rightProfessionDescription.Count; i++ )
                    {
                        b.DrawString( Game1.smallFont, Game1.parseText( this.rightProfessionDescription[ i ], Game1.smallFont, base.width / 2 - 48 ), new Vector2( -4 + base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + base.width / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * ( i + 1 ) ), this.rightProfessionColor );
                    }
                }
                if ( !Game1.options.SnappyMenus || this.hasMovedSelection )
                {
                    Game1.mouseCursorTransparency = 1f;
                    base.drawMouse( b );
                }
            }
        }
    }
}
