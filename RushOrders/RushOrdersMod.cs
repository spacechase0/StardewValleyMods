using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace RushOrders
{
    public class RushOrdersMod : Mod
    {
        public static RushOrdersMod instance;
        public static RushOrdersConfig ModConfig { get; private set; }
        public override void Entry( IModHelper helper )
        {
            instance = this;

            Log.info("Loading Config");
            ModConfig = Helper.ReadConfig<RushOrdersConfig>();

            GameEvents.UpdateTick += addRushOrderToShop;
            GameEvents.UpdateTick += checkForRushOrder;
        }

        private static IClickableMenu prevMenu = null;
        public static void addRushOrderToShop(object sender, EventArgs args)
        {
            if (Game1.activeClickableMenu == prevMenu) return;
            if (!(Game1.activeClickableMenu is ShopMenu))
            {
                prevMenu = Game1.activeClickableMenu;
                return;
            }
            ShopMenu shop = Game1.activeClickableMenu as ShopMenu;

            if (shop.portraitPerson == null || shop.portraitPerson.name != "Clint")
            {
                prevMenu = Game1.activeClickableMenu;
                return;
            }

            FieldInfo stockField = typeof(ShopMenu).GetField("itemPriceAndStock", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo itemsField = typeof(ShopMenu).GetField("forSale", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Dictionary<Item, int[]> toAddStock = new Dictionary<Item, int[]>();
            Dictionary<Item, int[]> stock = (Dictionary < Item, int[]>) stockField.GetValue(shop);
            List<Item> toAddItems = new List<Item>();
            List<Item> items = (List<Item>)itemsField.GetValue(shop);
            foreach ( KeyValuePair< Item, int[] > entry in stock )
            {
                if (!(entry.Key is Tool)) continue;
                Tool tool = entry.Key as Tool;

                if (!(tool is Axe || tool is Pickaxe || tool is Hoe || tool is WateringCan))
                    continue;

                // I'm going to edit the description, and I don't want to affect the original shop entry
                Tool toolRush = null, toolNow = null;
                if (tool is Axe)
                {
                    toolRush = new Axe();
                    toolNow = new Axe();
                }
                else if (tool is Pickaxe)
                {
                    toolRush = new Pickaxe();
                    toolNow = new Pickaxe();
                }
                else if (tool is Hoe)
                {
                    toolRush = new Hoe();
                    toolNow = new Hoe();
                }
                else if (tool is WateringCan)
                {
                    toolRush = new WateringCan();
                    toolNow = new WateringCan();
                }
                toolRush.UpgradeLevel = tool.UpgradeLevel;
                toolNow.UpgradeLevel = tool.UpgradeLevel;
                // For 1.2:
                //toolRush.DisplayName = tool.name + "         *RUSH*";
                //toolNow.DisplayName  = tool.name + "       =INSTANT=";
                //toolRush.description = "The tool will take one day to upgrade." + Environment.NewLine + Environment.NewLine + tool.description;
                //toolNow.description = "The tool will be immediately upgraded." + Environment.NewLine + Environment.NewLine + tool.description;
                toolRush.description = "* RUSH *" + Environment.NewLine + Environment.NewLine + tool.description;
                toolNow.description = "= INSTANT =" + Environment.NewLine + Environment.NewLine + tool.description;

                int price = getUpgradePrice(tool.upgradeLevel);
                if (entry.Value[0] == price)
                {
                    int[] entryDataRush = (int[])entry.Value.Clone();
                    int[] entryDataNow = (int[])entry.Value.Clone();
                    entryDataRush[0] = (int)(entry.Value[ 0 ] * ModConfig.PriceFactor.Tool.Rush);
                    entryDataNow[0] = (int)(entry.Value[ 0 ] * ModConfig.PriceFactor.Tool.Now);

                    if (entryDataRush[0] != entry.Value[0] && ModConfig.PriceFactor.Tool.Rush > 0)
                    {
                        toAddStock.Add(toolRush, entryDataRush);
                        toAddItems.Add(toolRush);
                    }
                    if (entryDataNow[0] != entry.Value[0] && ModConfig.PriceFactor.Tool.Now > 0)
                    {
                        toAddStock.Add(toolNow, entryDataNow);
                        toAddItems.Add(toolNow);
                    }
                }
            }
            foreach (var elem in toAddStock)
                stock.Add(elem.Key, elem.Value);
            foreach (var elem in toAddItems)
                items.Add(elem);
            
            itemsField.SetValue(shop, items.OrderBy(i => i.Name).ToList());

            prevMenu = Game1.activeClickableMenu;
        }

        private static bool hadDialogue = false;
        private static int prevMoney = 0;
        public static void checkForRushOrder(object sender, EventArgs args)
        {
            if (Game1.player == null) return;
            NPC clint = Game1.getCharacterFromName("Clint");
            if (clint == null) return;

            bool haveDialogue = false;
            if (clint.CurrentDialogue.Count > 0 && clint.CurrentDialogue.Peek().getCurrentDialogue() == "Thanks. I'll get started on this as soon as I can. It should be ready in a couple days.")
                haveDialogue = true;

            if ( !hadDialogue && haveDialogue && Game1.player.daysLeftForToolUpgrade == 2 && Game1.player.toolBeingUpgraded != null )
            {
                int currPrice = getUpgradePrice(Game1.player.toolBeingUpgraded.upgradeLevel);
                int diff = prevMoney - Game1.player.money;

                if (diff == (int)(currPrice * ModConfig.PriceFactor.Tool.Now))
                {
                    Game1.player.daysLeftForToolUpgrade = 0;
                    clint.CurrentDialogue.Pop();
                    Game1.drawDialogue(Game1.getCharacterFromName("Clint", false), "Thanks. I'll get started right away. It should be ready in a few minutes.");
                }
                else if ( diff == ( int )( currPrice * ModConfig.PriceFactor.Tool.Rush) )
                {
                    Game1.player.daysLeftForToolUpgrade = 1;
                    clint.CurrentDialogue.Pop();
                    Game1.drawDialogue(Game1.getCharacterFromName("Clint", false), "Thanks. I'll get started right away. It should be ready tomorrow.");
                }
            }

            hadDialogue = haveDialogue;
            prevMoney = Game1.player.money;
        }

        private static MethodInfo getUpgradePriceInfo;
        public static int getUpgradePrice( int level )
        {
            if (getUpgradePriceInfo == null)
            {
                getUpgradePriceInfo = typeof(Utility).GetMethod("priceForToolUpgradeLevel", BindingFlags.NonPublic | BindingFlags.Static);
            }
            return (int) getUpgradePriceInfo.Invoke(null, new object[] { level });
        }
    }
}
