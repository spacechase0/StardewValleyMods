using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Dialogs;
using SkillPrestige.Menus.Elements.Buttons;
using SkillPrestige.Professions;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus
{
    /// <summary>
    /// Decorates the Level Up Menu with a prestiged! indicator on prestiged professions.
    /// </summary>
    public class LevelUpMenuDecorator<T> : IClickableMenu, IInputHandler where T : IClickableMenu
    {
        private readonly Skill _currentSkill;
        private readonly int _currentLevel;
        private bool _isRightSideOfTree;
        private bool _uiInitiated;
        private bool _drawToggleSwitch;
        private bool _drawLeftPrestigedIndicator;
        private bool _drawRightPrestigedIndicator;
        private TextureButton _levelTenToggleButton;
        private readonly T _internalMenu;
        private readonly string _professionsToChooseInternalName;
        private readonly string _leftProfessionDescriptionInternalName;
        private readonly string _rightProfessionDescriptionInternalName;
        private readonly Func<int, List<string>> _getProfessionDescription;

        private Rectangle MessageDialogBounds
        {
            get
            {
                var viewport = Game1.graphics.GraphicsDevice.Viewport;
                var screenXCenter = (int)(viewport.Width * (1.0 / Game1.options.zoomLevel)) / 2;
                var screenYCenter = (int)(viewport.Height * (1.0 / Game1.options.zoomLevel)) / 2;
                var dialogWidth = Game1.tileSize * 10;
                var dialogHeight = Game1.tileSize * 8;
                var xLocation = screenXCenter - _internalMenu.width / 2;
                var yLocation = screenYCenter - _internalMenu.height / 2;
                return new Rectangle(xLocation, yLocation, dialogWidth, dialogHeight);
            }
        }
        private Rectangle ExtraTallMessageDialogBounds
        {
            get
            {
                var extraTallRectangle = MessageDialogBounds;
                extraTallRectangle.Height += Game1.tileSize * 4;
                return extraTallRectangle;
            }
        }


        public LevelUpMenuDecorator(Skill skill, int level, T internalMenu, string professionsToChooseInternalName, string leftProfessionDescriptionInternalName, string rightProfessionDescriptionInternalName, Func<int, List<string>> getProfessionDescription)
        {
            _internalMenu = internalMenu;
            _currentSkill = skill;
            _currentLevel = level;
            _professionsToChooseInternalName = professionsToChooseInternalName;
            _leftProfessionDescriptionInternalName = leftProfessionDescriptionInternalName;
            _rightProfessionDescriptionInternalName = rightProfessionDescriptionInternalName;
            _getProfessionDescription = getProfessionDescription;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            _internalMenu.receiveRightClick(x, y, playSound);
        }

        public override void update(GameTime time)
        {
            _internalMenu.update(time);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            _internalMenu.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            _internalMenu.draw(spriteBatch);
            if (!_uiInitiated) InitiateUi();
            DecorateUi(spriteBatch);
            drawMouse(spriteBatch);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            _levelTenToggleButton?.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            _levelTenToggleButton?.OnButtonPressed(e, isClick);
        }

        private void InitiateUi()
        {
            if (_uiInitiated) return;
            _uiInitiated = true;
            Logger.LogVerbose("Level Up Menu - initializing UI...");
            var prestigeData = PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == _currentSkill.Type);
            var prestigedProfessionsForThisSkillAndLevel =
                _currentSkill.Professions.Where(
                    x =>
                        prestigeData.PrestigeProfessionsSelected.Contains(x.Id) &&
                        x.LevelAvailableAt == _currentLevel).ToList();
            var professionsToChooseFrom = _currentSkill.Professions.Where(x => x.LevelAvailableAt == _currentLevel).ToList();

            if (_currentLevel == 5)
            {
                if (!prestigedProfessionsForThisSkillAndLevel.Any())
                {
                    Logger.LogVerbose(
                        "Level Up Menu - No prestiged professions found for this skill/level combination.");
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count == 1)
                {
                    Logger.LogInformation(
                        "Level Up Menu - One level 5 prestiged profession found, automatically selecting the other.");
                    var professionToAdd =
                        professionsToChooseFrom.First(x => !prestigedProfessionsForThisSkillAndLevel.Contains(x));
                    Game1.player.professions.Add(professionToAdd.Id);
                    professionToAdd.SpecialHandling?.ApplyEffect();
                    exitThisMenu(false);
                    Game1.activeClickableMenu = new LevelUpMessageDialogWithProfession(MessageDialogBounds,
                        $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel} and gained a profession!",
                        _currentSkill, professionToAdd);
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count >= 2)
                {
                    Logger.LogInformation(
                        "Level Up Menu - Both available level 5 professions are already prestiged.");
                    exitThisMenu(false);
                    Game1.activeClickableMenu = new LevelUpMessageDialog(MessageDialogBounds,
                        $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel}!", _currentSkill);
                    return;
                }
            }
            if (_currentLevel != 10) return;

            var levelFiveProfessionsCount =
                Game1.player.professions.Intersect(
                    _currentSkill.Professions.Where(x => x is TierOneProfession).Select(x => x.Id)).Count();
            if (levelFiveProfessionsCount == 1)
            {
                if (!prestigedProfessionsForThisSkillAndLevel.Any())
                {
                    Logger.LogVerbose(
                        "Level Up Menu - No prestiged professions found for this skill/level combination.");
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count == 1)
                {
                    Logger.LogInformation(
                        "Level Up Menu - One level 10 prestiged profession found for only one available level 5 skill (cheater!), automatically selecting the other.");
                    var tierOneProfession =
                        ((TierTwoProfession)prestigedProfessionsForThisSkillAndLevel.First()).TierOneProfession;
                    var professionToAdd =
                        professionsToChooseFrom.First(
                            x =>
                                (x as TierTwoProfession)?.TierOneProfession == tierOneProfession &&
                                !prestigedProfessionsForThisSkillAndLevel.Contains(x));
                    Game1.player.professions.Add(professionToAdd.Id);
                    professionToAdd.SpecialHandling?.ApplyEffect();
                    exitThisMenu(false);
                    Game1.activeClickableMenu = new LevelUpMessageDialogWithProfession(ExtraTallMessageDialogBounds,
                        $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel} and gained a profession! {Environment.NewLine} You may now prestige this skill again!",
                        _currentSkill, professionToAdd);
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count < 2) return;
                Logger.LogInformation(
                    "Level Up Menu - Only one level 5 profession found with both level 10 professions already prestiged (cheater!).");
                exitThisMenu(false);
                Game1.activeClickableMenu = new LevelUpMessageDialog(MessageDialogBounds,
                    $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel}!  {Environment.NewLine} You may now prestige this skill again!",
                    _currentSkill);
            }
            else
            {
                if (prestigedProfessionsForThisSkillAndLevel.Count <= 2)
                {
                    Logger.LogInformation(
                        "Level Up Menu - Two or less prestiged level 10 professions found for this skill, with more than one level 5 profession found.");
                    if (prestigedProfessionsForThisSkillAndLevel.Intersect(professionsToChooseFrom.Take(2)).Count() == 2)
                    {
                        Logger.LogInformation("Level Up Menu - All of one level 10 profession branch found, switching to remaining menu.");
                        ToggleLevelTenMenu();
                        return;
                    }
                    if (prestigedProfessionsForThisSkillAndLevel.Intersect(professionsToChooseFrom.Skip(2).Take(2)).Count() == 2)
                    {
                        Logger.LogInformation("Level Up Menu - All of one level 10 profession branch found, leaving at default menu.");
                        return;
                    }
                    Logger.LogInformation(
                            "Level Up Menu - Both level up menus found as viable, enabling user side toggle.");
                    SetupLevelTenToggleButton();
                    _drawToggleSwitch = true;
                    _drawLeftPrestigedIndicator =
                        prestigedProfessionsForThisSkillAndLevel.Contains(
                            professionsToChooseFrom.Skip(_isRightSideOfTree == false ? 0 : 2).First());
                    _drawRightPrestigedIndicator =
                        prestigedProfessionsForThisSkillAndLevel.Contains(
                            professionsToChooseFrom.Skip(_isRightSideOfTree == false ? 1 : 3).First());
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count == 3)
                {
                    Logger.LogInformation(
                        "Level Up Menu - All but one level 10 profession found, selecting remaining profession.");
                    var professionToAdd =
                        professionsToChooseFrom.First(x => !prestigedProfessionsForThisSkillAndLevel.Contains(x));
                    Game1.player.professions.Add(professionToAdd.Id);
                    professionToAdd.SpecialHandling?.ApplyEffect();
                    exitThisMenu(false);
                    Game1.activeClickableMenu = new LevelUpMessageDialogWithProfession(ExtraTallMessageDialogBounds,
                        $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel} and gained a profession!  {Environment.NewLine} You may now prestige this skill again!",
                        _currentSkill, professionToAdd);
                    return;
                }
                if (prestigedProfessionsForThisSkillAndLevel.Count < 4) return;
                Logger.LogInformation("Level Up Menu - All professions already prestiged for this skill.");
                exitThisMenu(false);
                Game1.activeClickableMenu = new LevelUpMessageDialog(ExtraTallMessageDialogBounds,
                        $"You levelled your {_currentSkill.Type.Name} skill to level {_currentLevel}!  {Environment.NewLine} Congratulations! You have prestiged all of your professions and reached level 10 again! You may continue to earn prestige points if you wish, as more prestige options are coming soon!",
                        _currentSkill);
            }
        }

        private void DecorateUi(SpriteBatch spriteBatch)
        {
            if (_drawToggleSwitch) _levelTenToggleButton.Draw(spriteBatch);
            DrawPrestigedIndicators(spriteBatch, _drawLeftPrestigedIndicator, _drawRightPrestigedIndicator);

        }

        private void DrawPrestigedIndicators(SpriteBatch spriteBatch, bool left, bool right)
        {
            const string text = "Prestiged!";
            var textPadding = Game1.tileSize;
            var yPositionOfText = _internalMenu.yPositionOnScreen + _internalMenu.height + textPadding;
            if (left) spriteBatch.DrawString(Game1.dialogueFont, text, new Vector2(_internalMenu.xPositionOnScreen + _internalMenu.width / 4 - Game1.dialogueFont.MeasureString(text).X / 2, yPositionOfText), Color.LimeGreen);
            if (right) spriteBatch.DrawString(Game1.dialogueFont, text, new Vector2(_internalMenu.xPositionOnScreen + _internalMenu.width * 3 / 4 - Game1.dialogueFont.MeasureString(text).X / 2, yPositionOfText), Color.LimeGreen);
        }

        private void SetupLevelTenToggleButton()
        {
            if (_levelTenToggleButton != null) return;
            Logger.LogInformation("Level Up Menu - initiating level 10 toggle button...");
            var position = new Vector2(_internalMenu.xPositionOnScreen + _internalMenu.width + Game1.tileSize, _internalMenu.yPositionOnScreen);
            var bounds = new Rectangle(position.X.Floor(), position.Y.Floor(), Game1.tileSize, Game1.tileSize);
            _levelTenToggleButton = new TextureButton(bounds, Game1.mouseCursors, new Rectangle(0, 192, 64, 64), ToggleLevelTenMenu, "More professions...");
            Logger.LogInformation("Level Up Menu - Level 10 toggle button initiated.");
        }

        private void ToggleLevelTenMenu()
        {
            Logger.LogInformation("Toggling level 10 menu...");
            _isRightSideOfTree = !_isRightSideOfTree;
            var professionsToChoose = _currentSkill.Professions.Where(x => x is TierTwoProfession).Skip(_isRightSideOfTree ? 2 : 0).ToList();
            _internalMenu.SetInstanceField(_professionsToChooseInternalName, professionsToChoose.Select(x => x.Id).ToList());
            _internalMenu.SetInstanceField(_leftProfessionDescriptionInternalName, _getProfessionDescription.Invoke(professionsToChoose[0].Id));
            _internalMenu.SetInstanceField(_rightProfessionDescriptionInternalName, _getProfessionDescription.Invoke(professionsToChoose[1].Id));
            var prestigeData = PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == _currentSkill.Type);
            var prestigedProfessionsForThisSkillAndLevel = _currentSkill.Professions.Where(x => prestigeData.PrestigeProfessionsSelected.Contains(x.Id) && x.LevelAvailableAt == _currentLevel).ToList();
            var professionsToChooseFrom = _currentSkill.Professions.Where(x => x.LevelAvailableAt == _currentLevel).ToList();
            _drawLeftPrestigedIndicator = prestigedProfessionsForThisSkillAndLevel.Contains(professionsToChooseFrom.Skip(_isRightSideOfTree == false ? 0 : 2).First());
            _drawRightPrestigedIndicator = prestigedProfessionsForThisSkillAndLevel.Contains(professionsToChooseFrom.Skip(_isRightSideOfTree == false ? 1 : 3).First());
        }

    }
}
