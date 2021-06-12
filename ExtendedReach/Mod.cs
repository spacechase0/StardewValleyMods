using System;
using System.Collections.Generic;
using ExtendedReach.Framework;
using ExtendedReach.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Spacechase.Shared.Harmony;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ExtendedReach
{
    internal class Mod : StardewModdingAPI.Mod
    {
        public static Mod Instance;
        private static Configuration Config;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;
            Log.Monitor = this.Monitor;
            Mod.Config = helper.ReadConfig<Configuration>();

            helper.Events.Display.RenderedWorld += this.OnRenderWorld;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            HarmonyPatcher.Apply(this,
                new TileRadiusPatcher()
            );
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var gmcm = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            gmcm.RegisterModConfig(this.ModManifest, () => Mod.Config = new Configuration(), () => this.Helper.WriteConfig(Mod.Config));
            gmcm.RegisterSimpleOption(this.ModManifest, "Wiggly Arms", "Show wiggly arms reaching out to your cursor.", () => Mod.Config.WigglyArms, (bool b) => Mod.Config.WigglyArms = b);
        }

        private float AmpDir = 1;
        private float Amp;
        private Vector2 PrevMousePos;
        private void OnRenderWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsPlayerFree || !Mod.Config.WigglyArms)
                return;

            var mousePos = this.Helper.Input.GetCursorPosition().ScreenPixels;
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
            var tex = this.Helper.Reflection.GetField<Texture2D>(Game1.player.FarmerRenderer, "baseTexture").GetValue();

            this.Amp += (mousePos - this.PrevMousePos).Length() / 64 * this.AmpDir;
            if (this.Amp >= 1 && this.AmpDir == 1)
                this.AmpDir = -1;
            else if (this.Amp <= -1 && this.AmpDir == -1)
                this.AmpDir = 1;
            if (this.Amp < -1)
                this.Amp = -1;
            if (this.Amp > 1)
                this.Amp = 1;

            var diff = (mousePos - farmerPos);
            diff.Normalize();
            var angle = Vector2.Transform(diff, Matrix.CreateRotationZ(3.1415926535f / 2));

            var points = new[]
            {
                farmerPos,
                new(),
                new(),
                new(),
                mousePos
            };
            points[1] = farmerPos + (mousePos - farmerPos) / 4 * 1 + angle * 64 * this.Amp;
            points[2] = farmerPos + (mousePos - farmerPos) / 4 * 2;
            points[3] = farmerPos + (mousePos - farmerPos) / 4 * 3 + angle * -64 * this.Amp;

            var curvePoints = Mod.ComputeCurvePoints((int)((farmerPos - mousePos).Length() / 32), points);

            for (int x = 0; x < curvePoints.Count - 1; x++)
            {
                Mod.DrawLine(b, tex, curvePoints[x], curvePoints[x + 1], Color.White, 12);
            }
            e.SpriteBatch.Draw(tex, mousePos, new Rectangle(153, 237, 4, 4), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);

            this.PrevMousePos = mousePos;
        }

        // The below comes from here: https://stackoverflow.com/questions/33977226/drawing-bezier-curves-in-monogame-xna-produces-scratchy-lines
        //This is what I call to get all points between which to draw.
        public static List<Vector2> ComputeCurvePoints(int steps, Vector2[] pointsQ)
        {
            List<Vector2> curvePoints = new List<Vector2>();
            for (float x = 0; x < 1; x += 1 / (float)steps)
            {
                curvePoints.Add(Mod.GetBezierPointRecursive(x, pointsQ));
            }
            return curvePoints;
        }

        //Calculates a point on the bezier curve based on the timeStep.
        private static Vector2 GetBezierPointRecursive(float timeStep, Vector2[] ps)
        {
            if (ps.Length > 2)
            {
                List<Vector2> newPoints = new List<Vector2>();
                for (int x = 0; x < ps.Length - 1; x++)
                {
                    newPoints.Add(Mod.InterpolatedPoint(ps[x], ps[x + 1], timeStep));
                }
                return Mod.GetBezierPointRecursive(timeStep, newPoints.ToArray());
            }
            else
            {
                return Mod.InterpolatedPoint(ps[0], ps[1], timeStep);
            }
        }

        //Gets the linearly interpolated point at t between two given points (without manual rounding).
        //Bad results!
        private static Vector2 InterpolatedPoint(Vector2 p1, Vector2 p2, float t)
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
            spriteBatch.Draw(pixel, r, new Rectangle(150, 237, 3, 3), color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
