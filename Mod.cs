using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Events;
using StardewValley.Quests;
using System.IO;
using SpaceCore.Events;

namespace CookingSkill
{
    // This really needs organizing/splitting
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public static readonly int[] expNeededForLevel = new int[] { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15000 };
        public static List<int> newCookingLevels = new List<int>();
        public static int getCookingLevel()
        {
            fixExpLength();

            for ( int i = expNeededForLevel.Length - 1; i >= 0; --i )
            {
                if ( Game1.player.experiencePoints[ 6 ] >= expNeededForLevel[ i ] )
                {
                    return i + 1;
                }
            }

            return 0;
        }

        public static void addCookingExp( int amt )
        {
            if (amt <= 0)
                return;
            fixExpLength();

            int oldLevel = getCookingLevel();
            Log.trace("Adding " + amt+ "experience to cooking, from " + Game1.player.experiencePoints[6]);
            Game1.player.experiencePoints[6] += amt;
            if (Game1.player.experiencePoints[6] > expNeededForLevel[expNeededForLevel.Length - 1])
                Game1.player.experiencePoints[6] = expNeededForLevel[expNeededForLevel.Length - 1];

            int newLevel = getCookingLevel();
            Log.debug("From level " + oldLevel + " to " + newLevel);
            for ( int i = oldLevel + 1; i <= newLevel; ++i )
            {
                if (i == 0)
                    continue;

                Log.debug("Adding new cooking level: " + i);
                newCookingLevels.Add(i);
            }
        }

        public static double getEdibilityMultiplier()
        {
            return 1 + getCookingLevel() * 0.03;
        }

        public static double getNoConsumeChance()
        {
            if (Game1.player.professions.Contains(PROFESSION_CONSERVATION))
                return 0.15;
            else
                return 0;

        }

        // Modifies the item based on professions and stuff
        // Returns for whether or not we should consume the ingredients
        public static bool onCook( CraftingRecipe recipe, Item item )
        {
            if (recipe.isCookingRecipe && item is Object)
            {
                Object obj = item as Object;
                int amtCrafted = 0;
                if (Game1.player.recipesCooked.ContainsKey(obj.parentSheetIndex))
                {
                    amtCrafted = Game1.player.recipesCooked[obj.parentSheetIndex];
                }
                Random rand = new Random((int)(Game1.stats.daysPlayed + Game1.uniqueIDForThisGame + (uint)obj.ParentSheetIndex + (uint)amtCrafted));

                obj.edibility = (int)(obj.edibility * getEdibilityMultiplier());

                if (Game1.player.professions.Contains(PROFESSION_SELLPRICE))
                {
                    obj.price = (int)(obj.price * 1.2);
                }

                if (Game1.player.professions.Contains(PROFESSION_SILVER))
                {
                    obj.quality = 1;
                }

                var used = new List<NewCraftingPage.ConsumedItem>();
                NewCraftingPage.myConsumeIngredients(recipe, false, used);

                int total = 0;
                foreach (NewCraftingPage.ConsumedItem ingr in used )
                    total += ingr.amt;

                for (int iq = 1; iq <= Object.bestQuality; ++iq)
                {
                    if (iq == 3) continue; // Not a real quality

                    double chance = 0;
                    foreach (NewCraftingPage.ConsumedItem ingr in used )
                    {
                        if (ingr.item.quality >= iq)
                            chance += (1.0 / total) * ingr.amt;
                    }

                    if (rand.NextDouble() < chance)
                        obj.quality = iq;
                }

                if (rand.NextDouble() < getNoConsumeChance())
                {
                    return false;
                }
                else return true;
            }

            return true;
        }

        private static void fixExpLength()
        {
            if ( Game1.player.experiencePoints.Length < 7 )
            {
                int[] newExp = new int[7];
                for ( int i = 0; i < 6; ++i )
                {
                    newExp[i] = Game1.player.experiencePoints[i];
                }
                Game1.player.experiencePoints = newExp;
            }
        }

        private void giveExpCommand( object sender, string[] args )
        {
            if ( args.Length != 1 )
            {
                Log.info("Command format: giveCookingExp <amount>");
                return;
            }

            int amt = 0;
            try
            {
                amt = Convert.ToInt32(args[0]);
            }
            catch ( Exception e )
            {
                Log.error( "Bad experience amount." );
                return;
            }

            addCookingExp(amt);
            Log.info("Added " + amt + " cooking experience.");
        }

        public const int PROFESSION_SELLPRICE = 50;
        public const int PROFESSION_BUFFTIME = 51;
        public const int PROFESSION_CONSERVATION = 52;
        public const int PROFESSION_SILVER = 53;
        public const int PROFESSION_BUFFLEVEL = 54;
        public const int PROFESSION_BUFFPLAIN = 55;

        public static Texture2D icon;

        public override void Entry( IModHelper helper )
        {
            instance = this;

            if (icon == null)
            {
                try
                {
                    string iconTex = Helper.DirectoryPath + Path.DirectorySeparatorChar + "iconA.png";
                    FileStream fs = new FileStream(iconTex, FileMode.Open);
                    icon = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fs);
                }
                catch (Exception e)
                {
                    Log.error("Failed to load icon: " + e);
                    icon = new Texture2D(Game1.graphics.GraphicsDevice, 16, 16);
                    icon.SetData(Enumerable.Range(0, 16 * 16).Select(i => new Color(225, 168, 255)).ToArray());
                }
            }

            Helper.ConsoleCommands.Add("player_givecookingexp", "player_givecookingexp <amount>", giveExpCommand);

            LocationEvents.CurrentLocationChanged += locChanged;
            GameEvents.UpdateTick += update;
            GraphicsEvents.OnPostRenderGuiEvent += drawAfterGui;
            
            SpaceEvents.ShowNightEndMenus += showLevelMenu;

            checkForExperienceBars();
            checkForLuck();
            checkForAllProfessions();
        }

        private bool wasEating = false;
        private int prevToEatStack = -1;
        private Buff lastFood = null, lastDrink = null;
        private void update(object sender, EventArgs args)
        {
            if (Game1.isEating != wasEating)
            {
                if ( !Game1.isEating )
                {
                    // Apparently this happens when the ask to eat dialog opens, but they pressed no.
                    // So make sure something was actually consumed.
                    if ( prevToEatStack != -1 && ( prevToEatStack - 1 == Game1.player.itemToEat.Stack ) )
                    {
                        Object obj = Game1.player.itemToEat as Object;
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
                                if (Game1.player.professions.Contains(PROFESSION_BUFFLEVEL))
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
                                if (newTime != -1 && Game1.player.professions.Contains(PROFESSION_BUFFTIME))
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
                                    lastFood = newBuff;
                                }
                                Game1.buffsDisplay.syncIcons();
                            }
                            else if (thisBuff == null && Game1.player.professions.Contains(PROFESSION_BUFFPLAIN))
                            {
                                Log.trace("Buffing plain");
                                Random rand = new Random();
                                int[] newAttr = new int[12];
                                int count = 1 + Math.Min(obj.edibility / 30, 3);
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

                                int newTime = 120 + obj.edibility / 10 * 30;

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
                                    if (Game1.buffsDisplay.food != null)
                                        Game1.buffsDisplay.drink.removeBuff();
                                    Game1.buffsDisplay.drink = newBuff;
                                    Game1.buffsDisplay.drink.addBuff();
                                    lastFood = newBuff;
                                }
                                Game1.buffsDisplay.syncIcons();
                            }
                        }
                    }
                }
                Log.trace("Eating:" + Game1.isEating);
                Log.trace("prev:" + prevToEatStack);
                Log.trace("I:"+Game1.player.itemToEat + " " + ((Game1.player.itemToEat != null) ? Game1.player.itemToEat.getStack() : -1));
                Log.trace("A:" + Game1.player.ActiveObject + " " + ((Game1.player.ActiveObject != null) ? Game1.player.ActiveObject.getStack() : -1));
                prevToEatStack = (Game1.player.itemToEat != null ? Game1.player.itemToEat.Stack : -1);
            }
            wasEating = Game1.isEating;

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

        private bool didInitSkills = false;
        private void drawAfterGui( object sender, EventArgs args )
        {
            if ( Game1.activeClickableMenu is GameMenu )
            {
                GameMenu menu = Game1.activeClickableMenu as GameMenu;
                if ( menu.currentTab == GameMenu.skillsTab )
                {
                    var tabs = ( List< IClickableMenu > ) Util.GetInstanceField(typeof(GameMenu), menu, "pages" );
                    var skills = (SkillsPage)tabs[GameMenu.skillsTab];

                    if ( !didInitSkills )
                    {
                        initCookingSkill(skills);
                        didInitSkills = true;
                    }
                    drawCookingSkill( skills );
                }
            }
            else didInitSkills = false;
        }

        private void initCookingSkill( SkillsPage skills )
        {
            int cookingLevel = getCookingLevel();

            // Bunch of stuff from the constructor
            int num2 = 0;
            int num3 = skills.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize - Game1.pixelZoom;
            int num4 = skills.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.pixelZoom * 3;
            for (int i = 4; i < 10; i += 5)
            {
                int j = 5;
                if (hasLuck)
                    j++;

                string text = "";
                string text2 = "";
                bool flag = false;
                int num5 = -1;

                flag = (cookingLevel > i);
                num5 = getProfessionForSkill( i + 1 );//Game1.player.getProfessionForSkill(5, i + 1);
                object[] args = new object[] { text, text2, CookingLevelUpMenu.getProfessionDescription(num5) };
                Util.CallInstanceMethod( typeof( SkillsPage ), skills, "parseProfessionDescription", args );
                text = (string)args[0];
                text2 = (string)args[1];

                if (flag && (i + 1) % 5 == 0)
                {
                    var skillBars = (List<ClickableTextureComponent>)Util.GetInstanceField(typeof(SkillsPage), skills, "skillBars");
                    skillBars.Add(new ClickableTextureComponent(string.Concat(num5), new Rectangle(num2 + num3 - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom), num4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6), 14 * Game1.pixelZoom, 9 * Game1.pixelZoom), null, text, Game1.mouseCursors, new Rectangle(159, 338, 14, 9), (float)Game1.pixelZoom, true));
                }
                num2 += Game1.pixelZoom * 6;
            }
            int k = 5;
            if (hasLuck)
                k++;
            int num6 = k;
            if (num6 == 1)
            {
                num6 = 3;
            }
            else if (num6 == 3)
            {
                num6 = 1;
            }
            string text3 = "";
            if (cookingLevel > 0)
            {
                text3 = string.Concat(new object[]
					    {
						    "+",
						    (int)((getEdibilityMultiplier() - 1) * 100),
						    "% edibility in home-cooked food",
						    Environment.NewLine,
					    });
            }
            var skillAreas = (List<ClickableTextureComponent>)Util.GetInstanceField(typeof(SkillsPage), skills, "skillAreas");
            skillAreas.Add(new ClickableTextureComponent(string.Concat(num6), new Rectangle(num3 - Game1.tileSize * 2 - Game1.tileSize * 3 / 4, num4 + k * (Game1.tileSize / 2 + Game1.pixelZoom * 6), Game1.tileSize * 2 + Game1.pixelZoom * 5, 9 * Game1.pixelZoom), string.Concat(num6), text3, null, Rectangle.Empty, 1f, false));
        }

        private void drawCookingSkill( SkillsPage skills )
        {
            int level = getCookingLevel();

            SpriteBatch b = Game1.spriteBatch;
            int j = 5;
            if (hasLuck)
                j++;

            int num;
            int num2;

            num = skills.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 * Game1.tileSize - 8;
            num2 = skills.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.pixelZoom * 2;

			int num3 = 0;
			for (int i = 0; i < 10; i++)
            {
                bool flag = false;
                bool flag2 = false;
                string text = "";
                int num4 = 0;
                Rectangle empty = Rectangle.Empty;

                flag = (level > i);
                if (i == 0)
                {
                    text = "Cooking";
                }
                num4 = level;
                flag2 = false;//(Game1.player.addedLuckLevel > 0);
                empty = new Rectangle(140, 512, 13, 16);

                if (!text.Equals(""))
                {
                    b.DrawString(Game1.smallFont, text, new Vector2((float)num - Game1.smallFont.MeasureString(text).X - (float)(Game1.pixelZoom * 4) - (float)Game1.tileSize, (float)(num2 + Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), Game1.textColor);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num - Game1.pixelZoom * 16), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(empty), Color.Black * 0.3f, 0f, Vector2.Zero, (float)Game1.pixelZoom * 0.75f, SpriteEffects.None, 0.85f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num - Game1.pixelZoom * 15), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(empty), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom * 0.75f, SpriteEffects.None, 0.87f);
                }
                if (!flag && (i + 1) % 5 == 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(145, 338, 14, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(145 + (flag ? 14 : 0), 338, 14, 9)), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                }
                else if ((i + 1) % 5 != 0)
                {
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num - Game1.pixelZoom + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(129, 338, 8, 9)), Color.Black * 0.35f, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.85f);
                    b.Draw(Game1.mouseCursors, new Vector2((float)(num3 + num + i * (Game1.tileSize / 2 + Game1.pixelZoom)), (float)(num2 - Game1.pixelZoom + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), new Rectangle?(new Rectangle(129 + (flag ? 8 : 0), 338, 8, 9)), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.87f);
                }
                if (i == 9)
                {
                    NumberSprite.draw(num4, b, new Vector2((float)(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 3 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0)), (float)(num2 + Game1.pixelZoom * 4 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), Color.Black * 0.35f, 1f, 0.85f, 1f, 0, 0);
                    NumberSprite.draw(num4, b, new Vector2((float)(num3 + num + (i + 2) * (Game1.tileSize / 2 + Game1.pixelZoom) + Game1.pixelZoom * 4 + ((num4 >= 10) ? (Game1.pixelZoom * 3) : 0)), (float)(num2 + Game1.pixelZoom * 3 + j * (Game1.tileSize / 2 + Game1.pixelZoom * 6))), (flag2 ? Color.LightGreen : Color.SandyBrown) * ((num4 == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0, 0);
                }
                if ((i + 1) % 5 == 0)
                {
                    num3 += Game1.pixelZoom * 6;
                }
            }
        }

        private void showLevelMenu( object sender, EventArgsShowNightEndMenus args )
        {
            if (args.Stage == EventStage.Before)
                return;

            Log.debug("Doing cooking menus");
            
            if (newCookingLevels.Count() > 0)
            {
                for (int i = newCookingLevels.Count() - 1; i >= 0; --i )
                {
                    int level = newCookingLevels[i];
                    Log.debug("Doing " + i + ": cooking level " + level + " screen");

                    if (Game1.activeClickableMenu != null)
                        Game1.endOfNightMenus.Push(Game1.activeClickableMenu);
                    Game1.activeClickableMenu = new CookingLevelUpMenu(level);
                }
                newCookingLevels.Clear();
            }
            else if ( getCookingLevel() >= 5 && !Game1.player.professions.Contains( PROFESSION_SELLPRICE ) &&!Game1.player.professions.Contains( PROFESSION_BUFFTIME ) )
            {
                Log.debug("Putting level 5 profession menu");
                if (Game1.activeClickableMenu != null)
                    Game1.endOfNightMenus.Push(Game1.activeClickableMenu);
                Game1.activeClickableMenu = new CookingLevelUpMenu(5);
            }
            else if (getCookingLevel() >= 10 && !Game1.player.professions.Contains(PROFESSION_CONSERVATION) && !Game1.player.professions.Contains(PROFESSION_SILVER) &&
                        !Game1.player.professions.Contains(PROFESSION_BUFFLEVEL) && !Game1.player.professions.Contains(PROFESSION_BUFFPLAIN))
            {
                Log.debug("Putting level 10 profession menu");
                if (Game1.activeClickableMenu != null)
                    Game1.endOfNightMenus.Push(Game1.activeClickableMenu);
                Game1.activeClickableMenu = new CookingLevelUpMenu(10);
            }
        }

        private void locChanged(object sender, EventArgs args)
        {
            if ( HAS_ALL_PROFESSIONS )
            {
                Util.DecompileComment("This is where AllProfessions does it.");
                Util.DecompileComment("This is that mod's code, too (from ILSpy, anyways). Just trying to give credit where credit is due. :P");
                List<int> professions = Game1.player.professions;
                List<List<int>> list = new List<List<int>> { professions5, professions10, };
                foreach (List<int> current in list)
                {
                    bool flag = professions.Intersect(current).Any<int>();
                    if (flag)
                    {
                        foreach (int current2 in current)
                        {
                            bool flag2 = !professions.Contains(current2);
                            if (flag2)
                            {
                                professions.Add(current2);
                            }
                        }
                    }
                }
                Util.DecompileComment("End of AllProfessions code.");
            }
        }

        private int getProfessionForSkill( int level )
        {
            if (level != 5 && level != 10)
                return -1;

            List<int> list = (level == 5 ? professions5 : professions10);
            foreach (int prof in list)
            {
                if (Game1.player.professions.Contains(prof))
                    return prof;
            }

            return -1;
        }

        private bool hasLuck = false;
        private void checkForLuck()
        {
            if ( !Helper.ModRegistry.IsLoaded( "spacechase0.LuckSkill" ) )
            {
                Log.info("Luck Skill not found");
                return;
            }

            Log.info("Luck found, making a note for later.");
            hasLuck = true;
        }

        private void checkForExperienceBars()
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.ExperienceBars"))
            {
                Log.info("Experience Bars not found");
                return;
            }

            Log.info("Experience Bars found, adding cooking experience bar renderer.");
            GraphicsEvents.OnPostRenderHudEvent += drawExperienceBar;
        }

        private void drawExperienceBar(object sender, EventArgs args_)
        {
            if (Game1.activeClickableMenu != null)
                return;

            try
            {
                if (Game1.player.experiencePoints.Count() < 7)
                    return;

                Type t = Type.GetType("ExperienceBars.Mod, ExperienceBars");

                int level = getCookingLevel();
                int exp = Game1.player.experiencePoints[6];
                int x = 10;
                int y = (int) Util.GetStaticField(t, "expBottom");

                int prevReq = 0, nextReq = 1;
                if (level == 0)
                {
                    nextReq = expNeededForLevel[0];
                }
                else if (level != 10)
                {
                    prevReq = expNeededForLevel[level - 1];
                    nextReq = expNeededForLevel[level];
                }

                int haveExp = exp - prevReq;
                int needExp = nextReq - prevReq;
                float progress = (float)haveExp / needExp;
                if (level == 10)
                {
                    progress = -1;
                }

                object[] args = new object[]
                {
                    x, y,
                    icon, new Rectangle( 0, 0, 16, 16 ), 
                    level, progress,
                    new Color( 196, 79, 255 ),
                };
                Util.CallStaticMethod(t, "renderSkillBar", args);

                Util.SetStaticField(t, "expBottom", y + 40);
            }
            catch ( Exception e)
            {
                Log.error( "Exception rendering cooking bar: " + e );
                GraphicsEvents.OnPostRenderHudEvent -= drawExperienceBar;
            }
        }

        private bool HAS_ALL_PROFESSIONS = false;
        private List<int> professions5 = new List<int>() { PROFESSION_SELLPRICE, PROFESSION_BUFFTIME };
        private List<int> professions10 = new List<int>() { PROFESSION_CONSERVATION, PROFESSION_SILVER, PROFESSION_BUFFLEVEL, PROFESSION_BUFFPLAIN };
        private void checkForAllProfessions()
        {
            if (!Helper.ModRegistry.IsLoaded("community.AllProfessions"))
            {
                Log.info("[CookingSkill] All Professions not found.");
                return;
            }

            Log.info("[CookingSkill] All Professions found. You will get every cooking profession for your level.");
            HAS_ALL_PROFESSIONS = true;
        }
    }
}
