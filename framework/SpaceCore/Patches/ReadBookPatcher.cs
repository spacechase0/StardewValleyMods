using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Patches
{
    internal class ReadBookPatcher : BasePatcher
    {
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<StardewValley.Object>("readBook"),
                prefix: this.GetHarmonyMethod(nameof(Before_ReadBook))
            );
        }

        public static bool Before_ReadBook(StardewValley.Object __instance, GameLocation location)
        {

            //Players can only "read books" Objects if they are the right catagory.
            //Here is a universal patch for players to add EXP books
            //For it to work, players will have to add a tag to their exp book that has their skill ID

            //Make a variable to see if we have found a custom book and have it be false for now.
            bool customSkillbook = false;



            //First, we check to see if the book being used is the book of stars.
            if (__instance.Name == "Book Of Stars")
            {
                //If it is, the book of stars is meant to give every skill 250 exp. So we look through all the skills here and grant it exp.
                //We don't want to cancel the normal behavior of the book of stars, so we don't set custom book = true.

                //We only want to apply exp from the book of stars if the skill is visible to the player.
                string[] VisibleSkills = Skills.GetSkillList().Where(s => Skills.GetSkill(s).ShouldShowOnSkillsPage).ToArray();
                foreach (string skill in VisibleSkills)
                {
                    Skills.AddExperience(Game1.player, skill, 250);
                }

                //If it isn't the book of stars, we then check the context tags of the book
            } else
            {
                //Go through each skill in the skill list that is loaded with space core
                foreach (string skill in Skills.GetSkillList())
                {
                    Log.Trace("For Each loop of reading the book. here is a list of custom skill: " + skill);
                    //If we found a book that has the skill ID as a tag continue
                    if (__instance.HasContextTag(skill))
                    {
    
                        Log.Trace("Found a custom tag for skill: " + skill + ". Adding exp to it");
    
                        //Copied from Vanilla
                        Game1.player.canMove = false;
                        Game1.player.freezePause = 1030;
                        Game1.player.faceDirection(2);
                        Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
                        {
                    new FarmerSprite.AnimationFrame(57, 1000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
                    {
                        frameEndBehavior = delegate
                        {
                            location.removeTemporarySpritesWithID(1987654);
                            Utility.addRainbowStarExplosion(location, Game1.player.getStandingPosition() + new Vector2(-40f, -156f), 8);
                        }
                    }
                        });
                        Game1.MusicDuckTimer = 4000f;
                        Game1.playSound("book_read");
                        Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
                        {
                            holdLastFrame = true,
                            id = 1987654
                        });
                        Color? colorFromTags = ItemContextTagManager.GetColorFromTags(__instance);
                        if (colorFromTags.HasValue)
                        {
                            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Microsoft.Xna.Framework.Rectangle(0, 20, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.0012f, 0f, colorFromTags.Value, 4f, 0f, 0f, 0f)
                            {
                                holdLastFrame = true,
                                id = 1987654
                            });
                        }
    
                        int count = Game1.player.newLevels.Count;
                        //Add skill exp, vanilla has it be 250
                        Skills.AddExperience(Game1.player, skill, 250);
    
                        //Code to send a message if the player's level has changed to sleep.
                        // disabled for now
                        ///if (Game1.player.newLevels.Count == count || (Game1.player.newLevels.Count > 1 && count >= 1))
                        ///{
                        ///    DelayedAction.functionAfterDelay(delegate
                        ///    {
                        ///        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:SkillBookMessage", Game1.content.LoadString("Strings\\1_6_Strings:SkillName_" + base.ItemId.Last()).ToLower()));
                        ///    }, 1000);
                        ///}
                        ///
    
                        //Break the foreach loop after setting that we found a custom skillbook
                        customSkillbook = true;
                        break;
                    }
    
                }
            }

            //If it is a custom skill, we don't want to continue the read book function
            // so we return false to cancel the rest of it.
            if (customSkillbook)
            {
                return false;
            }

            return true;
        }
    }
}
