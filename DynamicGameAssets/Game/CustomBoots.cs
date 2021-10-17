using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGABoots")]
    public partial class CustomBoots : Boots
    {
        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(BootsPackData data)
        {
            this.Name = this.Id;
            this.reloadData();
        }

        public bool LoadDisplayFields()
        {
            this.displayName = this.Data.Name;
            this.description = this.Data.Description;
            if (this.appliedBootSheetIndex.Value >= 0)
            {
                Game1.content.LoadString("Strings\\StringsFromCSFiles:CustomizedBootItemName", this.DisplayName);
            }
            return true;
        }

        public override void reloadData()
        {
            this.price.Value = this.Data.SellPrice;
            this.defenseBonus.Value = this.Data.Defense;
            this.immunityBonus.Value = this.Data.Immunity;
            this.indexInColorSheet.Value = this.FullId.GetDeterministicHashCode();
        }

        public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
            string str = I18n.ItemTooltip_AddedByMod(this.Data.pack.smapiPack.Manifest.Name);
            Utility.drawTextWithShadow(spriteBatch, Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), new Color(100, 100, 100));
            y += (int)font.MeasureString(Game1.parseText(str, Game1.smallFont, this.getDescriptionWidth())).Y + 10;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
            //ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var tex = this.Data.pack.GetTexture(this.Data.Texture, 16, 16);
            spriteBatch.Draw(tex.Texture, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), tex.Rect, color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            var ret = new CustomBoots(this.Data);
            // TODO: the field from tailoring boots over another?
            ret._GetOneFrom(this);
            return ret;
        }
    }
}
