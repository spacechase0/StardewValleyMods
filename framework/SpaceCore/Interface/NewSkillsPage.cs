using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.VanillaAssetExpansion;
using SpaceShared;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace SpaceCore.Interface
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.CopiedFromGameCode)]
    public class NewSkillsPage : IClickableMenu
    {
        public List<ClickableTextureComponent> skillBars = new();
        public List<ClickableTextureComponent> skillAreas = new();
        public ClickableComponent playerPanel;
        private readonly Rectangle skillsTabSource = new(16, 368, 16, 16);
        private int SkillTabRegionId => Game1.activeClickableMenu is GameMenu ? GameMenu.region_skillsTab : -1;

        private string hoverText = "";
        private string hoverTitle = "";
        private int professionImage = -1;
        private readonly int[] playerPanelFrames = new int[4]
        {
            0,
            1,
            0,
            2
        };
        public const string CustomSkillPrefix = "C";
        public const int SkillRegionStartId = 0;
        public const int SkillIdIncrement = 1;
        public const int SkillProfessionIncrement = 100;
        public const int PlayerPanelRegionId = 10275;
        private int playerPanelIndex;
        private int playerPanelTimer;

        private ClickableTextureComponent upButton;
        private ClickableTextureComponent downButton;
        private ClickableTextureComponent scrollBar;
        private Rectangle scrollBarRunner;
        private bool scrolling;
        private int skillScrollOffset;
        private Dictionary<int, int> skillAreaSkillIndexes = new();
        private Dictionary<int, int> skillBarSkillIndexes = new();

        private ClickableComponent lastSnappedComponent = null;

        private int timesClickedJunimo;

        private int GameSkillCount
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // allowing mods to patch this getter if needed (alternative luck skill implementations?)
            get => SpaceCore.Instance.Helper.ModRegistry.IsLoaded("spacechase0.LuckSkill") ? 6 : 5;
        }

        private string[] VisibleSkills { get; }

        private int AllSkillCount
            => this.GameSkillCount + VisibleSkills.Length;

        private int MaxSkillCountOnScreen
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // allowing mods to patch this getter if needed
            get => 5;
        }

        private int LastVisibleSkillIndex
            => this.skillScrollOffset + this.MaxSkillCountOnScreen - 1;

        private bool ShowsAllSkillsAtOnce
            => this.AllSkillCount <= this.MaxSkillCountOnScreen;

        public NewSkillsPage(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            // Player panel
            this.playerPanel = new ClickableComponent(bounds: new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 128, 192), name: null)
            {
                myID = NewSkillsPage.PlayerPanelRegionId
            };

            // Professions
            VisibleSkills = Skills.GetSkillList().Where(s => Skills.GetSkill(s).ShouldShowOnSkillsPage).ToArray();
            int drawX = 0;
            int addedX = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? this.xPositionOnScreen + width - 448 - 48 + 4 : this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 4;
            int drawY = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 12;
            int gameSkillCount = this.GameSkillCount;
            int leftSnapId = this.playerPanel.myID;

            // Professions for game skills
            for (int professionIndex = 1; professionIndex < 3; ++professionIndex)
            {
                for (int skillIndex = 0; skillIndex < gameSkillCount; ++skillIndex)
                {
                    bool drawRed = false;
                    int professionCheckLevel = professionIndex - 1 + (professionIndex * 4);
                    int whichProfession = -1;
                    string professionBlurb = "";
                    string professionTitle = "";

                    // case/index pairs for 1 and 3 are swapped to match internal skill order
                    switch (skillIndex)
                    {
                        case 0:
                            drawRed = Game1.player.FarmingLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(0, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                        case 1:
                            drawRed = Game1.player.MiningLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(3, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                        case 2:
                            drawRed = Game1.player.ForagingLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(2, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                        case 3:
                            drawRed = Game1.player.FishingLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(1, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                        case 4:
                            drawRed = Game1.player.CombatLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(4, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                        case 5:
                            drawRed = Game1.player.LuckLevel > professionCheckLevel;
                            whichProfession = Game1.player.getProfessionForSkill(5, professionCheckLevel + 1);
                            this.parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
                            break;
                    }
                    if (drawRed)
                    {
                        ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                            name: string.Concat(whichProfession),
                            bounds: new Rectangle(drawX + addedX - 4 + professionCheckLevel * 36, drawY + skillIndex * 56, 56, 36),
                            label: null, hoverText: professionBlurb,
                            texture: Game1.mouseCursors, sourceRect: new Rectangle(159, 338, 14, 9), scale: 4f,
                            drawShadow: true
                        )
                        {
                            myID = NewSkillsPage.SkillRegionStartId + (skillIndex * NewSkillsPage.SkillIdIncrement) + (professionIndex * NewSkillsPage.SkillProfessionIncrement)
                        };
                        textureComponent.leftNeighborID = textureComponent.myID - NewSkillsPage.SkillProfessionIncrement;
                        textureComponent.downNeighborID = skillIndex == gameSkillCount - 1 && VisibleSkills.Length == 0 ? -1 : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                        this.skillBars.Add(textureComponent);
                        this.skillBarSkillIndexes[textureComponent.myID] = skillIndex;
                    }
                }
                drawX += 24;
            }

            // Professions for custom skills
            drawX = 0;
            for (int professionIndex = 1; professionIndex < 3; ++professionIndex)
            {
                for (int skillIndex = 0; skillIndex < VisibleSkills.Length; ++skillIndex)
                {
                    int totalSkillIndex = gameSkillCount + skillIndex;
                    int professionLevel = professionIndex - 1 + (professionIndex * 4);
                    Skills.Skill skill = Skills.GetSkill(VisibleSkills[skillIndex]);
                    Skills.Skill.Profession profession = Skills.GetProfessionFor(skill, professionLevel + 1);// Game1.player.getProfessionForSkill(0, num4 + 1);
                    bool drawRed = Game1.player.GetCustomBuffedSkillLevel(skill) > professionLevel;
                    List<string> professionLines = new List<string>();
                    string professionBlurb = "";
                    string professionTitle = "";

                    if (profession != null)
                    {
                        professionLines.Add(profession.GetName());
                        professionLines.AddRange(profession.GetDescription().Split('\n'));
                    }
                    this.parseProfessionDescription(ref professionBlurb, ref professionTitle, professionLines);
                    if (drawRed && (professionLevel + 1) % 5 == 0 && profession != null)
                    {
                        List<ClickableTextureComponent> skillBars = this.skillBars;
                        ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                            name: NewSkillsPage.CustomSkillPrefix + profession.Id,
                            bounds: new Rectangle(drawX + addedX - 4 + (professionLevel * 36), drawY + (totalSkillIndex * 56), 56, 36),
                            label: null, hoverText: professionBlurb,
                            texture: Game1.mouseCursors, sourceRect: new Rectangle(159, 338, 14, 9), scale: 4f,
                            drawShadow: true
                        )
                        {
                            myID = (totalSkillIndex * NewSkillsPage.SkillIdIncrement) + (professionIndex * NewSkillsPage.SkillProfessionIncrement)
                        };
                        textureComponent.leftNeighborID = textureComponent.myID - NewSkillsPage.SkillProfessionIncrement;
                        textureComponent.rightNeighborID = professionIndex == 2 ? -1 : textureComponent.myID + NewSkillsPage.SkillProfessionIncrement;
                        textureComponent.downNeighborID = skillIndex == VisibleSkills.Length - 1 ? -1 : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                        skillBars.Add(textureComponent);
                        this.skillBarSkillIndexes[textureComponent.myID] = totalSkillIndex;
                    }
                }
                drawX += 24;
            }

            // For both vanilla and custom skills, allow skill bar navigation to skip missing professions when navigating up-and-down between skills
            // also permit navigation between columns of professions when reaching the ends of a column
            for (int index = 0; index < this.skillBars.Count; ++index)
            {
                if (index < this.skillBars.Count - 1 && Math.Abs(this.skillBars[index + 1].myID - this.skillBars[index].myID) < NewSkillsPage.SkillProfessionIncrement)
                {
                    this.skillBars[index].downNeighborID = this.skillBars[index + 1].myID;
                    this.skillBars[index + 1].upNeighborID = this.skillBars[index].myID;
                }
            }
            if (this.skillBars.Count > 1 && this.skillBars.Last().myID >= 2 * NewSkillsPage.SkillProfessionIncrement && this.skillBars[this.skillBars.Count - 2].myID >= 2 * NewSkillsPage.SkillProfessionIncrement)
                this.skillBars[this.skillBars.Count - 1].upNeighborID = this.skillBars[this.skillBars.Count - 2].myID;

            // Icons for vanilla skills
            for (int skillIndex = 0; skillIndex < gameSkillCount; ++skillIndex)
            {
                // actualIndex fetches skill data from the internal (actual skill index) sequence rather than the display sequence (skill index)
                int actualSkillIndex = skillIndex switch
                {
                    1 => 3,
                    3 => 1,
                    _ => skillIndex
                };
                string hoverText = "";
                switch (actualSkillIndex)
                {
                    case 0:
                        if (Game1.player.FarmingLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11592", Game1.player.FarmingLevel) + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11594", Game1.player.FarmingLevel);
                        }
                        break;
                    case 1:
                        if (Game1.player.FishingLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11598", Game1.player.FishingLevel);
                        }
                        break;
                    case 2:
                        if (Game1.player.ForagingLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11596", Game1.player.ForagingLevel);
                        }
                        break;
                    case 3:
                        if (Game1.player.MiningLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11600", Game1.player.MiningLevel);
                        }
                        break;
                    case 4:
                        if (Game1.player.CombatLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11602", Game1.player.CombatLevel * 5);
                        }
                        break;
                }

                ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                    name: string.Concat(actualSkillIndex),
                    bounds: new Rectangle(addedX - 128 - 48, drawY + (skillIndex * 56), 148, 36),
                    label: string.Concat(actualSkillIndex), hoverText,
                    texture: null, sourceRect: Rectangle.Empty, scale: 1f, drawShadow: false
                )
                {
                    myID = NewSkillsPage.SkillRegionStartId + (skillIndex * NewSkillsPage.SkillIdIncrement)
                };
                textureComponent.rightNeighborID = textureComponent.myID + NewSkillsPage.SkillProfessionIncrement;
                textureComponent.leftNeighborID = leftSnapId;
                textureComponent.downNeighborID = skillIndex == gameSkillCount - 1 && VisibleSkills.Length == 0 ? -1 : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                textureComponent.upNeighborID = skillIndex > 0 ? textureComponent.myID - NewSkillsPage.SkillProfessionIncrement : this.SkillTabRegionId;

                this.skillAreas.Add(textureComponent);
                this.skillAreaSkillIndexes[textureComponent.myID] = skillIndex;
            }

            // Icons for custom skills
            for (int skillIndex = 0; skillIndex < VisibleSkills.Length; ++skillIndex)
            {
                Skills.Skill skill = Skills.GetSkill(VisibleSkills[skillIndex]);
                int actualSkillIndex = gameSkillCount + skillIndex;
                string hoverText = "";
                if (Game1.player.GetCustomBuffedSkillLevel(skill) > 0)
                    hoverText = skill.GetSkillPageHoverText(Game1.player.GetCustomBuffedSkillLevel(skill));
                ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                    name: NewSkillsPage.CustomSkillPrefix + skill.GetName(),
                    bounds: new Rectangle(addedX - 128 - 48, drawY + (actualSkillIndex * 56), 148, 36),
                    label: string.Concat(actualSkillIndex), hoverText,
                    texture: null, sourceRect: Rectangle.Empty, scale: 1f, drawShadow: false
                )
                {
                    myID = NewSkillsPage.SkillRegionStartId + (actualSkillIndex * NewSkillsPage.SkillIdIncrement)
                };
                textureComponent.rightNeighborID = textureComponent.myID + NewSkillsPage.SkillProfessionIncrement;
                textureComponent.leftNeighborID = leftSnapId;
                textureComponent.downNeighborID = skillIndex == VisibleSkills.Length - 1 ? -1 : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                textureComponent.upNeighborID = textureComponent.myID - NewSkillsPage.SkillIdIncrement;

                this.skillAreas.Add(textureComponent);
                this.skillAreaSkillIndexes[textureComponent.myID] = actualSkillIndex;
            }

            // scrollbar

            /// 0.675 is as close as I can get to having the scroll bar end 2 pixels below the bottom most skill.
            /// 2 pixels below was chosen since the scroll bar goes 2 pixels above the top most skill.
            int shrunkHeight = (int)(height * 0.675);
            this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + shrunkHeight - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upButton.bounds.X + 12, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, this.scrollBar.bounds.Width, shrunkHeight - 128 - this.upButton.bounds.Height - 8);

            // Add/update navigation
            this.populateClickableComponentList();
        }

        private void parseProfessionDescription(ref string professionBlurb, ref string professionTitle, List<string> professionDescription)
        {
            if (professionDescription.Count <= 0)
                return;
            professionTitle = professionDescription[0];
            for (int index = 1; index < professionDescription.Count; ++index)
            {
                professionBlurb += professionDescription[index];
                if (index < professionDescription.Count - 1)
                    professionBlurb += Environment.NewLine;
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = this.skillAreas.Count > 0 ? this.getComponentWithID(0) : null;
            if (this.currentlySnappedComponent == null || !Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            // taking scroll into consideration
            foreach (ClickableTextureComponent skillArea in this.skillAreas)
                skillArea.bounds = new Rectangle(skillArea.bounds.Left, skillArea.bounds.Top - this.skillScrollOffset * 56, skillArea.bounds.Width, skillArea.bounds.Height);

            base.snapCursorToCurrentSnappedComponent();

            // resetting scroll offset
            foreach (ClickableTextureComponent skillArea in this.skillAreas)
                skillArea.bounds = new Rectangle(skillArea.bounds.Left, skillArea.bounds.Top + this.skillScrollOffset * 56, skillArea.bounds.Width, skillArea.bounds.Height);
        }

        public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
        {
            if (this.skillAreas.Contains(b))
            {
                if (this.skillAreaSkillIndexes.TryGetValue(b.myID, out int skillIndex) && (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex))
                    return false;
            }
            return base.IsAutomaticSnapValid(direction, a, b);
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            this.ConstrainVisibleSlotsToSelection();
        }

        public override void setCurrentlySnappedComponentTo(int id)
        {
            if (id == -1)
                return;
            this.currentlySnappedComponent = this.getComponentWithID(id);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // 1.6
            if (x > base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 && x < base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2 + 200 && y > base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)base.height / 2f) + 21 && y < base.yPositionOnScreen + base.height && Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.hasOrWillReceiveMail("JojaMember") && !Game1.player.mailReceived.Contains("activatedJungleJunimo"))
            {
                this.timesClickedJunimo++;
                if (this.timesClickedJunimo > 6)
                {
                    Game1.playSound("discoverMineral");
                    Game1.playSound("leafrustle");
                    Game1.player.mailReceived.Add("activatedJungleJunimo");
                }
                else
                {
                    Game1.playSound("hammer");
                }
            }

            // scrollbar
            if (!this.ShowsAllSkillsAtOnce)
            {
                if (this.upButton.containsPoint(x, y) && this.skillScrollOffset > 0)
                {
                    this.scrollSkillsUp();
                    Game1.playSound("shwip");
                    return;
                }
                if (this.downButton.containsPoint(x, y) && this.skillScrollOffset < this.AllSkillCount - this.MaxSkillCountOnScreen)
                {
                    this.scrollSkillsDown();
                    Game1.playSound("shwip");
                    return;
                }
                if (this.scrollBar.containsPoint(x, y))
                {
                    this.scrolling = true;
                    return;
                }
                if (!this.downButton.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
                {
                    this.scrolling = true;
                    this.leftClickHeld(x, y);
                    this.releaseLeftClick(x, y);
                    return;
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (!this.ShowsAllSkillsAtOnce && this.scrolling)
            {
                int y2 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upButton.bounds.Height + 20));
                float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
                this.skillScrollOffset = Math.Min(this.AllSkillCount - this.MaxSkillCountOnScreen, Math.Max(0, (int)((float)this.AllSkillCount * percentage)));
                this.setScrollBarToCurrentIndex();
                if (y2 != this.scrollBar.bounds.Y)
                {
                    Game1.playSound("shiny4");
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            this.scrolling = false;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height).Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                if (!this.ShowsAllSkillsAtOnce)
                {
                    // skill scrolling
                    if (direction > 0 && this.skillScrollOffset > 0)
                    {
                        this.scrollSkillsUp();
                        this.ConstrainSelectionToVisibleSlots();
                        Game1.playSound("shiny4");
                    }
                    else if (direction < 0 && this.skillScrollOffset < Math.Max(0, this.AllSkillCount - this.MaxSkillCountOnScreen))
                    {
                        this.scrollSkillsDown();
                        this.ConstrainSelectionToVisibleSlots();
                        Game1.playSound("shiny4");
                    }
                }
                // Skill page scrolls between skill components
                else if (this.currentlySnappedComponent is not null)
                {
                    int snapTo = this.currentlySnappedComponent.myID + ((direction < 0 ? 1 : -1) * NewSkillsPage.SkillIdIncrement);
                    if (this.getComponentWithID(snapTo) == null)
                        return;
                    this.setCurrentlySnappedComponentTo(snapTo);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
            this.hoverTitle = "";
            this.professionImage = -1;
            this.upButton.tryHover(x, y);
            this.downButton.tryHover(x, y);

            foreach (ClickableTextureComponent skillBar in this.skillBars)
            {
                if (this.skillBarSkillIndexes.TryGetValue(skillBar.myID, out int skillIndex) && (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex))
                    continue;

                skillBar.scale = 4f;
                // modified y, taking scroll into consideration
                if (skillBar.containsPoint(x, y + this.skillScrollOffset * 56) && skillBar.hoverText.Length > 0 && !skillBar.name.Equals("-1"))
                {
                    this.hoverText = skillBar.hoverText;
                    this.hoverTitle = skillBar.name.StartsWith(NewSkillsPage.CustomSkillPrefix) ? skillBar.name.Substring(1) : LevelUpMenu.getProfessionTitleFromNumber(Convert.ToInt32(skillBar.name));
                    this.professionImage = skillBar.name.StartsWith(NewSkillsPage.CustomSkillPrefix) ? 0 : Convert.ToInt32(skillBar.name);
                    skillBar.scale = 0.0f;
                }
            }
            foreach (ClickableTextureComponent skillArea in this.skillAreas)
            {
                if (this.skillAreaSkillIndexes.TryGetValue(skillArea.myID, out int skillIndex) && (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex))
                    continue;

                // modified y, taking scroll into consideration
                if (skillArea.containsPoint(x, y + this.skillScrollOffset * 56) && skillArea.hoverText.Length > 0)
                {
                    this.hoverText = skillArea.hoverText;
                    this.hoverTitle = skillArea.name.StartsWith(NewSkillsPage.CustomSkillPrefix) ? skillArea.name.Substring(1) : Farmer.getSkillDisplayNameFromIndex(Convert.ToInt32(skillArea.name));
                    break;
                }
            }
            if (this.playerPanel.bounds.Contains(x, y))
            {
                this.playerPanelTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                if (this.playerPanelTimer > 0)
                    return;
                this.playerPanelIndex = (this.playerPanelIndex + 1) % 4;
                this.playerPanelTimer = 150;
            }
            else
            {
                this.playerPanelIndex = 0;
            }
        }

        public override void draw(SpriteBatch b)
        {
            // controller support: updating scroll
            if (this.currentlySnappedComponent != this.lastSnappedComponent)
            {
                this.ConstrainVisibleSlotsToSelection();
                this.lastSnappedComponent = this.currentlySnappedComponent;
            }

            int x = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? this.xPositionOnScreen + this.width - 448 - 48 : this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8;
            int y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 8;
            int indexWithLuckSkill = this.GameSkillCount;
            int xOffset = 0;

            // Menu container
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height,
                speaker: false, drawOnlyBox: true, message: null, objectDialogueWithPortrait: false);

            // Skills tab icon
            if (Game1.activeClickableMenu is GameMenu gm)
            {
                const int selectedTabOffset = 8;
                ClickableComponent c = gm.tabs[GameMenu.skillsTab];
                b.Draw(texture: Game1.mouseCursors,
                    position: new Vector2(c.bounds.X, c.bounds.Y + selectedTabOffset),
                    sourceRectangle: this.skillsTabSource,
                    Color.White, rotation: 0f, origin: Vector2.Zero, scale: Game1.pixelZoom, SpriteEffects.None, layerDepth: 0.0001f);
                Game1.player.FarmerRenderer.drawMiniPortrat(b,
                    position: new Vector2(c.bounds.X + selectedTabOffset, c.bounds.Y + selectedTabOffset + 12),
                    layerDepth: 0.00011f, scale: 3f, facingDirection: 2, who: Game1.player);
            }

            // Farmer portrait area
            {
                int x1 = this.playerPanel.bounds.X - 12;
                int y1 = this.playerPanel.bounds.Y;

                // portrait background
                b.Draw(texture: Game1.timeOfDay >= 1900 ? Game1.nightbg : Game1.daybg, position: new Vector2(x1, y1), Color.White);

                // farmer portrait
                Game1.player.FarmerRenderer.draw(b,
                    new FarmerSprite.AnimationFrame(Game1.player.bathingClothes.Value ? 108 : this.playerPanelFrames[this.playerPanelIndex], 0, false, false),
                    currentFrame: Game1.player.bathingClothes.Value ? 108 : this.playerPanelFrames[this.playerPanelIndex],
                    sourceRect: new Rectangle(this.playerPanelFrames[this.playerPanelIndex] * 16, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32),
                    position: new Vector2(x1 + 32, y1 + 32),
                    origin: Vector2.Zero, layerDepth: 0.8f, facingDirection: 2, Color.White, rotation: 0.0f, scale: 1f, who: Game1.player);

                // dark overlay on farmer
                if (Game1.timeOfDay >= 1900)
                {
                    Game1.player.FarmerRenderer.draw(b,
                        new FarmerSprite.AnimationFrame(this.playerPanelFrames[this.playerPanelIndex], 0, false, false),
                        currentFrame: this.playerPanelFrames[this.playerPanelIndex],
                        sourceRect: new Rectangle(this.playerPanelFrames[this.playerPanelIndex] * 16, 0, 16, 32),
                        position: new Vector2(x1 + 32, y1 + 32),
                        origin: Vector2.Zero, layerDepth: 0.8f, facingDirection: 2, Color.DarkBlue * 0.3f, rotation: 0.0f, scale: 1f, who: Game1.player);
                }

                // subtitles
                b.DrawString(Game1.smallFont, text: Game1.player.Name, position: new Vector2(x1 + 64 - (Game1.smallFont.MeasureString(Game1.player.Name).X / 2f), y1 + 192 + 4), Game1.textColor);
                b.DrawString(Game1.smallFont, text: Game1.player.getTitle(), position: new Vector2(x1 + 64 - (Game1.smallFont.MeasureString(Game1.player.getTitle()).X / 2f), y1 + 256 - 32), Game1.textColor);
            }

            // taking scroll into consideration
            y -= this.skillScrollOffset * 56;

            // Vanilla skills
            for (int levelIndex = 0; levelIndex < 10; ++levelIndex)
            {
                for (int skillIndex = 0; skillIndex < indexWithLuckSkill; ++skillIndex)
                {
                    if (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex)
                        continue;

                    bool drawRed = false;
                    bool addedSkill = false;
                    string skillTitle = "";
                    int skillLevel = 0;
                    Rectangle iconSource = Rectangle.Empty;

                    switch (skillIndex)
                    {
                        case 0:
                            drawRed = Game1.player.FarmingLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604");
                            skillLevel = Game1.player.FarmingLevel;
                            addedSkill = Game1.player.buffs.FarmingLevel > 0;
                            iconSource = new Rectangle(10, 428, 10, 10);
                            break;
                        case 1:
                            drawRed = Game1.player.MiningLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605");
                            skillLevel = Game1.player.MiningLevel;
                            addedSkill = Game1.player.buffs.MiningLevel > 0;
                            iconSource = new Rectangle(30, 428, 10, 10);
                            break;
                        case 2:
                            drawRed = Game1.player.ForagingLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606");
                            skillLevel = Game1.player.ForagingLevel;
                            addedSkill = Game1.player.buffs.ForagingLevel > 0;
                            iconSource = new Rectangle(60, 428, 10, 10);
                            break;
                        case 3:
                            drawRed = Game1.player.FishingLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607");
                            skillLevel = Game1.player.FishingLevel;
                            addedSkill = Game1.player.buffs.FishingLevel > 0;
                            iconSource = new Rectangle(20, 428, 10, 10);
                            break;
                        case 4:
                            drawRed = Game1.player.CombatLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608");
                            skillLevel = Game1.player.CombatLevel;
                            addedSkill = Game1.player.buffs.CombatLevel > 0;
                            iconSource = new Rectangle(120, 428, 10, 10);
                            break;
                        case 5:
                            drawRed = Game1.player.LuckLevel > levelIndex;
                            if (levelIndex == 0)
                                skillTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11609");
                            skillLevel = Game1.player.LuckLevel;
                            addedSkill = Game1.player.buffs.LuckLevel > 0;
                            iconSource = new Rectangle(50, 428, 10, 10);
                            break;
                    }
                    if (skillTitle.Length > 0)
                    {
                        b.DrawString(Game1.smallFont, skillTitle, new Vector2((float)(x - Game1.smallFont.MeasureString(skillTitle).X + 4.0 - 64.0), y + 4 + skillIndex * 56), Game1.textColor);
                        b.Draw(Game1.mouseCursors, new Vector2(x - 56, y + skillIndex * 56), iconSource, Color.Black * 0.3f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
                        b.Draw(Game1.mouseCursors, new Vector2(x - 52, y - 4 + skillIndex * 56), iconSource, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                    }
                    if (!drawRed && (levelIndex + 1) % 5 == 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x - 4 + levelIndex * 36, y + skillIndex * 56), new Rectangle(145, 338, 14, 9), Color.Black * 0.35f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x + levelIndex * 36, y - 4 + skillIndex * 56), new Rectangle(145 + (drawRed ? 14 : 0), 338, 14, 9), Color.White * (drawRed ? 1f : 0.65f), 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                    }
                    else if ((levelIndex + 1) % 5 != 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x - 4 + levelIndex * 36, y + skillIndex * 56), new Rectangle(129, 338, 8, 9), Color.Black * 0.35f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x + levelIndex * 36, y - 4 + skillIndex * 56), new Rectangle(129 + (drawRed ? 8 : 0), 338, 8, 9), Color.White * (drawRed ? 1f : 0.65f), 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                    }
                    if (levelIndex == 9)
                    {
                        NumberSprite.draw(skillLevel, b, new Vector2(xOffset + x + (levelIndex + 2) * 36 + 12 + (skillLevel >= 10 ? 12 : 0), y + 16 + skillIndex * 56), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
                        NumberSprite.draw(skillLevel, b, new Vector2(xOffset + x + (levelIndex + 2) * 36 + 16 + (skillLevel >= 10 ? 12 : 0), y + 12 + skillIndex * 56), (addedSkill ? Color.LightGreen : Color.SandyBrown) * (skillLevel == 0 ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
                    }
                }
                if ((levelIndex + 1) % 5 == 0)
                    xOffset += 24;
            }

            // Custom skills
            foreach (string skillName in VisibleSkills)
            {
                if (indexWithLuckSkill < this.skillScrollOffset || indexWithLuckSkill > this.LastVisibleSkillIndex)
                {
                    ++indexWithLuckSkill;
                    continue;
                }

                xOffset = 0;
                Skills.Skill skill = Skills.GetSkill(skillName);
                for (int levelIndex = 0; levelIndex < skill.ExperienceCurve.Length; ++levelIndex)
                {
                    int skillLevel = 0;
                    bool drawRed = false;
                    bool addedSkill = false;
                    string skillTitle = "";

                    drawRed = Game1.player.GetCustomBuffedSkillLevel(skill) > levelIndex;
                    if (levelIndex == 0)
                        skillTitle = skill.GetName();
                    skillLevel = Game1.player.GetCustomBuffedSkillLevel(skill);
                    int Skillbuff = Game1.player.GetCustomSkillBuffAmount(skill);
                    if (Skillbuff != 0) {
                        addedSkill = true;
                    }
                    if (skillTitle.Length > 0)
                    {
                        b.DrawString(Game1.smallFont, skillTitle, position: new Vector2((float)(x - Game1.smallFont.MeasureString(skillTitle).X + 4.0 - 64.0), (y + 4 + (indexWithLuckSkill * 56))), Game1.textColor);
                        if (skill.SkillsPageIcon != null)
                        {
                            b.Draw(texture: skill.SkillsPageIcon,
                                position: new Vector2(x - 56, y + (indexWithLuckSkill * 56)),
                                sourceRectangle: null,
                                Color.Black * 0.3f, rotation: 0.0f, origin: Vector2.Zero, scale: 4f, SpriteEffects.None, layerDepth: 0.85f);
                            b.Draw(texture: skill.SkillsPageIcon,
                                position: new Vector2(x - 52, y - 4 + (indexWithLuckSkill * 56)),
                                sourceRectangle: null,
                                Color.White, rotation: 0.0f, origin: Vector2.Zero, scale: 4f, SpriteEffects.None, layerDepth: 0.87f);
                        }
                    }
                    if (!drawRed && (levelIndex + 1) % 5 == 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x - 4 + levelIndex * 36, y + indexWithLuckSkill * 56), new Rectangle(145, 338, 14, 9), Color.Black * 0.35f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x + levelIndex * 36, y - 4 + indexWithLuckSkill * 56), new Rectangle(145 + (drawRed ? 14 : 0), 338, 14, 9), Color.White * (drawRed ? 1f : 0.65f), 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                    }
                    else if ((levelIndex + 1) % 5 != 0)
                    {
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x - 4 + levelIndex * 36, y + indexWithLuckSkill * 56), new Rectangle(129, 338, 8, 9), Color.Black * 0.35f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
                        b.Draw(Game1.mouseCursors, new Vector2(xOffset + x + levelIndex * 36, y - 4 + indexWithLuckSkill * 56), new Rectangle(129 + (drawRed ? 8 : 0), 338, 8, 9), Color.White * (drawRed ? 1f : 0.65f), 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
                    }
                    if (levelIndex == 9)
                    {
                        NumberSprite.draw(skillLevel, b, new Vector2(xOffset + x + (levelIndex + 2) * 36 + 12 + (skillLevel >= 10 ? 12 : 0), y + 16 + indexWithLuckSkill * 56), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
                        NumberSprite.draw(skillLevel, b, new Vector2(xOffset + x + (levelIndex + 2) * 36 + 16 + (skillLevel >= 10 ? 12 : 0), y + 12 + indexWithLuckSkill * 56), (addedSkill ? Color.LightGreen : Color.SandyBrown) * (skillLevel == 0 ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
                    }

                    if ((levelIndex + 1) % 5 == 0)
                        xOffset += 24;
                }

                ++indexWithLuckSkill;
            }

            // taking scroll into consideration
            foreach (ClickableTextureComponent skillBar in this.skillBars)
                skillBar.bounds = new Rectangle(skillBar.bounds.Left, skillBar.bounds.Top - this.skillScrollOffset * 56, skillBar.bounds.Width, skillBar.bounds.Height);

            y -= this.skillScrollOffset * 56;

            // Vanilla and custom skill bars
            foreach (ClickableTextureComponent skillBar in this.skillBars)
            {
                if (this.skillBarSkillIndexes.TryGetValue(skillBar.myID, out int skillIndex) && (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex))
                    continue;
                skillBar.draw(b);
            }
            foreach (ClickableTextureComponent skillBar in this.skillBars)
            {
                if (this.skillBarSkillIndexes.TryGetValue(skillBar.myID, out int skillIndex) && (skillIndex < this.skillScrollOffset || skillIndex > this.LastVisibleSkillIndex))
                    continue;
                if (skillBar.scale == 0.0)
                {
                    IClickableMenu.drawTextureBox(b, skillBar.bounds.X - 16 - 8, skillBar.bounds.Y - 16 - 16, 96, 96, Color.White);
                    if (skillBar.name.StartsWith(NewSkillsPage.CustomSkillPrefix))
                    {
                        skillBar.scale = Game1.pixelZoom;
                        if (skillBar.containsPoint(Game1.getMouseX(), Game1.getMouseY()) && !skillBar.name.Equals("-1") && skillBar.hoverText.Length > 0)
                        {
                            List<Skills.Skill.Profession> professions = Skills.SkillsByName.SelectMany(s => s.Value.Professions).ToList();
                            Skills.Skill.Profession profession = professions.FirstOrDefault(p => NewSkillsPage.CustomSkillPrefix + p.Id == skillBar.name);
                            this.hoverText = profession.GetDescription();
                            this.hoverTitle = profession.GetName();
                            Texture2D actuallyAProfessionImage = profession.Icon ?? Game1.staminaRect;
                            skillBar.scale = 0.0f;
                            b.Draw(texture: actuallyAProfessionImage,
                                position: new Vector2(skillBar.bounds.X - (Game1.pixelZoom * 2), skillBar.bounds.Y - (Game1.tileSize / 2) + (Game1.tileSize / 4)),
                                sourceRectangle: new Rectangle(0, 0, 16, 16),
                                Color.White, rotation: 0.0f, origin: Vector2.Zero, scale: 4f, SpriteEffects.None, layerDepth: 1f);
                        }
                    }
                    else
                    {
                        b.Draw(texture: Game1.mouseCursors,
                            position: new Vector2(skillBar.bounds.X - 8, skillBar.bounds.Y - 32 + 16),
                            sourceRectangle: new Rectangle(this.professionImage % 6 * 16, 624 + (this.professionImage / 6 * 16), 16, 16),
                            Color.White, rotation: 0.0f, origin: Vector2.Zero, scale: 4f, SpriteEffects.None, layerDepth: 1f);
                    }
                }
            }

            // resetting scroll offset
            foreach (ClickableTextureComponent skillBar in this.skillBars)
                skillBar.bounds = new Rectangle(skillBar.bounds.Left, skillBar.bounds.Top + this.skillScrollOffset * 56, skillBar.bounds.Width, skillBar.bounds.Height);

            // Stardew Valley 1.5 unique items
            x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 + 24;
            y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 320 - 20;
            {
                Dictionary<string, int> active = new();
                foreach (var entry in VanillaAssetExpansion.VanillaAssetExpansion.virtualCurrencies)
                {
                    int amt = entry.Value.TeamWide ? Game1.player.team.GetVirtualCurrencyAmount(entry.Key) : Game1.player.GetVirtualCurrencyAmount(entry.Key);
                    if (amt > 0)
                    {
                        active.Add(entry.Key, amt);
                    }
                }

                if (active.Count > 0)
                {
                    x = xPositionOnScreen - 150;
                    y += -active.Count * 24;
                    IClickableMenu.drawTextureBox(b, x, y, 150, 32 + 48 * active.Count, Color.White);
                    x += 24; y += -24;
                    foreach (var entry in active)
                    {
                        var data = ItemRegistry.GetDataOrErrorItem($"(O){entry.Key}");
                        var tex = data.GetTexture();
                        var sourceRect = data.GetSourceRect();

                        y += 48;
                        b.Draw(texture: tex,
                            position: new Vector2(x, y),
                            sourceRectangle: sourceRect,
                            Color.White, rotation: 0f, origin: Vector2.Zero, scale: 2f, SpriteEffects.None, layerDepth: 0f);
                        x += 48;
                        b.DrawString(Game1.smallFont, text: string.Concat(entry.Value), position: new Vector2(x, y), Game1.textColor);
                        x -= 48;
                    }
                }

                x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 + 24;
                y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 320 - 36;

                int addedX = 48;
                int addedY = 48;
                if (Game1.netWorldState.Value.GoldenWalnuts > 0)
                {
                    b.Draw(texture: Game1.objectSpriteSheet,
                        position: new Vector2(x, y),
                        sourceRectangle: Game1.getSourceRectForStandardTileSheet(tileSheet: Game1.objectSpriteSheet, tilePosition: 73, 16, 16),
                        Color.White, rotation: 0f, origin: Vector2.Zero, scale: 2f, SpriteEffects.None, layerDepth: 0f);
                    x += addedX;
                    b.DrawString(Game1.smallFont, text: string.Concat(Game1.netWorldState.Value.GoldenWalnuts), position: new Vector2(x, y), Game1.textColor);
                    x -= addedX;
                }
                if (Game1.player.QiGems > 0)
                {
                    y += addedY;
                    b.Draw(texture: Game1.objectSpriteSheet,
                        position: new Vector2(x, y),
                        sourceRectangle: Game1.getSourceRectForStandardTileSheet(tileSheet: Game1.objectSpriteSheet, tilePosition: 858, 16, 16),
                        Color.White, rotation: 0f, origin: Vector2.Zero, scale: 2f, SpriteEffects.None, layerDepth: 0f);
                    x += addedX;
                    b.DrawString(Game1.smallFont, text: string.Concat(Game1.player.QiGems), position: new Vector2(x, y), Game1.textColor);
                    x -= addedX;
                }
            }

            // 1.6 stuff
            y = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)base.height / 2f) + 21;
		    x = base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder * 2;
		    bool isJoja = Game1.MasterPlayer.mailReceived.Contains("JojaMember");
		    x += 80;
		    y += 16;
		    if (isJoja || Game1.MasterPlayer.hasOrWillReceiveMail("canReadJunimoText") || Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
		    {
			    if (!isJoja)
			    {
				    b.Draw(Game1.mouseCursors_1_6, new Vector2(x, y), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccBulletin") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    }
			    else
			    {
				    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 80, y - 16), new Rectangle(363, 250, 51, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    }
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 60, y + 28), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccBoilerRoom") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 60, y + 88), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccVault") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 60, y + 28), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccCraftsRoom") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 60, y + 88), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccFishTank") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x, y + 120), new Rectangle(Game1.MasterPlayer.hasOrWillReceiveMail("ccPantry") ? 374 : 363, 298 + (isJoja ? 11 : 0), 11, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    if (!Utility.hasFinishedJojaRoute() && Game1.MasterPlayer.hasCompletedCommunityCenter())
			    {
				    b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(x - 4) + 30f, (float)(y + 52) + 30f), new Rectangle(386, 299, 13, 15), Color.White, 0f, new Vector2(7.5f), 4f + (float)this.timesClickedJunimo * 0.2f, SpriteEffects.None, 0.7f);
				    if (Game1.player.mailReceived.Contains("activatedJungleJunimo"))
				    {
					    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 80, y - 16), new Rectangle(311, 251, 51, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
				    }
			    }
		    }
		    else
		    {
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 80, y - 16), new Rectangle(414, 250, 52, 47), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    }
		    x += 124;
		    //b.Draw(Game1.staminaRect, new Rectangle(x, y - 16, 4, (int)((float)base.height / 3f) - 32 - 4), new Color(214, 143, 84));
		    int xHouseOffset = 0;
		    if (Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", (int)Game1.player.houseUpgradeLevel.Value + 1)).X > 120f)
		    {
			    xHouseOffset -= 20;
		    }
		    y += 108;
		    x += 28;
		    b.Draw(Game1.mouseCursors, new Vector2(x + xHouseOffset + 20, y - 4), new Rectangle(653, 880, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", (int)Game1.player.houseUpgradeLevel.Value + 1), Game1.smallFont, new Vector2(x + xHouseOffset + 72, y), Game1.textColor);
		    if ((int)Game1.player.houseUpgradeLevel.Value >= 3)
		    {
			    int interval = 709;
			    b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.01f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 2000f), (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.001f, new Vector2(3f, 3f), 0.5f + (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.01f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.001f, new Vector2(5f, 5f), 0.5f + (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors, new Vector2((float)(x + xHouseOffset) + 50f, (float)y - 4f) + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.01f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * 1f * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.001f, new Vector2(4f, 4f), 0.5f + (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 1000f, SpriteEffects.None, 0.7f);
		    }
		    x += 180;
		    y -= 8;
		    bool drawSkull = false;
		    int lowestLevel = MineShaft.lowestLevelReached;
		    if (lowestLevel > 120)
		    {
			    lowestLevel -= 120;
			    drawSkull = true;
		    }
		    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 8, y), new Rectangle((lowestLevel == 0) ? 434 : 385, 315, 13, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    if (lowestLevel != 0)
		    {
			    Utility.drawTextWithShadow(b, lowestLevel.ToString() ?? "", Game1.smallFont, new Vector2(x + 72 + (drawSkull ? 8 : 0), y + 8), Game1.textColor);
		    }
		    if (drawSkull)
		    {
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 40, y + 24), new Rectangle(412, 319, 8, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    }
		    x += 120;
		    int numStardrops = Utility.numStardropsFound();
		    if (numStardrops > 0)
		    {
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 32, y - 4), new Rectangle(399, 314, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    Utility.drawTextWithShadow(b, "x " + numStardrops, Game1.smallFont, new Vector2(x + 88, y + 8), (numStardrops >= 7) ? new Color(160, 30, 235) : Game1.textColor);
		    }
		    else
		    {
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 32, y - 4), new Rectangle(421, 314, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    }
		    if (Game1.stats.Get("MasteryExp") != 0)
		    {
			    int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
			    string masteryText = Game1.content.LoadString("Strings\\1_6_Strings:Mastery");
			    masteryText = masteryText.TrimEnd(':');
			    float masteryStringWidth = Game1.smallFont.MeasureString(masteryText).X;
			    xOffset = (int)masteryStringWidth - 64;
			    int yOffset = 84;
			    b.DrawString(Game1.smallFont, masteryText, new Vector2(base.xPositionOnScreen + 256, yOffset + base.yPositionOnScreen + 408), Game1.textColor);
			    Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2(xOffset + base.xPositionOnScreen + 332, yOffset + base.yPositionOnScreen + 400), new Rectangle(457, 298, 11, 11), Color.White, 0f, Vector2.Zero);
			    float width = 0.64f;
			    width -= (masteryStringWidth - 100f) / 800f;
			    if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
			    {
				    width += 0.1f;
			    }
			    b.Draw(Game1.staminaRect, new Rectangle(xOffset + base.xPositionOnScreen + 380 - 1, yOffset + base.yPositionOnScreen + 408, (int)(584f * width) + 4, 40), Color.Black * 0.35f);
			    b.Draw(Game1.staminaRect, new Rectangle(xOffset + base.xPositionOnScreen + 384, yOffset + base.yPositionOnScreen + 404, (int)((float)(((masteryLevel >= 5) ? 144 : 146) * 4) * width) + 4, 40), new Color(60, 60, 25));
			    b.Draw(Game1.staminaRect, new Rectangle(xOffset + base.xPositionOnScreen + 388, yOffset + base.yPositionOnScreen + 408, (int)(576f * width), 32), new Color(173, 129, 79));
			    MasteryTrackerMenu.drawBar(b, new Vector2(xOffset + base.xPositionOnScreen + 276, yOffset + base.yPositionOnScreen + 264), width);
			    NumberSprite.draw(masteryLevel, b, new Vector2(xOffset + base.xPositionOnScreen + 408 + (int)(584f * width), yOffset + base.yPositionOnScreen + 428), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
			    NumberSprite.draw(masteryLevel, b, new Vector2(xOffset + base.xPositionOnScreen + 412 + (int)(584f * width), yOffset + base.yPositionOnScreen + 424), Color.SandyBrown * ((masteryLevel == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
		    }
		    else
		    {
			    b.Draw(Game1.mouseCursors_1_6, new Vector2(x - 304, y - 88), new Rectangle(366, 236, 142, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    }
		    Rectangle doodleSource = new Rectangle(394, 120 + Game1.seasonIndex * 23, 33, 23);
		    if (Game1.isGreenRain)
		    {
			    doodleSource = new Rectangle(427, 143, 33, 23);
		    }
		    else if (Game1.player.activeDialogueEvents.ContainsKey("married"))
		    {
			    doodleSource = new Rectangle(427, 97, 33, 23);
		    }
		    else if (Game1.IsSpring && Game1.dayOfMonth == 13)
		    {
			    doodleSource.X += 33;
		    }
		    else if (Game1.IsSummer && Game1.dayOfMonth == 11)
		    {
			    doodleSource.X += 66;
		    }
		    else if (Game1.IsFall && Game1.dayOfMonth == 27)
		    {
			    doodleSource.X += 33;
		    }
		    else if (Game1.IsWinter && Game1.dayOfMonth == 25)
		    {
			    doodleSource.X += 33;
		    }
		    b.Draw(Game1.mouseCursors_1_6, new Vector2(x + 144, y - 20), doodleSource, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    if (Game1.IsWinter && Game1.player.mailReceived.Contains("sawSecretSanta" + Game1.year) && ((Game1.dayOfMonth >= 18 && Game1.dayOfMonth < 25) || (Game1.dayOfMonth == 25 && Game1.timeOfDay < 1500)))
		    {
			    NPC k = Utility.GetRandomWinterStarParticipant();
			    Texture2D character_texture;
			    try
			    {
				    character_texture = Game1.content.Load<Texture2D>("Characters\\" + k.Name + "_Winter");
			    }
			    catch
			    {
				    character_texture = k.Sprite.Texture;
			    }
			    Rectangle src = k.getMugShotSourceRect();
			    src.Height -= 5;
			    b.Draw(character_texture, new Vector2(x + 180, y), src, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
			    b.Draw(Game1.mouseCursors, new Vector2(x + 244, y + 40), new Rectangle(147, 412, 10, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.7f);
		    }
		    if (this.hoverText.Length > 0)
		    {
			    IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, 0, 0, -1, (this.hoverTitle.Length > 0) ? this.hoverTitle : null);
		    }

            // scrollbar
            if (!this.ShowsAllSkillsAtOnce)
            {
                this.upButton.draw(b);
                this.downButton.draw(b);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f);
                this.scrollBar.draw(b);
            }

            // Hover text
            if (this.hoverText.Length > 0)
                IClickableMenu.drawHoverText(b, text: this.hoverText, Game1.smallFont, xOffset: 0, yOffset: 0, boldTitleText: this.hoverTitle.Length > 0 ? this.hoverTitle : null);
        }

        private void setScrollBarToCurrentIndex()
        {
            this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.AllSkillCount - this.MaxSkillCountOnScreen + 1) * this.skillScrollOffset + this.upButton.bounds.Bottom + 4;
            if (this.skillScrollOffset == this.AllSkillCount - this.MaxSkillCountOnScreen)
                this.scrollBar.bounds.Y = this.downButton.bounds.Y - this.scrollBar.bounds.Height - 4;
        }

        private void scrollSkillsUp()
        {
            this.skillScrollOffset--;
            this.upButton.scale = 3.5f;
            this.setScrollBarToCurrentIndex();
        }

        private void scrollSkillsDown()
        {
            this.skillScrollOffset++;
            this.downButton.scale = 3.5f;
            this.setScrollBarToCurrentIndex();
        }

        private void ConstrainSelectionToVisibleSlots()
        {
            if (this.skillAreas.Contains(this.currentlySnappedComponent))
            {
                if (!this.skillAreaSkillIndexes.TryGetValue(this.currentlySnappedComponent.myID, out int skillIndex))
                {
                    if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                        this.snapCursorToCurrentSnappedComponent();
                    return;
                }

                if (skillIndex < this.skillScrollOffset)
                {
                    int skillAreaIDToSnapTo = this.skillAreaSkillIndexes.First(kvp => kvp.Value == this.skillScrollOffset).Key;
                    this.currentlySnappedComponent = this.skillAreas.First(a => a.myID == skillAreaIDToSnapTo);
                }
                else if (skillIndex > this.LastVisibleSkillIndex)
                {
                    int skillAreaIDToSnapTo = this.skillAreaSkillIndexes.First(kvp => kvp.Value == this.LastVisibleSkillIndex).Key;
                    this.currentlySnappedComponent = this.skillAreas.First(a => a.myID == skillAreaIDToSnapTo);
                }

                if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                    this.snapCursorToCurrentSnappedComponent();
            }
        }

        private void ConstrainVisibleSlotsToSelection()
        {
            if (this.skillAreas.Contains(this.currentlySnappedComponent))
            {
                if (!this.skillAreaSkillIndexes.TryGetValue(this.currentlySnappedComponent.myID, out int skillIndex))
                {
                    if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                        this.snapCursorToCurrentSnappedComponent();
                    return;
                }

                if (skillIndex < this.skillScrollOffset)
                {
                    this.skillScrollOffset = skillIndex;
                    this.setScrollBarToCurrentIndex();
                }
                else if (skillIndex > this.LastVisibleSkillIndex)
                {
                    this.skillScrollOffset = skillIndex - this.MaxSkillCountOnScreen + 1;
                    this.setScrollBarToCurrentIndex();
                }

                if (Game1.options.snappyMenus && Game1.options.gamepadControls)
                    this.snapCursorToCurrentSnappedComponent();
            }
        }
    }
}
