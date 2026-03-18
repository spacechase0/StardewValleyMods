using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Stardew3D.Handlers;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;

namespace Stardew3D.Patches;
internal class Menus
{
    [HarmonyPatch]
    public static class MaybeBypassMenuUpdatePatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return
            [
                AccessTools.Method( typeof( Game1 ), "_update" ),
                AccessTools.Method( typeof( Game1 ), nameof(Game1.updateActiveMenu) ),
                AccessTools.Method( typeof( Game1 ), "DrawOverlays" ),
                AccessTools.Method( typeof( Game1 ), "drawHUD" ),
                AccessTools.Method( typeof( Game1 ), nameof(Game1.updateTextEntry) ),
                AccessTools.Method( typeof( GameMenu ), nameof(GameMenu.update) ),
                AccessTools.Method( typeof( ItemGrabMenu ), nameof(ItemGrabMenu.update) ),
                AccessTools.Method( typeof( ShippingMenu ), nameof(ShippingMenu.update) ),
                AccessTools.Method( typeof( TitleMenu ), nameof(TitleMenu.update) ),
                AccessTools.Method( typeof( CollectionsPage ), nameof(CollectionsPage.update) ),
                AccessTools.Method( typeof( GrandpaStory ), nameof(GrandpaStory.tick) ),
                AccessTools.Method( typeof( FantasyBoardGame ), nameof(FantasyBoardGame.tick) ),
            ];
        }

        public static void HandleMenuUpdate(IClickableMenu menu, GameTime time)
        {
            bool didUpdateOnce = false;
            void forceMenuUpdateIfNotAlreadyRun(IUpdateHandler.UpdateContext ctx)
            {
                if (didUpdateOnce)
                    return;
                menu.update(ctx.Time);
                didUpdateOnce = true;
            }

            var currentMenuHandlers = Mod.State.GetUpdateHandlersFor(Game1.activeClickableMenu);
            foreach (var handler in currentMenuHandlers)
            {
                handler?.Update(new()
                {
                    Time = time,
                    ForceUpdateIfNotAlreadyRun = forceMenuUpdateIfNotAlreadyRun,
                });
            }

            if (currentMenuHandlers[0] == null)
            {
                forceMenuUpdateIfNotAlreadyRun(new()
                {
                    Time = time,
                    ForceUpdateIfNotAlreadyRun = forceMenuUpdateIfNotAlreadyRun,
                });
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originl )
        {
            // We can't use MethodReplacer because it replaces callvirt *and* call (the latter being used for base. calls)
            var ret = instructions.Manipulator(insn => insn.opcode == OpCodes.Callvirt && insn.operand is MethodInfo origMeth &&
                                               origMeth.DeclaringType == typeof( IClickableMenu ) && origMeth.Name == nameof( IClickableMenu.update ),
                                               insn => { insn.opcode = OpCodes.Call; insn.operand = AccessTools.Method(typeof(MaybeBypassMenuUpdatePatch), nameof(HandleMenuUpdate)); } );
            return ret;
        }
    }
}
