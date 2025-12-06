using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib; // el diavolo nuevo
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
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
            if (SkillBuff.TryGetAdditionalBuffEffects(buffData.CustomFields, out var skills, out float health, out float stamina))
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

            StringBuilder sb = new();
            sb.Append(SkillBuff.FormattedBuffEffect(skillLevel.Value, skill.GetName()));
            sb.AppendLine();
            sb.Append(Game1.content.LoadString("Strings/StringsFromCSFiles:Buff.cs.508"));
            sb.Append(buff.displaySource ?? buff.source);

            yield return new ClickableTextureComponent("", Rectangle.Empty, null, sb.ToString(), skill.Icon, new Rectangle(0, 0, 16, 16), 4f);
        }
    }

    private static IEnumerable<CodeInstruction> Transpile_IClickableMenu_DrawHoverText(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> il)
    {
        var matcher = new CodeMatcher(il);

        matcher.Start();

        // Add to HoverText TextureBox height:

        // MATCH: if (buffIconsToDisplay != null)
        matcher.MatchEndForward(
            new(OpCodes.Ldarg_S, (byte)8),
            new(OpCodes.Brfalse_S));

        // INSERT: height += SkillBuffPatcher.GetHeightAdjustment(buffIconsToDisplay, hoveredItem, height)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_S, 8),
            new(OpCodes.Ldarg_S, 9),
            new(OpCodes.Ldloc_2),
            CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(SkillBuffPatcher.GetHeightAdjustment)),
            new(OpCodes.Stloc_2));

        // Check to set HoverText TextureBox minimum width:

        // MATCH: if (buffIconsToDisplay != null)
        matcher.MatchEndForward(
            new(OpCodes.Ldarg_S, (byte)8),
            new(OpCodes.Brfalse_S));

        // INSERT: width = SkillBuffPatcher.GetWidthAdjustment(font, hoveredItem, width)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_S, 2),
            new(OpCodes.Ldarg_S, 9),
            new(OpCodes.Ldloc_1),
            CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(GetWidthAdjustment)),
            new(OpCodes.Stloc_1));

        // Draw SkillBuff custom skill buff effects:

        // above the divider (health + stamina):

        // these are attributes more closely tied to health/stamina than
        // skills and combat attributes, so they're drawn separately

        // MATCH: if (buffIconsToDisplay != null)
        matcher.MatchEndForward(
            new(OpCodes.Ldarg_S, (byte)8),
            new(OpCodes.Brfalse)); // not _s

        // INSERT: y += SkillBuffPatcher.DrawAdditionalBuffEffects(b, font, hoveredItem, x, y)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_S, 0),
            new(OpCodes.Ldarg_S, 2),
            new(OpCodes.Ldarg_S, 9),
            new(OpCodes.Ldloc, 5),
            new(OpCodes.Ldloc, 6),
            CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(DrawAdditionalBuffEffects)),
            new(OpCodes.Stloc, 6));

        // optional divider ( | skills + attributes):

        // given the base game divider draw behaviour requires you to have included some
        // basic buff attributes, items with only SpaceCore custom skill buff effects
        // need to manually draw the divider and their buff effects outside of the usual branch

        // INSERT: y += SkillBuffPatcher.DrawCustomSkillBuffEffectsIfNoBasicEffects(b, font, hoveredItem, x, y, width, buffIconsToDisplay, craftingIngredients)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_S, 0),
            new(OpCodes.Ldarg_S, 2),
            new(OpCodes.Ldarg_S, 9),
            new(OpCodes.Ldloc, 5),
            new(OpCodes.Ldloc, 6),
            new(OpCodes.Ldloc, 1),
            new(OpCodes.Ldarg_S, 8),
            new(OpCodes.Ldarg_S, 16),
            CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(DrawCustomSkillBuffEffectsIfNoBasicEffects)),
            new(OpCodes.Stloc, 6));

        // below the divider (skills + attributes):

        // these are drawn above basic skills, as it might look odd having them
        // below basic non-skill attributes, such as attack, defence, magnetism, ...

        // MATCH: b.Draw(Game1.staminaRect, new Rectangle(...), new Color(...));
        matcher.MatchEndForward(new CodeMatch(op => op.Is(OpCodes.Callvirt, AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw), [typeof(Texture2D), typeof(Rectangle), typeof(Color)]))));

        // INSERT: y += SkillBuffPatcher.DrawCustomSkillBuffEffects(b, font, hoveredItem, x, y)
        matcher.InsertAndAdvance(
            new(OpCodes.Ldarg_S, 0),
            new(OpCodes.Ldarg_S, 2),
            new(OpCodes.Ldarg_S, 9),
            new(OpCodes.Ldloc, 5),
            new(OpCodes.Ldloc, 6),
            CodeInstruction.Call(typeof(SkillBuffPatcher), nameof(DrawCustomSkillBuffEffects)),
            new(OpCodes.Stloc, 6));

        if (matcher.IsInvalid)
        {
            Log.Error($"Failed to apply {nameof(SkillBuffPatcher)} {nameof(Transpile_IClickableMenu_DrawHoverText)}. Custom buff effects will not be listed on items.");
            return il;
        }

        return matcher.InstructionEnumeration();
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
            if (SkillBuff.TryGetAdditionalBuffEffects(buffData.CustomFields, out var skills, out float health, out float stamina))
            {
                addedAny = true;
                foreach (var entry in skills)
                {
                    Skills.Skill skill = Skills.GetSkill(entry.Key);
                    if (skill is null)
                        continue;

                    height += 34 + 5;
                }
                if (health != 0)
                {
                    height += 34;
                }
                if (stamina != 0)
                {
                    height += 34;
                }
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

        foreach (var buffData in data.Buffs)
        {
            if (SkillBuff.TryGetAdditionalBuffEffects(buffData.CustomFields, out var skills, out float health, out float stamina))
            {
                foreach (var entry in skills)
                {
                    Skills.Skill skill = Skills.GetSkill(entry.Key);
                    if (skill is null)
                        continue;

                    width = Math.Max(width, (int)font.MeasureString("+99 " + skill.GetName()).X + 92 );
                }
                if (health != 0)
                {
                    width = Math.Max(width, (int)font.MeasureString("+999 " + I18n.HealthRegen()).X + 92);
                }
                if (stamina != 0)
                {
                    width = Math.Max(width, (int)font.MeasureString("+999 " + I18n.StaminaRegen()).X + 92);
                }
            }
        }

        return width;
    }

    /// <summary>
    /// For items with basic buff attributes, draws custom skill buff effects, or does nothing if no custom skill effects are found.
    /// </summary>
    private static int DrawCustomSkillBuffEffects(SpriteBatch b, SpriteFont font, Item hoveredItem, int x, int y)
    {
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return y;
        }

        Vector2 offset = new Vector2(16 + 4, 16);
        Point spacing = new Point(34, 34 + 5);

        foreach (var buffData in data.Buffs)
        {
            if (SkillBuff.TryGetAdditionalBuffEffects(buffData.CustomFields, out var skills, out float health, out float stamina))
            {
                foreach (var entry in skills)
                {
                    Skills.Skill skill = Skills.GetSkill(entry.Key);
                    if (skill is null)
                        continue;

                    SkillBuff.DrawBuffEffect(b, new Vector2(x, y) + offset, entry.Value, skill.GetName(), font: font, icon: skill.SkillsPageIcon, spacing: spacing.X);
                    y += spacing.Y;
                }
            }
        }

        return y;
    }

    /// <summary>
    /// For all items, draws any SpaceCore additional buff effects, or does nothing if no additional effects are found.
    /// </summary>
    private static int DrawAdditionalBuffEffects(SpriteBatch b, SpriteFont font, Item hoveredItem, int x, int y)
    {
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return y;
        }

        Vector2 offset = new Vector2(16 + 4, 16);
        Point spacing = new Point(34, 34);

        foreach (var buffData in data.Buffs)
        {
            if (SkillBuff.TryGetAdditionalBuffEffects(buffData.CustomFields, out var skills, out float health, out float stamina))
            {
                if (stamina != 0)
                {
                    SkillBuff.DrawStaminaRegenBuffEffect(b, new Vector2(x, y) + offset, stamina, font: font, spacing: spacing.X);
                    y += spacing.Y;
                }
                if (health != 0)
                {
                    SkillBuff.DrawHealthRegenBuffEffect(b, new Vector2(x, y) + offset, health, font: font, spacing: spacing.X);
                    y += spacing.Y;
                }
            }
        }

        return y;
    }

    /// <summary>
    /// For hovered items without basic buff attributes, draws a divider and custom skill buff effects, or does nothing if no custom skill effects are found..
    /// </summary>
    private static int DrawCustomSkillBuffEffectsIfNoBasicEffects(SpriteBatch b, SpriteFont font, Item hoveredItem, int x, int y, int width, string[] buffIconsToDisplay, CraftingRecipe craftingIngredients)
    {
        // duplicate spacecore code
        if (hoveredItem is null ||
            !Game1.objectData.TryGetValue(hoveredItem.ItemId, out ObjectData data) ||
            data.Buffs is null ||
            data.Buffs.All(b => b.CustomFields is null || b.CustomFields.Count == 0))
        {
            return y;
        }

        // handle alternate branch in base game code without transpiling labels
        if (buffIconsToDisplay is null)
        {
            // duplicate base game code
            y += 16;
            b.Draw(Game1.staminaRect, new Rectangle(x + 12, y + 6, width - ((craftingIngredients is not null) ? 4 : 24), 2), new Color(207, 147, 103) * 0.8f);

            // duplicate spacecore code
            y += SkillBuffPatcher.DrawCustomSkillBuffEffects(b, font, hoveredItem, x, y);
        }

        // this is a miserable method
        return y;
    }
}
