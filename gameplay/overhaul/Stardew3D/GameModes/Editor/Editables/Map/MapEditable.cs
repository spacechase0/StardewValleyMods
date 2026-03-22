using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui.Elements;
using SpaceShared;
using Stardew3D.GameModes.Editor.Editables;
using Stardew3D.GameModes.Editor.Editables.Map.EditingModes;
using Stardew3D.Handlers.Render;
using Stardew3D.Rendering;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Extensions;
using static Stardew3D.Handlers.IRenderHandler;

namespace Stardew3D.GameModes.Editor.Editables.Map;

public class MapEditable : IEditable
{
    public static Dictionary<string, Func<MapEditable, BaseEditingMode>> EditingModes = new()
    {
        ["Floor"] = e => new TileDataEditingMode(e, DimensionUtils.TileType.Floor),
        ["Ceiling"] = e => new TileDataEditingMode(e, DimensionUtils.TileType.Ceiling),
        ["Water"] = e => new TileDataEditingMode(e, DimensionUtils.TileType.Water),
    };

    public string Id { get; init; }
    public string LocationName { get; init; }
    public string AssetName { get; init; }
    public bool HasUnsavedChanges { get; private set; }

    private LocalizedContentManager ToLoadContentFrom;
    public DummyLocation Location;

    public BaseEditingMode EditingMode { get; set; }

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

            locRenderer.MarkDirty();
        }
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
                float y = new Point(ix, iy).To3D(Location.Map).Y;
                maxHeight = Math.Max(maxHeight, y);
            }
        }
        float camHeight = (mapSize.Y / 2 / 0.707f) * 0.55f;
        var camPos = new Vector3(mapSize.X / 2f, maxHeight + camHeight, mapSize.Y * 1.15f);
        (Mod.State.ActiveMode as EditorGameMode).SetCamera(camPos, new Vector3(0, -0.707f, -0.707f));

        Group modeButtons = new(MLEM.Ui.Anchor.AutoLeft, new Vector2(1, 10), setHeightBasedOnChildren: true);
        foreach (var potentialMode in EditingModes)
        {
            var factoryFunc = potentialMode.Value;
            modeButtons.AddChild(new Button(MLEM.Ui.Anchor.AutoInline, new Vector2(1, 32), potentialMode.Key)
            {
                SetWidthBasedOnChildren = true,
                OnPressed = _ => EditingMode = factoryFunc(this),
            });
        }

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
            modeButtons,
            dangerButtons,
        ];
    }

    private void Reset()
    {
        EditingMode = null;

        Location.reloadMap();
        MapModified();
    }

    private void Clear()
    {
        EditingMode = null;

        var layers = Location.Map.Layers.Where(l => l.Id.StartsWith($"{Mod.Instance.ModManifest.UniqueID}/"));
        foreach (var layer in layers.ToArray())
            Location.Map.RemoveLayer(layer);

        MapModified();
    }

    public void BeforeHidePanelContents()
    {
        Location.MapLoader?.Dispose();
        Location = null;
    }

    public void Update()
    {
        EditingMode?.Update();
    }

    public void RenderWorld(RenderBatcher b)
    {
        var editor = Mod.State.ActiveMode as EditorGameMode;
        RenderContext ctx = new()
        {
            Time = Game1.currentGameTime,
            TargetScreen = null,

            WorldSpriteBatch = new(Location),

            WorldBatch = b,
            WorldEnvironment = editor.EditorEnvironment,
            WorldCamera = editor.Camera,
            WorldTransform = Matrix.Identity,
        };

        foreach (var renderer in Mod.State.GetRenderHandlersFor(Location))
        {
            if (renderer is LocationRenderer locRenderer)
            {
                locRenderer.EvenMissing = true;
                locRenderer.Build();
            }

            renderer?.Render(ctx);
        }
    }

    public void AfterRenderWorld()
    {
        EditingMode?.Render();
    }

    public Dictionary<string, string> Save()
    {
        throw new System.NotImplementedException();
        HasUnsavedChanges = false;
    }
}
