using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Buildings;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Newtonsoft.Json;
using SpaceCore.Events;
using StardewValley.Network;

namespace CustomizeExterior
{
    public class Mod : StardewModdingAPI.Mod
    {
        public const string SEASONAL_INDICATOR = "%";

        private static readonly TimeSpan clickWindow = new TimeSpan(250 * TimeSpan.TicksPerMillisecond);

        public static Mod instance;
        public static SavedExteriors savedExteriors = new SavedExteriors();
        public static ContentManager content;

        public static Dictionary<string, List<string>> choices = new Dictionary<string, List<string>>();

        private const string MSG_CHOICES = "spacechase0.CustomizeExterior.Choices";

        public override void Entry(IModHelper helper)
        {
            instance = this;

            content = new ContentManager(Game1.content.ServiceProvider, Path.Combine(Helper.DirectoryPath, "Buildings"));
            compileChoices();
            
            helper.Events.GameLoop.UpdateTicked += onUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            helper.Events.GameLoop.Saving += onSaving;
            helper.Events.GameLoop.Saved += onSaved;
            helper.Events.Input.ButtonPressed += onButtonPressed;
            helper.Events.Player.Warped += onWarped;

            SpaceEvents.ServerGotClient += onClientConnected;
            SpaceCore.Networking.RegisterMessageHandler(MSG_CHOICES, onChoicesReceived);
        }

        private void onClientConnected(object sender, EventArgsServerGotClient args)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(savedExteriors.chosen.Count);
                foreach (var choice in savedExteriors.chosen)
                {
                    writer.Write(choice.Key);
                    writer.Write(choice.Value);
                }

                var server = (GameServer)sender;
                Log.trace("Sending exteriors data to " + args.FarmerID);
                SpaceCore.Networking.ServerSendTo(args.FarmerID, MSG_CHOICES, stream.ToArray());
            }
        }

        private void onChoicesReceived(IncomingMessage msg)
        {
            Log.trace("Got exterior data");
            int count = msg.Reader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string building = msg.Reader.ReadString();
                string texId = msg.Reader.ReadString();
                Log.trace("\t" + building + "=" + texId);

                savedExteriors.chosen[building] = texId;
            }
            syncTexturesWithChoices();
        }

        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
            {
                savedExteriors = this.Helper.Data.ReadSaveData<SavedExteriors>("building-exteriors");
                if (savedExteriors == null)
                {
                    string legacyPath = Path.Combine(Constants.CurrentSavePath, "building-exteriors.json");
                    if (File.Exists(legacyPath))
                    {
                        Log.info($"Loading per-save config file (\"{legacyPath}\")...");
                        savedExteriors = JsonConvert.DeserializeObject<SavedExteriors>(File.ReadAllText(legacyPath));
                    }
                }
                if (savedExteriors == null)
                    savedExteriors = new SavedExteriors();

                syncTexturesWithChoices();
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMultiplayer || Game1.IsMasterGame)
                this.Helper.Data.WriteSaveData("building-exteriors", savedExteriors);
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
                    Log.info("Removing legacy data file...");
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
                syncTexturesWithChoices();
        }

        private void onSeasonChange()
        {
            Log.debug("Season change, syncing textures...");
            syncTexturesWithChoices();
        }

        public string prevSeason = "";

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onUpdateTicked( object sender, UpdateTickedEventArgs e)
        {
            if ( Context.IsWorldReady && Game1.currentSeason != prevSeason )
            {
                onSeasonChange();
                prevSeason = Game1.currentSeason;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && e.Button == SButton.MouseRight)
            {
                Point pos = new Point((int)e.Cursor.AbsolutePixels.X,  (int)e.Cursor.AbsolutePixels.Y);
                if (Game1.currentLocation is BuildableGameLocation)
                {
                    var loc = Game1.currentLocation as BuildableGameLocation;

                    foreach (Building building in loc.buildings)
                    {
                        Rectangle tileBounds = new Rectangle(building.tileX.Value * Game1.tileSize, building.tileY.Value * Game1.tileSize, building.tilesWide.Value * Game1.tileSize, building.tilesHigh.Value * Game1.tileSize);
                        if (tileBounds.Contains(pos.X, pos.Y))
                        {
                            Log.trace($"Right clicked a building: {building.nameOfIndoors}");
                            checkBuildingClick(building.nameOfIndoors, building.buildingType.Value);
                        }
                    }
                }
                if (Game1.currentLocation is Farm)
                {
                    Rectangle house = new Rectangle(59 * Game1.tileSize, 11 * Game1.tileSize, 9 * Game1.tileSize, 6 * Game1.tileSize);
                    Rectangle greenhouse = new Rectangle(25 * Game1.tileSize, 10 * Game1.tileSize, 7 * Game1.tileSize, 6 * Game1.tileSize);

                    if (house.Contains(pos.X, pos.Y))
                    {
                        Log.trace("Right clicked the house.");
                        checkBuildingClick("FarmHouse", "houses");
                    }
                    else if (greenhouse.Contains(pos.X, pos.Y))
                    {
                        Log.trace("Right clicked the greenhouse.");
                        checkBuildingClick("Greenhouse", "houses");
                    }
                }
            }
        }

        private void compileChoices()
        {
            Log.info("Creating list of building choices...");
            var buildingsPath = Path.Combine(Helper.DirectoryPath, "Buildings");
            if (!Directory.Exists(buildingsPath))
                Directory.CreateDirectory(buildingsPath);

            var choices = Directory.GetDirectories(buildingsPath);
            foreach ( var choice in choices )
            {
                if (choice == "spring" || choice == "summer" || choice == "fall" || choice == "winter")
                {
                    Log.warn("A seasonal texture set was installed incorrectly. '" + choice + "' should not be directly in the Buildings folder.");
                    continue;
                }

                Log.info("Choice type: " + Path.GetFileName(choice));
                var types = Directory.GetFiles(choice);
                foreach ( var type in types )
                {
                    if (Path.GetExtension(type) != ".xnb" && Path.GetExtension(type) != ".png")
                        continue;

                    string choiceStr = Path.GetFileName(choice);
                    string typeStr = Path.GetFileNameWithoutExtension(type);
                    List<string> forType = Mod.choices.ContainsKey(typeStr) ? Mod.choices[typeStr] : new List<string>();
                    if ( !forType.Contains( choiceStr ) )
                        forType.Add(choiceStr);
                    if (!Mod.choices.ContainsKey(typeStr))
                        Mod.choices.Add(typeStr, forType);

                    Log.info("\tChoice: " + typeStr);
                }

                var seasons = Directory.GetDirectories(choice);
                bool foundSpring = false, foundSummer = false, foundFall = false, foundWinter = false;
                foreach ( var season in seasons )
                {
                    var filename = Path.GetFileName(season);
                    if (filename == "spring") foundSpring = true;
                    else if (filename == "summer") foundSummer = true;
                    else if (filename == "fall") foundFall = true;
                    else if (filename == "winter") foundWinter = true;
                }
                
                if ( foundSpring && foundSummer && foundFall && foundWinter )
                {
                    Log.info("Found a seasonal set: " + Path.GetFileName(choice));

                    var spring = new List<string>(Directory.GetFiles(Path.Combine(choice, "spring")));
                    var summer = new List<string>(Directory.GetFiles(Path.Combine(choice, "summer")));
                    var fall = new List<string>(Directory.GetFiles(Path.Combine(choice, "fall")));
                    var winter = new List<string>(Directory.GetFiles(Path.Combine(choice, "winter")));
                    spring = spring.Select(Path.GetFileName).ToList();
                    summer = summer.Select(Path.GetFileName).ToList();
                    fall = fall.Select(Path.GetFileName).ToList();
                    winter = winter.Select(Path.GetFileName).ToList();
                    
                    var common = new List<string>();
                    foreach ( var building in spring )
                    {
                        string choiceStr = Path.GetFileName(choice);
                        string typeStr = Path.GetFileNameWithoutExtension(building);
                        if ( summer.Contains( building ) && fall.Contains( building ) && winter.Contains( building ) )
                        {
                            List<string> forType = Mod.choices.ContainsKey(typeStr) ? Mod.choices[typeStr] : new List<string>();
                            if (!forType.Contains(SEASONAL_INDICATOR + choiceStr))
                                forType.Add(SEASONAL_INDICATOR + choiceStr);
                            if (!Mod.choices.ContainsKey(typeStr))
                                Mod.choices.Add(typeStr, forType);

                            Log.info("\tChoice: " + typeStr);
                        }
                    }
                }
            }
        }
        
        private DateTime recentClickTime;
        private string recentClickTarget = null;
        private void checkBuildingClick( string target, string type )
        {
            if (Game1.activeClickableMenu != null) return;

            if (recentClickTarget != target)
            {
                recentClickTarget = target;
                recentClickTime = DateTime.Now;
            }
            else
            {
                if (DateTime.Now - recentClickTime < clickWindow)
                    todoRenameFunction( target, type );
                else recentClickTime = DateTime.Now;
            }
        }

        private void todoRenameFunction( string target, string type )
        {
            Log.debug("Target: " + target + " " + type);

            if (!choices.ContainsKey(type))
                return;

            foreach ( var choice in choices[ type ] )
            {
                Log.debug("Choice: " + choice);
            }

            recentTarget = target;
            var menu = new SelectDisplayMenu(type, getChosenTexture(target))
            {
                onSelected = onExteriorSelected
            };
            Game1.activeClickableMenu = menu;
        }
        
        private string recentTarget = null;
        private void onExteriorSelected(string type, string choice) { onExteriorSelected(type, choice, true); }
        private void onExteriorSelected( string type, string choice, bool updateChosen )
        {
            Log.debug("onExteriorSelected: " + recentTarget + " " + type + " " + choice);
            
            Texture2D tex = getTextureForChoice(type, choice);
            if (tex == null)
            {
                Log.warn("Failed to load chosen texture '" + choice + "' for building type '" + type + "'.");
                return;
            }
            if (updateChosen)
            {
                savedExteriors.chosen[recentTarget] = choice;

                if ( Game1.IsMultiplayer )
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(1);
                        writer.Write(recentTarget);
                        writer.Write(choice);

                        Log.trace("Broadcasting choice");
                        SpaceCore.Networking.BroadcastMessage(MSG_CHOICES, stream.ToArray());
                    }
                }
            }

            if ( recentTarget == "FarmHouse" || recentTarget == "Greenhouse" )
            {
                housesHybrid = null;
                typeof(Farm).GetField("houseTextures").SetValue(null, getHousesTexture());
            }
            else
            {
                foreach ( Building building in Game1.getFarm().buildings )
                {
                    if (building.buildingType.Value == type && building.nameOfIndoors == recentTarget)
                    {
                        building.texture = new Lazy<Texture2D>(() => tex);
                        break;
                    }
                }
            }
        }

        private void syncTexturesWithChoices()
        {
            foreach (var choice in savedExteriors.chosen)
            {
                recentTarget = choice.Key;
                Log.debug("Saved choice: " + choice.Key + " " + choice.Value);

                string type = null;
                if (recentTarget == "FarmHouse" || recentTarget == "Greenhouse")
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
                    onExteriorSelected(type, choice.Value, false);
            }
        }

        public static string getChosenTexture( string target )
        {
            return savedExteriors.chosen.ContainsKey(target) ? savedExteriors.chosen[target] : "/";
        }

        public static Texture2D getTextureForChoice(string type, string choice)
        {
            try
            {
                if (choice == "/")
                    return Game1.content.Load<Texture2D>("Buildings/" + type);
                else if (choice.StartsWith(SEASONAL_INDICATOR))
                    return content.Load<Texture2D>(choice.Substring(SEASONAL_INDICATOR.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return content.Load<Texture2D>(choice + "/" + type);
            }
            catch (ContentLoadException)
            {
                if (choice.StartsWith(SEASONAL_INDICATOR))
                    return loadPng(choice.Substring(SEASONAL_INDICATOR.Length) + "/" + Game1.currentSeason + "/" + type);
                else
                    return loadPng(choice + "/" + type);
            }
        }

        private static Texture2D loadPng( string path )
        {
            FileStream fs = File.Open(Path.Combine(instance.Helper.DirectoryPath, "Buildings", path + ".png" ), FileMode.Open);
            Texture2D tex = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fs);
            fs.Dispose();
            return tex;
        }

        private static Texture2D housesHybrid = null;
        private static Texture2D getHousesTexture()
        {
            if (housesHybrid != null)
                return housesHybrid;

            Log.trace("Creating hybrid farmhouse/greenhouse texture");

            Texture2D baseTex = Farm.houseTextures;
            Rectangle houseRect = new Rectangle( 0, 0, 160, baseTex.Height );// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "houseSource");
            Rectangle greenhouseRect = new Rectangle(160, 0, 112, baseTex.Height);// instance.Helper.Reflection.GetPrivateValue<Rectangle>(farm, "greenhouseSource");

            GraphicsDevice dev = Game1.graphics.GraphicsDevice;
            RenderTarget2D ret = new RenderTarget2D(dev, baseTex.Width, baseTex.Height);
            SpriteBatch b = Game1.spriteBatch;
            dev.SetRenderTarget(ret);
            {
                dev.Clear(Color.Transparent);
                b.Begin();
                b.Draw(getTextureForChoice("houses", getChosenTexture("FarmHouse")), houseRect, houseRect, Color.White);
                b.Draw(getTextureForChoice("houses", getChosenTexture("Greenhouse")), greenhouseRect, greenhouseRect, Color.White);
                b.End();
            }
            dev.SetRenderTarget(null);

            housesHybrid = ret;
            return ret;
        }
    }
}
