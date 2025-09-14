using System;
using System.Collections.Generic;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI.Events;
using StardewValley;

namespace FlourFestival
{
    public class State
    {
        public List<Character>
    }

    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;

        internal static IJsonAssetsApi ja;

        public override void Entry(StardewModdingAPI.IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Festivals\\spring23"))
            {
                e.LoadFrom(() => GetFestivalData(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps\\Forest-Flour"))
            {
                e.LoadFromModFile<xTile.Map>("assets/Forest.tmx", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data\\Festivals\\FestivalDates"))
            {
                e.Edit((asset) => asset.AsDictionary<string, string>().Data.Add("spring23", I18n.Festival_Name()));
            }
        }

        private Dictionary<string, string> GetFestivalData()
        {
            var data = new Dictionary<string, string>
            {
                [ "name" ] = I18n.Festival_Name(),
                [ "conditions" ] = "Forest/900 1400",
                [ "set-up" ] = "event1/22 22/farmer 76 17 1/changeToTemporaryMap Forest-Flour/loadActors Set-Up/advancedMove Vincent true -6 0 0 -1 6 0 0 1/advancedMove Jas true 1 0 0 1 -6 0 0 -1 5 0/advancedMove Pam true 2 25000 0 -3 4 0 4 25000 -4 0 0 3/advancedMove Haley true 0 1 2 3000 2 0 0 -1 -2 0 3 500 4 500 1 500 2 500 2 0 1 500 2 500 3 500 4 500 0 2 0 -2 -2 0/advancedMove Caroline true 7 0 0 1 1 18000 0 -1 -7 0 3 18000/advancedMove Willy true 2 25000 -4 0 0 -12 4 0 0 -1 4 30000 0 1 -4 0 0 12 4 0/playerControl flourFestival",
                [ "mainEvent" ] = $@"pause 500/playMusic none/pause 500/globalFade/viewport -1000 -1000/loadActors MainEvent/warpContestants/viewport 9 24 true unfreeze/pause 2000/message ""{I18n.Event_Instructions()}""/speak Lewis ""{I18n.Event_LewisStart_0()}""/speak Lewis ""{I18n.Event_LewisStart_1()}""/speak Lewis ""{I18n.Event_LewisStart_2()}""/waitForOtherPlayers actualEvent/playSound whistle/playMusic cowboy_outlawsong/playerControl flourFestivalEvent",
                [ "afterEvent" ] = "pause 100/playSound whisle/waitForOtherPlayers endContest/pause 1000/globalFade/viewport -1000 -1000/playMusic event1/loadActors PostEvent/warpContestantsFinish/pause 1000/viewport 9 24 true/pause 2000/speak Lewis \"{{winDialog}}\"/awardFlourPrice/pause 600/viewport move 1 0 500/pause 2000/globalFade/viewport -1000 -1000/waitForOtherPlayers festivalEnd/end",
            };

            foreach (var translation in Helper.Translation.GetTranslations())
            {
                if (translation.Key.StartsWith("npc."))
                    data[translation.Key.Substring("npc.".Length)] = translation.ToString();
            }

            return data;
        }
    }
}
