using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.Data;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Render;
using Stardew3D.Models;
using StardewValley;

namespace Stardew3D.Rendering;

public class WorldRenderer : IDisposable
{
    private RenderBatcher worldBatch = new(Game1.graphics.GraphicsDevice);
    private ModelObject skybox = Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/Skybox");
    private PBREnvironment env = PBREnvironment.CreateDefault();

    public PBREnvironment CurrentEnvironment => env;

    public PBREnvironment GetCurrentEnvironmentFor(GameLocation location) => (Mod.State.GetRenderHandlersFor(location)[0] as LocationRenderer)?.Environment ?? CurrentEnvironment;
    public Matrix GetCurrentTransformFor(GameLocation location) => locationTransforms.GetOrCreateValue( location ).Value;

    private bool builtLocationRecently = false;
    private GameLocation lastLoc;

    private ConditionalWeakTable<GameLocation, Holder<Matrix>> locationTransforms = new();

    public void Dispose()
    {
        worldBatch.Dispose();
        worldBatch = null;
    }

    public void UpdateState()
    {
        builtLocationRecently = false;
    }

    public void Render(Matrix projectionMatrix, ICamera camera)
    {
        var drawCtx = Mod.State.ModelManager.DrawContext;
        drawCtx.SetCamera(camera.ViewMatrix.Invert());
        drawCtx.SetProjectionMatrix(projectionMatrix);

        skybox.Draw(env, Matrix.CreateTranslation(camera.Position));

        var loc = Game1.currentLocation;

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

                LocationModelData.Portal match = null;
                if (mainRenderer != null)
                {
                    if (!mainRenderer.ModelData.Portals.TryGetValue(entry.Value.MatchingPortal, out match))
                        match = null;
                }

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
            if ((other.Renderers[0] as LocationRenderer)?.Object != null)
            {
                locationTransforms.AddOrUpdate((other.Renderers[0] as LocationRenderer)?.Object, new(other.TransformFromCurrent));
             }
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
                    WorldCamera = camera,
                    WorldTransform = other.TransformFromCurrent
                });
            }
        }
        worldBatch.DrawBatched(env, Matrix.Identity, camera.ViewMatrix, projectionMatrix);
        worldBatch.HideInstancesAfterFrame();
    }
}
