using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomizeExterior.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceCore.Events;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Network;

namespace CustomizeExterior
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /// <summary>The building identifier and time which the player last clicked, if any.</summary>
        /// <remarks>This should only be used via <see cref="IsOpenMenuClick(string)"/>.</remarks>
        private Tuple<string, DateTime> LastRightClick = null;

        private string RecentTarget;

        public const string SeasonalIndicator = "%";

        private readonly TimeSpan ClickWindow = TimeSpan.FromMilliseconds(250);

        public static Mod Instance;
        public static SavedExteriors SavedExteriors = new();
        public static ContentManager Content;

        public static Dictionary<string, List<string>> Choices = new();

        private const string MsgChoices = "spacechase0.CustomizeExterior.Choices";

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            Mod.Content = new ContentManager(Game1.content.ServiceProvider, Path.Combine(this.Helper.DirectoryPath, "Buildings"));
            this.CompileChoices();

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.Saved += this.OnSaved;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.Warped += this.OnWarped;

            SpaceEvents.ServerGotClient += this.OnClientConnected;
            SpaceCore.Networking.RegisterMessageHandler(Mod.MsgChoices, this.OnChoicesReceived);
        }

        private void OnClientConnected(object sender, EventArgsServerGotClient args)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(Mod.SavedExteriors.Chosen.Count);
            foreach (var choice in Mod.SavedExteriors.Chosen)
            {
                writer.Write(choice.Key);
                writer.Write(choice.Value);
            }

            Log.Trace("Sending exteriors data to " + args.FarmerID);
            SpaceCore.Networking.ServerSendTo(args.FarmerID, Mod.MsgChoices, stream.ToArray());
        }

        private void OnChoicesReceived(IncomingMessage msg)
        {
            Log.Trace("Got exterior data");
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string building = msg.Reader.ReadString();
                string texId = msg.Reader.ReadString();
                Log.Trace("\t" + building + "=" + texId);

                Mod.SavedExteriors.Chosen[building] = texId;
            }
            this.SyncTexturesWithChoices();
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Mod.SavedExteriors = this.Helper.Data.ReadSaveData<SavedExteriors>("building-exteriors");
                if (Mod.SavedExteriors == null)
                {
                    string legacyPath = Path.Combine(Constants.CurrentSavePath, "building-exteriors.json");
                    if (File.Exists(legacyPath))
                    {
                        Log.Info($"Loading per-save config file (\"{legacyPath}\")...");
                        Mod.SavedExteriors = JsonConvert.DeserializeObject<SavedExteriors>(File.ReadAllText(legacyPath));
                    }
                }
                Mod.SavedExteriors ??= new SavedExteriors();

                this.SyncTexturesWithChoices();
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                this.Helper.Data.WriteSaveData("building-exteriors", Mod.SavedExteriors);
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                string legacyPath = Path.Combine(Constants.CurrentSavePath, "building-exteriors.json");
                if (File.Exists(legacyPath))
                {
                    Log.Info("Removing legacy data file...");
                    File.Delete(legacyPath);
                }
            }
        }

        /// <summary>Raised after a player warps to a new location. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLocation is BuildableGameLocation)
                this.SyncTexturesWithChoices();
        }

        private void OnSeasonChange()
        {
            Log.Debug("Season change, syncing textures...");
            this.SyncTexturesWithChoices();
        }

        public string PrevSeason = "";

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && Game1.currentSeason != this.PrevSeason)
            {
                this.OnSeasonChange();
                this.PrevSeason = Game1.currentSeason;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && e.Button == SButton.MouseRight)
            {
                Point tile = Utility.Vector2ToPoint(e.Cursor.Tile);

                if (Game1.currentLocation is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        Rectangle tileBounds = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                        if (tileBounds.Contains(tile))
                        {
                            string id = building is GreenhouseBuilding
                                ? "Greenhouse"
                                : building.nameOfIndoors;
                            string textureName = Path.GetFileName(building.textureName());

                            Log.Trace($"Right clicked a building (id: {id}, texture: {textureName})");

                            if (this.IsOpenMenuClick(building))
                                this.TryOpenCustomizationMenu(id, textureName);
                            return;
                        }
                    }
                }

                if (Game1.currentLocation is Farm farm)
                {
                    Rectangle house = farm.GetHouseRect();
                    if (house.Contains(tile))
                    {
                        Log.Trace("Right clicked the house.");

                        if (this.IsOpenMenuClick(nameof(FarmHouse)))
                            this.TryOpenCustomizationMenu("FarmHouse", "houses");
                        return;
                    }
                }
            }
        }

        private void CompileChoices()
        {
            Log.Trace("Creating list of building choices...");
            string buildingsPath = Path.Combine(this.Helper.DirectoryPath, "Buildings");
            if (!Directory.Exists(buildingsPath))
                Directory.CreateDirectory(buildingsPath);

            string[] choices = Directory.GetDirectories(buildingsPath);
            foreach (string choice in choices)
            {
                if (choice is "spring" or "summer" or "fall" or "winter")
                {
                    Log.Warn("A seasonal texture set was installed incorrectly. '" + choice + "' should not be directly in the Buildings folder.");
                    continue;
                }

                Log.Info("Choice type: " + Path.GetFileName(choice));
                string[] types = Directory.GetFiles(choice);
                foreach (string type in types)
                {
                    if (Path.GetExtension(type) != ".xnb" && Path.GetExtension(type) != ".png")
                        continue;

                    string choiceStr = Path.GetFileName(choice);
                    string typeStr = Path.GetFileNameWithoutExtension(type);

                    if (!Mod.Choices.TryGetValue(typeStr, out List<string> forType))
                        forType = new();

                    if (!forType.Contains(choiceStr))
                        forType.Add(choiceStr);
                    if (!Mod.Choices.ContainsKey(typeStr))
                        Mod.Choices.Add(typeStr, forType);

                    Log.Trace("\tChoice: " + typeStr);
                }

                string[] seasons = Directory.GetDirectories(choice);
                bool foundSpring = false, foundSummer = false, foundFall = false, foundWinter = false;
                foreach (string season in seasons)
                {
                    string filename = Path.GetFileName(season);
                    switch (filename)
                    {
                        case "spring":
                            foundSpring = true;
                            break;

                        case "summer":
                            foundSummer = true;
                            break;

                        case "fall":
                            foundFall = true;
                            break;

                        case "winter":
                            foundWinter = true;
                            break;
                    }
                }

                if (foundSpring && foundSummer && foundFall && foundWinter)
                {
                    Log.Trace("Found a seasonal set: " + Path.GetFileName(choice));

                    var spring = new List<string>(Directory.GetFiles(Path.Combine(choice, "spring")));
                    var summer = new List<string>(Directory.GetFiles(Path.Combine(choice, "summer")));
                    var fall = new List<string>(Directory.GetFiles(Path.Combine(choice, "fall")));
                    var winter = new List<string>(Directory.GetFiles(Path.Combine(choice, "winter")));
                    spring = spring.Select(Path.GetFileName).ToList();
                    summer = summer.Select(Path.GetFileName).ToList();
                    fall = fall.Select(Path.GetFileName).ToList();
                    winter = winter.Select(Path.GetFileName).ToList();

                    foreach (string building in spring)
                    {
                        string choiceStr = Path.GetFileName(choice);
                        string typeStr = Path.GetFileNameWithoutExtension(building);
                        if (summer.Contains(building) && fall.Contains(building) && winter.Contains(building))
                        {
                            if (!Mod.Choices.TryGetValue(typeStr, out List<string> forType))
                                forType = new();

                            if (!forType.Contains(Mod.SeasonalIndicator + choiceStr))
                                forType.Add(Mod.SeasonalIndicator + choiceStr);
                            if (!Mod.Choices.ContainsKey(typeStr))
                                Mod.Choices.Add(typeStr, forType);

                            Log.Trace("\tChoice: " + typeStr);
                        }
                    }
                }
            }
        }

        /// <summary>Get whether the player just double-right-clicked a given building.</summary>
        /// <param name="building">The building instance.</param>
        private bool IsOpenMenuClick(Building building)
        {
            return this.IsOpenMenuClick($"{building.buildingType.Value}|{building.tileX.Value}|{building.tileY.Value}");
        }

        /// <summary>Get whether the player just double-right-clicked a given building.</summary>
        /// <param name="target">A unique identifier for the building.</param>
        private bool IsOpenMenuClick(string target)
        {
            if (!Context.IsPlayerFree)
                return false;

            bool shouldOpen =
                this.LastRightClick != null
                && this.LastRightClick.Item1 == target
                && DateTime.Now - this.LastRightClick.Item2 < this.ClickWindow;

            this.LastRightClick = new(target, DateTime.Now);

            return shouldOpen;
        }

        private bool TryOpenCustomizationMenu(string target, string type)
        {
            if (!Mod.Choices.TryGetValue(type, out var choices))
            {
                Log.Trace($"Target: {target} ({type}), but no custom textures found.");
                return false;
            }
            Log.Trace($"Target: {target} ({type}), found {choices.Count} textures: '{string.Join("', '", choices)}'.");

            this.RecentTarget = target;
            var menu = new SelectDisplayMenu(type, Mod.GetChosenTexture(target))
            {
                OnSelected = this.OnExteriorSelected
            };
            Game1.activeClickableMenu = menu;
            return true;
        }

        private void OnExteriorSelected(string type, string choice) { this.OnExteriorSelected(type, choice, true); }
        private void OnExteriorSelected(string type, string choice, bool updateChosen)
        {
            Log.Trace("onExteriorSelected: " + this.RecentTarget + " " + type + " " + choice);

            Texture2D tex = Mod.GetTextureForChoice(type, choice);
            if (tex == null)
            {
                Log.Warn("Failed to load chosen texture '" + choice + "' for building type '" + type + "'.");
                return;
            }
            if (updateChosen)
            {
                Mod.SavedExteriors.Chosen[this.RecentTarget] = choice;

                if (Game1.IsMultiplayer)
                {
                    using var stream = new MemoryStream();
                    using var writer = new BinaryWriter(stream);
                    writer.Write(1);
                    writer.Write(this.RecentTarget);
                    writer.Write(choice);

                    Log.Trace("Broadcasting choice");
                    SpaceCore.Networking.BroadcastMessage(Mod.MsgChoices, stream.ToArray());
                }
            }

            if (this.RecentTarget == "FarmHouse" || this.RecentTarget == "Greenhouse")
            {
                Mod.HousesHybrid = null;
                typeof(Farm).GetField(nameof(Farm.houseTextures)).SetValue(null, Mod.GetHousesTexture());
            }
            else
            {
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.buildingType.Value == type && building.nameOfIndoors == this.RecentTarget)
                    {
                        building.texture = new Lazy<Texture2D>(() => tex);
                        break;
                    }
                }
            }
        }

        private void SyncTexturesWithChoices()
        {
            foreach (var choice in Mod.SavedExteriors.Chosen)
            {
                this.RecentTarget = choice.Key;
                Log.Trace("Saved choice: " + choice.Key + " " + choice.Value);

                string type = null;
                if (this.RecentTarget == "FarmHouse" || this.RecentTarget == "Greenhouse")
                {
                    type = "houses";
                }
                else
                {
                    foreach (Building building in Game1.getFarm().buildings)
                    {
                        if (building.nameOfIndoors == choice.Key)
                        {
                            type = building.buildingType.Value;
                        }
                    }
                }

                if (type != null)
                    this.OnExteriorSelected(type, choice.Value, false);
            }
        }

        public static string GetChosenTexture(string target)
        {
            return Mod.SavedExteriors.Chosen.TryGetValue(target, out string choice)
                ? choice
                : "/";
        }

        public static Texture2D GetTextureForChoice(string type, string choice)
        {
            try
            {
                if (choice == "/")
                    return Game1.content.Load<Texture2D>("Buildings/" + type);
                else if (choice.StartsWith(Mod.SeasonalIndicator))
                    return Mod.Content.Load<Texture2D>(choice.Substring(Mod.SeasonalIndicator.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return Mod.Content.Load<Texture2D>(choice + "/" + type);
            }
            catch (ContentLoadException)
            {
                if (choice.StartsWith(Mod.SeasonalIndicator))
                    return Mod.LoadPng(choice.Substring(Mod.SeasonalIndicator.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return Mod.LoadPng(choice + "/" + type);
            }
        }

        private static Texture2D LoadPng(string path)
        {
            FileStream fs = File.Open(Path.Combine(Mod.Instance.Helper.DirectoryPath, "Buildings", path + ".png"), FileMode.Open);
            Texture2D tex = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fs);
            fs.Dispose();
            return tex;
        }

        private static Texture2D HousesHybrid;
        private static Texture2D GetHousesTexture()
        {
            if (Mod.HousesHybrid != null)
                return Mod.HousesHybrid;

            Log.Trace("Creating hybrid farmhouse/greenhouse texture");

            Texture2D baseTex = Farm.houseTextures;
            Rectangle houseRect = new Rectangle(0, 0, 160, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "houseSource");
            Rectangle greenhouseRect = new Rectangle(160, 0, 112, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "greenhouseSource");

            GraphicsDevice dev = Game1.graphics.GraphicsDevice;
            RenderTarget2D ret = new RenderTarget2D(dev, baseTex.Width, baseTex.Height)
            {
                Name = Mod.Instance.ModManifest.UniqueID + ".houses"
            };
            SpriteBatch b = Game1.spriteBatch;
            dev.SetRenderTarget(ret);
            {
                dev.Clear(Color.Transparent);
                b.Begin();
                b.Draw(Mod.GetTextureForChoice("houses", Mod.GetChosenTexture("FarmHouse")), houseRect, houseRect, Color.White);
                b.Draw(Mod.GetTextureForChoice("houses", Mod.GetChosenTexture("Greenhouse")), greenhouseRect, greenhouseRect, Color.White);
                b.End();
            }
            dev.SetRenderTarget(null);

            Mod.HousesHybrid = ret;
            return ret;
        }
    }
}
