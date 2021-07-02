using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Logging;
using SkillPrestige.Menus.Dialogs;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SkillPrestige.Menus.Elements.Buttons
{
    /// <summary>
    /// Represents a prestige button inside the prestige menu.
    /// </summary>
    public class PrestigeButton : Button
    {

        public PrestigeButton(bool isDisabled, Skill skill)
        {
            TitleTextFont = Game1.dialogueFont;
            IsDisabled = isDisabled;
            Skill = skill;
        }

        private Skill Skill { get; }
        private bool IsDisabled { get; }
        private Color DisplayColor => IsDisabled ? Color.Gray : Color.White;


        protected override string HoverText
        {
            get
            {
                if (!IsDisabled)
                {
                    return $"Click to prestige your {Skill?.Type?.Name} skill.{Environment.NewLine}" 
                        + $"{(PerSaveOptions.Instance.UseExperienceMultiplier ? $"Next XP Bonus: {((PrestigeSaveData.CurrentlyLoadedPrestigeSet.Prestiges.Single(x => x.SkillType == Skill?.Type).PrestigePoints + PerSaveOptions.Instance.PointsPerPrestige) * PerSaveOptions.Instance.ExperienceMultiplier * 100).Floor()}%" : string.Empty)}";
                }
                if (!PerSaveOptions.Instance.PainlessPrestigeMode)
                {
                    return $"You must reach level 10 in this skill and then{Environment.NewLine}sleep at least once in order to prestige this skill.";
                }
                var currentExperience = Game1.player.experiencePoints[Skill.Type.Ordinal];
                var experienceNeeded = PerSaveOptions.Instance.ExperienceNeededPerPainlessPrestige;
                var availableExperience =
                    currentExperience - 15000; //Remove what it takes to get to level 10 in the first place.
                var remainingExperienceNeeded = experienceNeeded - availableExperience;
                return $"You do not have enough experience to prestige this skill.{Environment.NewLine}You need {remainingExperienceNeeded} more experience points to prestige this skill.";
            }
        }

        protected override string Text => "Prestige";

        public override void Draw(SpriteBatch spriteBatch)  
        {
            spriteBatch.Draw(ButtonTexture, Bounds, DisplayColor);
            DrawTitleText(spriteBatch);
        }

        /// <summary>Raised when the player begins hovering over the button.</summary>
        protected override void OnMouseHovered()
        {
            base.OnMouseHovered();
            if (IsDisabled)
                return;

            Game1.playSound("smallSelect");
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="e">The event data.</param>
        /// <param name="isClick">Whether the button press is a click.</param>
        public override void OnButtonPressed(ButtonPressedEventArgs e, bool isClick)
        {
            base.OnButtonPressed(e, isClick);
            if (IsDisabled)
                return;

            if (isClick && IsHovered)
            {
                Game1.playSound("bigSelect");
                if (PerSaveOptions.Instance.PainlessPrestigeMode)
                {
                    Game1.activeClickableMenu.exitThisMenuNoSound();
                    Prestige.PrestigeSkill(Skill);
                    return;
                }
                //Magic numbers for tile size multipliers have been determined through trial and error.
                var dialogWidth = Game1.tileSize * 12;
                var dialogHeight = Game1.tileSize * 6;

                var viewport = Game1.graphics.GraphicsDevice.Viewport;
                var screenXCenter = (int)(viewport.Width * (1.0 / Game1.options.zoomLevel)) / 2;
                var screenYCenter = (int)(viewport.Height * (1.0 / Game1.options.zoomLevel)) / 2;
                var dialogXCenter = (dialogWidth + IClickableMenu.borderWidth * 2) / 2;
                var dialogYCenter = (dialogHeight + IClickableMenu.borderWidth * 2) / 2;
                var bounds = new Rectangle(screenXCenter - dialogXCenter, screenYCenter - dialogYCenter,
                    dialogWidth + IClickableMenu.borderWidth * 2, dialogHeight + IClickableMenu.borderWidth * 2);
                Logger.LogVerbose($"{Skill.Type.Name} skill prestige attempted.");
                var message = $"Are you sure you wish to prestige your {Skill.Type.Name} skill? This cannot be undone and will revert you back to level 0 {Skill.Type.Name}. All associated benefits {(PerSaveOptions.Instance.ResetRecipesOnPrestige ? "and" : "except for")} crafting/cooking recipes will be lost.";
                Game1.activeClickableMenu.exitThisMenuNoSound();
                Game1.activeClickableMenu = new WarningDialog(bounds, message, () => { Prestige.PrestigeSkill(Skill); },
                    () => { });
            }
        }
    }
}