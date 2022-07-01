using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

namespace SpaceCore.Patches
{
    internal class PortableCarpenterPatcher : BasePatcher
    {
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.performHoverAction)),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.receiveLeftClick)),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.tryToBuild)),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.update)),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>("resetBounds"),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.CanDemolishThis), new Type[] { typeof( BluePrint ) } ),
                transpiler: this.GetHarmonyMethod(nameof(TranspileFarmToBGL))
            );
            harmony.Patch(
                original: this.RequireMethod<CarpenterMenu>(nameof(CarpenterMenu.returnToCarpentryMenuAfterSuccessfulBuild)),
                postfix: this.GetHarmonyMethod(nameof(DontReturnAfterBuild))
            );
            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.getLocationFromName), new Type[] { typeof(string), typeof(bool) }),
                prefix: this.GetHarmonyMethod(nameof(GetLocationFromName_ReturnCurrentForCarpenterMenu))
            );

            harmony.Patch(
                original: this.RequireMethod<Game1>(nameof(Game1.findStructure)),
                postfix: this.GetHarmonyMethod(nameof(Postfix_findStructure))
            );

            foreach (var meth in typeof(CarpenterMenu).GetMethods( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static ) )
            {
                if (meth.DeclaringType != typeof(CarpenterMenu))
                    continue;

                harmony.Patch(
                    original: meth,
                    prefix:  this.GetHarmonyMethod( nameof( CarpenterPrefix ), -100 ), // TODO: Are these priorities backwards?
                    postfix: this.GetHarmonyMethod( nameof( CarpenterPostfix ), 100 )
                );
            }
        }

        private static void CarpenterPrefix( MethodBase __originalMethod, ref object __state )
        {
            __state = shouldRedirectGetLocationFarm;
            ++shouldRedirectGetLocationFarm;

            // This should handle returnToCarpentryMenu, returnToCarpentryMenuAfterSuccessfulBuild,
            // and their lambdas
            if (__originalMethod.Name != null && (__originalMethod.Name.Contains("returnToCarpentryMenu") || __originalMethod.Name == nameof(CarpenterMenu.robinConstructionMessage)))
                shouldRedirectGetLocationFarm = 0;
        }

        private static void CarpenterPostfix( object __state )
        {
            shouldRedirectGetLocationFarm = (int)__state;
        }

        private static IEnumerable<CodeInstruction> TranspileFarmToBGL(IEnumerable<CodeInstruction> insns, MethodBase method)
        {
            List<CodeInstruction> ret = new();

            int counter = 0;
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Castclass && (insn.operand as Type) == typeof(Farm))
                    insn.operand = typeof(BuildableGameLocation);
                else if (insn.opcode == OpCodes.Call && (insn.operand as MethodBase).Name == "getFarm")
                {
                    if (method.Name != "performHoverAction" && counter != 2)
                    {
                        ret.Add(new CodeInstruction(OpCodes.Ldstr, "Farm"));
                        insn.operand = AccessTools.Method("StardewValley.Game1:getLocationFromName", new Type[] { typeof(string) });
                    }
                    ++counter;
                }
                ret.Add(insn);
            }

            return ret;
        }

        private static int shouldRedirectGetLocationFarm = 0;
        private static bool GetLocationFromName_ReturnCurrentForCarpenterMenu(string name, ref GameLocation __result)
        {
            if (shouldRedirectGetLocationFarm > 0 && name == "Farm" && Game1.currentLocation is BuildableGameLocation )
            {
                __result = Game1.currentLocation;
                return false;
            }
            return true;
        }
        private static void DontReturnAfterBuild()
        {
            if (Game1.currentLocation.Name != "Farm")
            {
                Game1.locationRequest.Location = Game1.currentLocation;
                Game1.locationRequest.Name = Game1.currentLocation.NameOrUniqueName;
                Game1.xLocationAfterWarp = Game1.player.getTileX();
                Game1.yLocationAfterWarp = Game1.player.getTileY();
            }
        }

        // This fixes "structures in BGLs in structures"'s interiors not being found
        public static void Postfix_findStructure(GameLocation parentLocation, string name, ref GameLocation __result)
        {
            if (__result == null && parentLocation is BuildableGameLocation bgl)
            {
                foreach (Building building in (parentLocation as BuildableGameLocation).buildings)
                {
                    if (building.indoors.Value != null && building.indoors.Value is BuildableGameLocation bgl2)
                    {
                        var found = Game1.findStructure(bgl2, name);
                        if (found != null)
                        {
                            __result = found;
                            return;
                        }
                    }
                }
            }
        }
    }
}
