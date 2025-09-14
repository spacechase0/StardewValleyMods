using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json.Linq;
using Spacechase.Shared.Patching;
using SpaceCore.Spawnables;
using SpaceShared;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Delegates;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace SpaceCore.Dungeons
{
    [XmlType("Mods_spacechase0_SpaceCore_SetPieceNetData")]
    public class SetPieceNetData : INetObject<NetFields>
    {
        public NetFields NetFields { get; }

        public readonly NetString FromSpawnDef = new();
        public readonly NetPoint Tile = new();
        public readonly NetInt PieceIndex = new();

        public SetPieceNetData()
        {
            NetFields = new("SetPieceNetData");
            NetFields.SetOwner(this)
                .AddField(FromSpawnDef)
                .AddField(Tile)
                .AddField(PieceIndex);
        }
    }

    public class GameLocationSpawnableExt
    {
        public readonly NetList<SetPieceNetData, NetRef<SetPieceNetData>> setPieceCache = new();

        public static ConditionalWeakTable<GameLocation, GameLocationSpawnableExt> data = new();
    }

    public static class SpawnableExtensions
    {
        public static GameLocationSpawnableExt GetSpawnableExtData(this GameLocation loc)
        {
            return GameLocationSpawnableExt.data.GetOrCreateValue(loc);
        }

        public static List<Item> SimplifyDrops(this List<List<Weighted<GenericSpawnItemDataWithCondition>>> items, GameLocation location, Farmer player, Random r)
        {
            List<Item> drops = new();
            foreach (var spawnList in items)
            {
                var itemSpawns = spawnList.ToList();
                itemSpawns.RemoveAll(w => w.Value.Condition != null && !w.Value.Condition.Equals("true", StringComparison.InvariantCultureIgnoreCase) && !GameStateQuery.CheckConditions(w.Value.Condition, location: location, player: player, random: r));
                GenericSpawnItemDataWithCondition itemSpawn = itemSpawns.Choose(r);
                if (itemSpawn == null)
                    continue;
                Item item = ItemQueryResolver.TryResolveRandomItem(itemSpawn, new ItemQueryContext(location, player, r, "SpaceCore Spawnable drops"));
                drops.Add(item);
            }

            return drops;
        }
    }

    [HarmonyPatch(typeof(GameLocation), "initNetFields")]
    public static class GameLocationSpawnableNetFieldAdditionsPatch
    {
        public static void Postfix(GameLocation __instance)
        {
            __instance.NetFields.AddField(__instance.GetSpawnableExtData().setPieceCache);
        }
    }

    internal static class SpawnableImpl
    {
        public static void Init()
        {
            SpaceCore.Instance.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            SpaceCore.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;
            SpaceCore.Instance.Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_TriggerSpawnGroup", (string[] args, TriggerActionContext context, out string error) =>
            {
                if (args.Length < 3)
                {
                    error = "Not enough arguments";
                    return false;
                }

                Random r = context.TriggerArgs.FirstOrDefault(o => o is Random) as Random;

                var loc = GameStateQuery.Helpers.RequireLocation(args[2], context.TriggerArgs.Length > 0 ? (context.TriggerArgs[0] as GameLocation) : null );
                List<Rectangle> includeRegions = new();
                if (args.Length >= 4 && args[3] != "null")
                {
                    string[] rectStrs = args[3].Split('/');
                    int i = 0;
                    foreach (string rectStr in rectStrs)
                    {
                        string[] rectParts = rectStr.Split(',');
                        if (!ArgUtility.TryGetRectangle(rectParts, 0, out Rectangle rect, out error))
                        {
                            error = $"Rectangle parsing error for includeRegion {i}: {error}";
                            return false;
                        }
                        includeRegions.Add(rect);
                        ++i;
                    }
                }
                else
                {
                    includeRegions.Add(new Rectangle(0, 0, loc.Map.Layers[0].LayerWidth, loc.Map.Layers[0].LayerHeight));
                }

                List<Rectangle> excludeRegions = new();
                if (args.Length >= 5 && args[4] != "null")
                {
                    string[] rectStrs = args[4].Split('/');
                    int i = 0;
                    foreach (string rectStr in rectStrs)
                    {
                        string[] rectParts = rectStr.Split(',');
                        if (!ArgUtility.TryGetRectangle(rectParts, 0, out Rectangle rect, out error))
                        {
                            error = $"Rectangle parsing error for excludeRegion {i}: {error}";
                            return false;
                        }
                        excludeRegions.Add(rect);
                        ++i;
                    }
                }

                error = null;
                DoSpawning(loc, args[1], includeRegions, excludeRegions, r);
                return true;
            });

            TriggerActionManager.RegisterAction("spacechase0.SpaceCore_ClearSetPiecesFromSpawnable", (string[] args, TriggerActionContext context, out string error) =>
            {
                if (args.Length < 3)
                {
                    error = "Not enough arguments";
                    return false;
                }

                var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
                var spawnDef = spawnDefs[args[1]];

                var loc = GameStateQuery.Helpers.RequireLocation(args[2], context.TriggerArgs.Length > 0 ? (context.TriggerArgs[0] as GameLocation) : null);
                var origMap = Game1.temporaryContent.Load<xTile.Map>(loc.mapPath.Value);
                var appliedOverrides = SpaceCore.Instance.Helper.Reflection.GetField<HashSet<string>>(loc, "_appliedMapOverrides").GetValue();
                var ext = loc.GetSpawnableExtData();

                var applicable = ext.setPieceCache.Where(sp => sp.FromSpawnDef.Value == args[1]);
                appliedOverrides.RemoveWhere(s => s.StartsWith($"spacecore_setpiece_{args[1]}"));

                foreach (var single in applicable)
                {
                    Rectangle rect = new(single.Tile.Value, new(spawnDef.SetPieceSizeX, spawnDef.SetPieceSizeY));
                    loc.ApplyMapOverride(origMap, "spacecore_tmp", rect, rect);
                    appliedOverrides.Remove("spacecore_tmp");
                }
                ext.setPieceCache.RemoveWhere(sp => sp.FromSpawnDef.Value == args[1]);

                error = null;
                return true;
            });
        }

        private static void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var sc = SpaceCore.Instance.Helper.ModRegistry.GetApi<IApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(SetPieceNetData));
        }

        private static void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            Utility.ForEachLocation(loc =>
            {
                foreach (var pair in loc.Objects.Pairs.ToList())
                {
                    if (pair.Value.modData.TryGetValue("spacechase0.SpaceCore/DisappearOnDate", out string dayStr) && int.TryParse( dayStr, out int day))
                    {
                        if (Game1.Date.TotalDays + 1 == day)
                        {
                            loc.Objects.Remove(pair.Key);
                        }
                    }
                }
                return true;
            });
        }

        private static void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/SpawnableDefinitions"))
                e.LoadFrom(() => new Dictionary<string, SpawnableDefinitionData>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
            else if (e.NameWithoutLocale.IsEquivalentTo("spacechase0.SpaceCore/SpawningGroups"))
                e.LoadFrom(() => new Dictionary<string, SpawnableSpawningGroupData>(), StardewModdingAPI.Events.AssetLoadPriority.Low);
        }

        public static void DoSpawning(GameLocation location, string spawnGroupId, List<Rectangle> includeRegions, List<Rectangle> excludeRegions, Random r = null)
        {
            r ??= Game1.random;

            List<Point> validTiles = new();
            foreach (var rect in includeRegions)
            {
                for (int ix = 0; ix < rect.Width; ++ix)
                {
                    for (int iy = 0; iy < rect.Height; ++iy)
                    {
                        validTiles.Add(new Point(rect.X + ix, rect.Y + iy));
                    }
                }
            }
            validTiles = validTiles.Distinct().ToList();
            foreach (var rect in excludeRegions)
            {
                for (int ix = 0; ix < rect.Width; ++ix)
                {
                    for (int iy = 0; iy < rect.Height; ++iy)
                    {
                        validTiles.Remove(new Point(rect.X + ix, rect.Y + iy));
                    }
                }
            }
            foreach (var tile in validTiles.ToList())
            {
                if (!IsTileValid(location, tile.ToVector2()))
                    validTiles.Remove(tile);
            }
            if (ArgUtility.TryGetPoint(location.GetMapPropertySplitBySpaces("spacechase0.SpaceCore_DungeonLadderEntrance"), 0, out Point entranceTile, out string err))
                validTiles.Remove(entranceTile);
            if (ArgUtility.TryGetPoint(location.GetMapPropertySplitBySpaces("spacechase0.SpaceCore_DungeonElevatorEntrance"), 0, out entranceTile, out err))
                validTiles.Remove(entranceTile);
            if (validTiles.Count == 0)
            {
                Log.Warn($"No valid spawn tiles, using spawn group '{spawnGroupId}'?");
                return;
            }
            validTiles.Shuffle();

            var spawnGroups = Game1.content.Load<Dictionary<string, SpawnableSpawningGroupData>>("spacechase0.SpaceCore/SpawningGroups");
            if (!spawnGroups.TryGetValue(spawnGroupId, out var spawnGroup))
            {
                Log.Warn($"No valid spawn group with ID '{spawnGroupId}'");
                return;
            }

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            List<(string id, SpawnableDefinitionData data)> toSpawn = new();
            foreach (var entry in spawnGroup.SpawnablesToSpawn)
            {
                var choices = entry.SpawnableIds.Select(ws => new Weighted<(string id, SpawnableDefinitionData data)>(ws.Weight, new(ws.Value, spawnDefs[ws.Value]))).ToList();

                int count = r.Next(entry.Maximum - entry.Minimum + 1);
                for (int i = 0; i < entry.Minimum + count; ++i)
                {
                    toSpawn.Add(choices.Choose(r));
                }
            }
            toSpawn.Shuffle(r);

            while (toSpawn.Count > 0 && validTiles.Count > 0)
            {
                var spawn = toSpawn[0];
                toSpawn.RemoveAt(0);

                for (int i = 0; i < 10; ++i)
                {
                    var tile = validTiles[0];
                    validTiles.RemoveAt(0);

                    if (spawnHandlers[spawn.data.Type](location, spawn.id, spawn.data, tile, r))
                        break;
                }
            }
        }

        private static Dictionary<SpawnableDefinitionData.SpawnableType, Func<GameLocation, string, SpawnableDefinitionData, Point, Random, bool>> spawnHandlers = new() 
        {
            { SpawnableDefinitionData.SpawnableType.SetPiece, HandleSpawnable_SetPiece },
            { SpawnableDefinitionData.SpawnableType.Forageable, HandleSpawnable_Forageable },
            { SpawnableDefinitionData.SpawnableType.Minable, HandleSpawnable_Minable },
            { SpawnableDefinitionData.SpawnableType.LargeMinable, HandleSpawnable_LargeMinable },
            { SpawnableDefinitionData.SpawnableType.Breakable, HandleSpawnable_Breakable },
            { SpawnableDefinitionData.SpawnableType.LootChest, HandleSpawnable_LootChest },
            { SpawnableDefinitionData.SpawnableType.Furniture, HandleSpawnable_Furniture },
            { SpawnableDefinitionData.SpawnableType.Monster, HandleSpawnable_Monster },
            //{ SpawnableDefinitionData.SpawnableType.Critter, HandleSpawnable_Critter },
            { SpawnableDefinitionData.SpawnableType.WildTree, HandleSpawnable_WildTree },
            { SpawnableDefinitionData.SpawnableType.FruitTree, HandleSpawnable_FruitTree },
        };

        private static bool IsTileValid(GameLocation loc, Vector2 tile)
        {
            if (loc.IsNoSpawnTile(tile))
                return false;

            if (!loc.CanItemBePlacedHere(tile, ignorePassables: CollisionMask.None))
                return false;
            if (loc.IsTileOccupiedBy(tile, CollisionMask.Characters | CollisionMask.Flooring | CollisionMask.TerrainFeatures))
                return false;
            if (loc.Objects.ContainsKey(tile))
                return false;
            if (loc.Map.RequireLayer("Back").Tiles[(int)tile.X, (int)tile.Y] == null)
                return false;
            if (loc.Map.RequireLayer("Front").Tiles[(int)tile.X, (int)tile.Y] != null)
                return false;
            if (loc.Map.RequireLayer("Buildings").Tiles[(int)tile.X, (int)tile.Y] != null)
                return false;

            var ext = loc.GetSpawnableExtData();
            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            foreach (var sp in ext.setPieceCache)
            {
                var data = spawnDefs[sp.FromSpawnDef.Value];
                if (new Rectangle(sp.Tile.Value, new(data.SetPieceSizeX, data.SetPieceSizeY)).Contains(tile))
                    return false;
            }

            return true;
        }

        private static bool HandleSpawnable_SetPiece(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (data.SetPiecesMap == null || !Game1.content.DoesAssetExist<xTile.Map>(data.SetPiecesMap))
            {
                Log.Warn($"Set piece spawnable '{id}' has invalid map path (does it exist?), skipping");
                return true; // So it doesn't try again even though it would fail
            }
            for (int ix = 0; ix < data.SetPieceSizeX; ++ix)
            {
                for (int iy = 0; iy < data.SetPieceSizeY; ++iy)
                {
                    Vector2 spot = tile.ToVector2() + new Vector2(ix, iy);
                    if (!IsTileValid(location, spot))
                        return false;
                }
            }

            var ext = location.GetSpawnableExtData();
            int ind = r.Next(data.SetPieceCount);
            ext.setPieceCache.Add(new()
            {
                FromSpawnDef = { id },
                Tile = { tile },
                PieceIndex = { ind },
            });


            var from = Game1.content.Load<xTile.Map>(data.SetPiecesMap);
            var paths = from.GetLayer("Paths");
            if (paths != null)
            {
                int cols = from.Layers[0].LayerWidth / data.SetPieceSizeX;
                int sx = (ind % cols) * data.SetPieceSizeX;
                int sy = (ind / cols) * data.SetPieceSizeY;

                for (int ix = 0; ix < data.SetPieceSizeX; ++ix)
                {
                    for (int iy = 0; iy < data.SetPieceSizeY; ++iy)
                    {
                        xTile.Tiles.Tile t = paths.Tiles[new(sx + ix, sy + iy)];
                        if (t != null && t.Properties != null && t.Properties.TryGetValue("spacechase0.SpaceCore/TriggerSpawnGroup", out string spawnGroupId))
                        {
                            DoSpawning(location, spawnGroupId, [new Rectangle( tile.X + ix, tile.Y + iy, 1, 1 )], []);
                        }
                    }
                }
            }

            return true;
        }

        private static bool HandleSpawnable_Forageable(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;

            var itemSpawns = data.ForageableItemData.ToList();
            itemSpawns.RemoveAll(w => w.Value.Condition != null && !w.Value.Condition.Equals("true", StringComparison.InvariantCultureIgnoreCase) && !GameStateQuery.CheckConditions(w.Value.Condition, location: location, random: r));
            GenericSpawnItemDataWithCondition itemSpawn = itemSpawns.Choose(r);
            if ( itemSpawn == null )
                return false;
            Item item = ItemQueryResolver.TryResolveRandomItem(itemSpawn, new ItemQueryContext(location, null, r, "SpaceCore Spawnable Forage drops"));

            StardewValley.Object obj = null;
            if (data.ForageableIsTillSpot)
            {
                obj = new StardewValley.Object("590", 1);
                obj.modData.Add("spacechase0.SpaceCore/TillDropOverride", item.QualifiedItemId);
            }
            else
            {
                obj = (StardewValley.Object)item;
                obj.IsSpawnedObject = true;
                obj.modData.Add("spacechase0.SpaceCore/IsForage", "meow");
            }

            if (data.ForageableExpiresWeekly)
            {
                WorldDate date = new(Game1.Date);
                while (date.DayOfWeek != DayOfWeek.Sunday)
                    date.TotalDays++;

                obj.modData.Add("spacechase0.SpaceCore/DisappearOnDate", date.TotalDays.ToString());
            }

            location.objects[tile.ToVector2()] = obj;

            return true;
        }

        private static bool HandleSpawnable_Minable(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;

            StardewValley.Object obj = (StardewValley.Object) ItemRegistry.Create( ItemRegistry.ManuallyQualifyItemId( data.MinableObjectId, "(O)" ) );
            obj.modData.Add("spacechase0.SpaceCore/Minable", id);
            obj.MinutesUntilReady = data.MineableHealth;

            location.objects[tile.ToVector2()] = obj;

            return true;
        }

        private static bool HandleSpawnable_LargeMinable(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            for (int ix = 0; ix < data.LargeMinableSizeX; ++ix)
            {
                for (int iy = 0; iy < data.LargeMinableSizeY; ++iy)
                {
                    Vector2 spot = tile.ToVector2() + new Vector2( ix, iy );
                    if (!IsTileValid(location, spot))
                        return false;
                }
            }

            ResourceClump rc = new(data.LargeMinableSpriteIndex, data.LargeMinableSizeX, data.LargeMinableSizeY, tile.ToVector2(), data.LargeMinableHealth, data.LargeMinableTexture);
            rc.modData.Add("spacechase0.SpaceCore/LargeMinable", id);

            location.resourceClumps.Add(rc);

            return true;
        }

        private static bool HandleSpawnable_Breakable(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;

            var breakable = new BreakableContainer(tile.ToVector2(), data.BreakableBigCraftableId, data.BreakableHealth, hitSound: data.BreakableHitSound, breakSound: data.BreakableBrokenSound);
            breakable.modData.Add("spacechase0.SpaceCore/Breakable", id);

            location.objects[tile.ToVector2()] = breakable;

            return true;
        }

        private static bool HandleSpawnable_LootChest(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;

            var chest = new Chest(false, data.LootChestBigCraftableId);
            chest.dropContents.Value = true;
            chest.synchronized.Value = true;
            chest.type.Value = "interactive";
            chest.startingLidFrame.Value = chest.ParentSheetIndex + 1;
            foreach (var drop in data.LootChestDrops.SimplifyDrops(location, null, Game1.random))
            {
                chest.addItem(drop);
            }
            chest.modData.Add("spacechase0.SpaceCore/SpawnableDrawing", "meow");

            location.objects[tile.ToVector2()] = chest;

            return true;
        }

        private static bool HandleSpawnable_Furniture(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            var furniture = ItemRegistry.Create<Furniture>(data.FurnitureQualifiedId);
            furniture.TileLocation = tile.ToVector2();
            furniture.currentRotation.Value = data.FurnitureRotation;
            furniture.RecalculateBoundingBox();
            furniture.IsOn = data.FurnitureIsOn;
            if (!data.FurnitureCanPickUp)
                furniture.modData.Add("spacechase0.SpaceCore/DisableRemoval", "meow");

            for (int ix = 0; ix < furniture.GetBoundingBox().Width / Game1.tileSize; ++ix)
            {
                for (int iy = 0; iy < furniture.GetBoundingBox().Height / Game1.tileSize; ++iy)
                {
                    Vector2 spot = tile.ToVector2() + new Vector2(ix, iy);
                    if (!IsTileValid(location, spot))
                        return false;
                }
            }

            var itemSpawns = data.FurnitureHeldObject.ToList();
            itemSpawns.RemoveAll(w => w.Value.Condition != null && !w.Value.Condition.Equals("true", StringComparison.InvariantCultureIgnoreCase) && !GameStateQuery.CheckConditions(w.Value.Condition, location: location, player: null, random: r));
            GenericSpawnItemDataWithCondition itemSpawn = itemSpawns.Choose(r);
            if (itemSpawn != null)
            {
                Item item = ItemQueryResolver.TryResolveRandomItem(itemSpawn, new ItemQueryContext(location, null, r, "SpaceCore Spawnable Furniture held object"));
                furniture.heldObject.Value = item as StardewValley.Object;
            }

            location.furniture.Add(furniture);

            return true;
        }

        internal static Dictionary<string, Func<Vector2, Dictionary<string, object>, Monster>> MonsterFactory = new()
        {
            { "Bat", (pos, data) => new Bat( pos ) },
            { "BigSlime", (pos, data) =>
            {
                var ret = new BigSlime( pos, 0 );
                if ( data.TryGetValue( "Color", out object val ) )
                {
                    if ( val is Color c )
                        ret.c.Value = c;
                    else if ( val is JObject jobj )
                        ret.c.Value = jobj.ToObject< Color >();
                }
                if ( data.TryGetValue( "HeldItemQualifiedId", out val ) && val is string qualId )
                {
                    ret.heldItem.Value = ItemRegistry.Create( qualId );
                }
                return ret;
            } },
            { "BlueSquid", (pos, data) => new BlueSquid( pos ) },
            { "Bug", (pos, data) =>
            {
                var ret = new Bug( pos, 0 );
                if ( data.TryGetValue( "IsArmored", out object val ) && (val is string str && str.Equals( "true", StringComparison.InvariantCultureIgnoreCase )) || (val is bool b && b) )
                    ret.isArmoredBug.Value = true;
                return ret;
            } },
            { "DinoMonster", (pos, data) => new DinoMonster( pos ) },
            { "Duggy", (pos, data) => new Duggy( pos ) },
            { "DustSpirit", (pos, data) => new DustSpirit( pos ) },
            { "DwarvishSentry", (pos, data) => new DwarvishSentry( pos ) },
            { "Fly", (pos, data) => new Fly( pos ) },
            { "Ghost", (pos, data) =>
            {
                var ret = new Ghost( pos );
                if ( data.TryGetValue( "IsPutrid", out object val ) && val is string str && str.Equals( "true", StringComparison.InvariantCultureIgnoreCase ) || (val is bool b && b) )
                    ret.variant.Value = Ghost.GhostVariant.Putrid;
                return ret;
            } },
            { "GreenSlime", (pos, data) =>
            {
                var ret = new GreenSlime( pos, 0 );
                if ( data.TryGetValue( "Color", out object val ) )
                {
                    if ( val is Color c )
                        ret.color.Value = c;
                    else if ( val is JObject jobj )
                        ret.color.Value = jobj.ToObject< Color >();
                }
                return ret;
            } },
            { "Grub", (pos, data) => new Grub( pos ) },
            { "HotHead", (pos, data) => new HotHead( pos ) },
            { "LavaLurk", (pos, data) => new LavaLurk( pos ) },
            { "Leaper", (pos, data) => new Leaper( pos ) },
            { "MetalHead", (pos, data) => new MetalHead( pos, 0 ) },
            { "Mummy", (pos, data) => new Mummy( pos ) },
            { "RockCrab", (pos, data) =>
            {
                var ret = new RockCrab( pos );
                if ( data.TryGetValue( "IsStickBug", out object val ) && val is string str && str.Equals( "true", StringComparison.InvariantCultureIgnoreCase ) || (val is bool b && b) )
                    SpaceCore.Instance.Helper.Reflection.GetField<NetBool>(ret, "isStickBug").GetValue().Value = true;
                return ret;
            } },
            { "RockGolem", (pos, data) => new RockGolem( pos ) },
            { "Serpent", (pos, data) =>
            {
                var ret = new Serpent( pos );
                if ( data.TryGetValue( "SegmentCount", out object val ) )
                {
                    if ( val is long l )
                        ret.segmentCount.Value = (int)l;
                    else if ( val is int i )
                        ret.segmentCount.Value = i;
                }
                return ret;
            } },
            { "ShadowBrute", (pos, data) => new ShadowBrute( pos ) },
            { "ShadowGirl", (pos, data) => new ShadowGirl( pos ) },
            { "ShadowGuy", (pos, data) => new ShadowGuy( pos ) },
            { "ShadowShaman", (pos, data) => new ShadowShaman( pos ) },
            { "Shooter", (pos, data) => new Shooter( pos ) },
            { "Skeleton", (pos, data) =>
            {
                var ret = new Skeleton( pos );
                if ( data.TryGetValue( "IsMage", out object val ) && val is string str && str.Equals( "true", StringComparison.InvariantCultureIgnoreCase ) || (val is bool b && b) )
                    ret.isMage.Value = true;
                return ret;
            } },
            { "Spiker", (pos, data) =>
            {
                int dir = 0;
                if ( data.TryGetValue( "Direction", out object val ) )
                {
                    if ( val is long l )
                        dir = (int)l;
                    else if ( val is int i )
                        dir = i;
                }
                return new Spiker( pos, dir );
            } },
            { "SquidKid", (pos, data) => new SquidKid( pos ) },
        };

        private static bool HandleSpawnable_Monster(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            var monster = MonsterFactory[ data.MonsterType ](tile.ToVector2() * Game1.tileSize, data.MonsterAdditionalData ?? new());

            for (int ix = 0; ix < monster.GetBoundingBox().Width / Game1.tileSize; ++ix)
            {
                for (int iy = 0; iy < monster.GetBoundingBox().Height / Game1.tileSize; ++iy)
                {
                    Vector2 spot = tile.ToVector2() + new Vector2(ix, iy);
                    if (!IsTileValid(location, spot))
                        return false;
                }
            }

            monster.modData.Add("spacechase0.SpaceCore/Spawnable", id);

            if (data.MonsterName != null)
            {
                SpaceCore.Instance.Helper.Reflection.GetMethod(monster, "parseMonsterInfo").Invoke(data.MonsterName);
            }
            if (data.MonsterTextureOverride != null)
            {
                bool old = monster.Sprite.ignoreSourceRectUpdates;
                monster.Sprite.ignoreSourceRectUpdates = true;
                monster.Sprite.LoadTexture(data.MonsterTextureOverride);
                monster.Sprite.ignoreSourceRectUpdates = old;
            }

            location.characters.Add(monster);

            return true;
        }

        /*
        internal static Dictionary<string, Func<Vector2, Critter>> CritterFactory = new()
        {
            { "Birdie", (pos, data) => new Birdie( pos ) },
        };

        private static bool HandleSpawnable_Critter(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            var monster = MonsterFactory[data.MonsterType](tile.ToVector2() * Game1.tileSize, data.MonsterAdditionalData ?? new());

            for (int ix = 0; ix < monster.GetBoundingBox().Width / Game1.tileSize; ++ix)
            {
                for (int iy = 0; iy < monster.GetBoundingBox().Height / Game1.tileSize; ++iy)
                {
                    Vector2 spot = tile.ToVector2() + new Vector2(ix, iy);
                    if (!location.CanItemBePlacedHere(spot) || location.IsNoSpawnTile(spot))
                        return false;
                }
            }

            monster.modData.Add("spacechase0.SpaceCore/Spawnable", id);

            if (data.MonsterName != null)
            {
                SpaceCore.Instance.Helper.Reflection.GetMethod(monster, "parseMonsterInfo").Invoke(data.MonsterName);
            }
            if (data.MonsterTextureOverride != null)
            {
                monster.modData.Add("spacechase0.SpaceCore/TextureOverride", data.MonsterTextureOverride);
                monster.reloadSprite(onlyAppearance: true);
            }

            location.characters.Add(monster);

            return true;
        }
        */

        private static bool HandleSpawnable_WildTree(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;
            if (location.terrainFeatures.ContainsKey(tile.ToVector2()))
                return false;

            var tree = new Tree(data.WildTreeType, 5);

            location.terrainFeatures[tile.ToVector2()] = tree;

            return true;
        }

        private static bool HandleSpawnable_FruitTree(GameLocation location, string id, SpawnableDefinitionData data, Point tile, Random r)
        {
            if (!IsTileValid(location, tile.ToVector2()))
                return false;
            if (location.terrainFeatures.ContainsKey(tile.ToVector2()))
                return false;

            var tree = new FruitTree(data.FruitTreeType, 4);
            tree.modData.Add("spacechase0.SpaceCore/PreventSaplingDrop", "meow");
            for ( int i = 0; i < 3; ++i )
                tree.TryAddFruit();

            location.terrainFeatures[tile.ToVector2()] = tree;

            return true;
        }
    }

    [HarmonyPatch(typeof(GameLocation), "resetLocalState")]
    public static class GameLocationApplySetPiecesPatch
    {
        public static void Postfix(GameLocation __instance)
        {
            var ext = __instance.GetSpawnableExtData();
            if (ext.setPieceCache.Count == 0)
                return;

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");

            Dictionary<string, int> counters = new();

            foreach (var entry in ext.setPieceCache)
            {
                string spawnableId = entry.FromSpawnDef.Value;
                if (!spawnDefs.TryGetValue(spawnableId, out var spawnDef))
                    continue;
                if (!counters.ContainsKey(spawnableId))
                    counters[spawnableId] = 0;
                else ++counters[spawnableId];

                var from = Game1.content.Load<xTile.Map>(spawnDef.SetPiecesMap);
                int cols = from.Layers[0].LayerWidth / spawnDef.SetPieceSizeX;

                int sx = (entry.PieceIndex.Value % cols) * spawnDef.SetPieceSizeX;
                int sy = (entry.PieceIndex.Value / cols) * spawnDef.SetPieceSizeY;

                __instance.ApplyMapOverride(from, $"spacecore_setpiece_{spawnableId}_{counters[spawnableId]}", new Rectangle(sx, sy, spawnDef.SetPieceSizeX, spawnDef.SetPieceSizeY), new Rectangle(entry.Tile.X, entry.Tile.Y, spawnDef.SetPieceSizeX, spawnDef.SetPieceSizeY));

                // TODO: do tile property stuff
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.isForage))]
    public static class ObjectIsForageSpawnablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            if (__instance.modData.ContainsKey("spacechase0.SpaceCore/IsForage"))
                __result = true;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction))]
    public static class ObjectToolActionTillableSpawnablePatch
    {
        public static bool Prefix(StardewValley.Object __instance, Tool t, ref bool __result)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/TillDropOverride", out string qualId ))
                return true;

            if (!__instance.isTemporarilyInvisible && t is Hoe)
            {
                GameLocation location = __instance.Location;

                Random r = Utility.CreateDaySaveRandom((0f - __instance.tileLocation.X) * 7f, __instance.tileLocation.Y * 777f, Game1.netWorldState.Value.TreasureTotemsUsed * 777);
                t.getLastFarmerToUse().stats.Increment("ArtifactSpotsDug", 1);
                if (t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") > 2 && r.NextDouble() < 0.008 + ((!t.getLastFarmerToUse().mailReceived.Contains("DefenseBookDropped")) ? ((double)t.getLastFarmerToUse().stats.Get("ArtifactSpotsDug") * 0.002) : 0.005))
                {
                    t.getLastFarmerToUse().mailReceived.Add("DefenseBookDropped");
                    Vector2 position2 = __instance.TileLocation * 64f;
                    Game1.createMultipleItemDebris(ItemRegistry.Create("(O)Book_Defense"), position2, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location);
                }

                Vector2 position = __instance.TileLocation * 64f;
                Game1.createMultipleItemDebris(ItemRegistry.Create(qualId), position, Utility.GetOppositeFacingDirection(t.getLastFarmerToUse().FacingDirection), location);

                if (!location.terrainFeatures.ContainsKey(__instance.tileLocation.Value))
                {
                    location.makeHoeDirt(__instance.tileLocation.Value, ignoreChecks: true);
                }
                __instance.playNearbySoundAll("hoeHit");
                location.objects.Remove(__instance.tileLocation.Value);

                __result = false;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.IsBreakableStone))]
    public static class ObjectIsStoneSpawnablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            if (__instance.modData.TryGetValue("spacechase0.SpaceCore/Minable", out string spawnId))
            {
                var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
                __result = spawnDefs[spawnId].MinableTool == SpawnableDefinitionData.MinableType.Pickaxe;
            }
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.IsTwig))]
    public static class ObjectIsTwigSpawnablePatch
    {
        public static void Postfix(StardewValley.Object __instance, ref bool __result)
        {
            if (__instance.modData.TryGetValue("spacechase0.SpaceCore/Minable", out string spawnId))
            {
                var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
                __result = spawnDefs[spawnId].MinableTool == SpawnableDefinitionData.MinableType.Axe;
            }
        }
    }

    [HarmonyPatch(typeof(GameLocation), "breakStone")]
    public static class GameLocationSpawnableStoneDestroyedPatch
    {
        public static bool Prefix(GameLocation __instance, string stoneId, int x, int y, Farmer who, Random r, ref bool __result)
        {
            if (!__instance.Objects.TryGetValue(new(x, y), out var actualObj) || !actualObj.modData.TryGetValue( "spacechase0.SpaceCore/Minable", out string spawnId ) )
                return true;
            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];

            foreach (var drop in spawnDef.MinableDrops.SimplifyDrops(__instance, who, r))
                Game1.createItemDebris(drop, new Vector2(x + 0.5f, y + 0.75f) * Game1.tileSize, who.FacingDirection, __instance);

            if (spawnDef.MinableExperienceGranted > 0)
            {
                who.gainExperience(Farmer.miningSkill, spawnDef.MinableExperienceGranted);
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(StardewValley.Object), nameof(StardewValley.Object.performToolAction))]
    public static class ObjectToolActionMinableAxeSpawnablePatch
    {
        public static bool Prefix(StardewValley.Object __instance, Tool t, ref bool __result)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/Minable", out string spawnId))
                return true;

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];
            if (spawnDef.MinableTool != SpawnableDefinitionData.MinableType.Axe)
                return true;

            if (!__instance.isTemporarilyInvisible && t is Axe)
            {
                GameLocation location = __instance.Location;

                __instance.fragility.Value = 2;
                __instance.playNearbySoundAll("axchop");

                foreach (var drop in spawnDef.MinableDrops.SimplifyDrops(__instance.Location, t.getLastFarmerToUse(), Game1.random))
                    Game1.createItemDebris(drop, new Vector2(__instance.TileLocation.X + 0.5f, __instance.TileLocation.Y + 0.75f) * Game1.tileSize, t.getLastFarmerToUse().FacingDirection, location);

                if (spawnDef.MinableExperienceGranted > 0)
                {
                    t.getLastFarmerToUse().gainExperience(Farmer.foragingSkill, spawnDef.MinableExperienceGranted);
                }

                __result = true;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ResourceClump), nameof(ResourceClump.performToolAction))]
    public static class ResourceClumpPerformToolActionForSpawnablePatch
    {
        public static bool Prefix(ResourceClump __instance, Tool t, int damage, Vector2 tileLocation, ref bool __result)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/LargeMinable", out string spawnId))
                return true;

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];
            if (t is not Axe && spawnDef.LargeMinableTool == SpawnableDefinitionData.MinableType.Axe ||
                 t is not Pickaxe && spawnDef.LargeMinableTool == SpawnableDefinitionData.MinableType.Pickaxe)
            {
                __result = false;
                return false;
            }

            var ticker = SpaceCore.Instance.Helper.Reflection.GetField<int>(__instance, "lastToolHitTicker");
            if (ticker.GetValue() == t.swingTicker)
            {
                __result = false;
                return false;
            }
            ticker.SetValue(t.swingTicker);

            if (t.UpgradeLevel < spawnDef.LargeMinableRequiredToolTier)
            {
                if (t is Axe)
                {
                    __instance.Location.playSound("axe", tileLocation);
                }
                else
                {
                    __instance.Location.playSound("clubhit", tileLocation);
                    __instance.Location.playSound("clank", tileLocation);
                }
                Game1.drawObjectDialogue(I18n.ResourceClump_BadToolTier());
                Game1.player.jitterStrength = 1;
                __result = false;
                return false;
            }

            if (t is Axe)
            {
                __instance.Location.playSound("axechop", tileLocation);
            }
            else
            {
                __instance.Location.playSound("hammer", tileLocation);
            }

            float power = Math.Max(1f, (float)((int)t.upgradeLevel.Value + 1) * 0.75f);
            __instance.health.Value -= power;

            if (t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(power / 12f) &&
                spawnDef.LargeMinableShavingDrop.Count > 0)
            {
                var itemSpawns = spawnDef.LargeMinableShavingDrop.ToList();
                itemSpawns.RemoveAll(w => w.Value.Condition != null && !w.Value.Condition.Equals("true", StringComparison.InvariantCultureIgnoreCase) && !GameStateQuery.CheckConditions(w.Value.Condition, location: __instance.Location, player: t.getLastFarmerToUse(), random: Game1.random));
                GenericSpawnItemDataWithCondition itemSpawn = itemSpawns.Choose(Game1.random);
                if (itemSpawn != null)
                {
                    Item item = ItemQueryResolver.TryResolveRandomItem(itemSpawn, new ItemQueryContext(__instance.Location, t.getLastFarmerToUse(), Game1.random, "SpaceCore Spawnable Large Minable shaving enchantment drops"));

                    Debris d = new Debris(item, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
                    d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
                    d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
                    __instance.Location.debris.Add(d);
                }
            }

            // radial debris

            if (__instance.health.Value <= 0)
            {
                __result = __instance.destroy(t, __instance.Location, tileLocation);
                return false;
            }

            __instance.shakeTimer = 100;
            __instance.NeedsUpdate = true;

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(ResourceClump), nameof(ResourceClump.destroy))]
    public static class ResourceClumpDestroyForSpawnablePatch
    {
        public static bool Prefix(ResourceClump __instance, Tool t, GameLocation location, Vector2 tileLocation, ref bool __result)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/LargeMinable", out string spawnId))
                return true;

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];

            if (t == null)
            {
                __result = false;
                return false;
            }

            Random r;
            if (Game1.IsMultiplayer)
            {
                Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
                r = Game1.recentMultiplayerRandom;
            }
            else
            {
                r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
            }

            foreach (var drop in spawnDef.LargeMinableDrops.SimplifyDrops(__instance.Location, t.getLastFarmerToUse(), r))
                Game1.createItemDebris(drop, new Vector2(tileLocation.X + __instance.width.Value / 2f, tileLocation.Y + __instance.height.Value / 2f) * Game1.tileSize, t.getLastFarmerToUse().FacingDirection, location);

            if (spawnDef.LargeMinableExperienceGranted > 0)
            {
                t.getLastFarmerToUse().gainExperience(spawnDef.LargeMinableTool == SpawnableDefinitionData.MinableType.Pickaxe ? Farmer.miningSkill : Farmer.foragingSkill, spawnDef.LargeMinableExperienceGranted);
            }

            if (t is Axe)
            {
                __instance.Location.playSound("stumpCrack", tileLocation);
            }
            else
            {
                __instance.Location.playSound("boulderBreak", tileLocation);
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(BreakableContainer), nameof(BreakableContainer.releaseContents))]
    public static class BreakableContainerContentsForSpawnablePatch
    {
        public static bool Prefix(BreakableContainer __instance, Farmer who)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/Breakable", out string spawnId))
                return true;

            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];

            if (__instance.Location == null)
                return false;

            Random r = Game1.random;
            int x = (int)__instance.tileLocation.X;
            int y = (int)__instance.tileLocation.Y;

            foreach (var drop in spawnDef.BreakableDrops.SimplifyDrops(__instance.Location, who, r))
                Game1.createItemDebris(drop, new Vector2(x + 0.5f, y + 0.5f) * Game1.tileSize, who.FacingDirection, __instance.Location);

            return false;
        }
    }

    [HarmonyPatch(typeof(Chest), nameof(Chest.draw), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)])]
    public static class ChestDrawCorrectlyForSpawnablePatch
    {
        public static bool Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.ContainsKey("spacechase0.SpaceCore/SpawnableDrawing"))
                return true;

            int currentLidFrame = SpaceCore.Instance.Helper.Reflection.GetField<int>(__instance, "currentLidFrame").GetValue();

            float draw_x = x;
            float draw_y = y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }
            float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
            if (__instance.localKickStartTile.HasValue)
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
                draw_y -= (float)Math.Sin((double)__instance.kickProgress * Math.PI) * 0.5f;
            }

            var data = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);

            Texture2D sprite_sheet = data.GetTexture();
            int y_offset = -64;

            spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset)), data.GetSourceRect(), __instance.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
            Vector2 lidPosition = new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset);
            
            spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), data.GetSourceRect( 0, currentLidFrame), __instance.tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);

            return false;
        }
    }

    [HarmonyPatch(typeof(Furniture), nameof(Furniture.canBeRemoved))]
    public static class FurnitureCanRemoveSpawnablePatch
    {
        public static void Postfix(Furniture __instance, ref bool __result)
        {
            if (__instance.modData.ContainsKey("spacechase0.SpaceCore/DisableRemoval"))
                __result = false;
        }
    }

    // reloadSprite isn't used anymore by us anyways
    /*
    [HarmonyPatch]
    public static class MonsterSpriteOverridesPatch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var subclasses = from asm in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.Contains("Steamworks.NET") && !a.IsDynamic)
                             from type in asm.GetExportedTypes()
                             where type.IsSubclassOf(typeof(Monster))
                             select type;

            var ps = new Type[] { typeof(bool) };

            yield return AccessTools.Method(typeof(Monster), nameof(Monster.reloadSprite));
            foreach (var subclass in subclasses)
            {
                var meth = subclass.GetMethod(nameof(Monster.reloadSprite), ps);
                if (meth != null && meth.DeclaringType == subclass)
                    yield return meth;
            }
        }

        public static void Postfix(Monster __instance)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/Spawnable", out string spawnId))
                return;
            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];
            if (spawnDef.MonsterTextureOverride == null)
                return;

            bool old = __instance.Sprite.ignoreSourceRectUpdates;
            __instance.Sprite.ignoreSourceRectUpdates = true;
            __instance.Sprite.LoadTexture(spawnDef.MonsterTextureOverride);
            __instance.Sprite.ignoreSourceRectUpdates = old;
        }
    }
    */


    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.monsterDrop))]
    public static class GameLocationMonsterDropOverrideForSpawnablesPatch
    {
        public static bool Prefix(GameLocation __instance, Monster monster, int x, int y, Farmer who)
        {
            if (!__instance.modData.TryGetValue("spacechase0.SpaceCore/Spawnable", out string spawnId))
                return true;
            var spawnDefs = Game1.content.Load<Dictionary<string, SpawnableDefinitionData>>("spacechase0.SpaceCore/SpawnableDefinitions");
            var spawnDef = spawnDefs[spawnId];
            if (spawnDef.MonsterDropOverride == null)
                return true;

            var drops = spawnDef.MonsterDropOverride.SimplifyDrops(__instance, who, Game1.random);
            foreach (var drop in drops)
            {
                __instance.debris.Add(monster.ModifyMonsterLoot(new Debris(drop, new Vector2(x, y), who.StandingPixel.ToVector2())));
            }

            Trinket.TrySpawnTrinket(__instance, monster, monster.getStandingPosition());

            if (__instance.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.033)
            {
                StardewValley.Object o = __instance.tryToCreateUnseenSecretNote(who);
                if (o != null)
                {
                    monster.ModifyMonsterLoot(Game1.createItemDebris(o, new Vector2(x, y), -1, __instance));
                }
            }
            Utility.trySpawnRareObject(who, new Vector2(x, y), __instance, 1.5);
            if (Utility.tryRollMysteryBox(0.01 + who.team.AverageDailyLuck() / 10.0 + (double)who.LuckLevel * 0.008))
            {
                monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create((who.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(x, y), -1, __instance));
            }
            if (who.stats.MonstersKilled > 10 && Game1.random.NextDouble() < 0.0001 + ((!who.mailReceived.Contains("voidBookDropped")) ? ((double)who.stats.MonstersKilled * 1.5E-05) : 0.0004))
            {
                monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)Book_Void"), new Vector2(x, y), -1, __instance));
                who.mailReceived.Add("voidBookDropped");
            }
            if (Game1.netWorldState.Value.GoldenWalnutsFound >= 100)
            {
                if ((bool)monster.isHardModeMonster.Value && Game1.stats.Get("hardModeMonstersKilled") > 50 && Game1.random.NextDouble() < 0.001 + (double)((float)who.LuckLevel * 0.0002f))
                {
                    monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)896"), new Vector2(x, y), -1, __instance));
                }
                else if ((bool)monster.isHardModeMonster.Value && Game1.random.NextDouble() < 0.008 + (double)((float)who.LuckLevel * 0.002f))
                {
                    monster.ModifyMonsterLoot(Game1.createItemDebris(ItemRegistry.Create("(O)858"), new Vector2(x, y), -1, __instance));
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1.createItemDebris))]
    public static class GameCreateDebrisPreventionPatch
    {
        public static string prevent = null;
        public static bool Prefix(Item item)
        {
            if (item == null) return true; // The original method will error in this case anyways, but at least people won't blame SpaceCore for it then (since it's another mod causing it)
            if (item.QualifiedItemId == prevent)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(FruitTree), nameof(FruitTree.performToolAction))]
    public static class FruitTreePerformToolActionPreventSaplingForSpawnablePatch
    {
        public static void Prefix(FruitTree __instance)
        {
            if (__instance.modData.ContainsKey("spacechase0.SpaceCore/PreventSaplingDrop"))
                GameCreateDebrisPreventionPatch.prevent = $"(O){__instance.treeId.Value}";
        }

        public static void Postfix()
        {
            GameCreateDebrisPreventionPatch.prevent = null;
        }
    }
}
