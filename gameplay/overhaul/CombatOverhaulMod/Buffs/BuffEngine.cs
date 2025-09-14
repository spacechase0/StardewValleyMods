using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs
{
    internal class BuffEngine
    {
        public BuffEngine()
        {
            Mod.instance.Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Mod.instance.Helper.Events.Content.AssetReady += this.Content_AssetReady;
            Mod.instance.Ready += OnReady;

            Mod.instance.Helper.ConsoleCommands.Add( "player_buff", "Apply a buff to the player", OnBuffCommand );
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.CombatOverhaulMod/Buffs"))
                e.LoadFrom(() => new Dictionary<string, CustomBuffData>(), AssetLoadPriority.Low);
        }

        private void Content_AssetReady(object sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.CombatOverhaulMod/Elements"))
                EffectRegistry.InitializeDefaultEffects();
        }

        private void OnBuffCommand( string cmd, string[] args )
        {
            if ( args.Length < 1 )
            {
                Log.Info( "Correct usage: player_buff <buff> [duration]" );
                return;
            }

            float dur = 0;
            if ( args.Length >= 2 && ( !float.TryParse( args[ 1 ], out dur ) || dur < 0 ) )
            {
                Log.Info( "Duration must be a float greater than or equal to 0." );
                return;
            }

            CustomBuffInstance cbi = new( args[ 0 ], dur );
            Game1.player.AddBuff( cbi );
        }

        private void OnReady( object sender, EventArgs e )
        {
            Mod.instance.SpaceCore.RegisterSerializerType( typeof( CustomBuffInstance ) );
            Character_Buffs.Register();
        }
    }
}
