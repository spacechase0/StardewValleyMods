using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore;
using SObject = StardewValley.Object;

namespace CookingSkill
{
    // This really needs organizing/splitting
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static Skill skill;

        public static double getEdibilityMultiplier()
        {
            return 1 + Game1.player.GetCustomSkillLevel( skill ) * 0.03;
        }

        public static double getNoConsumeChance()
        {
            if (Game1.player.HasCustomProfession(Skill.ProfessionConservation))
                return 0.15;
            else
                return 0;

        }

        // Modifies the item based on professions and stuff
        // Returns for whether or not we should consume the ingredients
        public static bool onCook( CraftingRecipe recipe, Item item )
        {
            if (recipe.isCookingRecipe && item is SObject obj)
            {
                int amtCrafted = 0;
                if (Game1.player.recipesCooked.ContainsKey(obj.ParentSheetIndex))
                {
                    amtCrafted = Game1.player.recipesCooked[obj.ParentSheetIndex];
                }
                Random rand = new Random((int)(Game1.stats.daysPlayed + Game1.uniqueIDForThisGame + (uint)obj.ParentSheetIndex + (uint)amtCrafted));

                obj.Edibility = (int)(obj.Edibility * getEdibilityMultiplier());

                if (Game1.player.HasCustomProfession(Skill.ProfessionSellPrice))
                {
                    obj.Price = (int)(obj.Price * 1.2);
                }

                if (Game1.player.HasCustomProfession(Skill.ProfessionSilver))
                {
                    obj.Quality = 1;
                }

                var used = new List<NewCraftingPage.ConsumedItem>();
                NewCraftingPage.myConsumeIngredients(recipe, false, used);

                int total = 0;
                foreach (NewCraftingPage.ConsumedItem ingr in used )
                    total += ingr.amt;

                for (int iq = 1; iq <= SObject.bestQuality; ++iq)
                {
                    if (iq == 3) continue; // Not a real quality

                    double chance = 0;
                    foreach (NewCraftingPage.ConsumedItem ingr in used )
                    {
                        if (ingr.item.Quality >= iq)
                            chance += (1.0 / total) * ingr.amt;
                    }

                    if (rand.NextDouble() < chance)
                        obj.Quality = iq;
                }

                return rand.NextDouble() >= getNoConsumeChance();
            }

            return true;
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry( IModHelper helper )
        {
            instance = this;

            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;

            Skills.RegisterSkill(skill = new Skill());
        }

        private bool wasEating = false;
        private int prevToEatStack = -1;
        private Buff lastDrink = null;

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.player.isEating != wasEating)
            {
                if ( !Game1.player.isEating )
                {
                    // Apparently this happens when the ask to eat dialog opens, but they pressed no.
                    // So make sure something was actually consumed.
                    if ( prevToEatStack != -1 && ( prevToEatStack - 1 == Game1.player.itemToEat.Stack ) )
                    {
                        SObject obj = Game1.player.itemToEat as SObject;
				        string[] info = Game1.objectInformation[obj.ParentSheetIndex].Split( '/' );
                        string[] buffData = ((Convert.ToInt32(info[2]) > 0 && info.Count() > 7) ? info[7].Split(' ') : null);

                        if (buffData != null)
                        {
                            bool allZero = true;
                            foreach (string bd in buffData)
                            {
                                allZero = (allZero && bd == "0");
                            }
                            if (allZero) buffData = null;
                        }

                        if (info[3] == "Cooking -7")
                        {
                            // Need to make sure this is the original buff first.
                            // So it doesn't get rebuffed from eating a buff food -> non buff food -> buff food or something
                            Buff oldBuff = (info[6] == "drink" ? Game1.buffsDisplay.drink : Game1.buffsDisplay.food);
                            Buff thisBuff = null;
                            if (info[6] == "drink")
                                thisBuff = buffData == null ? null : new Buff(Convert.ToInt32(buffData[0]), Convert.ToInt32(buffData[1]), Convert.ToInt32(buffData[2]), Convert.ToInt32(buffData[3]), Convert.ToInt32(buffData[4]), Convert.ToInt32(buffData[5]), Convert.ToInt32(buffData[6]), Convert.ToInt32(buffData[7]), Convert.ToInt32(buffData[8]), Convert.ToInt32(buffData[9]), Convert.ToInt32(buffData[10]), (buffData.Length > 10) ? Convert.ToInt32(buffData[10]) : 0, (info.Count<string>() > 8) ? Convert.ToInt32(info[8]) : -1, info[0], info[4]);
                            else 
                                thisBuff = buffData == null ? null : new Buff(Convert.ToInt32(buffData[0]), Convert.ToInt32(buffData[1]), Convert.ToInt32(buffData[2]), Convert.ToInt32(buffData[3]), Convert.ToInt32(buffData[4]), Convert.ToInt32(buffData[5]), Convert.ToInt32(buffData[6]), Convert.ToInt32(buffData[7]), Convert.ToInt32(buffData[8]), Convert.ToInt32(buffData[9]), Convert.ToInt32(buffData[10]), (buffData.Length > 11) ? Convert.ToInt32(buffData[11]) : 0, (info.Count<string>() > 8) ? Convert.ToInt32(info[8]) : -1, info[0], info[4]);
                            int[] oldAttr = (oldBuff == null ? null : ((int[])Util.GetInstanceField(typeof(Buff), oldBuff, "buffAttributes")));
                            int[] thisAttr = (thisBuff == null ? null : ((int[])Util.GetInstanceField(typeof(Buff), thisBuff, "buffAttributes")));
                            Log.trace("Ate something: " + obj + " " + Game1.objectInformation[obj.ParentSheetIndex] + " " + buffData + " " + oldBuff + " " + thisBuff + " " + oldAttr + " " + thisAttr);
                            if (oldBuff != null && thisBuff != null && Enumerable.SequenceEqual(oldAttr, thisAttr) &&
                                 ((info[6] == "drink" && oldBuff != lastDrink) || (info[6] != "drink" && oldBuff != lastDrink)))
                            {
                                // Now that we know that this is the original buff, we can buff the buff.
                                Log.trace("Buffing buff");
                                int[] newAttr = (int[])thisAttr.Clone();
                                if (Game1.player.HasCustomProfession(Skill.ProfessionBuffLevel))
                                {
                                    for (int i = 0; i < thisAttr.Length; ++i)
                                    {
                                        if (newAttr[i] <= 0)
                                            continue;

                                        if (i == 7 || i == 8)
                                            newAttr[i] = (int)(newAttr[i] * 1.2);
                                        else
                                            newAttr[i]++;
                                    }
                                }

                                int newTime = (info.Count<string>() > 8) ? Convert.ToInt32(info[8]) : -1;
                                if (newTime != -1 && Game1.player.HasCustomProfession(Skill.ProfessionBuffTime))
                                {
                                    newTime = (int)(newTime * 1.25);
                                }

                                Buff newBuff = new Buff(newAttr[0], newAttr[1], newAttr[2], newAttr[3], newAttr[4], newAttr[5], newAttr[6], newAttr[7], newAttr[8], newAttr[9], newAttr[10], newAttr[11], newTime, info[0], info[4]);
                                newBuff.millisecondsDuration = newTime / 10 * 7000;
                                // ^ The vanilla code decreases the duration based on the time of day.
                                // This is fine normally, since it ends as the day ends.
                                // However if you have something like TimeSpeed it just means it won't
                                // last as long later if eaten later in the day.

                                if (info[6] == "drink")
                                {
                                    Game1.buffsDisplay.drink.removeBuff();
                                    Game1.buffsDisplay.drink = newBuff;
                                    Game1.buffsDisplay.drink.addBuff();
                                    lastDrink = newBuff;
                                }
                                else
                                {
                                    Game1.buffsDisplay.food.removeBuff();
                                    Game1.buffsDisplay.food = newBuff;
                                    Game1.buffsDisplay.food.addBuff();
                                }
                                Game1.buffsDisplay.syncIcons();
                            }
                            else if (thisBuff == null && Game1.player.HasCustomProfession(Skill.ProfessionBuffPlain))
                            {
                                Log.trace("Buffing plain");
                                Random rand = new Random();
                                int[] newAttr = new int[12];
                                int count = 1 + Math.Min(obj.Edibility / 30, 3);
                                for (int i = 0; i < count; ++i)
                                {
                                    int attr = rand.Next(10);
                                    if (attr >= 3) ++attr; // 3 unused?
                                    if (attr >= 6) ++attr; // 6 is crafting speed, unused?

                                    int amt = 1;
                                    if (attr == 7 || attr == 8)
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

                                Buff newBuff = new Buff(newAttr[0], newAttr[1], newAttr[2], newAttr[3], newAttr[4], newAttr[5], newAttr[6], newAttr[7], newAttr[8], newAttr[9], newAttr[10], newAttr[11], newTime, info[0], info[4]);
                                newBuff.millisecondsDuration = newTime / 10 * 7000;

                                if (info[6] == "drink")
                                {
                                    if (Game1.buffsDisplay.drink != null)
                                        Game1.buffsDisplay.drink.removeBuff();
                                    Game1.buffsDisplay.drink = newBuff;
                                    Game1.buffsDisplay.drink.addBuff();
                                    lastDrink = newBuff;
                                }
                                else
                                {
                                    if (Game1.buffsDisplay.drink != null)
                                        Game1.buffsDisplay.drink.removeBuff();
                                    Game1.buffsDisplay.drink = newBuff;
                                    Game1.buffsDisplay.drink.addBuff();
                                }
                                Game1.buffsDisplay.syncIcons();
                            }
                        }
                    }
                }
                Log.trace("Eating:" + Game1.player.isEating);
                Log.trace("prev:" + prevToEatStack);
                Log.trace("I:"+Game1.player.itemToEat + " " + ((Game1.player.itemToEat != null) ? Game1.player.itemToEat.getStack() : -1));
                Log.trace("A:" + Game1.player.ActiveObject + " " + ((Game1.player.ActiveObject != null) ? Game1.player.ActiveObject.getStack() : -1));
                prevToEatStack = (Game1.player.itemToEat != null ? Game1.player.itemToEat.Stack : -1);
            }
            wasEating = Game1.player.isEating;

            if (Game1.activeClickableMenu != null)
            {
                if ( Game1.activeClickableMenu is CraftingPage )
                {
                    CraftingPage menu = Game1.activeClickableMenu as CraftingPage;
                    bool cooking = ( bool ) Util.GetInstanceField( typeof( CraftingPage), Game1.activeClickableMenu, "cooking" );
                    Game1.activeClickableMenu = new NewCraftingPage( menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height, cooking );
                }
            }
        }

        public override object GetApi()
        {
            return new CookingSkillAPI();
        }
    }
}
