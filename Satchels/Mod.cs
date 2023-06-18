using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace Satchels
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        // Dictionary of object IDs, unqualified, to base i18n key
        public static Dictionary<string, string> UpgradeList = new Dictionary<string, string>
        {
            { "spacechase0.Satchels_SatchelUpgrade_test", "satchel-upgrade.test" }
        };

        private static Satchel toOpen;
        public static void QueueOpeningSatchel(Satchel satchel)
        {
            if ( !satchel.isOpen.Value )
                toOpen = satchel;
        }

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;

            var def = new SatchelDataDefinition();
            ItemRegistry.ItemTypes.Add(def);
            Helper.Reflection.GetField<Dictionary<string, IItemDataDefinition>>(typeof(ItemRegistry), "IdentifierLookup").GetValue()[def.Identifier] = def;

            BaseEnchantment.GetAvailableEnchantments().Add(new SatchelInceptionEnchantment());

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (toOpen != null)
            {
                var menu = new SatchelMenu(toOpen);

                if (Game1.activeClickableMenu == null)
                    Game1.activeClickableMenu = menu;
                else
                {
                    var theMenu = Game1.activeClickableMenu;
                    while (theMenu.GetChildMenu() != null)
                    {
                        theMenu = theMenu.GetChildMenu();
                    }
                    theMenu.SetChildMenu(menu);
                }

                toOpen = null;
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(Satchel));
            sc.RegisterSerializerType(typeof(SatchelInceptionEnchantment));
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/Satchels"))
            {
                e.LoadFrom(() =>
                {
                    Dictionary<string, SatchelData> ret = new();
                    for (int i = 0; i < 6; ++i)
                    {
                        ret.Add($"Satchel.T{i}",
                            new SatchelData()
                            {
                                BaseTextureIndex = i, // TODO: tmp
                                DisplayName = I18n.GetByKey($"satchel.{i}.name"),
                                Description = I18n.GetByKey($"satchel.{i}.description"),
                                InlayTextureIndex = i,
                                Capacity = 9 * (i + 1),
                                MaxUpgrades = i,
                            });
                    }
                    return ret;
                }, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/satchels.png"))
            {
                e.LoadFromModFile<Texture2D>("assets/satchels.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"{ModManifest.UniqueID}/upgrades.png"))
            {
                e.LoadFromModFile<Texture2D>("assets/upgrades.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/EnchantmentNames"))
            {
                e.Edit((asset) => asset.AsDictionary<string, string>().Data.Add("Satchel Inception", I18n.Enchantment_SatchelInception()));
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/ObjectExtensionData"))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, SpaceCore.Framework.VanillaAssetExpansion.ObjectExtensionData>().Data;
                    foreach (string upgrade in UpgradeList.Keys)
                    {
                        data.Add(upgrade, new() { MaxStackSizeOverride = 1, CategoryTextOverride = I18n.Upgrade_CategoryText() });
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    int i = 0;
                    foreach (var upgrade in UpgradeList)
                    {
                        data.Add(upgrade.Key, $"{upgrade.Key}/100/-300/Junk -999/" + I18n.GetByKey($"{upgrade.Value}.name") + "/" + I18n.GetByKey($"{upgrade.Value}.description") + $"////{i++}/{ModManifest.UniqueID}\\upgrades.png");
                    }
                });
            }
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.MouseRight)
                return;

            if (Game1.activeClickableMenu is ShopMenu shop)
            {
                foreach (var slot in shop.inventory.inventory)
                {
                    if (slot.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        int i = shop.inventory.inventory.IndexOf(slot);
                        if (shop.inventory.actualInventory[i] is Satchel satchel)
                        {
                            if (shop.heldItem is Item item )
                            {
                                shop.heldItem = satchel.quickDeposit(item);
                            }
                            else
                            {
                                Game1.activeClickableMenu.SetChildMenu(new SatchelMenu(satchel));
                            }
                            Helper.Input.Suppress(e.Button);
                            break;
                        }
                    }
                }
            }
        }
    }
}
