using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace SkillPrestige.Menus.Dialogs
{
    /// <summary>
    /// Represents a message dialog for skill level up messages.
    /// </summary>
    internal class LevelUpMessageDialog : MessageDialog
    {
        private readonly Skill _skill;
        protected readonly int YPostionOfHeaderPartition;

        public LevelUpMessageDialog(Rectangle bounds, string message, Skill skill)
            : base(bounds, message)
        {
            _skill = skill;
            YPostionOfHeaderPartition = yPositionOnScreen + (Game1.tileSize*2.5).Floor();
        }

        protected override void DrawDecorations(SpriteBatch spriteBatch)
        {
            base.DrawDecorations(spriteBatch);
            DrawLevelUpHeader(spriteBatch);
        }

        private void DrawLevelUpHeader(SpriteBatch spriteBatch)
        {
            var title = $"{_skill.Type.Name} Level Up";
            DrawSkillIcon(spriteBatch, new Vector2(xPositionOnScreen + spaceToClearSideBorder + borderWidth, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4));
            spriteBatch.DrawString(Game1.dialogueFont, title, new Vector2(xPositionOnScreen + width / 2 - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4), Game1.textColor);
            DrawSkillIcon(spriteBatch, new Vector2(xPositionOnScreen + width - spaceToClearSideBorder - borderWidth - Game1.tileSize, yPositionOnScreen + spaceToClearTopBorder + Game1.tileSize / 4));
            drawHorizontalPartition(spriteBatch, yPositionOnScreen + (Game1.tileSize * 2.5).Floor());
        }

        private void DrawSkillIcon(SpriteBatch spriteBatch, Vector2 location)
        {
            Utility.drawWithShadow(spriteBatch, _skill.SkillIconTexture, location, _skill.SourceRectangleForSkillIcon, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, false, 0.88f);
        }

        protected override void DrawMessage(SpriteBatch spriteBatch)
        {
            var textPadding = 2 * Game1.pixelZoom;
            var xLocationOfMessage = xPositionOnScreen + borderWidth + textPadding;
            var yLocationOfMessage = YPostionOfHeaderPartition + spaceToClearTopBorder;
            DrawMessage(spriteBatch, Game1.dialogueFont, new Vector2(xLocationOfMessage, yLocationOfMessage), width - borderWidth * 2);
        }
    }
}
