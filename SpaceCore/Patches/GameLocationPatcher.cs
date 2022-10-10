using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using xTile.ObjectModel;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="GameLocation"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class GameLocationPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformAction)),
                postfix: this.GetHarmonyMethod(nameof(After_PerformAction))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.answerDialogueAction)),
                postfix: this.GetHarmonyMethod(nameof(After_AnswerDialogueAction))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.performTouchAction)),
                prefix: this.GetHarmonyMethod(nameof(Before_PerformTouchAction))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>(nameof(GameLocation.explode)),
                postfix: this.GetHarmonyMethod(nameof(After_Explode))
            );

            harmony.Patch(
                original: this.RequireMethod<GameLocation>( nameof( GameLocation.GetLocationContext ) ),
                prefix: this.GetHarmonyMethod( nameof( Before_GetLocationContext ) )
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="GameLocation.performAction"/>.</summary>
        private static bool Before_PerformAction(GameLocation __instance, string action, Farmer who, Location tileLocation)
        {
            return !SpaceEvents.InvokeActionActivated(who, action, tileLocation);
        }

        private static void After_PerformAction(GameLocation __instance, string action, Farmer who, Location tileLocation)
        {
            if (action == "DogStatue")
            {
                // close existing dialogue
                Game1.exitActiveMenu();

                if (Skills.CanRespecAnySkill())
                {
                    __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), __instance.createYesNoResponses(), "dogStatue");
                    return;
                }
                string displayed_text = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
                displayed_text = displayed_text.Substring(0, displayed_text.LastIndexOf('^'));
                Game1.drawObjectDialogue(displayed_text);
            }
        }

        private static void After_AnswerDialogueAction(GameLocation __instance, string questionAndAnswer, string[] questionParams)
        {
            if (questionAndAnswer == "dogStatue_Yes")
            {
                if (Game1.player.Money < 10000)
                {
                    return;
                }
                List<Response> responses = new List<Response>();
                for (int i = 0; i < 5; i++)
                {
                    if (GameLocation.canRespec(i))
                    {
                        responses.Add(new Response(Farmer.getSkillNameFromIndex(i).ToLower(), Farmer.getSkillDisplayNameFromIndex(i)));
                    }
                }

                foreach (string skill in Skills.GetSkillList())
                {
                    if (Skills.CanRespecCustomSkill(skill))
                    {
                        responses.Add(new Response(skill, Skills.GetSkill(skill).GetName()));
                    }
                }
                responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueQuestion"), responses.ToArray(), "professionForget");
            }
            else if (questionAndAnswer.StartsWith("professionForget_"))
            {
                Skills.Skill skill = Skills.GetSkill(questionAndAnswer.Split('_', 2)[1]);
                if (skill is null)
                {
                    return;
                }

                if (Skills.NewLevels.Contains(new KeyValuePair<string, int>(skill.Id, 5)) || Skills.NewLevels.Contains(new KeyValuePair<string, int>(skill.Id, 10)))
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueAlready"));
                    return;
                }

                Game1.player.Money = Math.Max(0, Game1.player.Money - 10000);
                foreach (Skills.Skill.Profession profession in skill.Professions)
                {
                    GameLocation.RemoveProfession(profession.GetVanillaId());
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));
                    int level = Skills.GetSkillLevel(Game1.player, skill.Id);
                    if (level >= 5)
                    {
                        Skills.NewLevels.Add(new KeyValuePair<string, int>(skill.Id, 5));
                    }
                    if (level >= 10)
                    {
                        Skills.NewLevels.Add(new KeyValuePair<string, int>(skill.Id, 10));
                    }
                    DelayedAction.playSoundAfterDelay("dog_bark", 300);
                    DelayedAction.playSoundAfterDelay("dog_bark", 900);
                    return;
                }
            }
        }

        /// <summary>The method to call before <see cref="GameLocation.performTouchAction"/>.</summary>
        private static bool Before_PerformTouchAction(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
        {
            return !SpaceEvents.InvokeTouchActionActivated(Game1.player, fullActionString, new Location(0, 0));
        }

        /// <summary>The method to call after <see cref="GameLocation.explode"/>.</summary>
        private static void After_Explode(GameLocation __instance, Vector2 tileLocation, int radius, Farmer who)
        {
            SpaceEvents.InvokeBombExploded(who, tileLocation, radius);
        }

        private static bool Before_GetLocationContext( GameLocation __instance, ref GameLocation.LocationContext __result )
        {
            __result = GetLocationContextImpl( __instance );
            return false;
        }

        private static GameLocation.LocationContext GetLocationContextImpl( GameLocation loc )
        {
            if ( loc.locationContext == ( GameLocation.LocationContext ) ( -1 ) )
            {
                if ( loc.map == null )
                {
                    loc.reloadMap();
                }
                loc.locationContext = GameLocation.LocationContext.Default;
                string location_context = null;
                PropertyValue value = null;
                if ( loc.map == null )
                {
                    return GameLocation.LocationContext.Default;
                }
                location_context = ( ( !loc.map.Properties.TryGetValue( "LocationContext", out value ) ) ? "" : value.ToString() );
                bool foundCustom = false;
                foreach ( var kvp in SpaceCore.CustomLocationContexts )
                {
                    if ( kvp.Value.Name == location_context )
                    {
                        loc.locationContext = kvp.Key;
                        foundCustom = true;
                        break;
                    }
                }
                if ( !foundCustom && location_context != "" && !Enum.TryParse<GameLocation.LocationContext>( location_context, out loc.locationContext ) )
                {
                    loc.locationContext = GameLocation.LocationContext.Default;
                }
            }
            return loc.locationContext;
        }
    }
}
