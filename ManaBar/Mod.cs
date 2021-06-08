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
        public static Mod instance;
        public static MultiplayerSaveData Data { get; private set; } = new MultiplayerSaveData();

        private static Texture2D manaBg;
        private static Texture2D manaFg;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            Command.register("player_addmana", addManaCommand);
            Command.register("player_setmaxmana", setMaxManaCommand);

            helper.Events.GameLoop.DayStarted += onDayStarted;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.GameLoop.Saved += onSaved;
            helper.Events.Display.RenderedHud += onRenderedHud;

            SpaceCore.Networking.RegisterMessageHandler(MultiplayerSaveData.MSG_DATA, onNetworkData);
            SpaceCore.Networking.RegisterMessageHandler(MultiplayerSaveData.MSG_MINIDATA, onNetworkMiniData);
            SpaceEvents.ServerGotClient += onClientConnected;

            manaBg = helper.Content.Load<Texture2D>("assets/manabg.png");

            Color manaCol = new Color(0, 48, 255);
            manaFg = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            manaFg.SetData(new Color[] { manaCol });
        }

        private static void addManaCommand(string[] args)
        {
            Game1.player.addMana(int.Parse(args[0]));
        }
        private static void setMaxManaCommand(string[] args)
        {
            Game1.player.setMaxMana(int.Parse(args[0]));
        }

        private IApi api;
        public override object GetApi()
        {
            if (api == null)
                api = new Api();
            return api;
        }

        private static void onNetworkData(IncomingMessage msg)
        {
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                Mod.Data.players[msg.Reader.ReadInt64()] = JsonConvert.DeserializeObject<MultiplayerSaveData.PlayerData>(msg.Reader.ReadString());
            }
        }

        private static void onNetworkMiniData(IncomingMessage msg)
        {
            Mod.Data.players[msg.FarmerID].mana = msg.Reader.ReadInt32();
            Mod.Data.players[msg.FarmerID].manaCap = msg.Reader.ReadInt32();
        }

        private static void onClientConnected(object sender, EventArgsServerGotClient args)
        {
            if (!Data.players.ContainsKey(args.FarmerID))
                Data.players[args.FarmerID] = new MultiplayerSaveData.PlayerData();

            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((int)Data.players.Count);
                foreach (var entry in Data.players)
                {
                    writer.Write(entry.Key);
                    writer.Write(JsonConvert.SerializeObject(entry.Value, MultiplayerSaveData.networkSerializerSettings));
                }
                SpaceCore.Networking.BroadcastMessage(MultiplayerSaveData.MSG_DATA, stream.ToArray());
            }
        }

        public static void onRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu != null || Game1.eventUp || Game1.player.getMaxMana() == 0)
                return;

            SpriteBatch b = e.SpriteBatch;

            Vector2 manaPos = new Vector2(20, Game1.uiViewport.Height - manaBg.Height * 4 - 20);
            b.Draw(manaBg, manaPos, new Rectangle(0, 0, manaBg.Width, manaBg.Height), Color.White, 0, new Vector2(), 4, SpriteEffects.None, 1);
            if (Game1.player.getCurrentMana() > 0)
            {
                Rectangle targetArea = new Rectangle(3, 13, 6, 41);
                float perc = Game1.player.getCurrentMana() / (float)Game1.player.getMaxMana();
                int h = (int)(targetArea.Height * perc);
                targetArea.Y += targetArea.Height - h;
                targetArea.Height = h;

                targetArea.X *= 4;
                targetArea.Y *= 4;
                targetArea.Width *= 4;
                targetArea.Height *= 4;
                targetArea.X += (int)manaPos.X;
                targetArea.Y += (int)manaPos.Y;
                b.Draw(manaFg, targetArea, new Rectangle(0, 0, 1, 1), Color.White);

                if ((double)Game1.getOldMouseX() >= (double)targetArea.X && (double)Game1.getOldMouseY() >= (double)targetArea.Y && (double)Game1.getOldMouseX() < (double)targetArea.X + targetArea.Width && Game1.getOldMouseY() < targetArea.Y + targetArea.Height)
                    Game1.drawWithBorder(Math.Max(0, (int)Game1.player.getCurrentMana()).ToString() + "/" + Game1.player.getMaxMana(), Color.Black * 0.0f, Color.White, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 32));
            }
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void onDayStarted(object sender, DayStartedEventArgs e)
        {
            Game1.player.addMana(Game1.player.getMaxMana());
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                {
                    Log.info($"Loading save data");
                    Data = Helper.Data.ReadSaveData<MultiplayerSaveData>(MultiplayerSaveData.SaveKey);
                    if (Data == null)
                    {
                        if (File.Exists(MultiplayerSaveData.OldFilePath))
                        {
                            Data = JsonConvert.DeserializeObject<MultiplayerSaveData>(File.ReadAllText(MultiplayerSaveData.OldFilePath));
                        }
                    }
                    if (Data == null)
                        Data = new MultiplayerSaveData();

                    if (!Data.players.ContainsKey(Game1.player.UniqueMultiplayerID))
                        Data.players[Game1.player.UniqueMultiplayerID] = new MultiplayerSaveData.PlayerData();
                }
            }
            catch (Exception ex)
            {
                Log.warn($"Exception loading save data: {ex}");
            }
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaved(object sender, SavedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Log.info($"Saving save data...");
                Helper.Data.WriteSaveData(MultiplayerSaveData.SaveKey, Data);
            }
        }
    }
}
