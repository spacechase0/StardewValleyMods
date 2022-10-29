using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MisappliedPhysicalities.Game;
using MisappliedPhysicalities.Game.Objects;
using MisappliedPhysicalities.VirtualProperties;
using Netcode;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;

/* Art:
 *  paradigmnomad (most art)
 */

namespace MisappliedPhysicalities
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        internal static Configuration Config;
        internal static IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        public override void Entry( IModHelper helper )
        {
            Log.Monitor = Monitor;
            instance = this;

            Config = Helper.ReadConfig<Configuration>();

            Assets.Load( helper.ModContent );

            Helper.ConsoleCommands.Add( "mp_items", "...", OnItemsCommand );

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            var harmony = new Harmony( ModManifest.UniqueID );
            harmony.PatchAll();
        }

        private void OnItemsCommand( string cmd, string[] args )
        {
            Dictionary<ISalable, int[]> stock = new();
            {
                stock.Add( new Drill(), new int[] { 0, int.MaxValue } );
                stock.Add( new WireCutter(), new int[] { 0, int.MaxValue } );
                stock.Add( new ConveyorBelt(), new int[] { 0, int.MaxValue } );
                stock.Add( new Unhopper( Vector2.Zero ), new int[] { 0, int.MaxValue } );
                stock.Add( new LogicConnector(), new int[] { 0, int.MaxValue } );
                stock.Add( new LeverBlock(), new int[] { 0, int.MaxValue } );
                foreach ( var data in dgaPack.GetItems() )
                {
                    var item = data.ToItem();
                    stock.Add( item, new int[] { 0, int.MaxValue } );
                }
            }
            Game1.activeClickableMenu = new ShopMenu( stock );
        }

        private void OnGameLaunched( object sender, GameLaunchedEventArgs e )
        {
            var sc = Helper.ModRegistry.GetApi< ISpaceCoreApi >( "spacechase0.SpaceCore" );
            sc.RegisterSerializerType( typeof( NullObject ) );
            sc.RegisterSerializerType( typeof( Drill ) );
            sc.RegisterSerializerType( typeof( ConveyorBelt ) );
            sc.RegisterSerializerType( typeof( Unhopper ) );
            sc.RegisterSerializerType( typeof( WireCutter ) );
            sc.RegisterSerializerType( typeof( ConnectorBase ) );
            sc.RegisterSerializerType( typeof( LogicConnector ) );
            sc.RegisterSerializerType( typeof( LeverBlock ) );
            sc.RegisterCustomProperty( typeof( GameLocation ), "BelowGroundObjects",
                                       typeof( NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> ),
                                       AccessTools.Method( typeof( GameLocation_BelowGroundObjects ), nameof( GameLocation_BelowGroundObjects.get_BelowGroundObjects ) ),
                                       AccessTools.Method( typeof( GameLocation_BelowGroundObjects ), nameof( GameLocation_BelowGroundObjects.set_BelowGroundObjects ) ) );
            sc.RegisterCustomProperty( typeof( GameLocation ), "ElevatedObjects",
                                       typeof( NetVector2Dictionary<StardewValley.Object, NetRef<StardewValley.Object>> ),
                                       AccessTools.Method( typeof( GameLocation_ElevatedObjects ), nameof( GameLocation_ElevatedObjects.get_ElevatedObjects ) ),
                                       AccessTools.Method( typeof( GameLocation_ElevatedObjects ), nameof( GameLocation_ElevatedObjects.set_ElevatedObjects ) ) );

            dga = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>( "spacechase0.DynamicGameAssets" );
            dga.AddEmbeddedPack( this.ModManifest, Path.Combine( Helper.DirectoryPath, "assets", "dga" ) );
            dgaPack = DynamicGameAssets.Mod.GetPacks().First( cp => cp.GetManifest().UniqueID == ModManifest.UniqueID );

            var gmcm = Helper.ModRegistry.GetApi< IGenericModConfigMenuApi >( "spacechase0.GenericModConfigMenu" );
            gmcm.Register( ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig( Config ) );
            gmcm.AddKeybindList( ModManifest, () => Config.PlacementModifier, ( kl ) => Config.PlacementModifier = kl, () => Helper.Translation.Get( "config.placement-modifier.name" ), () => Helper.Translation.Get( "config.placement-modifier.tooltip" ) );
        }
    }
}
