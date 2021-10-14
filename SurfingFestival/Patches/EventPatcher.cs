using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Patching;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SurfingFestival.Framework;

namespace SurfingFestival.Patches
{
    /// <summary>Applies Harmony patches to <see cref="Event"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class EventPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.setUpPlayerControlSequence)),
                postfix: this.GetHarmonyMethod(nameof(After_SetUpPlayerControlSequence))
            );

            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.setUpFestivalMainEvent)),
                postfix: this.GetHarmonyMethod(nameof(After_SetUpFestivalMainEvent))
            );

            harmony.Patch(
                original: this.RequireMethod<Event>(nameof(Event.draw)),
                postfix: this.GetHarmonyMethod(nameof(After_Draw))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="Event.setUpPlayerControlSequence"/>.</summary>
        private static void After_SetUpPlayerControlSequence(Event __instance, string id)
        {
            if (id == "surfing")
            {
                Mod.Instance.Helper.Reflection.GetField<NPC>(__instance, "festivalHost").SetValue(__instance.getActorByName("Lewis"));
                Mod.Instance.Helper.Reflection.GetField<string>(__instance, "hostMessage").SetValue($"$q -1 null#{I18n.Race_Start_Question()}#$r -1 0 yes#{I18n.Race_Start_Yes()}#$r -1 0 no#{I18n.Race_Start_No()}");
            }
        }

        /// <summary>The method to call after <see cref="Event.setUpFestivalMainEvent"/>.</summary>
        private static void After_SetUpFestivalMainEvent(Event __instance)
        {
            if (!__instance.isSpecificFestival("summer5"))
                return;

            // ...
        }

        /// <summary>The method to call after <see cref="Event.draw"/>.</summary>
        private static void After_Draw(Event __instance, SpriteBatch b)
        {
            if (!__instance.isSpecificFestival("summer5"))
                return;

            Mod.Instance.DrawObstacles(b);
        }
    }
}
