using System;
using System.Collections.Generic;
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
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace SpaceCore
{
    public class EquipmentMenu : MenuWithInventory
    {
        public List<ClickableTextureComponent> equipmentSlots = new();

        public const int PerRow = 4;

        public EquipmentMenu()
        : base()
        {
            xPositionOnScreen -= 40;
            yPositionOnScreen -= 80;
            width += 80;
            height += 80;
            AllowExitWithHeldItem = true;

            int PerRow = 4;
            int amt = SpaceCore.EquipmentSlots.Count;
            int wamt = Math.Min(amt, PerRow);
            int hamt = (int)Math.Ceiling(amt / (float)PerRow);
            int sx = xPositionOnScreen + 32;// + (200 * wamt + (16 * (wamt - 1))) / 2;
            int sy = yPositionOnScreen + 32; // (64 * hamt + (16 * (hamt - 1))) / 2;

            var ext = Game1.player.GetExtData();

            int ir = 0;
            foreach ( var slot in SpaceCore.EquipmentSlots)
            {
                int ix = ir % PerRow;
                int iy = ir / PerRow;
                ClickableTextureComponent ctc = new(new Rectangle(sx + ix * 216, sy + iy * 80, 64, 64), slot.Value.BackgroundTex, slot.Value.BackgroundRect ?? new(0, 0, 16, 16), 4)
                {
                    myID = 100 + ir,
                    leftNeighborID = (ix > 0) ? (100 + ir - 1) : -1,
                    rightNeighborID = (ix < PerRow - 1) ? (100 + ir + 1) : -1,
                    upNeighborID = (iy > 0) ? (100 + ir - PerRow) : -1,
                    downNeighborID = (ir < amt - PerRow) ? (100 + ir + PerRow) : inventory.inventory[0].myID,
                    label = slot.Value.DisplayName(),
                    item = SpaceCore.api.GetItemInEquipmentSlot(Game1.player, slot.Key),
                    name = slot.Key,
                };

                equipmentSlots.Add(ctc);

                if (!ext.ExtraEquippables.ContainsKey(slot.Key))
                {
                    ext.ExtraEquippables.Add(slot.Key, (Item)null);
                }

                ++ir;
            }

            List<ClickableComponent> list = base.inventory.inventory;
            if (list != null && list.Count >= 12)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (base.inventory.inventory[i] != null)
                    {
                        base.inventory.inventory[i].upNeighborID = 100 + amt - 1;
                    }
                }
            }

            if (Game1.options.SnappyMenus)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }
        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            for (int i = 0; i < equipmentSlots.Count; ++i)
            {
                var equipData = SpaceCore.EquipmentSlots[equipmentSlots[i].name];
                if (equipmentSlots[i].bounds.Contains(x, y) && equipData.SlotValidator(base.heldItem))
                {
                    NetRef<Item> slot = Game1.player.GetExtData().ExtraEquippables.FieldDict[equipmentSlots[i].name];

                    if (heldItem != null)
                        Game1.playSound("crit");
                    else if (slot.Value != null)
                        Game1.playSound("dwop");

                    heldItem = Game1.player.Equip(heldItem, slot);
                    equipmentSlots[i].item = slot.Value;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            foreach (var slot in equipmentSlots)
            {
                slot.tryHover(x, y);
                if (slot.bounds.Contains(x, y))
                {
                    hoveredItem = slot.item;
                    hoverText = slot.item?.getDescription();
                    //hoverTitle = slot.item.DisplayName;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width - IClickableMenu.borderWidth, 400, Color.White);
            //base.draw(b, drawUpperPortion: false, drawDescriptionArea: false );
            drawTextureBox(b, inventory.xPositionOnScreen - 24, inventory.yPositionOnScreen - 24, inventory.width + 24 * 2, inventory.height + 24 * 2, Color.White);
            inventory.draw(b);

            foreach (var slot in equipmentSlots)
            {
                slot.draw(b);
                slot.drawItem(b);
            }

            /*
            if (!base.hoverText.Equals(""))
            {
                IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont);
            }
            */
            if (base.hoveredItem != null)
            {
                IClickableMenu.drawToolTip(b, base.hoveredItem.getDescription(), base.hoveredItem.DisplayName, base.hoveredItem, base.heldItem != null);
            }
            base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

            if (!Game1.options.hardwareCursor)
            {
                base.drawMouse(b);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryPage), MethodType.Constructor, [typeof(int), typeof(int), typeof(int), typeof(int)])]
    public static class InventoryPageConstructorPatch
    {
        public static ConditionalWeakTable<InventoryPage, Holder<ClickableTextureComponent>> comps = new();

        public static void Postfix(InventoryPage __instance)
        {
            if (SpaceCore.EquipmentSlots.Count == 0)
                return;

            var holder = comps.GetOrCreateValue(__instance);
            holder.Value = new(new(__instance.xPositionOnScreen - 80, __instance.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), Game1.content.Load<Texture2D>("spacechase0.SpaceCore/ExtraEquipmentIcon"), new Rectangle(0, 0, 16, 16), 4)
            {
                myID = 1348000,
                rightNeighborID = InventoryPage.region_boots,
            };
            __instance.equipmentIcons.First(cc => cc.myID == InventoryPage.region_boots).leftNeighborID = 1348000;
        }
    }
    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.populateClickableComponentList))]
    public static class ClickableMeunClickableComponentPatch
    {
        public static void Postfix(IClickableMenu __instance)
        {
            if (__instance is not InventoryPage invPage)
                return;
            var holder = InventoryPageConstructorPatch.comps.GetOrCreateValue(invPage);
            if (holder.Value == null)
                return;

            __instance.allClickableComponents.Add(holder.Value);
        }
    }

    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.receiveLeftClick))]
    public static class InventoryPageLeftClickPatch
    {
        public static bool Prefix(InventoryPage __instance, int x, int y)
        {
            var holder = InventoryPageConstructorPatch.comps.GetOrCreateValue(__instance);
            if (holder.Value == null)
                return true;

            if (holder.Value.bounds.Contains(x, y))
            {
                Game1.activeClickableMenu.SetChildMenu(new EquipmentMenu());
                return false;
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(InventoryPage), nameof(InventoryPage.draw), [typeof(SpriteBatch)])]
    public static class InventoryPageDrawTooltipPatch
    {
        public static void Postfix(InventoryPage __instance, SpriteBatch b)
        {
            var holder = InventoryPageConstructorPatch.comps.GetOrCreateValue(__instance);
            if (holder.Value == null)
                return;

            holder.Value.draw(b);
        }
    }

    // I'm putting this here out of laziness
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.GetEquippedItems))]
    public static class FarmerExtraEquippablesPatch
    {
        public static IEnumerable<Item> Postfix(IEnumerable<Item> items, Farmer __instance)
        {
            foreach (var ret in items)
                yield return ret;

            foreach (var entry in __instance.GetExtData().ExtraEquippables.Pairs)
            {
                if (!SpaceCore.EquipmentSlots.ContainsKey(entry.Key) || entry.Value == null)
                    continue;
                yield return entry.Value;
            }
        }
    }
}
