using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using Stardew3D.GameModes.Editor.Editables;
using Stardew3D.Handlers.Render;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Extensions;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.GameModes.Editor.Editables.Map;

internal class MapEditable : IEditable
{
    private class DummyLocation : GameLocation
    {
        public LocalizedContentManager MapLoader;

        public DummyLocation(LocalizedContentManager mapLoader, string locName, string mapAsset)
        {
            name.Value = locName;
            mapPath.Value = mapAsset;
            MapLoader = mapLoader;

            _mapPathDirty = false;
            reloadMap();
        }

        protected override LocalizedContentManager getMapLoader()
        {
            return MapLoader.CreateTemporary();
        }
    }

    public string Id { get; init; }
    public string LocationName { get; init; }
    public string AssetName { get; init; }
    public bool HasUnsavedChanges { get; private set; }

    private LocalizedContentManager ToLoadContentFrom;
    private DummyLocation Location;

    public MapEditable(LocalizedContentManager toLoadContentFrom, string locName, string assetName)
    {
        ToLoadContentFrom = toLoadContentFrom;
        Id = assetName;
        LocationName = locName;
        AssetName = assetName;
    }

    public void Dispose()
    {
    }

    public void MapModified()
    {
        HasUnsavedChanges = true;

        foreach (var renderer in Mod.State.GetRenderHandlersFor(Location))
        {
            if (renderer is not LocationRenderer locRenderer)
                continue;

            locRenderer.Build(force: true);
        }
    }

    public void Reset()
    {
        Location.reloadMap();
        MapModified();
    }

    public void Clear()
    {
        var layers = Location.Map.Layers.Where(l => l.Id.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/"));
        foreach (var layer in layers.ToArray())
            Location.Map.RemoveLayer(layer);

        MapModified();
    }

    public ICollection<Element> PopulatePanelContents()
    {
        Location = new DummyLocation(ToLoadContentFrom, LocationName, AssetName);

        Point mapSize = new(Location.Map.Layers[0].LayerWidth, Location.Map.Layers[0].LayerHeight);

        float maxHeight = float.MinValue;
        for (int ix = 0; ix < mapSize.X; ++ix)
        {
            for (int iy = 0; iy < mapSize.Y; ++iy)
            {
                maxHeight = Math.Max(maxHeight, new Point(ix, iy).To3D(Location.Map).Y);
            }
        }
        float camHeight = (mapSize.Y / 2 / 0.707f) * 0.55f;
        var camPos = new Vector3(mapSize.X / 2f, maxHeight + camHeight, mapSize.Y * 1.15f);
        (Mod.State.ActiveMode as EditorGameMode).SetCamera(camPos, new Vector3(0, -0.707f, -0.707f));

        Group dangerButtons = new(MLEM.Ui.Anchor.BottomCenter, new Vector2(1, 0), setHeightBasedOnChildren: true);
        dangerButtons.AddChild(new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(1, 32), "Reset")
        {
            AutoSizeAddedAbsolute = new Vector2(-32, 0),
            OnPressed = _ =>
            {
                (Mod.State.ActiveMode as EditorGameMode).DoAfterConfirm(Reset);
            },
        });
        dangerButtons.AddChild(new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(1, 32), "Clear")
        {
            AutoSizeAddedAbsolute = new Vector2(-64, 0),
            NormalColor = Color.Red,
            HoveredColor = Color.DarkRed,
            OnPressed = _ =>
            {
                (Mod.State.ActiveMode as EditorGameMode).DoAfterConfirm(Clear);
            },
        });
        return
        [
            new Paragraph(MLEM.Ui.Anchor.TopLeft, 1, Id),
            dangerButtons,
        ];
    }

    public void BeforeHidePanelContents()
    {
        Location.MapLoader?.Dispose();
        Location = null;
    }

    public void RenderWorld(RenderBatcher b)
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        RenderContext ctx = new()
        {
            Time = Game1.currentGameTime,
            TargetScreen = null,

            WorldBatch = b,
            WorldEnvironment = editor.EditorEnvironment,
            WorldCamera = editor.Camera,
            WorldTransform = Matrix.Identity,
        };

        foreach (var renderer in Mod.State.GetRenderHandlersFor(Location))
        {
            if (renderer is LocationRenderer locRenderer)
                locRenderer.Build();

            renderer?.Render(ctx);
        }
    }

    public Dictionary<string, string> Save()
    {
        throw new System.NotImplementedException();
        HasUnsavedChanges = false;
    }
}
