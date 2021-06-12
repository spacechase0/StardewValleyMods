using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace ManaBar.Framework
{
    internal class MultiplayerSaveData
    {
        public const string MsgData = "spacechase0.ManaBar.Data";
        public const string MsgMinidata = "spacechase0.ManaBar.MiniData";

        public static string OldFilePath => Path.Combine(Constants.CurrentSavePath, "magic0.2.json");
        public static string SaveKey => "spacechase0.ManaBar.Mana";

        public class PlayerData
        {
            public int Mana = 0;
            public int ManaCap = 0;
        }
        public Dictionary<long, PlayerData> Players = new();

        internal static JsonSerializerSettings NetworkSerializerSettings { get; } = new()
        {
            Formatting = Formatting.None,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        internal void SyncMineFull()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(1);
                writer.Write(Game1.player.UniqueMultiplayerID);
                writer.Write(JsonConvert.SerializeObject(this.Players[Game1.player.UniqueMultiplayerID], MultiplayerSaveData.NetworkSerializerSettings));
                SpaceCore.Networking.BroadcastMessage(MultiplayerSaveData.MsgData, stream.ToArray());
            }
        }

        internal void SyncMineMini()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(this.Players[Game1.player.UniqueMultiplayerID].Mana);
                writer.Write(this.Players[Game1.player.UniqueMultiplayerID].ManaCap);
                SpaceCore.Networking.BroadcastMessage(MultiplayerSaveData.MsgMinidata, stream.ToArray());
            }
        }
    }
}
