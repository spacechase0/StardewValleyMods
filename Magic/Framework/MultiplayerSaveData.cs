using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;

namespace Magic.Framework
{
    internal class MultiplayerSaveData
    {
        public static string FilePath => Path.Combine(Constants.CurrentSavePath, "magic0.2.json");

        public class PlayerData
        {
            public int FreePoints = 0;

            public SpellBook SpellBook = new();
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
                SpaceCore.Networking.BroadcastMessage(Magic.MsgData, stream.ToArray());
            }
        }
    }
}
