using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.Buildings;

namespace RushOrders
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static RushOrdersConfig ModConfig { get; private set; }
        public override void Entry( IModHelper helper )
        {
            instance = this;

            Log.info("Loading Config");
            ModConfig = Helper.ReadConfig<RushOrdersConfig>();

            GameEvents.UpdateTick += checkCurrentMenu;
            GameEvents.UpdateTick += checkToolUpgradeForRushOrder;
        }
        
        private static IClickableMenu prevMenu = null;
        public static void checkCurrentMenu(object sender, EventArgs args)
        {
            if (Game1.activeClickableMenu == prevMenu) return;
            if (Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = Game1.activeClickableMenu as ShopMenu;
                if (shop.portraitPerson != null)
                {
                    if (shop.portraitPerson.name == "Clint")
                    {
                        addToolRushOrders(shop);
                    }
                    else if (shop.portraitPerson.name == "Robin")
                    {
                        if ((Game1.player.daysUntilHouseUpgrade > 1 || Game1.getFarm().isThereABuildingUnderConstruction() &&
                            (Game1.getFarm().getBuildingUnderConstruction().daysOfConstructionLeft > 1 ||
                             Game1.getFarm().getBuildingUnderConstruction().daysUntilUpgrade > 1)))
                        {
                            if (!(prevMenu is RushConstructionMenu))
                                doRushBuildingDialogue();
                        }
                    }
                }
            }
            else if (Game1.activeClickableMenu is DialogueBox)
            {
                DialogueBox diagBox = Game1.activeClickableMenu as DialogueBox;
                var diag = instance.Helper.Reflection.GetPrivateValue< Dialogue >(diagBox, "characterDialogue");
                if ( diag != null && diag.speaker != null && diag.speaker.name == "Robin" &&
                    ( Game1.player.daysUntilHouseUpgrade > 1 || Game1.getFarm().isThereABuildingUnderConstruction() &&
                     (Game1.getFarm().getBuildingUnderConstruction().daysOfConstructionLeft > 1 ||
                      Game1.getFarm().getBuildingUnderConstruction().daysUntilUpgrade > 1)) )
                {
                    if (!(prevMenu is RushConstructionMenu))
                        doRushBuildingDialogue();
                }
            }
            prevMenu = Game1.activeClickableMenu;
        }

        private static void addToolRushOrders( ShopMenu shop )
        {
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
                toolRush.DisplayName = tool.name + "         *RUSH*";
                toolNow.DisplayName  = tool.name + "       =INSTANT=";
                toolRush.description = "The tool will take one day to upgrade." + Environment.NewLine + Environment.NewLine + tool.description;
                toolNow.description = "The tool will be immediately upgraded." + Environment.NewLine + Environment.NewLine + tool.description;
                
                int price = getToolUpgradePrice(tool.upgradeLevel);
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
        private static void checkToolUpgradeForRushOrder(object sender, EventArgs args)
        {
            if (Game1.player == null) return;
            NPC clint = Game1.getCharacterFromName("Clint");
            if (clint == null) return;

            bool haveDialogue = false;
            if (clint.CurrentDialogue.Count > 0 && clint.CurrentDialogue.Peek().getCurrentDialogue() == Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"))
                haveDialogue = true;

            if ( !hadDialogue && haveDialogue && Game1.player.daysLeftForToolUpgrade == 2 && Game1.player.toolBeingUpgraded != null )
            {
                int currPrice = getToolUpgradePrice(Game1.player.toolBeingUpgraded.upgradeLevel);
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

        private static void doRushBuildingDialogue()
        {
            Game1.activeClickableMenu = new RushConstructionMenu( Game1.activeClickableMenu );
        }

        private static MethodInfo getToolUpgradePriceInfo;
        public static int getToolUpgradePrice( int level )
        {
            if (getToolUpgradePriceInfo == null)
            {
                getToolUpgradePriceInfo = typeof(Utility).GetMethod("priceForToolUpgradeLevel", BindingFlags.NonPublic | BindingFlags.Static);
            }
            return (int) getToolUpgradePriceInfo.Invoke(null, new object[] { level });
        }

        public static void rushBuilding()
        {
            if (Game1.player.daysUntilHouseUpgrade > 0)
                Game1.player.daysUntilHouseUpgrade--;
            else if (Game1.getFarm().getBuildingUnderConstruction() != null)
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft > 0)
                    building.daysOfConstructionLeft--;
                else if (building.daysUntilUpgrade > 0)
                    building.daysUntilUpgrade--;
            }
        }

        public static int getBuildingDaysLeft()
        {
            if (Game1.player.daysUntilHouseUpgrade > 0)
                return Game1.player.daysUntilHouseUpgrade;
            else if (Game1.getFarm().getBuildingUnderConstruction() != null)
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft > 0)
                    return building.daysOfConstructionLeft;
                else if (building.daysUntilUpgrade > 0)
                    return building.daysUntilUpgrade;
            }

            return -1;
        }

        public static int getBuildingRushPrice()
        {
            int num = 0;
            if (Game1.player.daysUntilHouseUpgrade > 0)
            {
                if (Game1.player.houseUpgradeLevel == 0)
                    num = 10000;
                else if (Game1.player.houseUpgradeLevel == 1)
                    num = 50000;
                else if (Game1.player.houseUpgradeLevel == 2)
                    num = 100000;
            }
            else if ( Game1.getFarm().getBuildingUnderConstruction() != null )
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft > 0)
                {
                    BluePrint bp = new BluePrint(building.buildingType);
                    num = bp.moneyRequired;
                }
                else if (building.daysUntilUpgrade > 0)
                {
                    BluePrint bp = new BluePrint(building.getNameOfNextUpgrade());
                    num = bp.moneyRequired;
                }
            }

            return (int)(num * Mod.ModConfig.PriceFactor.Building.RushOneDay);
        }
    }
}
