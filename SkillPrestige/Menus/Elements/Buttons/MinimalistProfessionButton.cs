using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Logging;
using SkillPrestige.Professions;
using StardewModdingAPI.Events;
using StardewValley;

namespace SkillPrestige.Menus.Elements.Buttons
{
    /// <summary>
    /// Represents a profession button on the prestige menu. 
    /// Used to allow the user to choose to permanently obtain a profession.
    /// </summary>
    public class MinimalistProfessionButton : Button
    {
        public MinimalistProfessionButton()
        {
            TitleTextFont = Game1.dialogueFont;
        }

        protected override Texture2D ButtonTexture
        {
            get { return ProfessionButtonTexture; }
            set { ProfessionButtonTexture = value; }
        }

        public static Texture2D ProfessionButtonTexture { get; set; }

        public Profession Profession { get; set; }
        public bool Selected { private get; set; }
        public bool IsObtainable { private get; set; }
        public bool CanBeAfforded { private get; set; }
        private bool IsDisabled => Selected || !IsObtainable || !CanBeAfforded;
        private Color DrawColor => IsDisabled ? Color.Gray : Color.White;
        private Rectangle _checkmarkSourceRectangle = new Rectangle(0,0, 64,64);

        private static int TextYOffset => 4 * Game1.pixelZoom;
        private Vector2 _iconLocation;

        protected override string HoverText => $"{HoverTextPrefix}{Environment.NewLine}{Environment.NewLine}{(Profession?.EffectText == null ? string.Empty : string.Join(Environment.NewLine, Profession.EffectText))}";

        private string HoverTextPrefix => Selected
                                            ? $"You already permanently have the {Profession.DisplayName} profession."
                                            : IsObtainable
                                                ? CanBeAfforded
                                                    ? $"Click to permanently obtain the {Profession.DisplayName} profession."
                                                    : $"You cannot afford this profession,{Environment.NewLine}you need {GetPrestigeCost()} prestige point(s) in this skill to purchase it."
                                                : $"This profession is not available to obtain permanently until the {Environment.NewLine}{(Profession as TierTwoProfession)?.TierOneProfession.DisplayName} profession has been permanently obtained.";

        protected override string Text => string.Join(Environment.NewLine,Profession.DisplayName.Split(' '));

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(ButtonTexture, Bounds, DrawColor);
            DrawIcon(spriteBatch);
            DrawText(spriteBatch);
            DrawCheckmark(spriteBatch);
        }

        private void DrawIcon(SpriteBatch spriteBatch)
        {
            var locationOfIconRelativeToButton = new Vector2(Bounds.Width / 2 - Profession.IconSourceRectangle.Width * Game1.pixelZoom / 2, TextYOffset);
            var buttonLocation = new Vector2(Bounds.X, Bounds.Y);
            _iconLocation = buttonLocation + locationOfIconRelativeToButton;
            spriteBatch.Draw(Profession.Texture, _iconLocation, Profession.IconSourceRectangle, DrawColor, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            var buttonXCenter = Bounds.Width/2;
            var textCenter = TitleTextFont.MeasureString(Text).X / 2;
            var textXLocationRelativeToButton = buttonXCenter - textCenter;
            var textYLocationRelativeToButton = TextYOffset*2 + Profession.IconSourceRectangle.Height*Game1.pixelZoom;
            var locationOfTextRelativeToButton = new Vector2(textXLocationRelativeToButton, textYLocationRelativeToButton);
            DrawTitleText(spriteBatch, locationOfTextRelativeToButton);
        }

        private void DrawCheckmark(SpriteBatch spriteBatch)
        {
            if (!Selected) return;
            var locationOfCheckmarkRelativeToButton = new Vector2(Bounds.Width - _checkmarkSourceRectangle.Width*Game1.pixelZoom/8, 0);
            var buttonLocation = new Vector2(Bounds.X, Bounds.Y);
            var checkmarkLocation = buttonLocation + locationOfCheckmarkRelativeToButton;
            spriteBatch.Draw(SkillPrestigeMod.CheckmarkTexture, checkmarkLocation, _checkmarkSourceRectangle, Color.White, 0f, Vector2.Zero, Game1.pixelZoom / 4f, SpriteEffects.None, 1f);
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
                Prestige.AddPrestigeProfession(Profession.Id);
                Selected = true;
            }
        }

        private int GetPrestigeCost()
        {
            var tier = Profession.LevelAvailableAt / 5;
            switch (tier)
            {
                case 1:
                    return PerSaveOptions.Instance.CostOfTierOnePrestige;
                case 2:
                    return PerSaveOptions.Instance.CostOfTierTwoPrestige;
                default:
                    Logger.LogWarning("Tier for profession not found, defaulting to a cost of 1.");
                    return 1;
            }
        }
    }
}
