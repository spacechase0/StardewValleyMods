using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.UI;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace PreexistingRelationship
{
    public class MarryMenu : IClickableMenu
    {
        private RootElement ui;
        private Table table;

        private StaticContainer selectedContainer;
        private string selectedNPC;

        public MarryMenu()
            : base((Game1.uiViewport.Width - 800) / 2, (Game1.uiViewport.Height - 700) / 2, 800, 700)
        {
            var valid = new List<NPC>();
            foreach (var npc in Utility.getAllCharacters())
            {
                if (npc.datable.Value && npc.getSpouse() == null)
                {
                    valid.Add(npc);
                }
            }

            valid.Sort((a, b) => a.Name.CompareTo(b.Name));

            /*
            for ( int i = 0; i < valid.Count; ++i )
            {
                int oi = Game1.random.Next( valid.Count );
                var other = valid[ oi ];
                valid[ oi ] = valid[ i ];
                valid[ i ] = other;
            }
            */

            this.ui = new RootElement()
            {
                LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen),
            };

            var title = new Label()
            {
                String = Mod.instance.Helper.Translation.Get("menu.title"),
                Bold = true,
            };
            title.LocalPosition = new Vector2((800 - title.Measure().X) / 2, 10);
            this.ui.AddChild(title);

            this.ui.AddChild(new Label()
            {
                String = Mod.instance.Helper.Translation.Get("menu.text").ToString().Replace("\\n", "\n"),
                LocalPosition = new Vector2(50, 75),
                NonBoldScale = 0.75f,
                NonBoldShadow = false,
            });

            this.table = new Table()
            {
                RowHeight = 200,
                Size = new Vector2(700, 500),
                LocalPosition = new Vector2(50, 225),
            };
            for (int i = 0; i < (valid.Count + 2) / 3; ++i)
            {
                var row = new StaticContainer();
                for (int n_ = i * 3; n_ < (i + 1) * 3; ++n_)
                {
                    if (n_ >= valid.Count)
                        continue;
                    int n = n_;

                    var cont = new StaticContainer()
                    {
                        Size = new Vector2(115 * 2, 97 * 2),
                        LocalPosition = new Vector2(250 * (n - i * 3) - 10, 0),
                    };
                    // Note: This is being called 4 times for some reason
                    // Probably a UI framework bug.
                    Action<Element> selCallback = (e) =>
                    {
                        if (this.selectedContainer != null)
                            this.selectedContainer.OutlineColor = null;
                        this.selectedContainer = cont;
                        this.selectedContainer.OutlineColor = Color.Green;
                        this.selectedNPC = valid[n].Name;
                        Log.Trace("Selected " + this.selectedNPC);
                    };
                    cont.AddChild(new Image()
                    {
                        Texture = Game1.mouseCursors,
                        TextureRect = new Rectangle(583, 411, 115, 97),
                        Scale = 2,
                        LocalPosition = new Vector2(0, 0),
                        Callback = selCallback,
                    });
                    cont.AddChild(new Image()
                    {
                        Texture = valid[n].Portrait,
                        TextureRect = new Rectangle(0, 128, 64, 64),
                        Scale = 2,
                        LocalPosition = new Vector2(50, 16),
                    });
                    var name = new Label()
                    {
                        String = valid[n].displayName,
                        NonBoldScale = 0.5f,
                        NonBoldShadow = false,
                    };
                    name.LocalPosition = new Vector2(115 - name.Measure().X / 2, 160);
                    cont.AddChild(name);

                    row.AddChild(cont);
                }
                this.table.AddRow(new Element[] { row });
            }
            this.ui.AddChild(this.table);

            this.ui.AddChild(new Label()
            {
                String = Mod.instance.Helper.Translation.Get("menu.button.cancel"),
                LocalPosition = new Vector2(175, 650),
                Callback = (e) => Game1.exitActiveMenu(),
            });
            this.ui.AddChild(new Label()
            {
                String = Mod.instance.Helper.Translation.Get("menu.button.accept"),
                LocalPosition = new Vector2(500, 650),
                Callback = (e) => { this.DoMarriage(); }
            });
        }

        public override void receiveScrollWheelAction(int direction)
        {
            this.table.Scrollbar.ScrollBy(direction / -120);
        }

        public override void update(GameTime time)
        {
            this.ui.Update();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

            this.ui.Draw(b);

            this.drawMouse(b);
        }

        private void DoMarriage()
        {
            Log.Debug("Marrying " + this.selectedNPC);
            if (this.selectedNPC == null)
                return;

            foreach (var player in Game1.getAllFarmers())
            {
                if (player.spouse == this.selectedNPC)
                {
                    Game1.addHUDMessage(new HUDMessage(Mod.instance.Helper.Translation.Get("spouse-taken")));
                    this.selectedContainer.OutlineColor = null;
                    this.selectedContainer = null;
                    this.selectedNPC = null;
                    return;
                }
            }

            if (!Game1.IsMasterGame)
            {
                Mod.instance.Helper.Multiplayer.SendMessage(new DoMarriageMessage() { NpcName = this.selectedNPC }, nameof(DoMarriageMessage), new[] { Mod.instance.ModManifest.UniqueID }/*, new long[] { Game1.MasterPlayer.UniqueMultiplayerID }*/ );
            }

            Mod.DoMarriage(Game1.player, this.selectedNPC, true);
            //Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "married" ) ) );

            this.selectedContainer.OutlineColor = null;
            this.selectedContainer = null;
            this.selectedNPC = null;
            Game1.exitActiveMenu();
        }
    }
}
