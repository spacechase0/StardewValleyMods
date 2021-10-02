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
using StardewModdingAPI.Utilities;
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
    internal class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public const int EventId = 91000;

        public static Mod Instance;
        internal static IJsonAssetsApi Ja;

        private SaveData SaveData = new();

        private Texture2D BossBarBg;
        private Texture2D BossBarFg;

        /// <summary>The unique key in <see cref="TerrainFeature.modData"/> which contains the original crop data for the Tempus Globe logic.</summary>
        private string PrevCropDataKey;

        /// <summary>The names of the custom locations to load.</summary>
        private readonly string[] LocationNames = new[]
        {
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

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;

            this.BossBarBg = this.Helper.Content.Load<Texture2D>("assets/bossbar-bg.png");
            this.BossBarFg = this.Helper.Content.Load<Texture2D>("assets/bossbar-fg.png");
            this.PrevCropDataKey = $"{this.ModManifest.UniqueID}/prev-data";

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveCreated += this.OnCreated;
            helper.Events.GameLoop.SaveLoaded += this.OnLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdated;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.RenderedHud += this.OnRenderedHud;

            SpaceEvents.OnBlankSave += this.OnBlankSave;
            SpaceEvents.ActionActivated += this.OnActionActivated;
            SpaceEvents.BombExploded += this.BombExploded;

            HarmonyPatcher.Apply(this,
                new HoeDirtPatcher()
            );
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return
                asset.AssetNameEquals("Data/CraftingRecipes")
                || asset.AssetNameEquals("Maps/Tunnel")
                || asset.AssetNameEquals("Strings/StringsFromMaps")
                || this.TryGetDecoSpots(asset, out _);
        }

        public void Edit<T>(IAssetData asset)
        {
            // scatter decorations
            if (this.TryGetDecoSpots(asset, out Vector2[] decoSpots))
                this.ScatterDecorationsIfNeeded(asset.Data as Map, decoSpots);

            // add Frosty Stardrop recipe
            if (asset.AssetNameEquals("Data/CraftingRecipes"))
            {
                if (Mod.Ja != null)
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict.Add("Frosty Stardrop", Mod.Ja.GetObjectId("Frosty Stardrop Piece") + " 5/Field/434/false/null");
                }
            }

            // add map strings
            else if (asset.AssetNameEquals("Strings/StringsFromMaps"))
            {
                var dict = asset.AsDictionary<string, string>().Data;
                dict.Add("FrostDungeon.LockedEntrance", "This door is locked right now.");
                dict.Add("FrostDungeon.Locked", "This door is locked. It probably needs a key.");
                dict.Add("FrostDungeon.LockedBoss", "This giant door is locked. Perhaps something nearby can open it.");
                dict.Add("FrostDungeon.Unlock", "The door has been unlocked.");
                dict.Add("FrostDungeon.ItemPuzzle", "There seems to be a silhouette on the pedestal.");
                dict.Add("FrostDungeon.Target", "A target.");
                dict.Add("FrostDungeon.Trail0", "Some festive lights.");
                dict.Add("FrostDungeon.Trail1", "A smashed candy cane.");
                dict.Add("FrostDungeon.Trail2", "Some festive ornaments.");
                dict.Add("FrostDungeon.Trail3", "A smashed miniature tree.");
            }

            // edit tunnel map
            else if (asset.AssetNameEquals("Maps/Tunnel"))
            {
                var overlay = Game1.currentSeason == "winter" && Game1.dayOfMonth < 25
                    ? this.Helper.Content.Load<Map>("assets/OverlayPortal.tmx")
                    : this.Helper.Content.Load<Map>("assets/OverlayPortalLocked.tmx");

                asset
                    .AsMap()
                    .PatchMap(overlay, targetArea: new Rectangle(7, 4, 3, 3));
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Mod.Ja = this.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            Mod.Ja.LoadAssets(Path.Combine(this.Helper.DirectoryPath, "assets", "ja"));
            Mod.Ja.IdsFixed += this.OnIdsFixed;
        }

        private void OnCreated(object sender, SaveCreatedEventArgs e)
        {
            this.SaveData = new SaveData();
        }

        private void OnLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.SaveData = this.Helper.Data.ReadSaveData<SaveData>("FrostDungeon.SaveData") ?? new SaveData();
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (Game1.IsMasterGame)
                this.Helper.Data.WriteSaveData("FrostDungeon.SaveData", this.SaveData);
        }

        public bool StartedBoss;
        public List<Projectile> PrevProjectiles;
        private void OnUpdated(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation.Name == "FrostDungeon.Arena")
            {
                if ((this.SaveData.ArenaStage is ArenaStage.Stage1 or ArenaStage.Stage2) &&
                    Game1.currentLocation.characters.Count(npc => npc is Monster) <= 0)
                {
                    Game1.playSound("questcomplete");
                    if (this.SaveData.ArenaStage == ArenaStage.Stage1)
                    {
                        this.SaveData.ArenaStage = ArenaStage.Finished1;
                        int key = Mod.Ja.GetObjectId("Festive Key");
                        var pos = new Vector2(6, 13);
                        var chest = new Chest(0, new List<Item>(new Item[] { new SObject(key, 1) }), pos);
                        Game1.currentLocation.overlayObjects[pos] = chest;
                        Game1.playSound("questcomplete");
                    }
                    else if (this.SaveData.ArenaStage == ArenaStage.Stage2)
                    {
                        this.SaveData.ArenaStage = ArenaStage.Finished2;
                        int stardropPiece = Mod.Ja.GetObjectId("Frosty Stardrop Piece");
                        var pos = new Vector2(13, 13);
                        var chest = new Chest(0, new List<Item>(new Item[] { new SObject(stardropPiece, 1) }), pos);
                        Game1.currentLocation.overlayObjects[pos] = chest;
                        Game1.playSound("questcomplete");
                    }
                }
            }
            else if (Game1.currentLocation.Name == "FrostDungeon.Bonus4")
            {
                if (!this.SaveData.DidProjectilePuzzle)
                {
                    var projectiles = Game1.currentLocation.projectiles.ToList();
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
                                Game1.currentLocation.overlayObjects[pos] = chest;
                                this.SaveData.DidProjectilePuzzle = true;
                                break;
                            }
                        }
                    }
                    this.PrevProjectiles = projectiles;
                }
            }
            else if (Game1.currentLocation.Name == "FrostDungeon.Boss")
            {
                if (this.StartedBoss & !this.SaveData.BeatBoss)
                {
                    if (Game1.currentLocation.characters.Count(npc => npc is Monster) <= 0)
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

                        Game1.drawObjectDialogue("You got the decorations back!\nYou also learned the recipe for the 'Tempus Globe'!");
                    }
                }
            }
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // update maps
            this.Helper.Content.InvalidateCache("Maps/Tunnel");
            foreach (string mapName in this.DecoSpots.Keys)
                this.Helper.Content.InvalidateCache($"Maps/{mapName}");

            // apply Tempus Globe logic
            int seasonalDelimiter = Mod.Ja.GetBigCraftableId("Tempus Globe");
            foreach (var loc in Game1.locations.Where(this.IsFarm))
            {
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
            }
        }

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
            foreach (var loc in Game1.locations.Where(this.IsFarm))
            {
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
            }
        }

        /// <summary>Get whether a location is farmable.</summary>
        /// <param name="location">The location to check.</param>
        private bool IsFarm(GameLocation location)
        {
            return
                location.IsFarm
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
                var loc = Game1.getLocationFromName("FrostDungeon." + locName);
                if (locName == "Entrance")
                {/*
                    Rectangle r = new Rectangle(9, 6, 9, 5);
                    for (int i = 0; i < 12; ++i)
                    {
                        var pos = new Vector2(r.X + Game1.random.Next(r.Width), r.Y + Game1.random.Next(r.Height));
                        var breakable = new BreakableContainer(pos, BreakableContainer.frostBarrel, null);
                        loc.objects[pos] = breakable;
                    }*/
                }
                if (locName is "Bonus1" or "Bonus2" or "Bonus3")
                {
                    var pos = new Vector2(9, 9);
                    if (locName == "Bonus2")
                    {
                        pos.X = 13;
                    }
                    var chest = new Chest(0, new List<Item>(new Item[] { new SObject(stardropPiece, 1) }), pos);
                    loc.overlayObjects[pos] = chest;
                }
                else if (locName == "WeaponRoom")
                {
                    var pos = new Vector2(13, 9);
                    var chest = new Chest(0, new List<Item>(new Item[] { new MeleeWeapon(scepter) }), pos);
                    loc.overlayObjects[pos] = chest;
                }
                else if (locName == "KeyRoom")
                {
                    var pos = new Vector2(13, 9);
                    var chest = new Chest(0, new List<Item>(new Item[] { new SObject(key, 1) }), pos);
                    loc.overlayObjects[pos] = chest;
                }
                else if (locName == "Maze")
                {
                    var pos = new Vector2(20, 26);
                    var chest = new Chest(0, new List<Item>(new Item[] { new SObject(keyHalfB, 1) }), pos);
                    loc.overlayObjects[pos] = chest;
                }
                else if (locName == "Branch2")
                {/*
                    Rectangle r = new Rectangle(8, 6, 4, 3);
                    for ( int i = 0; i < 4; ++i )
                    {
                        var pos = new Vector2(r.X + Game1.random.Next(r.Width), r.Y + Game1.random.Next(r.Height));
                        var breakable = new BreakableContainer(pos, BreakableContainer.frostBarrel, null);
                        loc.objects[pos] = breakable;
                    }*/
                }
            }
        }

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

            if (e.NewLocation.Name == "Farm" && !Game1.player.eventsSeen.Contains(Mod.EventId) && Game1.currentSeason == "winter" && Game1.dayOfMonth < 25)
            {
                string eventStr = "continue/64 15/farmer 64 16 2 Lewis 64 18 0/skippable/pause 1500/speak Lewis \"Hello, @.#$b#I was making preparations for the Feast of the Winter Star and... I can't find any of the decorations!$s#$b#It seems someone stole the decorations.$4#$b#I'm not sure why somebody would do this... but decorations don't just disappear by themselves!$s#$b#Anyways, I was hoping you could retrieve them for us?$h#$b#There was a trail of broken decorations leading down the tunnel to the left of the bus stop. We'd all appreciate it if you could do this for us.$n#$b#Or we could hire Marlon but that's going to be costly.$s#$b#Good luck!$n\"/pause 500/end";
                e.NewLocation.currentEvent = new Event(eventStr, Mod.EventId);
                Game1.eventUp = true;
                Game1.displayHUD = false;
                Game1.player.CanMove = false;
                Game1.player.showNotCarrying();

                Game1.player.eventsSeen.Add(Mod.EventId);
            }
            else if (e.NewLocation.Name == "FrostDungeon.Boss")
            {
                if (!this.StartedBoss && !this.SaveData.BeatBoss)
                {
                    var witch = new Witch();
                    e.NewLocation.characters.Add(witch);

                    var dummySpeaker = new NPC(new AnimatedSprite("Characters\\Penny"), new Vector2(-1, -1), "", 0, "Witch", false, null, witch.Portrait);
                    var dialogue = new Dialogue("How DARE they have fun without me! They'll never get their decorations back!", dummySpeaker);
                    var dialogueBox = new DialogueBox(dialogue);

                    Game1.activeClickableMenu = dialogueBox;
                    Game1.dialogueUp = true;
                    Game1.player.Halt();
                    Game1.player.CanMove = false;
                    Game1.currentSpeaker = dummySpeaker;

                    this.StartedBoss = true;
                }
            }
        }

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
            string mapName = PathUtilities.GetSegments(asset.AssetName).Last();
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

                    tilesheet = new TileSheet(map, this.Helper.Content.GetActualAssetKey("assets/trail-decorations.png"), new xTile.Dimensions.Size(2, 2), new xTile.Dimensions.Size(16, 16));
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

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.Player.knowsRecipe("Frosty Stardrop"))
            {
                foreach (var item in e.Added)
                {
                    if (item is SObject obj && obj.ParentSheetIndex == Mod.Ja.GetObjectId("Frosty Stardrop Piece"))
                        e.Player.craftingRecipes.Add("Frosty Stardrop", 0);
                }
            }
        }

        private static int BossKeysUsed;
        private void OnBlankSave(object sender, EventArgs e)
        {
            Log.Debug("Adding frost dungeon");

            foreach (string locName in this.LocationNames)
            {
                var location = new GameLocation(this.Helper.Content.GetActualAssetKey($"assets/{locName}.tmx"), $"FrostDungeon.{locName}");
                Game1.locations.Add(location);
            }
        }

        private void OnActionActivated(object sender, EventArgsAction e)
        {
            if (sender is not Farmer farmer)
                return;

            if (e.ActionString == "Message \"FrostDungeon.Locked\"")
            {
                int key = Mod.Ja.GetObjectId("Festive Key");
                if (farmer.ActiveObject?.ParentSheetIndex == key)
                {
                    farmer.removeFirstOfThisItemFromInventory(key);
                    farmer.mailReceived.Add("FrostDungeon.Locked." + farmer.currentLocation.doesTileHaveProperty(e.Position.X, e.Position.Y, "Buildings", "UnlockId"));

                    string newAction = farmer.currentLocation.doesTileHaveProperty(e.Position.X, e.Position.Y, "UnlockAction", "Buildings");
                    farmer.currentLocation.setTileProperty(e.Position.X, e.Position.Y, "Buildings", "Action", newAction);
                    farmer.currentLocation.setMapTileIndex(e.Position.X, e.Position.Y - 2, 48, "Buildings");

                    Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:FrostDungeon.Unlock"));
                    Game1.playSound("crystal");

                    e.Cancel = true;
                }
            }
            else
            {
                switch (e.Action)
                {
                    case "ActivateArena" when farmer.currentLocation.Name == "FrostDungeon.Arena":
                        {
                            Log.Trace("Activate arena: Stage " + this.SaveData.ArenaStage);
                            Game1.playSound("batScreech");
                            Game1.playSound("rockGolemSpawn");
                            if (this.SaveData.ArenaStage == ArenaStage.NotTriggered)
                            {
                                this.SaveData.ArenaStage = ArenaStage.Stage1;
                                for (int i = 0; i < 9; ++i)
                                {
                                    int cx = e.Position.X, cy = e.Position.Y;
                                    int dx = (int)(Math.Cos(Math.PI * 2 / 9 * i) * 5);
                                    int dy = (int)(Math.Sin(Math.PI * 2 / 9 * i) * 5);
                                    int x = cx + dx, y = cy + dy;
                                    x *= Game1.tileSize;
                                    y *= Game1.tileSize;

                                    if (i % 3 == 0)
                                        farmer.currentLocation.addCharacter(new Ghost(new Vector2(x, y)));
                                    else if (i % 3 == 1)
                                        farmer.currentLocation.addCharacter(new Skeleton(new Vector2(x, y)));
                                    else if (i % 3 == 2)
                                        farmer.currentLocation.addCharacter(new DustSpirit(new Vector2(x, y)));
                                }
                            }
                            else if (this.SaveData.ArenaStage == ArenaStage.Finished1)
                            {
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
                                        farmer.currentLocation.addCharacter(new Bat(new Vector2(x, y), 77377));
                                }
                                farmer.currentLocation.addCharacter(new DinoMonster(new Vector2(9 * Game1.tileSize, 8 * Game1.tileSize)));
                            }
                        }
                        break;

                    case "ItemPuzzle":
                        {
                            string[] tokens = e.ActionString.Split(' ');
                            int item = int.Parse(tokens[1]);
                            if (farmer.ActiveObject?.ParentSheetIndex == item)
                            {
                                farmer.removeFirstOfThisItemFromInventory(item);
                                farmer.currentLocation.removeTileProperty(e.Position.X, e.Position.Y, "Buildings", "Action");

                                int warpIndex = farmer.currentLocation.Map.GetLayer("Buildings").Tiles[e.Position.X, e.Position.Y].TileIndex - 32;
                                var back = farmer.currentLocation.Map.GetLayer("Back");
                                back.Tiles[e.Position.X, e.Position.Y + 3] = new StaticTile(back, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, warpIndex);

                                var warp = new Warp(e.Position.X, e.Position.Y + 3, tokens[2], 7, 9, false);
                                farmer.currentLocation.warps.Add(warp);

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
                            if (farmer.ActiveObject?.ParentSheetIndex == key)
                            {
                                farmer.removeFirstOfThisItemFromInventory(key);

                                farmer.currentLocation.removeTile(e.Position.X, e.Position.Y - 1, "Front");
                                farmer.currentLocation.removeTile(e.Position.X, e.Position.Y, "Buildings");

                                Game1.playSound("secret1");

                                if (++Mod.BossKeysUsed >= 2)
                                {
                                    var buildings = farmer.currentLocation.Map.GetLayer("Buildings");
                                    int bx = 9, by = 4;
                                    for (int i = 0; i < 4; ++i)
                                    {
                                        int ix = i % 2, iy = i / 2;
                                        int x = bx + ix, y = @by + iy;
                                        buildings.Tiles[x, y].TileIndex += 2;

                                        string prop = farmer.currentLocation.doesTileHaveProperty(x, y, "UnlockAction", "Buildings");
                                        if (!string.IsNullOrEmpty(prop))
                                        {
                                            farmer.currentLocation.setTileProperty(x, y, "Buildings", "Action", prop);
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
                                if (!validPuzzleTiles.Contains(farmer.currentLocation.getTileIndexAt(tx, ty, "Back")) || farmer.currentLocation.doesTileHaveProperty(tx, ty, "Action", "Buildings") == "Movable")
                                {
                                    tx -= ox;
                                    ty -= oy;
                                    break;
                                }
                            }

                            int curIndex = farmer.currentLocation.getTileIndexAt(e.Position.X, e.Position.Y, "Buildings");
                            farmer.currentLocation.removeTile(e.Position.X, e.Position.Y, "Buildings");
                            var buildings = farmer.currentLocation.Map.GetLayer("Buildings");
                            buildings.Tiles[tx, ty] = new StaticTile(buildings, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, curIndex);
                            farmer.currentLocation.setTileProperty(tx, ty, "Buildings", "Action", "Movable");
                            Game1.playSound("throw");

                            if (farmer.currentLocation.getTileIndexAt(tx, ty, "Back") == target)
                            {
                                var back = farmer.currentLocation.Map.GetLayer("Back");
                                back.Tiles[tx, ty] = new StaticTile(back, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, 257);
                                var pos = new Vector2(14, 13);
                                var chest = new Chest(0, new List<Item>(new Item[] { new SObject(Mod.Ja.GetObjectId("Festive Big Key (B)"), 1) }), pos);
                                farmer.currentLocation.overlayObjects[pos] = chest;
                                Game1.playSound("secret1");

                                for (int ix = 0; ix < back.LayerWidth; ++ix)
                                {
                                    for (int iy = 0; iy < back.LayerHeight; ++iy)
                                    {
                                        if (farmer.currentLocation.doesTileHaveProperty(ix, iy, "Action", "Buildings") == "Movable")
                                            farmer.currentLocation.removeTile(ix, iy, "Buildings");
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

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

        private void DoBombableCheck(GameLocation loc, Vector2 pos)
        {
            string propVal = loc.doesTileHaveProperty((int)pos.X, (int)pos.Y, "Bombable", "Buildings");
            if (string.IsNullOrEmpty(propVal))
                return;

            string[] bombActions = propVal.Split(' ');
            foreach (string actStr in bombActions)
            {
                int eq = actStr.IndexOf('=');
                string act = actStr.Substring(0, eq);
                string rest = actStr.Substring(eq + 1);

                if (act == "Buildings")
                {
                    int index = int.Parse(rest);
                    var buildings = loc.Map.GetLayer("Buildings");
                    var existingTile = buildings.Tiles[(int)pos.X, (int)pos.Y];
                    buildings.Tiles[(int)pos.X, (int)pos.Y] = (index == -1) ? null : new StaticTile(buildings, existingTile.TileSheet, BlendMode.Additive, index);
                }
                else if (act == "Warp")
                {
                    string[] toks = rest.Split(',');
                    var warp = new Warp((int)pos.X, (int)pos.Y, toks[2], int.Parse(toks[0]), int.Parse(toks[1]), false);
                    loc.warps.Add(warp);
                }
            }

            loc.removeTileProperty((int)pos.X, (int)pos.Y, "Buildings", "Bombable");
            this.DoBombableCheck(loc, new Vector2(pos.X + 1, pos.Y));
            this.DoBombableCheck(loc, new Vector2(pos.X - 1, pos.Y));
            this.DoBombableCheck(loc, new Vector2(pos.X, pos.Y + 1));
            this.DoBombableCheck(loc, new Vector2(pos.X, pos.Y - 1));
        }
    }
}
