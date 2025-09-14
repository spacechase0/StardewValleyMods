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

        public override string DisplayName => GetDisplayName();
        public string Description { get; set; }

        public override string TypeDefinitionId => "(SC0_MM_N)";

        public Necklace() { }
        public Necklace( string type )
        {
            ItemId = type;
            ReloadData();
        }

        private string GetDisplayName()
        {
            var data = Game1.content.Load<Dictionary<string, NecklaceData>>("spacechase0.MoonMisadventures/Necklaces");
            return data[ItemId].DisplayName;
        }

        public void ReloadData()
        {
            var data = Game1.content.Load< Dictionary< string, NecklaceData > >( "spacechase0.MoonMisadventures/Necklaces" );
            Name = "Necklace." + ItemId;
            Description = data[ItemId].Description;
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            //spriteBatch.Draw( Assets.Necklaces, location + new Vector2( 32, 32 ), new Rectangle( ( ( int ) necklaceType.Value ) % 4 * 16, ( ( int ) necklaceType.Value ) / 4 * 16, 16, 16 ), color * transparency, 0, new Vector2( 8, 8 ) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth );
            var data = ItemRegistry.GetDataOrErrorItem(QualifiedItemId);
            Rectangle rect = data.GetSourceRect(0);
            spriteBatch.Draw(data.GetTexture(), location + new Vector2(32, 32) * scaleSize, rect, color * transparency, 0, new Vector2(8, 8) * scaleSize, scaleSize * 4, SpriteEffects.None, layerDepth);
        }

        public override int maximumStackSize()
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

        protected override Item GetOneNew()
        {
            return new Necklace();
        }

        protected override void GetOneCopyFrom(Item source)
        {
            base.GetOneCopyFrom(source);

            ItemId = source.ItemId;
            ReloadData();
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
            if ( ItemId == Type.Health )
            {
                int diff = ( int )( player.maxHealth * 0.5f );
                player.maxHealth += diff;
                player.health = Math.Min( player.maxHealth, player.health + diff );
            }
        }

        public override void onUnequip( Farmer player )
        {
            if ( ItemId == Type.Health )
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
