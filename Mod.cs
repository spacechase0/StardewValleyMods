using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using Netcode;
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

            // Before we populate the route list, we need to fix doors from conditional CP patches and such.
            // This can be removed once SMAPI 3.0 comes out.
            foreach (var loc in Game1.locations)
            {
                loc.doors.Clear();
                for (int x = 0; x < loc.map.Layers[0].LayerWidth; ++x)
                {
                    for (int y = 0; y < loc.map.Layers[0].LayerHeight; ++y)
                    {
                        if (loc.map.GetLayer("Buildings").Tiles[x, y] != null)
                        {
                            PropertyValue propertyValue3 = (PropertyValue)null;
                            loc.map.GetLayer("Buildings").Tiles[x, y].Properties.TryGetValue("Action", out propertyValue3);
                            if (propertyValue3 != null && propertyValue3.ToString().Contains("Warp"))
                            {
                                string[] strArray = propertyValue3.ToString().Split(' ');
                                if (strArray[0].Equals("WarpCommunityCenter"))
                                    loc.doors.Add(new Point(x, y), new NetString("CommunityCenter"));
                                else if ((!loc.name.Equals((object)"Mountain") || x != 8 || y != 20) && strArray.Length > 2)
                                    loc.doors.Add(new Point(x, y), new NetString(strArray[3]));
                            }
                        }
                    }
                }
            }

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
                        Monitor.Log("Exception doing schedule for NPC " + npc.Name + ": " + e, LogLevel.Error);
                    }
                }
            }
        }
    }
}
