using System;
using System.Collections.Generic;
using System.Linq;
using CookingSkill.Framework;
using CookingSkill.Patches;
using Spacechase.Shared.Patching;
using SpaceCore;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace CookingSkill
{
    // This really needs organizing/splitting
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static Skill Skill;

        public static double GetEdibilityMultiplier()
        {
            return 1 + Game1.player.GetCustomSkillLevel(Mod.Skill) * 0.03;
        }

        public static double GetNoConsumeChance()
        {
            return Game1.player.HasCustomProfession(Skill.ProfessionConservation)
                ? 0.15
                : 0;
        }

        // Modifies the item based on professions and stuff
        // Returns for whether or not we should consume the ingredients
        public static bool OnCook(CraftingRecipe recipe, Item item, List<Chest> additionalItems)
        {
            if (recipe.isCookingRecipe && item is SObject obj)
            {
                if (!Game1.player.recipesCooked.TryGetValue(obj.ParentSheetIndex, out int timesCooked))
                    timesCooked = 0;

                Random rand = new Random((int)(Game1.stats.daysPlayed + Game1.uniqueIDForThisGame + (uint)obj.ParentSheetIndex + (uint)timesCooked));

                obj.Edibility = (int)(obj.Edibility * Mod.GetEdibilityMultiplier());

                if (Game1.player.HasCustomProfession(Skill.ProfessionSellPrice))
                    obj.Price = (int)(obj.Price * 1.2);

                if (Game1.player.HasCustomProfession(Skill.ProfessionSilver))
                    obj.Quality = SObject.medQuality;

                ConsumedItem[] used;
                try
                {
                    CraftingRecipePatcher.ShouldConsumeItems = false;
                    recipe.consumeIngredients(additionalItems);
                    used = CraftingRecipePatcher.LastUsedItems.ToArray();
                }
                finally
                {
                    CraftingRecipePatcher.ShouldConsumeItems = true;
                }

                int total = 0;
                foreach (ConsumedItem ingredient in used)
                    total += ingredient.Amount;

                for (int quality = 1; quality <= SObject.bestQuality; ++quality)
                {
                    if (quality == 3)
                        continue; // not a real quality

                    double chance = 0;
                    foreach (ConsumedItem ingredient in used)
                    {
                        if (ingredient.Item.Quality >= quality)
                            chance += (1.0 / total) * ingredient.Amount;
                    }

                    if (rand.NextDouble() < chance)
                        obj.Quality = quality;
                }

                return rand.NextDouble() >= Mod.GetNoConsumeChance();
            }

            return true;
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            SpaceEvents.OnItemEaten += this.OnItemEaten;

            Skills.RegisterSkill(Mod.Skill = new Skill());

            HarmonyPatcher.Apply(this,
                new CraftingPagePatcher(),
                new CraftingRecipePatcher(),
                new ObjectPatcher()
            );
        }

        public override object GetApi()
        {
            return new Api();
        }

        private Buff LastDrink;

        private void OnItemEaten(object sender, EventArgs e)
        {
            // get object eaten
            if (Game1.player.itemToEat is not SObject { Category: SObject.CookingCategory } obj || !Game1.objectInformation.TryGetValue(obj.ParentSheetIndex, out string rawObjData))
                return;
            string[] objFields = rawObjData.Split('/');
            bool isDrink = objFields.GetOrDefault(SObject.objectInfoMiscIndex) == "drink";

            // get buff data
            Buff oldBuff = isDrink ? Game1.buffsDisplay.drink : Game1.buffsDisplay.food;
            Buff curBuff = this.CreateBuffFromObject(obj, objFields);
            if (oldBuff != null && curBuff != null && oldBuff.buffAttributes.SequenceEqual(curBuff.buffAttributes) && oldBuff != this.LastDrink)
            {
                // Now that we know that this is the original buff, we can buff the buff.
                Log.Trace("Buffing buff");
                int[] newAttr = (int[])curBuff.buffAttributes.Clone();
                if (Game1.player.HasCustomProfession(Skill.ProfessionBuffLevel))
                {
                    for (int id = 0; id < newAttr.Length; ++id)
                    {
                        if (newAttr[id] <= 0)
                            continue;

                        if (id is Buff.maxStamina or Buff.magneticRadius)
                            newAttr[id] = (int)(newAttr[id] * 1.2);
                        else
                            newAttr[id]++;
                    }
                }

                float minutesDuration = curBuff.millisecondsDuration / 7000f * 10f;
                if (Game1.player.HasCustomProfession(Skill.ProfessionBuffTime))
                    minutesDuration *= 1.25f;

                Buff newBuff = this.CreateBuff(newAttr, (int)minutesDuration, objFields);
                this.ReplaceBuff(newBuff, isDrink);
            }
            else if (curBuff == null && Game1.player.HasCustomProfession(Skill.ProfessionBuffPlain))
            {
                Log.Trace("Buffing plain");
                Random rand = new();
                int[] newAttr = new int[12];
                int count = 1 + Math.Min(obj.Edibility / 30, 3);
                for (int i = 0; i < count; ++i)
                {
                    int attr = rand.Next(10);
                    if (attr >= 3)
                        ++attr; // 3 unused?
                    if (attr >= Buff.speed)
                        ++attr; // unused?

                    int amt = 1;
                    if (attr is Buff.maxStamina or Buff.magneticRadius)
                        amt = 25 + rand.Next(4) * 5;
                    else
                    {
                        // 36% (assuming I used this probability calculator right) chance for a buff to be level 2
                        // 4% chance for it to be 3
                        if (rand.NextDouble() < 0.2)
                            ++amt;
                        if (rand.NextDouble() < 0.2)
                            ++amt;
                    }
                    newAttr[attr] += amt;
                }

                int newTime = 120 + obj.Edibility / 10 * 30;

                Buff newBuff = this.CreateBuff(newAttr, newTime, objFields);
                this.ReplaceBuff(newBuff, isDrink);
            }
        }

        /// <summary>Create a buff instance.</summary>
        /// <param name="attr">The buff attributes.</param>
        /// <param name="minutesDuration">The buff duration in minutes.</param>
        /// <param name="objectFields">The raw object fields from <see cref="Game1.objectInformation"/>.</param>
        private Buff CreateBuff(int[] attr, int minutesDuration, string[] objectFields)
        {
            return new(
                farming: attr.GetOrDefault(Buff.farming),
                fishing: attr.GetOrDefault(Buff.fishing),
                mining: attr.GetOrDefault(Buff.mining),
                digging: attr.GetOrDefault(3),
                luck: attr.GetOrDefault(Buff.luck),
                foraging: attr.GetOrDefault(Buff.foraging),
                crafting: attr.GetOrDefault(Buff.crafting),
                maxStamina: attr.GetOrDefault(Buff.maxStamina),
                magneticRadius: attr.GetOrDefault(Buff.magneticRadius),
                speed: attr.GetOrDefault(Buff.speed),
                defense: attr.GetOrDefault(Buff.defense),
                attack: attr.GetOrDefault(Buff.attack),
                minutesDuration: minutesDuration,
                source: objectFields.GetOrDefault(SObject.objectInfoNameIndex),
                displaySource: objectFields.GetOrDefault(SObject.objectInfoDisplayNameIndex)
            );
        }

        /// <summary>Create a buff instance for an object, if valid.</summary>
        /// <param name="obj">The object instance.</param>
        /// <param name="fields">The raw object fields from <see cref="Game1.objectInformation"/>.</param>
        private Buff CreateBuffFromObject(SObject obj, string[] fields)
        {
            // get object info
            int edibility = Convert.ToInt32(fields.GetOrDefault(SObject.objectInfoEdibilityIndex));
            string name = fields.GetOrDefault(SObject.objectInfoNameIndex);
            string displayName = fields.GetOrDefault(SObject.objectInfoDisplayNameIndex);

            // ignore if item doesn't provide a buff
            if (edibility < 0 || fields.Length <= SObject.objectInfoBuffTypesIndex)
                return null;

            // get buff duration
            if (!fields.TryGetIndex(SObject.objectInfoBuffDurationIndex, out string rawDuration) || !int.TryParse(rawDuration, out int minutesDuration))
                minutesDuration = 0;

            // get buff fields
            string[] attr = fields[SObject.objectInfoBuffTypesIndex].Split(' ');
            obj.ModifyItemBuffs(attr);
            if (attr.All(val => val == "0"))
                return null;

            // parse buff
            int GetAttr(int index) => attr.TryGetIndex(index, out string raw) && int.TryParse(raw, out int value) ? value : 0;
            return new Buff(
                farming: GetAttr(Buff.farming),
                fishing: GetAttr(Buff.fishing),
                mining: GetAttr(Buff.mining),
                digging: GetAttr(3),
                luck: GetAttr(Buff.luck),
                foraging: GetAttr(Buff.foraging),
                crafting: GetAttr(Buff.crafting),
                maxStamina: GetAttr(Buff.maxStamina),
                magneticRadius: GetAttr(Buff.magneticRadius),
                speed: GetAttr(Buff.speed),
                defense: GetAttr(Buff.defense),
                attack: GetAttr(Buff.attack),
                minutesDuration: minutesDuration,
                source: name,
                displaySource: displayName
            );
        }

        /// <summary>Replace the current food or drink buff.</summary>
        /// <param name="newBuff">The new buff to set.</param>
        /// <param name="isDrink">Whether the buff is a drink buff; else it's a food buff.</param>
        private void ReplaceBuff(Buff newBuff, bool isDrink)
        {
            if (isDrink)
            {
                Game1.buffsDisplay.drink?.removeBuff();
                Game1.buffsDisplay.drink = newBuff;
                Game1.buffsDisplay.drink.addBuff();
                this.LastDrink = newBuff;
            }
            else
            {
                Game1.buffsDisplay.food?.removeBuff();
                Game1.buffsDisplay.food = newBuff;
                Game1.buffsDisplay.food.addBuff();
            }

            Game1.buffsDisplay.syncIcons();
        }
    }
}
