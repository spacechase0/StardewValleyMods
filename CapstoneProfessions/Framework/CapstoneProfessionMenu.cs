using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace CapstoneProfessions.Framework
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.CopiedFromGameCode)]
    internal class CapstoneProfessionMenu : IClickableMenu
    {
        private readonly Rectangle cursorsGoldIcon = new(280, 411, 16, 16);

        public const int region_okButton = 101;

        public const int region_leftProfession = 102;

        public const int region_rightProfession = 103;

        public const int basewidth = 768;

        public const int baseheight = 512;

        public bool informationUp;

        public bool isActive;

        public bool hasUpdatedProfessions;

        private int timerBeforeStart;

        private Color leftProfessionColor = Game1.textColor;

        private Color rightProfessionColor = Game1.textColor;

        private MouseState oldMouseState;

        public ClickableTextureComponent starIcon;

        public ClickableTextureComponent okButton;

        public ClickableComponent leftProfession;

        public ClickableComponent rightProfession;

        private readonly List<string> extraInfoForLevel = new();

        private List<string> leftProfessionDescription = new();

        private List<string> rightProfessionDescription = new();

        private readonly Rectangle sourceRectForLevelIcon;

        private readonly string title;

        private readonly List<int> professionsToChoose = new();

        private readonly List<TemporaryAnimatedSprite> littleStars = new();

        public bool hasMovedSelection;

        public CapstoneProfessionMenu()
            : base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
        {
            Game1.player.team.endOfNightStatus.UpdateState("level");
            this.timerBeforeStart = 250;
            this.isActive = true;
            this.width = 960;
            this.height = 512;
            this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 101
            };
            this.extraInfoForLevel.Clear();
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.informationUp = true;
            this.title = I18n.Menu_Title();
            this.extraInfoForLevel = this.getExtraInfoForLevel();
            this.sourceRectForLevelIcon = Game1.whichFarm switch
            {
                Farm.default_layout => new Rectangle(0, 324, 22, 20),
                Farm.riverlands_layout => new Rectangle(22, 324, 22, 20),
                Farm.forest_layout => new Rectangle(44, 324, 22, 20),
                Farm.mountains_layout => new Rectangle(66, 324, 22, 20),
                Farm.combat_layout => new Rectangle(88, 324, 22, 20),
                Farm.fourCorners_layout => new Rectangle(0, 344, 22, 20),
                Farm.beach_layout => new Rectangle(22, 344, 22, 20),
                _ => Rectangle.Empty
            };
            int newHeight = 0;
            this.height = newHeight + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
            Game1.player.freezePause = 100;
            this.gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
            //if ( this.isProfessionChooser )
            {
                this.leftProfession = new ClickableComponent(new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen + 128, this.width / 2, this.height), "")
                {
                    myID = 102,
                    rightNeighborID = 103
                };
                this.rightProfession = new ClickableComponent(new Rectangle(this.width / 2 + this.xPositionOnScreen, this.yPositionOnScreen + 128, this.width / 2, this.height), "")
                {
                    myID = 103,
                    leftNeighborID = 102
                };
            }
            this.populateClickableComponentList();
        }

        public bool CanReceiveInput()
        {
            if (!this.informationUp)
            {
                return false;
            }
            if (this.timerBeforeStart > 0)
            {
                return false;
            }
            return true;
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.getComponentWithID(103);
            Game1.setMousePosition(this.xPositionOnScreen + this.width + 64, this.yPositionOnScreen + this.height + 64);
        }

        public override void applyMovementKey(int direction)
        {
            if (this.CanReceiveInput())
            {
                if (direction is 3 or 1)
                {
                    this.hasMovedSelection = true;
                }
                base.applyMovementKey(direction);
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            this.okButton.bounds = new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public List<string> getExtraInfoForLevel()
        {
            return new()
            {
                I18n.Menu_Extra()
            };
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
        }

        public override void update(GameTime time)
        {
            if (!this.isActive)
            {
                this.exitThisMenu();
                return;
            }
            if (!this.hasUpdatedProfessions)
            {
                this.professionsToChoose.Add(Mod.ProfessionTime);
                this.professionsToChoose.Add(Mod.ProfessionProfit);
                this.leftProfessionDescription = new List<string>(new[] { I18n.Profession_Time_Name(), I18n.Profession_Time_Description() });
                this.rightProfessionDescription = new List<string>(new[] { I18n.Profession_Profit_Name(), I18n.Profession_Profit_Description() });
                this.hasUpdatedProfessions = true;
            }
            for (int i = this.littleStars.Count - 1; i >= 0; i--)
            {
                if (this.littleStars[i].update(time))
                {
                    this.littleStars.RemoveAt(i);
                }
            }
            if (Game1.random.NextDouble() < 0.03)
            {
                Vector2 position = new Vector2(0f, Game1.random.Next(this.yPositionOnScreen - 128, this.yPositionOnScreen - 4) / 20 * 4 * 5 + 32)
                {
                    X = Game1.random.NextDouble() < 0.5
                        ? Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 228, this.xPositionOnScreen + this.width / 2 - 132)
                        : Game1.random.Next(this.xPositionOnScreen + this.width / 2 + 116, this.xPositionOnScreen + this.width - 160)
                };
                if (position.Y < this.yPositionOnScreen - 64 - 8)
                {
                    position.X = Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 116, this.xPositionOnScreen + this.width / 2 + 116);
                }
                position.X = position.X / 20f * 4f * 5f;
                this.littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
                {
                    local = true
                });
            }
            if (this.timerBeforeStart > 0)
            {
                this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
                if (this.timerBeforeStart <= 0 && Game1.options.SnappyMenus)
                {
                    this.populateClickableComponentList();
                    this.snapToDefaultClickableComponent();
                }
                return;
            }
            if (this.isActive)
            {
                this.leftProfessionColor = Game1.textColor;
                this.rightProfessionColor = Game1.textColor;
                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.player.freezePause = 100;
                if (Game1.getMouseY() > this.yPositionOnScreen + 192 && Game1.getMouseY() < this.yPositionOnScreen + this.height)
                {
                    if (Game1.getMouseX() > this.xPositionOnScreen && Game1.getMouseX() < this.xPositionOnScreen + this.width / 2)
                    {
                        this.leftProfessionColor = Color.Green;
                        if (Game1.didPlayerJustLeftClick() && this.readyToClose())
                        {
                            Game1.player.professions.Add(this.professionsToChoose[0]);
                            this.isActive = false;
                            this.informationUp = false;
                        }
                    }
                    else if (Game1.getMouseX() > this.xPositionOnScreen + this.width / 2 && Game1.getMouseX() < this.xPositionOnScreen + this.width)
                    {
                        this.rightProfessionColor = Color.Green;
                        if (Game1.didPlayerJustLeftClick() && this.readyToClose())
                        {
                            Game1.player.professions.Add(this.professionsToChoose[1]);
                            this.isActive = false;
                            this.informationUp = false;
                        }
                    }
                }
                this.height = 512;
            }
            this.oldMouseState = Game1.input.GetMouseState();
            if (this.isActive && !this.informationUp && this.starIcon != null)
            {
                this.starIcon.sourceRect.X = this.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())
                    ? 294
                    : 310;
            }
            if (!this.isActive || !this.informationUp)
            {
                return;
            }
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
            Game1.player.freezePause = 100;
        }

        public void okButtonClicked()
        {
            this.isActive = false;
            this.informationUp = false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (Game1.options.SnappyMenus && ((!Game1.options.doesInputListContain(Game1.options.cancelButton, key) && !Game1.options.doesInputListContain(Game1.options.menuButton, key))))
            {
                base.receiveKeyPress(key);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (this.timerBeforeStart > 0)
            {
                return;
            }
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            foreach (TemporaryAnimatedSprite littleStar in this.littleStars)
            {
                littleStar.draw(b);
            }
            b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width / 2 - 116, this.yPositionOnScreen - 32 + 12), new Rectangle(363, 87, 58, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            if (!this.informationUp && this.isActive && this.starIcon != null)
            {
                this.starIcon.draw(b);
            }
            else
            {
                if (!this.informationUp)
                {
                    return;
                }
                //if ( this.isProfessionChooser )
                {
                    if (!this.professionsToChoose.Any())
                    {
                        return;
                    }
                    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true);
                    this.drawHorizontalPartition(b, this.yPositionOnScreen + 192);
                    this.drawVerticalIntersectingPartition(b, this.xPositionOnScreen + this.width / 2 - 32, this.yPositionOnScreen + 192);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
                    b.DrawString(Game1.dialogueFont, this.title, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.dialogueFont.MeasureString(this.title).X / 2f, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f);
                    string chooseProfession = I18n.Menu_Extra();
                    b.DrawString(Game1.smallFont, chooseProfession, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.smallFont.MeasureString(chooseProfession).X / 2f, this.yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
                    b.DrawString(Game1.dialogueFont, this.leftProfessionDescription[0], new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.leftProfessionColor);
                    b.Draw(Mod.ClockTex, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2 - 112, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
                    for (int j = 1; j < this.leftProfessionDescription.Count; j++)
                    {
                        b.DrawString(Game1.smallFont, Game1.parseText(this.leftProfessionDescription[j], Game1.smallFont, this.width / 2 - 64), new Vector2(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (j + 1)), this.leftProfessionColor);
                    }
                    b.DrawString(Game1.dialogueFont, this.rightProfessionDescription[0], new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.rightProfessionColor);
                    b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width - 128, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), this.cursorsGoldIcon, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    for (int i = 1; i < this.rightProfessionDescription.Count; i++)
                    {
                        b.DrawString(Game1.smallFont, Game1.parseText(this.rightProfessionDescription[i], Game1.smallFont, this.width / 2 - 48), new Vector2(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (i + 1)), this.rightProfessionColor);
                    }
                }
                if (!Game1.options.SnappyMenus || this.hasMovedSelection)
                {
                    Game1.mouseCursorTransparency = 1f;
                    this.drawMouse(b);
                }
            }
        }
    }
}
