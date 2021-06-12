using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class Mod : StardewModdingAPI.Mod
    {
        public const string SEASONAL_INDICATOR = "%";

        private static readonly TimeSpan clickWindow = new(250 * TimeSpan.TicksPerMillisecond);

        public static Mod instance;
        public static SavedExteriors savedExteriors = new();
        public static ContentManager content;

        public static Dictionary<string, List<string>> choices = new();

        private const string MSG_CHOICES = "spacechase0.CustomizeExterior.Choices";

        public override void Entry(IModHelper helper)
        {
            Mod.instance = this;
            Log.Monitor = this.Monitor;

            Mod.content = new ContentManager(Game1.content.ServiceProvider, Path.Combine(this.Helper.DirectoryPath, "Buildings"));
            this.compileChoices();

            helper.Events.GameLoop.UpdateTicked += this.onUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            helper.Events.GameLoop.Saving += this.onSaving;
            helper.Events.GameLoop.Saved += this.onSaved;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;
            helper.Events.Player.Warped += this.onWarped;

            SpaceEvents.ServerGotClient += this.onClientConnected;
            SpaceCore.Networking.RegisterMessageHandler(Mod.MSG_CHOICES, this.onChoicesReceived);
        }

        private void onClientConnected(object sender, EventArgsServerGotClient args)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Mod.savedExteriors.chosen.Count);
                foreach (var choice in Mod.savedExteriors.chosen)
                {
                    writer.Write(choice.Key);
                    writer.Write(choice.Value);
                }

                var server = (GameServer)sender;
                Log.Trace("Sending exteriors data to " + args.FarmerID);
                SpaceCore.Networking.ServerSendTo(args.FarmerID, Mod.MSG_CHOICES, stream.ToArray());
            }
        }

        private void onChoicesReceived(IncomingMessage msg)
        {
            Log.Trace("Got exterior data");
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string building = msg.Reader.ReadString();
                string texId = msg.Reader.ReadString();
                Log.Trace("\t" + building + "=" + texId);

                Mod.savedExteriors.chosen[building] = texId;
            }
            this.syncTexturesWithChoices();
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                Mod.savedExteriors = this.Helper.Data.ReadSaveData<SavedExteriors>("building-exteriors");
                if (Mod.savedExteriors == null)
                {
                    string legacyPath = Path.Combine(Constants.CurrentSavePath, "building-exteriors.json");
                    if (File.Exists(legacyPath))
                    {
                        Log.Info($"Loading per-save config file (\"{legacyPath}\")...");
                        Mod.savedExteriors = JsonConvert.DeserializeObject<SavedExteriors>(File.ReadAllText(legacyPath));
                    }
                }
                if (Mod.savedExteriors == null)
                    Mod.savedExteriors = new SavedExteriors();

                this.syncTexturesWithChoices();
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                this.Helper.Data.WriteSaveData("building-exteriors", Mod.savedExteriors);
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaved(object sender, SavedEventArgs e)
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
        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLocation is BuildableGameLocation)
                this.syncTexturesWithChoices();
        }

        private void onSeasonChange()
        {
            Log.Debug("Season change, syncing textures...");
            this.syncTexturesWithChoices();
        }

        public string prevSeason = "";

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && Game1.currentSeason != this.prevSeason)
            {
                this.onSeasonChange();
                this.prevSeason = Game1.currentSeason;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && e.Button == SButton.MouseRight)
            {
                Point pos = new Point((int)e.Cursor.AbsolutePixels.X, (int)e.Cursor.AbsolutePixels.Y);
                if (Game1.currentLocation is BuildableGameLocation)
                {
                    var loc = Game1.currentLocation as BuildableGameLocation;

                    foreach (Building building in loc.buildings)
                    {
                        Rectangle tileBounds = new Rectangle(building.tileX.Value * Game1.tileSize, building.tileY.Value * Game1.tileSize, building.tilesWide.Value * Game1.tileSize, building.tilesHigh.Value * Game1.tileSize);
                        if (tileBounds.Contains(pos.X, pos.Y))
                        {
                            Log.Trace($"Right clicked a building: {building.nameOfIndoors}");
                            this.checkBuildingClick(building.nameOfIndoors, building.buildingType.Value);
                        }
                    }
                }
                if (Game1.currentLocation is Farm)
                {
                    Rectangle house = new Rectangle(59 * Game1.tileSize, 11 * Game1.tileSize, 9 * Game1.tileSize, 6 * Game1.tileSize);
                    Rectangle greenhouse = new Rectangle(25 * Game1.tileSize, 10 * Game1.tileSize, 7 * Game1.tileSize, 6 * Game1.tileSize);
                    if (Game1.whichFarm == Farm.fourCorners_layout)
                    {
                        greenhouse.X = 36 * Game1.tileSize;
                        greenhouse.Y = 25 * Game1.tileSize;
                    }

                    if (house.Contains(pos.X, pos.Y))
                    {
                        Log.Trace("Right clicked the house.");
                        this.checkBuildingClick("FarmHouse", "houses");
                    }
                    else if (greenhouse.Contains(pos.X, pos.Y))
                    {
                        Log.Trace("Right clicked the greenhouse.");
                        this.checkBuildingClick("Greenhouse", "houses");
                    }
                }
            }
        }

        private void compileChoices()
        {
            Log.Trace("Creating list of building choices...");
            string buildingsPath = Path.Combine(this.Helper.DirectoryPath, "Buildings");
            if (!Directory.Exists(buildingsPath))
                Directory.CreateDirectory(buildingsPath);

            string[] choices = Directory.GetDirectories(buildingsPath);
            foreach (string choice in choices)
            {
                if (choice == "spring" || choice == "summer" || choice == "fall" || choice == "winter")
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
                    List<string> forType = Mod.choices.ContainsKey(typeStr) ? Mod.choices[typeStr] : new List<string>();
                    if (!forType.Contains(choiceStr))
                        forType.Add(choiceStr);
                    if (!Mod.choices.ContainsKey(typeStr))
                        Mod.choices.Add(typeStr, forType);

                    Log.Trace("\tChoice: " + typeStr);
                }

                string[] seasons = Directory.GetDirectories(choice);
                bool foundSpring = false, foundSummer = false, foundFall = false, foundWinter = false;
                foreach (string season in seasons)
                {
                    string filename = Path.GetFileName(season);
                    if (filename == "spring") foundSpring = true;
                    else if (filename == "summer") foundSummer = true;
                    else if (filename == "fall") foundFall = true;
                    else if (filename == "winter") foundWinter = true;
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

                    var common = new List<string>();
                    foreach (string building in spring)
                    {
                        string choiceStr = Path.GetFileName(choice);
                        string typeStr = Path.GetFileNameWithoutExtension(building);
                        if (summer.Contains(building) && fall.Contains(building) && winter.Contains(building))
                        {
                            List<string> forType = Mod.choices.ContainsKey(typeStr) ? Mod.choices[typeStr] : new List<string>();
                            if (!forType.Contains(Mod.SEASONAL_INDICATOR + choiceStr))
                                forType.Add(Mod.SEASONAL_INDICATOR + choiceStr);
                            if (!Mod.choices.ContainsKey(typeStr))
                                Mod.choices.Add(typeStr, forType);

                            Log.Trace("\tChoice: " + typeStr);
                        }
                    }
                }
            }
        }

        private DateTime recentClickTime;
        private string recentClickTarget;
        private void checkBuildingClick(string target, string type)
        {
            if (Game1.activeClickableMenu != null) return;

            if (this.recentClickTarget != target)
            {
                this.recentClickTarget = target;
                this.recentClickTime = DateTime.Now;
            }
            else
            {
                if (DateTime.Now - this.recentClickTime < Mod.clickWindow)
                    this.todoRenameFunction(target, type);
                else
                    this.recentClickTime = DateTime.Now;
            }
        }

        private void todoRenameFunction(string target, string type)
        {
            if (!Mod.choices.TryGetValue(type, out var choices))
            {
                Log.Trace($"Target: {target} ({type}), but no custom textures found.");
                return;
            }
            Log.Trace($"Target: {target} ({type}), found {choices.Count} textures: '{string.Join("', '", choices)}'.");

            this.recentTarget = target;
            var menu = new SelectDisplayMenu(type, Mod.getChosenTexture(target))
            {
                onSelected = this.onExteriorSelected
            };
            Game1.activeClickableMenu = menu;
        }

        private string recentTarget;
        private void onExteriorSelected(string type, string choice) { this.onExteriorSelected(type, choice, true); }
        private void onExteriorSelected(string type, string choice, bool updateChosen)
        {
            Log.Trace("onExteriorSelected: " + this.recentTarget + " " + type + " " + choice);

            Texture2D tex = Mod.getTextureForChoice(type, choice);
            if (tex == null)
            {
                Log.Warn("Failed to load chosen texture '" + choice + "' for building type '" + type + "'.");
                return;
            }
            if (updateChosen)
            {
                Mod.savedExteriors.chosen[this.recentTarget] = choice;

                if (Game1.IsMultiplayer)
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(1);
                        writer.Write(this.recentTarget);
                        writer.Write(choice);

                        Log.Trace("Broadcasting choice");
                        SpaceCore.Networking.BroadcastMessage(Mod.MSG_CHOICES, stream.ToArray());
                    }
                }
            }

            if (this.recentTarget == "FarmHouse" || this.recentTarget == "Greenhouse")
            {
                Mod.housesHybrid = null;
                typeof(Farm).GetField(nameof(Farm.houseTextures)).SetValue(null, Mod.getHousesTexture());
            }
            else
            {
                foreach (Building building in Game1.getFarm().buildings)
                {
                    if (building.buildingType.Value == type && building.nameOfIndoors == this.recentTarget)
                    {
                        building.texture = new Lazy<Texture2D>(() => tex);
                        break;
                    }
                }
            }
        }

        private void syncTexturesWithChoices()
        {
            foreach (var choice in Mod.savedExteriors.chosen)
            {
                this.recentTarget = choice.Key;
                Log.Trace("Saved choice: " + choice.Key + " " + choice.Value);

                string type = null;
                if (this.recentTarget == "FarmHouse" || this.recentTarget == "Greenhouse")
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
                    this.onExteriorSelected(type, choice.Value, false);
            }
        }

        public static string getChosenTexture(string target)
        {
            return Mod.savedExteriors.chosen.ContainsKey(target) ? Mod.savedExteriors.chosen[target] : "/";
        }

        public static Texture2D getTextureForChoice(string type, string choice)
        {
            try
            {
                if (choice == "/")
                    return Game1.content.Load<Texture2D>("Buildings/" + type);
                else if (choice.StartsWith(Mod.SEASONAL_INDICATOR))
                    return Mod.content.Load<Texture2D>(choice.Substring(Mod.SEASONAL_INDICATOR.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return Mod.content.Load<Texture2D>(choice + "/" + type);
            }
            catch (ContentLoadException)
            {
                if (choice.StartsWith(Mod.SEASONAL_INDICATOR))
                    return Mod.loadPng(choice.Substring(Mod.SEASONAL_INDICATOR.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return Mod.loadPng(choice + "/" + type);
            }
        }

        private static Texture2D loadPng(string path)
        {
            FileStream fs = File.Open(Path.Combine(Mod.instance.Helper.DirectoryPath, "Buildings", path + ".png"), FileMode.Open);
            Texture2D tex = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fs);
            fs.Dispose();
            return tex;
        }

        private static Texture2D housesHybrid;
        private static Texture2D getHousesTexture()
        {
            if (Mod.housesHybrid != null)
                return Mod.housesHybrid;

            Log.Trace("Creating hybrid farmhouse/greenhouse texture");

            Texture2D baseTex = Farm.houseTextures;
            Rectangle houseRect = new Rectangle(0, 0, 160, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "houseSource");
            Rectangle greenhouseRect = new Rectangle(160, 0, 112, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "greenhouseSource");

            GraphicsDevice dev = Game1.graphics.GraphicsDevice;
            RenderTarget2D ret = new RenderTarget2D(dev, baseTex.Width, baseTex.Height);
            ret.Name = Mod.instance.ModManifest.UniqueID + ".houses";
            SpriteBatch b = Game1.spriteBatch;
            dev.SetRenderTarget(ret);
            {
                dev.Clear(Color.Transparent);
                b.Begin();
                b.Draw(Mod.getTextureForChoice("houses", Mod.getChosenTexture("FarmHouse")), houseRect, houseRect, Color.White);
                b.Draw(Mod.getTextureForChoice("houses", Mod.getChosenTexture("Greenhouse")), greenhouseRect, greenhouseRect, Color.White);
                b.End();
            }
            dev.SetRenderTarget(null);

            Mod.housesHybrid = ret;
            return ret;
        }
    }
}
