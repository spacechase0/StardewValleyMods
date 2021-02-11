using Harmony;
using JsonAssets.Game;
using JsonAssets.PackData;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.Patches
{
    [HarmonyPatch(typeof(NPC), nameof(NPC.receiveGift))]
    public static class NPCGiftFriendshipPatch
    {
        public static bool Prefix(NPC __instance, StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo, float friendshipChangeMultiplier, bool showResponse )
        {
            if ( o is CustomObject )
            {
                DoReceiveGift( __instance, o, giver, updateGiftLimitInfo, friendshipChangeMultiplier, showResponse );
                return false;
            }
            return true;
        }

        private static void DoReceiveGift( NPC npc, StardewValley.Object o, Farmer giver, bool updateGiftLimitInfo, float friendshipChangeMultiplier, bool showResponse )
        {
            giver?.onGiftGiven( npc, o );
            if ( !Game1.NPCGiftTastes.ContainsKey( npc.Name ) )
                return;

            Game1.stats.GiftsGiven++;
            giver.currentLocation.localSound( "give_gift" );

            if ( updateGiftLimitInfo )
            {
                giver.friendshipData[ npc.Name ].GiftsToday++;
                giver.friendshipData[ npc.Name ].GiftsThisWeek++;
                giver.friendshipData[ npc.Name ].LastGiftDate = new WorldDate( Game1.Date );
            }

            if ( npc.getSpouse() == giver )
            {
                friendshipChangeMultiplier /= 2;
            }

            var obj = o as CustomObject;
            var data = obj.Data;
            int amt = data.UniversalGiftTaste;
            data.GiftTasteOverride.TryGetValue( npc.Name, out var overrideEntry );
            if ( overrideEntry != null )
                amt = overrideEntry.Amount;

            int giftTaste = 0;
            if ( amt >= 80 ) giftTaste = NPC.gift_taste_love;
            else if ( amt >= 45 ) giftTaste = NPC.gift_taste_like;
            else if ( amt <= -40 ) giftTaste = NPC.gift_taste_hate;
            else if ( amt <= -20 ) giftTaste = NPC.gift_taste_dislike;
            else giftTaste = NPC.gift_taste_neutral;

            float qualMult = 1;
            switch ( o.Quality )
            {
                case 1: qualMult = 1.1f; break;
                case 2: qualMult = 1.25f; break;
                case 4: qualMult = 1.5f; break;
            }
            // Vanilla only has a quality multiplier for liked or loved
            if ( giftTaste != NPC.gift_taste_like && giftTaste != NPC.gift_taste_love )
                qualMult = 1;

            string response = null;
            if ( npc.Birthday_Season == Game1.currentSeason && npc.Birthday_Day == Game1.dayOfMonth )
            {
                friendshipChangeMultiplier = 8;

                if ( overrideEntry != null )
                    response = data.parent.smapiPack.Translation.Get( overrideEntry.BirthdayTextTranslationKey ).ToString();
                if ( response == null )
                {
                    switch ( giftTaste )
                    {
                        case NPC.gift_taste_love:
                        case NPC.gift_taste_like:
                            response = ( npc.Manners == 2 ) ? Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4274" ) : Game1.content.LoadString( "Strings\\StringsFromCSFiles:NPC.cs.4275" );
                            if ( Game1.random.NextDouble() < 0.5 )
                            {
                                response = ( ( npc.Manners == 2 ) ? Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4276" ) : Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4277" ) );
                            }
                            break;
                        case NPC.gift_taste_neutral:
                            response = ( npc.Manners == 2 ) ? Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4278" ) : Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4279" );
                            break;
                        case NPC.gift_taste_dislike:
                        case NPC.gift_taste_hate:
                            response = ( npc.Manners == 2 ) ? Game1.content.LoadString( "Strings\\StringsFromCSFiles:NPC.cs.4280" ) : Game1.LoadStringByGender( npc.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4281" );
                            break;
                    }
                }
            }
            else
            {
                if ( overrideEntry != null )
                    response = data.parent.smapiPack.Translation.Get( overrideEntry.NormalTextTranslationKey ).ToString();
                if ( response == null )
                {
                    var reactions = Game1.NPCGiftTastes[ npc.Name ].Split( '/' );
                    response = reactions[ giftTaste ];
                }
            }

            // Special NPC cases
            if ( npc.Name.Contains( "Dwarf" ) && !giver.canUnderstandDwarves )
                response = Dialogue.convertToDwarvish( response );
            if ( npc.Name == "Krobus" && Game1.shortDayNameFromDayOfSeason( Game1.dayOfMonth ) == "Fri" )
                response = "...";

            int? emote = null;
            if ( overrideEntry != null )
                emote = overrideEntry.EmoteId;
            if ( emote == null )
            {
                switch ( giftTaste )
                {
                    case NPC.gift_taste_love: emote = 20; break;
                    case NPC.gift_taste_hate: emote = 12; break;
                }
            }

            Game1.drawDialogue( npc, response );
            giver.changeFriendship( ( int )( amt * friendshipChangeMultiplier * qualMult ), npc );
            switch ( giftTaste )
            {
                case NPC.gift_taste_love: npc.faceTowardFarmerForPeriod( 15000, 5, faceAway: false, giver ); break;
                case NPC.gift_taste_like: npc.faceTowardFarmerForPeriod( 7000, 5, faceAway: true, giver ); break;
                case NPC.gift_taste_hate: npc.faceTowardFarmerForPeriod( 15000, 5, faceAway: true, giver ); break;
            }
            if ( emote.HasValue )
                giver.doEmote( emote.Value );
        }
    }
}
