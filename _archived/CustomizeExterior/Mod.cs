using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomizeExterior.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace CustomizeExterior
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The building identifier and time which the player last clicked, if any.</summary>
        /// <remarks>This should only be used via <see cref="IsOpenMenuClick(string)"/>.</remarks>
        private Tuple<string, DateTime> LastRightClick;

        private readonly TimeSpan ClickWindow = TimeSpan.FromMilliseconds(250);
        private readonly Dictionary<string, List<string>> AssetsByBuildingType = new();
        private string PrevSeason = "";


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Log.Monitor = this.Monitor;

            this.CompileChoices();

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saved += this.OnSaved;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Player.Warped += this.OnWarped;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player loads a save slot.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            LegacyDataMigrator.OnSaveLoaded(this.Helper.Data);

            this.SyncTexturesWithChoices();
        }

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaved(object sender, SavedEventArgs e)
        {
            LegacyDataMigrator.OnSaved();
        }

        /// <summary>Raised after a player warps to a new location. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer && e.NewLocation is Farm)
                this.SyncTexturesWithChoices();
        }

        private void OnSeasonChange()
        {
            Log.Debug("Season change, syncing textures...");
            this.SyncTexturesWithChoices();
        }

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

                // customize building
                if (Game1.currentLocation is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        Rectangle tileBounds = new Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);
                        if (tileBounds.Contains(tile))
                        {
                            Log.Trace($"Right clicked a building {this.GetBuildingLabel(building)}.");

                            if (this.IsOpenMenuClick(building))
                                this.TryShowCustomizationMenuForBuilding(building);
                            return;
                        }
                    }
                }

                // customize farmhouse
                if (Game1.currentLocation is Farm farm)
                {
                    Rectangle house = farm.GetHouseRect();
                    if (house.Contains(tile))
                    {
                        Log.Trace("Right clicked the house.");

                        if (this.IsOpenMenuClick(nameof(FarmHouse)))
                            this.TryShowCustomizationMenuForFarmhouse(farm);
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

                    List<string> forType = this.AssetsByBuildingType.GetOrDefault(typeStr) ?? new();

                    if (!forType.Contains(choiceStr))
                        forType.Add(choiceStr);
                    if (!this.AssetsByBuildingType.ContainsKey(typeStr))
                        this.AssetsByBuildingType.Add(typeStr, forType);

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
                            List<string> forType = this.AssetsByBuildingType.GetOrDefault(typeStr) ?? new();

                            if (!forType.Contains(choiceStr))
                                forType.Add(choiceStr);
                            if (!this.AssetsByBuildingType.ContainsKey(typeStr))
                                this.AssetsByBuildingType.Add(typeStr, forType);

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

        private void TryShowCustomizationMenuForBuilding(Building building)
        {
            string debugLabel = this.GetBuildingLabel(building);

            this.TryShowCustomizationMenu(
                buildingType: building.buildingType.Value,
                debugLabel: debugLabel,
                currentAsset: building.GetAssetPackName(),
                onSelected: choice =>
                {
                    Log.Trace($"onExteriorSelected: {debugLabel} => {choice}");

                    building.SetAssetPack(choice);
                    this.UpdateTextureForBuilding(building);
                }
            );
        }

        private void TryShowCustomizationMenuForFarmhouse(Farm farm)
        {
            string debugLabel = "farmhouse";

            this.TryShowCustomizationMenu(
                buildingType: "houses",
                debugLabel: debugLabel,
                currentAsset: farm.GetFarmhouseAssetPackName(),
                onSelected: folderName =>
                {
                    Log.Trace($"onExteriorSelected: {debugLabel} => {folderName}");

                    farm.SetFarmhouseAssetPackName(folderName);
                    this.UpdateTextureForFarmhouse(farm);
                }
            );
        }

        private void TryShowCustomizationMenu(string buildingType, string debugLabel, string currentAsset, Action<string> onSelected)
        {
            // get available choices
            if (!this.AssetsByBuildingType.TryGetValue(buildingType, out var choices))
            {
                Log.Trace($"Target: {debugLabel}, but no custom textures found.");
                return;
            }
            Log.Trace($"Target: {debugLabel}, found {choices.Count} textures: '{string.Join("', '", choices)}'.");

            // open menu & save changes
            var menu = new SelectDisplayMenu(buildingType, currentAsset, this.AssetsByBuildingType[buildingType], this.GetBuildingTexture)
            {
                OnSelected = onSelected
            };
            Game1.activeClickableMenu = menu;
        }

        private void SyncTexturesWithChoices()
        {
            Farm farm = Game1.getFarm();

            this.UpdateTextureForFarmhouse(farm);
            foreach (Building building in Game1.getFarm().buildings)
                this.UpdateTextureForBuilding(building);
        }

        /// <summary>Update the farmhouse's texture to match the configured asset pack, if needed.</summary>
        /// <param name="farm">The farm whose farmhouse to update.</param>
        private void UpdateTextureForFarmhouse(Farm farm)
        {
            this.UpdateTexture(
                buildingType: "houses",
                folderName: farm.GetFarmhouseAssetPackName(),
                debugLabel: "farmhouse",
                update: texture => typeof(Farm).GetField(nameof(Farm.houseTextures)).SetValue(null, texture) // TODO when updated to Stardew Valley 1.5.5: Farm.houseTextures = texture
            );
        }

        /// <summary>Update a building's texture to match the configured asset pack, if needed.</summary>
        /// <param name="building">The building to update.</param>
        private void UpdateTextureForBuilding(Building building)
        {
            this.UpdateTexture(
                buildingType: building.buildingType.Value,
                folderName: building.GetAssetPackName(),
                debugLabel: this.GetBuildingLabel(building),
                update: texture => building.texture = new Lazy<Texture2D>(() => texture)
            );
        }

        /// <summary>Update a building or farmhouse texture to match the configured asset pack, if needed.</summary>
        /// <param name="buildingType">The building type.</param>
        /// <param name="folderName">The name of the asset pack's folder.</param>
        /// <param name="debugLabel">A human-readable label for the building in debug logs.</param>
        /// <param name="update">Apply the given texture to the building or farmhouse.</param>
        private void UpdateTexture(string buildingType, string folderName, string debugLabel, Action<Texture2D> update)
        {
            Texture2D texture = this.GetBuildingTexture(buildingType, folderName);
            if (texture == null) // should never happen since it defaults to the game asset
                Log.Warn($"Failed to load chosen texture '{folderName}' for building {debugLabel}.");
            else
                update(texture);
        }

        /// <summary>Get the building texture for an asset pack.</summary>
        /// <param name="buildingType">The building type.</param>
        /// <param name="folderName">The name of the asset pack's folder.</param>
        private Texture2D GetBuildingTexture(string buildingType, string folderName)
        {
            // get from asset pack
            if (folderName is not (null or "/"))
            {
                foreach (string path in new[] { $"{Game1.currentSeason}/{buildingType}", buildingType })
                {
                    foreach (string extension in new[] { "png", "xnb" })
                    {
                        try
                        {
                            return this.Helper.Content.Load<Texture2D>($"Buildings/{folderName}/{path}.{extension}");
                        }
                        catch (ContentLoadException)
                        {
                            // skip if file doesn't exist
                        }
                    }
                }
            }

            // get default texture
            return this.Helper.Content.Load<Texture2D>($"Buildings/{buildingType}", ContentSource.GameContent);
        }

        /// <summary>Get a human-readable label for a building in debug logs.</summary>
        /// <param name="building">The building instance.</param>
        private string GetBuildingLabel(Building building)
        {
            return $"{building.buildingType.Value} at ({building.tileX.Value}, {building.tileY.Value})";
        }
    }
}
