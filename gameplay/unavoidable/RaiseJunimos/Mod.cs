using System;
using System.Collections.Generic;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace RaiseJunimos
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.ConsoleCommands.Add("junimos_add", "...", OnJunimosAdd);

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
        }

        private void OnJunimosAdd(string cmd, string[] args)
        {
            if (Game1.currentLocation is not JunimoWoods jw)
            {
                Log.Error("Must be in the junimo woods");
                return;
            }

            var rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2( 0 * Game1.tileSize, Game1.tileSize );
            rj.SpeedStat.Value = RaisableJunimo.StatData.StatCap;
            rj.StrengthStat.Value = 0;
            rj.MindStat.Value = 0;
            rj.MagicStat.Value = 0;
            rj.StaminaStat.Value = 0;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(1*Game1.tileSize, Game1.tileSize);
            rj.SpeedStat.Value = 0;
            rj.StrengthStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MindStat.Value = 0;
            rj.MagicStat.Value = RaisableJunimo.StatData.StatCap / 5 * 1;
            rj.StaminaStat.Value = RaisableJunimo.StatData.StatCap / 5 * 1;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(2 * Game1.tileSize, Game1.tileSize);
            rj.SpeedStat.Value = 0;
            rj.StrengthStat.Value = 0;
            rj.MindStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MagicStat.Value = RaisableJunimo.StatData.StatCap / 5 * 2;
            rj.StaminaStat.Value = RaisableJunimo.StatData.StatCap / 5 * 2;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(3 * Game1.tileSize, Game1.tileSize);
            rj.SpeedStat.Value = RaisableJunimo.StatData.StatCap;
            rj.StrengthStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MindStat.Value = 0;
            rj.MagicStat.Value = RaisableJunimo.StatData.StatCap / 5 * 3;
            rj.StaminaStat.Value = RaisableJunimo.StatData.StatCap / 5 * 3;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(4 * Game1.tileSize, Game1.tileSize);
            rj.SpeedStat.Value = RaisableJunimo.StatData.StatCap;
            rj.StrengthStat.Value = 0;
            rj.MindStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MagicStat.Value = RaisableJunimo.StatData.StatCap / 5 * 4;
            rj.StaminaStat.Value = RaisableJunimo.StatData.StatCap / 5 * 4;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(5 * Game1.tileSize, Game1.tileSize);
            rj.SpeedStat.Value = RaisableJunimo.StatData.StatCap;
            rj.StrengthStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MindStat.Value = RaisableJunimo.StatData.StatCap;
            rj.MagicStat.Value = RaisableJunimo.StatData.StatCap / 5 * 5;
            rj.StaminaStat.Value = RaisableJunimo.StatData.StatCap / 5 * 5;
            jw.junimos.Add(rj);
            rj = new RaisableJunimo();
            rj.Position = Game1.player.Position - new Microsoft.Xna.Framework.Vector2(6 * Game1.tileSize, Game1.tileSize);
            jw.junimos.Add(rj);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(JunimoWoods));
            sc.RegisterSerializerType(typeof(RaisableJunimo));
            sc.RegisterSerializerType(typeof(RaisableJunimo.StatData));
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button.IsActionButton())
            {
                List<RaisableJunimo> junimos = new();
                if (Game1.currentLocation is JunimoWoods jw)
                {
                    junimos.AddRange(jw.junimos);
                }
                // TODO: add junimos following farmers here

                RaisableJunimo junimo = null;
                foreach (var check in junimos)
                {
                    if (check.GetBoundingBox().Contains(e.Cursor.AbsolutePixels))
                    {
                        junimo = check;
                        break;
                    }
                }

                if (junimo != null)
                {
                    List<Response> responses = new();
                    responses.Add(new Response("info", I18n.Action_Info()));
                    if ( !junimo.WasPetToday )
                        responses.Add(new Response("pet", I18n.Action_Pet()));
                    if ((Game1.player.CurrentItem is Hat && junimo.Hat == null) || (junimo.GiftsGivenToday < 3 && Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBeGivenAsGift()))
                        responses.Add(new Response("gift", I18n.Action_Gift()));
                    if (junimo.Hat != null)
                        responses.Add(new Response("take-hat", I18n.Action_TakeHat()));
                    //only show this one if  they aren't following anyone
                    responses.Add(new Response("follow", I18n.Action_Follow()));
                    // only show this one if they are following you
                    //responses.Add(new Response("stop-follow", I18n.Action_StopFollow()));
                    responses.Add(new Response("rename", I18n.Action_Rename()));
                    responses.Add(new Response("cancel", I18n.Action_Cancel()));
                    Game1.currentLocation.createQuestionDialogue(I18n.Action_Prompt( junimo.Name ), responses.ToArray(), "raising-junimos");
                    Game1.currentLocation.afterQuestion = (who, resp) =>
                    {
                        switch (resp)
                        {
                            case "info": junimo.ShowInfo(); break;
                            case "pet": junimo.Pet(Game1.player); break;
                            case "gift":
                                if (Game1.player.CurrentItem is Hat hat)
                                {
                                    junimo.Hat = hat;
                                    Game1.player.reduceActiveItemByOne();
                                }
                                else
                                    junimo.GiveGift(Game1.player);
                                break;
                            case "take-hat":
                                Game1.currentLocation.debris.Add(new Debris(junimo.Hat, junimo.GetBoundingBox().Center.ToVector2()));
                                junimo.Hat = null;
                                break;
                            case "follow":
                                // todo
                                break;
                            case "stop-follow":
                                // todo
                                break;
                            case "rename": junimo.Rename(); break;
                        }
                    };
                }
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.NewLocation.Name == "Woods")
                e.NewLocation.setMapTile(10, 7, 1967, "Buildings", "Warp 10 8 Custom_JunimoWoods", 1);
        }

        private void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage == LoadStage.CreatedInitialLocations || e.NewStage == LoadStage.SaveAddedLocations)
            {
                Game1.locations.Add(new JunimoWoods(Helper.ModContent));
            }
        }
    }
}
