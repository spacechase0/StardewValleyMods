using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using Magic;

namespace Magic.Game.Interface
{
    // Copy from LevelUpMenu
    public class MagicLevelUpMenu : IClickableMenu
    {
        public const int basewidth = 768;

        public const int baseheight = 512;

        public bool informationUp;

        public bool isActive;

        public bool isProfessionChooser;

        private int currentLevel;

        private int timerBeforeStart;

        private float scale;

        private Color leftProfessionColor = Game1.textColor;

        private Color rightProfessionColor = Game1.textColor;

        private MouseState oldMouseState;

        private ClickableTextureComponent starIcon;

        private ClickableTextureComponent okButton;

        private List<CraftingRecipe> newCraftingRecipes = new List<CraftingRecipe>();

        private List<string> extraInfoForLevel = new List<string>();

        private List<string> leftProfessionDescription = new List<string>();

        private List<string> rightProfessionDescription = new List<string>();

        private Rectangle sourceRectForLevelIcon;

        private string title;

        private List<int> professionsToChoose = new List<int>();

        private List<TemporaryAnimatedSprite> littleStars = new List<TemporaryAnimatedSprite>();

        public MagicLevelUpMenu()
            : base(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 256, 768, 512, false)
        {
            this.width = Game1.tileSize * 12;
            this.height = Game1.tileSize * 8;
            this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - Game1.tileSize - IClickableMenu.borderWidth, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
        }

        public MagicLevelUpMenu(int level)
            : base(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 256, 768, 512, false)
        {
            this.timerBeforeStart = 250;
            this.isActive = true;
            this.width = Game1.tileSize * 12;
            this.height = Game1.tileSize * 8;
            this.okButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - Game1.tileSize - IClickableMenu.borderWidth, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f,false);
            this.newCraftingRecipes.Clear();
            this.extraInfoForLevel.Clear();
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.informationUp = true;
            this.isProfessionChooser = false;
            this.currentLevel = level;
            if (level == 10)
            {
                Game1.getSteamAchievement("Achievement_SingularTalent");
                if (Game1.player.farmingLevel == 10 && Game1.player.miningLevel == 10 && Game1.player.fishingLevel == 10 && Game1.player.foragingLevel == 10 && Game1.player.combatLevel == 10)
                {
                    Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
                }
            }
            this.title = string.Concat(new object[]
			{
				"Level ",
				this.currentLevel,
				" Magic"
			});
            this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentLevel);
            sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
            /*if ((this.currentLevel == 5 || this.currentLevel == 10))
            {
                this.professionsToChoose.Clear();
                this.isProfessionChooser = true;
                if (this.currentLevel == 5)
                {
                    this.professionsToChoose.Add(Mod.PROFESSION_SELLPRICE);
                    this.professionsToChoose.Add(Mod.PROFESSION_BUFFTIME);
                }
                else if (Game1.player.professions.Contains(Mod.PROFESSION_SELLPRICE))
                {
                    this.professionsToChoose.Add(Mod.PROFESSION_CONSERVATION);
                    this.professionsToChoose.Add(Mod.PROFESSION_SILVER);
                }
                else
                {
                    this.professionsToChoose.Add(Mod.PROFESSION_BUFFLEVEL);
                    this.professionsToChoose.Add(Mod.PROFESSION_BUFFPLAIN);
                }
                this.leftProfessionDescription = CookingLevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
                this.rightProfessionDescription = CookingLevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
            }*/
            int num = 0;
            this.height = num + Game1.tileSize * 4 + this.extraInfoForLevel.Count<string>() * Game1.tileSize * 3 / 4;
            Game1.player.freezePause = 100;
            this.gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = Game1.viewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.viewport.Height / 2 - this.height / 2;
            this.okButton.bounds = new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - Game1.tileSize - IClickableMenu.borderWidth, Game1.tileSize, Game1.tileSize);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public List<string> getExtraInfoForLevel(int whichLevel)
        {
            List<string> list = new List<string>();
            list.Add("+10 max mana");
            return list;
        }

        public static void addProfessionDescription( List< string > list, int whichProfession)
        {/*
            switch (whichProfession)
            {
                case Mod.PROFESSION_SELLPRICE:
                    list.Add("Gourmet");
                    list.Add("+20% sell price");
                    break;
                case Mod.PROFESSION_BUFFTIME:
                    list.Add("Satisfying");
                    list.Add("+25% buff duration once eaten");
                    break;
                case Mod.PROFESSION_CONSERVATION:
                    list.Add("Efficient");
                    list.Add("15% chance to not consume ingredients");
                    break;
                case Mod.PROFESSION_SILVER:
                    list.Add("Professional Chef");
                    list.Add("Home-cooked meals are always at least silver");
                    break;
                case Mod.PROFESSION_BUFFLEVEL:
                    list.Add("Intense Flavors");
                    list.Add("Food buffs are one level stronger once eaten");
                    list.Add("(+20% for max energy or magnetism)");
                    break;
                case Mod.PROFESSION_BUFFPLAIN:
                    list.Add("Secret Spices");
                    list.Add("Provides a few random buffs when eating unbuffed food.");
                    break;
            }*/
        }

        public static string getProfessionName(int whichProfession)
        {/*
            switch (whichProfession)
            {
                case Mod.PROFESSION_SELLPRICE:
                    return "Gourmet";
                case Mod.PROFESSION_BUFFTIME:
                    return "Satisfying";
                case Mod.PROFESSION_CONSERVATION:
                    return "Efficient";
                case Mod.PROFESSION_SILVER:
                    return "Professional Chef";
                case Mod.PROFESSION_BUFFLEVEL:
                    return "Intense Flavors";
                case Mod.PROFESSION_BUFFPLAIN:
                    return "Secret Spices";
            }*/

            return "???";
        }

        public static List< string > getProfessionDescription( int whichProfession )
        {
            List<string> list = new List<string>();
            //CookingLevelUpMenu.addProfessionDescription(list, whichProfession);
            return list;
        }

        public static string getProfessionTitleFromNumber( int whichProfession )
        {
            return getProfessionName(whichProfession);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
        }

        public void getImmediateProfessionPerk(int whichProfession)
        {
        }

        public override void update(GameTime time)
        {
            if (!this.isActive)
            {
                base.exitThisMenu(true);
                return;
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
                Vector2 position = new Vector2(0f, (float)(Game1.random.Next(this.yPositionOnScreen - Game1.tileSize * 2, this.yPositionOnScreen - Game1.pixelZoom) / (Game1.pixelZoom * 5) * Game1.pixelZoom * 5 + Game1.tileSize / 2));
                if (Game1.random.NextDouble() < 0.5)
                {
                    position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 57 * Game1.pixelZoom, this.xPositionOnScreen + this.width / 2 - 33 * Game1.pixelZoom);
                }
                else
                {
                    position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 + 29 * Game1.pixelZoom, this.xPositionOnScreen + this.width - 40 * Game1.pixelZoom);
                }
                if (position.Y < (float)(this.yPositionOnScreen - Game1.tileSize - Game1.pixelZoom * 2))
                {
                    position.X = (float)Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 29 * Game1.pixelZoom, this.xPositionOnScreen + this.width / 2 + 29 * Game1.pixelZoom);
                }
                position.X = position.X / (float)(Game1.pixelZoom * 5) * (float)Game1.pixelZoom * 5f;
                this.littleStars.Add(new TemporaryAnimatedSprite(Game1.mouseCursorsName, new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, false, false, 1f, 0f, Color.White, (float)Game1.pixelZoom, 0f, 0f, 0f, false)
                {
                    local = true
                });
            }
            if (this.timerBeforeStart > 0)
            {
                this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
                return;
            }
            if (this.isActive && this.isProfessionChooser)
            {
                this.leftProfessionColor = Game1.textColor;
                this.rightProfessionColor = Game1.textColor;
                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.player.freezePause = 100;
                if (Game1.getMouseY() > this.yPositionOnScreen + Game1.tileSize * 3 && Game1.getMouseY() < this.yPositionOnScreen + this.height)
                {
                    if (Game1.getMouseX() > this.xPositionOnScreen && Game1.getMouseX() < this.xPositionOnScreen + this.width / 2)
                    {
                        this.leftProfessionColor = Color.Green;
                        if (((Mouse.GetState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
                        {
                            Game1.player.professions.Add(this.professionsToChoose[0]);
                            this.getImmediateProfessionPerk(this.professionsToChoose[0]);
                            this.isActive = false;
                            this.informationUp = false;
                            this.isProfessionChooser = false;
                        }
                    }
                    else if (Game1.getMouseX() > this.xPositionOnScreen + this.width / 2 && Game1.getMouseX() < this.xPositionOnScreen + this.width)
                    {
                        this.rightProfessionColor = Color.Green;
                        if (((Mouse.GetState().LeftButton == ButtonState.Pressed && this.oldMouseState.LeftButton == ButtonState.Released) || (Game1.options.gamepadControls && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) && !Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
                        {
                            Game1.player.professions.Add(this.professionsToChoose[1]);
                            this.getImmediateProfessionPerk(this.professionsToChoose[1]);
                            this.isActive = false;
                            this.informationUp = false;
                            this.isProfessionChooser = false;
                        }
                    }
                }
                this.height = Game1.tileSize * 8;
            }
            this.oldMouseState = Mouse.GetState();
            if (this.isActive && !this.informationUp && this.starIcon != null)
            {
                if (this.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
                {
                    this.starIcon.sourceRect.X = 294;
                }
                else
                {
                    this.starIcon.sourceRect.X = 310;
                }
            }
            if (this.isActive && this.starIcon != null && !this.informationUp && (this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.starIcon.containsPoint(this.oldMouseState.X, this.oldMouseState.Y))
            {
                this.newCraftingRecipes.Clear();
                this.extraInfoForLevel.Clear();
                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.playSound("bigSelect");
                this.informationUp = true;
                this.isProfessionChooser = false;
                this.currentLevel = Magic.newMagicLevels.First();//Game1.player.newLevels.First<Point>().Y;
                this.title = string.Concat(new object[]
				{
					"Level ",
					this.currentLevel,
					" Cooking"
				});
                this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentLevel);
                sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
                /*if ((this.currentLevel == 5 || this.currentLevel == 10))
                {
                    this.professionsToChoose.Clear();
                    this.isProfessionChooser = true;
                    if (this.currentLevel == 5)
                    {
                        this.professionsToChoose.Add(Mod.PROFESSION_SELLPRICE);
                        this.professionsToChoose.Add(Mod.PROFESSION_BUFFTIME);
                    }
                    else if (Game1.player.professions.Contains(Mod.PROFESSION_SELLPRICE))
                    {
                        this.professionsToChoose.Add(Mod.PROFESSION_CONSERVATION);
                        this.professionsToChoose.Add(Mod.PROFESSION_SILVER);
                    }
                    else
                    {
                        this.professionsToChoose.Add(Mod.PROFESSION_BUFFLEVEL);
                        this.professionsToChoose.Add(Mod.PROFESSION_BUFFPLAIN);
                    }
                    this.leftProfessionDescription = CookingLevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
                    this.rightProfessionDescription = CookingLevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
                }*/
                int num = 0;
                this.height = num + Game1.tileSize * 4 + this.extraInfoForLevel.Count<string>() * Game1.tileSize * 3 / 4;
                Game1.player.freezePause = 100;
            }
            if (this.isActive && this.informationUp)
            {
                Game1.player.completelyStopAnimatingOrDoingAction();
                if (this.okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.isProfessionChooser)
                {
                    this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
                    if ((this.oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && this.readyToClose())
                    {
                        this.getLevelPerk(this.currentLevel);
                        this.isActive = false;
                        this.informationUp = false;
                    }
                }
                else
                {
                    this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
                }
                Game1.player.freezePause = 100;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
        }

        public void getLevelPerk(int level)
        {
        }

        public override void draw(SpriteBatch b)
        {
            if (this.timerBeforeStart > 0)
            {
                return;
            }
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            foreach (TemporaryAnimatedSprite current in this.littleStars)
            {
                current.draw(b, false, 0, 0);
            }
            b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - 58 * Game1.pixelZoom / 2), (float)(this.yPositionOnScreen - Game1.tileSize / 2 + Game1.pixelZoom * 3)), new Rectangle?(new Rectangle(363, 87, 58, 22)), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1f);
            if (!this.informationUp && this.isActive && this.starIcon != null)
            {
                this.starIcon.draw(b);
                return;
            }
            if (this.informationUp)
            {
                if (this.isProfessionChooser)
                {
                    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false);
                    base.drawHorizontalPartition(b, this.yPositionOnScreen + Game1.tileSize * 3, false);
                    base.drawVerticalIntersectingPartition(b, this.xPositionOnScreen + this.width / 2 - Game1.tileSize / 2, this.yPositionOnScreen + Game1.tileSize * 3);
                    Utility.drawWithShadow(b, Magic.expIcon, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 0.88f, -1, -1, 0.35f);
                    b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), Game1.textColor);
                    Utility.drawWithShadow(b, Magic.expIcon, new Vector2((float)(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - Game1.tileSize), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 0.88f, -1, -1, 0.35f);
                    b.DrawString(Game1.smallFont, "Choose a profession:", new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString("Choose a profession:").X / 2f, (float)(this.yPositionOnScreen + Game1.tileSize + IClickableMenu.spaceToClearTopBorder)), Game1.textColor);
                    b.DrawString(Game1.dialogueFont, this.leftProfessionDescription[0], new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + Game1.tileSize / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2)), this.leftProfessionColor);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2 - Game1.tileSize * 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2 - Game1.tileSize / 4)), new Rectangle?(new Rectangle(this.professionsToChoose[0] % 6 * 16, 624 + this.professionsToChoose[0] / 6 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    for (int i = 1; i < this.leftProfessionDescription.Count<string>(); i++)
                    {
                        b.DrawString(Game1.smallFont, Game1.parseText(this.leftProfessionDescription[i], Game1.smallFont, this.width / 2 - 64), new Vector2((float)(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + Game1.tileSize / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2 + 8 + Game1.tileSize * (i + 1))), this.leftProfessionColor);
                    }
                    b.DrawString(Game1.dialogueFont, this.rightProfessionDescription[0], new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2)), this.rightProfessionColor);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width - Game1.tileSize * 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2 - Game1.tileSize / 4)), new Rectangle?(new Rectangle(this.professionsToChoose[1] % 6 * 16, 624 + this.professionsToChoose[1] / 6 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    for (int j = 1; j < this.rightProfessionDescription.Count<string>(); j++)
                    {
                        b.DrawString(Game1.smallFont, Game1.parseText(this.rightProfessionDescription[j], Game1.smallFont, this.width / 2 - 48), new Vector2((float)(-4 + this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 2 + 8 + Game1.tileSize * (j + 1))), this.rightProfessionColor);
                    }
                }
                else
                {
                    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false);
                    Utility.drawWithShadow(b, Magic.expIcon, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 0.88f, -1, -1, 0.35f);
                    b.DrawString(Game1.dialogueFont, this.title, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.dialogueFont.MeasureString(this.title).X / 2f, (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), Game1.textColor);
                    Utility.drawWithShadow(b, Magic.expIcon, new Vector2((float)(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - Game1.tileSize), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize / 4)), this.sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, false, 0.88f, -1, -1, 0.35f);
                    int num = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + Game1.tileSize * 5 / 4;
                    foreach (string current2 in this.extraInfoForLevel)
                    {
                        b.DrawString(Game1.smallFont, current2, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString(current2).X / 2f, (float)num), Game1.textColor);
                        num += Game1.tileSize * 3 / 4;
                    }
                    foreach (CraftingRecipe current3 in this.newCraftingRecipes)
                    {
                        b.DrawString(Game1.smallFont, "New " + (current3.isCookingRecipe ? "cooking" : "crafting") + " recipe: " + current3.name, new Vector2((float)(this.xPositionOnScreen + this.width / 2) - Game1.smallFont.MeasureString("New crafting recipe: " + current3.name).X / 2f - (float)Game1.tileSize, (float)(num + (current3.bigCraftable ? (Game1.tileSize * 3 / 5) : (Game1.tileSize / 5)))), Game1.textColor);
                        current3.drawMenuView(b, (int)((float)(this.xPositionOnScreen + this.width / 2) + Game1.smallFont.MeasureString("New crafting recipe: " + current3.name).X / 2f - (float)(Game1.tileSize * 3 / 4)), num - Game1.tileSize / 4, 0.88f, true);
                        num += (current3.bigCraftable ? (Game1.tileSize * 2) : Game1.tileSize) + Game1.pixelZoom * 2;
                    }
                    this.okButton.draw(b);
                }
                base.drawMouse(b);
            }
        }
    }
}
