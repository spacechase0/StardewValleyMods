using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using static System.Net.Mime.MediaTypeNames;
using static SpaceCore.Skills;

namespace SpaceCore.Patches;
internal class SkillBuffPatcher : BasePatcher
{
    public override void Apply(Harmony harmony, IMonitor monitor)
    {
        harmony.Patch(
            original: this.RequireMethod<StardewValley.Object>(nameof(StardewValley.Object.GetFoodOrDrinkBuffs)),
            postfix: this.GetHarmonyMethod(nameof(After_Object_GetFoodOrDrinkBuffs))
        );
        harmony.Patch(
            original: this.RequireMethod<BuffsDisplay>(nameof(BuffsDisplay.getClickableComponents)),
            postfix: this.GetHarmonyMethod(nameof(After_BuffsDisplay_GetClickableComponents))
        );
        harmony.Patch(
            original: this.RequireMethod<IClickableMenu>(nameof(IClickableMenu.drawHoverText), new Type[] { typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string), typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int), typeof(float), typeof(CraftingRecipe), typeof(List<Item>), typeof(Texture2D), typeof(Rectangle?), typeof(Color?), typeof(Color?), typeof( float ), typeof( int ), typeof( int ) }),
            transpiler: this.GetHarmonyMethod(nameof(Transpile_IClickableMenu_DrawHoverText))
        );
    }


    private static IEnumerable<Buff> After_Object_GetFoodOrDrinkBuffs(IEnumerable<Buff> values, StardewValley.Object __instance)
    {
        // If there is no custom data, return normal buffs.
        if (!Game1.objectData.TryGetValue(__instance.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            foreach (Buff buff in values)
            {
                yield return buff;
            }
            yield break;
        }
        // If there is custom data, find the matching buff to wrap.
        foreach ( var buffData in data.Buffs )
        {
            if (buffData.CustomFields?.Any(b => b.Key.StartsWith("spacechase.SpaceCore.SkillBuff.") ||
                                                b.Key.StartsWith("spacechase0.SpaceCore.SkillBuff.") ||
                                                b.Key.StartsWith("spacechase0.SpaceCore/HealthRegeneration") ||
                                                b.Key.StartsWith("spacechase0.SpaceCore/StaminaRegeneration") ) ?? false)
            {
                Buff matchingBuff = null;
                string id = buffData.BuffId;
                if (string.IsNullOrWhiteSpace(id))
                {
                    id = data.IsDrink ? "drink" : "food";
                }
                foreach (Buff buff in values)
                {
                    matchingBuff = buff;
                }

                if (matchingBuff != null)
                {
                    yield return new Skills.SkillBuff(matchingBuff, id, buffData.CustomFields);
                } else
                {

                    float durationMultiplier = ((__instance.Quality != 0) ? 1.5f : 1f);
                    matchingBuff = new(
                        id: buffData.BuffId,
                        source: __instance.Name,
                        displaySource: __instance.DisplayName,
                        iconSheetIndex: buffData.IconSpriteIndex,
                        duration: (int)((float)buffData.Duration * durationMultiplier) * Game1.realMilliSecondsPerGameMinute
                    );
                    yield return new Skills.SkillBuff(matchingBuff, id, buffData.CustomFields);
                }
            }
        }
    }

    private static IEnumerable<ClickableTextureComponent> After_BuffsDisplay_GetClickableComponents(IEnumerable<ClickableTextureComponent> values, Buff buff)
    {
        foreach (ClickableTextureComponent value in values)
        {
            yield return value;
        }

        if (buff.iconTexture is not null)
        {
            yield break;
        }

        if (buff is not Skills.SkillBuff customBuff)
        {
            yield break;
        }

        foreach (var skillLevel in customBuff.SkillLevelIncreases)
        {
            Skills.Skill skill = Skills.GetSkill(skillLevel.Key);
            if (skill is null)
            {
                Log.Error($"Found no skill by name {skillLevel.Key}");
                continue;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("+");
            sb.Append(skillLevel.Value);
            sb.Append(" ");
            sb.Append(skill.GetName());
            sb.Append("\n");
            sb.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.508"));
            sb.Append(buff.displaySource ?? buff.source);

            yield return new ClickableTextureComponent("", Rectangle.Empty, null, sb.ToString(), skill.Icon, new Rectangle(0, 0, 16, 16), 4f);
        }
    }

    private static IEnumerable<CodeInstruction> Transpile_IClickableMenu_DrawHoverText(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codeInstructions = new List<CodeInstruction>(instructions);
        int step = 0;
        yield return codeInstructions[0];
        for (int i = 1; i < codeInstructions.Count; i++)
        {
            if (!codeInstructions[i - 1].Is(OpCodes.Ldarg_S, 8) || (!(codeInstructions[i].opcode == OpCodes.Brfalse_S) && !(codeInstructions[i].opcode == OpCodes.Brfalse)))
            {
                yield return codeInstructions[i];
                continue;
            }

            if (step == 0)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_S, 8);
                yield return new CodeInstruction(OpCodes.Ldarg_S, 9);
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(GetHeightAdjustment));
                yield return new CodeInstruction(OpCodes.Stloc_2);
            }
            else if (step == 1)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_S, 2);
                yield return new CodeInstruction(OpCodes.Ldarg_S, 9);
                yield return new CodeInstruction(OpCodes.Ldloc_1);
                yield return CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(GetWidthAdjustment));
                yield return new CodeInstruction(OpCodes.Stloc_1);
            }
            else if (step == 2)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_S, 0);
                yield return new CodeInstruction(OpCodes.Ldarg_S, 2);
                yield return new CodeInstruction(OpCodes.Ldarg_S, 9);
                yield return new CodeInstruction(OpCodes.Ldloc, 5);
                yield return new CodeInstruction(OpCodes.Ldloc, 6);
                yield return CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(DrawCustomSkillBuff));
                yield return new CodeInstruction(OpCodes.Stloc, 6);
            }

            yield return codeInstructions[i];
            step++;
        }
    }

    private static int GetHeightAdjustment(string[] buffIconsToDisplay, Item hoveredItem, int height)
    {
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return height;
        }

        bool addedAny = false;
        foreach (var buffData in data.Buffs)
        {
            if (buffData.CustomFields is null)
                continue;
            foreach (var entry in Skills.SkillBuff.ParseCustomFields(buffData.CustomFields))
            {
                addedAny = true;
                height += 34;
            }
            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/HealthRegeneration"))
            {
                addedAny = true;
                height += 34;
            }
            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/StaminaRegeneration"))
            {
                addedAny = true;
                height += 34;
            }
        }

        if (buffIconsToDisplay is null && addedAny)
        {
            height += 4;
        }

        return height;
    }

    private static int GetWidthAdjustment(SpriteFont font, Item hoveredItem, int width)
    {
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return width;
        }

        foreach ( var buffData in data.Buffs )
        {
            if (buffData.CustomFields is null)
                continue;
            foreach (var entry in Skills.SkillBuff.ParseCustomFields(buffData.CustomFields))
            {
                Skills.Skill skill = Skills.GetSkill(entry.Key);

                if (skill is null)
                {
                    continue;
                }

                width = Math.Max(width, (int)font.MeasureString("+99 " + skill.GetName()).X) + 92;
            }

            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/HealthRegeneration"))
            {
                width = Math.Max(width, (int)font.MeasureString("+999 " + I18n.HealthRegen()).X) + 92;
            }
            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/StaminaRegeneration"))
            {
                width = Math.Max(width, (int)font.MeasureString("+999 " + I18n.StaminaRegen()).X) + 92;
            }
        }

        return width;
    }

    private static int DrawCustomSkillBuff(SpriteBatch b, SpriteFont font, Item hoveredItem, int x, int y)
    {
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return y;
        }

        foreach (var buffData in data.Buffs)
        {
            if (buffData.CustomFields is null)
                continue;
            foreach (var entry in Skills.SkillBuff.ParseCustomFields(buffData.CustomFields))
            {
                Skills.Skill skill = Skills.GetSkill(entry.Key);

                if (skill is null)
                {
                    continue;
                }
                string text = $"+{entry.Value}  {skill.GetName()}";

                Utility.drawWithShadow(b, skill.SkillsPageIcon, new Vector2(x + 16 + 4, y + 16), new Rectangle(0, 0, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 16 + 34 + 4, y + 16), Game1.textColor);
                y += 34;
            }

            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/HealthRegeneration"))
            {
                float amt = float.Parse(buffData.CustomFields["spacechase0.SpaceCore/HealthRegeneration"]);
                string text = (amt >= 0 ? "+" : "") + amt + " " + I18n.HealthRegen();
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16), new Rectangle(0, 438, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 16 + 34 + 4, y + 16), Game1.textColor);
                y += 34;
            }
            if (buffData.CustomFields.ContainsKey("spacechase0.SpaceCore/StaminaRegeneration"))
            {
                float amt = float.Parse(buffData.CustomFields["spacechase0.SpaceCore/StaminaRegeneration"]);
                string text = (amt >= 0 ? "+" : "") + amt + " " + I18n.StaminaRegen();
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16), new Rectangle((amt < 0) ? 140 : 0, 428, 10, 10), Color.White, 0f, Vector2.Zero, 3f, flipped: false, 0.95f);
                Utility.drawTextWithShadow(b, text, font, new Vector2(x + 16 + 34 + 4, y + 16), Game1.textColor);
                y += 34;
            }
        }

        return y;
    }
}
