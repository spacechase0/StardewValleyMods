using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RushOrders.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Tools;

namespace RushOrders
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static ModConfig ModConfig { get; private set; }
        private static Api Api;
        private static bool HadDialogue;
        private static int PrevMoney;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Log.Info("Loading Config");
            Mod.ModConfig = this.Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        public override object GetApi()
        {
            return (Mod.Api = new Api());
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetGenericModConfigMenuApi(this.Monitor);
            if (configMenu != null)
            {
                configMenu.Register(
                    mod: this.ModManifest,
                    reset: () => Mod.ModConfig = new ModConfig(),
                    save: () => this.Helper.WriteConfig(Mod.ModConfig)
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_PriceToolOneDay_Name,
                    tooltip: I18n.Config_PriceToolOneDay_Tooltip,
                    getValue: () => (float)Mod.ModConfig.PriceFactor.Tool.Rush,
                    setValue: value => Mod.ModConfig.PriceFactor.Tool.Rush = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_PriceToolInstant_Name,
                    tooltip: I18n.Config_PriceToolInstant_Tooltip,
                    getValue: () => (float)Mod.ModConfig.PriceFactor.Tool.Now,
                    setValue: value => Mod.ModConfig.PriceFactor.Tool.Now = value
                );
                configMenu.AddNumberOption(
                    mod: this.ModManifest,
                    name: I18n.Config_PriceBuilding_Name,
                    tooltip: I18n.Config_PriceBuilding_Tooltip,
                    getValue: () => (float)Mod.ModConfig.PriceFactor.Building.RushOneDay,
                    setValue: value => Mod.ModConfig.PriceFactor.Building.RushOneDay = value
                );
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        public void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // add rush option to menu
            switch (Game1.activeClickableMenu)
            {
                case ShopMenu shop:
                    {
                        switch (shop.portraitPerson?.Name)
                        {
                            case "Clint":
                                Mod.AddToolRushOrders(shop);
                                break;

                            case "Robin":
                                if (this.HasBuildingToRush() && e.OldMenu is not RushConstructionMenu)
                                    Mod.DoRushBuildingDialogue();
                                break;
                        }
                        break;
                    }

                case DialogueBox diagBox:
                    {
                        var diag = diagBox.characterDialogue;
                        if (diag?.speaker != null && diag.speaker.Name == "Robin" && this.HasBuildingToRush() && e.OldMenu is not RushConstructionMenu)
                            Mod.DoRushBuildingDialogue();

                        break;
                    }
            }
        }

        private static void AddToolRushOrders(ShopMenu shop)
        {
            Dictionary<ISalable, ItemStockInformation> toAddStock = new();
            var stock = shop.itemPriceAndStock;
            List<ISalable> toAddItems = new();
            var items = shop.forSale;
            foreach (var entry in stock)
            {
                if (entry.Key is not (Tool tool and (Axe or Pickaxe or Hoe or WateringCan)))
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
                toolRush.description = I18n.Clint_Rush_Description() + Environment.NewLine + Environment.NewLine + tool.description;
                toolNow.description = I18n.Clint_Instant_Description() + Environment.NewLine + Environment.NewLine + tool.description;

                int price = Mod.GetToolUpgradePrice(tool.UpgradeLevel);
                if (entry.Value.price == price)
                {
                    var entryDataRush = entry.Value.Clone();
                    var entryDataNow = entry.Value.Clone();
                    entryDataRush.price = (int)(entry.Value.price * Mod.ModConfig.PriceFactor.Tool.Rush);
                    entryDataNow.price = (int)(entry.Value.price * Mod.ModConfig.PriceFactor.Tool.Now);

                    if (entryDataRush.price != entry.Value.price && Mod.ModConfig.PriceFactor.Tool.Rush > 0)
                    {
                        toAddStock.Add(toolRush, entryDataRush);
                        toAddItems.Add(toolRush);
                    }
                    if (entryDataNow.price != entry.Value.price && Mod.ModConfig.PriceFactor.Tool.Now > 0)
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

            shop.forSale = items.OrderBy(i => i.Name).ToList();
        }

        /// <summary>Get whether there's a building being upgraded or constructed that can be rushed.</summary>
        private bool HasBuildingToRush()
        {
            // farmhouse upgrade
            if (Game1.player.daysUntilHouseUpgrade.Value > 1)
                return true;

            // building
            Building constructing = Game1.getFarm().getBuildingUnderConstruction();
            return constructing != null && (constructing.daysOfConstructionLeft.Value > 1 || constructing.daysUntilUpgrade.Value > 1);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            // add tool rush order
            NPC clint = Game1.getCharacterFromName("Clint");
            bool hasDialog = clint?.CurrentDialogue.Count > 0 && clint.CurrentDialogue.Peek().getCurrentDialogue() == Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317");
            if (hasDialog && !Mod.HadDialogue && Game1.player.daysLeftForToolUpgrade.Value == 2 && Game1.player.toolBeingUpgraded.Value != null)
            {
                int curPrice = Mod.GetToolUpgradePrice(Game1.player.toolBeingUpgraded.Value.UpgradeLevel);
                int diff = Mod.PrevMoney - Game1.player.Money;

                if (diff == (int)(curPrice * Mod.ModConfig.PriceFactor.Tool.Now))
                {
                    Game1.player.daysLeftForToolUpgrade.Value = 0;
                    clint.CurrentDialogue.Pop();
                    Game1.drawDialogue(clint, I18n.Clint_Instant_Dialogue());
                    Mod.Api.InvokeToolRushed(Game1.player.toolBeingUpgraded.Value);
                }
                else if (diff == (int)(curPrice * Mod.ModConfig.PriceFactor.Tool.Rush))
                {
                    Game1.player.daysLeftForToolUpgrade.Value = 1;
                    clint.CurrentDialogue.Pop();
                    Game1.drawDialogue(clint, I18n.Clint_Rush_Dialogue());
                    Mod.Api.InvokeToolRushed(Game1.player.toolBeingUpgraded.Value);
                }
            }
            Mod.HadDialogue = hasDialog;
            Mod.PrevMoney = Game1.player.Money;
        }

        private static void DoRushBuildingDialogue()
        {
            Game1.activeClickableMenu = new RushConstructionMenu(Game1.activeClickableMenu);
        }

        private static MethodInfo GetToolUpgradePriceInfo;
        public static int GetToolUpgradePrice(int level)
        {
            Mod.GetToolUpgradePriceInfo ??= Mod.Instance.Helper.Reflection.GetMethod(typeof(Utility), "priceForToolUpgradeLevel").MethodInfo;
            return (int)Mod.GetToolUpgradePriceInfo.Invoke(null, new object[] { level });
        }

        public static void RushBuilding()
        {
            if (Game1.player.daysUntilHouseUpgrade.Value > 0)
                Game1.player.daysUntilHouseUpgrade.Value--;
            else if (Game1.getFarm().getBuildingUnderConstruction() != null)
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft.Value > 0)
                    building.daysOfConstructionLeft.Value--;
                else if (building.daysUntilUpgrade.Value > 0)
                    building.daysUntilUpgrade.Value--;
            }
            Mod.Api.InvokeBuildingRushed();
        }

        public static int GetBuildingDaysLeft()
        {
            if (Game1.player.daysUntilHouseUpgrade.Value > 0)
                return Game1.player.daysUntilHouseUpgrade.Value;
            else if (Game1.getFarm().getBuildingUnderConstruction() != null)
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft.Value > 0)
                    return building.daysOfConstructionLeft.Value;
                else if (building.daysUntilUpgrade.Value > 0)
                    return building.daysUntilUpgrade.Value;
            }

            return -1;
        }

        public static int GetBuildingRushPrice()
        {
            int num = 0;
            if (Game1.player.daysUntilHouseUpgrade.Value > 0)
            {
                num = Game1.player.HouseUpgradeLevel switch
                {
                    0 => 10000,
                    1 => 50000,
                    2 => 100000,
                    _ => num
                };
            }
            else if (Game1.getFarm().getBuildingUnderConstruction() != null)
            {
                Building building = Game1.getFarm().getBuildingUnderConstruction();
                if (building.daysOfConstructionLeft.Value > 0)
                {
                    BluePrint bp = new BluePrint(building.buildingType.Value);
                    num = bp.moneyRequired;
                }
                else if (building.daysUntilUpgrade.Value > 0)
                {
                    BluePrint bp = new BluePrint(building.getNameOfNextUpgrade());
                    num = bp.moneyRequired;
                }
            }

            return (int)(num * Mod.ModConfig.PriceFactor.Building.RushOneDay);
        }
    }

    public static class Extensions
    {
        internal static ItemStockInformation Clone(this ItemStockInformation isi)
        {
            ItemStockInformation ret = new();
            ret.price = isi.price;
            ret.stock = isi.stock;
            ret.tradeItem = isi.tradeItem;
            ret.tradeItemCount = isi.tradeItemCount;
            return ret;
        }
    }
}
