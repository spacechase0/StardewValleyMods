using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Spacechase.Shared.Patching;
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
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
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
            if (!__instance.bigCraftable.Value || __instance.ItemID != "Statue_of_Generosity")
                return;

            __instance.MinutesUntilReady = 1;
            __instance.heldObject.Value = ObjectPatcher.GetRandomGift();
        }

        /// <summary>Get the random gift for today.</summary>
        private static SObject GetRandomGift()
        {
            SObject gift = ObjectPatcher
                .GetCandidateVillagers()
                .Select(ObjectPatcher.GetRandomGift)
                .FirstOrDefault(p => p is not null);

            return gift ?? new SObject(SObject.prismaticShardIndex, 1);
        }

        /// <summary>Get a random loved gift for a villager.</summary>
        /// <param name="villager">The villager for whom to create a gift.</param>
        private static SObject GetRandomGift(NPC villager)
        {
            // extract loved items list
            string[] rawIds;
            {
                rawIds = Game1.NPCGiftTastes.TryGetValue(villager.Name, out string data) && data != null && data.Split('/').TryGetIndex(1, out string field)
                    ? field.Split(' ')
                    : Array.Empty<string>();
            }

            // get random valid item
            foreach (string rawId in rawIds.OrderBy(_ => Game1.random.Next()))
            {
                if (!int.TryParse(rawId, out int id))
                    continue; // context tag or invalid ID

                try
                {
                    SObject item = new SObject(id, 1);
                    if (item.Name is not (null or "Error Item"))
                        return item;
                }
                catch
                {
                    // ignore invalid items
                }
            }

            return null;
        }

        /// <summary>Get the villager NPCs in the order in which to try creating a gift.</summary>
        private static IEnumerable<NPC> GetCandidateVillagers()
        {
            return Utility
                .getAllCharacters(new List<NPC>())
                .Where(npc => npc.isVillager())
                .OrderByDescending(npc => npc.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                .ThenBy(_ => Game1.random.Next());
        }
    }
}
