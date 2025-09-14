using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.UI;
using StardewValley;
using StardewValley.Menus;

namespace PreexistingRelationship.Framework
{
    internal class MarryMenu : IClickableMenu
    {
        private readonly RootElement Ui;
        private readonly Table Table;

        private StaticContainer SelectedContainer;
        private string SelectedNpc;

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

            this.Ui = new RootElement
            {
                LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen)
            };

            var title = new Label
            {
                String = I18n.Menu_Title(),
                Bold = true
            };
            title.LocalPosition = new Vector2((800 - title.Measure().X) / 2, 10);
            this.Ui.AddChild(title);

            this.Ui.AddChild(new Label
            {
                String = I18n.Menu_Text(),
                LocalPosition = new Vector2(50, 75),
                NonBoldScale = 0.75f,
                NonBoldShadow = false
            });

            this.Table = new Table
            {
                RowHeight = 200,
                Size = new Vector2(700, 500),
                LocalPosition = new Vector2(50, 225)
            };
            for (int row = 0; row < (valid.Count + 2) / 3; ++row)
            {
                var rowContainer = new StaticContainer();
                for (int col = row * 3; col < (row + 1) * 3; ++col)
                {
                    if (col >= valid.Count)
                        continue;

                    var cont = new StaticContainer
                    {
                        Size = new Vector2(115 * 2, 97 * 2),
                        LocalPosition = new Vector2(250 * (col - row * 3) - 10, 0)
                    };

                    // Note: This is being called 4 times for some reason
                    // Probably a UI framework bug.
                    string curNpcName = valid[col].Name; // avoid capturing the loop variable in the callback, since it'll change value
                    void SelCallback(Element e)
                    {
                        if (this.SelectedContainer != null)
                            this.SelectedContainer.OutlineColor = null;
                        this.SelectedContainer = cont;
                        this.SelectedContainer.OutlineColor = Color.Green;
                        this.SelectedNpc = curNpcName;
                        Log.Trace("Selected " + this.SelectedNpc);
                    }

                    cont.AddChild(new Image
                    {
                        Texture = Game1.mouseCursors,
                        TexturePixelArea = new Rectangle(583, 411, 115, 97),
                        Scale = 2,
                        LocalPosition = new Vector2(0, 0),
                        Callback = SelCallback
                    });
                    cont.AddChild(new Image
                    {
                        Texture = valid[col].Portrait,
                        TexturePixelArea = new Rectangle(0, 128, 64, 64),
                        Scale = 2,
                        LocalPosition = new Vector2(50, 16)
                    });
                    var name = new Label
                    {
                        String = valid[col].displayName,
                        NonBoldScale = 0.5f,
                        NonBoldShadow = false
                    };
                    name.LocalPosition = new Vector2(115 - name.Measure().X / 2, 160);
                    cont.AddChild(name);

                    rowContainer.AddChild(cont);
                }
                this.Table.AddRow(new Element[] { rowContainer });
            }
            this.Ui.AddChild(this.Table);

            this.Ui.AddChild(new Label
            {
                String = I18n.Menu_Button_Cancel(),
                LocalPosition = new Vector2(175, 650),
                Callback = e => Game1.exitActiveMenu()
            });
            this.Ui.AddChild(new Label
            {
                String = I18n.Menu_Button_Accept(),
                LocalPosition = new Vector2(500, 650),
                Callback = e => this.DoMarriage()
            });
        }

        /// <inheritdoc />
        public override bool overrideSnappyMenuCursorMovementBan()
        {
            return true;
        }

        /// <inheritdoc />
        public override void receiveScrollWheelAction(int direction)
        {
            this.Table.Scrollbar.ScrollBy(direction / -120);
        }

        /// <inheritdoc />
        public override void update(GameTime time)
        {
            this.Ui.Update();
        }

        /// <inheritdoc />
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

            this.Ui.Draw(b);

            this.drawMouse(b);
        }

        private void DoMarriage()
        {
            Log.Debug("Marrying " + this.SelectedNpc);
            if (this.SelectedNpc == null)
                return;

            foreach (var player in Game1.getAllFarmers())
            {
                if (player.spouse == this.SelectedNpc)
                {
                    Game1.addHUDMessage(new HUDMessage(I18n.SpouseTaken()));
                    this.SelectedContainer.OutlineColor = null;
                    this.SelectedContainer = null;
                    this.SelectedNpc = null;
                    return;
                }
            }

            if (!Game1.IsMasterGame)
            {
                Mod.Instance.Helper.Multiplayer.SendMessage(new DoMarriageMessage { NpcName = this.SelectedNpc }, nameof(DoMarriageMessage), new[] { Mod.Instance.ModManifest.UniqueID }/*, new long[] { Game1.MasterPlayer.UniqueMultiplayerID }*/ );
            }

            Mod.DoMarriage(Game1.player, this.SelectedNpc, true);
            //Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "married" ) ) );

            this.SelectedContainer.OutlineColor = null;
            this.SelectedContainer = null;
            this.SelectedNpc = null;
            Game1.exitActiveMenu();
        }
    }
}
