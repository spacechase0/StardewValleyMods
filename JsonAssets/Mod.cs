using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Harmony;
using JsonAssets.Game;
using JsonAssets.PackData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using SpaceCore;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace JsonAssets
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public static Mod instance;
        private ExpandedPreconditionsUtilityAPI epu;
        private HarmonyInstance harmony;

        private static Dictionary<string, ContentPack> contentPacks = new Dictionary<string, ContentPack>();

        public static CommonPackData Find( string fullId )
        {
            int slash = fullId.IndexOf( '/' );
            string pack = fullId.Substring( 0, slash );
            string item = fullId.Substring( slash + 1 );
            return contentPacks.ContainsKey( pack ) ? contentPacks[ pack ].Find( item ) : null;
        }
        
        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            helper.ConsoleCommands.Add( "list_ja", "...", OnListCommand );
            helper.ConsoleCommands.Add( "player_addja", "...", OnAddCommand );

            harmony = HarmonyInstance.Create( "spacechase0.JsonAssets" );
            harmony.PatchAll();

            LoadContentPacks();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            epu = Helper.ModRegistry.GetApi<ExpandedPreconditionsUtilityAPI>( "Cherry.ExpandedPreconditionsUtility" );
            epu.Initialize( false, ModManifest.UniqueID );

            var spacecore = Helper.ModRegistry.GetApi<SpaceCoreAPI>( "spacechase0.SpaceCore" );
            spacecore.RegisterSerializerType( typeof( CustomObject ) );
        }

        private void OnListCommand( string cmd, string[] args )
        {
            string output = "";
            foreach ( var cp in contentPacks )
            {
                output += cp.Key + ":\n";
                foreach ( var entry in cp.Value.items )
                {
                    output += "\t" + entry.Key + "\n";
                }
                output += "\n";
            }

            Log.info( output );
        }

        private void OnAddCommand( string cmd, string[] args )
        {
            if ( args.Length < 1 )
            {
                Log.info( "Usage: player_addja <mod.id/ItemId> [amount]" );
                return;
            }

            var data = Find( args[ 0 ] );
            if ( data == null )
            {
                Log.error( $"Item '{args[ 0 ]}' not found." );
                return;
            }

            var item = data.ToItem();
            if ( item == null )
            {
                Log.error( $"The item '{args[ 0 ]}' has no inventory form." );
                return;
            }
            if ( args.Length >= 2 )
            {
                item.Stack = int.Parse( args[ 1 ] );
            }

            Game1.player.addItemByMenuIfNecessary( item );
        }

        private void LoadContentPacks()
        {
            foreach ( var cp in Helper.ContentPacks.GetOwned() )
            {
                Log.debug( $"Loading content pack \"{cp.Manifest.Name}\"..." );
                if ( cp.Manifest.ExtraFields == null ||
                     !cp.Manifest.ExtraFields.ContainsKey( "JAFormatVersion" ) ||
                     !int.TryParse( cp.Manifest.ExtraFields[ "JAFormatVersion" ].ToString(), out int ver ) ||
                     ver < 2 )
                {
                    Log.error( "Old-style JA packs not supported! Please use the converter." );
                    continue;
                }
                var pack = new ContentPack( cp );
                contentPacks.Add( cp.Manifest.UniqueID, pack );
            }
        }

        public bool CanEdit<T>( IAssetInfo asset )
        {
            return asset.AssetNameEquals( "Data\\ObjectInformation" );
        }

        public void Edit<T>( IAssetData asset )
        {
            asset.AsDictionary<int, string>().Data.Add( 1720, "JA Dummy Object/0/0/Basic -20/JA Dummy Object/You shouldn't have this./food/0 0 0 0 0 0 0 0 0 0 0 0/0" );
        }
    }
}
