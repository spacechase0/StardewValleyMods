using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;
using xTile.Display;

namespace TerraformingHoe
{
    public class Mod : StardewModdingAPI.Mod, IAssetEditor, IAssetLoader
    {
        public static Mod instance;
        public static Configuration Config;

        private static PerScreen<ScreenState> _state = new(() => new ScreenState());
        public static ScreenState State => _state.Value;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            I18n.Init(Helper.Translation);

            Config = Helper.ReadConfig<Configuration>();

            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Player.Warped += OnPlayerWarped;
            Helper.Events.Display.RenderedHud += OnRenderedHud;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Strings\\EnchantmentNames"))
                return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Strings\\EnchantmentNames"))
            {
                var data = asset.AsDictionary<string, string>().Data;
                data.Add("Terraform", I18n.Enchantment_Terraform_Name());
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps//grass-back-8x8"))
                return true;
            if (asset.AssetNameEquals("Maps//water-back-8x8"))
                return true;
            if (asset.AssetNameEquals("Maps//water-buildings-8x8"))
                return true;
            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Maps//grass-back-8x8"))
                return (T)(object)Helper.Content.Load<Texture2D>("assets/grass.png");
            if (asset.AssetNameEquals("Maps//water-back-8x8"))
                return (T)(object)Helper.Content.Load<Texture2D>("assets/water-back.png");
            if (asset.AssetNameEquals("Maps//water-buildings-8x8"))
                return (T)(object)Helper.Content.Load<Texture2D>("assets/water-buildings.png");

            return default(T);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spacecore = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spacecore.RegisterSerializerType(typeof(TerraformEnchantment));

            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm != null)
            {
                gmcm.Register(ModManifest, () => Config = new Configuration(), () => Helper.WriteConfig(Config));
                gmcm.AddKeybindList(ModManifest, () => Config.HoeModeKey, (v) => Config.HoeModeKey = v, () => I18n.Config_HoeModeKey_Name(), () => I18n.Config_HoeModeKey_Description());
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Config.HoeModeKey.JustPressed())
            {
                State.Mode = (HoeMode)(((int)State.Mode + 1) % Enum.GetValues<HoeMode>().Length);
                Log.Trace("next hoe mode: " + State.Mode);
                // TODO render this
            }
            if (State.LocationDirty)
            {
                UpdateLocationLayer();
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            State.LocationDirty = true;
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player.CurrentTool is not Hoe hoe || !hoe.hasEnchantmentOfType< TerraformEnchantment >())
                return;

            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().First();
            if (Helper.Reflection.GetField<Item>(toolbar, "hoverItem").GetValue() != null)
                return;

            int y = 0;
            int toolbarY = Helper.Reflection.GetField<int>(toolbar, "yPositionOnScreen").GetValue(); // the public value isn't the right one - Toolbar also has a `private new` one

            // I don't know why the following values work, but they do
            if (toolbarY < Game1.uiViewport.Height / 2)
                y = toolbarY + 0;
            else
                y = toolbarY - 128 - 32;

            int x = Game1.uiViewport.Width / 2 - 384 + Game1.player.CurrentToolIndex * 64 + 32;

            SpriteText.drawStringHorizontallyCenteredAt(e.SpriteBatch, State.Mode.ToString(), x, y);
        }

        private void UpdateLocationLayer()
        {
            State.LocationDirty = false;

            // https://gamedev.stackexchange.com/a/177562
            const int edgesA = 1 + 8 + 128;
            const int edgesB = 1 + 2 + 16;
            const int edgesC = 4 + 8 + 64;
            const int edgesD = 4 + 2 + 32;
            Dictionary<int, int> mapA = new() { [0] = 8, [128] = 8, [1] = 16, [8] = 10, [9] = 2, [137] = 18, [136] = 10, [129] = 16 };
            Dictionary<int, int> mapB = new() { [0] = 11, [16] = 11, [1] = 19, [2] =  9, [3] = 3, [19] = 17, [18] = 9, [17] = 19 };
            Dictionary<int, int> mapC = new() { [0] = 20, [64] = 20, [4] = 12, [8] = 22, [12] = 6, [76] = 14, [72] = 22, [68] = 12 };
            Dictionary<int, int> mapD = new() { [0] = 23, [32] = 23, [4] = 15, [2] = 21, [6] = 7, [38] = 13, [34] = 21, [36] = 15 };

            var loc = Game1.currentLocation;

            xTile.Layers.Layer back = loc.Map.GetLayer("Back");
            xTile.Layers.Layer origBack = loc.Map.GetLayer("OrigBack");
            if (origBack == null)
            {
                origBack = new("OrigBack", loc.Map, back.LayerSize, back.TileSize);
                for (int ix = 0; ix < back.LayerWidth; ++ix )
                {
                    for (int iy = 0; iy < back.LayerHeight; ++iy)
                    {
                        origBack.Tiles[ix, iy] = back.Tiles[ix, iy]?.Clone( origBack );
                    }
                }
                loc.Map.AddLayer(origBack);
            }
            xTile.Layers.Layer buildings = loc.Map.GetLayer("Buildings");
            xTile.Layers.Layer origBuildings = loc.Map.GetLayer("OrigBuildings");
            if (origBuildings == null)
            {
                origBuildings = new("OrigBuildings", loc.Map, buildings.LayerSize, buildings.TileSize);
                for (int ix = 0; ix < buildings.LayerWidth; ++ix )
                {
                    for (int iy = 0; iy < buildings.LayerHeight; ++iy)
                    {
                        origBuildings.Tiles[ix, iy] = buildings.Tiles[ix, iy]?.Clone(origBuildings);
                    }
                }
                loc.Map.AddLayer(origBuildings);
            }

            xTile.Layers.Layer backLayer = loc.Map.GetLayer("Back9");
            xTile.Layers.Layer buildingsLayer = loc.Map.GetLayer("Buildings9");
            if (backLayer == null)
            {
                backLayer = new("Back9", loc.Map, new(loc.Map.Layers[0].LayerWidth * 2, loc.Map.Layers[0].LayerHeight * 2), loc.map.Layers[0].TileSize);
                loc.Map.AddLayer(backLayer);
            }
            if (buildingsLayer == null)
            {
                buildingsLayer = new("Buildings9", loc.Map, new(loc.Map.Layers[0].LayerWidth * 2, loc.Map.Layers[0].LayerHeight * 2), loc.map.Layers[0].TileSize);
                loc.Map.AddLayer(buildingsLayer);
            }

            bool added = false;
            xTile.Tiles.TileSheet getTs( string id, string path )
            {
                if (loc.map.GetTileSheet( id ) == null)
                {
                    xTile.Tiles.TileSheet ts = new(loc.Map, path, new(2, 3), new(16, 16));
                    ts.Id = id;
                    loc.map.AddTileSheet(ts);
                    added = true;

                    if (id == "zz_tf_dummy")
                    {
                        ts.TileIndexProperties[1].Add("Type", new("Grass"));
                        ts.TileIndexProperties[2].Add("Water", new("T"));
                    }

                    return ts;
                }
                return loc.map.GetTileSheet(id);
            };

            var grassTs = getTs("zz_tf_grass", "Maps/grass-back-8x8");
            var waterBackTs = getTs("zz_tf_waterback", "Maps/water-back-8x8");
            var waterBuildingsTs = getTs("zz_tf_waterbuildings", "Maps/water-buildings-8x8");
            var dummyTs = getTs("zz_tf_dummy", Helper.Content.GetActualAssetKey("assets/dummy.png"));

            if (added)
                loc.map.LoadTileSheets(Game1.mapDisplayDevice);

            var tiles = loc.get_tileOverrides();
            for (int ix = 0; ix < loc.map.Layers[0].LayerWidth; ++ix)
            {
                for (int iy = 0; iy < loc.map.Layers[0].LayerHeight; ++iy)
                {
                    var tcurr = new Vector2(ix, iy);
                    Func<Vector2, TileOverride?> getTile = ( tkey ) => tiles.ContainsKey(tkey) ? (TileOverride)tiles[tkey] : null;

                    var currTile = getTile(tcurr);
                    if (!currTile.HasValue)
                    {
                        back.Tiles[ix, iy] = origBack.Tiles[ix, iy]?.Clone(back);
                        backLayer.Tiles[ix*2+0, iy*2+0] = null;
                        backLayer.Tiles[ix*2+1, iy*2+0] = null;
                        backLayer.Tiles[ix*2+0, iy*2+1] = null;
                        backLayer.Tiles[ix*2+1, iy*2+1] = null;
                        buildings.Tiles[ix, iy] = origBuildings.Tiles[ix, iy]?.Clone(buildings);
                        buildingsLayer.Tiles[ix * 2 + 0, iy * 2 + 0] = null;
                        buildingsLayer.Tiles[ix * 2 + 1, iy * 2 + 0] = null;
                        buildingsLayer.Tiles[ix * 2 + 0, iy * 2 + 1] = null;
                        buildingsLayer.Tiles[ix * 2 + 1, iy * 2 + 1] = null;

                        string water_property = loc.doesTileHaveProperty(ix, iy, "Water", "Back");
                        if (water_property != null)
                        {
                            if (water_property == "I")
                                loc.waterTiles.waterTiles[ix, iy] = new(is_water: true, is_visible: false);
                            else
                                loc.waterTiles[ix, iy] = true;
                        }
                        else
                            loc.waterTiles[ix, iy] = false;

                        continue;
                    }

                    void doTile( TileOverride to, xTile.Tiles.TileSheet ts, xTile.Layers.Layer l, bool doWater = false )
                    {
                        int tl = 0, tr = 0, bl = 0, br = 0;
                        int n = 0;
                        if (getTile(tcurr + new Vector2(+0, -1)) == to) n += 1;
                        if (getTile(tcurr + new Vector2(+1, +0)) == to) n += 2;
                        if (getTile(tcurr + new Vector2(+0, +1)) == to) n += 4;
                        if (getTile(tcurr + new Vector2(-1, +0)) == to) n += 8;
                        if (getTile(tcurr + new Vector2(+1, -1)) == to) n += 16;
                        if (getTile(tcurr + new Vector2(+1, +1)) == to) n += 32;
                        if (getTile(tcurr + new Vector2(-1, +1)) == to) n += 64;
                        if (getTile(tcurr + new Vector2(-1, -1)) == to) n += 128;

                        if (n == 0)
                        {
                            tl = 0;
                            tr = 1;
                            bl = 4;
                            br = 5;
                        }
                        else
                        {
                            tl = mapA[n & edgesA];
                            tr = mapB[n & edgesB];
                            bl = mapC[n & edgesC];
                            br = mapD[n & edgesD];
                        }

                        l.Tiles[ix * 2 + 0, iy * 2 + 0] = new xTile.Tiles.StaticTile(l, ts, xTile.Tiles.BlendMode.Alpha, tl);
                        l.Tiles[ix * 2 + 1, iy * 2 + 0] = new xTile.Tiles.StaticTile(l, ts, xTile.Tiles.BlendMode.Alpha, tr);
                        l.Tiles[ix * 2 + 0, iy * 2 + 1] = new xTile.Tiles.StaticTile(l, ts, xTile.Tiles.BlendMode.Alpha, bl);
                        l.Tiles[ix * 2 + 1, iy * 2 + 1] = new xTile.Tiles.StaticTile(l, ts, xTile.Tiles.BlendMode.Alpha, br);

                        if (doWater)
                        {
                            if (n == 255)
                            {
                                back.Tiles[ix, iy] = new xTile.Tiles.StaticTile(back, dummyTs, xTile.Tiles.BlendMode.Alpha, 2);
                                buildings.Tiles[ix, iy] = null;
                            }
                            else
                                buildings.Tiles[ix, iy] = new xTile.Tiles.StaticTile(buildings, dummyTs, xTile.Tiles.BlendMode.Alpha, 0);
                        }
                    }

                    if (currTile == TileOverride.Water)
                    {
                        loc.waterTiles[ix, iy] = true;
                        doTile(TileOverride.Water, waterBackTs, backLayer, doWater: true);
                        doTile(TileOverride.Water, waterBuildingsTs, buildingsLayer);
                    }
                    else if (currTile == TileOverride.Grass)
                    {
                        doTile(TileOverride.Grass, grassTs, backLayer);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(BaseEnchantment), nameof(BaseEnchantment.GetAvailableEnchantments))]
    public static class BaseEnchantmentEnchantmentListPatch
    {
        public static void Postfix(ref List<BaseEnchantment> __result)
        {
            __result.Add(new TerraformEnchantment());
        }
    }

    [HarmonyPatch(typeof(Hoe), nameof(Hoe.DoFunction))]
    public static class HoeOverrideFunctionPatch
    {
        public static bool Prefix(Hoe __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            if ( !__instance.hasEnchantmentOfType< TerraformEnchantment >() || Mod.State.Mode == HoeMode.Normal )
                return true;

            Impl(__instance, location, x, y, power, who);
            return false;
        }

        private static void Impl(Hoe hoe, GameLocation location, int x, int y, int power, Farmer who)
        {
            // from base.DoFunction
            Mod.instance.Helper.Reflection.GetField<Farmer>(hoe, "lastUser").SetValue(who);
            Game1.recentMultiplayerRandom = new Random((short)Game1.random.Next(-32768, 32768));

            power = who.toolPower;
            who.stopJittering();
            Vector2 initialTile = new Vector2(x / 64, y / 64);
            List<Vector2> tiles = Mod.instance.Helper.Reflection.GetMethod( hoe, "tilesAffected" ).Invoke<List<Vector2>>(initialTile, power, who);

            var overrides = location.get_tileOverrides();

            foreach (var tile in tiles)
            {
                if (!overrides.ContainsKey( tile ) && !location.isTileLocationTotallyClearAndPlaceable(tile))
                    continue;

                switch (Mod.State.Mode)
                {
                    case HoeMode.Normal:
                        Log.Warn("This should never happen");
                        break;
                    case HoeMode.Water:
                        DoWater(location, tile);
                        break;
                    case HoeMode.Grass:
                        DoGrass(location, tile);
                        break;
                }
            }

            Mod.State.LocationDirty = true;
        }

        private static void DoWater(GameLocation loc, Vector2 tile)
        {
            var overrides = loc.get_tileOverrides();
            TileOverride? currTile = overrides.ContainsKey(tile) ? (TileOverride)overrides[tile] : null;

            if (currTile == TileOverride.Water)
                overrides.Remove(tile);
            else
                overrides.Add(tile, (int)TileOverride.Water);
        }

        private static void DoGrass(GameLocation loc, Vector2 tile)
        {
            var overrides = loc.get_tileOverrides();
            TileOverride? currTile = overrides.ContainsKey(tile) ? (TileOverride)overrides[tile] : null;

            if (currTile == TileOverride.Grass)
                overrides.Remove(tile);
            else
                overrides.Add(tile, (int)TileOverride.Grass);
        }
    }

    [HarmonyPatch(typeof(xTile.Layers.Layer), "DrawNormal")]
    public static class LayerDrawMiniLayerPatch
    {
        public static bool Prefix(xTile.Layers.Layer __instance,
                                  IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, xTile.Dimensions.Location displayOffset, int pixelZoom,
                                  xTile.Dimensions.Size ___m_layerSize, ref int[,] ____skipMap, HashSet<int> ____dirtyRows, xTile.Tiles.Tile[,] ___m_tiles)
        {
            //Log.Debug("Draw " + __instance.Id);
            if (__instance.Id == "Back9" || __instance.Id == "Buildings9")
            {
                Impl(__instance, displayDevice, mapViewport, displayOffset, pixelZoom, ___m_layerSize, ref ____skipMap, ____dirtyRows, ___m_tiles);
                return false;
            }
            return true;
        }
        private static int Wrap(int value, int span)
        {
            value %= span;
            if (value < 0)
            {
                value += span;
            }
            return value;
        }

        private static void Impl(xTile.Layers.Layer __instance,
                                 IDisplayDevice displayDevice, xTile.Dimensions.Rectangle mapViewport, xTile.Dimensions.Location displayOffset, int pixelZoom,
                                 xTile.Dimensions.Size ___m_layerSize, ref int[,] ____skipMap, HashSet<int> ____dirtyRows, xTile.Tiles.Tile[,] ___m_tiles)
        {/*
            if (__instance.BeforeDraw != null)
            {
                __instance.BeforeDraw(__instance, new LayerEventArgs(__instance, mapViewport));
            }
            */
            int tileWidth = pixelZoom * 16 / 2; // Mine: /2
            int tileHeight = pixelZoom * 16 / 2; // Mine: /2
            xTile.Dimensions.Location tileInternalOffset = new xTile.Dimensions.Location(/*__instance.*/Wrap(mapViewport.X, tileWidth), /*__instance.*/Wrap(mapViewport.Y, tileHeight));
            int tileXMin = ((mapViewport.X >= 0) ? (mapViewport.X / tileWidth) : ((mapViewport.X - tileWidth + 1) / tileWidth));
            int tileYMin = ((mapViewport.Y >= 0) ? (mapViewport.Y / tileHeight) : ((mapViewport.Y - tileHeight + 1) / tileHeight));
            if (tileXMin < 0)
            {
                displayOffset.X -= tileXMin * tileWidth;
                tileXMin = 0;
            }
            if (tileYMin < 0)
            {
                displayOffset.Y -= tileYMin * tileHeight;
                tileYMin = 0;
            }
            int tileColumns = 1 + (mapViewport.Size.Width - 1) / tileWidth;
            int tileRows = 1 + (mapViewport.Size.Height - 1) / tileHeight;
            if (tileInternalOffset.X != 0)
            {
                tileColumns++;
            }
            if (tileInternalOffset.Y != 0)
            {
                tileRows++;
            }
            int tileXMax = Math.Min(tileXMin + tileColumns, ___m_layerSize.Width);
            int tileYMax = Math.Min(tileYMin + tileRows, ___m_layerSize.Height);
            xTile.Dimensions.Location tileLocation = displayOffset - tileInternalOffset;
            int offset = (__instance.Id.Equals("Front") ? (16 * pixelZoom) : 0);
            if (____skipMap == null)
            {
                ____dirtyRows.Clear();
                ____skipMap = new int[__instance.LayerWidth, __instance.LayerHeight];
                for (int y2 = 0; y2 < __instance.LayerHeight; y2++)
                {
                    //__instance._RebakeRow(y2);
                    Mod.instance.Helper.Reflection.GetMethod(__instance, "_RebakeRow").Invoke(y2);
                }
            }
            else
            {
                foreach (int y in ____dirtyRows)
                {
                    //__instance._RebakeRow(y);
                    Mod.instance.Helper.Reflection.GetMethod(__instance, "_RebakeRow").Invoke(y);
                }
                ____dirtyRows.Clear();
            }
            for (int tileY = tileYMin; tileY < tileYMax; tileY++)
            {
                tileLocation.X = displayOffset.X - tileInternalOffset.X;
                int skip_amount;
                for (int tileX = tileXMin; tileX < tileXMax; tileX += skip_amount)
                {
                    skip_amount = 1;
                    skip_amount = ____skipMap[tileX, tileY];
                    xTile.Tiles.Tile tile = ___m_tiles[tileX, tileY];
                    if (tile != null)
                    {
                        displayDevice.DrawTile(tile, tileLocation, (float)(tileY * (16/2 * pixelZoom) + 16/2 * pixelZoom + offset) / 10000f);
                    }
                    tileLocation.X += tileWidth * skip_amount;
                    if (skip_amount == -1)
                    {
                        break;
                    }
                }
                tileLocation.Y += tileHeight;
            }
            /*
            if (__instance.AfterDraw != null)
            {
                __instance.AfterDraw(__instance, new LayerEventArgs(__instance, mapViewport));
            }
            */
        }
    }


    [HarmonyPatch(typeof(xTile.Tiles.TileSheet), nameof(xTile.Tiles.TileSheet.GetTileImageBounds))]
    public static class TileSheetMiniBoundsPatch
    {
        public static bool Prefix(xTile.Tiles.TileSheet __instance, int tileIndex, ref xTile.Dimensions.Rectangle __result)
        {
            if (__instance.ImageSource.Contains("8x8"))
            {
                __result = Impl(__instance, tileIndex);
                return false;
            }
            return true;
        }

        private static xTile.Dimensions.Rectangle Impl(xTile.Tiles.TileSheet __instance, int tileIndex)
        {
            return new xTile.Dimensions.Rectangle(8 * (tileIndex % (__instance.SheetSize.Width * 2)), 8 * (tileIndex / (__instance.SheetSize.Width * 2)), 8, 8);
        }
    }
}
