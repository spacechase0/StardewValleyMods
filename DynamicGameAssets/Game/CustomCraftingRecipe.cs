using System.Text;
using System.Xml.Serialization;
using DynamicGameAssets.Framework;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGACraftingRecipe")] // Shouldn't ever exist inside an inventory, but just in case
    public partial class CustomCraftingRecipe : Object
    {
        private Item craftedCache = null;

        partial void DoInit(CraftingRecipePackData data)
        {
            this.ParentSheetIndex = Mod.BaseFakeObjectId;
            this.name = data.ID + " Recipe";
            this.type.Value = "Basic";
            if (data.IsCooking)
                this.category.Value = StardewValley.Object.CookingCategory;

            this.IsRecipe = true;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
            this.NetId.fieldChangeEvent += (_, _, _) =>
            {
                this.craftedCache = this.Data.Result[0].Value.Create();
            };
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
            ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }


        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            transparency = 0.5f;
            scaleSize *= 0.75f;

            this.craftedCache.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }

        public override Item getOne()
        {
            var ret = new CustomCraftingRecipe(this.Data);
            // TODO: All the other fields objects does??
            ret.Quality = this.Quality;
            ret.Stack = 1;
            ret.Price = this.Price;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override string DisplayName { get => this.Data.Name; set { } }

        public override string getDescription()
        {
            return this.Data.Description;
        }
    }
}
