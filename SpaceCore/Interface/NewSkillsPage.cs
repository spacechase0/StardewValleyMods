using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceCore.Events;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Interface
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = DiagnosticMessages.CopiedFromGameCode)]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = DiagnosticMessages.CopiedFromGameCode)]
    public class NewSkillsPage : IClickableMenu
    {
        public Texture2D texture;
        public List<ClickableTextureComponent> skillBars = new();
        public List<ClickableTextureComponent> skillAreas = new();
        public List<ClickableTextureComponent> specialItems = new();
        public List<ClickableTextureComponent> specialItemComponents = new();
        public ClickableTextureComponent walletUpArrow;
        public ClickableTextureComponent walletDownArrow;
        public ClickableComponent playerPanel;
        private Rectangle walletArea;
        private Rectangle walletIconArea;
        private readonly Rectangle walletIconAreaSource = new(293, 360, 24, 24);
        private readonly Rectangle walletIconSource = new(0, 0, 12, 12);
        private readonly Rectangle skillsTabSource = new(16, 368, 16, 16);
        private int SkillTabRegionId => Game1.activeClickableMenu is GameMenu ? GameMenu.region_skillsTab : -1;
        private bool IsWalletRightSide => SpaceCore.Instance.Config.WalletOnRightOfSkillPage
            && !SpaceCore.Instance.Helper.ModRegistry.IsLoaded("alphablackwolf.skillPrestige"); // Skill prestige places icons in right-side wallet area
        private bool IsLegacyWallet => SpaceCore.Instance.Config.WalletLegacyStyle;
        private int walletSelectionOffset;
        private int WalletSelectionOffset
        {
            get => this.walletSelectionOffset;
            set => this.walletSelectionOffset = value < 0 ? ((this.specialItemComponents.Count - 1) * 2) - 1 : value % ((this.specialItemComponents.Count - 1) * 2);
        }
        private bool CanScrollWalletItems => !this.IsLegacyWallet && this.specialItems.Count > this.specialItemComponents.Count;

        private bool CanNavigateToWallet => this.specialItems.Any() && (!this.IsLegacyWallet || this.specialItemComponents.First().bounds.Y > 0);
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
        public const int WalletRegionStartId = 10250;
        public const int WalletIdIncrement = 1;
        public const int WalletUpArrowRegionId = 10201;
        public const int WalletDownArrowRegionId = 10202;
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

        private int GameSkillCount
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // allowing mods to patch this getter if needed (alternative luck skill implementations?)
            get => SpaceCore.Instance.Helper.ModRegistry.IsLoaded("spacechase0.LuckSkill") ? 6 : 5;
        }

        private int AllSkillCount
            => this.GameSkillCount + Skills.GetSkillList().Length;

        private int MaxSkillCountOnScreen
        {
            [MethodImpl(MethodImplOptions.NoInlining)] // allowing mods to patch this getter if needed
            get => 9;
        }

        private int LastVisibleSkillIndex
            => this.skillScrollOffset + this.MaxSkillCountOnScreen - 1;

        private bool ShowsAllSkillsAtOnce
            => this.AllSkillCount <= this.MaxSkillCountOnScreen;

        public NewSkillsPage(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.texture = SpaceCore.Instance.Helper.ModContent.Load<Texture2D>(Path.Combine("assets/sprites.png"));

            // Player panel
            this.playerPanel = new ClickableComponent(bounds: new Rectangle(this.xPositionOnScreen + 64, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 128, 192), name: null)
            {
                myID = NewSkillsPage.PlayerPanelRegionId
            };
            // navigation is handled in receiveKeyPress to avoid confusing the navigation logic

            // Wallet area
            {
                int padInner = this.IsWalletRightSide ? -8 : -64;
                int iconAreaWidth = this.walletIconAreaSource.Width * Game1.pixelZoom;
                int iconAreaHeight = this.walletIconAreaSource.Height * Game1.pixelZoom;
                int walletX = this.IsWalletRightSide ? this.xPositionOnScreen + this.width + padInner : this.xPositionOnScreen - iconAreaWidth + padInner;
                this.walletArea = this.IsLegacyWallet
                    ? new Rectangle(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)(this.height / 2.0) - 48 - 600, this.width - 64 - (IClickableMenu.spaceToClearSideBorder * 2), (this.height / 4) + 64)
                    : new Rectangle(walletX, this.yPositionOnScreen, 72 + iconAreaHeight, this.height);
                this.walletIconArea = new Rectangle(this.walletArea.X + ((this.walletArea.Width - iconAreaWidth) / 2), this.walletArea.Y + (iconAreaHeight / 2) - 8, iconAreaWidth, iconAreaHeight);
            }

            int iconWidth = 16;

            // Wallet contents
            {
                // Special item names and whether they've been unlocked
                List<KeyValuePair<string, bool>> specialFlags = new List<KeyValuePair<string, bool>>
                {
                    new("StringsFromCSFiles:SkillsPage.cs.11587", Game1.player.canUnderstandDwarves),
                    new("StringsFromCSFiles:SkillsPage.cs.11588", Game1.player.hasRustyKey),
                    new("StringsFromCSFiles:SkillsPage.cs.11589", Game1.player.hasClubCard),
                    new("StringsFromCSFiles:SkillsPage.cs.11590", Game1.player.hasSpecialCharm),
                    new("StringsFromCSFiles:SkillsPage.cs.11591", Game1.player.hasSkullKey),
                    new("StringsFromCSFiles:SkillsPage.cs.magnifyingglass", Game1.player.hasMagnifyingGlass),
                    new("Objects:DarkTalisman", Game1.player.hasDarkTalisman),
                    new("Objects:MagicInk", Game1.player.hasMagicInk),
                    new("Objects:BearPaw", Game1.player.eventsSeen.Contains("2120303")),
                    new("Objects:SpringOnionBugs", Game1.player.eventsSeen.Contains("3910979"))
                };

                const int padTop = 16;

                // Actual wallet special items
                {
                    int iconDestWidth = iconWidth * Game1.pixelZoom;
                    int offsetIndex = specialFlags.FindIndex(pair => pair.Key.EndsWith("MagicInk"));
                    for (int i = 0; i < specialFlags.Count; ++i)
                    {
                        if (!specialFlags[i].Value)
                            continue;

                        ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                            name: "", bounds: new Rectangle(-1, -1, iconDestWidth, iconDestWidth),
                            label: null, hoverText: Game1.content.LoadString($"Strings\\{specialFlags[i].Key}"),
                            texture: Game1.mouseCursors, sourceRect: new Rectangle(128 + (i <= offsetIndex ? i * iconWidth : (iconWidth * 4) + (iconWidth * (i - offsetIndex - 1))), i <= offsetIndex ? 320 : 320 + iconWidth, iconWidth, iconWidth), scale: 4f, drawShadow: true);

                        this.specialItems.Add(textureComponent);
                    }
                    if (Game1.player.HasTownKey)
                    {
                        this.specialItems.Add(new ClickableTextureComponent(
                            name: "", bounds: new Rectangle(-1, -1, iconDestWidth, iconDestWidth),
                            label: null, hoverText: Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"),
                            texture: Game1.objectSpriteSheet, sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 912, iconWidth, iconWidth), scale: 4f, drawShadow: true));
                    }

                    SpaceEvents.InvokeAddWalletItems( this );
                }

                // Wallet navigation arrows
                iconWidth = 32;
                this.walletUpArrow = new ClickableTextureComponent(
                    name: "", bounds: new Rectangle(this.walletArea.X + ((this.walletArea.Width - iconWidth) / 2), this.walletArea.Y + 148, iconWidth, iconWidth),
                    label: null, hoverText: null,
                    texture: Game1.mouseCursors, sourceRect: new Rectangle(442, 96, iconWidth, iconWidth), scale: 1f, drawShadow: false);
                this.walletDownArrow = new ClickableTextureComponent(
                    name: "", bounds: new Rectangle(this.walletUpArrow.bounds.X, this.walletUpArrow.bounds.Y + this.walletArea.Height - 224, this.walletUpArrow.bounds.Width, this.walletUpArrow.bounds.Height),
                    label: null, hoverText: null,
                    texture: this.walletUpArrow.texture, sourceRect: this.walletUpArrow.sourceRect, scale: this.walletUpArrow.scale, drawShadow: this.walletUpArrow.drawShadow);

                // Wallet item icons for navigation
                const int padRight = 4;
                iconWidth = 16;
                int iconCount = this.IsLegacyWallet ? 10 : (this.walletDownArrow.bounds.Y - this.walletUpArrow.bounds.Y - (padTop * 2)) / iconWidth / Game1.pixelZoom;
                bool shouldNavButtonsBeShown = !this.IsLegacyWallet && this.specialItems.Count > iconCount;
                for (int index = 0; index < iconCount; ++index)
                {
                    int iconDestWidth = iconWidth * Game1.pixelZoom;
                    int x2 = this.IsLegacyWallet ? this.walletArea.X + 48 + ((iconDestWidth + padRight) * index) : this.walletUpArrow.bounds.X + (this.walletUpArrow.bounds.Width / 2) - (iconDestWidth / 2);
                    int y2 = this.IsLegacyWallet ? this.walletArea.Y + 120 : this.walletUpArrow.bounds.Y + (this.specialItems.Count > iconCount ? this.walletUpArrow.bounds.Height : 0) + padTop + (index * iconDestWidth);
                    ClickableTextureComponent textureComponent = new ClickableTextureComponent(
                        name: "",
                        bounds: new Rectangle(x2, y2, iconDestWidth, iconDestWidth),
                        label: "",
                        hoverText: "",
                        texture: Game1.mouseCursors, sourceRect: new Rectangle(-1, -1, iconWidth, iconWidth),
                        scale: Game1.pixelZoom, drawShadow: true
                    )
                    {
                        myID = NewSkillsPage.WalletRegionStartId + this.specialItemComponents.Count
                    };
                    if (this.IsLegacyWallet)
                    {
                        textureComponent.leftNeighborID = index == 0 ? -1 : textureComponent.myID - NewSkillsPage.WalletIdIncrement;
                        textureComponent.rightNeighborID = index < this.specialItems.Count - 1 ? index == iconCount - 1 ? -1 : textureComponent.myID + NewSkillsPage.WalletIdIncrement : -1;
                    }
                    else
                    {
                        // left/right neighbour IDs are omitted here, navigating to SkillRegionStartId confuses the snapping logic
                        textureComponent.upNeighborID = index == 0 ? shouldNavButtonsBeShown ? NewSkillsPage.WalletUpArrowRegionId : -1 : textureComponent.myID - NewSkillsPage.WalletIdIncrement;
                        textureComponent.downNeighborID = index < this.specialItems.Count - 1 ? index == iconCount - 1 ? shouldNavButtonsBeShown ? NewSkillsPage.WalletDownArrowRegionId : -1 : textureComponent.myID + NewSkillsPage.WalletIdIncrement : -1;
                    }
                    this.specialItemComponents.Add(textureComponent);
                }
            }

            // Wallet nav arrow navigation
            this.walletUpArrow.myID = NewSkillsPage.WalletUpArrowRegionId;
            this.walletDownArrow.myID = NewSkillsPage.WalletDownArrowRegionId;
            this.walletUpArrow.downNeighborID = this.specialItemComponents.First().myID;
            this.walletDownArrow.upNeighborID = this.specialItemComponents.Last().myID;

            // Professions
            string[] skills = Skills.GetSkillList();
            int drawX = 0;
            int addedX = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? this.xPositionOnScreen + width - 448 - 48 + 4 : this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 4;
            int drawY = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 12;
            int gameSkillCount = this.GameSkillCount;
            int walletSnapId = this.specialItems.Any() ? NewSkillsPage.WalletRegionStartId : -1;
            int leftSnapId = this.playerPanel.myID;
            int rightSnapId = this.IsLegacyWallet || !this.IsWalletRightSide ? -1 : walletSnapId;

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
                        textureComponent.rightNeighborID = professionIndex == 2 ? rightSnapId : textureComponent.myID + NewSkillsPage.SkillProfessionIncrement;
                        textureComponent.downNeighborID = skillIndex == gameSkillCount - 1 && skills.Length == 0 ? walletSnapId : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
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
                for (int skillIndex = 0; skillIndex < skills.Length; ++skillIndex)
                {
                    int totalSkillIndex = gameSkillCount + skillIndex;
                    int professionLevel = professionIndex - 1 + (professionIndex * 4);
                    Skills.Skill skill = Skills.GetSkill(skills[skillIndex]);
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
                        textureComponent.rightNeighborID = professionIndex == 2 ? rightSnapId : textureComponent.myID + NewSkillsPage.SkillProfessionIncrement;
                        textureComponent.downNeighborID = skillIndex == skills.Length - 1 ? walletSnapId : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
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
                            break;
                        }
                        break;
                    case 1:
                        if (Game1.player.FishingLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11598", Game1.player.FishingLevel);
                            break;
                        }
                        break;
                    case 2:
                        if (Game1.player.ForagingLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11596", Game1.player.ForagingLevel);
                            break;
                        }
                        break;
                    case 3:
                        if (Game1.player.MiningLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11600", Game1.player.MiningLevel);
                            break;
                        }
                        break;
                    case 4:
                        if (Game1.player.CombatLevel > 0)
                        {
                            hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11602", Game1.player.CombatLevel * 5);
                            break;
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
                textureComponent.downNeighborID = skillIndex == gameSkillCount - 1 && skills.Length == 0 ? walletSnapId : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                textureComponent.upNeighborID = skillIndex > 0 ? textureComponent.myID - NewSkillsPage.SkillProfessionIncrement : this.SkillTabRegionId;

                if (this.IsWalletRightSide)
                {
                    int level = Game1.player.GetSkillLevel(actualSkillIndex);
                    if (level < 10)
                    {
                        // allow the player to navigate to the wallet without having profession 2 unlocked for a skill
                        for (int professionIndex = 2; professionIndex >= 0; --professionIndex)
                        {
                            int snapTo = textureComponent.myID + (professionIndex * NewSkillsPage.SkillProfessionIncrement);
                            var clickable = (ClickableComponent)this.skillBars.FirstOrDefault(c => c.myID == snapTo);
                            if (clickable != null)
                                clickable.rightNeighborID = rightSnapId;
                            else
                                textureComponent.rightNeighborID = rightSnapId;
                        }
                    }
                }

                this.skillAreas.Add(textureComponent);
                this.skillAreaSkillIndexes[textureComponent.myID] = skillIndex;
            }

            // Icons for custom skills
            for (int skillIndex = 0; skillIndex < skills.Length; ++skillIndex)
            {
                Skills.Skill skill = Skills.GetSkill(skills[skillIndex]);
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
                textureComponent.downNeighborID = skillIndex == skills.Length - 1 ? -1 : textureComponent.myID + NewSkillsPage.SkillIdIncrement;
                textureComponent.upNeighborID = textureComponent.myID - NewSkillsPage.SkillIdIncrement;

                if (this.IsWalletRightSide && !this.IsLegacyWallet)
                {
                    int level = Game1.player.GetSkillLevel(actualSkillIndex);
                    if (level < 10)
                    {
                        // allow the player to navigate to the wallet without having profession 2 unlocked for a skill
                        for (int professionIndex = 2; professionIndex >= 0; --professionIndex)
                        {
                            int snapTo = textureComponent.myID + (professionIndex * NewSkillsPage.SkillProfessionIncrement);
                            var clickable = (ClickableComponent)this.skillBars.FirstOrDefault(c => c.myID == snapTo);
                            if (clickable != null)
                                clickable.rightNeighborID = rightSnapId;
                            else
                                textureComponent.rightNeighborID = rightSnapId;
                        }
                    }
                }

                this.skillAreas.Add(textureComponent);
                this.skillAreaSkillIndexes[textureComponent.myID] = actualSkillIndex;
            }

            // scrollbar
            this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 16, this.yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upButton.bounds.X + 12, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upButton.bounds.Y + this.upButton.bounds.Height + 4, this.scrollBar.bounds.Width, height - 128 - this.upButton.bounds.Height - 8);

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
            // Wallet up-down arrows scroll list of items if there are more than can be shown in walletArea
            if (this.CanScrollWalletItems)
            {
                if (this.walletUpArrow.containsPoint(x, y))
                {
                    --this.WalletSelectionOffset;
                    if (playSound)
                        Game1.playSound("Cowboy_gunshot");
                }
                else if (this.walletDownArrow.containsPoint(x, y))
                {
                    ++this.WalletSelectionOffset;
                    if (playSound)
                        Game1.playSound("Cowboy_gunshot");
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

        public override void receiveKeyPress(Keys key)
        {
            // Left-right navigation from wallet items redirects to the skills page/player panel
            // Page navigation is corrupted when skills page is used as a neighbour for these components, so we handle it here
            if (Game1.options.SnappyMenus && this.currentlySnappedComponent != null)
            {
                // wallet navigation
                if (this.currentlySnappedComponent.myID is NewSkillsPage.WalletUpArrowRegionId or NewSkillsPage.WalletDownArrowRegionId || (this.currentlySnappedComponent.myID >= NewSkillsPage.WalletRegionStartId && this.currentlySnappedComponent.myID < NewSkillsPage.WalletRegionStartId + (this.IsLegacyWallet ? this.specialItemComponents.Count : this.specialItems.Count)))
                {
                    // navigating left with a right-side non-legacy wallet will move to the skills page
                    if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key) && this.IsWalletRightSide && !this.IsLegacyWallet)
                    {
                        this.setCurrentlySnappedComponentTo(NewSkillsPage.SkillRegionStartId);
                        return;
                    }
                    // navigating right with a left-side non-legacy wallet will move to the player panel
                    else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key) && !this.IsWalletRightSide && !this.IsLegacyWallet)
                    {
                        this.setCurrentlySnappedComponentTo(NewSkillsPage.PlayerPanelRegionId);
                        return;
                    }
                    // navigating down with a legacy wallet will move to the skill tab
                    else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key) && this.IsLegacyWallet)
                    {
                        this.setCurrentlySnappedComponentTo(this.SkillTabRegionId);
                        return;
                    }
                }
                // player panel navigation
                else if (this.currentlySnappedComponent.myID == NewSkillsPage.PlayerPanelRegionId)
                {
                    // moving left with a left-side non-legacy wallet will move to the wallet
                    if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key) && !this.IsWalletRightSide && !this.IsLegacyWallet)
                    {
                        this.setCurrentlySnappedComponentTo(NewSkillsPage.WalletRegionStartId);
                        return;
                    }
                    // moving right will move to the skills page
                    else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
                    {
                        this.setCurrentlySnappedComponentTo(NewSkillsPage.SkillRegionStartId);
                        return;
                    }
                    // moving up will move to the skill tab
                    else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
                    {
                        this.setCurrentlySnappedComponentTo(this.SkillTabRegionId);
                        return;
                    }
                }
                // skill tab navigation
                else if (this.currentlySnappedComponent.myID == this.SkillTabRegionId)
                {
                    // navigating up with a legacy wallet will move to the wallet if on-screen
                    if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key) && this.CanNavigateToWallet)
                    {
                        this.setCurrentlySnappedComponentTo(NewSkillsPage.WalletRegionStartId);
                        return;
                    }
                }
            }
            base.receiveKeyPress(key);
        }

        public override void receiveGamePadButton(Buttons b)
        {
            // Shoulder buttons navigate between skills and wallet
            if (b is Buttons.LeftShoulder or Buttons.RightShoulder)
            {
                if (this.currentlySnappedComponent?.myID < NewSkillsPage.WalletRegionStartId && this.CanNavigateToWallet)
                {
                    this.setCurrentlySnappedComponentTo(NewSkillsPage.WalletRegionStartId);
                }
                else
                {
                    this.setCurrentlySnappedComponentTo(0);
                }
                Game1.playSound("smallSelect");
            }

            base.receiveGamePadButton(b);
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
            // Wallet area scrolls list of items without moving cursor
            else if (this.CanScrollWalletItems && this.walletArea.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                if (direction < 0)
                    ++this.WalletSelectionOffset;
                else
                    --this.WalletSelectionOffset;
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
            this.hoverTitle = "";
            this.professionImage = -1;
            this.upButton.tryHover(x, y);
            this.downButton.tryHover(x, y);

            if (this.walletIconArea.Contains(x, y))
                this.hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11610");

            for (int index = 0; index < this.specialItemComponents.Count && this.hoverText.Length == 0; ++index)
            {
                if (this.specialItemComponents[index].containsPoint(x, y))
                {
                    this.hoverText = (index < this.specialItems.Count || this.CanScrollWalletItems) ? this.specialItems[(index + this.walletSelectionOffset) % this.specialItems.Count].hoverText : "";
                }
            }
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
            foreach (string skillName in Skills.GetSkillList())
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
                    // TODO: Detect skill buffs? Is that even possible?
                    addedSkill = false; // (int)((NetFieldBase<int, NetInt>)Game1.player.addedFarmingLevel) > 0;
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
            x = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32 + 16;
            y = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 320 - 8;
            {
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

            // Wallet
            if (this.IsLegacyWallet)
            {
                Game1.drawDialogueBox(this.walletArea.X, this.walletArea.Y, this.walletArea.Width, this.walletArea.Height, speaker: false, drawOnlyBox: true);
                this.drawBorderLabel(b, text: Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11610"), Game1.smallFont, this.walletArea.X + 64, this.walletArea.Y);
                for (int index = 0; index < this.specialItems.Count; ++index)
                {
                    b.Draw(texture: Game1.mouseCursors,
                        destinationRectangle: this.specialItemComponents[index].bounds,
                        sourceRectangle: this.specialItems[index].sourceRect,
                        Color.White, rotation: 0f, origin: Vector2.Zero, SpriteEffects.None, layerDepth: 1f);
                }
            }
            else if (this.specialItems.Count > 0)
            {
                // container
                Game1.drawDialogueBox(this.walletArea.X, this.walletArea.Y, this.walletArea.Width, this.walletArea.Height, speaker: false, drawOnlyBox: true);
                // wallet label container
                b.Draw(texture: Game1.mouseCursors,
                    destinationRectangle: this.walletIconArea,
                    sourceRectangle: this.walletIconAreaSource,
                    Color.White, rotation: 0f, origin: Vector2.Zero, SpriteEffects.None, 1f);
                // wallet label icon
                b.Draw(this.texture,
                    destinationRectangle: new Rectangle(this.walletIconArea.X + ((24 - this.walletIconSource.Width) * Game1.pixelZoom / 2), this.walletIconArea.Y + ((24 - this.walletIconSource.Height) * Game1.pixelZoom / 2), this.walletIconSource.Width * Game1.pixelZoom, this.walletIconSource.Height * Game1.pixelZoom),
                    sourceRectangle: this.walletIconSource,
                    Color.White, rotation: 0f, origin: Vector2.Zero, SpriteEffects.None, 1f);
                // wallet label item count
                Utility.drawTinyDigits(toDraw: this.specialItems.Count, b,
                    position: new Vector2(this.walletIconArea.X + (28 * Game1.pixelZoom / 2), this.walletIconArea.Y + (15 * Game1.pixelZoom)),
                    scale: 3f, layerDepth: 0.87f, Color.AntiqueWhite);
                // wallet item scroll arrows
                if (this.CanScrollWalletItems)
                {
                    // rotate arrows 270 and 90 degrees around their centre, and correct the draw position from doing this
                    b.Draw(texture: Game1.mouseCursors,
                        destinationRectangle: new Rectangle(this.walletUpArrow.bounds.X + (this.walletUpArrow.bounds.Width / 2), this.walletUpArrow.bounds.Y + (this.walletUpArrow.bounds.Height / 2), this.walletUpArrow.bounds.Width, this.walletUpArrow.bounds.Height),
                        sourceRectangle: this.walletUpArrow.sourceRect,
                        Color.White, rotation: (float)(Math.PI / 180 * 270), origin: new Vector2(this.walletUpArrow.bounds.Width / 2), SpriteEffects.None, layerDepth: 1f);
                    b.Draw(texture: Game1.mouseCursors,
                        destinationRectangle: new Rectangle(this.walletDownArrow.bounds.X + (this.walletDownArrow.bounds.Width / 2), this.walletDownArrow.bounds.Y + (this.walletDownArrow.bounds.Height / 2), this.walletDownArrow.bounds.Width, this.walletDownArrow.bounds.Height),
                        sourceRectangle: this.walletDownArrow.sourceRect,
                        Color.White, rotation: (float)(Math.PI / 180 * 90), origin: new Vector2(this.walletDownArrow.bounds.Width / 2), SpriteEffects.None, layerDepth: 1f);
                }
                // wallet items
                for (int iconIndex = 0; iconIndex < this.specialItemComponents.Count; ++iconIndex)
                {
                    int itemIndex = (iconIndex + this.WalletSelectionOffset) % this.specialItems.Count;
                    if (iconIndex < this.specialItems.Count)
                    {
                        b.Draw(texture: this.specialItems[itemIndex].texture,
                            destinationRectangle: this.specialItemComponents[iconIndex % this.specialItemComponents.Count].bounds,
                            sourceRectangle: this.specialItems[itemIndex].sourceRect,
                            Color.White, rotation: 0f, origin: Vector2.Zero, SpriteEffects.None, layerDepth: 1f);
                    }
                }
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
