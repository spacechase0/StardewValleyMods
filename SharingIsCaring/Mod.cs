using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace SharingIsCaring
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        private HashSet<string> alreadyShared = new();
        private Random shuffle = new();

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = this.Monitor;

            helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            alreadyShared.Clear();
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsMultipleOf(10))
                return;

            List<NPC> npcs = new();
            foreach (var npc in Game1.currentLocation.characters)
            {
                if (!npc.isVillager() || alreadyShared.Contains(npc.Name))
                    continue;

                float dist = Vector2.Distance(Game1.player.Position, npc.Position);
                if ( dist <= 300 && Game1.random.Next( (int)( 15 + dist / 10 ) * 10 )/20 == 0 )
                    npcs.Add(npc);
            }
            if (npcs.Count == 0)
                return;

            Utility.Shuffle(shuffle, npcs);

            foreach (var npc in npcs)
            {
                List<Item> choices = new();
                foreach (var item in Game1.player.Items)
                {
                    if (item == null) continue;
                    int taste = npc.getGiftTasteForThisItem(item);
                    if (taste == NPC.gift_taste_love || taste == NPC.gift_taste_like)
                        choices.Add(item);
                }

                if (choices.Count == 0)
                    continue;
                Utility.Shuffle(shuffle, choices);

                var choice = choices[0];
                var clone = choice.getOne();
                if (choice.Stack == 1)
                {
                    for (int i = 0; i < Game1.player.Items.Count; ++i)
                    {
                        if (Game1.player.Items[i] == choice)
                            Game1.player.Items[i] = null;
                    }
                    // This line might erase a bigger stack if you have multiple of the same item but one of it in another stack
                    //Game1.player.Items[Game1.player.getIndexOfInventoryItem(choice)] = null;
                }
                else
                    choice.Stack--;


                Game1.activeClickableMenu = new DialogueBox(new Dialogue(npc, "share", "Wow, a " + clone.DisplayName + "! Thanks!"));

                return;
            }
        }
    }
}
