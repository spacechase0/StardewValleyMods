using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.UI;
using StardewValley;
using StardewValley.Menus;

namespace MageDelve.Alchemy
{
    public class AlchemyRecipesMenu : IClickableMenu
    {
        private RootElement ui;
        private List<ItemWithBorder> recipes = new();

        public AlchemyRecipesMenu()
        : base(Game1.viewport.Width / 2 - 320, Game1.viewport.Height / 2 - 240, 640, 480, true)
        {
            ui = new();
            ui.LocalPosition = new(xPositionOnScreen, yPositionOnScreen);

            Table table = new()
            {
                RowHeight = 110,
                Size = new(640, 480),
            };

            List<Element> currRow = new();
            var recipes = AlchemyRecipes.Get();
            foreach (var recipe in recipes)
            {
                CraftingRecipe fake = new("");
                fake.recipeList.Clear();
                fake.itemToProduce.Clear();
                int qtySpot = recipe.Key.IndexOf('/');
                fake.itemToProduce.Add(qtySpot == -1 ? recipe.Key.Substring(3) : recipe.Key.Substring(3, qtySpot - 3));
                fake.numberProducedPerCraft = qtySpot == -1 ? 1 : int.Parse(recipe.Key.Substring(qtySpot + 1));
                var tmp = ItemRegistry.Create(fake.itemToProduce[0], fake.numberProducedPerCraft);
                fake.DisplayName = tmp.DisplayName;
                fake.description = tmp.getDescription();

                foreach (string item in recipe.Value)
                {
                    string itemId = item[0] == '-' ? item : item.Substring(3);
                    if (fake.recipeList.ContainsKey(itemId))
                        fake.recipeList[itemId] = fake.recipeList[itemId] + 1;
                    else
                        fake.recipeList.Add(itemId, 1);
                }

                ItemWithBorder recipe_ = new()
                {
                    ItemDisplay = tmp,
                    LocalPosition = new Vector2(currRow.Count * 110, 0),
                    UserData = fake,
                    Callback = (e) =>
                    {
                        var parent = GetParentMenu() as FancyAlchemyMenu;
                        if (parent == null) return;

                        if (parent.ingreds.Any(slot => slot.Item != null))
                            return;
                        if (!fake.doesFarmerHaveIngredientsInInventory())
                            return;

                        for (int i = 0; i < recipe.Value.Length; ++i)
                        {
                            string item = recipe.Value[i];
                            int? cat = null;
                            if (int.TryParse(item, out int cati))
                                cat = cati;

                            for (int j = 0; j < Game1.player.Items.Count; ++j)
                            {
                                var invItem = Game1.player.Items[j];
                                if (invItem == null) continue;

                                if (invItem.QualifiedItemId == item || cat.HasValue && invItem.Category == cat.Value)
                                {
                                    parent.ingreds[i].Item = invItem.getOne();
                                    invItem.Stack--;
                                    if (invItem.Stack <= 0)
                                        Game1.player.Items[j] = null;
                                    break;
                                }
                            }
                        }

                        parent.CheckRecipe();
                        exitThisMenu();
                    },
                };
                if (!fake.doesFarmerHaveIngredientsInInventory())
                    recipe_.TransparentItemDisplay = true;
                currRow.Add(recipe_);
                this.recipes.Add(recipe_);
                if (currRow.Count == 6)
                {
                    table.AddRow(currRow.ToArray());
                    currRow.Clear();
                }
            }
            if (currRow.Count != 0)
                table.AddRow(currRow.ToArray());
            ui.AddChild(table);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            var menu = Game1.activeClickableMenu;
            bool check = menu == GetParentMenu();
            bool active = IsActive();

            ui.Draw(b);

            drawMouse(b);

            if (ItemWithBorder.HoveredElement != null)
            {
                var fake = ItemWithBorder.HoveredElement.UserData as CraftingRecipe;
                drawHoverText(b, " ", Game1.smallFont, boldTitleText: fake.DisplayName, craftingIngredients: fake);
            }
        }
    }
}
