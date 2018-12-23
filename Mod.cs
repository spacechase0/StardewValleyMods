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
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, EventArgs e)
        {
            Event @event = Game1.currentLocation?.currentEvent;
            if (Game1.currentLocation?.Name != "Temp" || @event?.FestivalName.Equals("Flower Dance") != true)
                return;

            foreach ( NPC npc in @event.actors )
            {
                if ( !npc.datable.Value || npc.HasPartnerForDance)
                    continue;

                try
                {
                    if (!npc.CurrentDialogue.Any()) return;
                    Dialogue reject = new Dialogue( Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{npc.Name}")["danceRejection"], npc );
                    Dialogue curr = npc.CurrentDialogue.Peek();
                    if (curr == null)
                        continue;

                    if ( curr.getCurrentDialogue() == reject.getCurrentDialogue() )
                    {
                        NPC who = npc;
                        // The original stuff, only the relationship point check is modified. (1000 -> 750)
                        if (!who.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.Name) >= 750)
                        {
                            string s = "";
                            switch (who.Gender)
                            {
                                case NPC.male:
                                    s = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633");
                                    break;
                                case NPC.female:
                                    s = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634");
                                    break;
                            }
                            try
                            {
                                Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name));
                            }
                            catch (Exception)
                            {
                            }
                            Game1.player.dancePartner.Value = who;
                            who.setNewDialogue(s);

                            foreach (NPC actor in @event.actors)
                            {
                                if (actor.CurrentDialogue != null && actor.CurrentDialogue.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                                    actor.CurrentDialogue.Clear();
                            }

                            // Okay, looks like I need to fix the current dialog box
                            Game1.activeClickableMenu = new DialogueBox(new Dialogue(s, who) { removeOnNextMove = false });
                        }
                    }
                }
                catch ( Exception ex)
                {
                    this.Monitor.Log($"Exception: {ex}", LogLevel.Error);
                }
            }
        }
    }
}
