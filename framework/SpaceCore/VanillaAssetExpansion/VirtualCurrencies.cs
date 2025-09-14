using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using static StardewValley.GameStateQuery;

namespace SpaceCore.VanillaAssetExpansion
{
    public class VirtualCurrencyData
    {
        public bool TeamWide { get; set; } = false;
        public string ObtainSound { get; set; } = null;
    }

    public static class VirtualCurrencyExtensions
    {

        public static ConditionalWeakTable<FarmerTeam, NetStringDictionary<int, NetIntDelta>> TeamCurrencies = new();
        public static ConditionalWeakTable<Farmer, NetStringDictionary<int, NetIntDelta>> PersonalCurrencies = new();

        public static NetStringDictionary<int, NetIntDelta> get_PersonalCurrencies(Farmer farmer)
        {
            return PersonalCurrencies.GetOrCreateValue(farmer);
        }
        public static void set_PersonalCurrencies(Farmer farmer, NetStringDictionary<int, NetIntDelta> newVal) { }

        public static int GetVirtualCurrencyAmount(this Farmer farmer, string currency)
        {
            if (PersonalCurrencies.GetOrCreateValue(farmer).TryGetValue(currency, out int amount))
                return amount;
            return 0;
        }
        public static void AddVirtualCurrencyAmount(this Farmer farmer, string currency, int amt)
        {
            var currencies = PersonalCurrencies.GetOrCreateValue(farmer);
            if (!currencies.ContainsKey(currency))
            {
                currencies.Add(currency, Math.Max(0, amt));
                return;
            }
            else
            {
                currencies[currency] = Math.Max(0, currencies[currency] + amt);
            }

            if (amt < 0) return;

            var data = ItemRegistry.GetDataOrErrorItem($"(O){currency}");
            var sourceRect = data.GetSourceRect();

            if (VanillaAssetExpansion.virtualCurrencies[currency].ObtainSound != null)
            {
                Game1.playSound(VanillaAssetExpansion.virtualCurrencies[currency].ObtainSound);
            }
            farmer.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(data.GetTextureName(), sourceRect, 100f, 1, 8, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                motion = new Vector2(0f, -6f),
                acceleration = new Vector2(0f, 0.2f),
                stopAcceleratingWhenVelocityIsZero = true,
                attachedCharacter = farmer,
                positionFollowsAttachedCharacter = true
            });
        }

        public static int GetVirtualCurrencyAmount(this FarmerTeam team, string currency)
        {
            if (TeamCurrencies.GetOrCreateValue(team).TryGetValue(currency, out int amount))
                return amount;
            return 0;
        }
        public static void AddVirtualCurrencyAmount(this FarmerTeam team, string currency, int amt)
        {
            var currencies = TeamCurrencies.GetOrCreateValue(team);
            if (!currencies.ContainsKey(currency))
            {
                currencies.Add(currency, Math.Max(0, amt));
                return;
            }
            else
            {
                currencies[currency] = Math.Max(0, currencies[currency] + amt);
            }

            if (amt < 0) return;

            var data = ItemRegistry.GetDataOrErrorItem($"(O){currency}");
            var sourceRect = data.GetSourceRect();

            if (VanillaAssetExpansion.virtualCurrencies[currency].ObtainSound != null)
            {
                Game1.playSound(VanillaAssetExpansion.virtualCurrencies[currency].ObtainSound);
            }
            Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(data.GetTextureName(), sourceRect, 100f, 1, 8, new Vector2(0f, -96f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                motion = new Vector2(0f, -6f),
                acceleration = new Vector2(0f, 0.2f),
                stopAcceleratingWhenVelocityIsZero = true,
                attachedCharacter = Game1.player,
                positionFollowsAttachedCharacter = true
            });
        }
    }

    [HarmonyPatch(typeof(FarmerTeam), MethodType.Constructor)]
    public static class FarmerTeamAddVirtualCurrenciesPatch
    {
        public static void Postfix(FarmerTeam __instance)
        {
            var dict = VirtualCurrencyExtensions.TeamCurrencies.GetOrCreateValue(__instance);
            __instance.NetFields.AddField(dict, "SpaceCore_TeamCurrencies");
            dict.OnValueAdded += (string key, int val) =>
            {
                Game1.specialCurrencyDisplay?.Register(key, dict.FieldDict[key]);
            };
        }
    }

    [HarmonyPatch(typeof(Farmer), "initNetFields")]
    public static class FarmerAddVirtualCurrenciesPatch
    {
        public static void Postfix(Farmer __instance)
        {
            var dict = VirtualCurrencyExtensions.PersonalCurrencies.GetOrCreateValue(__instance);
            __instance.NetFields.AddField(dict, "SpaceCore_PersonalCurrencies");
            dict.OnValueAdded += (string key, int val) =>
            {
                Game1.specialCurrencyDisplay?.Register(key, dict.FieldDict[key]);
            };
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.OnItemReceived))]
    public static class FarmerReceiveVirtualCurrencyPatch
    {
#pragma warning disable IDE0008
        public static bool Prefix(Farmer __instance, Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification)
        {
            var currencies = Game1.content.Load<Dictionary<string, VirtualCurrencyData>>("spacechase0.SpaceCore/VirtualCurrencyData");
            if (!item.QualifiedItemId.StartsWith("(O)") || !currencies.ContainsKey(item.ItemId))
                return true;

            if (!__instance.IsLocalPlayer)
            {
                return false;
            }
            (item as StardewValley.Object)?.reloadSprite();
            if (item.HasBeenInInventory)
            {
                return false;
            }
            Item actualItem = mergedIntoStack ?? item;
            if (!hideHudNotification)
            {
                __instance.GetItemReceiveBehavior(actualItem, out var _, out var showHudNotification);
                if (showHudNotification)
                {
                    __instance.ShowItemReceivedHudMessage(actualItem, countAdded);
                }
            }
            if (__instance.freezePause <= 0)
            {
                __instance.mostRecentlyGrabbedItem = actualItem;
            }
            if (item.SetFlagOnPickup != null)
            {
                if (!__instance.hasOrWillReceiveMail(item.SetFlagOnPickup))
                {
                    Game1.addMail(item.SetFlagOnPickup, noLetter: true);
                }
                actualItem.SetFlagOnPickup = null;
            }
            (actualItem as SpecialItem)?.actionWhenReceived(__instance);
            StardewValley.Object obj = actualItem as StardewValley.Object;
            if (obj != null && obj.specialItem)
            {
                string key = (obj.IsRecipe ? ("-" + obj.ItemId) : obj.ItemId);
                if ((bool)obj.bigCraftable.Value || obj is Furniture)
                {
                    if (!__instance.specialBigCraftables.Contains(key))
                    {
                        __instance.specialBigCraftables.Add(key);
                    }
                }
                else if (!__instance.specialItems.Contains(key))
                {
                    __instance.specialItems.Add(key);
                }
            }
            int originalStack = actualItem.Stack;
            try
            {
                actualItem.Stack = countAdded;
                __instance.NotifyQuests(q => q.OnItemReceived(actualItem, countAdded));
                foreach (SpecialOrder specialOrder in __instance.team.specialOrders)
                {
                    specialOrder.onItemCollected?.Invoke(__instance, actualItem);
                }
            }
            finally
            {
                actualItem.Stack = originalStack;
            }
            if (actualItem.HasTypeObject())
            {
                StardewValley.Object obj2 = actualItem as StardewValley.Object;
                if (obj2 != null)
                {
                    if (obj2.Category == -2 || obj2.Type == "Minerals")
                    {
                        __instance.foundMineral(obj2.ItemId);
                    }
                    else if (obj2.Type == "Arch")
                    {
                        __instance.foundArtifact(obj2.ItemId, 1);
                    }
                }
            }

            if (currencies[item.ItemId].TeamWide)
            {
                __instance.team.AddVirtualCurrencyAmount(item.ItemId, countAdded);
            }
            else
            {
                __instance.AddVirtualCurrencyAmount(item.ItemId, countAdded);
            }
            __instance.removeItemFromInventory(actualItem);

            actualItem.HasBeenInInventory = true;

            return false;
        }
#pragma warning enable IDE0008
    }

    [HarmonyPatch(typeof(GameStateQuery.DefaultResolvers), nameof(GameStateQuery.DefaultResolvers.PLAYER_HAS_ITEM))]
    public static class GSQPlayerHasVirtualCurrencyPatch
    {
        public static bool Prefix(string[] query, GameStateQueryContext context, ref bool __result)
        {
            if (!ArgUtility.TryGet(query, 1, out var playerKey, out var error) || !ArgUtility.TryGet(query, 2, out var itemId, out error) || !ArgUtility.TryGetOptionalInt(query, 3, out var minCount, out error, 1) || !ArgUtility.TryGetOptionalInt(query, 4, out var maxCount, out error, int.MaxValue))
            {
                return true;
            }

            if (itemId.StartsWith("(O)"))
                itemId = itemId.Substring("(O)".Length);

            if (VanillaAssetExpansion.virtualCurrencies.ContainsKey(itemId))
            {
                __result = Helpers.WithPlayer(context.Player, playerKey, (Farmer f) =>
                {
                    int amt = 0;
                    if (VanillaAssetExpansion.virtualCurrencies[itemId].TeamWide)
                        amt = Game1.player.team.GetVirtualCurrencyAmount(itemId);
                    else
                        amt = Game1.player.GetVirtualCurrencyAmount(itemId);

                    if (amt >= minCount)
                        return amt <= maxCount;
                    return false;
                });
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.getPlayerCurrencyAmount))]
    public static class ShopMenuVirtualCurrencyAmountPatch
    {
        public static void Postfix(Farmer who, int currencyType, ref int __result)
        {
            foreach (var currency in VanillaAssetExpansion.virtualCurrencies)
            {
                if (currency.Key.GetDeterministicHashCode() == currencyType)
                {
                    if (currency.Value.TeamWide)
                        __result = who.team.GetVirtualCurrencyAmount(currency.Key);
                    else
                        __result = who.GetVirtualCurrencyAmount(currency.Key);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.chargePlayer))]
    public static class ShopMenuChargeVirtualCurrencyPatch
    {
        public static void Postfix(Farmer who, int currencyType, int amount)
        {
            foreach (var currency in VanillaAssetExpansion.virtualCurrencies)
            {
                if (currency.Key.GetDeterministicHashCode() == currencyType)
                {
                    if (currency.Value.TeamWide)
                        who.team.AddVirtualCurrencyAmount(currency.Key, -amount);
                    else
                        who.AddVirtualCurrencyAmount(currency.Key, -amount);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.HasTradeItem))]
    public static class ShopMenuHasVirtualCurrencyAsTradeItemPatch
    {
        public static void Postfix(string itemId, int count, ref bool __result)
        {
            if (itemId.StartsWith("(O)")) itemId = itemId.Substring("(O)".Length);
            else if (itemId.StartsWith("(")) return;

            if (VanillaAssetExpansion.virtualCurrencies.ContainsKey(itemId))
            {
                if (VanillaAssetExpansion.virtualCurrencies[itemId].TeamWide)
                    __result = Game1.player.team.GetVirtualCurrencyAmount(itemId) >= count;
                else
                    __result = Game1.player.GetVirtualCurrencyAmount(itemId) >= count;
            }
        }
    }


    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.ConsumeTradeItem))]
    public static class ShopMenuConsumeVirtualCurrencyAsTradeItemPatch
    {
        public static bool Prefix(string itemId, int count)
        {
            if (itemId.StartsWith("(O)")) itemId = itemId.Substring("(O)".Length);
            else if (itemId.StartsWith("(")) return true;

            if (VanillaAssetExpansion.virtualCurrencies.ContainsKey(itemId))
            {
                if (VanillaAssetExpansion.virtualCurrencies[itemId].TeamWide)
                    Game1.player.team.AddVirtualCurrencyAmount(itemId, -count);
                else
                    Game1.player.AddVirtualCurrencyAmount(itemId, -count);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), new Type[] { typeof( Item ) } )]
    public static class FarmerAlwaysAcceptVirtualCurrencyPatch1
    {
        public static bool Prefix(Item item, ref bool __result)
        {
            if (item.QualifiedItemId.StartsWith("(O)") && VanillaAssetExpansion.virtualCurrencies.ContainsKey(item.ItemId))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), new Type[] { typeof(string), typeof( int ), typeof( int ) })]
    public static class FarmerAlwaysAcceptVirtualCurrencyPatch2
    {
        public static bool Prefix(string id, int stack, int quality, ref bool __result)
        {
            if (id.StartsWith("(O)") && VanillaAssetExpansion.virtualCurrencies.ContainsKey(id))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(Farmer), nameof(Farmer.GetItemReceiveBehavior))]
    public static class FarmerAlwaysAcceptVirtualCurrencyPatch3
    {
        public static void Postfix(Item item, ref bool needsInventorySpace, ref bool showNotification)
        {
            if (item.QualifiedItemId.StartsWith("(O)") && VanillaAssetExpansion.virtualCurrencies.ContainsKey(item.ItemId))
            {
                needsInventorySpace = false;
                showNotification = true;
            }
        }
    }

    [HarmonyPatch(typeof(ShopMenu), nameof(ShopMenu.drawCurrency))]
    public static class ShopMenuDrawCurrencyPatch
    {
        public static void Postfix(ShopMenu __instance, SpriteBatch b)
        {
            string itemId = null;
            int amt = 0;
            foreach (var currency in VanillaAssetExpansion.virtualCurrencies)
            {
                if (currency.Key.GetDeterministicHashCode() == __instance.currency)
                {
                    itemId = currency.Key;
                    if (currency.Value.TeamWide)
                        amt = Game1.player.team.GetVirtualCurrencyAmount(currency.Key);
                    else
                        amt = Game1.player.GetVirtualCurrencyAmount(currency.Key);
                    break;
                }
            }

            if (itemId != null)
            {
                var data = ItemRegistry.GetDataOrErrorItem($"(O){itemId}");
                var tex = data.GetTexture();
                var sourceRect = data.GetSourceRect();

                int x = __instance.xPositionOnScreen + 36;
                int y = __instance.yPositionOnScreen + __instance.height - __instance.inventory.height - 12;
                IClickableMenu.drawTextureBox(b, x, y, 150, 80, Color.White);

                x += 32;
                y += 24;

                b.Draw(texture: tex,
                    position: new Vector2(x, y),
                    sourceRectangle: sourceRect,
                    Color.White, rotation: 0f, origin: Vector2.Zero, scale: 2f, SpriteEffects.None, layerDepth: 0f);

                b.DrawString(Game1.smallFont, text: string.Concat(amt), position: new Vector2(x + 48, y), Game1.textColor);
            }
        }
    }
}
