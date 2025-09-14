using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SpaceShared.UI;
using StardewValley;
using StardewValley.Menus;

namespace MageDelve.Alchemy
{
    internal class FancyAlchemyMenu : IClickableMenu
    {
        private RootElement ui;
        internal ItemSlot[] ingreds;
        private ItemSlot output;

        private InventoryMenu inventory;

        private class Pixel
        {
            public float x;
            public float y;
            public Color color;
            public float scale;
            public Vector2 velocity;
        }
        private List<Pixel> pixels = new();

        private Item held;
        private float? animStart;
        private bool playedSynthesizeSound = true;

        public FancyAlchemyMenu()
        : base((Game1.viewport.Width - 64 * 12 - 32) / 2, (Game1.viewport.Height - 480 - 250) / 2, 64 * 12 + 32, 480 + 250)
        {
            ui = new RootElement();
            ui.LocalPosition = new Vector2(xPositionOnScreen, yPositionOnScreen);

            Vector2 basePoint = new(width / 2, (height - 200) / 2);

            output = new ItemSlot()
            {
                LocalPosition = basePoint,
                TransparentItemDisplay = true,
                Callback = (e) => DoCraftingIfPossible(),
            };
            output.LocalPosition -= new Vector2(output.Width / 2, output.Height / 2);
            ui.AddChild(output);

            ingreds = new ItemSlot[6];
            for (int i = 0; i < 6; ++i)
            {
                ingreds[i] = new ItemSlot()
                {
                    LocalPosition = basePoint +
                                    new Vector2(MathF.Cos(3.14f * 2 / 6 * i) * 200,
                                                 MathF.Sin(3.14f * 2 / 6 * i) * 200) +
                                    -new Vector2(output.Width / 2, output.Height / 2),
                    Callback = (e) => CheckRecipe(),
                };
                ui.AddChild(ingreds[i]);
            }

            var recipesButton = new Image()
            {
                Texture = Game1.objectSpriteSheet,
                TexturePixelArea = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 102, 16, 16),
                Scale = 4,
                Callback = (e) => SetChildMenu(new AlchemyRecipesMenu()),
                LocalPosition = new(32, 32),
            };
            ui.AddChild(recipesButton);

            inventory = new InventoryMenu(xPositionOnScreen + 16, yPositionOnScreen + height - 64 * 3 - 16, true, Game1.player.Items);
        }

        private void Pixelize(ItemSlot slot)
        {
            var obj = slot.Item as StardewValley.Object;
            if (obj == null)
                return;

            var tex = ItemRegistry.GetData(slot.Item.QualifiedItemId).GetTexture();
            var rect = ItemRegistry.GetData(slot.Item.QualifiedItemId).GetSourceRect();

            var cols = new Color[16 * 16];
            tex.GetData(0, rect, cols, 0, cols.Length);

            for (int i = 0; i < cols.Length; ++i)
            {
                int ix = i % 16;
                int iy = i / 16;

                float velDir = (float)Game1.random.NextDouble() * 3.14f * 2;
                Vector2 vel = new Vector2(MathF.Cos(velDir), MathF.Sin(velDir)) * (60 + Game1.random.Next(70));

                pixels.Add(new Pixel()
                {
                    x = slot.Bounds.Location.X + 16 + ix * Game1.pixelZoom,
                    y = slot.Bounds.Location.Y + 16 + iy * Game1.pixelZoom,
                    color = cols[i],
                    scale = 3 + (float)Game1.random.NextDouble() * 3,
                    velocity = vel,
                });
            }
        }

        private void DoCraftingIfPossible()
        {
            if (output.Item == null && output.ItemDisplay != null && pixels.Count == 0)
            {
                foreach (var ingred in ingreds)
                {
                    Pixelize(ingred);
                }
                Game1.playSound("spacechase0.MageDelve_alchemy_particlize");
                playedSynthesizeSound = false;
                animStart = (float)Game1.currentGameTime.TotalGameTime.TotalSeconds;
                foreach (var ingred in ingreds)
                    ingred.Item = null;
            }
        }

        internal void CheckRecipe()
        {
            this.output.ItemDisplay = null;
            foreach (var recipeData in AlchemyRecipes.Get())
            {
                var recipe = new Tuple<string, bool>[6];
                int outX = recipeData.Key.IndexOf("/");
                string output = outX >= 0 ? recipeData.Key.Substring(0, outX) : recipeData.Key;
                int outputQty = outX == -1 ? 1 : int.Parse(recipeData.Key.Substring(outX + 1));

                int ir = 0;
                foreach (string ingredData in recipeData.Value)
                {
                    recipe[ir] = new(ingredData, false);
                    ++ir;
                }
                for (; ir < recipe.Length; ++ir)
                    recipe[ir] = new(null, true); // Invalid ingredient, but marked as found so it doesn't matter

                List<ItemSlot> notUsed = new(ingreds);
                for (int i = notUsed.Count - 1; i >= 0; --i)
                {
                    if (notUsed[i].Item == null)
                        notUsed.RemoveAt(i);
                }
                for (int i = 0; i < ingreds.Length; ++i)
                {
                    for (int j = 0; j < recipe.Length; ++j)
                    {
                        if (!notUsed.Contains(ingreds[i]))
                            continue;
                        if (recipe[j].Item1 == null)
                            continue;

                        int? cat = null;
                        if (int.TryParse(recipe[j].Item1, out int cati))
                            cat = cati;
                        if (cat.HasValue && cati < 0)
                        {
                            if (ingreds[i].Item.Category == cati)
                            {
                                recipe[j] = new(recipe[j].Item1, true);
                                notUsed.Remove(ingreds[i]);
                            }
                        }
                        else
                        {
                            if (ingreds[i].Item.QualifiedItemId == recipe[j].Item1 && !recipe[j].Item2)
                            {
                                recipe[j] = new(recipe[j].Item1, true);
                                notUsed.Remove(ingreds[i]);
                            }
                        }
                    }
                }

                bool okay = true;
                for (int i = 0; i < recipe.Length; ++i)
                {
                    if (!recipe[i].Item2)
                    {
                        okay = false;
                        break;
                    }
                }
                if (notUsed.Count > 0)
                    okay = false;

                if (okay)
                {
                    this.output.ItemDisplay = ItemRegistry.Create(output, outputQty);
                    return;
                }
            }
        }

        protected override void cleanupBeforeExit()
        {
            if (output.Item != null)
                Game1.createItemDebris(output.Item, Game1.player.Position, 0, Game1.player.currentLocation);
            foreach (var ingred in ingreds)
            {
                if (ingred.Item != null)
                    Game1.createItemDebris(ingred.Item, Game1.player.Position, 0, Game1.player.currentLocation);
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            held = inventory.leftClick(x, y, held, playSound);

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                if (slot != output)
                {
                    if (slot.Item == null && held != null && held is StardewValley.Object)
                    {
                        if (held.Stack > 1)
                        {
                            slot.Item = held.getOne();
                            slot.Item.Stack = 1;
                            held.Stack -= 1;
                        }
                        else
                        {
                            slot.Item = held;
                            held = null;
                        }
                    }
                    else if (slot.Item != null && held == null)
                    {
                        held = slot.Item;
                        slot.Item = null;
                    }
                    else if (slot.Item != null && held != null && held is StardewValley.Object)
                    {
                        int left = slot.Item.addToStack(held);
                        if (slot.Item.Stack > 1)
                        {
                            left += slot.Item.Stack - 1;
                            slot.Item.Stack = 1;
                        }
                        held.Stack = left;
                        if (left <= 0)
                            held = null;
                    }

                    CheckRecipe();
                }
                else if (output.Item != null)
                {
                    if (held == null || held.canStackWith(output.Item))
                    {
                        held = output.Item;
                        output.Item = null;
                    }
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
            held = inventory.rightClick(x, y, held, playSound);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            ui.Update();
            inventory.update(time);

            if (animStart != null && pixels.Count == 0 && output.ItemDisplay != null && output.Item == null)
            {
                animStart = null;
                output.Item = output.ItemDisplay;
                output.ItemDisplay = null;
            }
        }

        public override void draw(SpriteBatch b)
        {
            drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

            ui.Draw(b);
            inventory.draw(b);

            float delta = (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            float ts = (float)(Game1.currentGameTime.TotalGameTime.TotalSeconds - animStart ?? 0);
            if (ts < 0) ts = 0;
            Vector2 center = new(xPositionOnScreen + width / 2, yPositionOnScreen + (height - 200) / 2);
            float velMult = ts * ts * ts * ts * 5;
            if (ts >= 1.4 && !playedSynthesizeSound)
            {
                Game1.playSound("spacechase0.MageDelve_alchemy_synthesize");
                playedSynthesizeSound = true;
            }
            List<Pixel> toRemove = new();
            for (int i = 0; i < pixels.Count; ++i)
            {
                Pixel pixel = pixels[i];
                float actualScale = (pixel.scale + MathF.Sin(ts * 3) - 3) % 3 + 3;

                Vector2 ppos = new Vector2(pixel.x, pixel.y) + pixel.velocity * delta;
                pixel.x = ppos.X;
                pixel.y = ppos.Y;
                Vector2 toCenter = center - ppos;
                float dist = Vector2.Distance(center, ppos);
                pixel.velocity = pixel.velocity * 0.99f + toCenter / dist * velMult;

                b.Draw(Game1.staminaRect, new Vector2(pixel.x, pixel.y), null, pixel.color, 0, Vector2.Zero, actualScale, SpriteEffects.None, 1);

                if (float.IsNaN(dist))
                {
                    //Console.WriteLine("wat");
                }

                if (dist < 24 || float.IsNaN(dist))
                {
                    toRemove.Add(pixel);
                }
            }
            pixels.RemoveAll((p) => toRemove.Contains(p));

            held?.drawInMenu(b, Game1.getMousePosition().ToVector2(), 1);

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
                var hover = inventory.hover(Game1.getMouseX(), Game1.getMouseY(), null);
                if (hover != null)
                {
                    drawToolTip(b, inventory.hoverText, inventory.hoverTitle, hover);
                }
            }

            drawMouse(b);
        }
    }
}
