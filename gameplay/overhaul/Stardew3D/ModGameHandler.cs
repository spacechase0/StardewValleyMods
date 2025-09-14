using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SpaceShared;
using Stardew3D.Data;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Util;
using xTile;
using xTile.Tiles;
using static Stardew3D.IGameHandler;

namespace Stardew3D;

public abstract class ModGameHandler : IGameHandler
{
    public abstract string Id { get; }
    public abstract string[] Tags { get; }

    public abstract ICamera Camera { get; }

    public abstract Matrix ProjectionMatrix { get; protected set; }

    public virtual void SwitchOn() { }
    public virtual void SwitchOff() { }

    private Dictionary<Type, (Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool allowsSubclasses)> menuHandlers = new();
    private Dictionary<Type, List<(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool allowsSubclasses)>> menuHandlerAddons = new();
    public void SetMenuHandler<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true)
    {
        if (!typeof(MenuType).IsAssignableTo(typeof(IClickableMenu)))
            throw new ArgumentException("Must inherit from StardewValley.Menus.IClickableMenu", nameof(MenuType));

        menuHandlers[typeof(MenuType)] = new(createHandlerFunc, includeMenuSubclasses);
    }

    public void AddMenuHandlerAddon<MenuType>(Func<IClickableMenu, IMenuHandler> createHandlerFunc, bool includeMenuSubclasses = true)
    {
        if (!typeof(MenuType).IsAssignableTo(typeof(IClickableMenu)))
            throw new ArgumentException("Must inherit from StardewValley.Menus.IClickableMenu", nameof(MenuType));

        menuHandlerAddons.TryAdd(typeof(MenuType), new());
        menuHandlerAddons[typeof(MenuType)].Add(new(createHandlerFunc, includeMenuSubclasses));
    }

    public IMenuHandler[] CreateApplicableMenuHandlers(IClickableMenu menu)
    {
        List<IMenuHandler> ret = new();
        bool didPrimary = false;
        for (Type check = menu.GetType(); check != typeof(IClickableMenu).BaseType; check = check.BaseType)
        {
            if (!didPrimary)
            {
                if (menuHandlers.TryGetValue(check, out var handlerData))
                {
                    if (handlerData.allowsSubclasses || check == menu.GetType())
                    {
                        didPrimary = true;
                        ret.Insert(0, handlerData.createHandlerFunc(menu)); // Insert, not add, so that even if some addons get added first, the main handler goes first
                    }
                }
            }

            if (menuHandlerAddons.TryGetValue(check, out var addonDataList))
            {
                foreach (var addonData in addonDataList)
                {
                    if (addonData.allowsSubclasses || check == menu.GetType())
                    {
                        ret.Insert(didPrimary ? 1 : 0, addonData.createHandlerFunc(menu)); // Insert, not add, so that parent class addons come first
                    }
                }
            }
        }
        return ret.ToArray();
    }

    public abstract void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling);

    protected abstract void DoCamera();

    public virtual void BeforeUpdate() { }
    public virtual void AfterUpdate() { }

    private IClickableMenu lastClickableMenu = null;

    public virtual bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender)
    {
        if (Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget != targetScreen)
            Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);

        if (step >= RenderSteps.MenuBackground && step < RenderSteps.GlobalFade)
        {
            bool didRenderOnce = false;
            void forceMenuRenderIfNotAlreadyRun(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
            {
                if (didRenderOnce)
                    return;

                defaultRender(step, sb, time, targetScreen);
                didRenderOnce = true;
            }

            if (Game1.activeClickableMenu != null)
            {
                var currentMenuHandlers = Mod.State.GetMenuHandlersFor(Game1.activeClickableMenu);
                foreach (var handler in currentMenuHandlers)
                {
                    handler.RenderMenu(step, sb, time, targetScreen, forceMenuRenderIfNotAlreadyRun);
                }
                return currentMenuHandlers.Length == 0 ? true : didRenderOnce;
            }
            return true;
        }

        if (step != RenderSteps.World)
            return true;

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;

        var models = Game1.content.Load<Dictionary<string, ModelData>>("kittycatcasey.Stardew3D/Models");

        DoCamera();

        Game1.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
        Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        if ( Mod.Config.RenderDebugDraw )
            RenderHelper.DebugRender(Camera);
        if (Mod.Config.RenderDebugGrid)
            RenderHelper.DebugRenderGrid();

        if (models.ContainsKey(Game1.currentLocation.Name))
        {
#if false
            Game1.graphics.GraphicsDevice.RasterizerState = rasterStateCull;
            Game1.graphics.GraphicsDevice.SamplerStates[0] = samplerState;

            var m = models[Game1.currentLocation.Name];
            var inst = m.Model.Instance.CreateInstance();
            basicEffect.World = Matrix.CreateScale(m.Scale) *
                                (Matrix.CreateRotationX(m.Rotation.X) * Matrix.CreateRotationY(m.Rotation.Y) * Matrix.CreateRotationZ(m.Rotation.Z)) *
                                Matrix.CreateTranslation(m.Translation);
            // Can't find SetAnimationFrame method in Controller... hmm...
            inst.Draw(projectionMatrix, basicEffect.View, worldMatrixOrigin);
#endif
        }

        // TODO: Cache
        Dictionary<string, (Texture2D tex, List<SimpleVertex> verts)> locationVertices = new();
        List<xTile.Layers.Layer> applicableLayers = new();
        List<xTile.Layers.Layer> ceilingLayers = new();
        applicableLayers.AddRange(Game1.currentLocation.backgroundLayers.Select(kvp => kvp.Key));
        applicableLayers.AddRange(Game1.currentLocation.buildingLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(Game1.currentLocation.frontLayers.Select(kvp => kvp.Key));
        //applicableLayers.AddRange(Game1.currentLocation.alwaysFrontLayers.Select(kvp => kvp.Key));
        ceilingLayers.AddRange(Game1.currentLocation.Map.Layers.Where( l => l.Id == "kittycatcasey.Stardew3D/Ceiling" || l.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_")));
        ceilingLayers.Sort((l1, l2) => (l1.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l1.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0) -
                                       (l2.Id.StartsWith("kittycatcasey.Stardew3D/Ceiling_") ? int.Parse(l2.Id.Substring("kittycatcasey.Stardew3D/Ceiling_".Length)) : 0));
        applicableLayers.AddRange(ceilingLayers);
        for (int ix = 0; ix < Game1.currentLocation.Map.Layers[0].LayerSize.Width; ++ix)
        {
            for (int iy = 0; iy < Game1.currentLocation.Map.Layers[0].LayerSize.Height; ++iy)
            {
                const float tuck = 0.00001f;
                foreach (var layer in applicableLayers)
                {
                    bool isCeiling = ceilingLayers.Contains(layer);

                    var tile = layer.Tiles[ix, iy];
                    if (tile == null)
                        continue;

                    if (tile is AnimatedTile animTile)
                    {
                        // TODO: handle elsewhere
                        continue;
                    }

                    string ts = PathUtilities.NormalizeAssetName(tile.TileSheet.ImageSource);
                    if (!locationVertices.TryGetValue(ts, out var verts))
                        locationVertices.Add(ts, verts = new(Game1.content.Load<Texture2D>(ts), new()));

                    int tr = tile.TileSheet.SheetWidth;
                    float tw = verts.tex.ActualWidth;
                    float twIncr = Game1.smallestTileSize / tw;
                    float th = verts.tex.ActualHeight;
                    float thIncr = Game1.smallestTileSize / th;

                    var tilePos = Extensions.GetPositionForTile(Game1.currentLocation.Map, new(ix, iy), isCeiling);
                    if (float.IsNaN(tilePos.Position.Y))
                        continue;

                    int layerNum = applicableLayers.IndexOf(layer);
                    RenderHelper.GenerateQuad(verts.verts, tilePos.Position + new Vector3(0, 0.001f * layerNum, 0), tilePos.QuadSize, tile.TileIndex % tr * twIncr + tuck, tile.TileIndex / tr * thIncr + tuck, twIncr - tuck * 2, thIncr - tuck * 2, tilePos.QuadFacingNormal, upOverride: tilePos.QuadUpNormal);
                }

                var floorWalls = Game1.content.Load<Dictionary<string, FloorWallAssociationData>>($"{Mod.Instance.ModManifest.UniqueID}/FloorWallAssociations");
                var wallDefs = Game1.content.Load<Dictionary<string, WallDefinitionData>>($"{Mod.Instance.ModManifest.UniqueID}/WallDefinitions");
                WallDefinitionData wallDef = null;
                if (floorWalls.TryGetValue($"{PathUtilities.NormalizeAssetName(Game1.currentLocation.Map.GetTileSheet(Game1.currentLocation.getTileSheetIDAt(ix, iy, "Back"))?.ImageSource)}:{Game1.currentLocation.getTileIndexAt(new Point(ix, iy), "Back")}", out var assocData) &&
                    wallDefs.TryGetValue(assocData.WallDefinitionId, out wallDef))
                {
                    Vector3 floorWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.West, forCeiling: false);
                    Vector3 floorNorth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.North, forCeiling: false);
                    Vector3 floorEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.East, forCeiling: false);
                    Vector3 floorSouth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.South, forCeiling: false);
                    Vector3 otherFloorWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.East, forCeiling: false);
                    Vector3 otherFloorNorth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.South, forCeiling: false);
                    Vector3 otherFloorEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.West, forCeiling: false);
                    Vector3 otherFloorSouth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.North, forCeiling: false);
                    float floorNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.NorthWest, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.NorthEast, forCeiling: false).Y;

                    if (ix == 34 && iy == 8)
                    {
                        Log.Debug("meow?");
                    }
                    float otherVerticalSpotForFloorNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.SouthWest, forCeiling: false).Y;
                    float floorNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.NorthEast, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.NorthWest, forCeiling: false).Y;
                    float otherVerticalSpotForFloorNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.SouthEast, forCeiling: false).Y;
                    float floorSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.SouthWest, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.SouthEast, forCeiling: false).Y;
                    float otherVerticalSpotForFloorSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.NorthWest, forCeiling: false).Y;
                    float floorSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.SouthEast, forCeiling: false).Y;
                    float otherHorizontalSpotForFloorSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.SouthWest, forCeiling: false).Y;
                    float otherVerticalSpotForFloorSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.NorthEast, forCeiling: false).Y;
                    Vector3 ceilingWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.West, forCeiling: true);
                    Vector3 ceilingNorth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.North, forCeiling: true);
                    Vector3 ceilingEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.East, forCeiling: true);
                    Vector3 ceilingSouth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.South, forCeiling: true);
                    Vector3 otherCeilingWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.East, forCeiling: true);
                    Vector3 otherCeilingNorth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.South, forCeiling: true);
                    Vector3 otherCeilingEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.West, forCeiling: true);
                    Vector3 otherCeilingSouth = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.North, forCeiling: true);
                    float ceilingNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.NorthWest, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.NorthEast, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingNorthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.SouthWest, forCeiling: true).Y;
                    float ceilingNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.NorthEast, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.NorthWest, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingNorthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy - 1), TileSpot.SouthEast, forCeiling: true).Y;
                    float ceilingSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.SouthWest, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix - 1, iy), TileSpot.SouthEast, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingSouthWest = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.NorthWest, forCeiling: true).Y;
                    float ceilingSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy), TileSpot.SouthEast, forCeiling: true).Y;
                    float otherHorizontalSpotForCeilingSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix + 1, iy), TileSpot.SouthWest, forCeiling: true).Y;
                    float otherVerticalSpotForCeilingSouthEast = Extensions.GetPositionAtTile(Game1.currentLocation.Map, new(ix, iy + 1), TileSpot.NorthEast, forCeiling: true).Y;

                    var customWallSize = new float?[4];
                    var customWallOffset = new float?[4];
                    string[] dirNames = ["West", "North", "East", "South"];
                    for (int i = 0; i < customWallSize.Count(); ++i)
                    {
                        string dataSizeLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallData_{dirNames[i]}_Size";
                        string dataOffsetLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallData_{dirNames[i]}_Offset";

                        var dataSize = Game1.currentLocation.Map.GetLayer(dataSizeLayer);
                        var dataOffset = Game1.currentLocation.Map.GetLayer(dataOffsetLayer);

                        customWallSize[i] = Extensions.GetValueForDataTileIndex(dataSize?.GetTileIndexAt(ix, iy) ?? -1);
                        customWallOffset[i] = Extensions.GetValueForDataTileIndex(dataOffset?.GetTileIndexAt(ix, iy) ?? -1);

                        if (float.IsNaN(customWallSize[i].Value)) customWallSize[i] = null;
                        if (float.IsNaN(customWallOffset[i].Value)) customWallOffset[i] = null;
                    }
                    var customWallSizeMods = new float[4];
                    var customWallOffsetMods = new float[4];
                    string dataSizeModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallSizeModifierData";
                    string dataOffsetModifierLayer = $"{Mod.Instance.ModManifest.UniqueID}/WallOffsetModifierData";
                    var dataSizeModifier = Game1.currentLocation.Map.GetLayer(dataSizeModifierLayer);
                    var dataOffsetModifier = Game1.currentLocation.Map.GetLayer(dataOffsetModifierLayer);
                    Extensions.ModifyValueForDataTileIndex(dataSizeModifier?.GetTileIndexAt(ix, iy) ?? -1, ref customWallSizeMods[0], ref customWallSizeMods[1], ref customWallSizeMods[3], ref customWallSizeMods[2]);
                    Extensions.ModifyValueForDataTileIndex(dataOffsetModifier?.GetTileIndexAt(ix, iy) ?? -1, ref customWallOffsetMods[0], ref customWallOffsetMods[1], ref customWallOffsetMods[3], ref customWallOffsetMods[2]);

                    TileSpot[] walls = [TileSpot.West, TileSpot.North, TileSpot.East, TileSpot.South];
                    Vector3[] wallsFacing = [Vector3.Right, Vector3.Backward, Vector3.Left, Vector3.Forward];
                    Vector3[] wallOffsets = [Vector3.Left * 0.5f, Vector3.Forward * 0.5f, Vector3.Right * 0.5f, Vector3.Backward * 0.5f];
                    bool[,] valid = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3]
                    {
                        {
                            !float.IsNaN( floorWest.Y  ) && !float.IsNaN( ceilingWest.Y  ) && float.IsNaN( otherFloorWest.Y  ) || customWallSize[0].HasValue,
                            !float.IsNaN( floorWest.Y  ) && !float.IsNaN( otherFloorWest.Y  ) && Math.Abs(floorWest.Y - otherFloorWest.Y) >= 0.1,
                            !float.IsNaN( ceilingWest.Y ) && !float.IsNaN( otherCeilingWest.Y ),
                            !float.IsNaN( otherFloorWest.Y  ) && !float.IsNaN( otherCeilingWest.Y  ),
                        },
                        {
                            !float.IsNaN( floorNorth.Y ) && !float.IsNaN( ceilingNorth.Y ) && float.IsNaN( otherFloorNorth.Y ) || customWallSize[1].HasValue,
                            !float.IsNaN( floorNorth.Y ) && !float.IsNaN( otherFloorNorth.Y ) && Math.Abs(floorNorth.Y - otherFloorNorth.Y) >= 0.1,
                            !float.IsNaN( ceilingNorth.Y ) && !float.IsNaN( otherCeilingNorth.Y ),
                            !float.IsNaN( otherFloorNorth.Y ) && !float.IsNaN( otherCeilingNorth.Y ),
                        },
                        {
                            !float.IsNaN( floorEast.Y  ) && !float.IsNaN( ceilingEast.Y  ) && float.IsNaN( otherFloorEast.Y  ) || customWallSize[2].HasValue,
                            !float.IsNaN( floorEast.Y  ) && !float.IsNaN( otherFloorEast.Y  ) && Math.Abs(floorEast.Y - otherFloorEast.Y) >= 0.1,
                            !float.IsNaN( ceilingEast.Y ) && !float.IsNaN( otherCeilingEast.Y ),
                            !float.IsNaN( otherFloorEast.Y  ) && !float.IsNaN( otherCeilingEast.Y  ),
                        },
                        {
                            !float.IsNaN( floorSouth.Y ) && !float.IsNaN( ceilingSouth.Y ) && float.IsNaN( otherFloorSouth.Y ) || customWallSize[3].HasValue,
                            !float.IsNaN( floorSouth.Y ) && !float.IsNaN( otherFloorSouth.Y ) && Math.Abs(floorSouth.Y - otherFloorSouth.Y) >= 0.1,
                            !float.IsNaN( ceilingSouth.Y ) && !float.IsNaN( otherCeilingSouth.Y ),
                            !float.IsNaN( otherFloorSouth.Y ) && !float.IsNaN( otherCeilingSouth.Y ),
                        },
                    };
                    float[,] edges = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4]
                    {
                        { floorWest.Y, floorWest.Y, ceilingWest.Y, otherFloorWest.Y },
                        { floorNorth.Y, floorNorth.Y, ceilingNorth.Y, otherFloorNorth.Y },
                        { floorEast.Y, floorEast.Y, ceilingEast.Y, otherFloorEast.Y },
                        { floorSouth.Y, floorSouth.Y, ceilingSouth.Y, otherFloorSouth.Y },
                    };
                    for (int ioffset = 0; ioffset < customWallOffset.Length; ++ioffset)
                    {
                        for (int iedge = 0; iedge < edges.GetLength( 1 ); ++iedge)
                        {
                            if (customWallOffset[ioffset].HasValue)
                                edges[ioffset, iedge] += customWallOffset[ioffset].Value;
                        }
                    }
                    float[,,] yForWalls = // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4][leftWallBase=0, rightWallBase=0, leftWallTarget=2, rightWallTarget=3]
                    {
                        {
                            { floorSouthWest, floorNorthWest, ceilingSouthWest, ceilingNorthWest },
                            { floorSouthWest, floorNorthWest, otherHorizontalSpotForFloorSouthWest, otherHorizontalSpotForFloorNorthWest },
                            { ceilingSouthWest, ceilingNorthWest, otherHorizontalSpotForCeilingSouthWest, otherHorizontalSpotForCeilingNorthWest },
                            { otherHorizontalSpotForFloorSouthWest, otherHorizontalSpotForFloorNorthWest, otherHorizontalSpotForCeilingSouthWest, otherHorizontalSpotForCeilingNorthWest }
                        },
                        {
                            { floorNorthWest, floorNorthEast, ceilingNorthWest, ceilingNorthEast },
                            { floorNorthWest, floorNorthEast, otherVerticalSpotForFloorNorthWest, otherVerticalSpotForFloorNorthEast },
                            { ceilingNorthWest, ceilingNorthEast, otherVerticalSpotForCeilingNorthWest, otherVerticalSpotForCeilingNorthEast },
                            { otherVerticalSpotForFloorNorthWest, otherVerticalSpotForFloorNorthEast, otherVerticalSpotForCeilingNorthWest, otherVerticalSpotForCeilingNorthEast },
                        },
                        {
                            { floorNorthEast, floorSouthEast, ceilingNorthEast, ceilingSouthEast },
                            { floorNorthEast, floorSouthEast, otherHorizontalSpotForFloorNorthEast, otherHorizontalSpotForFloorSouthEast },
                            { ceilingNorthEast, ceilingSouthEast, otherHorizontalSpotForCeilingNorthEast, otherHorizontalSpotForCeilingSouthEast },
                            { otherHorizontalSpotForFloorNorthEast, otherHorizontalSpotForFloorSouthEast, otherHorizontalSpotForCeilingNorthEast, otherHorizontalSpotForCeilingSouthEast },
                        },
                        {
                            { floorSouthEast, floorSouthWest, ceilingSouthEast, ceilingSouthWest },
                            { floorSouthEast, floorSouthWest, otherVerticalSpotForFloorSouthEast, otherVerticalSpotForFloorSouthWest },
                            { ceilingSouthEast, ceilingSouthWest, otherVerticalSpotForCeilingSouthEast, otherVerticalSpotForCeilingSouthWest },
                            { otherVerticalSpotForFloorSouthEast, otherVerticalSpotForFloorSouthWest, otherVerticalSpotForCeilingSouthEast, otherVerticalSpotForCeilingSouthWest },
                        },
                    };
                    for (int ioffset = 0; ioffset < customWallOffset.Length; ++ioffset)
                    {
                        for (int iedge = 0; iedge < yForWalls.GetLength(1); ++iedge)
                        {
                            for (int ival = 0; ival < yForWalls.GetLength(2); ++ival)
                            {
                                if (customWallOffset[ioffset].HasValue)
                                    yForWalls[ioffset, iedge, ival] += customWallOffset[ioffset].Value;
                            }

                            if (yForWalls[ioffset, iedge, 0] > yForWalls[ioffset, iedge, 2])
                                Util.Swap(ref yForWalls[ioffset, iedge, 0], ref yForWalls[ioffset, iedge, 2]);
                            if (yForWalls[ioffset, iedge, 1] > yForWalls[ioffset, iedge, 3])
                                Util.Swap(ref yForWalls[ioffset, iedge, 1], ref yForWalls[ioffset, iedge, 3]);
                        }
                    }
                    float[,,] heightsForWalls = new float[yForWalls.GetLength( 0 ), yForWalls.GetLength( 1 ), 2]; // [direction][floor_to_ceiling=0, floor_to_adjacent_floor=1, ceiling_to_adjacent_ceiling=2, adjacent_floor_to_adjacent_ceiling=3, custom=4][left=0, right=1]
                    for (int idir = 0; idir < yForWalls.GetLength(0); ++idir)
                    {
                        for (int itype = 0; itype < yForWalls.GetLength(1); ++itype)
                        {
                            heightsForWalls[idir, itype, 0] = yForWalls[idir, itype, 2] - yForWalls[idir, itype, 0];
                            heightsForWalls[idir, itype, 1] = yForWalls[idir, itype, 3] - yForWalls[idir, itype, 1];

                            for (int ival = 0; ival < heightsForWalls.GetLength(2); ++ival)
                            {
                                if (customWallSize[idir].HasValue)
                                    heightsForWalls[idir, itype, ival] = customWallSize[idir].Value;
                            }

                            if (heightsForWalls[idir, itype, 0] < 0 && heightsForWalls[idir, itype, 1] < 0)
                            {
                                heightsForWalls[idir, itype, 0] = -heightsForWalls[idir, itype, 0];
                                heightsForWalls[idir, itype, 1] = -heightsForWalls[idir, itype, 1];
                                Util.Swap(ref yForWalls[idir, itype, 2], ref yForWalls[idir, itype, 0]);
                                Util.Swap(ref yForWalls[idir, itype, 3], ref yForWalls[idir, itype, 1]);
                            }
                        }
                    }

                    int[,] whichModIndices =
                    {
                        { 2, 0 },
                        { 0, 1 },
                        { 1, 3 },
                        { 3, 2 },
                    };

                    bool[] canResizeSegment = wallDef.VerticalSegments.Select(s => s.ContinuationMode != WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                    var resizableSegments = wallDef.VerticalSegments.Where(s => s.ContinuationMode != WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                    var nonresizableSegments = wallDef.VerticalSegments.Where(s => s.ContinuationMode == WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded).ToArray();
                    int resizableSegmentCount = canResizeSegment.Count(b => b);
                    int nonresizableSegmentCount = wallDef.VerticalSegments.Count - resizableSegmentCount;
                    int largestSegSize = wallDef.VerticalSegments.Max(s => s.TextureRegion.Height);
                    int heightOfAllSegs = wallDef.VerticalSegments.Sum(s => s.TextureRegion.Height);
                    int heightOfAllNonresizable = nonresizableSegments.Sum(s => s.TextureRegion.Height);
                    float[] relativeSegSizes = wallDef.VerticalSegments.Select(s => s.TextureRegion.Height / (float)largestSegSize).ToArray();
                    for (int iwall = 0; iwall < walls.Length; ++iwall)
                    {
                        int whichForWallBase = 0;
                        for (; whichForWallBase < edges.GetLength(1); ++whichForWallBase)
                        {
                            if (valid[iwall, whichForWallBase] && !float.IsNaN(edges[iwall, whichForWallBase]))
                                break;
                        }
                        if (whichForWallBase == edges.GetLength(1))
                            continue;

                        var leftY = yForWalls[iwall, whichForWallBase, 0];
                        var rightY = yForWalls[iwall, whichForWallBase, 1];
                        leftY += customWallOffsetMods[whichModIndices[iwall,0]];
                        rightY += customWallOffsetMods[whichModIndices[iwall,1]];
                        var centerY = (leftY + rightY) / 2;
                        var leftSize = heightsForWalls[iwall, whichForWallBase, 0];
                        var rightSize = heightsForWalls[iwall, whichForWallBase, 1];
                        leftSize += customWallSizeMods[whichModIndices[iwall, 0]];
                        rightSize += customWallSizeMods[whichModIndices[iwall, 1]];
                        var centerSize = (leftSize + rightSize) / 2;

                        if (centerSize == 0)
                            continue;

                        float tilesHigh = Math.Max(leftSize, rightSize);

                        float[] relativeSegSizesFull = new float[canResizeSegment.Length];
                        for (int i = 0; i < relativeSegSizesFull.Length; ++i)
                            relativeSegSizesFull[i] = canResizeSegment[i] ? ((tilesHigh - heightOfAllNonresizable / 16f) / tilesHigh / resizableSegmentCount) : (wallDef.VerticalSegments[i].TextureRegion.Height / (tilesHigh * 16));

                        float segStartPerc = 1;
                        for (int iseg = 0; iseg < wallDef.VerticalSegments.Count; ++iseg)
                        {
                            var segment = wallDef.VerticalSegments[iseg];

                            string ts = PathUtilities.NormalizeAssetName(segment.Tilesheet);
                            if (!locationVertices.TryGetValue(ts, out var verts))
                                locationVertices.Add(ts, verts = new(Game1.content.Load<Texture2D>(ts), new()));

                            float thisSegmentPerc = relativeSegSizesFull[ iseg ];
                            segStartPerc -= thisSegmentPerc;

                            float segLeftY = leftY + leftSize * segStartPerc;
                            float segRightY = rightY + rightSize * segStartPerc;
                            float segCenterY = (segLeftY + segRightY) / 2;
                            float segLeftSize = leftSize * thisSegmentPerc;
                            float segRightSize = rightSize * thisSegmentPerc;
                            float segCenterSize = (segLeftSize + segRightSize) / 2;

                            switch (segment.ContinuationMode)
                            {
                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.StretchIfNeeded:
                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.Stretch:
                                    RenderHelper.GenerateQuad(verts.verts, new Vector3(ix + 0.5f, 0, iy + 0.5f) + wallOffsets[iwall] + wallsFacing[iwall] * tuck,
                                                              new(0.5f, segRightY), new(-0.5f, segLeftY), new(0.5f, segRightY + segRightSize), new(-0.5f, segLeftY + segLeftSize),
                                                              segment.TextureRegion.X / (float)verts.tex.Width + tuck, segment.TextureRegion.Y / (float)verts.tex.Height + tuck, segment.TextureRegion.Width / (float)verts.tex.Width - tuck * 2, segment.TextureRegion.Height / (float)verts.tex.Height - tuck * 2,
                                                              -wallsFacing[iwall]);
                                    break;

                                case WallDefinitionData.WallSegmentData.SegmentContinuationMode.Tile:
                                    float tileSegLeftSize = (segment.TextureRegion.Height / 16);
                                    float tileSegRightSize = (segment.TextureRegion.Height / 16);
                                    float tileSegCenterSize = (tileSegLeftSize + tileSegRightSize) / 2;
                                    float tilesToDoRaw = MathF.Max(segLeftSize / tileSegLeftSize, segRightSize / tileSegRightSize);
                                    int tilesToDo = (int)MathF.Ceiling(tilesToDoRaw);
                                    for (int itiled = 0; itiled < tilesToDo; ++itiled)
                                    {
                                        float thisTileSegLeftSize = Math.Min(tileSegLeftSize, segLeftSize - tileSegLeftSize * itiled );
                                        float thisTileSegRightSize = Math.Min(tileSegRightSize, segRightSize - tileSegRightSize * itiled );
                                        float thisTileSegCenterSize = (thisTileSegLeftSize + thisTileSegRightSize) / 2;

                                        float thisTileSegSizePerc = Math.Min(1, tilesToDoRaw - itiled);

                                        float tileSegLeftY = segLeftY + tileSegLeftSize * itiled;
                                        float tileSegRightY = segRightY + tileSegRightSize * itiled;
                                        float tileSegCenterY = (tileSegLeftY + tileSegRightY) / 2;

                                        RenderHelper.GenerateQuad(verts.verts, new Vector3(ix + 0.5f, 0, iy + 0.5f) + wallOffsets[iwall] + wallsFacing[iwall] * tuck,
                                                                  new(0.5f, tileSegRightY), new(-0.5f, tileSegLeftY), new(0.5f, tileSegRightY + thisTileSegRightSize), new(-0.5f, tileSegLeftY + thisTileSegLeftSize),
                                                                  segment.TextureRegion.X / (float)verts.tex.Width + tuck, segment.TextureRegion.Y / (float)verts.tex.Height + tuck, segment.TextureRegion.Width / (float)verts.tex.Width - tuck * 2, (segment.TextureRegion.Height * thisTileSegSizePerc) / (float)verts.tex.Height - tuck * 2,
                                                                  -wallsFacing[iwall]);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        string[] tilesheetsWithTranslucency = [ "Maps\\townInterior" ]; // TODO: Unhardcode
        var locationVerticesForRender = locationVertices.ToList();
        locationVerticesForRender.Sort( (a, b) => tilesheetsWithTranslucency.Contains( a.Key ).CompareTo( tilesheetsWithTranslucency.Contains( b.Key ) ) );

        Game1.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        RenderHelper.GenericEffect.World = Matrix.Identity;
        foreach (var verts in locationVerticesForRender)
        {
            if (verts.Value.verts.Count == 0)
                continue;

            using VertexBuffer vbo = new(Game1.graphics.GraphicsDevice, typeof(SimpleVertex), verts.Value.verts.Count, BufferUsage.WriteOnly);
            vbo.SetData(verts.Value.verts.ToArray());
            Game1.graphics.GraphicsDevice.SetVertexBuffer(vbo);

            RenderHelper.GenericEffect.Texture = verts.Value.tex;

            foreach (var pass in RenderHelper.GenericEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Game1.graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Value.verts.Count / 3);
            }
        }

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;

        foreach (var obj in Game1.currentLocation.Objects.Pairs)
        {
            if (models.ContainsKey(obj.Value.QualifiedItemId))
            {
#if false
                    var model = models[obj.Value.QualifiedItemId];

                    var inst = model.Model.Instance.CreateInstance();
                    Matrix m1 = Matrix.CreateScale(model.Scale) *
                                (Matrix.CreateRotationX(model.Rotation.X) * Matrix.CreateRotationY(model.Rotation.Y) * Matrix.CreateRotationZ(model.Rotation.Z)) *
                                Matrix.CreateTranslation(model.Translation);
                    Matrix m = m1 * Matrix.CreateWorld((obj.Key + new Vector2( 0.5f, 0.5f )).To3D(), Vector3.Forward, Vector3.Up);
                    inst.Draw(projectionMatrix, basicEffect.View, m);

                    if (obj.Value.readyForHarvest)
                    {
                        if (model.HeldObjectOffset.HasValue)
                        {
                            if (models.ContainsKey(obj.Value.heldObject.Value.QualifiedItemId))
                            {
                                var mo = models[obj.Value.heldObject.Value.QualifiedItemId];
                                inst = mo.Model.Instance.CreateInstance();
                                m1 = Matrix.CreateScale(mo.Scale) *
                                     (Matrix.CreateRotationX(mo.Rotation.X) * Matrix.CreateRotationY(mo.Rotation.Y) * Matrix.CreateRotationZ(mo.Rotation.Z)) *
                                     Matrix.CreateTranslation(mo.Translation);
                                m = m1 * Matrix.CreateWorld((obj.Key + new Vector2( 0.5f, 0.5f )).To3D() + model.HeldObjectOffset.Value, Vector3.Forward, Vector3.Up);
                                inst.Draw(projectionMatrix, basicEffect.View, Matrix.CreateScale(model.HeldObjectScale) * m );
                            }
                            else
                            {
                                ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                                DoDrawBillboard(draw.GetTexture(), (obj.Key + new Vector2(0.5f, 0.5f)).To3D() + model.HeldObjectOffset.Value, new Vector2(1, 1) * model.HeldObjectScale, draw.GetSourceRect(0));
                            }
                        }
                        else
                        {
                            Vector3 pos = (obj.Key + new Vector2(0.5f, 0.5f)).To3D() + new Vector3(0, 2, 0);
                            DoDrawBillboard(Game1.mouseCursors, pos, new Vector2( 1, 1 ), new Rectangle(141, 465, 20, 24) );
                            ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                            DoDrawBillboard(draw.GetTexture(), pos, new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
                        }
                    }
#endif
            }
            else
            {
                ParsedItemData draw = ItemRegistry.GetDataOrErrorItem(obj.Value.QualifiedItemId);
                RenderHelper.DrawBillboard(Camera, draw.GetTexture(), obj.Key.ToPoint().To3D(Game1.currentLocation.Map) + new Vector3(0, obj.Value.bigCraftable.Value ? 1 : 0.5f,0), new Vector2(1, obj.Value.bigCraftable.Value ? 2 : 1), draw.GetSourceRect(obj.Value.showNextIndex.Value ? 1 : 0));

                if (obj.Value.readyForHarvest.Value)
                {
                    Vector3 pos = obj.Key.ToPoint().To3D(Game1.currentLocation.Map) + new Vector3(0, 2.5f, 0);
                    RenderHelper.DrawBillboard(Camera, Game1.mouseCursors, pos, new Vector2(1, 1), new Rectangle(141, 465, 20, 24));
                    draw = ItemRegistry.GetDataOrErrorItem(obj.Value.heldObject.Value.QualifiedItemId);
                    RenderHelper.DrawBillboard(Camera, draw.GetTexture(), pos + new Vector3(0, 0.1f, 0), new Vector2(0.9f, 0.9f), draw.GetSourceRect(0));
                }
            }
        }

        foreach (var farmer in Game1.currentLocation.farmers)
        {
#if false
                var model = models["farmer"];
                if (!State.CharacterModels.ContainsKey(farmer))
                {
                    var newModel = model.Model.Instance.CreateInstance();
                    State.CharacterModels.Add(farmer, newModel);
                }
                var modelInst = State.CharacterModels[farmer];

                int ind = 0;
                for (; ind < modelInst.Controller.Armature.AnimationTracks.Count; ++ind)
                {
                    if (modelInst.Controller.Armature.AnimationTracks[ind].Name == "GeneWalking.002")
                    {
                        break;
                    }
                }
                if (ind < modelInst.Controller.Armature.AnimationTracks.Count)
                {
                    modelInst.Controller.Armature.SetAnimationFrame(ind, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds, true);
                }

                Matrix m1 = Matrix.CreateScale(model.Scale) *
                            (Matrix.CreateRotationX(model.Rotation.X) * Matrix.CreateRotationY(model.Rotation.Y) * Matrix.CreateRotationZ(model.Rotation.Z)) *
                            Matrix.CreateTranslation(model.Translation);
                Matrix m = m1 * Matrix.CreateRotationY(farmer.GetFacing3D()) * Matrix.CreateWorld(farmer.GetPosition3D(), Vector3.Forward, Vector3.Up);

                modelInst.Draw(projectionMatrix, basicEffect.View, m);
#endif
        }

        foreach (var character in Game1.currentLocation.characters)
        {
            var spr = character.Sprite;

            // todo - models

            RenderHelper.DrawBillboard(Camera, spr.Texture, character.GetPosition3D() + new Vector3(0, spr.SourceRect.Height / 16 / 2f, 0 ), new Vector2(spr.SourceRect.Width / 16, spr.SourceRect.Height / 16), spr.SourceRect);
        }

        return false;
    }
    public virtual bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        return true;
    }

    protected void DoFarmerMovePosition(Farmer player, Vector3 movement, GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
    {
        if (player.IsSitting())
            return;

        if (Game1.activeClickableMenu != null && (Game1.CurrentEvent == null || Game1.CurrentEvent.playerControlSequence))
        {
            return;
        }

        BoundingBoxGroup temporaryPassableTiles = Mod.Instance.Helper.Reflection.GetField<BoundingBoxGroup>(player, "temporaryPassableTiles").GetValue();
        if (player.CanMove || Game1.eventUp || player.controller != null)
        {
            // Need to rewrite to check both at once?

            float movementSpeed = player.getMovementSpeed();

            Vector3 actualMovement = movement * movementSpeed;

            Rectangle next = player.GetBoundingBox();
            Rectangle nextH = next, nextH2 = next;
            Rectangle nextV = next, nextV2 = next;
            nextH.Location += new Point((int)Math.Ceiling(actualMovement.X), 0);
            nextH2.Location += new Point((int)Math.Ceiling(actualMovement.X / 2), 0);
            nextV.Location += new Point(0, (int)Math.Ceiling(actualMovement.Z));
            nextV2.Location += new Point(0, (int)Math.Ceiling(actualMovement.Z / 2));

            temporaryPassableTiles.ClearNonIntersecting(player.GetBoundingBox());
            player.temporarySpeedBuff = 0f;
            if (actualMovement.Z != 0)
            {
                Warp warp = Game1.currentLocation.isCollidingWithWarp(nextV, player);
                if (warp != null && player.IsLocalPlayer)
                {
                    player.warpFarmer(warp, 0);
                    return;
                }
                if (!currentLocation.isCollidingPosition(nextV, viewport, isFarmer: true, 0, glider: false, player) || player.ignoreCollisions)
                {
                    player.position.Y += actualMovement.Z;
                    //player.behaviorOnMovement(0);
                }
                else if (!currentLocation.isCollidingPosition(nextV2, viewport, isFarmer: true, 0, glider: false, player))
                {
                    player.position.Y += actualMovement.Z / 2;
                    //player.behaviorOnMovement(0);
                }
                //else Log.Debug("vcoll");
            }
            if (actualMovement.X != 0)
            {
                Warp warp3 = Game1.currentLocation.isCollidingWithWarp(nextH, player);
                if (warp3 != null && player.IsLocalPlayer)
                {
                    player.warpFarmer(warp3, 1);
                    return;
                }
                if (!currentLocation.isCollidingPosition(nextH, viewport, isFarmer: true, 0, glider: false, player) || player.ignoreCollisions)
                {
                    player.position.X += actualMovement.X;
                    //player.behaviorOnMovement(1);
                }
                else if (!currentLocation.isCollidingPosition(nextH2, viewport, isFarmer: true, 0, glider: false, player))
                {
                    player.position.X += actualMovement.X / 2f;
                    //player.behaviorOnMovement(1);
                }
                //else Log.Debug("hcoll");
            }
        }
        /*
        if (currentLocation != null && currentLocation.isFarmerCollidingWithAnyCharacter())
        {
            temporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)player.getTileLocation().X * 64, (int)player.getTileLocation().Y * 64, 64, 64));
        }
        */
    }
}
