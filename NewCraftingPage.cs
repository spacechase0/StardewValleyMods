using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SObject = StardewValley.Object;
using SpaceCore;

namespace CookingSkill {
    public class NewCraftingPage : IClickableMenu {
        private string descriptionText = "";
        private string hoverText = "";
        public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();
        private string hoverTitle = "";
        public const int howManyRecipesFitOnPage = 40;
        public const int numInRow = 10;
        public const int numInCol = 4;
        public const int region_upArrow = 88;
        public const int region_downArrow = 89;
        public const int region_craftingSelectionArea = 8000;
        public const int region_craftingModifier = 200;
        private Item hoverItem;
        private Item lastCookingHover;
        public InventoryMenu inventory;
        private Item heldItem;
        private int currentCraftingPage;
        private CraftingRecipe hoverRecipe;
        public ClickableTextureComponent upButton;
        public ClickableTextureComponent downButton;
        private bool cooking;
        public ClickableTextureComponent trashCan;
        public float trashCanLidRotation;
        
        /// Use an variable function for the fridge items so that it can be changed through the API.
        public static Func<IList<Item>> fridge = () => Utility.getHomeOfFarmer(Game1.player)?.fridge.Value.items;

        /////
        /// Copied from CraftingPage. Changed denoted by /////
        /////
        public NewCraftingPage( int x, int y, int width, int height, bool cooking = false )
            : base(x, y, width, height, false) {
            this.cooking = cooking;
            this.inventory = new InventoryMenu(this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16, false, (IList<Item>)null, (InventoryMenu.highlightThisItem)null, -1, 3, 0, 0, true);
            this.inventory.showGrayedOutSlots = true;
            if (cooking)
                this.initializeUpperRightCloseButton();
            ClickableTextureComponent textureComponent1 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + width + 4, this.yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), 4f, false);
            textureComponent1.myID = 106;
            this.trashCan = textureComponent1;
            List<string> playerRecipes = new List<string>();
            if (!cooking) {
                foreach (string key in CraftingRecipe.craftingRecipes.Keys) {
                    if (Game1.player.craftingRecipes.ContainsKey(key))
                        playerRecipes.Add(key);
                }
            }
            else {
                Game1.playSound("bigSelect");
                foreach (string key in CraftingRecipe.cookingRecipes.Keys)
                    playerRecipes.Add(key);
            }
            this.layoutRecipes(playerRecipes);
            if (this.pagesOfCraftingRecipes.Count <= 1)
                return;
            ClickableTextureComponent textureComponent2 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 768 + 32, this.craftingPageY(), 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12, -1, -1), 0.8f, false);
            textureComponent2.myID = 88;
            textureComponent2.downNeighborID = 89;
            textureComponent2.rightNeighborID = 106;
            this.upButton = textureComponent2;
            ClickableTextureComponent textureComponent3 = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + 768 + 32, this.craftingPageY() + 192 + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11, -1, -1), 0.8f, false);
            textureComponent3.myID = 89;
            textureComponent3.upNeighborID = 88;
            textureComponent3.rightNeighborID = 106;
            this.downButton = textureComponent3;
        }

       private int craftingPageY() {
            return this.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
        }

        private ClickableTextureComponent[,] createNewPageLayout() {
            return new ClickableTextureComponent[10, 4];
        }

        private Dictionary<ClickableTextureComponent, CraftingRecipe> createNewPage() {
            Dictionary<ClickableTextureComponent, CraftingRecipe> dictionary = new Dictionary<ClickableTextureComponent, CraftingRecipe>();
            this.pagesOfCraftingRecipes.Add(dictionary);
            return dictionary;
        }

        private bool spaceOccupied( ClickableTextureComponent[,] pageLayout, int x, int y, CraftingRecipe recipe ) {
            if (pageLayout[x, y] != null)
                return true;
            if (!recipe.bigCraftable)
                return false;
            if (y + 1 < 4)
                return pageLayout[x, y + 1] != null;
            return true;
        }

        private int? getNeighbor( ClickableTextureComponent[,] pageLayout, int x, int y, int dx, int dy ) {
            if (x < 0 || y < 0 || (x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1)))
                return new int?();
            ClickableTextureComponent textureComponent1 = pageLayout[x, y];
            ClickableTextureComponent textureComponent2;
            for (textureComponent2 = textureComponent1; textureComponent2 == textureComponent1; textureComponent2 = pageLayout[x, y]) {
                x += dx;
                y += dy;
                if (x < 0 || y < 0 || (x >= pageLayout.GetLength(0) || y >= pageLayout.GetLength(1)))
                    return new int?();
            }
            if (textureComponent2 == null)
                return new int?();
            return new int?(textureComponent2.myID);
        }

        private void layoutRecipes( List<string> playerRecipes ) {
            int num1 = this.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
            int num2 = 8;
            Dictionary<ClickableTextureComponent, CraftingRecipe> newPage = this.createNewPage();
            int x1 = 0;
            int y1 = 0;
            int num3 = 0;
            ClickableTextureComponent[,] newPageLayout = this.createNewPageLayout();
            List<ClickableTextureComponent[,]> textureComponentArrayList = new List<ClickableTextureComponent[,]>();
            textureComponentArrayList.Add(newPageLayout);
            foreach (string playerRecipe in playerRecipes) {
                ++num3;
                int num4 = this.cooking ? 1 : 0;
                CraftingRecipe recipe = new CraftingRecipe(playerRecipe, num4 != 0);
                while (this.spaceOccupied(newPageLayout, x1, y1, recipe)) {
                    ++x1;
                    if (x1 >= 10) {
                        x1 = 0;
                        ++y1;
                        if (y1 >= 4) {
                            newPage = this.createNewPage();
                            newPageLayout = this.createNewPageLayout();
                            textureComponentArrayList.Add(newPageLayout);
                            x1 = 0;
                            y1 = 0;
                        }
                    }
                }
                int num5 = 200 + num3;
                ClickableTextureComponent textureComponent = new ClickableTextureComponent("", new Rectangle(num1 + x1 * (64 + num2), this.craftingPageY() + y1 * 72, 64, recipe.bigCraftable ? 128 : 64), (string)null, !this.cooking || Game1.player.cookingRecipes.ContainsKey(recipe.name) ? "" : "ghosted", recipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, recipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, recipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, recipe.getIndexOfMenuView(), 16, 16), 4f, false);
                textureComponent.myID = num5;
                textureComponent.rightNeighborID = y1 >= 2 || this.pagesOfCraftingRecipes.Count <= 0 ? 89 : 88;
                textureComponent.leftNeighborID = -1;
                textureComponent.upNeighborID = 12344;
                textureComponent.downNeighborID = x1;
                textureComponent.fullyImmutable = true;
                textureComponent.region = 8000;
                ClickableTextureComponent key = textureComponent;
                newPage.Add(key, recipe);
                newPageLayout[x1, y1] = key;
                if (recipe.bigCraftable)
                    newPageLayout[x1, y1 + 1] = key;
            }
            foreach (ClickableTextureComponent[,] pageLayout in textureComponentArrayList) {
                for (int x2 = 0; x2 < pageLayout.GetLength(0); ++x2) {
                    for (int y2 = 0; y2 < pageLayout.GetLength(1); ++y2) {
                        ClickableTextureComponent textureComponent = pageLayout[x2, y2];
                        if (textureComponent != null) {
                            int? neighbor = this.getNeighbor(pageLayout, x2, y2, 1, 0);
                            int num4 = neighbor ?? textureComponent.rightNeighborID;
                            neighbor = this.getNeighbor(pageLayout, x2, y2, -1, 0);
                            int num5 = neighbor ?? textureComponent.leftNeighborID;
                            neighbor = this.getNeighbor(pageLayout, x2, y2, 0, -1);
                            int num6 = neighbor ?? textureComponent.upNeighborID;
                            neighbor = this.getNeighbor(pageLayout, x2, y2, 0, 1);
                            int num7 = neighbor ?? textureComponent.downNeighborID;
                            textureComponent.rightNeighborID = num4;
                            textureComponent.leftNeighborID = num5;
                            textureComponent.upNeighborID = num6;
                            textureComponent.downNeighborID = num7;
                        }
                    }
                }
            }
        }

        protected override void noSnappedComponentFound( int direction, int oldRegion, int oldID ) {
            base.noSnappedComponentFound(direction, oldRegion, oldID);
            if (oldRegion != 8000 || direction != 2)
                return;
            this.currentlySnappedComponent = this.getComponentWithID(oldID % 10);
            this.currentlySnappedComponent.upNeighborID = oldID;
        }

        public override void snapToDefaultClickableComponent() {
            this.currentlySnappedComponent = this.currentCraftingPage < this.pagesOfCraftingRecipes.Count ? (ClickableComponent)this.pagesOfCraftingRecipes[this.currentCraftingPage].First<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key : (ClickableComponent)null;
            this.snapCursorToCurrentSnappedComponent();
        }

        protected override void actionOnRegionChange( int oldRegion, int newRegion ) {
            base.actionOnRegionChange(oldRegion, newRegion);
            if (newRegion != 9000 || oldRegion == 0)
                return;
            for (int index = 0; index < 10; ++index) {
                if (this.inventory.inventory.Count > index)
                    this.inventory.inventory[index].upNeighborID = this.currentlySnappedComponent.upNeighborID;
            }
        }

        public override void receiveKeyPress( Keys key ) {
            base.receiveKeyPress(key);
            if (!key.Equals((object)Keys.Delete) || this.heldItem == null || !this.heldItem.canBeTrashed())
                return;
            if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((int)((NetFieldBase<int, NetInt>)(this.heldItem as StardewValley.Object).parentSheetIndex)))
                Game1.player.specialItems.Remove((int)((NetFieldBase<int, NetInt>)(this.heldItem as StardewValley.Object).parentSheetIndex));
            this.heldItem = (Item)null;
            Game1.playSound("trashcan");
        }

        public override void receiveScrollWheelAction( int direction ) {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentCraftingPage > 0) {
                --this.currentCraftingPage;
                Game1.playSound("shwip");
                if (!Game1.options.SnappyMenus)
                    return;
                ClickableTextureComponent upButton = this.upButton;
                KeyValuePair<ClickableTextureComponent, CraftingRecipe> keyValuePair = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>();
                int id1 = keyValuePair.Key.myID;
                upButton.leftNeighborID = id1;
                this.setCurrentlySnappedComponentTo(88);
                this.snapCursorToCurrentSnappedComponent();
                ClickableTextureComponent downButton = this.downButton;
                keyValuePair = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>();
                int id2 = keyValuePair.Key.myID;
                downButton.leftNeighborID = id2;
            }
            else {
                if (direction >= 0 || this.currentCraftingPage >= this.pagesOfCraftingRecipes.Count - 1)
                    return;
                ++this.currentCraftingPage;
                Game1.playSound("shwip");
                if (!Game1.options.SnappyMenus)
                    return;
                ClickableTextureComponent downButton = this.downButton;
                KeyValuePair<ClickableTextureComponent, CraftingRecipe> keyValuePair = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>();
                int id1 = keyValuePair.Key.myID;
                downButton.leftNeighborID = id1;
                this.setCurrentlySnappedComponentTo(89);
                this.snapCursorToCurrentSnappedComponent();
                ClickableTextureComponent upButton = this.upButton;
                keyValuePair = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>();
                int id2 = keyValuePair.Key.myID;
                upButton.leftNeighborID = id2;
            }
        }

        public override void receiveLeftClick( int x, int y, bool playSound = true ) {
            base.receiveLeftClick(x, y, true);
            this.heldItem = this.inventory.leftClick(x, y, this.heldItem, true);
            if (this.upButton != null && this.upButton.containsPoint(x, y) && this.currentCraftingPage > 0) {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Max(0, this.currentCraftingPage - 1);
                this.upButton.scale = this.upButton.baseScale;
                this.upButton.leftNeighborID = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
            }
            if (this.downButton != null && this.downButton.containsPoint(x, y) && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1) {
                Game1.playSound("coin");
                this.currentCraftingPage = Math.Min(this.pagesOfCraftingRecipes.Count - 1, this.currentCraftingPage + 1);
                this.downButton.scale = this.downButton.baseScale;
                this.downButton.leftNeighborID = this.pagesOfCraftingRecipes[this.currentCraftingPage].Last<KeyValuePair<ClickableTextureComponent, CraftingRecipe>>().Key.myID;
            }
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys) {
                int num = Game1.oldKBState.IsKeyDown(Keys.LeftShift) ? 5 : 1;
                for (int index = 0; index < num; ++index) {
                    if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? fridge() : (IList<Item>)null))
                        this.clickCraftingRecipe(key, index == 0);
                }
            }
            if (this.trashCan != null && this.trashCan.containsPoint(x, y) && (this.heldItem != null && this.heldItem.canBeTrashed())) {
                if (this.heldItem is StardewValley.Object && Game1.player.specialItems.Contains((int)((NetFieldBase<int, NetInt>)(this.heldItem as StardewValley.Object).parentSheetIndex)))
                    Game1.player.specialItems.Remove((int)((NetFieldBase<int, NetInt>)(this.heldItem as StardewValley.Object).parentSheetIndex));
                this.heldItem = (Item)null;
                Game1.playSound("trashcan");
            }
            else {
                if (this.heldItem == null || this.isWithinBounds(x, y) || !this.heldItem.canBeTrashed())
                    return;
                Game1.playSound("throwDownITem");
                Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection, (GameLocation)null, -1);
                this.heldItem = (Item)null;
            }
        }

        private void clickCraftingRecipe( ClickableTextureComponent c, bool playSound = true ) {
            Item obj = this.pagesOfCraftingRecipes[this.currentCraftingPage][c].createItem();
            /////
            bool consume = Mod.onCook(this.pagesOfCraftingRecipes[this.currentCraftingPage][c], obj);
            SObject heldObj = this.heldItem as SObject;
            SObject itemObj = obj as SObject;
            bool didCraft = false;
            /////
            Game1.player.checkForQuestComplete((NPC)null, -1, -1, obj, (string)null, 2, -1);
            if (this.heldItem == null) {
                /////
                if (consume)
                    NewCraftingPage.myConsumeIngredients(this.pagesOfCraftingRecipes[this.currentCraftingPage][c]);
                didCraft = true;
                /////
                this.heldItem = obj;
                if (playSound)
                    Game1.playSound("coin");
            }
            else if (this.heldItem.Name.Equals(obj.Name) && this.heldItem.Stack + this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft - 1 < this.heldItem.maximumStackSize()) {
                this.heldItem.Stack += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;
                /////
                if (consume)
                    NewCraftingPage.myConsumeIngredients(this.pagesOfCraftingRecipes[this.currentCraftingPage][c]);
                didCraft = true;
                /////
                if (playSound)
                    Game1.playSound("coin");
            }
            if (!this.cooking && Game1.player.craftingRecipes.ContainsKey(this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name))
                Game1.player.craftingRecipes[this.pagesOfCraftingRecipes[this.currentCraftingPage][c].name] += this.pagesOfCraftingRecipes[this.currentCraftingPage][c].numberProducedPerCraft;

            /////
            if (!didCraft)
                return;
            /////

            if (this.cooking) {
                Game1.player.cookedRecipe((int)((NetFieldBase<int, NetInt>)this.heldItem.parentSheetIndex));
                /////
                Game1.player.AddCustomSkillExperience(Mod.skill, itemObj.Edibility);
                /////
            }
            if (!this.cooking)
                Game1.stats.checkForCraftingAchievements();
            else
                Game1.stats.checkForCookingAchievements();
            if (!Game1.options.gamepadControls || this.heldItem == null || !Game1.player.couldInventoryAcceptThisItem(this.heldItem))
                return;
            Game1.player.addItemToInventoryBool(this.heldItem, false);
            this.heldItem = (Item)null;
        }

        public override void receiveRightClick( int x, int y, bool playSound = true ) {
            this.heldItem = this.inventory.rightClick(x, y, this.heldItem, true);
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys) {
                if (key.containsPoint(x, y) && !key.hoverText.Equals("ghosted") && this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? (IList<Item>)fridge() : (IList<Item>)null))
                    this.clickCraftingRecipe(key, true);
            }
        }

        public override void performHoverAction( int x, int y ) {
            base.performHoverAction(x, y);
            this.hoverTitle = "";
            this.descriptionText = "";
            this.hoverText = "";
            this.hoverRecipe = (CraftingRecipe)null;
            this.hoverItem = this.inventory.hover(x, y, this.hoverItem);
            if (this.hoverItem != null) {
                this.hoverTitle = this.inventory.hoverTitle;
                this.hoverText = this.inventory.hoverText;
            }
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys) {
                if (key.containsPoint(x, y)) {
                    if (key.hoverText.Equals("ghosted")) {
                        this.hoverText = "???";
                    }
                    else {
                        this.hoverRecipe = this.pagesOfCraftingRecipes[this.currentCraftingPage][key];
                        if (this.lastCookingHover == null || !this.lastCookingHover.Name.Equals(this.hoverRecipe.name)) {
                            this.lastCookingHover = this.hoverRecipe.createItem();
                            /////
                            Mod.onCook(this.hoverRecipe, this.lastCookingHover);
                            /////
                        }
                        key.scale = Math.Min(key.scale + 0.02f, key.baseScale + 0.1f);
                    }
                }
                else
                    key.scale = Math.Max(key.scale - 0.02f, key.baseScale);
            }
            if (this.upButton != null) {
                if (this.upButton.containsPoint(x, y))
                    this.upButton.scale = Math.Min(this.upButton.scale + 0.02f, this.upButton.baseScale + 0.1f);
                else
                    this.upButton.scale = Math.Max(this.upButton.scale - 0.02f, this.upButton.baseScale);
            }
            if (this.downButton != null) {
                if (this.downButton.containsPoint(x, y))
                    this.downButton.scale = Math.Min(this.downButton.scale + 0.02f, this.downButton.baseScale + 0.1f);
                else
                    this.downButton.scale = Math.Max(this.downButton.scale - 0.02f, this.downButton.baseScale);
            }
            if (this.trashCan == null)
                return;
            if (this.trashCan.containsPoint(x, y)) {
                if ((double)this.trashCanLidRotation <= 0.0)
                    Game1.playSound("trashcanlid");
                this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, 1.570796f);
            }
            else
                this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - (float)Math.PI / 48f, 0.0f);
        }

        public override bool readyToClose() {
            return this.heldItem == null;
        }

        public override void draw( SpriteBatch b ) {
            if (this.cooking)
                Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true, (string)null, false);
            this.drawHorizontalPartition(b, this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256, false);
            this.inventory.draw(b);
            if (this.trashCan != null) {
                this.trashCan.draw(b);
                b.Draw(Game1.mouseCursors, new Vector2((float)(this.trashCan.bounds.X + 60), (float)(this.trashCan.bounds.Y + 40)), new Rectangle?(new Rectangle(686, 256, 18, 10)), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
            }
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            foreach (ClickableTextureComponent key in this.pagesOfCraftingRecipes[this.currentCraftingPage].Keys) {
                if (key.hoverText.Equals("ghosted"))
                    key.draw(b, Color.Black * 0.35f, 0.89f);
                else if (!this.pagesOfCraftingRecipes[this.currentCraftingPage][key].doesFarmerHaveIngredientsInInventory(this.cooking ? (IList<Item>)fridge() : (IList<Item>)null)) {
                    key.draw(b, Color.LightGray * 0.4f, 0.89f);
                }
                else {
                    key.draw(b);
                    if (this.pagesOfCraftingRecipes[this.currentCraftingPage][key].numberProducedPerCraft > 1)
                        NumberSprite.draw(this.pagesOfCraftingRecipes[this.currentCraftingPage][key].numberProducedPerCraft, b, new Vector2((float)(key.bounds.X + 64 - 2), (float)(key.bounds.Y + 64 - 2)), Color.Red, (float)(0.5 * ((double)key.scale / 4.0)), 0.97f, 1f, 0, 0);
                }
            }
            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
            if (this.hoverItem != null)
                IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoverItem, this.heldItem != null, -1, 0, -1, -1, (CraftingRecipe)null, -1);
            else if (!string.IsNullOrEmpty(this.hoverText))
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont, this.heldItem != null ? 64 : 0, this.heldItem != null ? 64 : 0, -1, (string)null, -1, (string[])null, (Item)null, 0, -1, -1, -1, -1, 1f, (CraftingRecipe)null);
            if (this.heldItem != null)
                this.heldItem.drawInMenu(b, new Vector2((float)(Game1.getOldMouseX() + 16), (float)(Game1.getOldMouseY() + 16)), 1f);
            base.draw(b);
            if (this.downButton != null && this.currentCraftingPage < this.pagesOfCraftingRecipes.Count - 1)
                this.downButton.draw(b);
            if (this.upButton != null && this.currentCraftingPage > 0)
                this.upButton.draw(b);
            if (this.cooking)
                this.drawMouse(b);
            if (this.hoverRecipe == null)
                return;
            SpriteBatch b1 = b;
            string text = " ";
            SpriteFont smallFont = Game1.smallFont;
            int xOffset = this.heldItem != null ? 48 : 0;
            int yOffset = this.heldItem != null ? 48 : 0;
            int moneyAmountToDisplayAtBottom = -1;
            string displayName = this.hoverRecipe.DisplayName;
            int healAmountToDisplay = -1;
            string[] buffIconsToDisplay;
            if (this.cooking && this.lastCookingHover != null) {
                if (Game1.objectInformation[(int)((NetFieldBase<int, NetInt>)(this.lastCookingHover as StardewValley.Object).parentSheetIndex)].Split('/').Length > 7) {
                    buffIconsToDisplay = Game1.objectInformation[(int)((NetFieldBase<int, NetInt>)(this.lastCookingHover as StardewValley.Object).parentSheetIndex)].Split('/')[7].Split(' ');
                    goto label_32;
                }
            }
            buffIconsToDisplay = (string[])null;
        label_32:
            Item lastCookingHover = this.lastCookingHover;
            int currencySymbol = 0;
            int extraItemToShowIndex = -1;
            int extraItemToShowAmount = -1;
            int overrideX = -1;
            int overrideY = -1;
            double num = 1.0;
            CraftingRecipe hoverRecipe = this.hoverRecipe;
            IClickableMenu.drawHoverText(b1, text, smallFont, xOffset, yOffset, moneyAmountToDisplayAtBottom, displayName, healAmountToDisplay, buffIconsToDisplay, lastCookingHover, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, overrideX, overrideY, (float)num, hoverRecipe);
        }

        /////
        public class ConsumedItem 
        {
            public StardewValley.Object item;
            public int amt;

            public ConsumedItem( StardewValley.Object theItem ) 
            {
                item = theItem;
                amt = item.Stack;
            }
        }
    

        public static void myConsumeIngredients( CraftingRecipe recipe, bool actuallyConsume = true, List<ConsumedItem> used = null ) 
        { 
            ///// (removed this)
            Dictionary<int, int> recipeList = (Dictionary<int, int>)Util.GetInstanceField(typeof(CraftingRecipe), recipe, "recipeList");
            /////
            for (int index1 = recipeList.Count - 1; index1 >= 0; --index1) {
                int recipe1 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                bool flag = false;
                for (int index2 = Game1.player.items.Count - 1; index2 >= 0; --index2) {
                    if (Game1.player.items[index2] != null && Game1.player.items[index2] is SObject && !(bool)((NetFieldBase<bool, NetBool>)(Game1.player.items[index2] as SObject).bigCraftable) && ((int)((NetFieldBase<int, NetInt>)Game1.player.items[index2].parentSheetIndex) == recipeList.Keys.ElementAt<int>(index1) || Game1.player.items[index2].Category == recipeList.Keys.ElementAt<int>(index1))) {
                        int recipe2 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                        recipeList[recipeList.Keys.ElementAt<int>(index1)] -= Game1.player.items[index2].Stack;
                        /////
                        if (used != null)
                            used.Add(new ConsumedItem(Game1.player.items[index2] as SObject));
                        if (actuallyConsume)
                        /////

                        Game1.player.items[index2].Stack -= recipe2;
                        if (Game1.player.items[index2].Stack <= 0)
                            Game1.player.items[index2] = (Item)null;
                        if (recipeList[recipeList.Keys.ElementAt<int>(index1)] <= 0) {
                            recipeList[recipeList.Keys.ElementAt<int>(index1)] = recipe1;
                            flag = true;
                            break;
                        }
                    }
                }
                ///// Use of recipe, StardewValley.Locations
                if (recipe.isCookingRecipe && !flag) {
                    StardewValley.Locations.FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
                    if (homeOfFarmer != null) {
                        for (int index2 = fridge().Count - 1; index2 >= 0; --index2) {
                            if (fridge()[index2] != null && fridge()[index2] is SObject && ((int)((NetFieldBase<int, NetInt>)fridge()[index2].parentSheetIndex) == recipeList.Keys.ElementAt<int>(index1) || fridge()[index2].Category == recipeList.Keys.ElementAt<int>(index1))) {
                                int recipe2 = recipeList[recipeList.Keys.ElementAt<int>(index1)];
                                recipeList[recipeList.Keys.ElementAt<int>(index1)] -= fridge()[index2].Stack;
                                /////
                                if (used != null)
                                    used.Add(new ConsumedItem(fridge()[index2] as SObject));
                                if (actuallyConsume)
                                /////
                                    fridge()[index2].Stack -= recipe2;
                                if (fridge()[index2].Stack <= 0)
                                    fridge()[index2] = (Item)null;
                                if (recipeList[recipeList.Keys.ElementAt<int>(index1)] <= 0) {
                                    recipeList[recipeList.Keys.ElementAt<int>(index1)] = recipe1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        /////
    }
}
