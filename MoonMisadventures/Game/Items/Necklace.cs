using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;

namespace MoonMisadventures.Game.Items
{
    [XmlType("Mods_spacechase0_MoonMisadventures_Necklace")]
    public class Necklace : Item
    {
        public static class Type
        {
            public static readonly string Looting = "looting";
            public static readonly string Shocking = "shocking";
            public static readonly string Speed = "speed";
            public static readonly string Health = "health";
            public static readonly string Cooling = "cooling";
            public static readonly string Lunar = "lunar";
            public static readonly string Water = "water";
            public static readonly string Sea = "sea";
        }

        public override string DisplayName { get; set; }
        public string Description { get; set; }
        public override int Stack { get => 1; set { } }

        public override string GetItemQualifier()
        {
            return "(SC0_MM_N)";
        }

        public Necklace() { }
        public Necklace( string type )
        {
            ItemID = type;
            ReloadData();
        }

        public void ReloadData()
        {
            var data = Game1.content.Load< Dictionary< string, NecklaceData > >( "spacechase0.MoonMisadventures/Necklaces" );
            Name = "Necklace." + ItemID;
            DisplayName = data[ItemID].DisplayName;
            Description = data[ItemID].Description;
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            //spriteBatch.Draw( Assets.Necklaces, location + new Vector2( 32, 32 ), new Rectangle( ( ( int ) necklaceType.Value ) % 4 * 16, ( ( int ) necklaceType.Value ) / 4 * 16, 16, 16 ), color * transparency, 0, new Vector2( 8, 8 ) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth );
            var data = Utility.GetItemDataForItemID(QualifiedItemID);
            Rectangle rect = data.GetSourceRect(0);
            spriteBatch.Draw(data.texture, location + new Vector2(32, 32) * scaleSize, rect, color * transparency, 0, new Vector2(8, 8) * scaleSize, scaleSize * 4, SpriteEffects.None, layerDepth);
        }

        public override int maximumStackSize()
        {
            return 1;
        }

        public override int addToStack( Item stack )
        {
            return 1;
        }

        public override string getDescription()
        {
            return Game1.parseText(Description, Game1.smallFont, getDescriptionWidth());
        }

        public override bool isPlaceable()
        {
            return false;
        }

        public override Item getOne()
        {
            var ret = new Necklace( ItemID );
            ret._GetOneFrom( this );
            return ret;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override bool canBeGivenAsGift()
        {
            return false;
        }

        public override bool canBeDropped()
        {
            return false;
        }

        public override void onEquip( Farmer player )
        {
            if ( ItemID == Type.Health )
            {
                int diff = ( int )( player.maxHealth * 0.5f );
                player.maxHealth += diff;
                player.health = Math.Min( player.maxHealth, player.health + diff );
            }
        }

        public override void onUnequip( Farmer player )
        {
            if ( ItemID == Type.Health )
            {
                int oldHealth = player.health;
                int oldMax = player.maxHealth;
                player.maxHealth = 100;
                LevelUpMenu.RevalidateHealth( player );
                int diff = oldMax - player.maxHealth;
                player.health -= diff;
            }
        }
    }
}
