using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.Buffs
{
    public static class Character_Buffs
    {
        internal class Holder { public readonly NetObjectList<CustomBuffInstance> Value = new(); }

        internal static ConditionalWeakTable< Character, Holder > values = new();

        internal static void Register()
        {
            Mod.instance.SpaceCore.RegisterCustomProperty(
                typeof( Character_Buffs ), "Buffs",
                typeof( NetObjectList<CustomBuffInstance> ),
                AccessTools.Method( typeof( Character_Buffs ), nameof( get_Buffs ) ),
                AccessTools.Method( typeof( Character_Buffs ), nameof( set_Buffs ) ) );
        }

        public static void set_Buffs( this Character character, NetObjectList<CustomBuffInstance> newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetObjectList<CustomBuffInstance> get_Buffs( this Character character )
        {
            var holder = values.GetOrCreateValue( character );
            return holder.Value;
        }
    }

    [HarmonyPatch( typeof( Character ), "initNetFields" )]
    public static class FarmerAddBuffsNetPatch
    {
        public static void Postfix( Character __instance )
        {
            __instance.NetFields.AddField( __instance.get_Buffs(), "spacechase0.CombatOverhaulMod.Buffs" );
        }
    }

    [HarmonyPatch( typeof( Character ), nameof( Character.update ), new Type[] { typeof( GameTime ), typeof( GameLocation ), typeof( long ), typeof( bool ) } )]
    public static class CharacterUpdateBuffsPatch
    {
        public static void Postfix( Character __instance, GameTime time )
        {
            // This is only called for NPCs (including monsters)
            // For some reason Farmer's doesn't get called, instead it has Update
            if ( Game1.IsMasterGame )
                __instance.TickBuffs( time );
        }
    }

    [HarmonyPatch( typeof( Farmer ), nameof( Farmer.Update ) )]
    public static class FarmerUpdateBuffsPatch
    {
        public static void Postfix( Farmer __instance, GameTime time )
        {
            // This is only called for the master game, other ones call UpdateEvenIfOtherPlayer
            // This is good because we only want the local one to update anyways
            __instance.TickBuffs( time );
        }
    }
}
