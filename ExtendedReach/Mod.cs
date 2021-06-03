using System;
using StardewModdingAPI;
using System.Reflection;
using Harmony;
using StardewValley;
using SpaceShared;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using SpaceShared.APIs;

namespace ExtendedReach
{
    public class Mod : StardewModdingAPI.Mod
    {
        public static Mod instance;
        private static Configuration config;
        private HarmonyInstance harmony;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            Log.Monitor = Monitor;
            config = helper.ReadConfig<Configuration>();

            helper.Events.Display.RenderedWorld += onRenderWorld;
            helper.Events.GameLoop.GameLaunched += onGameLaunched;

            try
            {
                harmony = HarmonyInstance.Create("spacechase0.ExtendedReach");
                doTranspiler(typeof(Utility), nameof(Utility.canGrabSomethingFromHere));
                doTranspiler(typeof(Utility), nameof(Utility.checkForCharacterInteractionAtTile));
                doTranspiler(typeof(Game1), nameof(Game1.pressActionButton));
                doTranspiler(typeof(Game1), nameof(Game1.pressUseToolButton));
                doTranspiler(typeof(Game1), nameof(Game1.tryToCheckAt));
                doTranspiler(typeof(GameLocation), nameof(GameLocation.isActionableTile));
            }
            catch ( Exception e )
            {
                Log.error("Exception patching: ");
                Log.error(e.ToString());
            }
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            gmcm.RegisterModConfig(ModManifest, () => config = new Configuration(), () => Helper.WriteConfig(config));
            gmcm.RegisterSimpleOption(ModManifest, "Wiggly Arms", "Show wiggly arms reaching out to your cursor.", () => config.WigglyArms, (bool b) => config.WigglyArms = b);
        }

        private float ampDir = 1;
        private float amp = 0;
        private Vector2 prevMousePos;
        private void onRenderWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsPlayerFree || !config.WigglyArms)
                return;

            var mousePos = Helper.Input.GetCursorPosition().ScreenPixels;
            var farmerPos = Game1.GlobalToLocal(Game1.player.Position);
            
            if (Game1.player.FacingDirection == 1)
            {
                farmerPos += new Vector2(18, -10);
            }
            else if (Game1.player.FacingDirection == 3)
            {
                farmerPos += new Vector2(50, -10);
            }
            else
            {
                farmerPos += new Vector2(18, -10);
            }

            if ((farmerPos - mousePos).Length() <= 64)
                return;

            var b = e.SpriteBatch;
            var tex = Helper.Reflection.GetField<Texture2D>(Game1.player.FarmerRenderer, "baseTexture").GetValue();

            amp += (mousePos - prevMousePos).Length() / 64 * ampDir;
            if (amp >= 1 && ampDir == 1)
                ampDir = -1;
            else if (amp <= -1 && ampDir == -1)
                ampDir = 1;
            if (amp < -1) amp = -1;
            if (amp > 1) amp = 1;

            var diff = (mousePos - farmerPos);
            diff.Normalize();
            var angle = Vector2.Transform(diff, Matrix.CreateRotationZ(3.1415926535f / 2));

            var points = new Vector2[]
            {
                farmerPos,
                new Vector2(),
                new Vector2(),
                new Vector2(),
                mousePos
            };
            points[1] = farmerPos + (mousePos - farmerPos) / 4 * 1 + angle * 64 * amp;
            points[2] = farmerPos + (mousePos - farmerPos) / 4 * 2;
            points[3] = farmerPos + (mousePos - farmerPos) / 4 * 3 + angle * -64 * amp;

            var curvePoints = computeCurvePoints((int)((farmerPos - mousePos).Length() / 32), points);
            
            for (int x = 0; x < curvePoints.Count - 1; x++)
            {
                DrawLine(b, tex, curvePoints[x], curvePoints[x + 1], Color.White, 12);
            }
            e.SpriteBatch.Draw(tex, mousePos, new Rectangle(153, 237, 4, 4), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);

            prevMousePos = mousePos;
        }

        private void doTranspiler(Type origType, string origMethod)
        {
            doTranspiler(origType.GetMethod(origMethod), typeof(TileRadiusFix).GetMethod(nameof(TileRadiusFix.IncreaseRadiusChecks)));
        }
        private void doTranspiler(MethodInfo orig, MethodInfo transpiler)
        {
            try
            {
                Log.trace($"Doing transpiler pacomputch {orig}:{transpiler}...");
                harmony.Patch(orig, null, null, new HarmonyMethod(transpiler));
            }
            catch (Exception e)
            {
                Log.error($"Exception doing transpiler patch {orig}:{transpiler}: {e}");
            }
        }

        // The below comes from here: https://stackoverflow.com/questions/33977226/drawing-bezier-curves-in-monogame-xna-produces-scratchy-lines
        //This is what I call to get all points between which to draw.
        public static List<Vector2> computeCurvePoints(int steps, Vector2[] pointsQ)
        {
            List<Vector2> curvePoints = new List<Vector2>();
            for (float x = 0; x < 1; x += 1 / (float)steps)
            {
                curvePoints.Add(getBezierPointRecursive(x, pointsQ));
            }
            return curvePoints;
        }

        //Calculates a point on the bezier curve based on the timeStep.
        private static Vector2 getBezierPointRecursive(float timeStep, Vector2[] ps)
        {
            if (ps.Length > 2)
            {
                List<Vector2> newPoints = new List<Vector2>();
                for (int x = 0; x < ps.Length - 1; x++)
                {
                    newPoints.Add(interpolatedPoint(ps[x], ps[x + 1], timeStep));
                }
                return getBezierPointRecursive(timeStep, newPoints.ToArray());
            }
            else
            {
                return interpolatedPoint(ps[0], ps[1], timeStep);
            }
        }

        //Gets the linearly interpolated point at t between two given points (without manual rounding).
        //Bad results!
        private static Vector2 interpolatedPoint(Vector2 p1, Vector2 p2, float t)
        {
            Vector2 roundedVector = (Vector2.Multiply(p2 - p1, t) + p1);
            return new Vector2((int)roundedVector.X, (int)roundedVector.Y);
        }

        //Method used to draw a line between two points.
        public static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(pixel, r, new Rectangle( 150, 237, 3, 3 ), color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
