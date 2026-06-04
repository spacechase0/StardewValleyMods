using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Handlers;
using Stardew3D.Models;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace Stardew3D.Rendering;

public class WorldRenderer : IDisposable
{
    private Texture2D skyTex = Mod.Instance.Helper.ModContent.Load<Texture2D>(Mod.Instance.Helper.ModContent.GetInternalAssetName("assets/skybox.png").BaseName);
    private Texture2D skyColors = Mod.Instance.Helper.ModContent.Load<Texture2D>("assets/skybox_colors.png");
    private Color[] skyColorsData;

    private RenderBatcher worldBatch = new(Game1.graphics.GraphicsDevice);
    private ModelObject skybox = Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/Skybox");
    private WorldEnvironment env = new();
    private WorldEnvironment skyboxEnv = new();

    public WorldEnvironment CurrentEnvironment => env;

    public WorldEnvironment GetCurrentEnvironmentFor(GameLocation location) => (Mod.State.GetRenderHandlersFor(location)[0] as LocationHandler)?.Environment ?? CurrentEnvironment;
    public Matrix GetCurrentTransformFor(GameLocation location) => locationTransforms.GetOrCreateValue( location ).Value;

    private bool builtLocationRecently = false;
    private GameLocation lastLoc;

    private ConditionalWeakTable<GameLocation, Holder<Matrix>> locationTransforms = new();

    public WorldRenderer()
    {
        skyColorsData = skyColors.GetColorData();
    }

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
        if (Mod.Instance.clearWorld)
        {
            worldBatch.ClearData();
            Mod.Instance.clearWorld = false;
        }

        var drawCtx = Mod.State.ModelManager.DrawContext;
        drawCtx.SetCamera(camera.ViewMatrix.Inverted());
        drawCtx.SetProjectionMatrix(projectionMatrix);

        {
            int time = Game1.timeOfDay % 2400;
            int tenMinNum = (time / 100 * 6) + (time % 100 / 10);
            float subNumPerc = Game1.gameTimeInterval / (float)(Game1.realMilliSecondsPerGameTenMinutes + Game1.currentLocation?.ExtraMillisecondsPerInGameMinute * 10);
            int subNum = (int)(subNumPerc * 16);

            int ind = tenMinNum * 16 + subNum;
            if (skyColorsData[ind].A == 255)
            {
                foreach (var effect in skybox.Matches.SelectMany(p => p.Values.SelectMany(l => l)).SelectMany(p => p.Mesh.OpaqueEffects))
                {
                    foreach (var param in effect.Parameters)
                    {
                        if (param.ParameterType != EffectParameterType.Texture2D)
                            continue;
                        Texture2D tex = param.GetValueTexture2D();
                        if (tex == null || !tex.Name.EndsWith("skybox.png"))
                            continue;

                        Color[] skyData = tex.GetColorData();
                        for (int i = 0; i < skyData.Length; ++i)
                        {
                            skyData[i] = skyColorsData[ind];

                            if ((i + 1) % tex.Width == 0)
                                ind += skyColors.Width;
                        }
                        tex.SetData(skyData);
                    }
                }
            }
        }
        skybox.Draw(skyboxEnv, Matrix.CreateTranslation(camera.Position));

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
            var mainRenderer = renderers[0] as LocationHandler;
            AddAdjacenciesForPortals(adjacencies[i].Location, adjacencies[i].TransformFromCurrent);

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
                if ((other.Renderers[0] as LocationHandler)?.Object != null)
                {
                    locationTransforms.AddOrUpdate((other.Renderers[0] as LocationHandler)?.Object, new(other.TransformFromCurrent));
                }
                var env = (other.Renderers[0] as LocationHandler).Environment;
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
        for (int i = 0; i < env.Lights.Length; ++i)
            env.Lights[i] = null;

        Color lightingCol = ((!(Game1.currentLocation is StardewValley.Locations.MineShaft mine)) ? ((Game1.ambientLight.Equals(Color.White) || (Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsRainingHere())) ? Game1.outdoorLight : Game1.ambientLight) : mine.getLightingColor(Game1.currentGameTime));
        env.AmbientLight = lightingCol == Color.White ? Color.White : new Color(255 - lightingCol.R, 255 - lightingCol.G, 255 - lightingCol.B);
        env.Lights[0] = new Light()
        {
            Type = Light.LightType.Point,
            Range = 16,
            Color = Color.White,
            Intensity = 1,
            Falloff = 0.5f,
            Position = Game1.player.StandingPixel3D + new Vector3(0, 1.5f, 0),
        };
        //env.AmbientLight = Color.White * 0.05f;
        //Game1.gameTimeInterval += Game1.currentGameTime.ElapsedGameTime.Milliseconds * 7;

        worldBatch.DrawBatched(env, Matrix.Identity, camera.ViewMatrix, projectionMatrix);

        if (renderMode.HasFlag( RenderMode.ClearDataAfterRendering ) )
            worldBatch.HideInstancesAfterFrame();
    }
}
