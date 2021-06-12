using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Network;

namespace ManaBar
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        public static MultiplayerSaveData Data { get; private set; } = new();

        private static Texture2D ManaBg;
        private static Texture2D ManaFg;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Command.Register("player_addmana", Mod.AddManaCommand);
            Command.Register("player_setmaxmana", Mod.SetMaxManaCommand);

            helper.Events.GameLoop.DayStarted += Mod.OnDayStarted;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saved += this.OnSaved;
            helper.Events.Display.RenderedHud += Mod.OnRenderedHud;

            SpaceCore.Networking.RegisterMessageHandler(MultiplayerSaveData.MsgData, Mod.OnNetworkData);
            SpaceCore.Networking.RegisterMessageHandler(MultiplayerSaveData.MsgMinidata, Mod.OnNetworkMiniData);
            SpaceEvents.ServerGotClient += Mod.OnClientConnected;

            Mod.ManaBg = helper.Content.Load<Texture2D>("assets/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            Mod.ManaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Mod.ManaFg.SetData(new[] { manaCol });
        }

        private static void AddManaCommand(string[] args)
        {
            Game1.player.AddMana(int.Parse(args[0]));
        }
        private static void SetMaxManaCommand(string[] args)
        {
            Game1.player.SetMaxMana(int.Parse(args[0]));
        }

        private IApi Api;
        public override object GetApi()
        {
            if (this.Api == null)
                this.Api = new Api();
            return this.Api;
        }

        private static void OnNetworkData(IncomingMessage msg)
        {
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                Mod.Data.Players[msg.Reader.ReadInt64()] = JsonConvert.DeserializeObject<MultiplayerSaveData.PlayerData>(msg.Reader.ReadString());
            }
        }

        private static void OnNetworkMiniData(IncomingMessage msg)
        {
            Mod.Data.Players[msg.FarmerID].Mana = msg.Reader.ReadInt32();
            Mod.Data.Players[msg.FarmerID].ManaCap = msg.Reader.ReadInt32();
        }

        private static void OnClientConnected(object sender, EventArgsServerGotClient args)
        {
            if (!Mod.Data.Players.ContainsKey(args.FarmerID))
                Mod.Data.Players[args.FarmerID] = new MultiplayerSaveData.PlayerData();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Mod.Data.Players.Count);
                foreach (var entry in Mod.Data.Players)
                {
                    writer.Write(entry.Key);
                    writer.Write(JsonConvert.SerializeObject(entry.Value, MultiplayerSaveData.NetworkSerializerSettings));
                }
                SpaceCore.Networking.BroadcastMessage(MultiplayerSaveData.MsgData, stream.ToArray());
            }
        }

        public static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp || Game1.player.GetMaxMana() == 0)
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 manaPos = new Vector2(20, Game1.uiViewport.Height - Mod.ManaBg.Height * 4 - 20);
            b.Draw(Mod.ManaBg, manaPos, new Rectangle(0, 0, Mod.ManaBg.Width, Mod.ManaBg.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.GetCurrentMana() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float perc = Game1.player.GetCurrentMana() / (float)Game1.player.GetMaxMana();
                int h = (int)(targetArea.Height * perc);
                targetArea.Y += targetArea.Height - h;
                targetArea.Height = h;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)manaPos.X;
                targetArea.Y += (int)manaPos.Y;
                b.Draw(Mod.ManaFg, targetArea, new Rectangle(0, 0, 1, 1), Color.White);

                if (Game1.getOldMouseX() >= (double)targetArea.X && Game1.getOldMouseY() >= (double)targetArea.Y && Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, Game1.player.GetCurrentMana()).ToString() + "/" + Game1.player.GetMaxMana(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.AddMana(Game1.player.GetMaxMana());
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                {
                    Log.Info($"Loading save data");
                    Mod.Data = this.Helper.Data.ReadSaveData<MultiplayerSaveData>(MultiplayerSaveData.SaveKey);
                    if (Mod.Data == null)
                    {
                        if (File.Exists(MultiplayerSaveData.OldFilePath))
                        {
                            Mod.Data = JsonConvert.DeserializeObject<MultiplayerSaveData>(File.ReadAllText(MultiplayerSaveData.OldFilePath));
                        }
                    }
                    if (Mod.Data == null)
                        Mod.Data = new MultiplayerSaveData();

                    if (!Mod.Data.Players.ContainsKey(Game1.player.UniqueMultiplayerID))
                        Mod.Data.Players[Game1.player.UniqueMultiplayerID] = new MultiplayerSaveData.PlayerData();
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Exception loading save data: {ex}");
            }
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Log.Info($"Saving save data...");
                this.Helper.Data.WriteSaveData(MultiplayerSaveData.SaveKey, Mod.Data);
            }
        }
    }
}
