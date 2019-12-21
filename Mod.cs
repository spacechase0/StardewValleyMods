using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

                Helper.Events.GameLoop.DayStarted += (s, a) => { spawnNpcs(); fixSchedules(); }; // See comments in doNpcFixes. This handles conditional spawning.
            }
        }

        public void doNpcFixes(object sender, EventArgs args)
        {
            // This needs to be called again so that custom NPCs spawn in locations added after the original call
            //Game1.fixProblems();
            spawnNpcs();

            // Similarly, this needs to be called again so that pathing works.
            NPC.populateRoutesFromLocationToLocationList();

            // Schedules for new NPCs don't work the first time.
            fixSchedules();
        }

        private void spawnNpcs()
        {
            List<NPC> pooledList = Utility.getPooledList();
            try
            {
                Utility.getAllCharacters(pooledList);
                Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
                foreach (string key in dictionary.Keys)
                {
                    bool flag = false;
                    if (!(key == "Kent") || Game1.player.friendshipData.ContainsKey(key))
                    {
                        foreach (NPC npc in pooledList)
                        {
                            if (npc.isVillager() && npc.Name.Equals(key))
                            {
                                flag = true;
                                if ((bool)((NetFieldBase<bool, NetBool>)npc.datable))
                                {
                                    if (npc.getSpouse() == null)
                                    {
                                        string str = dictionary[key].Split('/')[10].Split(' ')[0];
                                        if (npc.DefaultMap != str)
                                        {
                                            if (!npc.DefaultMap.ToLower().Contains("cabin"))
                                            {
                                                if (!npc.DefaultMap.Equals("FarmHouse"))
                                                    break;
                                            }
                                            Console.WriteLine("Fixing " + npc.Name + " who was improperly divorced and left stranded");
                                            npc.PerformDivorce();
                                            break;
                                        }
                                        break;
                                    }
                                    break;
                                }
                                break;
                            }
                        }
                        if (!flag)
                        {
                            try
                            {
                                Game1.getLocationFromName(dictionary[key].Split('/')[10].Split(' ')[0]).addCharacter(new NPC(new AnimatedSprite("Characters\\" + key, 0, 16, 32), new Vector2((float)(Convert.ToInt32(dictionary[key].Split('/')[10].Split(' ')[1]) * 64), (float)(Convert.ToInt32(dictionary[key].Split('/')[10].Split(' ')[2]) * 64)), dictionary[key].Split('/')[10].Split(' ')[0], 0, key, (Dictionary<int, int[]>)null, Game1.content.Load<Texture2D>("Portraits\\" + key), false, (string)null));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
            }
            finally
            {
                Utility.returnPooledList(pooledList);
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
