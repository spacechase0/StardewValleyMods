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

        private ClickableTextureComponent trashCan;
        public float trashCanLidRotation;

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
                        //slotClicked = (elem as ItemSlot);
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

            {
                var buttonTex = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/buttons.png");
                var depositAllButton = new Button()
                {
                    Texture = buttonTex,
                    IdleTextureRect = new Rectangle(60, 0, 60, 60),
                    HoverTextureRect = new Rectangle(60, 0, 60, 60),
                    LocalPosition = new Vector2(width - 60 - 4, 16),
                };
                depositAllButton.Callback = (elem) =>
                {
                    Game1.playSound("stoneStep");
                    for (int i = Farmer.hotbarSize; i < Game1.player.Items.Count; ++i)
                    {
                        var item = Game1.player.Items[i];
                        if (item != null && item != satchel && (item is not Satchel s2 || s2.hasEnchantmentOfType<SatchelInceptionEnchantment>()))
                        {
                            Game1.player.Items[i] = satchel.Inventory.DepositItem(item);
                        }
                    }
                };
                container.AddChild(depositAllButton);

                var withdrawAllButton = new Button()
                {
                    Texture = buttonTex,
                    IdleTextureRect = new Rectangle(0, 0, 60, 60),
                    HoverTextureRect = new Rectangle(0, 0, 60, 60),
                    LocalPosition = new Vector2(width - 60 - 4, 16 + 60 + 4),
                };
                withdrawAllButton.Callback = (elem) =>
                {
                    Game1.playSound("stoneStep");
                    for (int i = 0; i < satchel.Inventory.Count; ++i)
                    {
                        var item = satchel.Inventory[i];
                        if (item != null)
                        {
                            satchel.Inventory[i] = Game1.player.Items.DepositItem(item);
                        }
                    }
                };
                container.AddChild(withdrawAllButton);
            }

            this.trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - Game1.tileSize * 3 - Game1.tileSize / 2 - borderWidth - 104, Game1.tileSize, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), Game1.pixelZoom);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                foreach (var item in invMenu.inventory)
                {
                    if (!item.containsPoint(x, y))
                        continue;
                    int slotNum = Convert.ToInt32(item.name);
                    if (invMenu.actualInventory.Count <= slotNum)
                        continue;
                    if (invMenu.actualInventory[slotNum] == null)
                        break;
                    if (invMenu.actualInventory[slotNum] is Satchel && !allowNestedSatchels)
                        break;

                    invMenu.actualInventory[slotNum] = satchel.Inventory.DepositItem(invMenu.actualInventory[slotNum]);
                    Game1.playSound("dwop");
                    break;
                }
            }
            else
            {
                Game1.player.CursorSlotItem = invMenu.leftClick(x, y, Game1.player.CursorSlotItem, playSound);
            }

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                if (!(slot.UserData as SlotUserData).Filter( Game1.player.CursorSlotItem ) )
                {
                    return;
                }
                slotClicked = slot;

                if (slot.Item == null && Game1.player.CursorSlotItem != null)
                {
                    Game1.playSound("stoneStep");
                    slot.Item = Game1.player.CursorSlotItem;
                    Game1.player.CursorSlotItem = null;
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem == null)
                {
                    Game1.playSound("dwop");
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                    {
                        slot.Item = Game1.player.Items.DepositItem(slot.Item);
                    }
                    else
                    {
                        Game1.player.CursorSlotItem = slot.Item;
                        slot.Item = null;
                    }
                }
                else if (slot.Item != null && Game1.player.CursorSlotItem != null)
                {
                    Game1.playSound("stoneStep");
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

            if (trashCan != null && trashCan.containsPoint(x, y) && Game1.player.CursorSlotItem != null && Game1.player.CursorSlotItem.canBeTrashed())
            {
                Utility.trashItem(Game1.player.CursorSlotItem);
                Game1.player.CursorSlotItem = null;
            }
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

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                if (!(slot.UserData as SlotUserData).Filter(Game1.player.CursorSlotItem) ||
                    !(slot.UserData as SlotUserData).IsMainInventory)
                {
                    return;
                }
                slotClicked = slot;

                if (slot.Item == null && Game1.player.CursorSlotItem != null)
                {
                    Game1.playSound("stoneStep");
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
                    Game1.playSound("dwop");
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
                    Game1.playSound("stoneStep");
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

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            if (trashCan != null)
            {
                if (trashCan.containsPoint(x, y))
                {
                    if (trashCanLidRotation <= 0f) Game1.playSound("trashcanlid");
                    trashCanLidRotation = Math.Min(trashCanLidRotation + (float)(Math.PI / 48f), (float)Math.PI / 2f);

                    /*
                    if (Game1.player.CursorSlotItem != null && Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player) > 0)
                    {
                        hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
                        hoverAmount = Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player);
                    }
                    */
                }
                else
                {
                    trashCanLidRotation = Math.Max(trashCanLidRotation - (float)(Math.PI / 48f), 0f);
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


            if (trashCan != null)
            {
                trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, trashCanLidRotation, new Vector2(16, 10), Game1.pixelZoom, SpriteEffects.None, .86f);
            }

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
