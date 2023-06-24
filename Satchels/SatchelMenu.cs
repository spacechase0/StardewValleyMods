using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.UI;
using StardewValley;
using StardewValley.Menus;

namespace Satchels
{
    internal class SatchelMenu : IClickableMenu
    {
        private class SlotUserData
        {
            public Func<Item, bool> Filter;
            public bool IsMainInventory { get; set; } = true;
            public int Slot { get; set; }
        }

        private Satchel satchel;
        private bool allowNestedSatchels = false;

        private RootElement ui;
        private InventoryMenu invMenu;

        private ItemSlot slotClicked = null;

        private List<ItemSlot> slots = new();

        public SatchelMenu(Satchel satchel)
        : base(Game1.uiViewport.Width / 2 - 64 * 6 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - (64 * (satchel.Inventory.Count / 9 + 3)) / 2 - IClickableMenu.borderWidth, 64 * 12, 72 * (satchel.Inventory.Count / 9 + 3) + IClickableMenu.borderWidth * 2)
        {
            this.satchel = satchel;
            satchel.isOpen.Value = true;
            var data = SatchelDataDefinition.GetSpecificData(satchel.ItemId);
            if (satchel.hasEnchantmentOfType<SatchelInceptionEnchantment>())
            {
                allowNestedSatchels = true;
            }

            invMenu = new(Game1.uiViewport.Width / 2 - 72 * 6 + 8, yPositionOnScreen + height - 64 * 3 - 24, true);

            for (int ii = 0, ic = 0; ii < Game1.player.Items.Count; ++ii, ++ic)
            {
                if (Game1.player.Items[ii] is Satchel s && s.isOpen.Value)
                {
                    // TODO: Gamepad support
                    invMenu.inventory.RemoveAt(ic--);
                }
            }

            ui = new();
            StaticContainer container = new()
            {
                LocalPosition = new(xPositionOnScreen, yPositionOnScreen),
                Size = new(width, height),
            };
            ui.AddChild(container);

            for (int iy = 0; iy < satchel.Inventory.Count / 9; ++iy)
            {
                for (int ix = 0; ix < 9; ++ix)
                {
                    int i = ix + iy * 9;
                    if (i >= satchel.Inventory.Count) continue;

                    var slot = new ItemSlot()
                    {
                        LocalPosition = new(ix * 64 + (width - 64 * 9) / 2, iy * 64 + IClickableMenu.borderWidth),
                        Item = satchel.Inventory[i],
                        BoxIsThin = true,
                    };
                    slot.Callback = (elem) =>
                    {
                        slotClicked = (elem as ItemSlot);
                    };
                    slot.SecondaryCallback = slot.Callback;
                    slot.UserData = new SlotUserData() { Slot = i, Filter = (item) =>
                    {
                        if (item is Satchel && !allowNestedSatchels)
                        {
                            Game1.addHUDMessage(new HUDMessage(I18n.Error_Nested(), HUDMessage.error_type));
                            return false;
                        }
                        return true;
                    }
                    };
                    container.AddChild(slot);
                    slots.Add(slot);
                }
            }

            for (int i = 0; i < satchel.Upgrades.Count; ++i)
            {
                int ix = -4;
                int iy = i * 88 + IClickableMenu.borderWidth / 2;

                var slot = new ItemSlot()
                {
                    LocalPosition = new(ix, iy),
                    Item = satchel.Upgrades[i],
                };
                slot.Callback = (elem) =>
                {
                    slotClicked = (elem as ItemSlot);
                };
                slot.SecondaryCallback = (elem) =>
                {
                    slotClicked = (elem as ItemSlot);

                    var theMenu = Game1.activeClickableMenu;
                    while (theMenu.GetChildMenu() != null)
                    {
                        theMenu = theMenu.GetChildMenu();
                    }
                    theMenu.SetChildMenu(Mod.GetSatchelUpgradeMenu(satchel, (elem as ItemSlot).Item));
                };
                slot.UserData = new SlotUserData() { IsMainInventory = false, Slot = i, Filter = ( item ) =>
                {
                    return item == null || (Mod.UpgradeList.ContainsKey(item.ItemId));
                }
                };
                container.AddChild(slot);
                slots.Add(slot);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            Game1.player.CursorSlotItem = invMenu.leftClick(x, y, Game1.player.CursorSlotItem, playSound);

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                if (!(slot.UserData as SlotUserData).Filter( Game1.player.CursorSlotItem ) )
                {
                    return;
                }

                if (slot.Item == null && Game1.player.CursorSlotItem != null)
                {
                    slot.Item = Game1.player.CursorSlotItem;
                    Game1.player.CursorSlotItem = null;
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem == null)
                {
                    Game1.player.CursorSlotItem = slot.Item;
                    slot.Item = null;
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem != null)
                {
                    if (slot.Item.canStackWith(Game1.player.CursorSlotItem))
                    {
                        int left = slot.Item.addToStack(Game1.player.CursorSlotItem);
                        Game1.player.CursorSlotItem.Stack = left;
                        if (left <= 0)
                            Game1.player.CursorSlotItem = null;
                    }
                    else
                    {
                        var tmp = Game1.player.CursorSlotItem;
                        Game1.player.CursorSlotItem = slot.Item;
                        slot.Item = tmp;
                    }
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
            Game1.player.CursorSlotItem = invMenu.rightClick(x, y, Game1.player.CursorSlotItem, playSound);

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                if (!(slot.UserData as SlotUserData).Filter(Game1.player.CursorSlotItem) ||
                    !(slot.UserData as SlotUserData).IsMainInventory)
                {
                    return;
                }

                if (slot.Item == null && Game1.player.CursorSlotItem != null)
                {
                    if (Game1.player.CursorSlotItem.Stack > 1)
                    {
                        slot.Item = Game1.player.CursorSlotItem.getOne();
                        slot.Item.Stack = 1;
                        Game1.player.CursorSlotItem.Stack -= 1;
                    }
                    else
                    {
                        slot.Item = Game1.player.CursorSlotItem;
                        Game1.player.CursorSlotItem = null;
                    }
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem == null)
                {
                    if (slot.Item.Stack > 1)
                    {
                        Game1.player.CursorSlotItem = slot.Item.getOne();
                        Game1.player.CursorSlotItem.Stack = slot.Item.Stack / 2;
                        slot.Item.Stack = (int)Math.Ceiling(slot.Item.Stack / 2f);
                    }
                    else
                    {
                        Game1.player.CursorSlotItem = slot.Item;
                        slot.Item = null;
                    }
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem != null)
                {
                    if (slot.Item.canStackWith(Game1.player.CursorSlotItem))
                    {
                        var one = Game1.player.CursorSlotItem.getOne();
                        one.Stack = 1;
                        int left = slot.Item.addToStack(one);
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
                        Game1.player.CursorSlotItem = slot.Item;
                        slot.Item = tmp;
                    }
                }
            }
        }

        public override void update(GameTime time)
        {
            base.update(time);

            if (slotClicked != null)
            {
                var data = (slotClicked.UserData as SlotUserData);
                if (data.IsMainInventory)
                    satchel.Inventory[data.Slot] = slotClicked.Item;
                else
                    satchel.Upgrades[data.Slot] = slotClicked.Item;
                slotClicked = null;
            }

            foreach (var slot in slots)
            {
                var data = (slot.UserData as SlotUserData);
                if (data.IsMainInventory)
                    slot.Item = satchel.Inventory[data.Slot];
                else
                    slot.Item = satchel.Upgrades[data.Slot];
            }

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

        protected override void cleanupBeforeExit()
        {
            base.cleanupBeforeExit();
            satchel.isOpen.Value = false;
        }

        public override void emergencyShutDown()
        {
            base.emergencyShutDown();
            satchel.isOpen.Value = false;
        }
    }
}
