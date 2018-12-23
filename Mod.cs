using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using Magic.Other;
using Newtonsoft.Json;

namespace Magic
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        public static MultiplayerSaveData Data { get; private set; } = new MultiplayerSaveData();
        public static Configuration Config { get; private set; }
        
        internal static JsonAssetsApi ja;

        public override void Entry(IModHelper helper)
        {
            instance = this;

            Config = Helper.ReadConfig<Configuration>();

            GameEvents.FirstUpdateTick += firstUpdate;
            SaveEvents.AfterLoad += afterLoad;
            SaveEvents.AfterSave += afterSave;

            Magic.init();
        }

        private void firstUpdate(object sender, EventArgs args)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsApi>("spacechase0.JsonAssets");
            if (api == null)
            {
                Log.error("No Json Assets API???");
                return;
            }
            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }

        private void afterLoad(object sender, EventArgs args)
        {
            try
            {
                if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                {
                    Log.info($"Loading save data (\"{SaveData.FilePath}\")...");
                    var oldData = File.Exists(SaveData.FilePath)
                        ? JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(SaveData.FilePath))
                        : null;
                    Data = File.Exists(MultiplayerSaveData.FilePath)
                        ? JsonConvert.DeserializeObject<MultiplayerSaveData>(File.ReadAllText(MultiplayerSaveData.FilePath))
                        : new MultiplayerSaveData();

                    if (oldData != null && !Data.players.ContainsKey(Game1.MasterPlayer.UniqueMultiplayerID))
                    {
                        var player = new MultiplayerSaveData.PlayerData
                        {
                            mana = oldData.mana,
                            manaCap = oldData.manaCap,
                            magicLevel = oldData.magicLevel,
                            magicExp = oldData.magicExp,
                            freePoints = oldData.freePoints,
                            spellBook = oldData.spellBook
                        };

                        Data.players[Game1.MasterPlayer.UniqueMultiplayerID] = player;
                    }
                    
                    if ( !Data.players.ContainsKey( Game1.player.UniqueMultiplayerID ) )
                        Data.players[Game1.player.UniqueMultiplayerID] = new MultiplayerSaveData.PlayerData();
                }
            }
            catch ( Exception e )
            {
                Log.warn("Exception loading save data: " + e);
            }
        }
        
        private void afterSave(object sender, EventArgs args)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Log.info($"Saving save data (\"{MultiplayerSaveData.FilePath}\")...");
                File.WriteAllText(MultiplayerSaveData.FilePath, JsonConvert.SerializeObject(Data));
            }
        }
    }
}
