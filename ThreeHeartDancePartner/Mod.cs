using System;
using System.Collections.Generic;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ThreeHeartDancePartner
{
    internal class Mod : StardewModdingAPI.Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // get dialog box
            if (e.NewMenu is not DialogueBox dialogBox)
                return;

            // get festival
            Event festival = Game1.currentLocation?.currentEvent;
            if (Game1.currentLocation?.Name != "Temp" || festival?.FestivalName != "Flower Dance")
                return;

            // check if rejection dialogue
            Dialogue dialog = dialogBox.characterDialogue;
            NPC npc = dialog.speaker;
            if (!npc.datable.Value || npc.HasPartnerForDance)
                return;
            string rejectionText = new Dialogue(Game1.content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{dialog.speaker.Name}")["danceRejection"], dialog.speaker).getCurrentDialogue();
            if (dialog.getCurrentDialogue() != rejectionText)
                return;

            // replace with accept dialog
            // The original stuff, only the relationship point check is modified. (1000 -> 750)
            if (!npc.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(npc.Name) >= 750)
            {
                string s = npc.Gender switch
                {
                    NPC.male => Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633"),
                    NPC.female => Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634"),
                    _ => ""
                };
                try
                {
                    Game1.player.changeFriendship(250, Game1.getCharacterFromName(npc.Name));
                }
                catch (Exception)
                {
                }
                Game1.player.dancePartner.Value = npc;
                npc.setNewDialogue(s);

                foreach (NPC actor in festival.actors)
                {
                    if (actor.CurrentDialogue?.Count > 0 && actor.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                        actor.CurrentDialogue.Clear();
                }

                // Okay, looks like I need to fix the current dialog box
                Game1.activeClickableMenu = new DialogueBox(new Dialogue(s, npc) { removeOnNextMove = false });
            }
        }
    }
}
