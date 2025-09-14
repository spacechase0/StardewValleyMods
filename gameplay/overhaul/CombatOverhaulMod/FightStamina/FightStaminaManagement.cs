using CombatOverhaulMod.Combat;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CombatOverhaulMod.FightStamina
{
    public static class Farmer_FightStamina
    {
        internal class Holder { public readonly NetFloat Value = new( 1 ); public readonly NetFloat MaxValue = new( 1 ); }

        internal static ConditionalWeakTable< Farmer, Holder > values = new();

        internal static void Register()
        {
            Mod.instance.SpaceCore.RegisterCustomProperty(
                typeof( Farmer ), "FightStamina",
                typeof( NetFloat ),
                AccessTools.Method( typeof(Farmer_FightStamina), nameof( get_FightStaminaValue ) ),
                AccessTools.Method( typeof(Farmer_FightStamina), nameof( set_FightStaminaValue ) ) );
            Mod.instance.SpaceCore.RegisterCustomProperty(
                typeof(Farmer), "MaxFightStamina",
                typeof(NetFloat),
                AccessTools.Method(typeof(Farmer_FightStamina), nameof(get_FightStaminaMaxValue)),
                AccessTools.Method(typeof(Farmer_FightStamina), nameof(set_FightStaminaMaxValue)));
        }

        public static void set_FightStaminaValue( this Farmer item, NetFloat newVal )
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetFloat get_FightStaminaValue( this Farmer item )
        {
            var holder = values.GetOrCreateValue( item );
            return holder.Value;
        }

        public static void set_FightStaminaMaxValue(this Farmer item, NetFloat newVal)
        {
            // We don't actually want a setter for this one, since it should be readonly
            // Net types are weird
            // Or do we? Serialization
        }

        public static NetFloat get_FightStaminaMaxValue(this Farmer item)
        {
            var holder = values.GetOrCreateValue(item);
            return holder.MaxValue;
        }
    }

    // TODO: Make melee weapon use it
    [HarmonyPatch(typeof(Farmer), "performBeginUsingTool")]
    [HarmonyPriority(Priority.High)]
    public static class MeleeWeaponUseStaminaPatch
    {
        public static bool Prefix(Farmer __instance)
        {
            if (__instance != Game1.player || __instance.CurrentTool is not MeleeWeapon mw || mw.isScythe())
                return true;

            float stamAmt = WeaponTypeManager.GetWeaponType(mw.type.Value).BaseStaminaUsage;

            if (__instance.GetFightStamina() < stamAmt)
            {
                return false;
            }

            __instance.AddFightStamina(-stamAmt);
            FightStaminaEngine.regenTimer = 1f;

            return true;
        }
    }
}
