using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.ObjectModel;

namespace CustomNPCFixes
{
    public class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = Monitor;

            // We need to make sure to run after Content Patcher, which registers its events after its first update tick.
            helper.Events.GameLoop.UpdateTicked += onUpdate;
        }

        private bool firstTick = true;
        private void onUpdate(object sender, UpdateTickedEventArgs e)
        {
            if ( firstTick )
            {
                firstTick = false;

                Helper.Events.GameLoop.SaveCreated += doNpcFixes;
                Helper.Events.GameLoop.SaveLoaded += doNpcFixes;

                Helper.Events.GameLoop.DayStarted += (s, a) => { Game1.fixProblems(); fixSchedules(); }; // See comments in doNpcFixes. This handles conditional spawning.
            }
        }

        public void doNpcFixes(object sender, EventArgs args)
        {
            // This needs to be called again so that custom NPCs spawn in locations added after the original call
            Game1.fixProblems();

            // Similarly, this needs to be called again so that pathing works.
            NPC.populateRoutesFromLocationToLocationList();

            // Schedules for new NPCs don't work the first time.
            fixSchedules();
        }

        private void fixSchedules()
        {
            foreach (var npc in Utility.getAllCharacters())
            {
                if (npc.Schedule == null)
                {
                    try
                    {
                        npc.Schedule = npc.getSchedule(Game1.dayOfMonth);
                        npc.checkSchedule(Game1.timeOfDay);
                    }
                    catch (Exception e)
                    {
                        Log.error("Exception doing schedule for NPC " + npc.Name + ": " + e);
                    }
                }
            }
        }
    }
}
