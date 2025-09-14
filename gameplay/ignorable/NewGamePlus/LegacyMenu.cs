using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace NewGamePlus
{
    public class LegacyMenu : IClickableMenu
    {
        private RootElement ui;
        private Table table;

        private Label pointsLeft;
        private Label goldGiven;

        private int pointsTotal;
        private int points;

        private List<LegacySlot> slots = new();

        private int goldPerPoint;

        public LegacyMenu()
        :   base( ( Game1.uiViewport.Width - 900 ) / 2, ( Game1.uiViewport.Height - (Game1.uiViewport.Height - 100) ) / 2, 900, (Game1.uiViewport.Height - 100))
        {
            pointsTotal = points = Game1.MasterPlayer.modData.TryGetValue("NG+/startingPoints", out string pointStr) ? int.Parse(pointStr) : Mod.Config.StartingPoints;
            goldPerPoint = Game1.MasterPlayer.modData.TryGetValue("NG+/goldPerPoint", out string goldStr) ? int.Parse(goldStr) : Mod.Config.GoldPerLeftoverPoint;

            ui = new RootElement();
            ui.LocalPosition = new Vector2(xPositionOnScreen, yPositionOnScreen);

            var title = new Label()
            {
                String = I18n.Menu_Title(),
                Bold = true,
            };
            title.LocalPosition = new Vector2((width - title.Width) / 2, 10);
            ui.AddChild(title);

            pointsLeft = new Label()
            {
                String = I18n.Menu_Points(points),
                LocalPosition = new Vector2(15, 15),
                NonBoldScale = 0.75f,
                NonBoldShadow = false,
            };
            ui.AddChild(pointsLeft);

            goldGiven = new Label()
            {
                String = I18n.Menu_Gold(points * goldPerPoint),
                LocalPosition = new Vector2(width - 15, 15),
                NonBoldScale = 0.75f,
                NonBoldShadow = false,
            };
            goldGiven.LocalPosition -= new Vector2(goldGiven.Width, 0);
            ui.AddChild(goldGiven);

            // Accept needs to come before the table so its click gets processed first.
            // There's a weird bug where if something is displaying outside the scissor rect,
            // it'll still get clicked. Unsure how to cleanly fix.
            var accept = new Label()
            {
                String = I18n.Menu_Accept(),
                Bold = true,
                LocalPosition = new Vector2(500, height - 50),
                Callback = (e) => Accept(),
            };
            ui.AddChild(accept);
            var cancel = new Label()
            {
                String = I18n.Menu_Cancel(),
                Bold = true,
                LocalPosition = new Vector2(200, height - 50),
                Callback = (e) => Game1.activeClickableMenu = null,
            };
            ui.AddChild(cancel);

            PrepareLegacySlots();

            table = new Table()
            {
                RowHeight = ( 128 - 16 ) / 8,
                Size = new Vector2( 700, height - 200 ),
                LocalPosition = new Vector2( 50, 100 ),
            };
            for (int i = 0; i < slots.Count; i += 5)
            {
                List<Element> rowSlots = new();
                rowSlots.Add(slots[i]);
                if (i + 1 < slots.Count)
                {
                    slots[i + 1].LocalPosition = new Vector2(150, 0);
                    rowSlots.Add(slots[i + 1]);
                }
                if (i + 2 < slots.Count)
                {
                    slots[i + 2].LocalPosition = new Vector2(300, 0);
                    rowSlots.Add(slots[i + 2]);
                }
                if (i + 3 < slots.Count)
                {
                    slots[i + 3].LocalPosition = new Vector2(450, 0);
                    rowSlots.Add(slots[i + 3]);
                }
                if (i + 4 < slots.Count)
                {
                    slots[i + 4].LocalPosition = new Vector2(600, 0);
                    rowSlots.Add(slots[i + 4]);
                }
                table.AddRow(rowSlots.ToArray());
                for (int j = 0; j < 5; ++j)
                    table.AddRow(new Element[0]);
            }
            ui.AddChild(table);
        }

        public void Accept()
        {
            if (points < 0)
                return;
            foreach (var slot in slots)
                slot.ApplyCurrent();

            if (points > 0)
                Game1.player.Money += points * goldPerPoint;

            Game1.player.reduceActiveItemByOne();
            Game1.activeClickableMenu = null;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            table.Scrollbar.ScrollBy(-direction / 60);
        }

        public override void update(GameTime time)
        {
            ui.Update();

            points = pointsTotal;
            foreach (var slot in slots)
                points -= slot.slots[slot.active].PointCost;
            pointsLeft.String = I18n.Menu_Points(points);

            goldGiven.String = I18n.Menu_Gold(points * goldPerPoint);
            goldGiven.LocalPosition = new Vector2(width - 15 - goldGiven.Width, goldGiven.LocalPosition.Y);
        }

        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(b, xPositionOnScreen - 12, yPositionOnScreen - 12, width + 24, height + 24, Color.White);

            ui.Draw(b);

            drawMouse(b);
        }

        private void PrepareLegacySlots()
        {
            // Tools
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                   {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 32, 16, 16 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 32, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Game1.player.getToolFromName( "Hoe" ).UpgradeLevel = 1
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112 * 2, 32, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Game1.player.getToolFromName( "Hoe" ).UpgradeLevel = 2
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 32 + 32, 16, 16 ),
                        PointCost = 4,
                        Apply = () => Game1.player.getToolFromName( "Hoe" ).UpgradeLevel = 3
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 32 + 32, 16, 16 ),
                        PointCost = 6,
                        Apply = () => Game1.player.getToolFromName( "Hoe" ).UpgradeLevel = 4
                    }
                   }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 96, 16, 16 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 96, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Game1.player.getToolFromName( "Pickaxe" ).UpgradeLevel = 1
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112 * 2, 96, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Game1.player.getToolFromName( "Pickaxe" ).UpgradeLevel = 2
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 96 + 32, 16, 16 ),
                        PointCost = 4,
                        Apply = () => Game1.player.getToolFromName( "Pickaxe" ).UpgradeLevel = 3
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 96 + 32, 16, 16 ),
                        PointCost = 6,
                        Apply = () => Game1.player.getToolFromName( "Pickaxe" ).UpgradeLevel = 4
                    }
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 160, 16, 16 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 160, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Game1.player.getToolFromName( "Axe" ).UpgradeLevel = 1
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112 * 2, 160, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Game1.player.getToolFromName( "Axe" ).UpgradeLevel = 2
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80, 160 + 32, 16, 16 ),
                        PointCost = 4,
                        Apply = () => Game1.player.getToolFromName( "Axe" ).UpgradeLevel = 3
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 80 + 112, 160 + 32, 16, 16 ),
                        PointCost = 6,
                        Apply = () => Game1.player.getToolFromName( "Axe" ).UpgradeLevel = 4
                    }
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32, 225, 16, 16 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32 + 112, 225, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Game1.player.getToolFromName( "Watering Can" ).UpgradeLevel = 1
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32 + 112 * 2, 225, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Game1.player.getToolFromName( "Watering Can" ).UpgradeLevel = 2
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32, 225 + 32, 16, 16 ),
                        PointCost = 4,
                        Apply = () => Game1.player.getToolFromName( "Watering Can" ).UpgradeLevel = 3
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32 + 112, 225 + 32, 16, 16 ),
                        PointCost = 6,
                        Apply = () => Game1.player.getToolFromName( "Watering Can" ).UpgradeLevel = 4
                    }
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 112, 80, 16, 16 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 80, 96, 16, 16 ),
                        PointCost = 2,
                        Apply = () =>
                        {
                            Game1.player.mailReceived.Add("gotGoldenScythe");
                            Game1.player.Items[ Game1.player.Items.IndexOf( Game1.player.Items.First( i => i is MeleeWeapon mw && mw.ItemId == MeleeWeapon.scytheId ) ) ]
                                = new MeleeWeapon( MeleeWeapon.goldenScytheId );
                        }
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.mouseCursors,
                        TexRect = new Rectangle( 564, 102, 18, 26 ),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.mouseCursors,
                        TexRect = new Rectangle( 564 + 18 * 2, 102, 18, 26 ),
                        PointCost = 1,
                        Apply = () => Game1.player.trashCanLevel = 2,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.mouseCursors,
                        TexRect = new Rectangle( 564 + 18 * 4, 102, 18, 26 ),
                        PointCost = 2,
                        Apply = () => Game1.player.trashCanLevel = 4,
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_ReturnScepter(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.toolSpriteSheet,
                        TexRect = new Rectangle( 32, 0, 16, 16 ),
                        PointCost = 3,
                        Apply = () => Utility.CollectOrDrop( new Wand() ),
                    },
                }));

            // Weapons
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Sword(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 16, 0, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "1" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 112, 0, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "7" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 64, 0, 16, 16 ),
                        PointCost = 4,
                        Apply = () =>
                        {
                            Game1.addMail( "galaxySword", true );
                            Utility.CollectOrDrop( new MeleeWeapon( "4" ) );
                        }
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Dagger(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 64, 32, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "20" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 48, 96, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "51" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = new Rectangle( 112, 32, 16, 16 ),
                        PointCost = 4,
                        Apply = () =>
                        {
                            Game1.addMail( "galaxySword", true );
                            Utility.CollectOrDrop( new MeleeWeapon( "23" ) );
                        }
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Club(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Tool.weaponsTexture, 24, 16, 16 ),
                        PointCost = 1,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "24" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Tool.weaponsTexture, 46, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Utility.CollectOrDrop( new MeleeWeapon( "46" ) ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Tool.weaponsTexture,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Tool.weaponsTexture, 29, 16, 16 ),
                        PointCost = 4,
                        Apply = () =>
                        {
                            Game1.addMail( "galaxySword", true );
                            Utility.CollectOrDrop( new MeleeWeapon( "29" ) );
                        }
                    },
                }));

            // Misc
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_House_0(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_House_1(),
                        PointCost = 5,
                        Apply = () => Game1.game1.parseDebugInput( "hu 1" ),
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_TownKey(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.objectSpriteSheet,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 912, 16, 16 ),
                        PointCost = 5,
                        Apply = () => Game1.player.HasTownKey = true,
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Horse(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.objectSpriteSheet,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 911, 16, 16 ),
                        PointCost = 5,
                        Apply = () =>
                        {
                            Utility.CollectOrDrop( new StardewValley.Object( $"{Mod.instance.ModManifest.UniqueID}_StableToken", 1 ) );
                            Utility.CollectOrDrop( new StardewValley.Object( "911", 1 ) );
                        }
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Backpack(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.mouseCursors,
                        TexRect = new Rectangle( 255, 1436, 12, 14 ),
                        PointCost = 2,
                        Apply = () => Game1.player.increaseBackpackSize( 12 ),
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.mouseCursors,
                        TexRect = new Rectangle( 255 + 12, 1436, 12, 14 ),
                        PointCost = 5,
                        Apply = () => Game1.player.increaseBackpackSize( 24 ),
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Recipes(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/recipes1.png"),
                        TexRect = new Rectangle( 0, 0, 50, 50 ),
                        PointCost = 1,
                        Apply = () =>
                        {
                            Game1.player.craftingRecipes.Add( "Scarecrow", 0 );
                            Game1.player.craftingRecipes.Add( "Sprinkler", 0 );
                            Game1.player.craftingRecipes.Add( "Cherry Bomb", 0 );
                        }
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Mod.instance.Helper.ModContent.Load<Texture2D>("assets/recipes2.png"),
                        TexRect = new Rectangle( 0, 0, 50, 50 ),
                        PointCost = 3,
                        Apply = () =>
                        {
                            Game1.player.craftingRecipes.Add( "Scarecrow", 0 );
                            Game1.player.craftingRecipes.Add( "Sprinkler", 0 );
                            Game1.player.craftingRecipes.Add( "Cherry Bomb", 0 );
                            Game1.player.craftingRecipes.Add( "Crab Pot", 0 );
                            Game1.player.craftingRecipes.Add( "Quality Sprinkler", 0 );
                            Game1.player.craftingRecipes.Add( "Bomb", 0 );
                        }
                    },
                }));
            slots.Add(new LegacySlot(new LegacySlot.SlotEntry[]
                {
                    new LegacySlot.SlotEntry()
                    {
                        Text = I18n.Menu_Item_Ring(),
                        PointCost = 0,
                    },
                    new LegacySlot.SlotEntry()
                    {
                        Texture = Game1.objectSpriteSheet,
                        TexRect = Game1.getSourceRectForStandardTileSheet( Game1.objectSpriteSheet, 527, 16, 16 ),
                        PointCost = 2,
                        Apply = () => Utility.CollectOrDrop( new Ring( "527" ) )
                    },
                }));
        }
    }
}
