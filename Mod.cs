using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ThreeHeartDancePartner
{
    public class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            GameEvents.UpdateTick += onUpdate;
        }

        private void onUpdate(object sender, EventArgs args)
        {
            if (Game1.currentLocation == null || Game1.currentLocation.name != "Temp" || Game1.currentLocation.currentEvent == null)
                return;
            Event @event = Game1.currentLocation.currentEvent;
            //Dictionary<string, string> data = (Dictionary<string, string>)Util.GetInstanceField(typeof(Event), @event, "festivalData");

            if (!@event.FestivalName.Equals("Flower Dance"))
                return;

            foreach ( NPC npc in @event.actors )
            {
                if ( !npc.datable || npc.hasPartnerForDance) continue;
                try
                {
                    if (npc.CurrentDialogue.Count() <= 0) return;
                    Dialogue reject = new Dialogue( Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + npc.name)["danceRejection"], npc );
                    Dialogue curr = npc.CurrentDialogue.Peek();
                    if (reject == null || curr == null) continue;

                    //Log.Async("Dialogue " + curr.getCurrentDialogue() + " " + reject.getCurrentDialogue());
                    if ( curr.getCurrentDialogue() == reject.getCurrentDialogue() )
                    {
                        NPC who = npc;
                        // The original stuff, only the relationship point check is modified. (1000 -> 750)
                        if (!who.hasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.name) >= 750)
                        {
                            string s = "";
                            switch (who.gender)
                            {
                                case 0:
                                    s = "You want to be my partner for the flower dance?#$b#Okay. I look forward to it.$h";
                                    break;
                                case 1:
                                    s = "You want to be my partner for the flower dance?#$b#Okay! I'd love to.$h";
                                    break;
                            }
                            if (Game1.IsMultiplayer)
                            {
                                MultiplayerUtility.sendMessageToEveryone(0, who.name, Game1.player.uniqueMultiplayerID);
                            }
                            try
                            {
                                Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.name));
                            }
                            catch (Exception)
                            {
                            }
                            if (!Game1.IsMultiplayer || Game1.IsServer)
                            {
                                Game1.player.dancePartner = who;
                                who.hasPartnerForDance = true;
                            }
                            who.setNewDialogue(s, false, false);

                            // Okay, looks like I need to fix the current dialog box
                            Game1.activeClickableMenu = new DialogueBox(new Dialogue(s, who) { removeOnNextMove = false });
                        }
                    }
                }
                catch ( Exception e)
                {
                    this.Monitor.Log("Exception: " + e, LogLevel.Error);
                    continue;
                }
            }
        }
    }
}
