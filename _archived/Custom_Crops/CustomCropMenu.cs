using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using StardewValley;
using StardewValley.Menus;

namespace Custom_Crops
{
    public class CustomCropMenu : IClickableMenu
    {
        public const int WIDTH = 800;
        public const int HEIGHT = 600;

        private RootElement ui;
        private StaticContainer stuff;
        private ItemSlot seedSlot;
        private Label growthLabel;
        private Label valueLabel;
        private Label yieldLabel;
        private Label seedYieldLabel;
        private Table traitsTable;

        private InventoryMenu playerInv;

        private Item held;

        private int animDir = 0;

        public CustomCropMenu()
        : base((Game1.uiViewport.Width - WIDTH) / 2, (Game1.uiViewport.Height - HEIGHT) / 2, WIDTH, HEIGHT)
        {
            ui = new RootElement();
            ui.LocalPosition = new Microsoft.Xna.Framework.Vector2(xPositionOnScreen, yPositionOnScreen);

            seedSlot = new ItemSlot()
            {
                ItemDisplay = new DummyItem(I18n.Menu_Seeds_Name(), I18n.Menu_Seeds_Description(), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 472, 16, 16)),
                TransparentItemDisplay = true,
            };
            seedSlot.LocalPosition = new(0, (HEIGHT - seedSlot.Height) / 2 - 125);
            seedSlot.UserData = (Func<Item, bool>)((item) => (item is StardewValley.Object sobj && sobj.Category == StardewValley.Object.SeedsCategory));

            stuff = new StaticContainer()
            {
                LocalPosition = new((WIDTH - seedSlot.Width) / 2, 0),
            };
            stuff.AddChild(seedSlot);
            ui.AddChild(stuff);

            var statsLebel = new Label()
            {
                String = I18n.Stats(),
                LocalPosition = new((WIDTH - seedSlot.Width) / 2 + seedSlot.Width, 8),
            };
            statsLebel.LocalPosition = new(statsLebel.LocalPosition.X - statsLebel.Measure().X / 2, statsLebel.LocalPosition.Y);
            stuff.AddChild(statsLebel);

            stuff.AddChild(growthLabel = new Label()
            {
                String = I18n.Stats_Growth( 0 ),
                LocalPosition = new(150, statsLebel.LocalPosition.Y + statsLebel.Measure().Y + 16 ),
                HoverTextColor = Color.Red,
                IdleTextColor = Color.Red,
                NonBoldScale = 0.5f,
                NonBoldShadow = false,
            });
            stuff.AddChild(valueLabel = new Label()
            {
                String = I18n.Stats_Value(0),
                LocalPosition = new(growthLabel.LocalPosition.X + WIDTH / 5 * 1, statsLebel.LocalPosition.Y + statsLebel.Measure().Y + 16),
                HoverTextColor = Color.Red,
                IdleTextColor = Color.Red,
                NonBoldScale = 0.5f,
                NonBoldShadow = false,
            });

            stuff.AddChild(yieldLabel = new Label()
            {
                String = I18n.Stats_Yield(0),
                LocalPosition = new(growthLabel.LocalPosition.X + WIDTH / 5 * 2, statsLebel.LocalPosition.Y + statsLebel.Measure().Y + 16),
                HoverTextColor = Color.Red,
                IdleTextColor = Color.Red,
                NonBoldScale = 0.5f,
                NonBoldShadow = false,
            });

            stuff.AddChild(seedYieldLabel = new Label()
            {
                String = I18n.Stats_SeedYield(0),
                LocalPosition = new(growthLabel.LocalPosition.X + WIDTH / 5 * 3, statsLebel.LocalPosition.Y + statsLebel.Measure().Y + 16),
                HoverTextColor = Color.Red,
                IdleTextColor = Color.Red,
                NonBoldScale = 0.5f,
                NonBoldShadow = false,
            });

            var traitsLabel = new Label()
            {
                String = I18n.Traits(),
                LocalPosition = new((WIDTH - seedSlot.Width) / 2 + seedSlot.Width, growthLabel.LocalPosition.Y + growthLabel.Measure().Y + 8 ),
            };
            traitsLabel.LocalPosition = new(traitsLabel.LocalPosition.X - traitsLabel.Measure().X / 2, traitsLabel.LocalPosition.Y);
            stuff.AddChild(traitsLabel);

            traitsTable = new Table()
            {
                RowHeight = 2, // We want two rows per row of 20px for smoother scrolling, but the table as padding of 16, (20 - 16)/2 = 2
                LocalPosition = new( seedSlot.LocalPosition.X + seedSlot.Width + 48, traitsLabel.LocalPosition.Y + traitsLabel.Measure().Y + 20 ),
                Size = new( 550, 160 ),
            };

            string[] validTraits = new[]
            {
                "scythable",
                "paddy",
                "sturdy",
                "season-spring",
                "season-summer",
                "season-fall",
                "season-winter",
                "hardy",
                "rain-lover",
                "improved-vacuoles",
                "macronutrient-replenisher-1",
                "macronutrient-replenisher-2",
                "macronutrient-replenisher-3",
            };

            foreach ( string trait in validTraits )
            {
                var traitLabel = new Label()
                {
                    String = Mod.instance.Helper.Translation.Get($"traits.{trait}.name"),
                    LocalPosition = new(16, 0),
                    IdleTextColor = Color.Red,
                    HoverTextColor = Color.Red,
                    Callback = (e) => { /* todo - apply trait as pending */ },
                    NonBoldScale = 0.75f,
                    NonBoldShadow = false,
                    UserData = trait,
                };
                if (trait == "season-summer") // Temp for showing off GIF
                    traitLabel.IdleTextColor = traitLabel.HoverTextColor = Color.Green;
                traitsTable.AddRow(new[] { traitLabel });
                traitsTable.AddRow(new Element[ 0 ]); // Trick for smoother scrolling - also why row height isn't 20
            }

            foreach (var child in stuff.Children)
            {
                if (child is Label l)
                {
                    l.IdleTextColor = new(l.IdleTextColor, 0);
                    l.HoverTextColor = new(l.HoverTextColor, 0);
                }
            }

            playerInv = new InventoryMenu(xPositionOnScreen + (WIDTH - 768) / 2, yPositionOnScreen + HEIGHT - 192, true, Game1.player.Items );
        }

        protected override void cleanupBeforeExit()
        {
            if (seedSlot.Item != null)
            {
                Utility.CollectOrDrop(seedSlot.Item);
                seedSlot.Item = null;
            }
            base.cleanupBeforeExit();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (animDir != 0)
                return;

            base.receiveLeftClick(x, y, playSound);
            this.held = playerInv.leftClick(x, y, this.held, playSound);

            bool seedSlotFull = seedSlot.Item != null;

            if (ItemWithBorder.HoveredElement is ItemSlot slot)
            {
                int maxInSlot = 1;
                var matcher = (Func<Item, bool>) slot.UserData;
                if (slot.Item == null && held != null && ( matcher == null || matcher( held ) ))
                {
                    if (held.Stack > maxInSlot)
                    {
                        slot.Item = held.getOne();
                        slot.Item.Stack = maxInSlot;
                        held.Stack -= maxInSlot;
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
                else if (slot.Item != null && held != null && ( matcher == null || matcher( held ) ))
                {
                    int stackLeft = slot.Item.addToStack(held);
                    if (slot.Item.Stack > maxInSlot)
                    {
                        stackLeft += slot.Item.Stack - maxInSlot;
                        slot.Item.Stack = maxInSlot;
                    }
                    held.Stack = stackLeft;
                    if (stackLeft <= 0)
                        held = null;
                }
            }

            if ( seedSlotFull != ( seedSlot.Item != null ) )
            {
                if ( seedSlot.Item != null ) // now full
                {
                    animDir = 1;
                }
                else // now empty
                {
                    animDir = -3;
                }
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
            this.held = playerInv.rightClick(x, y, this.held, playSound);
        }

        public override void update(GameTime time)
        {
            ui.Update();
            playerInv.update(time);

            if ( animDir > 0 )
            {
                switch ( animDir )
                {
                    case 1:
                        if (stuff.LocalPosition.X > 16)
                            stuff.LocalPosition = new Vector2(stuff.LocalPosition.X - 6, stuff.LocalPosition.Y);
                        else
                            ++animDir;
                        break;
                    case 2:
                        bool hitEnd = false;
                        foreach (var child in stuff.Children)
                        {
                            if (child is Label label)
                            {
                                label.IdleTextColor = new Color(label.IdleTextColor, label.IdleTextColor.A + 10);
                                label.HoverTextColor = new Color(label.HoverTextColor, label.HoverTextColor.A + 10);
                                if (label.IdleTextColor.A >= 255)
                                    hitEnd = true;
                            }
                        }
                        if (hitEnd)
                            ++animDir;
                        break;
                    case 3:
                        stuff.AddChild(traitsTable);
                        ++animDir;
                        break;
                    default:
                        animDir = 0;
                        break;
                }
            }
            else if ( animDir < 0 )
            {
                switch (animDir)
                {
                    case -1:
                        if (stuff.LocalPosition.X < (WIDTH - seedSlot.Width ) / 2 )
                            stuff.LocalPosition = new Vector2(stuff.LocalPosition.X + 6, stuff.LocalPosition.Y);
                        else
                            ++animDir;
                        break;
                    case -2:
                        bool hitEnd = false;
                        foreach (var child in stuff.Children)
                        {
                            if (child is Label label)
                            {
                                label.IdleTextColor = new Color(label.IdleTextColor, label.IdleTextColor.A - 10);
                                label.HoverTextColor = new Color(label.HoverTextColor, label.HoverTextColor.A - 10);
                                if (label.IdleTextColor.A <= 0)
                                    hitEnd = true;
                            }
                        }
                        if (hitEnd)
                            ++animDir;
                        break;
                    case -3:
                        stuff.RemoveChild(traitsTable);
                        ++animDir;
                        break;
                    default:
                        animDir = 0;
                        break;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, xPositionOnScreen - 12, yPositionOnScreen - 12, width + 24, height + 24, Color.White);
            IClickableMenu.drawTextureBox(b, playerInv.xPositionOnScreen - 12, playerInv.yPositionOnScreen - 12 - 36, playerInv.width + 24, playerInv.height + 24 + 24, Color.White);

            ui.Draw(b);
            playerInv.draw(b);

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
                var hover = playerInv.hover(Game1.getMouseX(), Game1.getMouseY(), null);
                if (hover != null)
                {
                    drawToolTip(b, playerInv.hoverText, playerInv.hoverTitle, hover);
                }
            }

            drawMouse(b);
        }
    }
}
