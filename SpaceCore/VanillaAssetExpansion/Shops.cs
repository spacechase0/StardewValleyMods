using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.VanillaAssetExpansion
{
    public class ShopExtensionData
    {
        public enum TabType
        {
            None,
            FurnitureCatalogue,
            Catalogue,
            Custom,
        }

        public TabType Tabs { get; set; } = TabType.None;

        public class CustomTab
        {
            public string Id { get; set; }
            public string IconTexture { get; set; }
            public Rectangle IconRect { get; set; }
            public string FilterCondition { get; set; } = "TRUE";
        }
        public List<CustomTab> CustomTabs { get; } = new List<CustomTab>();
    }

    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.setUpStoreForContext))]
    public static class ShopMenuSetupTabsPatch
    {
        public static void Postfix(ShopMenu __instance)
        {
            var dict = Game1.content.Load<Dictionary<string, ShopExtensionData>>("spacechase0.SpaceCore/ShopExtensionData");
            if (!dict.TryGetValue(__instance.ShopId, out var data))
                return;

            switch ( data.Tabs )
            {
                case ShopExtensionData.TabType.None:
                    __instance.UseNoTabs();
                    break;
                case ShopExtensionData.TabType.FurnitureCatalogue:
                    __instance.UseFurnitureCatalogueTabs();
                    break;
                case ShopExtensionData.TabType.Catalogue:
                    __instance.UseCatalogueTabs();
                    break;
                case ShopExtensionData.TabType.Custom:
                    int tabId = 100000;
                    foreach ( var tab_ in data.CustomTabs )
                    {
                        var tab = tab_; // I *think* capturing tab_ would stick with the last one? Since it changes each iteration

                        __instance.tabButtons.Add(new ShopMenu.ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.content.Load<Texture2D>(tab.IconTexture), tab.IconRect, 4)
                        {
                            myID = tabId++,
                            upNeighborID = -99998,
                            downNeighborID = -99998,
                            rightNeighborID = 3456,
                            Filter = (ISalable item) =>
                            {
                                if (item is not Item i)
                                    return tab.FilterCondition.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                                return GameStateQuery.CheckConditions(tab.FilterCondition, inputItem: i);
                            }
                        });
                    }
                    __instance.repositionTabs();
                    break;
            }
        }
    }
}
