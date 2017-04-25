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

        public const int region_upArrow = 88;

        public const int region_downArrow = 89;

        public const int region_craftingSelectionArea = 8000;

        public const int region_craftingModifier = 200;

        private string descriptionText = "";

        private string hoverText = "";

        private Item hoverItem;

        private Item lastCookingHover;

        public InventoryMenu inventory;

        private Item heldItem;

        public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

        private int currentCraftingPage;

        private CraftingRecipe hoverRecipe;

        public ClickableTextureComponent upButton;

        public ClickableTextureComponent downButton;

        private bool cooking;

        public ClickableTextureComponent trashCan;

        public float trashCanLidRotation;

        private string hoverTitle = "";

        public NewCraftingPage(int x, int y, int width, int height, bool cooking = false) : base(x, y, width, height, false)
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
            this.trashCan = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 4, this.yPositionOnScreen + height - Game1.tileSize * 3 - Game1.tileSize / 2 - IClickableMenu.borderWidth - 104, Game1.tileSize, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), (float)Game1.pixelZoom, false)
            {
                myID = 106
            };
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
                    goto IL_26A;
                }
            }
            Game1.playSound("bigSelect");
            foreach (string current3 in CraftingRecipe.cookingRecipes.Keys)
            {
                list.Add(new string(current3.ToCharArray()));
            }
        IL_26A:
            int arg_271_0 = list.Count;
            int num6 = 0;
            while (list.Count > 0)
            {
                CraftingRecipe craftingRecipe;
                int num9;
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
                    int num8 = list.Count;
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
                    num9 = num5 / 40;
                    clickableTextureComponent = new ClickableTextureComponent("", new Rectangle(num + num5 % num4 * (Game1.tileSize + num3), num2 + num7 * (Game1.tileSize + 8), Game1.tileSize, craftingRecipe.bigCraftable ? (Game1.tileSize * 2) : Game1.tileSize), null, (cooking && !Game1.player.cookingRecipes.ContainsKey(craftingRecipe.name)) ? "ghosted" : "", craftingRecipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, craftingRecipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, craftingRecipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, craftingRecipe.getIndexOfMenuView(), 16, 16), (float)Game1.pixelZoom, false)
                    {
                        myID = 200 + num5,
                        myAlternateID = (craftingRecipe.bigCraftable ? (200 + num5 + num4) : -500),
                        rightNeighborID = ((num5 % num4 < num4 - 1) ? (200 + num5 + 1) : ((num7 < 2 && num9 > 0) ? 88 : 89)),
                        leftNeighborID = ((num5 % num4 > 0) ? (200 + num5 - 1) : -1),
                        upNeighborID = ((num7 == 0) ? 12344 : (200 + num5 - num4)),
                        downNeighborID = ((num7 == 40 / num4 - 1 || (num7 == 40 / num4 - 2 && craftingRecipe.bigCraftable) || list.Count <= 10) ? (num5 % num4) : (200 + num5 + (craftingRecipe.bigCraftable ? (num4 * 2) : num4))),
                        fullyImmutable = true,
                        region = 8000
                    };
                    flag = false;
                    using (Dictionary<ClickableTextureComponent, CraftingRecipe>.KeyCollection.Enumerator enumerator3 = this.pagesOfCraftingRecipes[num9].Keys.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            if (enumerator3.Current.bounds.Intersects(clickableTextureComponent.bounds))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
                while (flag);
                this.pagesOfCraftingRecipes[num9].Add(clickableTextureComponent, craftingRecipe);
                list.RemoveAt(num6);
                num6 = 0;
            }
            if (this.pagesOfCraftingRecipes.Count > 1)
            {
                this.upButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, num2, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false)
                {
                    myID = 88,
                    downNeighborID = 89,
                    rightNeighborID = 106
                };
                this.downButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize * 12 + Game1.tileSize / 2, num2 + Game1.tileSize * 3 + Game1.tileSize / 2, Game1.tileSize, Game1.tileSize), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false)
                {
                    myID = 89,
                    upNeighborID = 88,
                    rightNeighborID = 106
                };
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            this.currentlySnappedComponent = ((this.currentCraftingPage < this.pagesOfCraftingRecipes.Count) ? this.pagesOfCraftingRecipes[this.currentCraftingPage].First<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key : null);
            this.snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange(int oldRegion, int newRegion)
        {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion == 9000 && oldRegion != 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (this.inventory.inventory.Count > i)
                    {
                        this.inventory.inventory[i].upNeighborID = this.currentlySnappedComponent.upNeighborID;
                    }
                }
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
            if (this.upButton != null && this.upButton.containsPoint(x, y) && this.currentCraftingPage > 0)
            {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Max(0, this.currentCraftingPage - 1);
                this.upButton.scale = this.upButton.baseScale;
                this.upButton.leftNeighborID = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
            }
            if (this.downButton != null && this.downButton.containsPoint(x, y) && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
            {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this.currentCraftingPage + 1);
                this.downButton.scale = this.downButton.baseScale;
                this.downButton.leftNeighborID = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
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
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, null);
                this.heldItem = null;
            }
        }

        private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
        {
            Item item = this.pagesOfCraftingRecipes[this.currentCraftingPage][c].createItem();
            bool consume = Mod.onCook(this.pagesOfCraftingRecipes[this.currentCraftingPage][c], item);
            Object heldObj = this.heldItem as Object;
            Object itemObj = item as Object;
            bool didCraft = false;
            
            Game1.player.checkForQuestComplete(null, -1, -1, item, null, 2, -1);
            if (this.heldItem == null)
            {
                if (consume)
                    NewCraftingPage.myConsumeIngredients(this.pagesOfCraftingRecipes[this.currentCraftingPage][c]);
                didCraft = true;
                this.heldItem = item;
                if (playSound)
                {
                    Game1.playSound("coin");
                }
            }
            else if (this.heldItem.Name.Equals(item.Name) && heldObj.quality == itemObj.quality &&this.heldItem.Stack + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft - 1 < this.heldItem.maximumStackSize())
            {
                this.heldItem.Stack += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
                if (consume)
                    NewCraftingPage.myConsumeIngredients(this.pagesOfCraftingRecipes[this.currentCraftingPage][c]);
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
                Mod.addCookingExp(itemObj.edibility);
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
                            Mod.onCook(this.hoverRecipe, this.lastCookingHover);
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
            else if (!string.IsNullOrEmpty(this.hoverText))
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
                IClickableMenu.drawHoverText(b, " ", Game1.smallFont, (this.heldItem != null) ? (Game1.tileSize * 3 / 4) : 0, (this.heldItem != null) ? (Game1.tileSize * 3 / 4) : 0, -1, this.hoverRecipe.DisplayName, -1, (this.cooking && this.lastCookingHover != null && Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex].Split(new char[]
                {
                    '/'
                }).Length > 7) ? Game1.objectInformation[(this.lastCookingHover as StardewValley.Object).parentSheetIndex].Split(new char[]
                {
                    '/'
                })[7].Split(new char[]
                {
                    ' '
                }) : null, this.lastCookingHover, 0, -1, -1, -1, -1, 1f, this.hoverRecipe);
            }
        }

        public class ConsumedItem
        {
            public StardewValley.Object item;
            public int amt;

            public ConsumedItem(StardewValley.Object theItem)
            {
                item = theItem;
                amt = item.Stack;
            }
        }
        public static void myConsumeIngredients(CraftingRecipe recipe, bool actuallyConsume = true, List<ConsumedItem> used = null)
        {
            Dictionary<int, int> recipeList = (Dictionary<int, int>)Util.GetInstanceField(typeof(CraftingRecipe), recipe, "recipeList");
            for (int i = recipeList.Count - 1; i >= 0; i--)
            {
                int value = recipeList[recipeList.Keys.ElementAt(i)];
                bool flag = false;
                for (int j = Game1.player.items.Count - 1; j >= 0; j--)
                {
                    if (Game1.player.items[j] != null && Game1.player.items[j] is StardewValley.Object && !(Game1.player.items[j] as Object).bigCraftable && (((Object)Game1.player.items[j]).parentSheetIndex == recipeList.Keys.ElementAt(i) || ((StardewValley.Object)Game1.player.items[j]).category == recipeList.Keys.ElementAt(i)))
                    {
                        int num = recipeList[recipeList.Keys.ElementAt(i)];
                        Dictionary<int, int> dictionary = recipeList;
                        int key = recipeList.Keys.ElementAt(i);
                        dictionary[key] -= Game1.player.items[j].Stack;
                        ///////////////////////////////////////////////////////
                        if (used != null)
                            used.Add(new ConsumedItem(Game1.player.items[j] as StardewValley.Object));
                        if (actuallyConsume)
                        ///////////////////////////////////////////////////////
                        Game1.player.items[j].Stack -= num;
                        if (Game1.player.items[j].Stack <= 0)
                        {
                            Game1.player.items[j] = null;
                        }
                        if (recipeList[recipeList.Keys.ElementAt(i)] <= 0)
                        {
                            recipeList[recipeList.Keys.ElementAt(i)] = value;
                            flag = true;
                            break;
                        }
                    }
                }
                if (recipe.isCookingRecipe && !flag)
                {
                    StardewValley.Locations.FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
                    if (homeOfFarmer != null)
                    {
                        for (int k = homeOfFarmer.fridge.items.Count - 1; k >= 0; k--)
                        {
                            if (homeOfFarmer.fridge.items[k] != null && homeOfFarmer.fridge.items[k] is StardewValley.Object && (((StardewValley.Object)homeOfFarmer.fridge.items[k]).parentSheetIndex == recipeList.Keys.ElementAt(i) || ((Object)homeOfFarmer.fridge.items[k]).category == recipeList.Keys.ElementAt(i)))
                            {
                                int num2 = recipeList[recipeList.Keys.ElementAt(i)];
                                Dictionary<int, int> dictionary = recipeList;
                                int key = recipeList.Keys.ElementAt(i);
                                dictionary[key] -= homeOfFarmer.fridge.items[k].Stack;
                                ///////////////////////////////////////////////////////
                                if (used != null)
                                    used.Add(new ConsumedItem(homeOfFarmer.fridge.items[k] as StardewValley.Object));
                                if (actuallyConsume)
                                ///////////////////////////////////////////////////////
                                homeOfFarmer.fridge.items[k].Stack -= num2;
                                if (homeOfFarmer.fridge.items[k].Stack <= 0)
                                {
                                    homeOfFarmer.fridge.items[k] = null;
                                }
                                if (recipeList[recipeList.Keys.ElementAt(i)] <= 0)
                                {
                                    recipeList[recipeList.Keys.ElementAt(i)] = value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
