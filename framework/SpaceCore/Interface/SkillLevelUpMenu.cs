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
using static SpaceCore.Skills;

namespace SpaceCore.Interface
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.CopiedFromGameCode)]
    public class SkillLevelUpMenu : IClickableMenu
    {
        private Color leftProfessionColor = Game1.textColor;
        private Color rightProfessionColor = Game1.textColor;
        private readonly List<CraftingRecipe> newCraftingRecipes = new();
        private List<string> extraInfoForLevel = new();
        private List<string> leftProfessionDescription = new();
        private List<string> rightProfessionDescription = new();
        private readonly List<int> professionsToChoose = new();
        private readonly List<TemporaryAnimatedSprite> littleStars = new();
        public const int region_okButton = 101;
        public const int region_leftProfession = 102;
        public const int region_rightProfession = 103;
        public const int basewidth = 768;
        public const int baseheight = 512;
        public bool informationUp;
        public bool isActive;
        public bool isProfessionChooser;
        public bool hasUpdatedProfessions;
        private int currentLevel;
        private string currentSkill; // Used to be int
        private int timerBeforeStart;
        private MouseState oldMouseState;
        public ClickableTextureComponent starIcon;
        public ClickableTextureComponent okButton;
        public ClickableComponent leftProfession;
        public ClickableComponent rightProfession;
        private string title;
        public bool hasMovedSelection;

        private Skills.Skill.ProfessionPair profPair;

        /*
        public LevelUpMenu()
            : base(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 256, 768, 512, false)
        {
            this.width = 768;
            this.height = 512;
            ClickableTextureComponent textureComponent = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 1f, false);
            textureComponent.myID = 101;
            this.okButton = textureComponent;
            if (!Game1.options.SnappyMenus)
                return;
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }*/

        // Constructor changed: int skill -> string skillName
        public SkillLevelUpMenu(string skillName, int level)
            : base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
        {

            Game1.player.team.endOfNightStatus.UpdateState("level");
            this.timerBeforeStart = 250;
            this.isActive = true;
            this.width = 960;
            this.height = 512;
            this.okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
            {
                myID = 101
            };
            this.newCraftingRecipes.Clear();
            this.extraInfoForLevel.Clear();
            Game1.player.completelyStopAnimatingOrDoingAction();
            this.informationUp = true;
            this.isProfessionChooser = false;
            this.currentLevel = level;
            this.currentSkill = skillName;
            /*if (level == 10)
            {
                Game1.getSteamAchievement("Achievement_SingularTalent");
                if ((int)((NetFieldBase<int, NetInt>)Game1.player.farmingLevel) == 10 && (int)((NetFieldBase<int, NetInt>)Game1.player.miningLevel) == 10 && ((int)((NetFieldBase<int, NetInt>)Game1.player.fishingLevel) == 10 && (int)((NetFieldBase<int, NetInt>)Game1.player.foragingLevel) == 10) && (int)((NetFieldBase<int, NetInt>)Game1.player.combatLevel) == 10)
                    Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
                if (skill == 0)
                    Game1.addMailForTomorrow("marnieAutoGrabber", false, false);
            }
            */
            this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", this.currentLevel, Skills.SkillsByName[this.currentSkill].GetName());
            this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentSkill, this.currentLevel);
            /*
            switch (this.currentSkill)
            {
                case 0:
                    this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
                    break;
                case 1:
                    this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
                    break;
                case 2:
                    this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
                    break;
                case 3:
                    this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
                    break;
                case 4:
                    this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
                    break;
                case 5:
                    this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
                    break;
            }
            */
            var skill = Skills.SkillsByName[skillName];
            this.profPair = null;
            foreach (var pair in skill.ProfessionsForLevels)
                if (pair.Level == this.currentLevel && (pair.Requires == null || Game1.player.professions.Contains(pair.Requires.GetVanillaId())))
                {
                    this.profPair = pair;
                    break;
                }
            if (this.profPair != null)
            {
                this.professionsToChoose.Clear();
                this.isProfessionChooser = true;
            }
            /*
            if ((this.currentLevel == 5 || this.currentLevel == 10) && this.currentSkill != 5)
            {
                this.professionsToChoose.Clear();
                this.isProfessionChooser = true;
            }
            */
            /*
            int num = 0;
            foreach (KeyValuePair<string, string> craftingRecipe in CraftingRecipe.craftingRecipes)
            {
                string str = craftingRecipe.Value.Split('/')[4];
                if (str.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && str.Contains(string.Concat((object)this.currentLevel)))
                {
                    this.newCraftingRecipes.Add(new CraftingRecipe(craftingRecipe.Key, false));
                    if (!Game1.player.craftingRecipes.ContainsKey(craftingRecipe.Key))
                        Game1.player.craftingRecipes.Add(craftingRecipe.Key, 0);
                    num += this.newCraftingRecipes.Last<CraftingRecipe>().bigCraftable ? 128 : 64;
                }
            }
            foreach (KeyValuePair<string, string> cookingRecipe in CraftingRecipe.cookingRecipes)
            {
                string str = cookingRecipe.Value.Split('/')[3];
                if (str.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && str.Contains(string.Concat((object)this.currentLevel)))
                {
                    this.newCraftingRecipes.Add(new CraftingRecipe(cookingRecipe.Key, true));
                    if (!Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Key))
                    {
                        Game1.player.cookingRecipes.Add(cookingRecipe.Key, 0);
                        if (!Game1.player.hasOrWillReceiveMail("robinKitchenLetter"))
                            Game1.mailbox.Add("robinKitchenLetter");
                    }
                    num += this.newCraftingRecipes.Last<CraftingRecipe>().bigCraftable ? 128 : 64;
                }
            }
            this.height = num + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
            */

            //Get Crafting Recipes learned at this level
            List<CraftingRecipe> levelUpCraftingRecipes = GetCraftingRecipesForLevel(this.currentSkill, this.currentLevel);

            if (levelUpCraftingRecipes is not null && levelUpCraftingRecipes.Count > 0)
            {
                foreach (CraftingRecipe recipe in levelUpCraftingRecipes.Where(r => !Game1.player.craftingRecipes.ContainsKey(r.name)))
                {
                    Game1.player.craftingRecipes[recipe.name] = 0;
                }
            }

            //Get Cooking Recipes learned at this level
            List<CraftingRecipe> levelUpCookingRecipes = GetCookingRecipesForLevel(this.currentSkill, this.currentLevel);

            if (levelUpCookingRecipes is not null && levelUpCookingRecipes.Count > 0)
            {
                foreach (CraftingRecipe recipe in levelUpCookingRecipes.Where(r => !Game1.player.cookingRecipes.ContainsKey(r.name)))
                {
                    Game1.player.cookingRecipes[recipe.name] = 0;
                }
            }

            levelUpCraftingRecipes.AddRange(levelUpCookingRecipes);
            this.newCraftingRecipes = levelUpCraftingRecipes;
            // Adjust menu to fit if necessary
            const int defaultMenuHeightInRecipes = 4;
            int menuHeightInRecipes = levelUpCraftingRecipes.Count + levelUpCraftingRecipes.Count(recipe => recipe.bigCraftable);
            if (menuHeightInRecipes >= defaultMenuHeightInRecipes)
            {
                this.height += (menuHeightInRecipes - defaultMenuHeightInRecipes) * StardewValley.Object.spriteSheetTileSize * Game1.pixelZoom;
            }


            Game1.player.freezePause = 100;
            this.gameWindowSizeChanged(Rectangle.Empty, Rectangle.Empty);
            if (this.isProfessionChooser)
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
            this.RepositionOkButton();


        }
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }
        public virtual void RepositionOkButton()
        {
            this.okButton.bounds = new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64);
            if (this.okButton.bounds.Right > Game1.uiViewport.Width)
            {
                this.okButton.bounds.X = Game1.uiViewport.Width - 64;
            }
            if (this.okButton.bounds.Bottom > Game1.uiViewport.Height)
            {
                this.okButton.bounds.Y = Game1.uiViewport.Height - 64;
            }
        }

        public bool CanReceiveInput()
        {
            return this.informationUp && this.timerBeforeStart <= 0;
        }

        public override void snapToDefaultClickableComponent()
        {
            if (this.isProfessionChooser)
            {
                this.currentlySnappedComponent = this.getComponentWithID(103);
                Game1.setMousePosition(this.xPositionOnScreen + this.width + 64, this.yPositionOnScreen + this.height + 64);
            }
            else
            {
                this.currentlySnappedComponent = this.getComponentWithID(101);
                this.snapCursorToCurrentSnappedComponent();
            }
        }

        public override void applyMovementKey(int direction)
        {
            if (!this.CanReceiveInput())
                return;
            if (direction is 3 or 1)
                this.hasMovedSelection = true;
            base.applyMovementKey(direction);
        }


        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            this.okButton.bounds = new Rectangle(this.xPositionOnScreen + this.width + 4, this.yPositionOnScreen + this.height - 64 - IClickableMenu.borderWidth, 64, 64);
            this.RepositionOkButton();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
        }

        public List<string> getExtraInfoForLevel(string whichSkill, int whichLevel)
        {
            return Skills.SkillsByName[whichSkill].GetExtraLevelUpInfo(whichLevel);
            /*
            List<string> stringList = new List<string>();
            switch (whichSkill)
            {
                case 0:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming1"));
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming2"));
                    break;
                case 1:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Fishing"));
                    break;
                case 2:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging1"));
                    if (whichLevel == 1)
                        stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging2"));
                    if (whichLevel == 4 || whichLevel == 8)
                    {
                        stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging3"));
                        break;
                    }
                    break;
                case 3:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Mining"));
                    break;
                case 4:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Combat"));
                    break;
                case 5:
                    stringList.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Luck"));
                    break;
            }
            return stringList;
            */
        }

        public static List<CraftingRecipe> GetCraftingRecipesForLevel(string whichSkill, int level)
        {
            // Level used for professions, no new recipes added
            List<CraftingRecipe> newRecipes = [];
            if (level % 5 == 0)
            {
                return newRecipes;
            }

            foreach (KeyValuePair<string, string> recipePair in CraftingRecipe.craftingRecipes)
            {
                string conditions = ArgUtility.Get(recipePair.Value.Split('/'), 4, "");
                if (!conditions.Contains(whichSkill) || !conditions.Contains(level.ToString()))
                {
                    continue;
                }

                CraftingRecipe recipe = new(recipePair.Key, isCookingRecipe: false);
                newRecipes.Add(recipe);
                Game1.player.craftingRecipes.TryAdd(recipePair.Key, 0);
            }
            return newRecipes;
        }

        public static List<CraftingRecipe> GetCookingRecipesForLevel(string whichSkill, int level)
        {
            // Level used for professions, no new recipes added
            List<CraftingRecipe> newRecipes = [];
            if (level % 5 == 0)
            {
                return newRecipes;
            }
            foreach (KeyValuePair<string, string> recipePair in CraftingRecipe.cookingRecipes)
            {
                string conditions = ArgUtility.Get(recipePair.Value.Split('/'), 3, "");
                if (!conditions.Contains(whichSkill) || !conditions.Contains(level.ToString()))
                {
                    continue;
                }

                CraftingRecipe recipe = new(recipePair.Key, isCookingRecipe: true);
                newRecipes.Add(recipe);
                if (Game1.player.cookingRecipes.TryAdd(recipePair.Key, 0) &&
                    !Game1.player.hasOrWillReceiveMail("robinKitchenLetter"))
                {
                    Game1.mailbox.Add("robinKitchenLetter");
                }

            }

            return newRecipes;
        }

        private static void addProfessionDescriptions(List<string> descriptions, string professionName)
        {
            foreach (var skill in Skills.SkillsByName)
            {
                foreach (var prof in skill.Value.Professions)
                {
                    if (prof.Id == professionName)
                    {
                        descriptions.AddRange(prof.GetDescription().Split('\n'));
                        break;
                    }
                }
            }
            /*
            descriptions.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + professionName));
            descriptions.AddRange((IEnumerable<string>)Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + professionName).Split('\n'));
            */
        }

        private static string getProfessionName(int whichProfession)
        {
            foreach (var skill in Skills.SkillsByName)
            {
                foreach (var prof in skill.Value.Professions)
                {
                    if (prof.GetVanillaId() == whichProfession)
                    {
                        return prof.GetName();
                    }
                }
            }
            return "n/a";
            /*
            switch (whichProfession)
            {
                case 0:
                    return "Rancher";
                case 1:
                    return "Tiller";
                case 2:
                    return "Coopmaster";
                case 3:
                    return "Shepherd";
                case 4:
                    return "Artisan";
                case 5:
                    return "Agriculturist";
                case 6:
                    return "Fisher";
                case 7:
                    return "Trapper";
                case 8:
                    return "Angler";
                case 9:
                    return "Pirate";
                case 10:
                    return "Mariner";
                case 11:
                    return "Luremaster";
                case 12:
                    return "Forester";
                case 13:
                    return "Gatherer";
                case 14:
                    return "Lumberjack";
                case 15:
                    return "Tapper";
                case 16:
                    return "Botanist";
                case 17:
                    return "Tracker";
                case 18:
                    return "Miner";
                case 19:
                    return "Geologist";
                case 20:
                    return "Blacksmith";
                case 21:
                    return "Prospector";
                case 22:
                    return "Excavator";
                case 23:
                    return "Gemologist";
                case 24:
                    return "Fighter";
                case 25:
                    return "Scout";
                case 26:
                    return "Brute";
                case 27:
                    return "Defender";
                case 28:
                    return "Acrobat";
                default:
                    return "Desperado";
            }*/
        }

        public static List<string> getProfessionDescription(int whichProfession)
        {
            List<string> descriptions = new List<string>();
            SkillLevelUpMenu.addProfessionDescriptions(descriptions, SkillLevelUpMenu.getProfessionName(whichProfession));
            return descriptions;
            /* 
            List<string> descriptions = new List<string>();
            LevelUpMenu.addProfessionDescriptions(descriptions, LevelUpMenu.getProfessionName(whichProfession));
            return descriptions;
            */
        }

        public static string getProfessionTitleFromNumber(int whichProfession)
        {
            return SkillLevelUpMenu.getProfessionName(whichProfession);
            //return Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + LevelUpMenu.getProfessionName(whichProfession));
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void performHoverAction(int x, int y)
        {
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
            if (b != Buttons.Start && b != Buttons.B || (this.isProfessionChooser || !this.isActive))
                return;
            this.okButtonClicked();
        }

        public void getImmediateProfessionPerk(int whichProfession)
        {
            var skill = Skills.SkillsByName[this.currentSkill];
            foreach (var prof in skill.Professions)
            {
                if (prof.GetVanillaId() == whichProfession)
                {
                    prof.DoImmediateProfessionPerk();
                }
            }
            /*
            if (whichProfession != 24)
            {
                if (whichProfession != 27)
                    return;
                Game1.player.maxHealth += 25;
            }
            else
                Game1.player.maxHealth += 15;
            */

            //This was missing when compared to vanilla perks. When ever you level up in a skill,
            // you always gained maxd stamina and health, even if you exhaust yourself.


            Game1.player.health = Game1.player.maxHealth;
            Game1.player.stamina = Game1.player.MaxStamina;
        }

        public override void update(GameTime time)
        {
            var Game1input = Game1.input;

            if (!this.isActive)
            {
                this.exitThisMenu();
            }
            else
            {
                if (/*this.isProfessionChooser &&*/ !this.hasUpdatedProfessions)
                {
                    var skill = Skills.SkillsByName[this.currentSkill];
                    this.profPair = null;
                    foreach (var pair in skill.ProfessionsForLevels)
                        if (pair.Level == this.currentLevel && (pair.Requires == null || Game1.player.professions.Contains(pair.Requires.GetVanillaId())))
                        {
                            this.profPair = pair;
                            break;
                        }
                    if (this.profPair != null)
                    {
                        this.isProfessionChooser = true;
                        this.professionsToChoose.Add(this.profPair.First.GetVanillaId());
                        this.professionsToChoose.Add(this.profPair.Second.GetVanillaId());
                    }
                    /*
                    if (this.currentLevel == 5)
                    {
                        this.professionsToChoose.Add(this.currentSkill * 6);
                        this.professionsToChoose.Add(this.currentSkill * 6 + 1);
                    }
                    else if (Game1.player.professions.Contains(this.currentSkill * 6))
                    {
                        this.professionsToChoose.Add(this.currentSkill * 6 + 2);
                        this.professionsToChoose.Add(this.currentSkill * 6 + 3);
                    }
                    else
                    {
                        this.professionsToChoose.Add(this.currentSkill * 6 + 4);
                        this.professionsToChoose.Add(this.currentSkill * 6 + 5);
                    }
                    */
                    if (this.profPair != null)
                    {
                        var la = new List<string>(new[] { this.profPair.First.GetName() });
                        la.AddRange(this.profPair.First.GetDescription().Split('\n'));
                        var ra = new List<string>(new[] { this.profPair.Second.GetName() });
                        ra.AddRange(this.profPair.Second.GetDescription().Split('\n'));
                        this.leftProfessionDescription = la;// LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
                        this.rightProfessionDescription = ra;//LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
                    }
                    this.hasUpdatedProfessions = true;
                }
                for (int index = this.littleStars.Count - 1; index >= 0; --index)
                {
                    if (this.littleStars[index].update(time))
                        this.littleStars.RemoveAt(index);
                }
                if (Game1.random.NextDouble() < 0.03)
                {
                    Vector2 position = new Vector2(
                        x: 0.0f,
                        y: Game1.random.Next(this.yPositionOnScreen - 128, this.yPositionOnScreen - 4) / 20 * 4 * 5 + 32
                    )
                    {
                        X = Game1.random.NextDouble() >= 0.5
                            ? Game1.random.Next(this.xPositionOnScreen + this.width / 2 + 116, this.xPositionOnScreen + this.width - 160)
                            : Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 228, this.xPositionOnScreen + this.width / 2 - 132)
                    };
                    if (position.Y < (double)(this.yPositionOnScreen - 64 - 8))
                        position.X = Game1.random.Next(this.xPositionOnScreen + this.width / 2 - 116, this.xPositionOnScreen + this.width / 2 + 116);
                    position.X = (float)(position.X / 20.0 * 4.0 * 5.0);
                    this.littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, false, false, 1f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
                    {
                        local = true
                    });
                }
                if (this.timerBeforeStart > 0)
                {
                    this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
                    if (this.timerBeforeStart > 0 || !Game1.options.SnappyMenus)
                        return;
                    this.populateClickableComponentList();
                    this.snapToDefaultClickableComponent();
                }
                else
                {
                    if (this.isActive && this.isProfessionChooser)
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
                                    this.getImmediateProfessionPerk(this.professionsToChoose[0]);
                                    this.isActive = false;
                                    this.informationUp = false;
                                    this.isProfessionChooser = false;
                                }
                            }
                            else if (Game1.getMouseX() > this.xPositionOnScreen + this.width / 2 && Game1.getMouseX() < this.xPositionOnScreen + this.width)
                            {
                                this.rightProfessionColor = Color.Green;
                                if (Game1.didPlayerJustLeftClick() && this.readyToClose())
                                {
                                    Game1.player.professions.Add(this.professionsToChoose[1]);
                                    this.getImmediateProfessionPerk(this.professionsToChoose[1]);
                                    this.isActive = false;
                                    this.informationUp = false;
                                    this.isProfessionChooser = false;
                                }
                            }
                        }
                        this.height = 512;
                    }
                    this.oldMouseState = Game1input.GetMouseState();
                    if (this.isActive && !this.informationUp && this.starIcon != null)
                        this.starIcon.sourceRect.X = !this.starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 310 : 294;
                    if (this.isActive && this.starIcon != null && !this.informationUp && (this.oldMouseState.LeftButton == ButtonState.Pressed || Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A)) && this.starIcon.containsPoint(this.oldMouseState.X, this.oldMouseState.Y))
                    {
                        this.newCraftingRecipes.Clear();
                        this.extraInfoForLevel.Clear();
                        Game1.player.completelyStopAnimatingOrDoingAction();
                        Game1.playSound("bigSelect");
                        this.informationUp = true;
                        this.isProfessionChooser = false;
                        this.currentLevel = Skills.NewLevels.First().Value;
                        this.currentSkill = Skills.NewLevels.First().Key;
                        /*
                        this.currentLevel = Game1.player.newLevels.First<Point>().Y;
                        this.currentSkill = Game1.player.newLevels.First<Point>().X;
                        */
                        this.title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", this.currentLevel, Skills.SkillsByName[this.currentSkill].GetName());
                        this.extraInfoForLevel = this.getExtraInfoForLevel(this.currentSkill, this.currentLevel);
                        /*switch (this.currentSkill)
                        {
                            case 0:
                                this.sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
                                break;
                            case 1:
                                this.sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
                                break;
                            case 2:
                                this.sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
                                break;
                            case 3:
                                this.sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
                                break;
                            case 4:
                                this.sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
                                break;
                            case 5:
                                this.sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
                                break;
                        }*/
                        var skill = Skills.SkillsByName[this.currentSkill];
                        this.profPair = null;
                        foreach (var pair in skill.ProfessionsForLevels)
                            if (pair.Level == this.currentLevel && Game1.player.professions.Contains(pair.Requires.GetVanillaId()))
                            {
                                this.profPair = pair;
                                break;
                            }
                        if (this.profPair != null)
                        {
                            this.professionsToChoose.Clear();
                            this.isProfessionChooser = true;
                            this.professionsToChoose.Add(this.profPair.First.GetVanillaId());
                            this.professionsToChoose.Add(this.profPair.Second.GetVanillaId());
                        }
                        /*
                        if ((this.currentLevel == 5 || this.currentLevel == 10) && this.currentSkill != 5)
                        {
                            this.professionsToChoose.Clear();
                            this.isProfessionChooser = true;
                            if (this.currentLevel == 5)
                            {
                                this.professionsToChoose.Add(this.currentSkill * 6);
                                this.professionsToChoose.Add(this.currentSkill * 6 + 1);
                            }
                            else if (Game1.player.professions.Contains(this.currentSkill * 6))
                            {
                                this.professionsToChoose.Add(this.currentSkill * 6 + 2);
                                this.professionsToChoose.Add(this.currentSkill * 6 + 3);
                            }
                            else
                            {
                                this.professionsToChoose.Add(this.currentSkill * 6 + 4);
                                this.professionsToChoose.Add(this.currentSkill * 6 + 5);
                            }
                            this.leftProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[0]);
                            this.rightProfessionDescription = LevelUpMenu.getProfessionDescription(this.professionsToChoose[1]);
                        }
                        */
                        /*
                        int num = 0;
                        foreach (KeyValuePair<string, string> craftingRecipe in CraftingRecipe.craftingRecipes)
                        {
                            string str = craftingRecipe.Value.Split('/')[4];
                            if (str.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && str.Contains(string.Concat((object)this.currentLevel)))
                            {
                                this.newCraftingRecipes.Add(new CraftingRecipe(craftingRecipe.Key, false));
                                if (!Game1.player.craftingRecipes.ContainsKey(craftingRecipe.Key))
                                    Game1.player.craftingRecipes.Add(craftingRecipe.Key, 0);
                                num += this.newCraftingRecipes.Last<CraftingRecipe>().bigCraftable ? 128 : 64;
                            }
                        }
                        foreach (KeyValuePair<string, string> cookingRecipe in CraftingRecipe.cookingRecipes)
                        {
                            string str = cookingRecipe.Value.Split('/')[3];
                            if (str.Contains(Farmer.getSkillNameFromIndex(this.currentSkill)) && str.Contains(string.Concat((object)this.currentLevel)))
                            {
                                this.newCraftingRecipes.Add(new CraftingRecipe(cookingRecipe.Key, true));
                                if (!Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Key))
                                    Game1.player.cookingRecipes.Add(cookingRecipe.Key, 0);
                                num += this.newCraftingRecipes.Last<CraftingRecipe>().bigCraftable ? 128 : 64;
                            }
                        }
                        this.height = num + 256 + this.extraInfoForLevel.Count * 64 * 3 / 4;
                        */
                        Game1.player.freezePause = 100;
                    }
                    if (!this.isActive || !this.informationUp)
                        return;
                    Game1.player.completelyStopAnimatingOrDoingAction();
                    if (this.okButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.isProfessionChooser)
                    {
                        this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
                        if (Game1.didPlayerJustLeftClick() && this.readyToClose())
                            this.okButtonClicked();
                    }
                    else
                        this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
                    Game1.player.freezePause = 100;
                }
            }
        }

        public void okButtonClicked()
        {
            this.getLevelPerk(this.currentSkill, this.currentLevel);
            this.isActive = false;
            this.informationUp = false;
        }

        public override void receiveKeyPress(Keys key)
        {
            if (!Game1.options.SnappyMenus || (Game1.options.doesInputListContain(Game1.options.cancelButton, key) || Game1.options.doesInputListContain(Game1.options.menuButton, key)) && this.isProfessionChooser)
                return;
            base.receiveKeyPress(key);
        }

        public void getLevelPerk(string skill, int level)
        {
            Skills.SkillsByName[skill].DoLevelPerk(level);
            /*
            switch (skill)
            {
                case 1:
                    switch (level)
                    {
                        case 2:
                            if (!Game1.player.hasOrWillReceiveMail("fishing2"))
                            {
                                Game1.addMailForTomorrow("fishing2", false, false);
                                break;
                            }
                            break;
                        case 6:
                            if (!Game1.player.hasOrWillReceiveMail("fishing6"))
                            {
                                Game1.addMailForTomorrow("fishing6", false, false);
                                break;
                            }
                            break;
                    }
                    break;
                case 4:
                    Game1.player.maxHealth += 5;
                    break;
            }
            Game1.player.health = Game1.player.maxHealth;
            Game1.player.Stamina = (float)(int)((NetFieldBase<int, NetInt>)Game1.player.maxStamina);
            */
        }

        public override void draw(SpriteBatch b)
        {
            if (this.timerBeforeStart > 0)
                return;
            b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);
            foreach (TemporaryAnimatedSprite littleStar in this.littleStars)
                littleStar.draw(b);
            b.Draw(Game1.mouseCursors, new Vector2(this.xPositionOnScreen + this.width / 2 - 116, this.yPositionOnScreen - 32 + 12), new Rectangle(363, 87, 58, 22), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            if (!this.informationUp && this.isActive && this.starIcon != null)
            {
                this.starIcon.draw(b);
            }
            else
            {
                if (!this.informationUp)
                    return;
                if (this.isProfessionChooser)
                {
                    if (!this.professionsToChoose.Any())
                        return;
                    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
                    this.drawHorizontalPartition(b, this.yPositionOnScreen + 192);
                    this.drawVerticalIntersectingPartition(b, this.xPositionOnScreen + this.width / 2 - 32, this.yPositionOnScreen + 192);
                    if (Skills.SkillsByName[this.currentSkill].Icon != null)
                        Utility.drawWithShadow(b, Skills.SkillsByName[this.currentSkill].Icon, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Skills.SkillsByName[this.currentSkill].Icon.Bounds, Color.White, 0.0f, Vector2.Zero, 4f, false, 0.88f);
                    b.DrawString(Game1.dialogueFont, this.title, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.dialogueFont.MeasureString(this.title).X / 2f, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
                    if (Skills.SkillsByName[this.currentSkill].Icon != null)
                        Utility.drawWithShadow(b, Skills.SkillsByName[this.currentSkill].Icon, new Vector2(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Skills.SkillsByName[this.currentSkill].Icon.Bounds, Color.White, 0.0f, Vector2.Zero, 4f, false, 0.88f);
                    string text = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
                    b.DrawString(Game1.smallFont, text, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.smallFont.MeasureString(text).X / 2f, this.yPositionOnScreen + 64 + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
                    b.DrawString(Game1.dialogueFont, this.leftProfessionDescription[0], new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.leftProfessionColor);
                    if (this.profPair.First.Icon != null)
                        b.Draw(this.profPair.First.Icon, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2 - 112, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(0, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    //b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2 - 112), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16)), new Rectangle(this.professionsToChoose[0] % 6 * 16, 624 + this.professionsToChoose[0] / 6 * 16, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    for (int index = 1; index < this.leftProfessionDescription.Count; ++index)
                        b.DrawString(Game1.smallFont, Game1.parseText(this.leftProfessionDescription[index], Game1.smallFont, this.width / 2 - 64), new Vector2(this.xPositionOnScreen - 4 + IClickableMenu.spaceToClearSideBorder + 32, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (index + 1)), this.leftProfessionColor);
                    b.DrawString(Game1.dialogueFont, this.rightProfessionDescription[0], new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160), this.rightProfessionColor);
                    if (this.profPair.Second.Icon != null)
                        b.Draw(this.profPair.Second.Icon, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width - 128, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(0, 0, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    //b.Draw(Game1.mouseCursors, new Vector2((float)(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + this.width - 128), (float)(this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 160 - 16)), new Rectangle(this.professionsToChoose[1] % 6 * 16, 624 + this.professionsToChoose[1] / 6 * 16, 16, 16), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    for (int index = 1; index < this.rightProfessionDescription.Count; ++index)
                        b.DrawString(Game1.smallFont, Game1.parseText(this.rightProfessionDescription[index], Game1.smallFont, this.width / 2 - 48), new Vector2(this.xPositionOnScreen - 4 + IClickableMenu.spaceToClearSideBorder + this.width / 2, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 128 + 8 + 64 * (index + 1)), this.rightProfessionColor);
                }
                else
                {
                    Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
                    if (Skills.SkillsByName[this.currentSkill].Icon != null)
                        Utility.drawWithShadow(b, Skills.SkillsByName[this.currentSkill].Icon, new Vector2(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Skills.SkillsByName[this.currentSkill].Icon.Bounds, Color.White, 0.0f, Vector2.Zero, 4f, false, 0.88f);
                    b.DrawString(Game1.dialogueFont, this.title, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.dialogueFont.MeasureString(this.title).X / 2f, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
                    if (Skills.SkillsByName[this.currentSkill].Icon != null)
                        Utility.drawWithShadow(b, Skills.SkillsByName[this.currentSkill].Icon, new Vector2(this.xPositionOnScreen + this.width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Skills.SkillsByName[this.currentSkill].Icon.Bounds, Color.White, 0.0f, Vector2.Zero, 4f, false, 0.88f);
                    int num = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 80;
                    foreach (string text in this.extraInfoForLevel)
                    {
                        b.DrawString(Game1.smallFont, text, new Vector2(this.xPositionOnScreen + this.width / 2 - Game1.smallFont.MeasureString(text).X / 2f, num), Game1.textColor);
                        num += 48;
                    }
                    foreach (CraftingRecipe newCraftingRecipe in this.newCraftingRecipes)
                    {
                        string str = Game1.content.LoadString("Strings\\UI:LearnedRecipe_" + (newCraftingRecipe.isCookingRecipe ? "cooking" : "crafting"));
                        string text = Game1.content.LoadString("Strings\\UI:LevelUp_NewRecipe", str, newCraftingRecipe.DisplayName);
                        b.DrawString(Game1.smallFont, text, new Vector2((float)(this.xPositionOnScreen + this.width / 2 - Game1.smallFont.MeasureString(text).X / 2.0 - 64.0), num + (newCraftingRecipe.bigCraftable ? 38 : 12)), Game1.textColor);
                        newCraftingRecipe.drawMenuView(b, (int)(this.xPositionOnScreen + this.width / 2 + Game1.smallFont.MeasureString(text).X / 2.0 - 48.0), num - 16);
                        num += (newCraftingRecipe.bigCraftable ? 128 : 64) + 8;
                    }
                    this.okButton.draw(b);
                }
                this.drawMouse(b);
            }
        }
    }
}
