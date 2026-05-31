using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using Stardew3D.DataModels;
using Stardew3D.Handlers;
using Stardew3D.Models;
using Stardew3D.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;

namespace Stardew3D.Rendering;

public class WorldRenderer : IDisposable
{
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
            Falloff = 0.15f,
            Position = Game1.player.StandingPixel3D + new Vector3(0, 1.5f, 0),
        };

        if (Game1.currentSong?.Name == "EmilyDance")
        {
            env.AmbientLight = Color.White * 0.025f;
            env.Lights[0] = null;
            float time = (Game1.currentSong as CueWrapper).cue._time;
            if (time >= 3.969f && time <= 62)
            {
                Color[][] cols =
                [
                    [new Color(91, 206, 250), new Color(245, 169, 184), Color.White, new Color(245, 169, 184), new Color(91, 206, 250)], //transgender
                    [new Color(213, 45, 0), new Color(255, 154, 86), Color.White, new Color(255, 154, 86), new Color(213, 45, 0)], //lesbian
                    [new Color(252, 244, 52), Color.White, new Color(156, 89, 209), new Color(64, 64, 64)], // nonbinary
                    [new Color(7, 141, 112), new Color(152, 232, 193), Color.White, new Color(123, 173, 226), new Color(61, 26, 120)], // gay
                    [new Color(214, 2, 112), new Color(214, 2, 112), new Color(155, 79, 150), new Color(0, 56, 168), new Color(0, 56, 168)], //bi
                    [new Color(64, 64, 64), new Color(163, 163, 163), Color.White, new Color(128, 0, 128)], // ace
                ];

                float strength = 0.0725f;
                if (time >= 10.1f)
                {
                    strength += Math.Min((time - 10.1f) / 0.4f, 1) * 0.05f;
                }

                if (time < 5)
                    strength *= Utility.Lerp(0, 1, time / 5f);

                if (time >= 60)
                    strength *= Utility.Lerp(1, 0, (time - 60) / 2f);

                float progress = (time - 3.969f) * 1000f;
                if (time < 60)
                {
                    if (time > 10.1f)
                    {
                        float interval = 60 - 10.1f;
                        float currTime = time - 10.1f;
                        float period = interval / (cols.Length * 2);

                        Point[] fiveCol = [new(2, 9), new(2, 5), new(6, 4), new(10, 5), new(10, 9)];
                        Point[] fourCol = [new(2, 7), new(4, 4), new(8, 4), new(10, 7)];

                        float progressSingle = (currTime % period) / period;
                        int curr = (int)(currTime / period) % cols.Length;
                        int next = (curr + 1) % cols.Length;
                        if (cols[curr].Length == cols[next].Length)
                        {
                            Point[] pts = cols[curr].Length == 5 ? fiveCol : fourCol;
                            for (int i = 0; i < env.Lights.Length; ++i)
                            {
                                env.Lights[i] = null;
                                if (i >= pts.Length)
                                    continue;

                                Color col = GetPrismaticColor([cols[curr][i], cols[next][i]], progressSingle * 1000);
                                col.A = 255;
                                env.Lights[i] = new()
                                {
                                    Position = pts[i].To3D(Game1.currentLocation) + new Vector3(0, 1, 0),
                                    Range = 3,
                                    Color = col,
                                    Intensity = 3,
                                };
                            }
                        }
                        else
                        {
                            Point[] ptsA = cols[curr].Length == 5 ? fiveCol : fourCol;
                            Point[] ptsB = cols[next].Length == 5 ? fiveCol : fourCol;

                            for (int i = 0; i < env.Lights.Length; ++i)
                            {
                                env.Lights[i] = null;
                                if (i < ptsA.Length)
                                {
                                    Color col = cols[curr][i];
                                    env.Lights[i] = new()
                                    {
                                        Position = ptsA[i].To3D(Game1.currentLocation) + new Vector3(0, 1, 0),
                                        Range = 3,
                                        Color = col,
                                        Intensity = interp(1, 0, progressSingle) * 3,
                                    };
                                    if (env.Lights[i].Intensity < 0.02f)
                                        env.Lights[i] = null;
                                }
                                else if (i - ptsA.Length < ptsB.Length)
                                {
                                    Color col = cols[next][i - ptsA.Length];
                                    env.Lights[i] = new()
                                    {
                                        Position = ptsB[i - ptsA.Length].To3D(Game1.currentLocation) + new Vector3(0, 1, 0),
                                        Range = 3,
                                        Color = col,
                                        Intensity = interp(0, 1, progressSingle) * 3,
                                    };
                                }

                            }
                        }
                    }

                    env.AmbientLight = GetPrismaticColor(Utility.PRISMATIC_COLORS, progress);
                    //strength += (env.AmbientLight.A / 255f) * 0.2f;
                    env.AmbientLight = new((int)(env.AmbientLight.R * strength), (int)(env.AmbientLight.G * strength), (int)(env.AmbientLight.B * strength), 255);
                }
            }
        }
        else progress = 0;

        foreach (var c in Game1.currentLocation.characters)
        {
            if (c is not Pet pet)
                continue;

            pet.Position = new Vector2(5.5f, 7) * Game1.tileSize;
            if (pet.CurrentBehavior != "SitDownLickRepeat")
                pet.CurrentBehavior = "SitDownLickRepeat";
        }

        worldBatch.DrawBatched(env, Matrix.Identity, camera.ViewMatrix, projectionMatrix);

        if (renderMode.HasFlag( RenderMode.ClearDataAfterRendering ) )
            worldBatch.HideInstancesAfterFrame();
    }

    public static float progress = 0;
    public static float interp(float a, float b, float x)
    {
        float i = b - a;

        const float c4 = (2 * MathF.PI) / 3;
        // https://easings.net/#easeInElastic
        float ret = x <= 0
          ? 0
          : x >= 1
          ? 1
          : -MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * c4);

        ret = MathF.Pow(MathF.Abs(2 * x - 1), 4);
        ret = MathF.Pow(x, 5);
        return ret * i + a;
    }
    public static float interpWrap(float a, float b, float x)
    {
        float interval = b - a;
        if (Math.Abs(interval) > 180)
            interval = (b + 180) % 360 - (a + 180) % 360;

        return (a + interp(0, interval, x)) % 360;
    }
    // https://stackoverflow.com/a/1626175
    public static void ColorToHSV(Color color, out float hue, out float saturation, out float value)
    {
        int max = Math.Max(color.R, Math.Max(color.G, color.B));
        int min = Math.Min(color.R, Math.Min(color.G, color.B));
        hue = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B).GetHue();
        saturation = (max == 0) ? 0 : 1f - (1f * min / max);
        value = max / 255f;
    }
    public static Color GetPrismaticColor(Color[] colors, float time)
    {

        float num = 1000f;
        int num2 = ((int)(time / num)) % colors.Length;
        int num3 = (num2 + 1) % colors.Length;
        float t = time / num % 1f;

        Color a = colors[num2];
        Color b = colors[num3];
        ColorToHSV(a, out float ahu, out float asa, out float ava);
        ColorToHSV(b, out float bhu, out float bsa, out float bva);
        float rhu = interpWrap(ahu, bhu, t);
        float rsa = interp(asa, bsa, t);
        float rva = interp(ava, bva, t);

        Color result = Util.ColorFromHsv(rhu, rsa, rva);
        result.A = (byte)(interp(0, 1, (time - 0.3f) / num % 1f) * 255f);
        return result;
    }
}
