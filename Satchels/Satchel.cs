using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Tools;

namespace Satchels
{
    [XmlType("Mods_spacechase0_Satchels_Satchel")]
    public class Satchel : GenericTool
    {
        public override string DisplayName => GetDisplayName();
        public string Description { get; set; }

        public override string TypeDefinitionId => "(SC0_S_S)";

        public Inventory Inventory => netInventory.Value;
        public Inventory Upgrades => netUpgrades.Value;

        public readonly NetString satchelName = new();
        public readonly NetRef<Inventory> netInventory = new(new());
        public readonly NetRef<Inventory> netUpgrades = new(new());

        [XmlIgnore]
        public readonly NetBool isOpen = new(false);

        public Satchel()
        {
            NetFields.AddField(satchelName)
                .AddField(netInventory)
                .AddField(netUpgrades)
                .AddField(isOpen);

            InstantUse = true;
        }
        public Satchel( string type )
        :   this()
        {
            ItemId = type;
            ReloadData();
            while (Inventory.Count < SatchelDataDefinition.GetSpecificData(ItemId).Capacity)
                Inventory.Add(null);
            while (Upgrades.Count < SatchelDataDefinition.GetSpecificData(ItemId).MaxUpgrades)
                Upgrades.Add(null);
        }

        private string GetDisplayName()
        {
            try
            {
                if (!string.IsNullOrEmpty(satchelName.Value))
                    return satchelName.Value;

                var data = Game1.content.Load<Dictionary<string, SatchelData>>("spacechase0.Satchels/Satchels");
                return data[ItemId].DisplayName;
            }
            catch (Exception e)
            {
                return "Error Item";
            }
        }

        public void ReloadData()
        {
            var data = Game1.content.Load< Dictionary< string, SatchelData > >("spacechase0.Satchels/Satchels");
            Name = ItemId;
            Description = data[ItemId].Description + "^^" + I18n.Satchel_Description();
        }

        public override bool canThisBeAttached(StardewValley.Object o)
        {
            return true;
        }

        public override StardewValley.Object attach(StardewValley.Object o)
        {
            if (o == null)
            {
                Mod.QueueOpeningSatchel(this);
                return null;
            }
            else
                return (StardewValley.Object)quickDeposit(o);
        }

        public Item quickDeposit(Item item)
        {
            for (int i = 0; i < Inventory.Count; ++i)
            {
                if (Inventory[i]?.canStackWith(item) ?? false)
                {
                    int left = Inventory[i].addToStack(item);
                    if (left <= 0)
                        return null;
                    item.Stack = left;
                }
            }

            for (int i = 0; i < Inventory.Count; ++i)
            {
                if (Inventory[i] == null)
                {
                    Inventory[i] = item;
                    return null;
                }
            }

            return item;
        }

        public override void drawInMenu( SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow )
        {
            //spriteBatch.Draw( Assets.Necklaces, location + new Vector2( 32, 32 ), new Rectangle( ( ( int ) necklaceType.Value ) % 4 * 16, ( ( int ) necklaceType.Value ) / 4 * 16, 16, 16 ), color * transparency, 0, new Vector2( 8, 8 ) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth );
            var data = ItemRegistry.GetDataOrErrorItem(QualifiedItemId);
            Rectangle rect = data.GetSourceRect(0);
            spriteBatch.Draw(data.GetTexture(), location + new Vector2(32, 32) * scaleSize, rect, color * transparency, 0, new Vector2(8, 8) * scaleSize, scaleSize * 4, SpriteEffects.None, layerDepth);
        }

        public override string getDescription()
        {
            return Game1.parseText(Description.Replace('^', '\n'), Game1.smallFont, getDescriptionWidth());
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            Mod.QueueOpeningSatchel(this);
            //who.forceCanMove();
        }

        protected override Item GetOneNew()
        {
            return new Satchel();
        }

        protected override void GetOneCopyFrom(Item source)
        {
            var satchel = source as Satchel;
            base.GetOneCopyFrom(source);

            ItemId = source.ItemId;
            ReloadData();

            while (Inventory.Count < SatchelDataDefinition.GetSpecificData(ItemId).Capacity)
                Inventory.Add(null);
            while (Upgrades.Count < SatchelDataDefinition.GetSpecificData(ItemId).MaxUpgrades)
                Upgrades.Add(null);

            for (int i = 0; i < Inventory.Count; ++i)
            {
                Inventory[i] = satchel.Inventory[i]?.getOne();
            }
            for (int i = 0; i < Upgrades.Count; ++i)
            {
                Upgrades[i] = satchel.Upgrades[i]?.getOne();
            }
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

        public override bool CanAddEnchantment(BaseEnchantment enchantment)
        {
            return (enchantment is SatchelInceptionEnchantment);
        }

        public override bool ForEachItem(ForEachItemDelegate handler)
        {
            if (!base.ForEachItem(handler))
                return false;

            if (!ForEachItemHelper.ApplyToList(Inventory, handler, true))
                return false;

            if (!ForEachItemHelper.ApplyToList(Upgrades, handler, true))
                return false;

            return true;
        }
    }
}
