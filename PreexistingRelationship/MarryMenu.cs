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

        private StaticContainer selectedContainer = null;
        private string selectedNPC = null;

        public MarryMenu()
        : base( ( Game1.uiViewport.Width - 800 ) / 2, ( Game1.uiViewport.Height - 700 ) / 2, 800, 700 )
        {
            var valid = new List<NPC>();
            foreach ( var npc in Utility.getAllCharacters() )
            {
                if ( npc.datable.Value && npc.getSpouse() == null )
                {
                    valid.Add( npc );
                }
            }

            valid.Sort( ( a, b ) => a.Name.CompareTo( b.Name ) );

            /*
            for ( int i = 0; i < valid.Count; ++i )
            {
                int oi = Game1.random.Next( valid.Count );
                var other = valid[ oi ];
                valid[ oi ] = valid[ i ];
                valid[ i ] = other;
            }
            */

            ui = new RootElement()
            {
                LocalPosition = new Vector2( xPositionOnScreen, yPositionOnScreen ),
            };

            var title = new Label()
            {
                String = Mod.instance.Helper.Translation.Get( "menu.title" ),
                Bold = true,
            };
            title.LocalPosition = new Vector2( ( 800 - title.Measure().X ) / 2, 10 );
            ui.AddChild( title );

            ui.AddChild( new Label()
            {
                String = Mod.instance.Helper.Translation.Get( "menu.text" ).ToString().Replace( "\\n", "\n" ),
                LocalPosition = new Vector2( 50, 75 ),
                NonBoldScale = 0.75f,
                NonBoldShadow = false,
            } );

            table = new Table()
            {
                RowHeight = 200,
                Size = new Vector2( 700, 500 ),
                LocalPosition = new Vector2( 50, 225 ),
            };
            for ( int i = 0; i < ( valid.Count + 2 ) / 3; ++i )
            {
                var row = new StaticContainer();
                for ( int n_ = i * 3; n_ < ( i + 1 ) * 3; ++n_ )
                {
                    if ( n_ >= valid.Count )
                        continue;
                    int n = n_;

                    var cont = new StaticContainer()
                    {
                        Size = new Vector2( 115 * 2, 97 * 2 ),
                        LocalPosition = new Vector2( 250 * ( n - i * 3 ) - 10, 0 ),
                    };
                    // Note: This is being called 4 times for some reason
                    // Probably a UI framework bug.
                    Action< Element > selCallback = ( e ) =>
                    {
                        if ( selectedContainer != null )
                            selectedContainer.OutlineColor = null;
                        selectedContainer = cont;
                        selectedContainer.OutlineColor = Color.Green;
                        selectedNPC = valid[ n ].Name;
                        Log.trace("Selected " + selectedNPC );
                    };
                    cont.AddChild( new Image()
                    {
                        Texture = Game1.mouseCursors,
                        TextureRect = new Rectangle( 583, 411, 115, 97 ),
                        Scale = 2,
                        LocalPosition = new Vector2( 0, 0 ),
                        Callback = selCallback,
                    } );
                    cont.AddChild( new Image()
                    {
                        Texture = valid[ n ].Portrait,
                        TextureRect = new Rectangle( 0, 128, 64, 64 ),
                        Scale = 2,
                        LocalPosition = new Vector2( 50, 16 ),
                    } );
                    var name = new Label()
                    {
                        String = valid[ n ].displayName,
                        NonBoldScale = 0.5f,
                        NonBoldShadow = false,
                    };
                    name.LocalPosition = new Vector2( 115 - name.Measure().X / 2, 160 );
                    cont.AddChild( name );

                    row.AddChild( cont );
                }
                table.AddRow( new Element[] { row } );
            }
            ui.AddChild( table );

            ui.AddChild( new Label()
            {
                String = Mod.instance.Helper.Translation.Get( "menu.button.cancel" ),
                LocalPosition = new Vector2( 175, 650 ),
                Callback = ( e ) => Game1.exitActiveMenu(),
            } );
            ui.AddChild( new Label()
            {
                String = Mod.instance.Helper.Translation.Get( "menu.button.accept" ),
                LocalPosition = new Vector2( 500, 650 ),
                Callback = ( e ) => { DoMarriage(); }
            } );
        }

        public override void receiveScrollWheelAction( int direction )
        {
            table.Scrollbar.ScrollBy( direction / -120 );
        }

        public override void update( GameTime time )
        {
            ui.Update();
        }

        public override void draw( SpriteBatch b )
        {
            base.draw( b );
            IClickableMenu.drawTextureBox( b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White );

            ui.Draw( b );

            drawMouse( b );
        }

        private void DoMarriage()
        {
            Log.debug( "Marrying " + selectedNPC );
            if ( selectedNPC == null )
                return;

            foreach ( var player in Game1.getAllFarmers() )
            {
                if ( player.spouse == selectedNPC )
                {
                    Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "spouse-taken" ) ) );
                    selectedContainer.OutlineColor = null;
                    selectedContainer = null;
                    selectedNPC = null;
                    return;
                }
            }

            if( !Game1.IsMasterGame )
            {
                Mod.instance.Helper.Multiplayer.SendMessage( new DoMarriageMessage() { NpcName = selectedNPC }, nameof( DoMarriageMessage ), new string[] { Mod.instance.ModManifest.UniqueID }/*, new long[] { Game1.MasterPlayer.UniqueMultiplayerID }*/ );
            }

            Mod.DoMarriage( Game1.player, selectedNPC, true );
            //Game1.addHUDMessage( new HUDMessage( Mod.instance.Helper.Translation.Get( "married" ) ) );

            selectedContainer.OutlineColor = null;
            selectedContainer = null;
            selectedNPC = null;
            Game1.exitActiveMenu();
        }
    }
}
