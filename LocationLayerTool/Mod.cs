using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;

namespace LocationLayerTool
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            var harmony = HarmonyInstance.Create( ModManifest.UniqueID );
            harmony.PatchAll();

            Helper.ConsoleCommands.Add( "llt_adddummy", "", doCommand );
        }

        private void doCommand( string cmd, string[] args )
        {
            Game1.locations.Add( new GameLocation( Helper.Content.GetActualAssetKey( "assets/Farm_overlay.tbin" ), "Farm_overlay" ) );
            Game1.game1.parseDebugInput( "warp Farm_overlay 39 31" );
        }
    }
}
