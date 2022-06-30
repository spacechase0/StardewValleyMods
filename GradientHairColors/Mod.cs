using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            Helper.ConsoleCommands.Add("dyehair", "...", DoHairCommand);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            fs = Helper.ModRegistry.GetApi<IFashionSenseApi>("PeacefulEnd.FashionSense");
        }

        private void DoHairCommand(string cmd, string[] args)
        {
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

            string appId = fs.GetCurrentAppearanceId(IFashionSenseApi.Type.Hair).Value;

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

            fs.SetAppearanceTexture(appId, tex, ModManifest);
            if (Game1.player.modData.ContainsKey(ModManifest.UniqueID))
                Game1.player.modData.Remove(ModManifest.UniqueID);
            Game1.player.modData.Add(ModManifest.UniqueID, $"{h1} {s1} {v1} {h2} {s2} {v2}");
        }
    }
}
