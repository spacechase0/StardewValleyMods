using StardewModdingAPI;
using StardewValley;

namespace AnotherHungerMod.Framework
{
    internal class SaveData
    {
        public double Fullness { get; set; } = 100;
        public bool FedSpouseMeal = false;

        public void SyncToHost()
        {
            if (Context.IsMainPlayer)
                Mod.Instance.Helper.Data.WriteSaveData($"spacechase0.AnotherHungerMod.{Game1.player.UniqueMultiplayerID}", Mod.Data);
            else
            {
                Mod.Instance.Helper.Multiplayer.SendMessage(Mod.Data, Mod.MsgHungerData, null, new[] { Game1.MasterPlayer.UniqueMultiplayerID });
            }
        }
    }
}
