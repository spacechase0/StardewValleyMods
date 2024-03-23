using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Spawnables;
using SpaceCore.UI;
using StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace Arsenal;

public class ArsenalMenu : IClickableMenu
{
    private RootElement ui;
    private ItemSlot weaponSlot;
    private ItemSlot gemSlot, coatingSlot, alloyingSlot;
    private InventoryMenu invMenu;

    public ArsenalMenu()
    :   base( Game1.uiViewport.Width / 2 - 350 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - 150 - 100 - IClickableMenu.borderWidth, 700 + IClickableMenu.borderWidth * 2, 300 + IClickableMenu.borderWidth * 2 )
    {
        invMenu = new(Game1.uiViewport.Width / 2 - 72 * 5 - 36 + 8, yPositionOnScreen + height, true, highlightMethod:
            (item) =>
            {
                return (item == null || item is MeleeWeapon mw || item.HasContextTag("exquisite_gem") ||
                        Mod.CoatingIconMapping.ContainsKey(item.QualifiedItemId) ||
                        Mod.AlloyIconMapping.ContainsKey(item.QualifiedItemId));
            });

        ui = new RootElement();
        StaticContainer container = new()
        {
            LocalPosition = new(this.xPositionOnScreen, this.yPositionOnScreen),
            Size = new(this.width, height),
        };
        ui.AddChild(container);

        weaponSlot = new ItemSlot()
        {
            LocalPosition = new(100, height / 2 - 96 / 2),
            ItemDisplay = new MeleeWeapon("0" ),
            TransparentItemDisplay = true,
        };
        this.weaponSlot.Callback = this.weaponSlot.SecondaryCallback = (elem) =>
        {
            if (Game1.player.CursorSlotItem != null && Game1.player.CursorSlotItem is not MeleeWeapon)
                return;

            if (Game1.player.CursorSlotItem == null && this.weaponSlot.Item == null)
                return;

            if (weaponSlot.Item != null)
            {
                SyncSlotsToWeapon();
            }

            (this.weaponSlot.Item, Game1.player.CursorSlotItem) = (Game1.player.CursorSlotItem, this.weaponSlot.Item);

            SyncWeaponToSlots();
        };
        container.AddChild(this.weaponSlot);

        void ModifierSlotShenanigans(ItemSlot modifierSlot, int reqQty)
        {
            if (this.weaponSlot.Item == null)
                return;

            if (Game1.player.CursorSlotItem == null && modifierSlot.Item == null)
                return;

            if (Game1.player.CursorSlotItem?.QualifiedItemId != modifierSlot.Item?.QualifiedItemId &&
                (modifierSlot.Item != null && (Game1.player.CursorSlotItem?.Stack ?? reqQty) != reqQty))
                return;

            Game1.playSound("dwop");

            string cursorId = Game1.player.CursorSlotItem?.QualifiedItemId;
            if (cursorId == null)
            {
                Game1.player.CursorSlotItem = modifierSlot.Item;
                modifierSlot.Item = null;
            }
            else if (modifierSlot.Item != null)
            {
                (Game1.player.CursorSlotItem, modifierSlot.Item) = (modifierSlot.Item, Game1.player.CursorSlotItem);
            }
            else
            {
                modifierSlot.Item = Game1.player.CursorSlotItem.getOne();
                modifierSlot.Item.Stack = reqQty;
                if (Game1.player.CursorSlotItem.Stack <= reqQty)
                    Game1.player.CursorSlotItem = null;
                else
                    Game1.player.CursorSlotItem.Stack -= reqQty;
            }
            this.SyncSlotsToWeapon();
        }

        string spacing = "  ";
        string newline = "\n";

        this.gemSlot = new ItemSlot()
        {
            LocalPosition = new(250, height / 4 - 64 / 2),
            BoxIsThin = true,
            ItemDisplay = new StardewValley.Object("spacechase0.Arsenal_ExquisiteDiamond", 1),
            TransparentItemDisplay = true,
            UserData = $"{I18n.Anvil_ValidOptions()}:\n{string.Join(newline, Mod.ExquisiteGemMappings.Values.Select(s => (spacing + ItemRegistry.GetData(s).DisplayName)))}"
        };
        this.gemSlot.Callback = this.gemSlot.SecondaryCallback = (elem) =>
        {
            if (Game1.player.CursorSlotItem != null &&
                !Game1.player.CursorSlotItem.HasContextTag("exquisite_gem"))
            {

                Game1.addHUDMessage(new HUDMessage(I18n.Anvil_NotHere()));
                return;
            }
            ModifierSlotShenanigans(gemSlot, 1);
        };
        container.AddChild(gemSlot);
        var gemLabel = new Label()
        {
            LocalPosition = this.gemSlot.LocalPosition + new Vector2(this.gemSlot.Bounds.Width + 16, 0),
            String = I18n.Anvil_GemSocket(),
        };
        container.AddChild(gemLabel);

        this.coatingSlot = new ItemSlot()
        {
            LocalPosition = new(250, height / 4 * 2 - 64 / 2),
            BoxIsThin = true,
            ItemDisplay = new StardewValley.Object("766", 1),
            TransparentItemDisplay = true,
            UserData = $"{I18n.Anvil_ValidOptions()}:\n{string.Join(newline, Mod.CoatingIconMapping.Keys.Select(s => (spacing + ItemRegistry.GetData(s).DisplayName)))}"
        };
        this.coatingSlot.Callback = this.coatingSlot.SecondaryCallback = (elem) =>
        {
            if (Game1.player.CursorSlotItem != null &&
                !Mod.CoatingIconMapping.ContainsKey(Game1.player.CursorSlotItem.QualifiedItemId))
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Anvil_NotHere()));
                return;
            }

            if (Game1.player.CursorSlotItem != null &&
                Game1.player.CursorSlotItem.Stack < Mod.CoatingQuantities[ Game1.player.CursorSlotItem.QualifiedItemId ])
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Anvil_NotEnough(Mod.CoatingQuantities[ Game1.player.CursorSlotItem.QualifiedItemId ])));
                return;
            }

            ModifierSlotShenanigans(coatingSlot,
                Game1.player.CursorSlotItem != null
                    ? Mod.CoatingQuantities[Game1.player.CursorSlotItem.QualifiedItemId]
                    : 1);
        };
        container.AddChild(coatingSlot);
        var coatingLabel = new Label()
        {
            LocalPosition = this.coatingSlot.LocalPosition + new Vector2(this.coatingSlot.Bounds.Width + 16, 0),
            String = I18n.Anvil_Coating(),
        };
        container.AddChild(coatingLabel);

        this.alloyingSlot = new ItemSlot()
        {
            LocalPosition = new(250, height / 4 * 3 - 64 / 2),
            BoxIsThin = true,
            ItemDisplay = new StardewValley.Object("334", 1),
            TransparentItemDisplay = true,
            UserData = $"{I18n.Anvil_ValidOptions()}:\n{string.Join(newline, Mod.AlloyIconMapping.Keys.Select(s => (spacing + ItemRegistry.GetData(s).DisplayName)))}"
        };
        this.alloyingSlot.Callback = this.alloyingSlot.SecondaryCallback = (elem) =>
        {
            if (Game1.player.CursorSlotItem != null &&
                !Mod.AlloyIconMapping.ContainsKey(Game1.player.CursorSlotItem.QualifiedItemId))
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Anvil_NotHere()));
                return;
            }

            if (Game1.player.CursorSlotItem != null &&
                Game1.player.CursorSlotItem.Stack < 25)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.Anvil_NotEnough(25)));
                return;
            }

            ModifierSlotShenanigans(alloyingSlot, 25);
        };
        container.AddChild(alloyingSlot);
        var alloyingLabel = new Label()
        {
            LocalPosition = this.alloyingSlot.LocalPosition + new Vector2(this.alloyingSlot.Bounds.Width + 16, 0),
            String = I18n.Anvil_Alloying(),
        };
        container.AddChild(alloyingLabel);
    }

    public override void update(GameTime time)
    {
        base.update(time);
        this.ui.Update();
        this.invMenu.update(time);
    }

    public override void draw(SpriteBatch b)
    {
        base.draw(b);

        IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White );
        IClickableMenu.drawTextureBox(b, this.invMenu.xPositionOnScreen - IClickableMenu.borderWidth, this.invMenu.yPositionOnScreen - IClickableMenu.borderWidth, this.invMenu.width + IClickableMenu.borderWidth * 2, this.invMenu.height + IClickableMenu.borderWidth * 2, Color.White);

        this.ui.Draw(b);
        this.invMenu.draw(b);

        // We use these in this menu, so draw them again here
        if (Game1.hudMessages.Count > 0)
        {
            int heightUsed = 0;
            for (int i = Game1.hudMessages.Count - 1; i >= 0; i--)
            {
                Game1.hudMessages[i].draw(Game1.spriteBatch, i, ref heightUsed);
            }
        }

        drawMouse(b);

        Game1.player.CursorSlotItem?.drawInMenu(b, Game1.getMousePosition().ToVector2(), 1);

        string GetDescription(Item item)
        {
            string desc = item.getDescription();
            if (item.HasContextTag("exquisite_gem"))
            {
                // So the gunther donate text doesn't show
                desc = ItemRegistry.GetData(item.QualifiedItemId).Description;
            }
            else if (Mod.CoatingIconMapping.ContainsKey(item.QualifiedItemId))
            {
                desc = I18n.GetByKey($"description.coating.{item.QualifiedItemId}");
            }
            else if (Mod.AlloyIconMapping.ContainsKey(item.QualifiedItemId))
            {
                desc = I18n.GetByKey($"description.alloying.{item.QualifiedItemId}");
            }

            return desc;
        }

        if (ItemWithBorder.HoveredElement != null)
        {
            if (ItemWithBorder.HoveredElement is ItemSlot slot && slot.Item != null)
            {
                drawToolTip(b, GetDescription(slot.Item), slot.Item.DisplayName, slot.Item);
            }
            else if (ItemWithBorder.HoveredElement.UserData is string s)
            {
                drawToolTip(b, s, null, null);
            }
        }
        else
        {
            var hover = invMenu.hover(Game1.getMouseX(), Game1.getMouseY(), null);
            if (hover != null)
            {
                drawToolTip(b, GetDescription(hover), invMenu.hoverTitle, hover);
            }
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        Game1.player.CursorSlotItem = this.invMenu.leftClick(x, y, Game1.player.CursorSlotItem, playSound);
    }

    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        base.receiveRightClick(x, y, playSound);
        Game1.player.CursorSlotItem = this.invMenu.rightClick(x, y, Game1.player.CursorSlotItem, playSound);
    }

    protected override void cleanupBeforeExit()
    {
        base.cleanupBeforeExit();
        if (this.weaponSlot.Item != null)
            Game1.player.addItemByMenuIfNecessary(this.weaponSlot.Item);
    }

    public override void emergencyShutDown()
    {
        base.cleanupBeforeExit();
        if (this.weaponSlot.Item != null)
            Game1.player.addItemByMenuIfNecessary(this.weaponSlot.Item);
    }

    private void SyncSlotsToWeapon()
    {
        if (this.weaponSlot.Item == null)
            return;

        (this.weaponSlot.Item as MeleeWeapon).SetExquisiteGemstone(this.gemSlot.Item?.QualifiedItemId);
        (this.weaponSlot.Item as MeleeWeapon).SetBladeCoating(this.coatingSlot.Item?.QualifiedItemId);
        (this.weaponSlot.Item as MeleeWeapon).SetBladeAlloying(this.alloyingSlot.Item?.QualifiedItemId);
    }

    private void SyncWeaponToSlots()
    {
        if (this.weaponSlot.Item == null)
        {
            this.gemSlot.Item = null;
            this.coatingSlot.Item = null;
            this.alloyingSlot.Item = null;
            return;
        }

        var mw = this.weaponSlot.Item as MeleeWeapon;

        this.gemSlot.Item = mw.GetExquisiteGemstone() != null ? new StardewValley.Object(mw.GetExquisiteGemstone(), 1) : null;
        this.coatingSlot.Item = mw.GetBladeCoating() != null ? new StardewValley.Object(mw.GetBladeCoating(), Mod.CoatingQuantities[ mw.GetBladeCoating() ] ) : null;
        this.alloyingSlot.Item = mw.GetBladeAlloying() != null ? new StardewValley.Object(mw.GetBladeAlloying(), 25) : null;
    }
}
