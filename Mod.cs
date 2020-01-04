using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheftOfTheWinterSTar.Overrides;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Events;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Tiles;
using StardewValley.Menus;

namespace TheftOfTheWinterSTar
{
    public enum ArenaStage
    {
        NotTriggered,
        Stage1,
        Finished1,
        Stage2,
        Finished2,
    }

    public class Mod : StardewModdingAPI.Mod, IAssetEditor
    {
        public const int EVENT_ID = 91000;

        public static Mod instance;
        internal static JsonAssetsAPI ja;
        
        private SaveData saveData;

        Texture2D bossBarBg, bossBarFg;

        private static string[] locs = new string[]
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
            "Boss",
        };

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;

            bossBarBg = Helper.Content.Load<Texture2D>("assets/bossbar-bg.png");
            bossBarFg = Helper.Content.Load<Texture2D>("assets/bossbar-fg.png");

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            helper.Events.GameLoop.SaveCreated += onCreated;
            helper.Events.GameLoop.SaveLoaded += onLoaded;
            helper.Events.GameLoop.Saved += onSaved;
            Helper.Events.GameLoop.UpdateTicked += onUpdated;
            helper.Events.GameLoop.DayStarted += onDayStarted;
            helper.Events.GameLoop.DayEnding += onDayEnding;
            Helper.Events.Player.Warped += onWarped;
            Helper.Events.Player.InventoryChanged += onInventoryChanged;
            Helper.Events.Input.ButtonPressed += onButtonPressed;
            Helper.Events.Display.RenderedHud += onRenderedHud;

            SpaceEvents.OnBlankSave += onBlankSave;
            SpaceEvents.ActionActivated += onActionActivated;
            SpaceEvents.BombExploded += bombExploded;

            try
            {
                HarmonyInstance harmony = HarmonyInstance.Create(ModManifest.UniqueID);
                harmony.Patch(AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.canPlantThisSeedHere)), prefix: new HarmonyMethod(AccessTools.Method(typeof(SeasonalDelimiterAllowPlantingHook1), nameof(SeasonalDelimiterAllowPlantingHook1.Prefix))));
                harmony.Patch(AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.plant)), prefix: new HarmonyMethod(AccessTools.Method(typeof(SeasonalDelimiterAllowPlantingHook2), nameof(SeasonalDelimiterAllowPlantingHook2.Prefix))));
            }
            catch (Exception e)
            {
                Log.error("Exception doing harmony patch: " + e);
            }
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\CraftingRecipes") || asset.AssetNameEquals("Strings\\StringsFromMaps");
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data\\CraftingRecipes"))
            {
                if (ja == null)
                    return;

                var dict = asset.AsDictionary<string, string>().Data;
                dict.Add("Frosty Stardrop", ja.GetObjectId("Frosty Stardrop Piece") + " 5/Field/434/false/null");
            }
            else if (asset.AssetNameEquals("Strings\\StringsFromMaps"))
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
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ja = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "ja"));
            ja.IdsFixed += onIdsFixed;
        }

        private void onCreated(object sender, SaveCreatedEventArgs e)
        {
            saveData = new SaveData();
        }

        private void onLoaded(object sender, SaveLoadedEventArgs e)
        {
            saveData = Helper.Data.ReadSaveData<SaveData>("FrostDungeon.SaveData") ?? new SaveData();
        }

        private void onSaved(object sender, SavedEventArgs e)
        {
            if ( Game1.IsMasterGame )
                Helper.Data.WriteSaveData("FrostDungeon.SaveData", saveData);
        }

        public bool startedBoss = false;
        public List<Projectile> prevProjectiles = null;
        private void onUpdated(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if ( Game1.currentLocation.Name == "FrostDungeon.Arena")
            {
                if ((saveData.ArenaStage == ArenaStage.Stage1 || saveData.ArenaStage == ArenaStage.Stage2) && 
                    Game1.currentLocation.characters.Count(npc => npc is Monster) <= 0 )
                {
                    Game1.playSound("questcomplete");
                    if (saveData.ArenaStage == ArenaStage.Stage1)
                    {
                        saveData.ArenaStage = ArenaStage.Finished1;
                        int key = ja.GetObjectId("Festive Key");
                        var pos = new Vector2(6, 13);
                        var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(key, 1) }), pos);
                        Game1.currentLocation.overlayObjects[pos] = chest;
                        Game1.playSound("questcomplete");
                    }
                    else if (saveData.ArenaStage == ArenaStage.Stage2)
                    {
                        saveData.ArenaStage = ArenaStage.Finished2;
                        int stardropPiece = ja.GetObjectId("Frosty Stardrop Piece");
                        var pos = new Vector2(13, 13);
                        var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(stardropPiece, 1) }), pos);
                        Game1.currentLocation.overlayObjects[pos] = chest;
                        Game1.playSound("questcomplete");
                    }
                }
            }
            else if ( Game1.currentLocation.Name == "FrostDungeon.Bonus4")
            {
                if ( !saveData.DidProjectilePuzzle )
                {
                    var projectiles = Game1.currentLocation.projectiles.ToList();
                    if ( prevProjectiles !=  null )
                    {
                        foreach (var projectile in projectiles)
                        {
                            if (prevProjectiles.Contains(projectile))
                                prevProjectiles.Remove(projectile);
                        }

                        foreach (var projectile in prevProjectiles)
                        {
                            if (projectile.getBoundingBox().Intersects(new Rectangle((int)(8.5 * Game1.tileSize), (int)(8.5 * Game1.tileSize), Game1.tileSize * 2, Game1.tileSize * 2)))
                            {
                                int stardropPiece = ja.GetObjectId("Frosty Stardrop Piece");
                                var pos = new Vector2(9, 13);
                                var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(stardropPiece, 1) }), pos);
                                Game1.currentLocation.overlayObjects[pos] = chest;
                                saveData.DidProjectilePuzzle = true;
                                break;
                            }
                        }
                    }
                    prevProjectiles = projectiles;
                }
            }
            else if ( Game1.currentLocation.Name == "FrostDungeon.Boss" )
            {
                if ( startedBoss & !saveData.BeatBoss )
                {
                    if (Game1.currentLocation.characters.Count(npc => npc is Monster) <= 0)
                    {
                        startedBoss = false;
                        saveData.BeatBoss = true;
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

        
        private void onDayStarted(object sender, DayStartedEventArgs e)
        {
            int seasonalDelimiter = ja.GetBigCraftableId("Tempus Globe");
            foreach (var loc in Game1.locations)
            {
                if (loc.IsFarm)
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
                                    if (!loc.terrainFeatures.ContainsKey(key))
                                        continue;
                                    var tf = loc.terrainFeatures[key];
                                    if ( tf is HoeDirt hd )
                                    {
                                        hd.state.Value = HoeDirt.watered;
                                        hd.updateNeighbors(loc, key);
                                    }
                                }
                            }
                            loc.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2176, 320, 320), 60f, 4, 100, pair.Key * 64f + new Vector2((float)sbyte.MinValue, (float)sbyte.MinValue), false, false)
                            {
                                color = Color.White * 0.4f,
                                delayBeforeAnimationStart = Game1.random.Next(1000),
                                id = pair.Key.X * 4000f + pair.Key.Y
                            });
                        }
                    }
                }
            }
        }

        private void onDayEnding(object sender, DayEndingEventArgs e)
        {
            if (saveData != null)
            {
                if (saveData.ArenaStage == ArenaStage.Stage1)
                    saveData.ArenaStage = ArenaStage.NotTriggered;
                else if (saveData.ArenaStage == ArenaStage.Stage2)
                    saveData.ArenaStage = ArenaStage.Finished1;
            }

            var arena = Game1.getLocationFromName("FrostDungeon.Arena");
            arena.characters.Clear();
            var bossArea = Game1.getLocationFromName("FrostDungeon.Boss");
            if ( !saveData.BeatBoss )
            {
                bossArea.characters.Clear();
                bossArea.netObjects.Clear();
                startedBoss = false;
            }

            int seasonalDelimiter = ja.GetBigCraftableId("Tempus Globe");
            foreach (var loc in Game1.locations)
            {
                if ( loc.IsFarm )
                {
                    int w = loc.map.Layers[0].LayerWidth, h = loc.map.Layers[0].LayerHeight;
                    bool[,] valid = new bool[w, h];

                    foreach ( var pair in loc.Objects.Pairs )
                    {
                        var obj = pair.Value;
                        if (obj.bigCraftable.Value && obj.ParentSheetIndex == seasonalDelimiter)
                        {
                            for (int ix = -2; ix <= 2; ++ix)
                                for (int iy = -2; iy <= 2; ++iy)
                                    valid[(int)pair.Key.X + ix, (int)pair.Key.Y + iy] = true;
                        }
                    }

                    foreach ( var pair in loc.terrainFeatures.Pairs )
                    {
                        var tf = pair.Value;
                        if ( tf is HoeDirt hd )
                        {
                            if (hd.crop == null)
                                continue;

                            if (valid[(int)pair.Key.X, (int)pair.Key.Y])
                            {
                                if (!hd.crop.seasonsToGrowIn.Contains(Game1.currentSeason))
                                    hd.crop.seasonsToGrowIn.Add(Game1.currentSeason);
                            }
                            else
                            {
                                var cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
                                if ( !cropData.ContainsKey(hd.crop.netSeedIndex.Value))
                                {
                                    Log.warn("no crop " + hd.crop.netSeedIndex.Value + "? ");
                                    continue;
                                }
                                string[] seasons = cropData[hd.crop.netSeedIndex.Value].Split('/')[1].Split(' ');
                                hd.crop.seasonsToGrowIn.Clear();
                                hd.crop.seasonsToGrowIn.AddRange(seasons);
                            }
                        }
                    }
                }
            }
        }

        private void onIdsFixed(object sender, EventArgs e)
        {
            Log.debug("Adding frost dungeon loot");

            int stardropPiece = ja.GetObjectId("Frosty Stardrop Piece");
            int scepter = ja.GetWeaponId("Festive Scepter");
            int key = ja.GetObjectId("Festive Key");
            int keyHalfB = ja.GetObjectId("Festive Big Key (A)");
            Log.trace("IDs for chests: " + stardropPiece + " " + scepter + " " + key + " " + keyHalfB);

            foreach (var locName in locs)
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
                if (locName == "Bonus1" || locName == "Bonus2" || locName == "Bonus3")
                {
                    var pos = new Vector2(9, 9);
                    if (locName == "Bonus2")
                    {
                        pos.X = 13;
                    }
                    var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(stardropPiece, 1) }), pos);
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
                    var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(key, 1) }), pos);
                    loc.overlayObjects[pos] = chest;
                }
                else if (locName == "Maze")
                {
                    var pos = new Vector2(20, 26);
                    var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(keyHalfB, 1) }), pos);
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

        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            if (e.NewLocation.Name.StartsWith("FrostDungeon."))
            {
                for (int ix = 0; ix < e.NewLocation.Map.Layers[0].LayerWidth; ++ix)
                {
                    for (int iy = 0; iy < e.NewLocation.Map.Layers[0].LayerHeight; ++iy)
                    {
                        var prop = e.NewLocation.doesTileHaveProperty(ix, iy, "UnlockId", "Buildings");

                        if (!string.IsNullOrEmpty(prop) && e.Player.mailReceived.Contains("FrostDungeon.Lock." + prop))
                        {
                            var newAction = e.Player.currentLocation.doesTileHaveProperty(ix, iy, "UnlockAction", "Buildings");
                            e.NewLocation.setTileProperty(ix, iy, "Buildings", "Action", newAction);
                            e.NewLocation.setMapTileIndex(ix, iy - 2, 48, "Buildings");
                        }
                    }
                }
            }

            var decoSpots = new Dictionary<string, Vector2[]>();
            decoSpots.Add("BusStop",   new Vector2[] { new Vector2( 5,  8), new Vector2( 9, 10), new Vector2(10, 14) });
            decoSpots.Add("Backwoods", new Vector2[] { new Vector2(40, 30), new Vector2(32, 31), new Vector2(25, 29) });
            decoSpots.Add("Tunnel",    new Vector2[] { new Vector2(33, 10), new Vector2(23,  9), new Vector2(10,  8) });

            if ( decoSpots.ContainsKey(e.NewLocation.Name) )
            {
                var spots = decoSpots[e.NewLocation.Name];
                if ( Game1.currentSeason == "winter" && Game1.dayOfMonth < 25 && !saveData.BeatBoss)
                {
                    TileSheet ts = null;
                    for (int i = 0; i < e.NewLocation.Map.TileSheets.Count; ++i)
                    {
                        if ( e.NewLocation.map.TileSheets[i].ImageSource.Contains("trail-decorations") )
                        {
                            ts = e.NewLocation.map.TileSheets[i];
                            break;
                        }
                    }
                    if (ts == null)
                    {
                        // AddTileSheet sorts the tilesheets by ID after adding them.
                        // The game sometimes refers to tilesheets by their index (such as in Beach.fixBridge)
                        // Prepending this to the ID should ensure that this tilesheet is added to the end,
                        // which preserves the normal indices of the tilesheets.
                        char comeLast = '\u03a9'; // Omega

                        ts = new TileSheet(e.NewLocation.Map, Helper.Content.GetActualAssetKey("assets/trail-decorations.png"), new xTile.Dimensions.Size(2, 2), new xTile.Dimensions.Size(16, 16));
                        ts.Id = comeLast + ts.Id;
                        e.NewLocation.map.AddTileSheet(ts);
                        e.NewLocation.map.LoadTileSheets(Game1.mapDisplayDevice);

                        Random r = new Random((int)Game1.uniqueIDForThisGame + e.NewLocation.Name.GetHashCode());
                        var layer = e.NewLocation.map.GetLayer("Buildings");
                        foreach (var spot in spots)
                        {
                            int tile = r.Next(4);
                            layer.Tiles[(int)spot.X, (int)spot.Y] = new StaticTile(layer, ts, BlendMode.Alpha, tile);
                            e.NewLocation.setTileProperty((int)spot.X, (int)spot.Y, "Buildings", "Action", $"Message \"FrostDungeon.Trail{tile}\"");
                        }
                    }
                }
                else
                {
                    var layer = e.NewLocation.Map.GetLayer("Buildings");
                    foreach ( var spot in spots )
                    {
                        layer.Tiles[(int)spot.X, (int)spot.Y] = null;
                    }
                }
            }

            if (e.NewLocation.Name == "Farm" && !Game1.player.eventsSeen.Contains(EVENT_ID) && Game1.currentSeason == "winter" && Game1.dayOfMonth < 25)
            {
                string eventStr = "continue/64 15/farmer 64 16 2 Lewis 64 18 0/pause 1500/speak Lewis \"Hello, @.#$b#I was making preparations for the Feast of the Winter Star and... I can't find any of the decorations!$s#$b#It seems someone stole the decorations.$4#$b#I'm not sure why somebody would do this... but decorations don't just disappear by themselves!$s#$b#Anyways, I was hoping you could retrieve them for us?$h#$b#There was a trail of broken decorations leading down the tunnel to the left of the bus stop. We'd all appreciate it if you could do this for us.$n#$b#Or we could hire Marlon but that's going to be costly.$s#$b#Good luck!$n\"/pause 500/end";
                e.NewLocation.currentEvent = new Event(eventStr, EVENT_ID);
                Game1.eventUp = true;
                Game1.displayHUD = false;
                Game1.player.CanMove = false;
                Game1.player.showNotCarrying();
                
                Game1.player.eventsSeen.Add(EVENT_ID);
            }
            else if ( e.NewLocation.Name == "Tunnel" )
            {
                var doorwayTs = e.NewLocation.Map.TileSheets.Single(ts => ts.ImageSource.Contains("magic-doorway"));
                if ( Game1.currentSeason == "winter" && Game1.dayOfMonth < 25 && doorwayTs.ImageSource.Contains("magic-doorway-locked.png") )
                {
                    doorwayTs.ImageSource = doorwayTs.ImageSource.Replace("magic-doorway-locked.png", "magic-doorway.png");
                    e.NewLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
                    e.NewLocation.setTileProperty(8, 6, "Buildings", "Action", "Warp 9 12 FrostDungeon.Entrance");
                }
                else if ( ( Game1.currentSeason != "winter" || Game1.dayOfMonth >= 25 ) && doorwayTs.ImageSource.Contains("magic-doorway.png") )
                {
                    doorwayTs.ImageSource = doorwayTs.ImageSource.Replace("magic-doorway.png", "magic-doorway-locked.png");
                    e.NewLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
                    e.NewLocation.setTileProperty(8, 6, "Buildings", "Action", "Message \"FrostDungeon.LockedEntrance\"");
                }
            }
            else if ( e.NewLocation.Name == "FrostDungeon.Boss" )
            {
                if ( !startedBoss && !saveData.BeatBoss )
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

                    startedBoss = true;
                }
            }
        }

        private void onInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if ( !e.Player.knowsRecipe("Frosty Stardrop") )
            {
                foreach (var item in e.Added)
                    if (item is StardewValley.Object obj && obj.ParentSheetIndex == ja.GetObjectId("Frosty Stardrop Piece"))
                        e.Player.craftingRecipes.Add("Frosty Stardrop", 0);
            }
        }

        private static int bossKeysUsed = 0;
        private void onBlankSave(object sender, EventArgs e)
        {
            Log.debug("Adding frost dungeon");

            foreach (var locName in locs)
            {
                var loc = new GameLocation(Helper.Content.GetActualAssetKey("assets/" + locName + ".tbin"), "FrostDungeon." + locName);
                Game1.locations.Add(loc);
            }

            // AddTileSheet sorts the tilesheets by ID after adding them.
            // The game sometimes refers to tilesheets by their index (such as in Beach.fixBridge)
            // Prepending this to the ID should ensure that this tilesheet is added to the end,
            // which preserves the normal indices of the tilesheets.
            char comeLast = '\u03a9'; // Omega

            var tunnel = Game1.getLocationFromName("Tunnel");
            var animDoorTilesheet = SpaceCore.Content.loadTsx(Helper, "assets/magic-doorway.tsx", "magic-doorway", tunnel.Map, out var animMapping);
            animDoorTilesheet.Id = comeLast + animDoorTilesheet.Id;
            tunnel.map.AddTileSheet(animDoorTilesheet);

            var buildings = tunnel.map.GetLayer("Buildings");
            buildings.Tiles[7, 4] = animMapping[0].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[8, 4] = animMapping[1].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[9, 4] = animMapping[2].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[7, 5] = animMapping[16].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[8, 5] = animMapping[17].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[9, 5] = animMapping[18].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[7, 6] = animMapping[32].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[8, 6] = animMapping[33].makeTile(animDoorTilesheet, buildings);
            buildings.Tiles[9, 6] = animMapping[34].makeTile(animDoorTilesheet, buildings);
            tunnel.setTileProperty(8, 6, "Buildings", "Action", "Warp 9 12 FrostDungeon.Entrance");
        }

        private void onActionActivated(object sender, EventArgsAction e)
        {
            var farmer = sender as Farmer;
            if (e.ActionString == "Message \"FrostDungeon.Locked\"")
            {
                int key = ja.GetObjectId("Festive Key");
                if (farmer.ActiveObject?.ParentSheetIndex == key)
                {
                    farmer.removeFirstOfThisItemFromInventory(key);
                    farmer.mailReceived.Add("FrostDungeon.Locked." + farmer.currentLocation.doesTileHaveProperty((int)e.Position.X, (int)e.Position.Y, "Buildings", "UnlockId"));

                    var newAction = farmer.currentLocation.doesTileHaveProperty((int)e.Position.X, (int)e.Position.Y, "UnlockAction", "Buildings");
                    farmer.currentLocation.setTileProperty((int)e.Position.X, (int)e.Position.Y, "Buildings", "Action", newAction);
                    farmer.currentLocation.setMapTileIndex((int)e.Position.X, (int)e.Position.Y - 2, 48, "Buildings");

                    Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:FrostDungeon.Unlock"));
                    Game1.playSound("crystal");

                    e.Cancel = true;
                }
            }
            else if (e.Action == "ActivateArena")
            {
                if ( farmer.currentLocation.Name == "FrostDungeon.Arena" )
                {
                    Log.trace("Activate arena: Stage " + saveData.ArenaStage);
                    Game1.playSound("batScreech");
                    Game1.playSound("rockGolemSpawn");
                    if (saveData.ArenaStage == ArenaStage.NotTriggered )
                    {
                        saveData.ArenaStage = ArenaStage.Stage1;
                        for ( int i = 0; i < 9; ++i )
                        {
                            int cx = (int)e.Position.X, cy = (int)e.Position.Y;
                            int dx = (int)(Math.Cos(Math.PI * 2 / 9 * i) * 5);
                            int dy = (int)(Math.Sin(Math.PI * 2 / 9 * i) * 5);
                            int x = cx + dx, y = cy + dy;
                            x *= Game1.tileSize;
                            y *= Game1.tileSize;

                            if ( i % 3 == 0 )
                                farmer.currentLocation.addCharacter(new Ghost(new Vector2(x, y)));
                            else if ( i % 3 == 1)
                                farmer.currentLocation.addCharacter(new Skeleton(new Vector2(x, y)));
                            else if (i % 3 == 2)
                                farmer.currentLocation.addCharacter(new DustSpirit(new Vector2(x, y)));
                        }
                    }
                    else if (saveData.ArenaStage == ArenaStage.Finished1 )
                    {
                        saveData.ArenaStage = ArenaStage.Stage2;
                        for (int i = 0; i < 3; ++i)
                        {
                            int cx = (int)e.Position.X, cy = (int)e.Position.Y;
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
            }
            else if (e.Action == "ItemPuzzle")
            {
                string[] toks = e.ActionString.Split(' ');
                int item = int.Parse(toks[1]);
                if (farmer.ActiveObject?.ParentSheetIndex == item)
                {
                    farmer.removeFirstOfThisItemFromInventory(item);
                    farmer.currentLocation.removeTileProperty((int)e.Position.X, (int)e.Position.Y, "Buildings", "Action");

                    int warpIndex = farmer.currentLocation.Map.GetLayer("Buildings").Tiles[(int)e.Position.X, (int)e.Position.Y].TileIndex - 32;
                    var back = farmer.currentLocation.Map.GetLayer("Back");
                    back.Tiles[(int)e.Position.X, (int)e.Position.Y + 3] = new StaticTile(back, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, warpIndex);

                    var warp = new Warp((int)e.Position.X, (int)e.Position.Y + 3, toks[2], 7, 9, false);
                    farmer.currentLocation.warps.Add(warp);

                    Game1.playSound("secret1");
                }
                else
                {
                    Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:FrostDungeon.ItemPuzzle"));
                }
            }
            else if (e.Action == "BossKeyHalf")
            {
                string[] toks = e.ActionString.Split(' ');

                int key = ja.GetObjectId("Festive Big Key (" + toks[1] + ")");
                if (farmer.ActiveObject?.ParentSheetIndex == key)
                {
                    farmer.removeFirstOfThisItemFromInventory(key);

                    farmer.currentLocation.removeTile((int)e.Position.X, (int)e.Position.Y - 1, "Front");
                    farmer.currentLocation.removeTile((int)e.Position.X, (int)e.Position.Y, "Buildings");

                    Game1.playSound("secret1");

                    if (++bossKeysUsed >= 2)
                    {
                        var buildings = farmer.currentLocation.Map.GetLayer("Buildings");
                        int bx = 9, by = 4;
                        for (int i = 0; i < 4; ++i)
                        {
                            int ix = i % 2, iy = i / 2;
                            int x = bx + ix, y = by + iy;
                            buildings.Tiles[x, y].TileIndex += 2;

                            string prop = farmer.currentLocation.doesTileHaveProperty(x, y, "UnlockAction", "Buildings");
                            if (prop != null && prop != "")
                            {
                                farmer.currentLocation.setTileProperty(x, y, "Buildings", "Action", prop);
                            }
                        }
                    }
                }
            }
            else if (e.Action == "Movable")
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

                int[] validPuzzleTiles = new int[]
                {
                    240, 241, 242, 243,
                    256, 257, 258, 259, 260,
                    272, 273, 274, 275, 276,
                };
                int target = 243;

                int tx = (int)e.Position.X, ty = (int)e.Position.Y;
                while (true)
                {
                    tx += ox;
                    ty += oy;
                    if (!validPuzzleTiles.Contains(farmer.currentLocation.getTileIndexAt(tx, ty, "Back")) ||
                         farmer.currentLocation.doesTileHaveProperty(tx, ty, "Action", "Buildings") == "Movable")
                    {
                        tx -= ox;
                        ty -= oy;
                        break;
                    }
                }

                int currIndex = farmer.currentLocation.getTileIndexAt((int)e.Position.X, (int)e.Position.Y, "Buildings");
                farmer.currentLocation.removeTile((int)e.Position.X, (int)e.Position.Y, "Buildings");
                var buildings = farmer.currentLocation.Map.GetLayer("Buildings");
                buildings.Tiles[tx, ty] = new StaticTile(buildings, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, currIndex);
                farmer.currentLocation.setTileProperty(tx, ty, "Buildings", "Action", "Movable");
                Game1.playSound("throw");

                if (farmer.currentLocation.getTileIndexAt(tx, ty, "Back") == target)
                {
                    var back = farmer.currentLocation.Map.GetLayer("Back");
                    back.Tiles[tx, ty] = new StaticTile(back, farmer.currentLocation.Map.TileSheets[0], BlendMode.Additive, 257);
                    var pos = new Vector2(14, 13);
                    var chest = new Chest(0, new List<Item>(new Item[] { new StardewValley.Object(ja.GetObjectId("Festive Big Key (B)"), 1) }), pos);
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
        }

        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Context.IsPlayerFree)
            {
                if (Game1.player.CurrentTool is MeleeWeapon weapon && weapon.InitialParentTileIndex == ja.GetWeaponId("Festive Scepter"))
                {
                    if (MeleeWeapon.defenseCooldown > 0)
                        return;

                    new Beam(Game1.player, e.Cursor.AbsolutePixels);
                }
            }
        }

        private void onRenderedHud(object sender, RenderedHudEventArgs e)
        {
            var b = e.SpriteBatch;

            var witch = Game1.currentLocation.characters.SingleOrDefault(npc => npc is Witch) as Witch;
            if ( witch != null )
            {
                int posX = (Game1.viewport.Width - bossBarBg.Width * 4) / 2;
                b.Draw(bossBarBg, new Vector2(posX, 5), null, Color.White, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);

                float perc = (float)witch.Health / Witch.WITCH_HEALTH;
                Rectangle r = new Rectangle(0, 0, (int)(bossBarFg.Width * perc), bossBarFg.Height);
                if (r.Width > 0)
                {
                    b.Draw(bossBarFg, new Vector2(posX, 5), r, Color.Green, 0, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1);
                }
            }
        }

        private void bombExploded(object sender, EventArgsBombExploded e)
        {
            var who = sender as Farmer;
            if (!who.currentLocation.Name.StartsWith("FrostDungeon."))
                return;

            int radius = e.Radius + 2;
            bool[,] circleOutlineGrid2 = Game1.getCircleOutlineGrid(radius);

            bool flag = false;
            Vector2 index1 = new Vector2((float)(int)((double)e.Position.X - (double)radius), (float)(int)((double)e.Position.Y - (double)radius));
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
                            doBombableCheck(who.currentLocation, index1);
                        }
                    }
                    if (flag )
                    {
                        doBombableCheck(who.currentLocation, index1);
                    }
                    ++index1.Y;
                    index1.Y = Math.Min((float)(who.currentLocation.map.Layers[0].LayerHeight - 1), Math.Max(0.0f, index1.Y));
                }
                ++index1.X;
                index1.Y = Math.Min((float)(who.currentLocation.map.Layers[0].LayerWidth - 1), Math.Max(0.0f, index1.X));
                index1.Y = e.Position.Y - (float)radius;
                index1.Y = Math.Min((float)(who.currentLocation.map.Layers[0].LayerHeight - 1), Math.Max(0.0f, index1.Y));
            }
        }

        private void doBombableCheck(GameLocation loc, Vector2 pos)
        {
            string propVal = loc.doesTileHaveProperty((int)pos.X, (int)pos.Y, "Bombable", "Buildings");
            if (propVal == null || propVal == "")
                return;

            string[] bombActions = propVal.Split(' ');
            foreach ( var actStr in bombActions)
            {
                int eq = actStr.IndexOf('=');
                string act = actStr.Substring(0, eq);
                string rest = actStr.Substring(eq + 1);

                if ( act == "Buildings" )
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
            doBombableCheck(loc, new Vector2(pos.X + 1, pos.Y));
            doBombableCheck(loc, new Vector2(pos.X - 1, pos.Y));
            doBombableCheck(loc, new Vector2(pos.X, pos.Y + 1));
            doBombableCheck(loc, new Vector2(pos.X, pos.Y - 1));
        }
    }
}
