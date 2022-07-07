using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceShared;
using SpaceShared.APIs;
using StardewValley;

namespace GradientHairColors
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private IFashionSenseApi fs;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            helper.Events.Multiplayer.ModMessageReceived += this.Multiplayer_ModMessageReceived;

            Helper.ConsoleCommands.Add("dyehair", "...", DoHairCommand);
        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID && e.Type == ModManifest.UniqueID)
            {
                var kvp = e.ReadAs<KeyValuePair<string, Color[]>>();
                fs.ResetAppearanceTexture(kvp.Key, ModManifest);
                if (Game1.IsMasterGame)
                    DoHair(kvp.Value[0], kvp.Value[1], kvp.Key);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Dictionary<string, Color[]> apps = new();
            if (Game1.MasterPlayer.modData.TryGetValue(ModManifest.UniqueID, out string dataStr))
            {
                apps = JsonConvert.DeserializeObject<Dictionary<string, Color[]>>(dataStr);
            }

            foreach (var app in apps)
            {
                DoHair(app.Value[0], app.Value[1], app.Key, false);
            }

            /*
            if (!Game1.MasterPlayer.modData.ContainsKey(ModManifest.UniqueID))
                Game1.MasterPlayer.modData.Add(ModManifest.UniqueID, JsonConvert.SerializeObject(new Dictionary<string, Color[]>()));
            */
            //Game1.MasterPlayer.modData.FieldDict[ModManifest.UniqueID].fieldChangeEvent += this.Mod_fieldChangeEvent;

            Game1.MasterPlayer.modData.OnValueAdded += this.ModData_OnValueAdded;
            Game1.MasterPlayer.modData.OnValueTargetUpdated += this.ModData_OnValueTargetUpdated; ;
            Game1.MasterPlayer.modData.OnConflictResolve += this.ModData_OnConflictResolve;
        }

        private void ModData_OnConflictResolve(string key, Netcode.NetString rejected, Netcode.NetString accepted)
        {
            /*
            if (key.Contains("spacechase0"))
                Log.Debug("ocr:" + key + " " + rejected.Value + " " + accepted.Value);
            */
            if (key == ModManifest.UniqueID)
                DoModData(accepted.Value);
        }

        private void ModData_OnValueTargetUpdated(string key, string old_target_value, string new_target_value)
        {
            /*
            if (key.Contains("spacechase0"))
                Log.Debug("ovtu:" + key + " " + old_target_value+" "+ new_target_value);
            */
            if (key == ModManifest.UniqueID)
                DoModData(new_target_value);
        }

        private void ModData_OnValueAdded(string key, string value)
        {
            /*
            if (key.Contains("spacechase0"))
                Log.Debug("ova:" + key + " " + value);
            */
            if (key == ModManifest.UniqueID)
                DoModData(value);
        }

        private void Mod_fieldChangeEvent(Netcode.NetString field, string oldValue, string newValue)
        {
            //Log.Debug("fce:" + " " + oldValue + " " + newValue);
            DoModData(newValue);
        }

        private void DoModData(string value)
        {
            var apps = JsonConvert.DeserializeObject<Dictionary<string, Color[]>>(value);
            foreach (var app in apps)
            {
                DoHair(app.Value[0], app.Value[1], app.Key, false);
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            fs = Helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");
        }

        private void DoHairCommand(string cmd, string[] args)
        {
            if (args.Length < 6)
            {
                Log.Info("Usage: dyehair hue1 saturation1 value1 hue2 saturation2 value2");

                var r_ = fs.GetCurrentAppearanceId(IFashionSenseApi.Type.Hair);
                if (r_.Key)
                {
                    Log.Info("Resetting hair color...");

                    fs.ResetAppearanceTexture(r_.Value, ModManifest);

                    Dictionary<string, Color[]> apps = new();
                    if (Game1.MasterPlayer.modData.TryGetValue(ModManifest.UniqueID, out string dataStr))
                    {
                        apps = JsonConvert.DeserializeObject<Dictionary<string, Color[]>>(dataStr);
                        Game1.MasterPlayer.modData.Remove(ModManifest.UniqueID);
                    }
                    apps.Remove(r_.Value);
                    Game1.MasterPlayer.modData.Add(ModManifest.UniqueID, JsonConvert.SerializeObject(apps));
                    //Log.Debug("set to " + Game1.MasterPlayer.modData[ModManifest.UniqueID]);
                    if (!Game1.IsMasterGame)
                    {
                        Helper.Multiplayer.SendMessage(new KeyValuePair<string, Color[]>(r_.Value, new Color[0]), ModManifest.UniqueID, new string[] { ModManifest.UniqueID });
                    }
                }
                return;
            }

            float h1 = 175, s1 = 100, v1 = 100;
            float h2 = 250, s2 = 100, v2 = 100;

            if (args.Length >= 6)
            {
                h1 = float.Parse(args[0]);
                s1 = float.Parse(args[1]);
                v1 = float.Parse(args[2]);
                h2 = float.Parse(args[3]);
                s2 = float.Parse(args[4]);
                v2 = float.Parse(args[5]);
            }

            Color a = Util.ColorFromHsv(h1, s1 / 100, v1 / 100);
            Color b = Util.ColorFromHsv(h2, s2 / 100, v2 / 100);

            var r = fs.GetCurrentAppearanceId(IFashionSenseApi.Type.Hair);
            if (!r.Key)
            {
                Log.Info("Requires a FashionSense hair type.");
                return;
            }
            string appId = r.Value;

            DoHair(a, b, appId);
        }
        private void DoHair( Color a, Color b, string appId, bool update = true )
        {
            fs.ResetAppearanceTexture(appId, ModManifest);
            var tex = fs.GetAppearanceTexture(appId).Value;//Game1.content.Load<Texture2D>("Characters\\Farmer\\hairstyles");
            var cols = tex.Data;

            //for (int ix = 0; ix < tex.Width / 16; ++ix)
            {
                //for (int iy = 0; iy < tex.Height / 32; ++iy)
                {
                    int ix = 0, iy = 0;

                    List<int> colSteps = new();
                    //for (int jx = ix * 16; jx < ix * 16 + 16; ++jx)
                    for (int jx = 0; jx < tex.Width; ++jx)
                    {
                        //for (int jy = iy * 32; jy < iy * 32 + 32; ++jy)
                        for (int jy = 0; jy < tex.Height; ++jy)
                        {
                            int ind = jx + jy * tex.Width;
                            if (cols[ind].A != 0 && !colSteps.Contains(cols[ind].R))
                                colSteps.Add(cols[ind].R);
                        }
                    }
                    colSteps.Sort();

                    int stepCount = colSteps.Count;

                    Color[] grad = new Color[stepCount];
                    for (int i = 0; i < stepCount; ++i)
                    {
                        /*
                        float h = h2 + ((h1 - h2) * (i / 256f));
                        float s = s2 + ((s1 - s2) * (i / 256f));
                        float v = v2 + ((v1 - v2) * (i / 256f));
                        */
                        grad[i] = Color.Lerp(a, b, i / (float)stepCount);// Util.ColorFromHsv(h, s / 100f, v / 100f);
                    }

                    //for (int jx = ix * 16; jx < ix * 16 + 16; ++jx)
                    for (int jx = 0; jx < tex.Width; ++jx)
                    {
                        //for (int jy = iy * 32; jy < iy * 32 + 32; ++jy)
                        for (int jy = 0; jy < tex.Height; ++jy)
                        {
                            int ind = jx + jy * tex.Width;
                            if (cols[ind].A != 0)
                            {
                                byte tmp = cols[ind].A;
                                cols[ind] = grad[colSteps.IndexOf(cols[ind].R)];
                                cols[ind].A = tmp;
                            }
                        }
                    }
                }
            }

            fs.SetAppearanceTexture(appId, tex, ModManifest, true);
            if (update)
            {
                Dictionary<string, Color[]> apps = new();
                if (Game1.MasterPlayer.modData.TryGetValue(ModManifest.UniqueID, out string dataStr))
                {
                    apps = JsonConvert.DeserializeObject<Dictionary<string, Color[]>>(dataStr);
                    Game1.MasterPlayer.modData.Remove(ModManifest.UniqueID);
                }
                apps[appId] = new Color[2] { a, b };
                Game1.MasterPlayer.modData.Add(ModManifest.UniqueID, JsonConvert.SerializeObject(apps));
                //Log.Debug("set to " + Game1.MasterPlayer.modData[ModManifest.UniqueID]);
                if (!Game1.IsMasterGame)
                {
                    Helper.Multiplayer.SendMessage(new KeyValuePair<string, Color[]>(appId, apps[appId]), ModManifest.UniqueID, new string[] { ModManifest.UniqueID });
                }
            }
        }
    }
}
