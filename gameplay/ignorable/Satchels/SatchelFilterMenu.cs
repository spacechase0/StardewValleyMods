using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared.UI;
using StardewValley;
using StardewValley.Menus;

namespace Satchels
{
    internal class SatchelFilterMenu : IClickableMenu
    {
        private Item upgrade;

        private RootElement ui;
        private InventoryMenu invMenu;

        private ItemSlot slotClicked = null;

        private List<ItemSlot> slots = new();

        public SatchelFilterMenu(Item upgrade)
        : base(Game1.uiViewport.Width / 2 - 64 * 6 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - (64 * (3 + 3)) / 2 - IClickableMenu.borderWidth, 64 * 12, 72 * (3 + 3) + IClickableMenu.borderWidth * 2)
        {
            this.upgrade = upgrade;

            invMenu = new(Game1.uiViewport.Width / 2 - 72 * 6 + 8, yPositionOnScreen + height - 64 * 3 - 24, true);

            for (int ii = 0, ic = 0; ii < Game1.player.Items.Count; ++ii, ++ic)
            {
                if (Game1.player.Items[ii] is Satchel s && s.isOpen.Value)
                {
                    // TODO: Gamepad support
                    invMenu.inventory[ic].visible = false;
                }
            }

            ui = new();
            StaticContainer container = new()
            {
                LocalPosition = new(xPositionOnScreen, yPositionOnScreen),
                Size = new(width, height),
            };
            ui.AddChild(container);

            Item[] ids = new Item[15];
            if (upgrade.modData.TryGetValue(Mod.AutoPickupDataKey, out string data))
            {
                string[] split = data.Split('/');
                for (int i = 0; i < Math.Min( ids.Length, split.Length ); ++i)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                        ids[i] = ItemRegistry.Create(split[i]);
                }
            }

            for (int iy = 0; iy < 3; ++iy)
            {
                for (int ix = 0; ix < 5; ++ix)
                {
                    int i = ix + iy * 5;

                    var slot = new ItemSlot()
                    {
                        LocalPosition = new(ix * 64 + (width - 64 * 5) / 2, iy * 64 + IClickableMenu.borderWidth),
                        BoxIsThin = true,
                        Item = ids[i],
                    };
                    slot.Callback = (elem) =>
                    {
                        if (Game1.player.CursorSlotItem != null)
                        {
                            slot.Item = ItemRegistry.Create(Game1.player.CursorSlotItem.QualifiedItemId);
                            Game1.playSound("stoneStep");
                        }
                        else
                        {
                            Game1.playSound("dwop");
                            slot.Item = null;
                        }
                        UpdateFilters();
                    };
                    slot.SecondaryCallback = (elem) =>
                    {
                        Game1.playSound("dwop");
                        slot.Item = null;
                        UpdateFilters();
                    };
                    container.AddChild(slot);
                    slots.Add(slot);
                }
            }
        }

        private void UpdateFilters()
        {
            string str = "";
            foreach (var slot in slots)
            {
                str += (slot.Item?.QualifiedItemId ?? "") +"/";
            }
            upgrade.modData[Mod.AutoPickupDataKey] = (str == "") ? "" : str.Substring(0, str.Length - 1);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            Game1.player.CursorSlotItem = invMenu.leftClick(x, y, Game1.player.CursorSlotItem, playSound);

        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);

            foreach (var item in invMenu.inventory)
            {
                if (!item.containsPoint(x, y))
                    continue;
                int slotNum = Convert.ToInt32(item.name);
                if (invMenu.actualInventory.Count <= slotNum)
                    continue;
                if (invMenu.actualInventory[slotNum] == null)
                {
                    Game1.playSound("stoneStep");
                    if (Game1.player.CursorSlotItem.Stack > 1)
                    {
                        invMenu.actualInventory[slotNum] = Game1.player.CursorSlotItem.getOne();
                        invMenu.actualInventory[slotNum].Stack = 1;
                        Game1.player.CursorSlotItem.Stack -= 1;
                    }
                    else
                    {
                        invMenu.actualInventory[slotNum] = Game1.player.CursorSlotItem;
                        Game1.player.CursorSlotItem = null;
                    }
                    break;
                }

                if (Game1.player.CursorSlotItem == null)
                {
                    Game1.playSound("dwop");
                    if (invMenu.actualInventory[slotNum].Stack > 1)
                    {
                        Game1.player.CursorSlotItem = invMenu.actualInventory[slotNum].getOne();
                        Game1.player.CursorSlotItem.Stack = invMenu.actualInventory[slotNum].Stack / 2;
                        invMenu.actualInventory[slotNum].Stack = (int)Math.Ceiling(invMenu.actualInventory[slotNum].Stack / 2f);
                    }
                    else
                    {
                        Game1.player.CursorSlotItem = invMenu.actualInventory[slotNum];
                        invMenu.actualInventory[slotNum] = null;
                    }
                }
                else
                {
                    Game1.playSound("stoneStep");
                    if (invMenu.actualInventory[slotNum].canStackWith(Game1.player.CursorSlotItem))
                    {
                        var one = Game1.player.CursorSlotItem.getOne();
                        one.Stack = 1;
                        int left = invMenu.actualInventory[slotNum].addToStack(one);
                        if (left <= 0)
                        {
                            Game1.player.CursorSlotItem.Stack--;
                            if (Game1.player.CursorSlotItem.Stack <= 0)
                            {
                                Game1.player.CursorSlotItem = null;
                            }
                        }
                    }
                    else
                    {
                        var tmp = Game1.player.CursorSlotItem;
                        Game1.player.CursorSlotItem = invMenu.actualInventory[slotNum];
                        invMenu.actualInventory[slotNum] = tmp;
                    }
                }

                break;
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);

            ui.Update();

            invMenu.update(time);
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, xPositionOnScreen - IClickableMenu.borderWidth / 2, yPositionOnScreen, width + IClickableMenu.borderWidth, height, Color.White);

            ui.Draw(b);
            invMenu.draw(b);

            Game1.player.CursorSlotItem?.drawInMenu(b, Game1.getMousePosition().ToVector2(), 1);

            if (ItemWithBorder.HoveredElement != null)
            {
                if (ItemWithBorder.HoveredElement is ItemSlot slot && slot.Item != null)
                {
                    drawToolTip(b, slot.Item.getDescription(), slot.Item.DisplayName, slot.Item);
                }
                else if (ItemWithBorder.HoveredElement.ItemDisplay != null)
                {
                    drawToolTip(b, ItemWithBorder.HoveredElement.ItemDisplay.getDescription(), ItemWithBorder.HoveredElement.ItemDisplay.DisplayName, ItemWithBorder.HoveredElement.ItemDisplay);
                }
            }
            else
            {
                var hover = invMenu.hover(Game1.getMouseX(), Game1.getMouseY(), null);
                if (hover != null)
                {
                    drawToolTip(b, invMenu.hoverText, invMenu.hoverTitle, hover);
                }
            }

            drawMouse(b);
        }
    }
}
