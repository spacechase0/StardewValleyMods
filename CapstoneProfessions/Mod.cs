using Harmony;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProfessions
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static readonly int PROFESSION_TIME = 1000;
        public static readonly int PROFESSION_PROFIT = 1001;

        internal static Texture2D clockTex;

        public override void Entry( IModHelper helper )
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.Events.Player.Warped += OnWarped;

            SpaceEvents.ShowNightEndMenus += OnNightMenus;

            clockTex = Helper.Content.Load<Texture2D>( "assets/clock.png" );
        }

        private void OnWarped( object sender, WarpedEventArgs e )
        {
            if ( e.IsLocalPlayer && Helper.ModRegistry.IsLoaded( "cantorsdust.AllProfessions" ) )
            {
                if ( e.Player.professions.Contains( PROFESSION_TIME ) && !e.Player.professions.Contains( PROFESSION_PROFIT ) )
                    e.Player.professions.Add( PROFESSION_PROFIT );
                if ( !e.Player.professions.Contains( PROFESSION_TIME ) && e.Player.professions.Contains( PROFESSION_PROFIT ) )
                    e.Player.professions.Add( PROFESSION_TIME );
            }
        }

        private void OnNightMenus( object sender, EventArgsShowNightEndMenus e )
        {
            if ( Game1.player.farmingLevel.Value == 10 && Game1.player.foragingLevel.Value == 10 &&
                 Game1.player.fishingLevel.Value == 10 && Game1.player.miningLevel.Value == 10 &&
                 Game1.player.combatLevel.Value == 10 )
            {
                if ( Game1.player.professions.Contains( PROFESSION_TIME ) || Game1.player.professions.Contains( PROFESSION_PROFIT ) )
                    return;

                Log.debug( "Doing profession menu" );

                if ( Game1.endOfNightMenus.Count == 0 )
                    Game1.endOfNightMenus.Push( new SaveGameMenu() );

                Game1.endOfNightMenus.Push( new CapstoneProfessionMenu() );
            }
        }
    }

    [HarmonyPatch( typeof( StardewValley.Object ), "getPriceAfterMultipliers" )]
    public static class ObjectPriceTranspiler
    {
        public static void Postfix( ref float __result )
        {
            float mult = 1;
            foreach ( var player in Game1.getAllFarmers() )
            {
                if ( player.professions.Contains( Mod.PROFESSION_PROFIT ) )
                {
                    mult += 0.05f;
                }
            }
            __result *= mult;
        }
    }

    [ HarmonyPatch( typeof( Game1 ), nameof( Game1.UpdateGameClock ) )]
    public static class GameClockTranspiler
    {
        public static int GetTimeInterval()
        {
            float mult = 1;
            foreach ( var player in Game1.getAllFarmers() )
            {
                if ( player.professions.Contains( Mod.PROFESSION_TIME ) )
                {
                    mult += 0.2f;
                }
            }

            return ( int ) ( 7000 * mult );
        }

        public static IEnumerable<CodeInstruction> Transpiler( ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns )
        {
            List<CodeInstruction> ret = new List<CodeInstruction>();
            foreach ( var insn in insns )
            {
                if ( insn.opcode == OpCodes.Ldc_I4 && (int) insn.operand == 7000 )
                {
                    ret.Add( new CodeInstruction( OpCodes.Call, AccessTools.Method( typeof( GameClockTranspiler ), nameof( GetTimeInterval ) ) ) );
                    continue;
                }
                ret.Add( insn );
            }
            return ret;
        }
    }
}
