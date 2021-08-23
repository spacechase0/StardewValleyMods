using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DynamicGameAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace DynamicGameAssets.Game
{
    [XmlType( "Mods_DGABoots" )]
    public class CustomBoots : Boots, IDGAItem
    {
        public readonly NetString _sourcePack = new NetString();
        public readonly NetString _id = new NetString();

        [XmlIgnore]
        public string SourcePack => _sourcePack.Value;
        [XmlIgnore]
        public string Id => _id.Value;
        [XmlIgnore]
        public string FullId => $"{SourcePack}/{Id}";
        [XmlIgnore]
        public BootsPackData Data => Mod.Find( FullId ) as BootsPackData;

        public CustomBoots()
        {
            this.NetFields.AddFields( _sourcePack, _id );
        }

        public CustomBoots( BootsPackData data )
        :   this()
        {
            _sourcePack.Value = data.parent.smapiPack.Manifest.UniqueID;
            _id.Value = data.ID;

            this.Name = Id;
            reloadData();
        }

        public bool LoadDisplayFields()
        {
            this.displayName = Data.Name;
            this.description = Data.Description;
            if ( this.appliedBootSheetIndex.Value >= 0 )
            {
                Game1.content.LoadString( "Strings\\StringsFromCSFiles:CustomizedBootItemName", this.DisplayName );
            }
            return true;
        }

        public override void reloadData()
        {
            this.price.Value = Data.SellPrice;
            this.defenseBonus.Value = Data.Defense;
            this.immunityBonus.Value = Data.Immunity;
            this.indexInColorSheet.Value = FullId.GetDeterministicHashCode();
        }
        public override void drawTooltip( SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText )
        {
            base.drawTooltip( spriteBatch, ref x, ref y, font, alpha, overrideText );
            string str = "Mod: " + Data.parent.smapiPack.Manifest.Name;
            Utility.drawTextWithShadow( spriteBatch, Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ), font, new Vector2( x + 16, y + 16 + 4 ), new Color( 100, 100, 100 ) );
            y += ( int ) font.MeasureString( Game1.parseText( str, Game1.smallFont, this.getDescriptionWidth() ) ).Y + 10;
        }

        public override Point getExtraSpaceNeededForTooltipSpecialIcons( SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom )
        {
            var ret = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom );
            //ret.Y = startingHeight;
            ret.Y += 48;
            return ret;
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            var tex = Data.parent.GetTexture( Data.Texture, 16, 16 );
            spriteBatch.Draw( tex.Texture, location + new Vector2( ( int ) ( 32f * scaleSize ), ( int ) ( 32f * scaleSize ) ), tex.Rect, color * transparency, 0f, new Vector2( 8f, 8f ) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth );
        }
    }
}
