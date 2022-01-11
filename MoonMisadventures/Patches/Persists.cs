using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Steamworks.NET"))
                             from type in asm.GetTypes()
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

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(IList<Item>) })]
    public static class IClickableMenuDrawPersistsHoverTextPatch
    {
        public static void DrawPersistsMessage(Item item, SpriteBatch b, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
        {
            item.drawTooltip(b, ref x, ref y, font, alpha, overrideText);

            if (item.modData.ContainsKey("persists"))
            {
                y += 35;
                b.DrawString(font, Mod.instance.Helper.Translation.Get("tooltip.persists"), new Vector2(x + 15, y - 15), Game1.textColor);
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

    [HarmonyPatch(typeof(Event), nameof(Event.command_minedeath))]
    public static class EventNoMineDeathPersistLossPatch
    {
        private static void Impl(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            if (Game1.dialogueUp)
            {
                return;
            }
            Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
            int moneyToLose = r.Next(Game1.player.Money / 20, Game1.player.Money / 4);
            moneyToLose = Math.Min(moneyToLose, 5000);
            moneyToLose -= (int)((double)Game1.player.LuckLevel * 0.01 * (double)moneyToLose);
            moneyToLose -= moneyToLose % 100;
            int numberOfItemsLost = 0;
            double itemLossRate = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
            Game1.player.itemsLostLastDeath.Clear();
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                if (Game1.player.Items[i] != null && !Game1.player.Items[i].modData.ContainsKey( "persists" ) && (!(Game1.player.Items[i] is Tool) || (Game1.player.Items[i] is MeleeWeapon && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[i].canBeTrashed() && !(Game1.player.Items[i] is Ring) && r.NextDouble() < itemLossRate)
                {
                    Item item = Game1.player.Items[i];
                    Game1.player.Items[i] = null;
                    numberOfItemsLost++;
                    Game1.player.itemsLostLastDeath.Add(item);
                }
            }
            Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
            Game1.player.Money = Math.Max(0, Game1.player.Money - moneyToLose);
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1057") + " " + ((moneyToLose <= 0) ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1058", moneyToLose)) + ((numberOfItemsLost <= 0) ? ((moneyToLose <= 0) ? "" : ".") : ((moneyToLose <= 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1060") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1063") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))))));
            List<string> tmp = __instance.eventCommands.ToList();
            tmp.Insert(__instance.CurrentCommand + 1, "showItemsLost");
            __instance.eventCommands = tmp.ToArray();
        }

        public static bool Prefix( Event __instance, GameLocation location, GameTime time, string[] split )
        {
            Impl(__instance, location, time, split);
            return false;
        }
    }

    [HarmonyPatch(typeof(Event), nameof(Event.command_hospitaldeath))]
    public static class EventNoHospitalDeathPersistLossPatch
    {
        private static void Impl(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            if (Game1.dialogueUp)
            {
                return;
            }
            Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
            int numberOfItemsLost = 0;
            double itemLossRate = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
            Game1.player.itemsLostLastDeath.Clear();
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                if (Game1.player.Items[i] != null && !Game1.player.Items[i].modData.ContainsKey("persists") && (!(Game1.player.Items[i] is Tool) || (Game1.player.Items[i] is MeleeWeapon && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[i].canBeTrashed() && !(Game1.player.Items[i] is Ring) && r.NextDouble() < itemLossRate)
                {
                    Item item = Game1.player.Items[i];
                    Game1.player.Items[i] = null;
                    numberOfItemsLost++;
                    Game1.player.itemsLostLastDeath.Add(item);
                }
            }
            Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
            int moneyToLose = Math.Min(1000, Game1.player.Money);
            Game1.player.Money -= moneyToLose;
            Game1.drawObjectDialogue(((moneyToLose > 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1068", moneyToLose) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1070")) + ((numberOfItemsLost > 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1071") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : ""));
            List<string> tmp = __instance.eventCommands.ToList();
            tmp.Insert(__instance.CurrentCommand + 1, "showItemsLost");
            __instance.eventCommands = tmp.ToArray();
        }

        public static bool Prefix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            Impl(__instance, location, time, split);
            return false;
        }
    }
}
