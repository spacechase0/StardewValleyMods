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
using Stardew3D.Models;
using Stardew3D.Rendering;
using Stardew3D.Rendering.Renderers;
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
using static Stardew3D.IGameHandler;

namespace Stardew3D;

public abstract partial class ModGameHandler : IGameHandler
{
    public abstract string Id { get; }
    public abstract string[] Tags { get; }

    public abstract ICamera Camera { get; }

    public abstract Matrix ProjectionMatrix { get; protected set; }

    public virtual void SwitchOn()
    {
        skybox = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/Skybox");
    }
    public virtual void SwitchOff()
    {
        skybox = null;
    }

    public abstract void HandleGameplayInput(ref KeyboardState keyboardState, ref MouseState mouseState, ref GamePadState gamePadState, DefaultInputHandling defaultInputHandling);

    protected abstract void DoCamera();

    public virtual void BeforeUpdate()
    {
        builtLocationRecently = false;
    }
    public virtual void AfterUpdate() { }

    public virtual bool HandlesUiElsewhere => false;

    private RenderBatcher menuBatch;
    private RenderBatcher worldBatch;

    private IClickableMenu lastMenu = null;

    protected PBREnvironment env = PBREnvironment.CreateDefault();
    protected ModelObject skybox;

    public ModGameHandler()
    {
        menuBatch = new(Game1.graphics.GraphicsDevice);
        worldBatch = new(Game1.graphics.GraphicsDevice);
    }

    private bool builtLocationRecently = false;
    public virtual bool HandleRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen, Func<RenderSteps, SpriteBatch, GameTime, RenderTarget2D, bool> defaultRender)
    {
        if (Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget != targetScreen)
            Game1.graphics.GraphicsDevice.SetRenderTarget(targetScreen);

        if (step >= RenderSteps.MenuBackground && step < RenderSteps.GlobalFade)
        {
            return true;
        }

        if (step != RenderSteps.World)
            return true;

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;

        DoCamera();

        Game1.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
        Game1.graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        if ( Mod.Config.RenderDebugDraw )
            RenderHelper.DebugRender(Camera);
        if (Mod.Config.RenderDebugGrid)
            RenderHelper.DebugRenderGrid();

        var drawCtx = Stardew3D.Mod.State.ModelManager.DrawContext;
        drawCtx.SetCamera(Camera.ViewMatrix.Invert());
        drawCtx.SetProjectionMatrix(ProjectionMatrix);

        skybox.Draw(env, Matrix.CreateTranslation(Camera.Position));

        RenderLocation(Game1.currentLocation);

        Game1.graphics.GraphicsDevice.RasterizerState = RenderHelper.RasterizerState;
        Game1.graphics.GraphicsDevice.DepthStencilState = RenderHelper.DepthState;


        return false;
    }

    public virtual bool AfterRender(RenderSteps step, SpriteBatch sb, GameTime time, RenderTarget2D targetScreen)
    {
        return true;
    }

    private GameLocation lastLoc;
    protected void RenderLocation(GameLocation loc)
    {
        List<(string LocationName, IRenderHandler[] Renderers, Matrix TransformFromCurrent)> adjacencies = new();
        adjacencies.Add(new(loc.NameOrUniqueName, Mod.State.GetRenderHandlersFor(loc), Matrix.Identity));

        void AddAdjacenciesForPortals(LocationModelData locModel, Matrix prevTransform)
        {
            if (locModel == null)
                return;

            foreach (var entry in locModel.Portals)
            {
                if (adjacencies.Any(p => p.LocationName == entry.Value.OtherLocation))
                    continue;

                var loc = Game1.getLocationFromName(entry.Value.OtherLocation);
                if (loc == null)
                    continue;

                var renderers = Mod.State.GetRenderHandlersFor(loc);
                var mainRenderer = renderers[0] as LocationRenderer;

                if (!mainRenderer.ModelData.Portals.TryGetValue(entry.Value.MatchingPortal, out var match))
                    match = null;

                // TODO: Support non-opposite facing portals
                Matrix oursToTheirs = prevTransform *
                                        Matrix.CreateTranslation(entry.Value.Position) *
                                        Matrix.CreateTranslation(-match.Position);

                adjacencies.Add(new(entry.Value.OtherLocation, renderers, oursToTheirs));
            }
        }

        for (int i = 0; i < adjacencies.Count; i++)
        {
            var renderers = adjacencies[i].Renderers;
            var mainRenderer = renderers[0] as LocationRenderer;
            AddAdjacenciesForPortals(mainRenderer.ModelData, adjacencies[i].TransformFromCurrent);

            if (mainRenderer.IsDirty && !builtLocationRecently)
            {
                mainRenderer.Build();
                builtLocationRecently = true;
            }
        }

        if (lastLoc != loc)
        {
            //worldBatch.ClearData();
            lastLoc = loc;
        }

        foreach (var other in adjacencies)
        {
            var env = (other.Renderers[0] as LocationRenderer).Environment;
            foreach (var renderer in other.Renderers)
            {
                renderer.Render(new()
                {
                    Time = Game1.currentGameTime,
                    TargetScreen = Game1.graphics.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D,

                    MenuSpriteBatch = Game1.spriteBatch,

                    WorldBatch = worldBatch,
                    WorldEnvironment = env,
                    WorldCamera = Camera,
                    WorldTransform = other.TransformFromCurrent
                });
            }
        }
        worldBatch.DrawBatched(env, Matrix.Identity, Camera.ViewMatrix, ProjectionMatrix);
        worldBatch.HideInstancesAfterFrame();
    }

#if false
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
#endif
}
