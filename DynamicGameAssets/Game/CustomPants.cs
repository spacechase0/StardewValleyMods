using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;
using StardewValley.Objects;

namespace DynamicGameAssets.Game
{
    [XmlType("Mods_DGAPants")]
    public partial class CustomPants : Clothing
    {
        partial void DoInit()
        {
            this.NetFields.AddFields(this.NetSourcePack, this.NetId);
        }

        partial void DoInit(PantsPackData data)
        {
            this.dyeable.Value = data.Dyeable;
            this.clothesType.Value = (int)ClothesType.PANTS;
            this.clothesColor.Value = data.DefaultColor;

            this.indexInTileSheetMale.Value = this.FullId.GetDeterministicHashCode();
        }

        public override string DisplayName { get => this.Data.Name; set { } }

        public override string getDescription()
        {
            return Game1.parseText(this.Data.Description, Game1.smallFont, this.getDescriptionWidth());
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Color clothes_color = this.clothesColor;
            if (this.isPrismatic.Value)
            {
                clothes_color = Utility.GetPrismaticColor();
            }

            var currTex = this.Data.GetTexture();

            spriteBatch.Draw(currTex.Texture, location + new Vector2(32f, 32f), currTex.Rect, Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            var ret = new CustomPants(this.Data);
            ret.clothesColor.Value = this.clothesColor.Value;
            ret._GetOneFrom(this);
            return ret;
        }
    }
}
