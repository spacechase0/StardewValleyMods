using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Spacechase.Shared.Patching;

using SpaceCore.Events;

using SpaceShared;
using SpaceShared.APIs;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

using TheftOfTheWinterStar.Framework;
using TheftOfTheWinterStar.Patches;

using xTile;
using xTile.Tiles;

using SObject = StardewValley.Object;

namespace TheftOfTheWinterStar
{
    /// <summary>The mod entry class.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        /// <summary>The ID for the introductory event with Lewis.</summary>
        private const int EventId = 91000;

        /// <summary>The number of boss keys used on the boss door.</summary>
        private static int BossKeysUsed;

        /// <summary>The saved player progress in the dungeons.</summary>
        private SaveData SaveData = new();

        /// <summary>The boss's health bar background.</summary>
        private Texture2D BossBarBg;

        /// <summary>The boss's health bar foreground.</summary>
        private Texture2D BossBarFg;

        /// <summary>Whether the player has started the boss fight.</summary>
        private bool StartedBoss;

        /// <summary>The projectiles fired by the boss which are still active.</summary>
        private List<Projectile> PrevProjectiles;

        /// <summary>The unique key in <see cref="TerrainFeature.modData"/> which contains the original crop data for the Tempus Globe logic.</summary>
        private string PrevCropDataKey;

        /// <summary>The names of the custom locations to load.</summary>
        private readonly string[] LocationNames = {
            "Entrance",
            "Arena",
            "Branch1",
            "ItemPuzzle",
            "Bonus1",
            "Bonus2",
            "WeaponRoom",
            "KeyRoom",
            "Branch2",
            "PushPuzzle",
            "Bonus3",
            "Maze",
            "Bonus4",
            "Boss"
        };

        /// <summary>The locations and tiles on which to drop decorations.</summary>
        private readonly IDictionary<string, Vector2[]> DecoSpots = new Dictionary<string, Vector2[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["BusStop"] = new Vector2[] { new(5, 8), new(9, 10), new(10, 14) },
            ["Backwoods"] = new Vector2[] { new(40, 30), new(32, 31), new(25, 29) },
            ["Tunnel"] = new Vector2[] { new(33, 10), new(23, 9), new(10, 8) }
        };


        /*********
        ** Accessors
        *********/
        public static Mod Instance;
        internal static IJsonAssetsApi Ja;


        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.BossBarBg = this.Helper.ModContent.Load<Texture2D>("assets/bossbar-bg.png");
            this.BossBarFg = this.Helper.ModContent.Load<Texture2D>("assets/bossbar-fg.png");
            this.PrevCropDataKey = $"{this.ModManifest.UniqueID}/prev-data";

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveCreated += this.OnSaveCreated;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            SpaceEvents.OnBlankSave += this.OnBlankSave;
            SpaceEvents.ActionActivated += this.OnActionActivated;
            SpaceEvents.BombExploded += this.BombExploded;

            HarmonyPatcher.Apply(this,
                new HoeDirtPatcher()
            );
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            // scatter decorations
            if (this.IsPossibleDecoSpotsMap(e.NameWithoutLocale))
                e.Edit(asset =>
                {
                    if (this.TryGetDecoSpots(asset, out Vector2[] decoSpots))
                        this.ScatterDecorationsIfNeeded(asset.Data as Map, decoSpots);
                });

            // add Frosty Stardrop recipe
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes") && Mod.Ja is not null)
            {
                e.Edit(static asset =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict.Add("Frosty Stardrop", $"{Mod.Ja.GetObjectId("Frosty Stardrop Piece")} 5/Field/434/false/null/{I18n.Recipe_FrostyStardrop_Name()}");
                });
            }

            // add map strings
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromMaps"))
            {
                e.Edit(static asset =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict.Add("FrostDungeon.LockedEntrance", I18n.MapMessages_LockedEntrance());
                    dict.Add("FrostDungeon.Locked", I18n.MapMessages_LockedDoor());
                    dict.Add("FrostDungeon.LockedBoss", I18n.MapMessages_LockedBoss());
                    dict.Add("FrostDungeon.Unlock", I18n.MapMessages_Unlocked());
                    dict.Add("FrostDungeon.ItemPuzzle", I18n.MapMessages_ItemPuzzle());
                    dict.Add("FrostDungeon.Target", I18n.MapMessages_Target());
                    dict.Add("FrostDungeon.Trail0", I18n.MapMessages_TrailLights());
                    dict.Add("FrostDungeon.Trail1", I18n.MapMessages_TrailCandyCane());
                    dict.Add("FrostDungeon.Trail2", I18n.MapMessages_TrailOrnaments());
                    dict.Add("FrostDungeon.Trail3", I18n.MapMessages_TrailTree());
                });
            }

            // edit tunnel map
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Tunnel"))
            {
                e.Edit(asset =>
                {
                    var overlay = Game1.currentSeason == "winter" && Game1.dayOfMonth < 25
                        ? this.Helper.ModContent.Load<Map>("assets/OverlayPortal.tmx")
                        : this.Helper.ModContent.Load<Map>("assets/OverlayPortalLocked.tmx");

                    asset
                        .AsMap()
                        .PatchMap(overlay, targetArea: new Rectangle(7, 4, 3, 3));
                });
            }
        }

        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            Mod.Ja.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "json-assets"), this.Helper.Translation);
            Mod.Ja.IdsFixed += this.OnIdsFixed;
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveCreated"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveCreated(object sender, SaveCreatedEventArgs e)
        {
            this.SaveData = new SaveData();
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.SaveData = this.Helper.Data.ReadSaveData<SaveData>("FrostDungeon.SaveData") ?? new SaveData();
        }

        /// <inheritdoc cref="IGameLoopEvents.Saving"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (Game1.IsMasterGame)
                this.Helper.Data.WriteSaveData("FrostDungeon.SaveData", this.SaveData);
        }

        /// <inheritdoc cref="IGameLoopEvents.UpdateTicked"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            GameLocation location = Game1.currentLocation;

            switch (location?.Name)
            {
                case "FrostDungeon.Arena":
                    if ((this.SaveData.ArenaStage is ArenaStage.Stage1 or ArenaStage.Stage2) && !location.characters.Any(npc => npc is Monster))
                    {
                        Game1.playSound("questcomplete");
                        switch (this.SaveData.ArenaStage)
                        {
                            case ArenaStage.Stage1:
                            {
                                this.SaveData.ArenaStage = ArenaStage.Finished1;
                                int key = Mod.Ja.GetObjectId("Festive Key");
                                var pos = new Vector2(6, 13);
                                var chest = new Chest(0, new List<Item>(new Item[] { new SObject(key, 1) }), pos);
                                location.overlayObjects[pos] = chest;
                                Game1.playSound("questcomplete");
                            }
                            break;

                            case ArenaStage.Stage2:
                            {
                                this.SaveData.ArenaStage = ArenaStage.Finished2;
                                int stardropPiece = Mod.Ja.GetObjectId("Frosty Stardrop Piece");
                                var pos = new Vector2(13, 13);
                                var chest = new Chest(0, new List<Item>(new Item[] { new SObject(stardropPiece, 1) }), pos);
                                location.overlayObjects[pos] = chest;
                                Game1.playSound("questcomplete");
                            }
                            break;
                        }
                    }
                    break;

                case "FrostDungeon.Bonus4":
                    if (!this.SaveData.DidProjectilePuzzle)
                    {
                        var projectiles = location.projectiles.ToList();
                        if (this.PrevProjectiles != null)
                        {
                            foreach (var projectile in projectiles)
                            {
                                if (this.PrevProjectiles.Contains(projectile))
                                    this.PrevProjectiles.Remove(projectile);
                            }

                            foreach (var projectile in this.PrevProjectiles)
                            {
                                if (projectile.getBoundingBox().Intersects(new Rectangle((int)(8.5 * Game1.tileSize), (int)(8.5 * Game1.tileSize), Game1.tileSize * 2, Game1.tileSize * 2)))
                                {
                                    int stardropPiece = Mod.Ja.GetObjectId("Frosty Stardrop Piece");
                                    var pos = new Vector2(9, 13);
                                    var chest = new Chest(0, new List<Item>(new Item[] { new SObject(stardropPiece, 1) }), pos);
                                    location.overlayObjects[pos] = chest;
                                    this.SaveData.DidProjectilePuzzle = true;
                                    break;
                                }
                            }
                        }
                        this.PrevProjectiles = projectiles;
                    }
                    break;

                case "FrostDungeon.Boss":
                    if (this.StartedBoss & !this.SaveData.BeatBoss)
                    {
                        if (location.characters.Count(npc => npc is Monster) <= 0)
                        {
                            this.StartedBoss = false;
                            this.SaveData.BeatBoss = true;
                            Game1.playSound("achievement");

                            foreach (var player in Game1.getAllFarmers())
                            {
                                if (!player.knowsRecipe("Tempus Globe"))
                                    player.craftingRecipes.Add("Tempus Globe", 0);
                                var npcList = new List<NPC>();
                                foreach (var npc in Utility.getAllCharacters(npcList))
                                    player.changeFriendship(250, npc);
                            }

                            Game1.drawObjectDialogue(I18n.FinalBoss_VictoryMessage());
                        }
                    }
                    break;
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.DayStarted"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // update maps
            this.Helper.GameContent.InvalidateCache("Maps/Tunnel");
            foreach (string mapName in this.DecoSpots.Keys)
                this.Helper.GameContent.InvalidateCache($"Maps/{mapName}");

            // apply Tempus Globe logic
            int seasonalDelimiter = Mod.Ja.GetBigCraftableId("Tempus Globe");
            Utility.ForAllLocations((loc) =>
            {
                if (!this.IsFarm(loc))
                    return;
                foreach (var pair in loc.Objects.Pairs)
                {
                    var obj = pair.Value;
                    if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                    {
                        for (int ix = -2; ix <= 2; ++ix)
                        {
                            for (int iy = -2; iy <= 2; ++iy)
                            {
                                var key = new Vector2(pair.Key.X + ix, pair.Key.Y + iy);
                                if (!loc.terrainFeatures.TryGetValue(key, out TerrainFeature feature))
                                    continue;
                                if (feature is HoeDirt dirt)
                                {
                                    dirt.state.Value = HoeDirt.watered;
                                    dirt.updateNeighbors(loc, key);
                                }
                            }
                        }

                        loc.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, pair.Key * 64f + new Vector2(sbyte.MinValue, sbyte.MinValue), false, false)
                        {
                            color = Color.White * 0.4f,
                            delayBeforeAnimationStart = Game1.random.Next(1000),
                            id = pair.Key.X * 4000f + pair.Key.Y
                        });
                    }
                }
            });
        }

        /// <inheritdoc cref="IGameLoopEvents.DayEnding"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            // save dungeon progress
            if (this.SaveData != null)
            {
                this.SaveData.ArenaStage = this.SaveData.ArenaStage switch
                {
                    ArenaStage.Stage1 => ArenaStage.NotTriggered,
                    ArenaStage.Stage2 => ArenaStage.Finished1,
                    _ => this.SaveData.ArenaStage
                };
            }

            // clear custom data from dungeon
            {
                var arena = Game1.getLocationFromName("FrostDungeon.Arena");
                arena.characters.Clear();
                var bossArea = Game1.getLocationFromName("FrostDungeon.Boss");
                if (this.SaveData?.BeatBoss != true)
                {
                    bossArea.characters.Clear();
                    bossArea.netObjects.Clear();
                    this.StartedBoss = false;
                }
            }

            // prevent crops from withering
            int tempusGlobeId = Mod.Ja.GetBigCraftableId("Tempus Globe");

            Utility.ForAllLocations((loc) =>
            {
                if (!this.IsFarm(loc))
                    return;
                // find Tempus Globes coverage
                HashSet<Vector2> covered = new();
                foreach (var pair in loc.Objects.Pairs)
                {
                    var obj = pair.Value;
                    if (obj.bigCraftable.Value && obj.ParentSheetIndex == tempusGlobeId)
                    {
                        for (int ix = -2; ix <= 2; ++ix)
                        {
                            for (int iy = -2; iy <= 2; ++iy)
                                covered.Add(new Vector2(pair.Key.X + ix, pair.Key.Y + iy));
                        }
                    }
                }

                // prevent crop withering
                if (covered.Any())
                {
                    foreach (var pair in loc.terrainFeatures.Pairs)
                    {
                        if (pair.Value is not HoeDirt dirt)
                            continue;

                        if (dirt.crop != null && covered.Contains(pair.Key))
                        {
                            if (dirt.crop.seasonsToGrowIn.Count < 4)
                            {
                                this.SaveCropData(dirt);
                                dirt.crop.seasonsToGrowIn.Set(new[] { "spring", "summer", "fall", "winter" });
                            }
                        }
                        else
                            this.RestoreCropDataIfNeeded(dirt);
                    }
                }
            });

        }

        /// <summary>Get whether a location can be farmed.</summary>
        /// <param name="location">The location to check.</param>
        private bool IsFarm(GameLocation location)
        {
            return
                location.IsFarm || location.IsGreenhouse
                || location is Farm or IslandWest;
        }

        /// <summary>Back up the original crop data before it's changed by a Tempus Globe.</summary>
        /// <param name="dirt">The dirt whose crop data to save.</param>
        private void SaveCropData(HoeDirt dirt)
        {
            if (dirt.crop != null)
                dirt.modData[this.PrevCropDataKey] = dirt.crop.indexOfHarvest.Value.ToString(CultureInfo.InvariantCulture) + "," + string.Join(",", dirt.crop.seasonsToGrowIn);
            else
                dirt.modData.Remove(this.PrevCropDataKey);
        }

        /// <summary>Restore the original crop data when a Tempus Globe no longer applies, if possible.</summary>
        /// <param name="dirt">The dirt whose crop data to save.</param>
        private void RestoreCropDataIfNeeded(HoeDirt dirt)
        {
            if (dirt.crop != null && dirt.modData.TryGetValue(this.PrevCropDataKey, out string prevData))
            {
                string[] parts = prevData.Split(',');

                if (dirt.crop.indexOfHarvest.Value.ToString(CultureInfo.InvariantCulture) == parts[0])
                    dirt.crop.seasonsToGrowIn.Set(parts.Skip(1).ToArray());
            }

            dirt.modData.Remove(this.PrevCropDataKey);
        }

        /// <inheritdoc cref="IJsonAssetsApi.IdsFixed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsFixed(object sender, EventArgs e)
        {
            Log.Debug("Adding frost dungeon loot");

            int stardropPiece = Mod.Ja.GetObjectId("Frosty Stardrop Piece");
            int scepter = Mod.Ja.GetWeaponId("Festive Scepter");
            int key = Mod.Ja.GetObjectId("Festive Key");
            int keyHalfB = Mod.Ja.GetObjectId("Festive Big Key (A)");
            Log.Trace("IDs for chests: " + stardropPiece + " " + scepter + " " + key + " " + keyHalfB);

            foreach (string locName in this.LocationNames)
            {
                GameLocation loc = Game1.getLocationFromName("FrostDungeon." + locName);
                switch (locName)
                {
                    case "Entrance":
                        /*
                        Rectangle r = new Rectangle(9, 6, 9, 5);
                        for (int i = 0; i < 12; ++i)
                        {
                            var pos = new Vector2(r.X + Game1.random.Next(r.Width), r.Y + Game1.random.Next(r.Height));
                            var breakable = new BreakableContainer(pos, BreakableContainer.frostBarrel, null);
                            loc.objects[pos] = breakable;
                        }
                        */
                        break;

                    case "Bonus1" or "Bonus2" or "Bonus3":
                    {
                        var pos = new Vector2(9, 9);
                        if (locName == "Bonus2")
                        {
                            pos.X = 13;
                        }
                        var chest = new Chest(0, new List<Item>(new Item[] { new SObject(stardropPiece, 1) }), pos);
                        loc.overlayObjects[pos] = chest;
                    }
                    break;

                    case "WeaponRoom":
                    {
                        var pos = new Vector2(13, 9);
                        var chest = new Chest(0, new List<Item>(new Item[] { new MeleeWeapon(scepter) }), pos);
                        loc.overlayObjects[pos] = chest;
                    }
                    break;

                    case "KeyRoom":
                    {
                        var pos = new Vector2(13, 9);
                        var chest = new Chest(0, new List<Item>(new Item[] { new SObject(key, 1) }), pos);
                        loc.overlayObjects[pos] = chest;
                    }
                    break;

                    case "Maze":
                    {
                        var pos = new Vector2(20, 26);
                        var chest = new Chest(0, new List<Item>(new Item[] { new SObject(keyHalfB, 1) }), pos);
                        loc.overlayObjects[pos] = chest;
                    }
                    break;
                    case "Branch2":
                        /*
                        Rectangle r = new Rectangle(8, 6, 4, 3);
                        for ( int i = 0; i < 4; ++i )
                        {
                            var pos = new Vector2(r.X + Game1.random.Next(r.Width), r.Y + Game1.random.Next(r.Height));
                            var breakable = new BreakableContainer(pos, BreakableContainer.frostBarrel, null);
                            loc.objects[pos] = breakable;
                        }
                        */
                        break;
                }
            }
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if (e.NewLocation.Name.StartsWith("FrostDungeon."))
            {
                for (int ix = 0; ix < e.NewLocation.Map.Layers[0].LayerWidth; ++ix)
                {
                    for (int iy = 0; iy < e.NewLocation.Map.Layers[0].LayerHeight; ++iy)
                    {
                        string prop = e.NewLocation.doesTileHaveProperty(ix, iy, "UnlockId", "Buildings");

                        if (!string.IsNullOrEmpty(prop) && e.Player.mailReceived.Contains("FrostDungeon.Lock." + prop))
                        {
                            string newAction = e.Player.currentLocation.doesTileHaveProperty(ix, iy, "UnlockAction", "Buildings");
                            e.NewLocation.setTileProperty(ix, iy, "Buildings", "Action", newAction);
                            e.NewLocation.setMapTileIndex(ix, iy - 2, 48, "Buildings");
                        }
                    }
                }
            }

            switch (e.NewLocation.Name)
            {
                case "Farm":
                    if (!Game1.player.eventsSeen.Contains(Mod.EventId) && Game1.currentSeason == "winter" && Game1.dayOfMonth < 25)
                    {
                        string eventStr = $"continue/64 15/farmer 64 16 2 Lewis 64 18 0/skippable/pause 1500/speak Lewis \"{I18n.Event_LewisSpeech()}\"/pause 500/end";
                        e.NewLocation.currentEvent = new Event(eventStr, Mod.EventId);
                        Game1.eventUp = true;
                        Game1.displayHUD = false;
                        Game1.player.CanMove = false;
                        Game1.player.showNotCarrying();

                        Game1.player.eventsSeen.Add(Mod.EventId);
                    }
                    break;

                case "FrostDungeon.Boss":
                    if (!this.StartedBoss && !this.SaveData.BeatBoss)
                    {
                        var witch = new Witch();
                        e.NewLocation.characters.Add(witch);

                        var dummySpeaker = new NPC(new AnimatedSprite("Characters\\Penny"), new Vector2(-1, -1), "", 0, "Witch", false, null, witch.Portrait);
                        var dialogue = new Dialogue(I18n.FinalBoss_Speech(), dummySpeaker);
                        var dialogueBox = new DialogueBox(dialogue);

                        Game1.activeClickableMenu = dialogueBox;
                        Game1.dialogueUp = true;
                        Game1.player.Halt();
                        Game1.player.CanMove = false;
                        Game1.currentSpeaker = dummySpeaker;

                        this.StartedBoss = true;
                    }
                    break;
            }
        }

        private bool IsPossibleDecoSpotsMap(IAssetName assetName)
            => assetName.StartsWith("Maps/") && this.DecoSpots.ContainsKey(Path.GetFileNameWithoutExtension(assetName.BaseName));

        /// <summary>Get the <see cref="DecoSpots"/> for a map asset, if any.</summary>
        /// <param name="asset">The map asset being edited.</param>
        /// <param name="decoSpots">The tiles on which to drop decorations.</param>
        private bool TryGetDecoSpots(IAssetInfo asset, out Vector2[] decoSpots)
        {
            // make sure it's a map asset
            if (!typeof(Map).IsAssignableFrom(asset.DataType))
            {
                decoSpots = null;
                return false;
            }

            // check for deco spots
            string mapName = Path.GetFileNameWithoutExtension(asset.NameWithoutLocale.BaseName);
            return
                this.DecoSpots.TryGetValue(mapName, out decoSpots)
                && decoSpots.Any();
        }

        /// <summary>Drop decorations on the given tiles.</summary>
        /// <param name="map">The map to edit.</param>
        /// <param name="spots">The tiles on which to drop decorations.</param>
        private void ScatterDecorationsIfNeeded(Map map, Vector2[] spots)
        {
            if (Game1.currentSeason == "winter" && Game1.dayOfMonth < 25 && !this.SaveData.BeatBoss)
            {
                TileSheet tilesheet = map.TileSheets.FirstOrDefault(p => p.ImageSource.Contains("trail-decorations"));
                if (tilesheet == null)
                {
                    // AddTileSheet sorts the tilesheets by ID after adding them.
                    // The game sometimes refers to tilesheets by their index (such as in Beach.fixBridge)
                    // Prepending this to the ID should ensure that this tilesheet is added to the end,
                    // which preserves the normal indices of the tilesheets.
                    char comeLast = '\u03a9'; // Omega

                    tilesheet = new TileSheet(map, this.Helper.ModContent.GetInternalAssetName("assets/trail-decorations.png").BaseName, new xTile.Dimensions.Size(2, 2), new xTile.Dimensions.Size(16, 16));
                    tilesheet.Id = comeLast + tilesheet.Id;
                    map.AddTileSheet(tilesheet);
                    map.LoadTileSheets(Game1.mapDisplayDevice);

                    Random r = new Random((int)Game1.uniqueIDForThisGame + map.assetPath.GetHashCode());
                    var buildingsLayer = map.GetLayer("Buildings");
                    foreach (var spot in spots)
                    {
                        int tile = r.Next(4);
                        buildingsLayer.Tiles[(int)spot.X, (int)spot.Y] = new StaticTile(buildingsLayer, tilesheet, BlendMode.Alpha, tile)
                        {
                            Properties = { ["Action"] = $"Message \"FrostDungeon.Trail{tile}\"" }
                        };
                    }
                }
            }
            else
            {
                var layer = map.GetLayer("Buildings");
                foreach (Vector2 spot in spots)
                    layer.Tiles[(int)spot.X, (int)spot.Y] = null;
            }
        }

        /// <inheritdoc cref="IPlayerEvents.InventoryChanged"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.Player.knowsRecipe("Frosty Stardrop"))
            {
                foreach (var item in e.Added)
                {
                    if (item is SObject obj && obj.ParentSheetIndex == Mod.Ja.GetObjectId("Frosty Stardrop Piece"))
                    {
                        e.Player.craftingRecipes.Add("Frosty Stardrop", 0);
                        this.Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
                    }
                }
            }
            else
            {
                this.Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
            }
        }

        /// <inheritdoc cref="SpaceEvents.OnBlankSave"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBlankSave(object sender, EventArgs e)
        {
            Log.Debug("Adding frost dungeon");

            foreach (string locName in this.LocationNames)
            {
                GameLocation location = new(this.Helper.ModContent.GetInternalAssetName($"assets/{locName}.tmx").BaseName, $"FrostDungeon.{locName}");
                Game1.locations.Add(location);
            }
        }

        /// <inheritdoc cref="SpaceEvents.ActionActivated"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnActionActivated(object sender, EventArgsAction e)
        {
            if (sender is not Farmer farmer)
                return;

            GameLocation location = farmer.currentLocation;

            if (e.ActionString == "Message \"FrostDungeon.Locked\"")
            {
                int key = Mod.Ja.GetObjectId("Festive Key");
                if (Utility.IsNormalObjectAtParentSheetIndex(farmer.ActiveObject, key))
                {
                    farmer.removeFirstOfThisItemFromInventory(key);
                    farmer.mailReceived.Add("FrostDungeon.Locked." + location.doesTileHaveProperty(e.Position.X, e.Position.Y, "Buildings", "UnlockId"));

                    string newAction = location.doesTileHaveProperty(e.Position.X, e.Position.Y, "UnlockAction", "Buildings");
                    location.setTileProperty(e.Position.X, e.Position.Y, "Buildings", "Action", newAction);
                    location.setMapTileIndex(e.Position.X, e.Position.Y - 2, 48, "Buildings");

                    Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:FrostDungeon.Unlock"));
                    Game1.playSound("crystal");

                    e.Cancel = true;
                }
            }
            else
            {
                switch (e.Action)
                {
                    case "ActivateArena" when location.Name == "FrostDungeon.Arena":
                    {
                        Log.Trace("Activate arena: Stage " + this.SaveData.ArenaStage);
                        Game1.playSound("batScreech");
                        Game1.playSound("rockGolemSpawn");
                        switch (this.SaveData.ArenaStage)
                        {
                            case ArenaStage.NotTriggered:
                                this.SaveData.ArenaStage = ArenaStage.Stage1;
                                for (int i = 0; i < 9; ++i)
                                {
                                    int cx = e.Position.X, cy = e.Position.Y;
                                    int dx = (int)(Math.Cos(Math.PI * 2 / 9 * i) * 5);
                                    int dy = (int)(Math.Sin(Math.PI * 2 / 9 * i) * 5);
                                    int x = cx + dx, y = cy + dy;
                                    x *= Game1.tileSize;
                                    y *= Game1.tileSize;

                                    Monster monster = (i % 3) switch
                                    {
                                        0 => new Ghost(new Vector2(x, y)),
                                        1 => new Skeleton(new Vector2(x, y)),
                                        2 => new DustSpirit(new Vector2(x, y)),
                                        _ => null
                                    };

                                    if (monster != null)
                                        location.addCharacter(monster);
                                }
                                break;

                            case ArenaStage.Finished1:
                                this.SaveData.ArenaStage = ArenaStage.Stage2;
                                for (int i = 0; i < 3; ++i)
                                {
                                    int cx = e.Position.X, cy = e.Position.Y;
                                    int dx = (int)(Math.Cos(Math.PI * 2 / 3 * i) * 4);
                                    int dy = (int)(Math.Sin(Math.PI * 2 / 3 * i) * 4);
                                    int x = cx + dx, y = cy + dy;
                                    x *= Game1.tileSize;
                                    y *= Game1.tileSize;

                                    if (i % 2 == 0)
                                        location.addCharacter(new Bat(new Vector2(x, y), 77377));
                                }
                                location.addCharacter(new DinoMonster(new Vector2(9 * Game1.tileSize, 8 * Game1.tileSize)));
                                break;
                        }
                    }
                    break;

                    case "ItemPuzzle":
                    {
                        string[] tokens = e.ActionString.Split(' ');
                        int item = int.Parse(tokens[1]);
                        if (Utility.IsNormalObjectAtParentSheetIndex(farmer.ActiveObject, item))
                        {
                            farmer.removeFirstOfThisItemFromInventory(item);
                            location.removeTileProperty(e.Position.X, e.Position.Y, "Buildings", "Action");

                            int warpIndex = location.Map.GetLayer("Buildings").Tiles[e.Position.X, e.Position.Y].TileIndex - 32;
                            var back = location.Map.GetLayer("Back");
                            back.Tiles[e.Position.X, e.Position.Y + 3] = new StaticTile(back, location.Map.TileSheets[0], BlendMode.Additive, warpIndex);

                            var warp = new Warp(e.Position.X, e.Position.Y + 3, tokens[2], 7, 9, false);
                            location.warps.Add(warp);

                            Game1.playSound("secret1");
                        }
                        else
                            Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:FrostDungeon.ItemPuzzle"));
                    }
                    break;

                    case "BossKeyHalf":
                    {
                        string[] tokens = e.ActionString.Split(' ');

                        int key = Mod.Ja.GetObjectId("Festive Big Key (" + tokens[1] + ")");
                        if (Utility.IsNormalObjectAtParentSheetIndex(farmer.ActiveObject, key))
                        {
                            farmer.removeFirstOfThisItemFromInventory(key);

                            location.removeTile(e.Position.X, e.Position.Y - 1, "Front");
                            location.removeTile(e.Position.X, e.Position.Y, "Buildings");

                            Game1.playSound("secret1");

                            if (++Mod.BossKeysUsed >= 2)
                            {
                                var buildings = location.Map.GetLayer("Buildings");
                                int bx = 9, by = 4;
                                for (int i = 0; i < 4; ++i)
                                {
                                    int ix = i % 2, iy = i / 2;
                                    int x = bx + ix, y = by + iy;
                                    buildings.Tiles[x, y].TileIndex += 2;

                                    string prop = location.doesTileHaveProperty(x, y, "UnlockAction", "Buildings");
                                    if (!string.IsNullOrEmpty(prop))
                                    {
                                        location.setTileProperty(x, y, "Buildings", "Action", prop);
                                    }
                                }
                            }
                        }
                    }
                    break;

                    case "Movable":
                    {
                        int dir = farmer.FacingDirection;
                        int ox = 0, oy = 0;
                        switch (dir)
                        {
                            case 2: oy = 1; break;
                            case 0: oy = -1; break;
                            case 3: ox = -1; break;
                            case 1: ox = 1; break;
                        }

                        int[] validPuzzleTiles = new[]
                        {
                                240, 241, 242, 243,
                                256, 257, 258, 259, 260,
                                272, 273, 274, 275, 276
                            };
                        int target = 243;

                        int tx = e.Position.X, ty = e.Position.Y;
                        while (true)
                        {
                            tx += ox;
                            ty += oy;
                            if (!validPuzzleTiles.Contains(location.getTileIndexAt(tx, ty, "Back")) || location.doesTileHaveProperty(tx, ty, "Action", "Buildings") == "Movable")
                            {
                                tx -= ox;
                                ty -= oy;
                                break;
                            }
                        }

                        int curIndex = location.getTileIndexAt(e.Position.X, e.Position.Y, "Buildings");
                        location.removeTile(e.Position.X, e.Position.Y, "Buildings");
                        var buildings = location.Map.GetLayer("Buildings");
                        buildings.Tiles[tx, ty] = new StaticTile(buildings, location.Map.TileSheets[0], BlendMode.Additive, curIndex);
                        location.setTileProperty(tx, ty, "Buildings", "Action", "Movable");
                        Game1.playSound("throw");

                        if (location.getTileIndexAt(tx, ty, "Back") == target)
                        {
                            var back = location.Map.GetLayer("Back");
                            back.Tiles[tx, ty] = new StaticTile(back, location.Map.TileSheets[0], BlendMode.Additive, 257);
                            var pos = new Vector2(14, 13);
                            var chest = new Chest(0, new List<Item>(new Item[] { new SObject(Mod.Ja.GetObjectId("Festive Big Key (B)"), 1) }), pos);
                            location.overlayObjects[pos] = chest;
                            Game1.playSound("secret1");

                            for (int ix = 0; ix < back.LayerWidth; ++ix)
                            {
                                for (int iy = 0; iy < back.LayerHeight; ++iy)
                                {
                                    if (location.doesTileHaveProperty(ix, iy, "Action", "Buildings") == "Movable")
                                        location.removeTile(ix, iy, "Buildings");
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }

        /// <inheritdoc cref="IInputEvents.ButtonPressed"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Context.IsPlayerFree)
            {
                if (Game1.player.CurrentTool is MeleeWeapon weapon && weapon.InitialParentTileIndex == Mod.Ja.GetWeaponId("Festive Scepter"))
                {
                    if (MeleeWeapon.defenseCooldown > 0)
                        return;

                    _ = new Beam(Game1.player, e.Cursor.AbsolutePixels);
                }
            }
        }

        /// <inheritdoc cref="IDisplayEvents.RenderedHud"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            var b = e.SpriteBatch;

            if (Game1.currentLocation.characters.SingleOrDefault(npc => npc is Witch) is Witch witch)
            {
                int posX = (Game1.viewport.Width - this.BossBarBg.Width * 4) / 2;
                b.Draw(this.BossBarBg, new Vector2(posX, 5), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);

                float percent = (float)witch.Health / Witch.WitchHealth;
                Rectangle sourceRect = new Rectangle(0, 0, (int)(this.BossBarFg.Width * percent), this.BossBarFg.Height);
                if (sourceRect.Width > 0)
                {
                    b.Draw(this.BossBarFg, new Vector2(posX, 5), sourceRect, Color.Green, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
                }
            }
        }

        /// <inheritdoc cref="SpaceEvents.BombExploded"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BombExploded(object sender, EventArgsBombExploded e)
        {
            if (sender is not Farmer who || !who.currentLocation.Name.StartsWith("FrostDungeon."))
                return;

            int radius = e.Radius + 2;
            bool[,] circleOutlineGrid2 = Game1.getCircleOutlineGrid(radius);

            bool flag = false;
            Vector2 index1 = new Vector2((int)(e.Position.X - (double)radius), (int)(e.Position.Y - (double)radius));
            for (int index2 = 0; index2 < radius * 2 + 1; ++index2)
            {
                for (int index3 = 0; index3 < radius * 2 + 1; ++index3)
                {
                    if (index2 == 0 || index3 == 0 || (index2 == radius * 2 || index3 == radius * 2))
                        flag = circleOutlineGrid2[index2, index3];
                    else if (circleOutlineGrid2[index2, index3])
                    {
                        flag = !flag;
                        if (!flag)
                        {
                            this.DoBombableCheck(who.currentLocation, index1);
                        }
                    }
                    if (flag)
                    {
                        this.DoBombableCheck(who.currentLocation, index1);
                    }
                    ++index1.Y;
                    index1.Y = Math.Min(who.currentLocation.map.Layers[0].LayerHeight - 1, Math.Max(0.0f, index1.Y));
                }
                ++index1.X;
                index1.Y = Math.Min(who.currentLocation.map.Layers[0].LayerWidth - 1, Math.Max(0.0f, index1.X));
                index1.Y = e.Position.Y - radius;
                index1.Y = Math.Min(who.currentLocation.map.Layers[0].LayerHeight - 1, Math.Max(0.0f, index1.Y));
            }
        }

        private void DoBombableCheck(GameLocation location, Vector2 tile)
        {
            string propVal = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Bombable", "Buildings");
            if (string.IsNullOrEmpty(propVal))
                return;

            string[] bombActions = propVal.Split(' ');
            foreach (string actStr in bombActions)
            {
                int eqIndex = actStr.IndexOf('=');
                string action = actStr.Substring(0, eqIndex);
                string arguments = actStr.Substring(eqIndex + 1);

                switch (action)
                {
                    case "Buildings":
                    {
                        int index = int.Parse(arguments);
                        var buildings = location.Map.GetLayer("Buildings");
                        var existingTile = buildings.Tiles[(int)tile.X, (int)tile.Y];
                        buildings.Tiles[(int)tile.X, (int)tile.Y] = (index == -1) ? null : new StaticTile(buildings, existingTile.TileSheet, BlendMode.Additive, index);
                    }
                    break;

                    case "Warp":
                    {
                        string[] tokens = arguments.Split(',');
                        var warp = new Warp((int)tile.X, (int)tile.Y, tokens[2], int.Parse(tokens[0]), int.Parse(tokens[1]), false);
                        location.warps.Add(warp);
                    }
                    break;
                }
            }

            location.removeTileProperty((int)tile.X, (int)tile.Y, "Buildings", "Bombable");
            this.DoBombableCheck(location, new Vector2(tile.X + 1, tile.Y));
            this.DoBombableCheck(location, new Vector2(tile.X - 1, tile.Y));
            this.DoBombableCheck(location, new Vector2(tile.X, tile.Y + 1));
            this.DoBombableCheck(location, new Vector2(tile.X, tile.Y - 1));
        }
    }
}
