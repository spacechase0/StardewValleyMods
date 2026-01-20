using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
using Stardew3D.Handlers;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Util;
using xTile;
using xTile.Tiles;
using static Stardew3D.Handlers.Game.IGameHandler;

namespace Stardew3D.Handlers.Game;

public abstract partial class CommonGameHandler : IGameHandler
{
    public abstract string Id { get; }
    public abstract string[] Tags { get; }

    public abstract ICamera Camera { get; }
    public abstract IReadOnlyList<IGameCursor> Cursors { get; }

    public RenderTarget2D CurrentTargetScreen { get => Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D; }
    public PBREnvironment GetCurrentEnvironmentFor(GameLocation location) => WorldRenderer.GetCurrentEnvironmentFor(location);
    public Matrix GetCurrentTransformFor(GameLocation location) => WorldRenderer.GetCurrentTransformFor( location );

    public abstract Matrix ProjectionMatrix { get; protected set; }
    protected WorldRenderer WorldRenderer { get; set; }

    public virtual void SwitchOn(IGameHandler previousHandler)
    {
        WorldRenderer = new();
    }

    public virtual void SwitchOff(IGameHandler nextHandler)
    {
        WorldRenderer?.Dispose();
        WorldRenderer = null;
    }

    protected abstract void UpdateCamera();

    public virtual void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling) { }

    public virtual void BeforeUpdate()
    {
        WorldRenderer.UpdateState();
    }
    public virtual void AfterUpdate() { }

    public virtual bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender)
    {
        if (Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget != targetScreen)
            Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);

        if (step >= RenderSteps.MenuBackground && step < RenderSteps.GlobalFade)
            return true;

        if (step != RenderSteps.World)
            return true;

        Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1, 0);

        UpdateCamera();

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;
        RenderHelper.GenericEffect.Projection = ProjectionMatrix;

        if ( Mod.State.RenderDebugDraw )
            RenderHelper.DebugRender(Camera);
        if (Mod.State.RenderDebugGrid)
            RenderHelper.DebugRenderGrid();

        WorldRenderer.Render(ProjectionMatrix, Camera);

        return false;
    }

    public virtual bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        return true;
    }
}
