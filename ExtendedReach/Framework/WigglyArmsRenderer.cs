using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace ExtendedReach.Framework
{
    /// <summary>Handles the logic for rendering wiggly arms.</summary>
    internal class WigglyArmsRenderer
    {
        /*********
        ** Fields
        *********/
        /// <summary>Provides an API for checking and changing input state.</summary>
        private readonly IInputHelper InputHelper;

        /// <summary>Provides an API for accessing inaccessible code.</summary>
        private readonly IReflectionHelper Reflection;

        private float AmpDir = 1;
        private float Amp;
        private Vector2 PrevMousePos;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="reflectionHelper">Provides an API for accessing inaccessible code.</param>
        public WigglyArmsRenderer(IInputHelper inputHelper, IReflectionHelper reflectionHelper)
        {
            this.InputHelper = inputHelper;
            this.Reflection = reflectionHelper;
        }

        /// <summary>Render the wiggly arms to the screen.</summary>
        /// <param name="spriteBatch">The sprite batch being rendered.</param>
        public void Render(SpriteBatch spriteBatch)
        {
            var mousePos = this.InputHelper.GetCursorPosition().ScreenPixels;
            var farmerPos = Game1.GlobalToLocal(Game1.player.Position);

            farmerPos += Game1.player.FacingDirection switch
            {
                1 => new Vector2(18, -10),
                3 => new Vector2(50, -10),
                _ => new Vector2(18, -10)
            };

            if ((farmerPos - mousePos).Length() <= 64)
                return;

            var tex = this.Reflection.GetField<Texture2D>(Game1.player.FarmerRenderer, "baseTexture").GetValue();

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

            var curvePoints = this.ComputeCurvePoints((int)((farmerPos - mousePos).Length() / 32), points);

            for (int x = 0; x < curvePoints.Count - 1; x++)
            {
                this.DrawLine(spriteBatch, tex, curvePoints[x], curvePoints[x + 1], Color.White, 12);
            }
            spriteBatch.Draw(tex, mousePos, new Rectangle(153, 237, 4, 4), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, 1);

            this.PrevMousePos = mousePos;
        }


        /*********
        ** Private methods
        *********/
        // The below comes from here: https://stackoverflow.com/questions/33977226/drawing-bezier-curves-in-monogame-xna-produces-scratchy-lines
        //This is what I call to get all points between which to draw.
        private List<Vector2> ComputeCurvePoints(int steps, Vector2[] pointsQ)
        {
            List<Vector2> curvePoints = new List<Vector2>();
            for (float x = 0; x < 1; x += 1 / (float)steps)
            {
                curvePoints.Add(this.GetBezierPointRecursive(x, pointsQ));
            }
            return curvePoints;
        }

        //Calculates a point on the bezier curve based on the timeStep.
        private Vector2 GetBezierPointRecursive(float timeStep, Vector2[] ps)
        {
            if (ps.Length > 2)
            {
                List<Vector2> newPoints = new List<Vector2>();
                for (int x = 0; x < ps.Length - 1; x++)
                {
                    newPoints.Add(this.InterpolatedPoint(ps[x], ps[x + 1], timeStep));
                }
                return this.GetBezierPointRecursive(timeStep, newPoints.ToArray());
            }
            else
            {
                return this.InterpolatedPoint(ps[0], ps[1], timeStep);
            }
        }

        //Gets the linearly interpolated point at t between two given points (without manual rounding).
        //Bad results!
        private Vector2 InterpolatedPoint(Vector2 p1, Vector2 p2, float t)
        {
            Vector2 roundedVector = (Vector2.Multiply(p2 - p1, t) + p1);
            return new Vector2((int)roundedVector.X, (int)roundedVector.Y);
        }

        //Method used to draw a line between two points.
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(pixel, r, new Rectangle(150, 237, 3, 3), color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
