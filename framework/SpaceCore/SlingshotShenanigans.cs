using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace SpaceCore
{
    [HarmonyPatch(typeof(BasicProjectile), nameof(BasicProjectile.behaviorOnCollisionWithMonster))]
    public static class SlingshotHitMonsterPatch
    {
        public static bool Prefix(BasicProjectile __instance, NPC n, GameLocation location)
        {
            if (__instance.damagesMonsters.Value && n is not Monster && __instance.itemId.Value != null)
            {
                Farmer player = __instance.GetPlayerWhoFiredMe(location);
                SpaceCore.Instance.Helper.Reflection.GetMethod(__instance, "explosionAnimation").Invoke(location);

                DoGetHitByPlayerOverride(n, __instance.itemId.Value, player, location);

                string projectileTokenizedName = TokenStringBuilder.ItemName(__instance.itemId.Value);
                Game1.Multiplayer.globalChatInfoMessage("Slingshot_Hit", player.Name, n.GetTokenizedDisplayName(), Lexicon.prependTokenizedArticle(projectileTokenizedName));

                return false;
            }

            return true;
        }

        private static void DoGetHitByPlayerOverride(NPC npc, string itemId, Farmer who, GameLocation location)
        {
            npc.doEmote(12);
            if (who == null)
            {
                if (Game1.IsMultiplayer)
                {
                    return;
                }
                who = Game1.player;
            }
            if (who.friendshipData.ContainsKey(npc.Name))
            {
                who.changeFriendship(-30, npc);
                if (who.IsLocalPlayer)
                {
                    npc.CurrentDialogue.Clear();
                    Dialogue d = npc.TryGetDialogue("HitBySlingshot_" + itemId);
                    if (d == null)
                    {
                        var item = ItemRegistry.Create(itemId);
                        foreach (string ctx in item.GetContextTags())
                        {
                            d ??= npc.TryGetDialogue("HitBySlingshot_" + ctx);
                            if (d != null)
                                break;
                        }
                    }
                    d ??= npc.TryGetDialogue("HitBySlingshot");

                    npc.CurrentDialogue.Push(d ?? (Game1.random.NextBool() ? new Dialogue(npc, "Strings\\StringsFromCSFiles:NPC.cs.4293", isGendered: true) : new Dialogue(npc, "Strings\\StringsFromCSFiles:NPC.cs.4294")));
                }
                if (npc.Sprite.Texture != null)
                {
                    location.debris.Add(new Debris(npc.Sprite.textureName.Value, Game1.random.Next(3, 8), Utility.PointToVector2(npc.StandingPixel)));
                }
            }
            if (npc.Name.Equals("Bouncer"))
            {
                location.localSound("crafting");
            }
            else
            {
                location.localSound("hitEnemy");
            }
        }
    }
}
