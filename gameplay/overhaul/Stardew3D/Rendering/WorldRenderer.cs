using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Handlers;
using Stardew3D.Handlers.Render;
using Stardew3D.Models;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Extensions;

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

    [Flags]
    public enum RenderMode
    {
        RecreateRenderData = 1 << 0,
        ClearDataAfterRendering = 1 << 1,

        Default = RecreateRenderData | ClearDataAfterRendering,
    }

    public void Render(Matrix projectionMatrix, ICamera camera, RenderMode renderMode = RenderMode.Default)
    {
        var drawCtx = Mod.State.ModelManager.DrawContext;
        drawCtx.SetCamera(camera.ViewMatrix.Inverted());
        drawCtx.SetProjectionMatrix(projectionMatrix);

        skybox.Draw(env, Matrix.CreateTranslation(camera.Position));

        var loc = Game1.currentLocation;

        List<(GameLocation Location, IRenderHandler[] Renderers, Matrix TransformFromCurrent)> adjacencies = new();
        adjacencies.Add(new(loc, Mod.State.GetRenderHandlersFor(loc), Matrix.Identity));

        void AddAdjacenciesForPortals(GameLocation loc, Matrix prevTransform)
        {
            if (loc == null || !loc.TryGetMapProperty($"{Mod.Instance.ModManifest.UniqueID}/Portals", out string mapProp))
                return;

            var portals = Portal.From(mapProp);
            foreach (var portal in portals)
            {
                if (adjacencies.Any(p => p.Location.NameOrUniqueName == portal.Value.OtherLocation))
                    continue;

                var otherLoc = Game1.getLocationFromName(portal.Value.OtherLocation);
                if (otherLoc == null || !otherLoc.TryGetMapProperty($"{Mod.Instance.ModManifest.UniqueID}/Portals", out string otherMapProp))
                    continue;

                var otherPortals = Portal.From(otherMapProp);
                if (!otherPortals.TryGetValue(portal.Value.MatchingPortal, out var match))
                    continue;

                // TODO: Support non-opposite facing portals
                Matrix oursToTheirs = prevTransform *
                                      Matrix.CreateTranslation(portal.Value.Position) *
                                      Matrix.CreateTranslation(-match.Position);

                adjacencies.Add(new(otherLoc, Mod.State.GetRenderHandlersFor(otherLoc), oursToTheirs));
            }
        }

        for (int i = 0; i < adjacencies.Count; i++)
        {
            var renderers = adjacencies[i].Renderers;
            var mainRenderer = renderers[0] as LocationRenderer;
            //AddAdjacenciesForPortals(adjacencies[i].Location, adjacencies[i].TransformFromCurrent);

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

        if (renderMode.HasFlag(RenderMode.RecreateRenderData))
        {
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
                        WorldSpriteBatch = new(other.Location),

                        WorldBatch = worldBatch,
                        WorldEnvironment = env,
                        WorldCamera = camera,
                        ParentWorldTransform = Matrix.Identity,
                        WorldTransform = other.TransformFromCurrent
                    });
                }
            }

            worldBatch.PrepareSprites(Matrix.Identity, camera);
        }

        worldBatch.DrawBatched(env, Matrix.Identity, camera.ViewMatrix, projectionMatrix);

        if (renderMode.HasFlag( RenderMode.ClearDataAfterRendering ) )
            worldBatch.HideInstancesAfterFrame();
    }
}
