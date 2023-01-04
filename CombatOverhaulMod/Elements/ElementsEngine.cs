using CombatOverhaulMod;
using HarmonyLib;
using Netcode;
using SpaceShared;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Elements
{
    internal class ElementsEngine
    {
        public ElementsEngine()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Mod.instance.Ready += OnReady;

            Mod.instance.Helper.ConsoleCommands.Add( "player_damage", "Damage the player", OnDamageCommand );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.CombatOverhaulMod\\Elements"))
                e.LoadFrom(() => new Dictionary<string, ElementData>(), AssetLoadPriority.Low);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.CombatOverhaulMod\\DefaultElementalStats"))
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, double>>(), AssetLoadPriority.Low);
        }

        private void OnReady( object sender, EventArgs e )
        {
            Item_ElementalStatOverrides.Register();
        }

        private void OnDamageCommand( string cmd, string[] args )
        {
            if ( args.Length < 1 )
            {
                Log.Info( "Proper usage: player_damage <amt> [element]" );
                return;
            }

            int amt = 0;
            if ( !int.TryParse( args[ 0 ], out amt ) )
            {
                Log.Info( "Amount must be an integer" );
                return;
            }

            string elem = args.Length >= 2 ? args[ 1 ] : null;
            var elements = Game1.content.Load< Dictionary< string, ElementData > >( "spacechase0.CombatOverhaulMod\\Elements" );
            if ( elem != null && !elements.ContainsKey( elem ) )
            {
                Log.Info( "Element must be valid if specified" );
                return;
            }

            Game1.player.TakeDamage( elem, amt );
        }
    }
}
