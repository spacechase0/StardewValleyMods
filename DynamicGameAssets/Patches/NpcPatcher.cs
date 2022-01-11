using System.Diagnostics.CodeAnalysis;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Spacechase.Shared.Patching;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;

namespace DynamicGameAssets.Patches
{
    /// <summary>Applies Harmony patches to <see cref="NPC"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class NpcPatcher : BasePatcher
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Apply(Harmony harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<NPC>(nameof(NPC.receiveGift)),
                prefix: this.GetHarmonyMethod(nameof(Before_ReceiveGift))
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method to call before <see cref="NPC.receiveGift"/>.</summary>
        /// <returns>Returns whether to run the original method.</returns>
        private static bool Before_ReceiveGift(NPC __instance, SObject o, Farmer giver, bool updateGiftLimitInfo, float friendshipChangeMultiplier, bool showResponse)
        {
            if (o is CustomObject customObj)
            {
                NpcPatcher.DoReceiveGift(__instance, customObj, giver, updateGiftLimitInfo, friendshipChangeMultiplier, showResponse);
                SpaceEvents.InvokeAfterGiftGiven( __instance, o, giver );
                return false;
            }
            return true;
        }

        private static void DoReceiveGift(NPC npc, CustomObject obj, Farmer giver, bool updateGiftLimitInfo, float friendshipChangeMultiplier, bool showResponse)
        {
            // run base logic
            giver.onGiftGiven(npc, obj);
            if (!Game1.NPCGiftTastes.TryGetValue(npc.Name, out string rawGiftTastes))
                return;
            if (!giver.friendshipData.TryGetValue(npc.Name, out Friendship friendship))
                return;
            giver.currentLocation.localSound("give_gift");

            // update stats
            Game1.stats.GiftsGiven++;
            if (updateGiftLimitInfo)
            {
                friendship.GiftsToday++;
                friendship.GiftsThisWeek++;
                friendship.LastGiftDate = new WorldDate(Game1.Date);
            }

            // collect info
            bool isBirthday = npc.Birthday_Season == Game1.currentSeason && npc.Birthday_Day == Game1.dayOfMonth;
            GiftTastePackData giftTasteData = NpcPatcher.GetGiftTastePackData(npc, obj);
            int friendshipChange = giftTasteData.Amount;
            int giftTaste = friendshipChange switch
            {
                (>= 80) => NPC.gift_taste_love,
                (>= 45) => NPC.gift_taste_like,
                (<= -40) => NPC.gift_taste_hate,
                (<= -20) => NPC.gift_taste_dislike,
                _ => NPC.gift_taste_neutral
            };

            // get quality multiplier
            float qualityMultiplier = obj.Quality switch
            {
                Object.medQuality => 1.1f,
                Object.highQuality => 1.25f,
                Object.bestQuality => 1.5f,
                _ => 1
            };
            if (giftTaste is not (NPC.gift_taste_like or NPC.gift_taste_love)) // vanilla only has a quality multiplier for liked or loved
                qualityMultiplier = 1;

            // adjust friendship change multiplier
            if (isBirthday)
                friendshipChangeMultiplier = 8;
            else if (npc.getSpouse() == giver)
                friendshipChangeMultiplier /= 2;

            // get NPC response
            string response = null;
            if (isBirthday)
            {
                if (giftTasteData.BirthdayTextTranslationKey != null)
                    response = giftTasteData.pack.smapiPack.Translation.Get(giftTasteData.BirthdayTextTranslationKey).ToString();
                else
                {
                    switch (giftTaste)
                    {
                        case NPC.gift_taste_love:
                        case NPC.gift_taste_like:
                            response = (npc.Manners == 2) ? Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4274") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4275");
                            if (Game1.random.NextDouble() < 0.5)
                            {
                                response = ((npc.Manners == 2) ? Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4276") : Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4277"));
                            }
                            break;

                        case NPC.gift_taste_neutral:
                            response = (npc.Manners == 2) ? Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4278") : Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4279");
                            break;

                        case NPC.gift_taste_dislike:
                        case NPC.gift_taste_hate:
                            response = (npc.Manners == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4280") : Game1.LoadStringByGender(npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4281");
                            break;
                    }
                }
            }
            else
            {
                if (giftTasteData.NormalTextTranslationKey != null)
                    response = giftTasteData.pack.smapiPack.Translation.Get(giftTasteData.NormalTextTranslationKey).ToString();
                else
                {
                    string[] reactions = rawGiftTastes.Split('/');
                    response = reactions[giftTaste];
                }
            }

            // adjust response for specific NPCs
            switch (npc.Name)
            {
                case "Dwarf":
                    if (!giver.canUnderstandDwarves)
                        response = Dialogue.convertToDwarvish(response);
                    break;

                case "Krobus":
                    if (Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth) == "Fri")
                        response = "...";
                    break;
            }

            // get emote
            int? emote = null;
            if (giftTasteData.EmoteId >= 0)
                emote = giftTasteData.EmoteId;
            else
            {
                emote = giftTaste switch
                {
                    NPC.gift_taste_love => 20,
                    NPC.gift_taste_hate => 12,
                    _ => emote
                };
            }

            // apply changes
            Game1.drawDialogue(npc, response);
            giver.changeFriendship((int)(friendshipChange * friendshipChangeMultiplier * qualityMultiplier), npc);
            switch (giftTaste)
            {
                case NPC.gift_taste_love:
                    npc.faceTowardFarmerForPeriod(15000, 5, faceAway: false, giver);
                    break;

                case NPC.gift_taste_like:
                    npc.faceTowardFarmerForPeriod(7000, 5, faceAway: true, giver);
                    break;

                case NPC.gift_taste_hate:
                    npc.faceTowardFarmerForPeriod(15000, 5, faceAway: true, giver);
                    break;
            }
            if (emote.HasValue)
                giver.doEmote(emote.Value);
        }

        private static GiftTastePackData GetGiftTastePackData(NPC npc, CustomObject obj)
        {
            // get from content pack
            if (Mod.giftTastes.TryGetValue(npc.Name, out var giftTasteDataForNpc) && giftTasteDataForNpc.TryGetValue(obj.FullId, out GiftTastePackData giftTasteData))
                return giftTasteData;

            // else get default values
            return new GiftTastePackData
            {
                Amount = obj.Data.UniversalGiftTaste
            };
        }
    }
}
