using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkillPrestige.Professions;
using StardewValley;
using System.Linq;
using SkillPrestige.Menus.Elements.Buttons;

namespace SkillPrestige.Menus.Dialogs
{
    /// <summary>
    /// represents a message dialog box for skil level up where a specific profession is granted.
    /// </summary>
    internal class LevelUpMessageDialogWithProfession : LevelUpMessageDialog
    {
        private readonly Profession _profession;
        private static int TextPadding => 4 * Game1.pixelZoom;
        private int SectionWidth => (width - spaceToClearSideBorder * 2) / 2 - TextPadding;
        private Vector2 _professionIconLocation;


        public LevelUpMessageDialogWithProfession(Rectangle bounds, string message, Skill skill, Profession profession) : base(bounds, message, skill)
        {
            _profession = profession;
        }

        protected override void DrawMessage(SpriteBatch spriteBatch)
        {
            var xLocationOfMessage = xPositionOnScreen + spaceToClearSideBorder * 2 + TextPadding;
            var yLocationOfMessage = YPostionOfHeaderPartition + spaceToClearTopBorder / 2;
            DrawMessage(spriteBatch, Game1.dialogueFont, new Vector2(xLocationOfMessage, yLocationOfMessage), SectionWidth);
        }

        protected override void DrawDecorations(SpriteBatch spriteBatch)
        {
            base.DrawDecorations(spriteBatch);
            DrawProfession(spriteBatch);
        }

        private void DrawProfession(SpriteBatch spriteBatch)
        {
            var xLocationOfProfessionBox = xPositionOnScreen + SectionWidth + TextPadding;
            var yLocationOfProfessionBox = YPostionOfHeaderPartition + spaceToClearTopBorder / 2 + TextPadding;
            var professionBoxBounds = new Rectangle(xLocationOfProfessionBox, yLocationOfProfessionBox, SectionWidth, (Game1.tileSize * 3.5).Ceiling());
            DrawProfessionBox(spriteBatch, professionBoxBounds);
        }


        private void DrawProfessionBox(SpriteBatch spriteBatch, Rectangle bounds)
        {

            spriteBatch.Draw(MinimalistProfessionButton.ProfessionButtonTexture, bounds, Color.White);
            DrawProfessionTitleText(spriteBatch, bounds);
            DrawProfessionIcon(spriteBatch, bounds);
            DrawProfessionEffectText(spriteBatch, bounds);
        }

        private void DrawProfessionTitleText(SpriteBatch spriteBatch, Rectangle professionBounds)
        {
            var textLocation = new Vector2(professionBounds.X + TextPadding, professionBounds.Y + TextPadding);
            spriteBatch.DrawString(Game1.dialogueFont, _profession.DisplayName, textLocation, Game1.textColor);
        }

        private void DrawProfessionIcon(SpriteBatch spriteBatch, Rectangle professionBounds)
        {
            var locationOfIconRelativeToButton = new Vector2(professionBounds.Width - (TextPadding + _profession.IconSourceRectangle.Width * Game1.pixelZoom), TextPadding);
            var buttonLocation = new Vector2(professionBounds.X, professionBounds.Y);
            _professionIconLocation = buttonLocation + locationOfIconRelativeToButton;
            spriteBatch.Draw(_profession.Texture, _professionIconLocation, _profession.IconSourceRectangle, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
        }

        private void DrawProfessionEffectText(SpriteBatch spriteBatch, Rectangle professionBounds)
        {
            var bottomOfIcon = _professionIconLocation.Y + _profession.IconSourceRectangle.Height * Game1.pixelZoom;
            var nextLineLocation = new Vector2(professionBounds.X + TextPadding, bottomOfIcon + TextPadding);
            var effectTextFont = Game1.smallFont;
            spriteBatch.DrawString(effectTextFont, string.Join("\n", _profession.EffectText.Select(effect => effect.WrapText(effectTextFont, professionBounds.Width - TextPadding * 2))), nextLineLocation, Game1.textColor);
        }
    }
}
