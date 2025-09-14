using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using SpaceCore.VanillaAssetExpansion;
using SpaceShared;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Triggers;
using xTile.Tiles;
using static HarmonyLib.Code;

namespace SpaceCore.Dungeons
{
    public class DungeonState
    {
        public Dictionary<string, GameLocation> ActiveLevels { get; } = new();
    }

    public class GameLocationDungeonExt
    {
        public readonly NetString spaceCoreDungeonId = new();
        public readonly NetInt spaceCoreDungeonLevel = new();
        public readonly NetInt spaceCoreDungeonSeed = new();
        public readonly NetPointDictionary<bool, NetBool> spaceCoreDungeonLadders = new();
        public readonly NetBool spaceCoreDungeonIsMonsterLevel = new();
        public LocalizedContentManager mapContent;
        public bool generated = false;

        public static ConditionalWeakTable<GameLocation, GameLocationDungeonExt> data = new();
    }

    public static class DungeonExtensions
    {
        public static GameLocationDungeonExt GetDungeonExtData(this GameLocation loc)
        {
            return GameLocationDungeonExt.data.GetOrCreateValue(loc);
        }
    }

    [HarmonyPatch(typeof(GameLocation), "initNetFields")]
    public static class GameLocationDungeonNetFieldAdditionsPatch
    {
        public static void Postfix(GameLocation __instance)
        {
            var ext = __instance.GetDungeonExtData();
            __instance.NetFields.AddField(ext.spaceCoreDungeonId)
                .AddField(ext.spaceCoreDungeonLevel)
                .AddField(ext.spaceCoreDungeonSeed)
                .AddField(ext.spaceCoreDungeonLadders)
                .AddField(ext.spaceCoreDungeonIsMonsterLevel);
            ext.spaceCoreDungeonLadders.OnValueAdded += (p, v) => DungeonImpl.OnLadderAdded( __instance, p, v );
        }
    }

    [HarmonyPatch(typeof(FarmerTeam), MethodType.Constructor)]
    public static class FarmerTeamDeepestDungeonLevelsNetFieldInjectionPatch
    {
        public static void Postfix(FarmerTeam __instance)
        {
            var spaceCoreDeepestDungeonLevels = DungeonImpl.deepestLevels.GetOrCreateValue(__instance);
            __instance.NetFields.AddField(spaceCoreDeepestDungeonLevels);
        }
    }

    internal static class DungeonImpl
    {
        private static PerScreen<DungeonState> _state = new(() => new DungeonState());
        internal static DungeonState State => _state.Value;

        internal static ConditionalWeakTable<FarmerTeam, NetStringDictionary<int, NetInt>> deepestLevels = new(); 

        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
            SpaceCore.Instance.Helper.Events.GameLoop.Saving += GameLoop_Saving;
            SpaceCore.Instance.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            SpaceCore.Instance.Helper.Events.Display.RenderedStep += Display_RenderedStep;

            GameLocation.RegisterTileAction("spacechase0.SpaceCore_DungeonEntrance", (loc, args, who, tile) =>
            {
                var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
                if (!dungeonsData.TryGetValue(args[1], out var dungeonData))
                {
                    Log.Error($"Failed to find dungeon data with ID \"{args[1]}\"");
                    return true;
                }

                string prefix = GetLevelNamePrefix(args[1]);
                Game1.warpFarmer($"{prefix}1", 0, 0, false);
                Game1.player.temporarilyInvincible = true;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.player.flashDuringThisTemporaryInvincibility = false;
                Game1.player.currentTemporaryInvincibilityDuration = 1000;

                return true;
            });
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_DungeonElevatorMenu", (loc, args, who, tile) =>
            {
                var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
                if (!dungeonsData.TryGetValue(args[1], out var dungeonData))
                {
                    Log.Error($"Failed to find dungeon data with ID \"{args[1]}\"");
                    return true;
                }

                Game1.activeClickableMenu = new DungeonElevatorMenu(args[1]);

                return true;
            });
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_DungeonLadderExit", (loc, args, who, tile) =>
            {
                var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
                if (!dungeonsData.TryGetValue(loc.GetDungeonExtData().spaceCoreDungeonId.Value, out var dungeonData))
                {
                    Log.Error($"Failed to find dungeon data with ID \"{loc.GetDungeonExtData().spaceCoreDungeonId.Value}\"");
                    return true;
                }

                Game1.warpFarmer(dungeonData.LadderExitLocation, dungeonData.LadderExitTile.X, dungeonData.LadderExitTile.Y, false);

                return true;
            });
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_DungeonLadder", (loc, args, who, tile) =>
            {
                var ext = loc.GetDungeonExtData();
                if (ext.spaceCoreDungeonId.Value == null)
                    return true;

                string prefix = GetLevelNamePrefix(ext.spaceCoreDungeonId.Value);
                Game1.warpFarmer($"{prefix}{ext.spaceCoreDungeonLevel.Value + 1}", 0, 0, false);
                Game1.player.temporarilyInvincible = true;
                Game1.player.temporaryInvincibilityTimer = 0;
                Game1.player.flashDuringThisTemporaryInvincibility = false;
                Game1.player.currentTemporaryInvincibilityDuration = 1000;
                loc.playSound("stairsdown");

                return true;
            });
            GameLocation.RegisterTileAction("spacechase0.SpaceCore_DungeonMineshaft", (loc, args, who, tile) =>
            {
                var ext = loc.GetDungeonExtData();
                if (ext.spaceCoreDungeonId.Value == null)
                    return true;
                var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
                if (!dungeonsData.TryGetValue(ext.spaceCoreDungeonId.Value, out var dungeonData))
                {
                    Log.Error($"Failed to find dungeon data with ID \"{ext.spaceCoreDungeonId.Value}\"");
                    return true;
                }

                Response[] options =
                [
                    new Response("Jump", Game1.content.LoadString("Strings\\Locations:Mines_ShaftJumpIn")).SetHotKey(Keys.Y),
                    new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
                ];
                loc.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Mines_Shaft"), options, "Shaft");
                loc.afterQuestion = (who, response) =>
                {
                    if (response != "Shaft_Jump")
                        return;

                    DelayedAction.playSoundAfterDelay("fallDown", 800, loc);
                    DelayedAction.playSoundAfterDelay("clubSmash", 1800);
                    Random random = Utility.CreateRandom(loc.NameOrUniqueName.GetDeterministicHashCode(), Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
                    int levelsDown = random.Next(3, 9);
                    if (random.NextDouble() < 0.1)
                    {
                        levelsDown = levelsDown * 2 - 1;
                    }
                    int max = dungeonData.GetMaxLevel();
                    if (ext.spaceCoreDungeonLevel.Value < max && ext.spaceCoreDungeonLevel.Value + levelsDown > max)
                    {
                        levelsDown = max - ext.spaceCoreDungeonLevel.Value;
                    }
                    Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * 3);
                    Game1.globalFadeToBlack(() =>
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString((levelsDown > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", levelsDown));
                        Game1.messagePause = true;
                        string prefix = GetLevelNamePrefix(ext.spaceCoreDungeonId.Value);
                        Game1.warpFarmer($"{prefix}{ext.spaceCoreDungeonLevel.Value + levelsDown}", 0, 0, false);
                        Game1.player.temporarilyInvincible = true;
                        Game1.player.temporaryInvincibilityTimer = 0;
                        Game1.player.flashDuringThisTemporaryInvincibility = false;
                        Game1.player.currentTemporaryInvincibilityDuration = 1000;
                        Game1.fadeToBlackAlpha = 1f;
                        Game1.player.faceDirection(2);
                        Game1.player.showFrame(5);
                    }, 0.045f);
                    Game1.player.CanMove = false;
                    Game1.player.jump();
                };

                return true;
            });
        }

        private static void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/Dungeons"))
                e.LoadFrom(() => new Dictionary<string, DungeonData>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
        }
        private static void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            var data = deepestLevels.GetOrCreateValue(Game1.player.team);
            Dictionary<string, int> toSave = new();
            foreach (var entry in data.Pairs)
                toSave.Add(entry.Key, entry.Value);
            SpaceCore.Instance.Helper.Data.WriteSaveData("spacechase0.SpaceCore_DeepestDungeonLevels", toSave);
        }
        private static void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            var data = deepestLevels.GetOrCreateValue(Game1.player.team);
            var saved = SpaceCore.Instance.Helper.Data.ReadSaveData<Dictionary<string, int>>("spacechase0.SpaceCore_DeepestDungeonLevels");
            if (saved == null)
                return;
            foreach (var entry in saved)
                data.Add(entry.Key, entry.Value);
        }

        private static void Display_RenderedStep(object sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
        {
            if (e.Step != StardewValley.Mods.RenderSteps.World)
                return;

            var ext = Game1.currentLocation.GetDungeonExtData();
            if (ext.spaceCoreDungeonLevel.Value == 0 || Game1.game1.takingMapScreenshot)
                return;

            Color col = SpriteText.color_White;
            string txt = ext.spaceCoreDungeonLevel.Value.ToString();
            Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
            int height = SpriteText.getHeightOfString(txt);
            SpriteText.drawString(e.SpriteBatch, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, height, 1f, 1f, junimoText: false, 2, "", col);
        }

        public static string GetLevelNamePrefix(string dungeonId)
        {
            return $"SpaceCore_Dungeon_{dungeonId}_";
        }

        public static bool TryGetLevelInstance(string name, out GameLocation level)
        {
            if (State.ActiveLevels.ContainsKey(name))
            {
                level = State.ActiveLevels[name];
                return true;
            }

            string toGenerate = null;
            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            foreach (string key in dungeonsData.Keys)
            {
                if (name.StartsWith(GetLevelNamePrefix(key)))
                {
                    toGenerate = key;
                    break;
                }
            }

            if (toGenerate == null)
            {
                level = null;
                return false;
            }

            int levelNum = int.Parse(name.Substring(GetLevelNamePrefix(toGenerate).Length));
            level = GenerateLevel(toGenerate, name, levelNum);
            return true;
        }

        private static GameLocation GenerateLevel(string dungeon, string locName, int level)
        {
            int seed = Game1.random.Next();

            var loc = new GameLocation();
            var ext = loc.GetDungeonExtData();

            loc.uniqueName.Value = locName;
            ext.spaceCoreDungeonId.Value = dungeon;
            ext.spaceCoreDungeonLevel.Value = level;
            ext.spaceCoreDungeonSeed.Value = seed;
            ext.mapContent = Game1.game1.xTileContent.CreateTemporary();

            DoGeneration(loc);
            State.ActiveLevels.Add(locName, loc);

            return loc;
        }

        internal static void DoGeneration(GameLocation loc)
        {
            var ext = loc.GetDungeonExtData();
            ext.generated = true;

            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            var dungeon = dungeonsData[ext.spaceCoreDungeonId.Value];

            Random r = new Random(ext.spaceCoreDungeonSeed.Value);
            var regions = dungeon.Regions.Values.Where(r => r.LevelRange.Begin <= ext.spaceCoreDungeonLevel.Value && r.LevelRange.End >= ext.spaceCoreDungeonLevel.Value).ToList();
            var choices = regions.SelectMany(r => r.MapPool.Select(w => new Weighted<(DungeonData.DungeonRegion region, string mapPath)>(w.Weight, new(r, w.Value)))).ToList();
            var choice = choices.Choose(r);

            loc.name.Value = choice.region.LocationDataEntry;
            loc.mapPath.Value = choice.mapPath;

            foreach (string triggerId in choice.region.TriggerActionsOnEntry)
            {
                if (!VanillaAssetExpansion.VanillaAssetExpansion.manualTriggerActionsById.TryGetValue(triggerId, out var trigger))
                {
                    Log.Error($"Failed to find trigger action \"{triggerId}\" with \"Manual\" trigger type");
                    return;
                }
                TriggerActionContext ctx = new(triggerId, [loc, r], trigger.Data);
                if (GameStateQuery.CheckConditions(trigger.Data.Condition) && (!trigger.Data.HostOnly || Game1.IsMasterGame))
                {
                    foreach (var action in trigger.Actions)
                    {
                        if (!TriggerActionManager.TryRunAction(action, ctx, out string error, out Exception e))
                        {
                            Log.Error($"Trigger action {trigger.Data.Id} failed: {error} {e}");
                        }
                    }

                    if (trigger.Data.MarkActionApplied)
                        Game1.player.triggerActionsRun.Add(trigger.Data.Id);
                }
            }

            loc.updateMap();

            if (Game1.IsMasterGame)
            {
                int stonesLeftOnThisLevel = loc.Objects.Values.Count(o => o.IsBreakableStone());
                int EnemyCount = loc.characters.Count(c => (c is Monster && (!(c is Bug bug) || !bug.isArmoredBug.Value)));

                if (stonesLeftOnThisLevel == 0)
                {
                    if (EnemyCount > 0)
                    {
                        ext.spaceCoreDungeonIsMonsterLevel.Value = true;
                    }
                    // We want to force a ladder to spawn if there are no stones or monsters
                    else if (ext.spaceCoreDungeonLevel.Value != dungeon.GetMaxLevel() &&
                        (dungeon.SpawnLadders || dungeon.SpawnMineshafts))
                    {
                        for (int i = 0; i < 100; ++i)
                        {
                            int x = r.Next(loc.Map.Layers[0].LayerWidth);
                            int y = r.Next(loc.Map.Layers[0].LayerHeight);
                            Vector2 tile = new(x, y);

                            if (!loc.CanItemBePlacedHere(tile) || loc.IsNoSpawnTile(tile))
                                continue;

                            ext.spaceCoreDungeonLadders[new(x, y)] = !dungeon.SpawnLadders;

                            break;
                        }
                    }
                }
            }

            loc.ExtraMillisecondsPerInGameMinute = dungeon.AdditionalTimeMilliseconds / 10;
        }

        public static void UpdateActiveLevels(GameTime time)
        {
            foreach (var level in State.ActiveLevels.Values)
            {
                if (level.farmers.Any())
                    level.UpdateWhenCurrentLocation(time);
                level.updateEvenIfFarmerIsntHere(time);
            }
        }

        public static void UpdateActiveLevels10Minutes(int time)
        {
            ClearInactiveLevels();

            if (Game1.IsClient)
                return;

            foreach (var level in State.ActiveLevels.Values)
            {
                if (level.farmers.Any())
                    level.performTenMinuteUpdate(time);
            }
        }

        public static void UpdateRoots(Multiplayer mp)
        {
            foreach (var level in State.ActiveLevels.Values)
            {
                if (level.Root != null)
                {
                    level.Root.Clock.InterpolationTicks = mp.interpolationTicks();
                    mp.updateRoot(level.Root);
                }
            }
        }

        private static void ClearInactiveLevels()
        {
            HashSet<string> dungeonsWithPlayers = new();
            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer.disconnectDay.Value == Game1.MasterPlayer.stats.DaysPlayed &&
                    State.ActiveLevels.TryGetValue(farmer.disconnectLocation.Value, out var loc))
                {
                    dungeonsWithPlayers.Add(loc.GetDungeonExtData().spaceCoreDungeonId.Value);
                }

                if (farmer.locationBeforeForcedEvent.Value != null &&
                     State.ActiveLevels.TryGetValue(farmer.locationBeforeForcedEvent.Value, out loc))
                {
                    dungeonsWithPlayers.Add(loc.GetDungeonExtData().spaceCoreDungeonId.Value);
                }

                if (farmer.currentLocation == null)
                    continue;

                var currExt = farmer.currentLocation.GetDungeonExtData();
                if (currExt.spaceCoreDungeonId.Value != null)
                    dungeonsWithPlayers.Add(currExt.spaceCoreDungeonId.Value);
            }

            State.ActiveLevels.RemoveWhere(kvp =>
            {
                var ext = kvp.Value.GetDungeonExtData();
                if (dungeonsWithPlayers.Contains(ext.spaceCoreDungeonId.Value))
                    return false;

                ext.mapContent.Dispose();
                return true;
            });
        }

        internal static void OnLadderAdded(GameLocation loc, Point spot, bool shaft)
        {
            loc.updateMap();

            var ext = loc.GetDungeonExtData();
            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            var dungeon = dungeonsData[ext.spaceCoreDungeonId.Value];

            string tsPath = shaft ? dungeon.MineshaftTileSheet : dungeon.LadderTileSheet;
            int tindex = shaft ? dungeon.MineshaftTileIndex : dungeon.LadderTileIndex;

            var ts = loc.Map.TileSheets.FirstOrDefault(t => t.ImageSource == tsPath);
            if (ts == null)
            {
                var tex = Game1.content.Load<Texture2D>(tsPath);
                loc.Map.AddTileSheet(ts = new(loc.Map, tsPath, new(tex.Width / 16, tex.Height / 16), new(16, 16)));
                if (Game1.currentLocation == loc)
                    loc.Map.LoadTileSheets(Game1.mapDisplayDevice);
            }

            var buildings = loc.Map.RequireLayer("Buildings");
            buildings.Tiles[spot.X, spot.Y] = new StaticTile(buildings, ts, BlendMode.Alpha, tindex)
            {
                Properties = { { "Action", shaft ? "spacechase0.SpaceCore_DungeonMineshaft" : "spacechase0.SpaceCore_DungeonLadder" } }
            };
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.getLocationFromNameInLocationsList))]
    public static class GameGetLocationFromDungeonsPatch
    {
        public static bool Prefix(string name, bool isStructure, ref GameLocation __result)
        {
            if (DungeonImpl.TryGetLevelInstance(name, out var level))
            {
                __result = level;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Game1), "UpdateLocations")]
    public static class GameUpdateDungeonsPatch
    {
        public static void Postfix(GameTime time)
        {
            if (Game1.IsClient)
                return;

            DungeonImpl.UpdateActiveLevels(time);
        }
    }

    [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.UpdateMines10Minutes))]
    public static class MineShaftUse10MinutesUpdateForDungeonsTooPatch
    {
        public static void Postfix(int timeOfDay)
        {
            DungeonImpl.UpdateActiveLevels10Minutes(timeOfDay);
        }
    }

    [HarmonyPatch(typeof(Multiplayer), nameof(Multiplayer.updateRoots))]
    public static class MultiplayerUpdateDungeonRootsPatch
    {
        public static void Postfix(Multiplayer __instance)
        {
            DungeonImpl.UpdateRoots(__instance);
        }
    }

    [HarmonyPatch(typeof(GameLocation), "getMapLoader")]
    public static class GameLocationOverrideMapLoaderForDungeonsPatch
    {
        public static void Postfix(GameLocation __instance, ref LocalizedContentManager __result)
        {
            var ext = __instance.GetDungeonExtData();
            if (ext.spaceCoreDungeonId.Value != null && ext.mapContent != null)
            {
                __result = ext.mapContent;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocation), "resetLocalState")]
    public static class GameLocationResetLocalStateDungeonsPatch
    {
        public static void Prefix(GameLocation __instance)
        {
            var ext = __instance.GetDungeonExtData();
            if (ext.spaceCoreDungeonId.Value == null)
                return;

            if (!ext.generated)
            {
                DungeonImpl.DoGeneration(__instance);
                __instance.reloadMap();
            }
        }

        public static void Postfix(GameLocation __instance)
        {
            var ext = __instance.GetDungeonExtData();
            if (ext.spaceCoreDungeonId.Value == null)
                return;

            ArgUtility.TryGetPoint(__instance.GetMapPropertySplitBySpaces("spacechase0.SpaceCore_DungeonLadderEntrance"), 0, out Point entranceTile, out string err);
            if (Game1.player.ridingMineElevator)
            {
                ArgUtility.TryGetPoint(__instance.GetMapPropertySplitBySpaces("spacechase0.SpaceCore_DungeonElevatorEntrance"), 0, out entranceTile, out err);
            }

            Game1.xLocationAfterWarp = entranceTile.X;
            Game1.yLocationAfterWarp = entranceTile.Y;
            //if (Game1.IsClient)
            {
                Game1.player.Position = entranceTile.ToVector2() * Game1.tileSize;
            }

            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            __instance.forceViewportPlayerFollow = dungeonsData[ext.spaceCoreDungeonId.Value].ViewportFollowsPlayer;

            var deepestLevels = DungeonImpl.deepestLevels.GetOrCreateValue(Game1.player.team);
            deepestLevels.TryGetValue(ext.spaceCoreDungeonId.Value, out int deepest);
            deepestLevels[ext.spaceCoreDungeonId.Value] = Math.Max(deepest, ext.spaceCoreDungeonLevel.Value);
        }
    }

    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.OnStoneDestroyed))]
    public static class GameLocationSpawnLaddersInDungeonPatch
    {
        public static void Postfix(GameLocation __instance, string stoneId, int x, int y, Farmer who)
        {
            var ext = __instance.GetDungeonExtData();
            if (ext.spaceCoreDungeonId.Value == null)
                return;
            var dungeonsData = Game1.content.Load<Dictionary<string, DungeonData>>("spacechase0.SpaceCore/Dungeons");
            var dungeon = dungeonsData[ext.spaceCoreDungeonId.Value];
            if (!dungeon.SpawnMineshafts && !dungeon.SpawnLadders )
                return;
            if (ext.spaceCoreDungeonLevel.Value == dungeon.GetMaxLevel())
                return;

            Random r = Utility.CreateDaySaveRandom(x * 1000, y, __instance.NameOrUniqueName.GetDeterministicHashCode());

            int stonesLeftOnThisLevel = __instance.Objects.Values.Count(o => o.IsBreakableStone());
            int EnemyCount = __instance.characters.Count(c => (c is Monster && (!(c is Bug bug) || !bug.isArmoredBug.Value)));

            double chanceForLadderDown = 0.02 + 1.0 / (double)Math.Max(1, stonesLeftOnThisLevel) + (double)(who?.LuckLevel ?? 0) / 100.0 + Game1.player.DailyLuck / 5.0;
            if (EnemyCount == 0)
            {
                chanceForLadderDown += 0.04;
            }
            if (who != null && who.hasBuff("dwarfStatue_1"))
            {
                chanceForLadderDown *= 1.25;
            }

            if (stonesLeftOnThisLevel == 1 || r.NextDouble() < chanceForLadderDown)
            {
                bool shaft = !dungeon.SpawnLadders;
                if (dungeon.SpawnLadders && Game1.random.NextDouble() < 0.2)
                    shaft = true;

                ext.spaceCoreDungeonLadders[new( x, y)] = shaft;
            }
        }
    }
}
