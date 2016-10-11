using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Object = StardewValley.Object;

namespace CookingSkill
{
    public class NewCraftingPage : IClickableMenu
    {
        public const int howManyRecipesFitOnPage = 40;

        private string descriptionText = "";

        private string hoverText = "";

        private Item hoverItem;

        private Item lastCookingHover;

        private InventoryMenu inventory;

        private Item heldItem;

        private List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

        private int currentCraftingPage;

        private CraftingRecipe hoverRecipe;

        private ClickableTextureComponent upButton;

        private ClickableTextureComponent downButton;

        private bool cooking;

        public ClickableTextureComponent trashCan;

        public float trashCanLidRotation;

        private string hoverTitle = "";

        public NewCraftingPage(int x, int y, int width, int height, bool cooking = false)
            : base(x, y, width, height, false)
        {
            this.cooking = cooking;
            this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + Game1.tileSize * 5 - Game1.tileSize / 4, false, null, null, -1, 3, 0, 0, true);
            this.inventory.showGrayedOutSlots = true;
            int num = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int num2 = this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int arg_CE_0 = Game1.tileSize;
            int num3 = 8;
            int num4 = 10;
            int num5 = -1;
            if (cooking)
            {
                base.initializeUpperRightCloseButton();
            }
            SerializableDictionary<string, int> serializableDictionary = new SerializableDictionary<string, int>();
            foreach (string current in CraftingRecipe.craftingRecipes.Keys)
            {
                if (Game1.player.craftingRecipes.ContainsKey(current))
                {
                    serializableDictionary.Add(current, Game1.player.craftingRecipes[current]);
                }
            }
            Game1.player.craftingRecipes = serializableDictionary;
            this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 4, this.yPositionOnScreen + height - Game1.tileSize * 3 - Game1.tileSize / 2 - IClickableMenu.borderWidth - 104, Game1.tileSize, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), (float)Game1.pixelZoom, false);
            List<string> list = new List<string>();
            if (!cooking)
            {
                using (Dictionary<string, int>.KeyCollection.Enumerator enumerator2 = Game1.player.craftingRecipes.Keys.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        string current2 = enumerator2.Current;
                        list.Add(new string(current2.ToCharArray()));
                    }
                    goto IL_26B;
                }
            }
            Game1.playSound("bigSelect");
            foreach (string current3 in CraftingRecipe.cookingRecipes.Keys)
            {
                list.Add(new string(current3.ToCharArray()));
            }
        IL_26B:
            int num6 = 0;
            while (list.Count > 0)
            {
                CraftingRecipe craftingRecipe;
                int index;
                ClickableTextureComponent clickableTextureComponent;
                bool flag;
                do
                {
                    num5++;
                    if (num5 % 40 == 0)
                    {
                        this.pagesOfCraftingRecipes.Add(new Dictionary<ClickableTextureComponent, CraftingRecipe>());
                    }
                    int num7 = num5 / num4 % (40 / num4);
                    craftingRecipe = new CraftingRecipe(list[num6], cooking);
                    int num8 = list.Count<string>();
                    while (craftingRecipe.bigCraftable && num7 == 40 / num4 - 1 && num8 > 0)
                    {
                        num6 = (num6 + 1) % list.Count;
                        num8--;
                        craftingRecipe = new CraftingRecipe(list[num6], false);
                        if (num8 == 0)
                        {
                            num5 += 40 - num5 % 40;
                            num7 = num5 / num4 % (40 / num4);
                            this.pagesOfCraftingRecipes.Add(new Dictionary<ClickableTextureComponent, CraftingRecipe>());
                        }
                    }
                    index = num5 / 40;
                    clickableTextureComponent = new ClickableTextureComponent("", new Rectangle(num + num5 % num4 * (Game1.tileSize + num3), num2 + num7 * (Game1.tileSize + 8), Game1.tileSize, craftingRecipe.bigCraftable ? (Game1.tileSize * 2) : Game1.tileSize), null, (cooking && !Game1.player.cookingRecipes.ContainsKey(craftingRecipe.name)) ? "ghosted" : "", craftingRecipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, craftingRecipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, craftingRecipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, craftingRecipe.getIndexOfMenuView(), 16, 16), (float)Game1.pixelZoom, false);
                    flag = false;
                    foreach (ClickableTextureComponent current4 in this.pagesOfCraftingRecipes[index].Keys)
                    {
                        if (current4.bounds.Intersects(clickableTextureComponent.bounds))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                while (flag);
                this.pagesOfCraftingRecipes[index].Add(clickableTextureComponent, craftingRecipe);
                list.RemoveAt(num6);
                num6 = 0;
            }
            if (this.pagesOfCraftingRecipes.Count<Dictionary<ClickableTextureComponent, CraftingRecipe>>() > 1)
            {
                this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, num2, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false);
                this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, num2 + Game1.tileSize * 3 + Game1.tileSize / 2, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false);
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
            if (key.Equals(Keys.Delete) && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((this.heldItem as StardewValley.Object).parentSheetIndex))
                {
                    Game1.player.specialItems.Remove((this.heldItem as StardewValley.Object).parentSheetIndex);
                }
                this.heldItem = null;
                Game1.playSound("trashcan");
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentCraftingPage > 0)
            {
                this.currentCraftingPage--;
                Game1.playSound("shwip");
                return;
            }
            if (direction < 0 && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                this.currentCraftingPage++;
                Game1.playSound("shwip");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, true);
            this.heldItem = this.inventory.leftClick(x, y, this.heldItem, true);
            if (this.upButton != null && this.upButton.containsPoint(x, y))
            {
                if (this.currentCraftingPage > 0)
                {
                    Game1.playSound("coin");
                }
                this.currentCraftingPage = Math.Max(0, this.currentCraftingPage - 1);
                this.upButton.scale = this.upButton.baseScale;
            }
            if (this.downButton != null && this.downButton.containsPoint(x, y))
            {
                if (this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
                {
                    Game1.playSound("coin");
                }
                this.currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this.currentCraftingPage + 1);
                this.downButton.scale = this.downButton.baseScale;
            }
            foreach (ClickableTextureComponent current in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                int num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (int i = 0; i < num; i++)
                {
                    if (current.containsPoint(x, y) && !current.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][current].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : null))
                    {
                        this.clickCraftingRecipe(current, i == 0);
                    }
                }
            }
            if (this.trashCan != null && this.trashCan.containsPoint(x, y) && this.heldItem != null && this.heldItem.canBeTrashed())
            {
                if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((this.heldItem as StardewValley.Object).parentSheetIndex))
                {
                    Game1.player.specialItems.Remove((this.heldItem as StardewValley.Object).parentSheetIndex);
                }
                this.heldItem = null;
                Game1.playSound("trashcan");
                return;
            }
            if (this.heldItem != null && !this.isWithinBounds(x, y) && this.heldItem.canBeTrashed())
            {
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
                this.heldItem = null;
            }
        }

        public static uint itemsMade = 0;
        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            Item item = this.pagesOfCraftingRecipes[this.currentCraftingPage][c].createItem();
            bool consume = CookingSkillMod.onCook(this.pagesOfCraftingRecipes[this.currentCraftingPage][c], item);
            Object heldObj = this.heldItem as Object;
            Object itemObj = item as Object;
            bool didCraft = false;

            Game1.player.checkForQuestComplete(null, -1, -1, item, null, 2, -1);
            if (this.heldItem == null)
            {
                if ( consume )
                    this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients();
                this.heldItem = item;
                didCraft = true;
                if (playSound)
                {
                    Game1.playSound("coin");
                }
            }
            else if ((heldObj != null && itemObj != null && heldObj.quality == itemObj.quality ) && this.heldItem.Name.Equals(item.Name) && this.heldItem.Stack + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft - 1 < this.heldItem.maximumStackSize())
            {
                this.heldItem.Stack += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
                if ( consume )
                    this.pagesOfCraftingRecipes[this.currentCraftingPage][c].consumeIngredients();
                didCraft = true;
                if (playSound)
                {
                    Game1.playSound("coin");
                }
            }
            if (!this.cooking && Game1.player.craftingRecipes.ContainsKey(this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name))
            {
                SerializableDictionary<string, int> craftingRecipes = Game1.player.craftingRecipes;
                string name = this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name;
                craftingRecipes[name] += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
            }

            if (!didCraft)
                return;

            if (this.cooking)
            {
                Game1.player.cookedRecipe(this.heldItem.parentSheetIndex);
                CookingSkillMod.addCookingExp(itemObj.edibility);
            }
            if (!this.cooking)
            {
                Game1.stats.checkForCraftingAchievements();
            }
            else
            {
                Game1.stats.checkForCookingAchievements();
            }
            if (Game1.options.gamepadControls && this.heldItem != null && Game1.player.couldInventoryAcceptThisItem(this.heldItem))
            {
                Game1.player.addItemToInventoryBool(this.heldItem, false);
                this.heldItem = null;
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.heldItem = this.inventory.rightClick(x, y, this.heldItem, true);
            foreach (ClickableTextureComponent current in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (current.containsPoint(x, y) && !current.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][current].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : null))
                {
                    this.clickCraftingRecipe(current, true);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.hoverTitle = "";
            this.descriptionText = "";
            this.hoverText = "";
            this.hoverRecipe = null;
            this.hoverItem = this.inventory.hover(x, y, this.hoverItem);
            if (this.hoverItem != null)
            {
                this.hoverTitle = this.inventory.hoverTitle;
                this.hoverText = this.inventory.hoverText;
            }
            foreach (ClickableTextureComponent current in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (current.containsPoint(x, y))
                {
                    if (current.hoverText.Equals("ghosted"))
                    {
                        this.hoverText = "???";
                    }
                    else
                    {
                        this.hoverRecipe = this.pagesOfCraftingRecipes[this.currentCraftingPage][current];
                        if (this.lastCookingHover == null || !this.lastCookingHover.Name.Equals(this.hoverRecipe.name))
                        {
                            this.lastCookingHover = this.hoverRecipe.createItem();
                            CookingSkillMod.onCook(this.hoverRecipe, this.lastCookingHover);
                        }
                        current.scale = Math.Min(current.scale + 0.02f, current.baseScale + 0.1f);
                    }
                }
                else
                {
                    current.scale = Math.Max(current.scale - 0.02f, current.baseScale);
                }
            }
            if (this.upButton != null)
            {
                if (this.upButton.containsPoint(x, y))
                {
                    this.upButton.scale = Math.Min(this.upButton.scale + 0.02f, this.upButton.baseScale + 0.1f);
                }
                else
                {
                    this.upButton.scale = Math.Max(this.upButton.scale - 0.02f, this.upButton.baseScale);
                }
            }
            if (this.downButton != null)
            {
                if (this.downButton.containsPoint(x, y))
                {
                    this.downButton.scale = Math.Min(this.downButton.scale + 0.02f, this.downButton.baseScale + 0.1f);
                }
                else
                {
                    this.downButton.scale = Math.Max(this.downButton.scale - 0.02f, this.downButton.baseScale);
                }
            }
            if (this.trashCan != null)
            {
                if (this.trashCan.containsPoint(x, y))
                {
                    if (this.trashCanLidRotation <= 0f)
                    {
                        Game1.playSound("trashcanlid");
                    }
                    this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + 0.06544985f, 1.57079637f);
                    return;
                }
                this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - 0.06544985f, 0f);
            }
        }

        public override bool readyToClose()
        {
            return this.heldItem == null;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.cooking)
            {
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, null, false);
            }
            base.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize, false);
            this.inventory.draw(b);
            if (this.trashCan != null)
            {
                this.trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(686, 256, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), (float)Game1.pixelZoom, SpriteEffects.None, 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null);
            foreach (ClickableTextureComponent current in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys)
            {
                if (current.hoverText.Equals("ghosted"))
                {
                    current.draw(b, Color.Black * 0.35f, 0.89f);
                }
                else if (!this.pagesOfCraftingRecipes[this.currentCraftingPage][current].doesFarmerHaveIngredientsInInventory(this.cooking ? Utility.getHomeOfFarmer(Game1.player).fridge.items : null))
                {
                    current.draw(b, Color.LightGray * 0.4f, 0.89f);
                }
                else
                {
                    current.draw(b);
                    if (this.pagesOfCraftingRecipes[this.currentCraftingPage][current].numberProducedPerCraft > 1)
                    {
                        NumberSprite.draw(this.pagesOfCraftingRecipes[this.currentCraftingPage][current].numberProducedPerCraft, b, new Vector2((float)(current.bounds.X + Game1.tileSize - 2), (float)(current.bounds.Y + Game1.tileSize - 2)), Color.Red, 0.5f * (current.scale / (float)Game1.pixelZoom), 0.97f, 1f, 0, 0);
                    }
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            if (this.hoverItem != null)
            {
                IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoverItem, this.heldItem != null, -1, 0, -1, -1, null, -1);
            }
            else if (this.hoverText != null)
            {
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, (this.heldItem != null) ? Game1.tileSize : 0, (this.heldItem != null) ? Game1.tileSize : 0, -1, null, -1, null, null, 0, -1, -1, -1, -1, 1f, null);
            }
            if (this.heldItem != null)
            {
                this.heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + Game1.tileSize / 4), (float)(Game1.getOldMouseY() + Game1.tileSize / 4)), 1f);
            }
            base.draw(b);
            if (this.downButton != null && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                this.downButton.draw(b);
            }
            if (this.upButton != null && this.currentCraftingPage > 0)
            {
                this.upButton.draw(b);
            }
            if (this.cooking)
            {
                base.drawMouse(b);
            }
            if (this.hoverRecipe != null)
            {
                IClickableMenu.drawHoverText(b, " ", Game1.smallFont, (this.heldItem != null) ? (Game1.tileSize * 3 / 4) : 0, (this.heldItem != null) ? (Game1.tileSize * 3 / 4) : 0, -1, this.hoverRecipe.name, -1, (this.cooking && this.lastCookingHover != null && Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex].Split(new char[]
				{
					'/'
				}).Count<string>() >= 7) ? Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex].Split(new char[]
				{
					'/'
				})[6].Split(new char[]
				{
					' '
				}) : null, this.lastCookingHover, 0, -1, -1, -1, -1, 1f, this.hoverRecipe);
            }
        }
    }
}
