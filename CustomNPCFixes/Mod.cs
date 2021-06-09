using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CustomNPCFixes
{
    public class Mod : StardewModdingAPI.Mod
    {
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            // We need to make sure to run after Content Patcher, which registers its events after its first update tick.
            helper.Events.GameLoop.UpdateTicked += this.onUpdate;
        }

        private bool firstTick = true;
        private void onUpdate(object sender, UpdateTickedEventArgs e)
        {
            if (this.firstTick)
            {
                this.firstTick = false;

                this.Helper.Events.GameLoop.SaveCreated += this.doNpcFixes;
                this.Helper.Events.GameLoop.SaveLoaded += this.doNpcFixes;

                this.Helper.Events.GameLoop.DayStarted += (s, a) => { this.spawnNpcs(); this.fixSchedules(); }; // See comments in doNpcFixes. This handles conditional spawning.
            }
        }

        public void doNpcFixes(object sender, EventArgs args)
        {
            // This needs to be called again so that custom NPCs spawn in locations added after the original call
            //Game1.fixProblems();
            this.spawnNpcs();

            // Similarly, this needs to be called again so that pathing works.
            NPC.populateRoutesFromLocationToLocationList();

            // Schedules for new NPCs don't work the first time.
            this.fixSchedules();
        }

        private void spawnNpcs()
        {
            List<NPC> allCharacters = Utility.getPooledList();
            try
            {
                Utility.getAllCharacters(allCharacters);
                Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                foreach (string s in NPCDispositions.Keys)
                {
                    bool found = false;
                    if ((s == "Kent" && Game1.year <= 1) || (s == "Leo" && !Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy")))
                    {
                        continue;
                    }
                    foreach (NPC n2 in allCharacters)
                    {
                        if (!n2.isVillager() || !n2.Name.Equals(s))
                        {
                            continue;
                        }
                        found = true;
                        if ((bool)n2.datable && n2.getSpouse() == null)
                        {
                            string defaultMap = NPCDispositions[s].Split('/')[10].Split(' ')[0];
                            if (n2.DefaultMap != defaultMap && (n2.DefaultMap.ToLower().Contains("cabin") || n2.DefaultMap.Equals("FarmHouse")))
                            {
                                Console.WriteLine("Fixing " + n2.Name + " who was improperly divorced and left stranded");
                                n2.PerformDivorce();
                            }
                        }
                        break;
                    }
                    if (!found)
                    {
                        try
                        {
                            Game1.getLocationFromName(NPCDispositions[s].Split('/')[10].Split(' ')[0]).addCharacter(new NPC(new AnimatedSprite("Characters\\" + NPC.getTextureNameForCharacter(s), 0, 16, 32), new Vector2(Convert.ToInt32(NPCDispositions[s].Split('/')[10].Split(' ')[1]) * 64, Convert.ToInt32(NPCDispositions[s].Split('/')[10].Split(' ')[2]) * 64), NPCDispositions[s].Split('/')[10].Split(' ')[0], 0, s, null, Game1.content.Load<Texture2D>("Portraits\\" + NPC.getTextureNameForCharacter(s)), eventActor: false));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            finally
            {
                Utility.returnPooledList(allCharacters);
            }
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
