using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;

namespace MoonMisadventures.Patches
{
    [HarmonyPatch]
    public static class ItemRoomForPersistsTooltipPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Steamworks.NET") && !a.IsDynamic)
                             from type in asm.GetExportedTypes()
                             where type.IsSubclassOf(typeof(Item))
                             select type;

            yield return AccessTools.Method(typeof(Item), nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons));
            foreach (var subclass in subclasses)
            {
                if (subclass == typeof(Tool)) // this calls base.___()
                    continue;

                var meth = subclass.GetMethod(nameof(Item.getExtraSpaceNeededForTooltipSpecialIcons));
                if (meth != null && meth.DeclaringType == subclass)
                    yield return meth;
            }
        }

        public static void Postfix(Item __instance, SpriteFont font, int startingHeight, ref Point __result)
        {
            if (__result.Y == 0)
                __result.Y = startingHeight;

            if (__instance.modData.ContainsKey("persists"))
                __result.Y += 40;
        }
    }

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof( Texture2D),typeof(Rectangle?),typeof(Color?), typeof(Color?) })]
    public static class IClickableMenuDrawPersistsHoverTextPatch
    {
        public static void DrawPersistsMessage(Item item, SpriteBatch b, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            item.drawTooltip(b, ref x, ref y, font, alpha, overrideText);

            if (item.modData.ContainsKey("persists"))
            {
                y += 35;
                b.DrawString(font, I18n.Tooltip_Persists(), new Vector2(x + 15, y - 15), Game1.textColor);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insns, ILGenerator ilgen)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in insns)
            {
                if (insn.Calls(AccessTools.Method(typeof(Item), nameof(Item.drawTooltip))))
                {
                    var tmp = CodeInstruction.Call(typeof(IClickableMenuDrawPersistsHoverTextPatch), nameof(DrawPersistsMessage));
                    insn.opcode = tmp.opcode;
                    insn.operand = tmp.operand;
                }

                ret.Add(insn);
            }

            return ret;
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.canStackWith))]
    public static class ItemCanStackPersistsPatch
    {
        public static bool Prefix(Item __instance, ISalable other, ref bool __result)
        {
            if (__instance != null && other is Item otherItem)
            {
                bool eoverrideA = __instance.modData.ContainsKey( "persists" );
                bool eoverrideB = otherItem.modData.ContainsKey("persists");

                if (eoverrideA != eoverrideB)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( Item ), nameof( Item.CanBeLostOnDeath ) )]
    public static class ItemLostOnDeathPatch
    {
        public static void Postfix(Item __instance, ref bool __result)
        {
            if (__instance.modData.ContainsKey("persists"))
                __result = false;
        }
    }
}
