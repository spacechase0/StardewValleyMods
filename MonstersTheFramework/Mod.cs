using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace MonstersTheFramework
{
    // TODO: Chunks
    public class Mod : StardewModdingAPI.Mod, IAssetLoader
    {
        public static Mod instance;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.ConsoleCommands.Add( "world_spawnmonster", "Spawn a (custom) monster.", OnSpawnCommand );
        }

        public bool CanLoad<T>( IAssetInfo asset )
        {
            // TODO: Spawning, monster eradication goals?
            if ( asset.AssetNameEquals( "spacechase0.MonstersTheFramework/Monsters" ) )
                return true;

            return false;
        }

        public T Load<T>( IAssetInfo asset )
        {
            if ( asset.AssetNameEquals( "spacechase0.MonstersTheFramework/Monsters" ) )
                return ( T ) ( object ) new Dictionary<string, MonsterType>();

            return default( T );
        }
        private void OnSpawnCommand( string cmd, string[] args )
        {
            var data = Game1.content.Load< Dictionary< string, MonsterType > >( "spacechase0.MonstersTheFramework/Monsters" );
            if ( !data.ContainsKey( args[ 0 ] ) )
            {
                Log.Info( "No such monster." );
                return;
            }

            Vector2 pos = new Vector2( Convert.ToSingle( args[ 1 ] ), Convert.ToSingle( args[ 2 ] ) ) * Game1.tileSize;

            var monster = new CustomMonster( args[ 0 ] );
            monster.Position = pos;
            Game1.currentLocation.characters.Add( monster );
        }
    }
}
