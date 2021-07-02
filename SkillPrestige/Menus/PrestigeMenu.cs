using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Elements.Buttons;
using SkillPrestige.Professions;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus
{
    /// <summary>
    /// Represents a menu where players can choose to prestige a skill and select prestige awards.
    /// </summary>
    internal class PrestigeMenu : IClickableMenu, IInputHandler
    {
        private readonly Skill _skill;
        private readonly Prestige _prestige;
        private PrestigeButton _prestigeButton;
        private TextureButton _settingsButton;
        private Vector2 _professionButtonRowStartLocation;
        private Vector2 _prestigePointBonusLocation;
        private readonly int _rowPadding = Game1.tileSize / 3;
        private int _leftProfessionStartingXLocation;
        private readonly IList<MinimalistProfessionButton> _professionButtons = new List<MinimalistProfessionButton>();
        private static int Offset => 4*Game1.pixelZoom;

        private static int ProfessionButtonHeight(Profession profession)
        {
                var iconHeight = profession.IconSourceRectangle.Height*Game1.pixelZoom;
                var textHeight = (Game1.dialogueFont.MeasureString(string.Join(Environment.NewLine, profession.DisplayName.Split(' '))).Y).Ceiling();
                return Offset*3 + iconHeight + textHeight;
        }

        private int GetRowHeight<T>() where T : Profession
        {
            return _skill.Professions.Where(x => x is T).Select(ProfessionButtonHeight).Max();
        }

        private int CostTextYOffset<T>() where T : Profession
        {
            return ((double)GetRowHeight<T>() / 2 - Game1.dialogueFont.MeasureString(CostText).Y / 2).Floor();
        } 

        private const string CostText = "Cost:";
        private int _debounceTimer = 10;
        private int _xSpaceAvailableForProfessionButtons;

        public PrestigeMenu(Rectangle bounds, Skill skill, Prestige prestige)
            : base(bounds.X, bounds.Y, bounds.Width, bounds.Height, true)
        {
            Logger.LogVerbose($"New {skill.Type.Name} Prestige Menu created.");
            _skill = skill;
            _prestige = prestige;
            InitiatePrestigeButton();
            InitiateSettingsButton();
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            if (_debounceTimer > 0)
                return;

            foreach (var button in _professionButtons)
                button.OnCursorMoved(e);
            _prestigeButton.OnCursorMoved(e);
            _settingsButton.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            if (_debounceTimer > 0)
                return;

            foreach (var button in _professionButtons)
                button.OnButtonPressed(e, isClick);
            _prestigeButton.OnButtonPressed(e, isClick);
            _settingsButton.OnButtonPressed(e, isClick);
        }

        private void InitiatePrestigeButton()
        {
            Logger.LogVerbose("Prestige menu - Initiating prestige button...");
            const int yOffset = 3;
            var buttonWidth = 100 * Game1.pixelZoom;
            var buttonHeight = 20 * Game1.pixelZoom;
            var rightEdgeOfDialog = xPositionOnScreen + width;
            var bounds = new Rectangle(rightEdgeOfDialog - spaceToClearSideBorder - buttonWidth,
                yPositionOnScreen + yOffset + (Game1.tileSize * 3.15).Floor(), buttonWidth, buttonHeight);

            var prestigeButtonDisabled = true;
            if (PerSaveOptions.Instance.PainlessPrestigeMode)
            {
                if (Game1.player.experiencePoints[_skill.Type.Ordinal] >= 15000 + PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige)
                {
                    prestigeButtonDisabled = false;
                }
            }
            else
            {
                if (_skill.GetSkillLevel() == 10)
                {
                    var newLevelForSkillExists = Game1.player.newLevels.Any(point => point.X == _skill.Type.Ordinal && point.Y > 0);
                    if (!newLevelForSkillExists)
                    {
                        prestigeButtonDisabled = false;
                    }
                }
            }

            _prestigeButton = new PrestigeButton(prestigeButtonDisabled, _skill)
            {
                Bounds = bounds,
            };
            Logger.LogVerbose("Prestige menu - Prestige button initiated.");
        }

        private void InitiateSettingsButton()
        {
            Logger.LogVerbose("Prestige menu - Initiating settings button...");
            var buttonWidth = 16 * Game1.pixelZoom;
            var buttonHeight = 16 * Game1.pixelZoom;
            var rightEdgeOfDialog = xPositionOnScreen + width;
            var bounds = new Rectangle(rightEdgeOfDialog - buttonWidth - Game1.tileSize, yPositionOnScreen, buttonWidth, buttonHeight);
            _settingsButton = new TextureButton(bounds, Game1.mouseCursors, new Rectangle(96, 368, 16, 16), OpenSettingsMenu, "Open Settings Menu");
            
            Logger.LogVerbose("Prestige menu - Settings button initiated.");
        }

        private void OpenSettingsMenu()
        {
            Logger.LogVerbose("Prestige Menu - Initiating Settings Menu...");
            var menuWidth = Game1.tileSize * 12;
            var menuHeight = Game1.tileSize * 10;
            var menuXCenter = (menuWidth + borderWidth * 2) / 2;
            var menuYCenter = (menuHeight + borderWidth * 2) / 2;
            var viewport = Game1.graphics.GraphicsDevice.Viewport;
            var screenXCenter = (int)(viewport.Width * (1.0 / Game1.options.zoomLevel)) / 2;
            var screenYCenter = (int)(viewport.Height * (1.0 / Game1.options.zoomLevel)) / 2;
            var bounds = new Rectangle(screenXCenter - menuXCenter, screenYCenter - menuYCenter,
                menuWidth + borderWidth*2, menuHeight + borderWidth*2);
            Game1.playSound("bigSelect");
            exitThisMenu(false);
            Game1.activeClickableMenu = new SettingsMenu(bounds);
            Logger.LogVerbose("Prestige Menu - Loaded Settings Menu.");
        }

        private void InitiateProfessionButtons()
        {
            Logger.LogVerbose("Prestige menu - Initiating profession buttons...");
            _xSpaceAvailableForProfessionButtons = xPositionOnScreen + width - spaceToClearSideBorder * 2 - _leftProfessionStartingXLocation;
            InitiateLevelFiveProfessionButtons();
            InitiateLevelTenProfessionButtons();
            Logger.LogVerbose("Prestige menu - Profession button initiated.");
        }

        private static int ProfessionButtonWidth(Profession profession)
        {
            return Game1.dialogueFont.MeasureString(string.Join(Environment.NewLine, profession.DisplayName.Split(' '))).X.Ceiling() + Offset * 2;
        }

        private void InitiateLevelFiveProfessionButtons()
        {
            Logger.LogVerbose("Prestige menu - Initiating level 5 profession buttons...");
            var leftProfessionButtonXCenter = _leftProfessionStartingXLocation + _xSpaceAvailableForProfessionButtons / 4;
            var rightProfessionButtonXCenter = _leftProfessionStartingXLocation + (_xSpaceAvailableForProfessionButtons * .75d).Floor();
            var firstProfession = _skill.Professions.Where(x => x is TierOneProfession).First();

            _professionButtons.Add(new MinimalistProfessionButton
            {

                Bounds = new Rectangle(leftProfessionButtonXCenter - ProfessionButtonWidth(firstProfession) / 2, (int)_professionButtonRowStartLocation.Y, ProfessionButtonWidth(firstProfession), ProfessionButtonHeight(firstProfession)),
                CanBeAfforded = _prestige.PrestigePoints >= PerSaveOptions.Instance.CostOfTierOnePrestige,
                IsObtainable = true,
                Selected = _prestige.PrestigeProfessionsSelected.Contains(firstProfession.Id),
                Profession = firstProfession
            });
            var secondProfession = _skill.Professions.Where(x => x is TierOneProfession).Skip(1).First();
            _professionButtons.Add(new MinimalistProfessionButton
            {

                Bounds = new Rectangle(rightProfessionButtonXCenter - ProfessionButtonWidth(secondProfession) / 2, (int)_professionButtonRowStartLocation.Y, ProfessionButtonWidth(secondProfession), ProfessionButtonHeight(secondProfession)),
                CanBeAfforded = _prestige.PrestigePoints >= PerSaveOptions.Instance.CostOfTierOnePrestige,
                IsObtainable = true,
                Selected = _prestige.PrestigeProfessionsSelected.Contains(secondProfession.Id),
                Profession = secondProfession
            });
            Logger.LogVerbose("Prestige menu - Level 5 profession buttons initiated.");
        }

        private void InitiateLevelTenProfessionButtons()
        {
            Logger.LogVerbose("Prestige menu - Initiating level 10 profession buttons...");
            var buttonCenterIndex = 1;
            var canBeAfforded = _prestige.PrestigePoints >= PerSaveOptions.Instance.CostOfTierTwoPrestige;
            foreach (var profession in _skill.Professions.Where(x => x is TierTwoProfession)
            )
            {
                var tierTwoProfession = (TierTwoProfession)profession;
                _professionButtons.Add(new MinimalistProfessionButton
                {
                    Bounds = new Rectangle(_leftProfessionStartingXLocation + (_xSpaceAvailableForProfessionButtons * (buttonCenterIndex / 8d)).Floor() - ProfessionButtonWidth(profession) / 2, (int)_professionButtonRowStartLocation.Y + GetRowHeight<TierOneProfession>() + _rowPadding, ProfessionButtonWidth(profession), ProfessionButtonHeight(profession)),
                    CanBeAfforded = canBeAfforded,
                    IsObtainable = _prestige.PrestigeProfessionsSelected.Contains(tierTwoProfession.TierOneProfession.Id),
                    Selected = _prestige.PrestigeProfessionsSelected.Contains(tierTwoProfession.Id),
                    Profession = tierTwoProfession
                });
                buttonCenterIndex += 2;
            }
            Logger.LogVerbose("Prestige menu - Level 10 profession buttons initiated.");
        }

        private void UpdateProfessionButtonAvailability()
        {
            foreach (var button in _professionButtons)
            {
                button.CanBeAfforded = _prestige.PrestigePoints >= PerSaveOptions.Instance.CostOfTierTwoPrestige || button.Profession is TierOneProfession && _prestige.PrestigePoints >= PerSaveOptions.Instance.CostOfTierOnePrestige;
                button.IsObtainable = button.Profession is TierOneProfession || _prestige.PrestigeProfessionsSelected.Contains(((TierTwoProfession)button.Profession).TierOneProfession.Id);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (_debounceTimer > 0)
                _debounceTimer--;

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            upperRightCloseButton?.draw(spriteBatch);
            DrawSettingsButton(spriteBatch);
            DrawHeader(spriteBatch);
            DrawPrestigePoints(spriteBatch);
            DrawPrestigePointBonus(spriteBatch);
            _prestigeButton.Draw(spriteBatch);
            DrawLevelFiveProfessionCost(spriteBatch);
            if (!_professionButtons.Any()) InitiateProfessionButtons();
            UpdateProfessionButtonAvailability();
            DrawProfessionButtons(spriteBatch);
            DrawLevelTenProfessionCost(spriteBatch);
            DrawButtonHoverText(spriteBatch);
            Mouse.DrawCursor(spriteBatch);
        }

        private void DrawSettingsButton(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.mouseCursors, new Vector2(_settingsButton.Bounds.X, _settingsButton.Bounds.Y), _settingsButton.SourceRectangle, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.0001f);

        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            var title = $"{_skill.Type.Name} Prestige";
            DrawSkillIcon(spriteBatch, new Vector2(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4));
            spriteBatch.DrawString(Game1.dialogueFont, title, new Vector2(xPositionOnScreen + width / 2 - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4), Game1.textColor);
            DrawSkillIcon(spriteBatch, new Vector2(xPositionOnScreen + width - spaceToClearSideBorder - borderWidth - Game1.tileSize, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4));
            drawHorizontalPartition(spriteBatch, yPositionOnScreen + (Game1.tileSize * 2.5).Floor());
        }

        private void DrawSkillIcon(SpriteBatch spriteBatch, Vector2 location)
        {
            Utility.drawWithShadow(spriteBatch, _skill.SkillIconTexture, location, _skill.SourceRectangleForSkillIcon, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, false, 0.88f);
        }

        private void DrawPrestigePoints(SpriteBatch spriteBatch)
        {
            const string pointText = "Prestige Points:";
            var yOffset = 5 * Game1.pixelZoom;
            var yLocation = yPositionOnScreen + yOffset + (Game1.tileSize*3.15).Floor();
            var textLocation = new Vector2(xPositionOnScreen + spaceToClearSideBorder / 2 + borderWidth, yLocation);
            spriteBatch.DrawString(Game1.dialogueFont, pointText, textLocation, Game1.textColor);
            var xPadding = (Game1.pixelZoom*4.25).Floor();
            var prestigePointWidth = (NumberSprite.getWidth(PerSaveOptions.Instance.CostOfTierTwoPrestige) * 3.0).Ceiling();
            var pointNumberLocation = new Vector2(textLocation.X + Game1.dialogueFont.MeasureString(pointText).X + xPadding + prestigePointWidth, textLocation.Y + Game1.pixelZoom * 5);
            NumberSprite.draw(_prestige.PrestigePoints, spriteBatch, pointNumberLocation, Color.SandyBrown, 1f, .85f, 1f, 0);
            _professionButtonRowStartLocation = new Vector2(textLocation.X, textLocation.Y + Game1.dialogueFont.MeasureString(pointText).Y + _rowPadding);
            _prestigePointBonusLocation = new Vector2(pointNumberLocation.X + prestigePointWidth + xPadding, yLocation);
        }

        private void DrawPrestigePointBonus(SpriteBatch spriteBatch)
        {
            if(PerSaveOptions.Instance.UseExperienceMultiplier) spriteBatch.DrawString(Game1.dialogueFont, $"({(_prestige.PrestigePoints * PerSaveOptions.Instance.ExperienceMultiplier * 100).Floor()}% XP bonus)", _prestigePointBonusLocation, Game1.textColor);
        }

        private void DrawLevelFiveProfessionCost(SpriteBatch spriteBatch)
        {
            var costTextLocation = _professionButtonRowStartLocation;
            costTextLocation.Y += CostTextYOffset<TierOneProfession>();
            spriteBatch.DrawString(Game1.dialogueFont, CostText, costTextLocation, Game1.textColor);
            var pointNumberLocation = new Vector2(costTextLocation.X + Game1.dialogueFont.MeasureString(CostText).X + (NumberSprite.getWidth(PerSaveOptions.Instance.CostOfTierOnePrestige) * 3.0).Ceiling(), costTextLocation.Y + Game1.pixelZoom * 5);
            NumberSprite.draw(PerSaveOptions.Instance.CostOfTierOnePrestige, spriteBatch, pointNumberLocation, Color.SandyBrown, 1f, .85f, 1f, 0);
            if (_leftProfessionStartingXLocation == 0) _leftProfessionStartingXLocation = pointNumberLocation.X.Ceiling() + NumberSprite.digitWidth;
        }

        private void DrawLevelTenProfessionCost(SpriteBatch spriteBatch)
        {
            var firstRowBottomLocation = _professionButtonRowStartLocation.Y + GetRowHeight<TierOneProfession>();
            var costTextLocation = new Vector2(_professionButtonRowStartLocation.X, firstRowBottomLocation + CostTextYOffset<TierTwoProfession>() + _rowPadding);
            spriteBatch.DrawString(Game1.dialogueFont, CostText, costTextLocation, Game1.textColor);
            var pointNumberLocation = new Vector2(costTextLocation.X + Game1.dialogueFont.MeasureString(CostText).X + (NumberSprite.getWidth(PerSaveOptions.Instance.CostOfTierTwoPrestige) *3.0).Ceiling(), costTextLocation.Y + Game1.pixelZoom * 5);
            NumberSprite.draw(PerSaveOptions.Instance.CostOfTierTwoPrestige, spriteBatch, pointNumberLocation, Color.SandyBrown, 1f, .85f, 1f, 0);
        }

        private void DrawProfessionButtons(SpriteBatch spriteBatch)
        {
            foreach (var button in _professionButtons)
            {
                button.Draw(spriteBatch);
            }
        }

        private void DrawButtonHoverText(SpriteBatch spriteBatch)
        {
            foreach (var button in _professionButtons)
            {
                button.DrawHoverText(spriteBatch);
            }
            _prestigeButton.DrawHoverText(spriteBatch);
            _settingsButton.DrawHoverText(spriteBatch);
        }
    }
}
