using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Mods;
using static Stardew3D.GameModes.IGameMode;

namespace Stardew3D.GameModes;

public abstract partial class BaseGameMode : IGameMode
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
    protected RenderTarget2D RenderTarget { get; set; }

    protected virtual bool NeedsRenderTargetHandling => true;

    public virtual void SwitchOn(IGameMode previousMode)
    {
        WorldRenderer = new();
        //RenderTarget = new(Game1.graphics.GraphicsDevice, GameRunner.instance.Window.ClientBounds.Width, GameRunner.instance.Window.ClientBounds.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
    }

    public virtual void SwitchOff(IGameMode nextMode)
    {
        WorldRenderer?.Dispose();
        WorldRenderer = null;
        RenderTarget?.Dispose();
        RenderTarget = null;
    }

    protected abstract void UpdateCamera();

    public virtual void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling) { }

    public virtual void BeforeUpdate()
    {
        foreach (IGameCursor cursor in Cursors)
            cursor?.Update(this);

        WorldRenderer.UpdateState();
    }
    public virtual void AfterUpdate() { }

    protected virtual WorldRenderer.RenderMode WorldRenderMode => WorldRenderer.RenderMode.Default;
    public virtual bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender )
    {
        if (step >= RenderSteps.MenuBackground && step < RenderSteps.GlobalFade)
            return true;

        if (step != RenderSteps.World)
            return true;

        if (!NeedsRenderTargetHandling)
        {
            if (Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget != targetScreen)
                Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);
        }
        else
        {
            if (RenderTarget == null || RenderTarget.Width != Game1.game1.localMultiplayerWindow.Width || RenderTarget.Height != Game1.game1.localMultiplayerWindow.Height)
            {
                RenderTarget?.Dispose();
                RenderTarget = new(Game1.graphics.GraphicsDevice, Game1.game1.localMultiplayerWindow.Width, Game1.game1.localMultiplayerWindow.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, Mod.Config.MultisampleCount, RenderTargetUsage.PreserveContents);
            }

            Game1.graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
        }

        Game1.graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.CornflowerBlue, 1, 0);

        UpdateCamera();

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;
        RenderHelper.GenericEffect.Projection = ProjectionMatrix;

        if ( Mod.State.RenderDebugDraw )
            RenderHelper.DebugRender(Camera);
        if (Mod.State.RenderDebugGrid)
            RenderHelper.DebugRenderGrid();

        RenderWorld();

        if (NeedsRenderTargetHandling)
        {
            Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);
            sb.Begin();
            sb.Draw(RenderTarget, new Rectangle(0, 0, targetScreen?.Width ?? GameRunner.instance.graphicsDeviceManager.PreferredBackBufferWidth, targetScreen?.Height ?? GameRunner.instance.graphicsDeviceManager.PreferredBackBufferHeight), Color.White);
            sb.End();
        }

        return false;
    }

    public virtual void RenderWorld()
    {
        WorldRenderer.Render(ProjectionMatrix, Camera, WorldRenderMode);
    }

    public virtual bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        return true;
    }
}
