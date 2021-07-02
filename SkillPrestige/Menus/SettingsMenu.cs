using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.InputHandling;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Elements.Buttons;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus
{
    /// <summary>
    /// Represents a menu where players can change their per-save settings.
    /// </summary>
    internal class SettingsMenu : IClickableMenu, IInputHandler
    {
        private int _debounceTimer = 10;
        private bool _inputInitiated;
        
        private Checkbox _resetRecipesCheckbox;
        private Checkbox _useExperienceMultiplierCheckbox;
        private Checkbox _painlessPrestigeModeCheckbox;
        private IntegerEditor _tierOneCostEditor;
        private IntegerEditor _tierTwoCostEditor;
        private IntegerEditor _pointsPerPrestigeEditor;
        private IntegerEditor _experiencePerPainlessPrestigeEditor;

        public SettingsMenu(Rectangle bounds)
            : base(bounds.X, bounds.Y, bounds.Width, bounds.Height, true)
        {
            Logger.LogVerbose("New Settings Menu created.");
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="e">The event data.</param>
        public void OnCursorMoved(CursorMovedEventArgs e)
        {
            if (_debounceTimer > 0)
                return;

            _resetRecipesCheckbox.OnCursorMoved(e);
            _useExperienceMultiplierCheckbox.OnCursorMoved(e);
            _painlessPrestigeModeCheckbox.OnCursorMoved(e);

            _tierOneCostEditor.OnCursorMoved(e);
            _tierTwoCostEditor.OnCursorMoved(e);
            _pointsPerPrestigeEditor.OnCursorMoved(e);
            _experiencePerPainlessPrestigeEditor.OnCursorMoved(e);
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            if (_debounceTimer > 0)
                return;

            _resetRecipesCheckbox.OnButtonPressed(e, isClick);
            _useExperienceMultiplierCheckbox.OnButtonPressed(e, isClick);
            _painlessPrestigeModeCheckbox.OnButtonPressed(e, isClick);

            _tierOneCostEditor.OnButtonPressed(e, isClick);
            _tierTwoCostEditor.OnButtonPressed(e, isClick);
            _pointsPerPrestigeEditor.OnButtonPressed(e, isClick);
            _experiencePerPainlessPrestigeEditor.OnButtonPressed(e, isClick);
        }

        private void InitiateInput()
        {
            if (_inputInitiated) return;
            _inputInitiated = true;
            Logger.LogVerbose("Settings menu - intiating input.");
            var resetRecipeCheckboxBounds = new Rectangle(xPositionOnScreen + spaceToClearSideBorder * 3, yPositionOnScreen + (Game1.tileSize * 3.5).Floor(), 9*Game1.pixelZoom, 9 * Game1.pixelZoom);
            _resetRecipesCheckbox = new Checkbox(PerSaveOptions.Instance.ResetRecipesOnPrestige, "Reset Recipes upon prestige.", resetRecipeCheckboxBounds, ChangeRecipeReset);
            var padding = 4*Game1.pixelZoom;
            var useExperienceMultiplierCheckboxBounds = resetRecipeCheckboxBounds;
            useExperienceMultiplierCheckboxBounds.Y += resetRecipeCheckboxBounds.Height + padding;
            _useExperienceMultiplierCheckbox = new Checkbox(PerSaveOptions.Instance.UseExperienceMultiplier, "Use prestige points experience multiplier.", useExperienceMultiplierCheckboxBounds, ChangeUseExperienceMultiplier);
            var tierOneEditorLocation = new Vector2(useExperienceMultiplierCheckboxBounds.X, useExperienceMultiplierCheckboxBounds.Y + useExperienceMultiplierCheckboxBounds.Height + padding);
            _tierOneCostEditor = new IntegerEditor("Cost of Tier 1 Prestige", PerSaveOptions.Instance.CostOfTierOnePrestige, 1, 100, tierOneEditorLocation, ChangeTierOneCost);
            var tierTwoEditorLocation = tierOneEditorLocation;
            tierTwoEditorLocation.X += _tierOneCostEditor.Bounds.Width + padding;
            _tierTwoCostEditor = new IntegerEditor("Cost of Tier 2 Prestige", PerSaveOptions.Instance.CostOfTierTwoPrestige, 1, 100, tierTwoEditorLocation, ChangeTierTwoCost);
            var pointsPerPrestigeEditorLocation = tierTwoEditorLocation;
            pointsPerPrestigeEditorLocation.Y += _tierTwoCostEditor.Bounds.Height + padding;
            pointsPerPrestigeEditorLocation.X = _tierOneCostEditor.Bounds.X;
            _pointsPerPrestigeEditor = new IntegerEditor("Points Per Prestige", PerSaveOptions.Instance.PointsPerPrestige, 1, 100, pointsPerPrestigeEditorLocation, ChangePointsPerPrestige);
            var painlessPrestigeModeCheckboxBounds = new Rectangle(_pointsPerPrestigeEditor.Bounds.X, _pointsPerPrestigeEditor.Bounds.Y + _pointsPerPrestigeEditor.Bounds.Height + padding, 9 * Game1.pixelZoom, 9 * Game1.pixelZoom);
            const string painlessPrestigeModeCheckboxText = "Painless Prestige Mode";
            _painlessPrestigeModeCheckbox = new Checkbox(PerSaveOptions.Instance.PainlessPrestigeMode, painlessPrestigeModeCheckboxText, painlessPrestigeModeCheckboxBounds, ChangePainlessPrestigeMode);
            var experiencePerPainlessPrestigeEditorLocation = new Vector2(painlessPrestigeModeCheckboxBounds.X, painlessPrestigeModeCheckboxBounds.Y);
            experiencePerPainlessPrestigeEditorLocation.X += painlessPrestigeModeCheckboxBounds.Width + Game1.dialogueFont.MeasureString(painlessPrestigeModeCheckboxText).X + padding;
            _experiencePerPainlessPrestigeEditor = new IntegerEditor("Extra Experience Cost", PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige, 1000, 100000, experiencePerPainlessPrestigeEditorLocation, ChangeExperiencePerPainlessPrestige, 1000);
        }

        private static void ChangeRecipeReset(bool resetRecipes)
        {
            PerSaveOptions.Instance.ResetRecipesOnPrestige = resetRecipes;
            PerSaveOptions.Save();
        }

        private static void ChangeUseExperienceMultiplier(bool useExperienceMultiplier)
        {
            PerSaveOptions.Instance.UseExperienceMultiplier = useExperienceMultiplier;
            PerSaveOptions.Save();
        }

        private static void ChangeTierOneCost(int cost)
        {
            PerSaveOptions.Instance.CostOfTierOnePrestige = cost;
            PerSaveOptions.Save();
        }

        private static void ChangeTierTwoCost(int cost)
        {
            PerSaveOptions.Instance.CostOfTierTwoPrestige = cost;
            PerSaveOptions.Save();
        }

        private static void ChangePointsPerPrestige(int points)
        {
            PerSaveOptions.Instance.PointsPerPrestige = points;
            PerSaveOptions.Save();
        }

        private static void ChangePainlessPrestigeMode(bool usePainlessPrestigeMode)
        {
            PerSaveOptions.Instance.PainlessPrestigeMode = usePainlessPrestigeMode;
            PerSaveOptions.Save();
        }

        private static void ChangeExperiencePerPainlessPrestige(int experienceNeeded)
        {
            PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige = experienceNeeded;
            PerSaveOptions.Save();
        }

        public override void draw(SpriteBatch spriteBatch)
        {
            if (_debounceTimer > 0)
                _debounceTimer--;

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            upperRightCloseButton?.draw(spriteBatch);
            DrawHeader(spriteBatch);
            if (!_inputInitiated) InitiateInput();
            _resetRecipesCheckbox.Draw(spriteBatch);
            _useExperienceMultiplierCheckbox.Draw(spriteBatch);
            _tierOneCostEditor.Draw(spriteBatch);
            _tierTwoCostEditor.Draw(spriteBatch);
            _pointsPerPrestigeEditor.Draw(spriteBatch);
            _painlessPrestigeModeCheckbox.Draw(spriteBatch);
            _experiencePerPainlessPrestigeEditor.Draw(spriteBatch);
            Mouse.DrawCursor(spriteBatch);
        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            const string title = "Skill Prestige Settings";
            spriteBatch.DrawString(Game1.dialogueFont, title, new Vector2(xPositionOnScreen + width / 2 - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4), Game1.textColor);
            drawHorizontalPartition(spriteBatch, yPositionOnScreen + (Game1.tileSize * 2.5).Floor());
        }
    }
}
