using System;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;

namespace RaiseJunimos
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Helper.ConsoleCommands.Add("junimos_add", "...", OnJunimosAdd);

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
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
