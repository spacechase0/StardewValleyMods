using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MageDelve.Mana
{
    public class ManaEngine
    {
        internal static Texture2D manaBg;
        internal static Color manaCol = new Color( 0, 48, 255 ); 

        public ManaEngine()
        {
            manaBg = Mod.instance.Helper.ModContent.Load<Texture2D>( "assets/manabg.png" );

            Mod.instance.Helper.ConsoleCommands.Add( "player_addmana", "Add mana to the player.", OnAddManaCommand );
            Mod.instance.Helper.ConsoleCommands.Add( "player_setmaxmana", "Set the max mana of the player.", OnMaxManaCommand );

            Mod.ApisReady += OnReady;
        }

        private void OnAddManaCommand( string cmd, string[] args )
        {
            if ( args.Length != 1 )
            {
                Log.Info( "Proper usage: player_addmana <amount>" );
                return;
            }

            float amt = 0;
            if ( !float.TryParse( args[ 0 ], out amt ) )
            {
                Log.Info( "Amount must be a number." );
                return;
            }

            Game1.player.AddMana( amt );
        }

        private void OnMaxManaCommand( string cmd, string[] args )
        {
            if ( args.Length != 1 )
            {
                Log.Info( "Proper usage: player_setmaxmana <amount>" );
                return;
            }

            float amt = 0;
            if ( !float.TryParse( args[ 0 ], out amt ) )
            {
                Log.Info( "Amount must be a number." );
                return;
            }

            Game1.player.SetMaxMana( amt );
        }

        private void OnReady( object sender, EventArgs e )
        {
            Farmer_Mana.Register();
        }
    }
}
