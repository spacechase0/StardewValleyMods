using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MonstersTheFramework
{
    // TODO: Chunks
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static IDynamicGameAssetsApi dga;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.ConsoleCommands.Add( "world_spawnmonster", "Spawn a (custom) monster.", OnSpawnCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.DayStarted += (e, a) => DoSpawning(SpawnTryTime.OnDayStart);
            Helper.Events.GameLoop.TimeChanged += (e, a) => DoSpawning(SpawnTryTime.OnTimeChange);
            Helper.Events.Player.Warped += (e, a) => DoSpawning(SpawnTryTime.OnLocationChange, a.NewLocation.NameOrUniqueName);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;

        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MonstersTheFramework/Monsters"))
                e.LoadFrom( static () => new Dictionary<string, MonsterType>(), AssetLoadPriority.Exclusive);
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.MonstersTheFramework/Spawning"))
                e.LoadFrom(static () => new Dictionary<string, SpawnData>(), AssetLoadPriority.Exclusive);
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

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
        }

        private void DoSpawning( SpawnTryTime when, string arg = "" )
        {
            if (!Context.IsMainPlayer && when != SpawnTryTime.OnLocationChange )
                return;

            var spawning = Game1.content.Load<Dictionary<string, SpawnData>>("spacechase0.MonstersTheFramework/Spawning");
            var monsters = Game1.content.Load<Dictionary<string, MonsterType>>("spacechase0.MonstersTheFramework/Monsters");

            foreach ( var spawn in spawning )
            {
                if (spawn.Value.When != when)
                    continue;

                string who = spawn.Value.Who.Choose();
                if ( !monsters.ContainsKey( who ) )
                {
                    Log.Warn("Can't spawn " + who + " because that monster does not exist!");
                    continue;
                }

                if (when == SpawnTryTime.OnLocationChange && spawn.Value.Where != arg)
                    continue;

                GameLocation where = Game1.getLocationFromName(spawn.Value.Where);
                if (spawn.Value.Where == null)
                    where = Game1.currentLocation;
                if (where == null)
                    continue;

                for (int i = 0; i < spawn.Value.HowMany; ++i)
                {
                    Rectangle wherea = spawn.Value.WhereArea ?? new Rectangle(0, 0, where.Map.Layers[0].LayerWidth, where.Map.Layers[0].LayerHeight);

                    Vector2 pos = new Vector2(-1, -1);
                    // Limit amount of tries so we don't hang the game for a long time on weird maps with bad luck
                    for (int t = 0; t < 25 && (pos == new Vector2(-1, -1) || !where.isTileLocationTotallyClearAndPlaceable(pos)); ++t)
                        pos = new Vector2(wherea.X + Game1.random.Next(wherea.Width), wherea.Y + Game1.random.Next(wherea.Height));
                    if (!where.isTileLocationTotallyClearAndPlaceable(pos))
                        continue;

                    var monster = new CustomMonster(who);
                    monster.Position = pos * Game1.tileSize;
                    where.characters.Add(monster);
                }
            }
        }
    }
}
