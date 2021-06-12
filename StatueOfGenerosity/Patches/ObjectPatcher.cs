using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Spacechase.Shared.Harmony;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace StatueOfGenerosity.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SObject"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class ObjectPatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Get the bigcraftable id for the Statue of Generosity.</summary>
        private static Func<int> GetStatueId;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="getStatueId">Get the bigcraftable id for the Statue of Generosity.</param>
        public ObjectPatcher(Func<int> getStatueId)
        {
            ObjectPatcher.GetStatueId = getStatueId;
        }

        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SObject>(nameof(SObject.DayUpdate)),
                postfix: this.GetHarmonyMethod(nameof(After_DayUpdate))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call after <see cref="SObject.DayUpdate"/>.</summary>
        public static void After_DayUpdate(SObject __instance, GameLocation location)
        {
            if (!__instance.bigCraftable.Value || __instance.ParentSheetIndex != ObjectPatcher.GetStatueId())
                return;

            NPC npc = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
            if (npc == null)
                npc = Utility.getRandomTownNPC();

            Game1.NPCGiftTastes.TryGetValue(npc.Name, out string str);
            string[] favs = str.Split('/')[1].Split(' ');

            __instance.MinutesUntilReady = 1;
            __instance.heldObject.Value = new SObject(int.Parse(favs[Game1.random.Next(favs.Length)]), 1, false, -1, 0);
        }
    }
}
