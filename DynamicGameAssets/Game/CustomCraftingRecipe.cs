using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Text;
using System.Xml.Serialization;

namespace DynamicGameAssets.Game
{
    [XmlType( "Mods_DGACraftingRecipe" )] // Shouldn't ever exist inside an inventory, but just in case
    public partial class CustomCraftingRecipe : StardewValley.Object
    {
        private Item craftedCache = null;

        partial void DoInit( CraftingRecipePackData data )
        {
            ParentSheetIndex = Mod.BaseFakeObjectId;
            name = data.ID + " Recipe";
            type.Value = "Basic";
            if (data.IsCooking)
                category.Value = StardewValley.Object.CookingCategory;

            IsRecipe = true;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields( _sourcePack, _id );
            _id.fieldChangeEvent += (f, ov, nv) => { craftedCache = Data.Result[ 0 ].Value.Create(); };
        }

        public override void drawTooltip( SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText )
        {
            base.drawTooltip( spriteBatch, ref x, ref y, font, alpha, overrideText );
            string str = "Mod: " + Data.pack.smapiPack.Manifest.Name;
            Utility.drawTextWithShadow( spriteBatch, Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ), font, new Vector2( x + 16, y + 16 + 4 ), new Color( 100, 100, 100 ) );
            y += ( int ) font.MeasureString( Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ) ).Y + 10;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons( SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom )
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom );
            ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }


        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            transparency = 0.5f;
            scaleSize *= 0.75f;

            craftedCache.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }

        public override Item getOne()
        {
            var ret = new CustomCraftingRecipe( Data );
            // TODO: All the other fields objects does??
            ret.Quality = Quality;
            ret.Stack = 1;
            ret.Price = Price;
            ret._GetOneFrom( this );
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override string DisplayName { get => Data.Name; set { } }

        public override string getDescription()
        {
            return Data.Description;
        }
    }
}
