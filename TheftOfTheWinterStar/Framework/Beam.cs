using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace TheftOfTheWinterStar.Framework
{
    internal class Beam
    {
        /*********
        ** Fields
        *********/
        private readonly Farmer Shooter;
        private readonly Vector2 BasePos;
        private readonly float Angle;
        private readonly ICue Sound;

        private int Timer = 30;


        /*********
        ** Public methods
        *********/
        public Beam(Farmer who, Vector2 aim)
        {
            this.Shooter = who;
            this.BasePos = this.Shooter.getStandingPosition();
            this.Sound = Game1.soundBank.GetCue("throwDownITem");
            this.Sound.Play();

            switch (who.FacingDirection)
            {
                case 2: this.BasePos.X += 44; this.BasePos.Y += 12; break;
                case 0: this.BasePos.X += -26; this.BasePos.Y += -100; break;
                case 3: this.BasePos.X += -40; this.BasePos.Y += -90; break;
                case 1: this.BasePos.X += 40; this.BasePos.Y += -90; break;
            }

            this.Angle = (float)Math.Atan2(this.BasePos.Y - aim.Y, this.BasePos.X - aim.X);

            Mod.Instance.Helper.Events.GameLoop.UpdateTicked += this.Update;
            Mod.Instance.Helper.Events.Display.RenderedWorld += this.Render;
        }


        /*********
        ** Private methods
        *********/
        private void Update(object sender, UpdateTickedEventArgs e)
        {
            if (this.Timer-- <= 0)
            {
                Mod.Instance.Helper.Events.GameLoop.UpdateTicked -= this.Update;
                Mod.Instance.Helper.Events.Display.RenderedWorld -= this.Render;
                this.Sound.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            }

            Vector2 lineEnd = new Vector2(this.BasePos.X + (float)Math.Cos(this.Angle + Math.PI) * 10000, this.BasePos.Y + (float)Math.Sin(this.Angle + Math.PI) * 10000);

            Vector2 higher = lineEnd.Y < this.BasePos.Y ? lineEnd : this.BasePos;
            Vector2 lower = lineEnd.Y < this.BasePos.Y ? this.BasePos : lineEnd;
            Vector2 lefter = lineEnd.X < this.BasePos.X ? lineEnd : this.BasePos;
            Vector2 righter = lineEnd.X < this.BasePos.X ? this.BasePos : lineEnd;
            foreach (var character in this.Shooter.currentLocation.characters.ToList())
            {
                if (character is Monster mob)
                {
                    var bb = mob.GetBoundingBox();
                    var tl = new Vector2(bb.Left, bb.Top);
                    var tr = new Vector2(bb.Right, bb.Top);
                    var bl = new Vector2(bb.Left, bb.Bottom);
                    var br = new Vector2(bb.Right, bb.Bottom);

                    var i1 = Beam.LSegsIntersectionPoint(this.BasePos, lineEnd, tl, tr);
                    var i2 = Beam.LSegsIntersectionPoint(this.BasePos, lineEnd, tr, br);
                    var i3 = Beam.LSegsIntersectionPoint(this.BasePos, lineEnd, bl, br);
                    var i4 = Beam.LSegsIntersectionPoint(this.BasePos, lineEnd, tl, bl);

                    Vector2 cont = Vector2.Zero;
                    if (i1.HasValue && bb.Contains((int)i1.Value.X, (int)i1.Value.Y)) cont = i1.Value;
                    if (i2.HasValue && bb.Contains((int)i2.Value.X, (int)i2.Value.Y)) cont = i2.Value;
                    if (i3.HasValue && bb.Contains((int)i3.Value.X, (int)i3.Value.Y)) cont = i3.Value;
                    if (i4.HasValue && bb.Contains((int)i4.Value.X, (int)i4.Value.Y)) cont = i4.Value;
                    if (cont.X >= lefter.X && cont.X <= righter.X &&
                         cont.Y >= higher.Y && cont.Y <= lower.Y &&
                         bb.Contains((int)cont.X, (int)cont.Y))
                    {
                        this.Shooter.currentLocation.damageMonster(bb, 6, 8, false, this.Shooter);
                        //mob.takeDamage(3, 0, 0, false, 0, shooter);
                    }
                }
            }
        }

        private void Render(object sender, RenderedWorldEventArgs e)
        {
            var b = e.SpriteBatch;
            Vector2 pos = Game1.GlobalToLocal(this.BasePos);

            Color[] colors = new[]
            {
                new(109, 180, 181),
                new(194, 238, 229),
                new(232, 255, 244),
                Color.White
            };

            int minW = 2, maxW = 12;
            int unitW = (maxW - minW) / (colors.Length - 1);
            for (int i = 0; i < colors.Length; ++i)
            {
                Color col = colors[i];
                int currW = minW + unitW * (colors.Length - i - 1);
                var drawPos = new Vector2(pos.X - currW / 2, pos.Y);
                var origin = new Vector2(Game1.staminaRect.Width / 2f, 0f);
                var scale = new Vector2(currW, 10000);
                b.Draw(Game1.staminaRect, drawPos, null, col, this.Angle + (float)Math.PI / 2, origin, scale, SpriteEffects.None, 1);
            }

            /*
            Vector2 lineEnd = new Vector2(basePos.X + (float)Math.Cos(angle + Math.PI) * 10000, basePos.Y + (float)Math.Sin(angle + Math.PI) * 10000);
            lineEnd = Game1.GlobalToLocal(lineEnd);
            Utility.drawLineWithScreenCoordinates((int)pos.X, (int)pos.Y, (int) lineEnd.X, (int)lineEnd.Y, b, Color.Red); foreach (var character in shooter.currentLocation.characters)
            lineEnd = new Vector2(basePos.X + (float)Math.Cos(angle + Math.PI) * 10000, basePos.Y + (float)Math.Sin(angle + Math.PI) * 10000);
            Vector2 higher = lineEnd.Y < basePos.Y ? lineEnd : basePos;
            Vector2 lower = lineEnd.Y < basePos.Y ? basePos : lineEnd;
            Vector2 lefter = lineEnd.X < basePos.X ? lineEnd : basePos;
            Vector2 righter = lineEnd.X < basePos.X ? basePos : lineEnd;
            foreach (var character in shooter.currentLocation.characters.ToList())
            {
                if (character is Monster mob)
                {
                    var bb = mob.GetBoundingBox();
                    var tl = Game1.GlobalToLocal(new Vector2(bb.Left, bb.Top));
                    var tr = Game1.GlobalToLocal(new Vector2(bb.Right, bb.Top));
                    var bl = Game1.GlobalToLocal(new Vector2(bb.Left, bb.Bottom));
                    var br = Game1.GlobalToLocal(new Vector2(bb.Right, bb.Bottom));
                    Utility.drawLineWithScreenCoordinates((int)tl.X, (int)tl.Y, (int)tr.X, (int)tr.Y, b, Color.Red);
                    Utility.drawLineWithScreenCoordinates((int)br.X, (int)br.Y, (int)tr.X, (int)tr.Y, b, Color.Red);
                    Utility.drawLineWithScreenCoordinates((int)tl.X, (int)tl.Y, (int)bl.X, (int)bl.Y, b, Color.Red);
                    Utility.drawLineWithScreenCoordinates((int)br.X, (int)br.Y, (int)bl.X, (int)bl.Y, b, Color.Red);

                    tl = new Vector2(bb.Left, bb.Top);
                    tr = new Vector2(bb.Right, bb.Top);
                    bl = new Vector2(bb.Left, bb.Bottom);
                    br = new Vector2(bb.Right, bb.Bottom);
                    var i1 = LSegsIntersectionPoint(basePos, lineEnd, tl, tr);
                    var i2 = LSegsIntersectionPoint(basePos, lineEnd, tr, br);
                    var i3 = LSegsIntersectionPoint(basePos, lineEnd, bl, br);
                    var i4 = LSegsIntersectionPoint(basePos, lineEnd, tl, bl);

                    Vector2 cont = Vector2.Zero;
                    if (i1.HasValue && bb.Contains((int)i1.Value.X, (int)i1.Value.Y)) cont = i1.Value;
                    if (i2.HasValue && bb.Contains((int)i2.Value.X, (int)i2.Value.Y)) cont = i2.Value;
                    if (i3.HasValue && bb.Contains((int)i3.Value.X, (int)i3.Value.Y)) cont = i3.Value;
                    if (i4.HasValue && bb.Contains((int)i4.Value.X, (int)i4.Value.Y)) cont = i4.Value;
                    if (cont.X >= lefter.X && cont.X <= righter.X && cont.Y >= higher.Y && cont.Y <= lower.Y && bb.Contains((int)cont.X, (int)cont.Y))
                    {
                        cont = Game1.GlobalToLocal(cont);
                        b.Draw(Game1.staminaRect, cont, null, Color.White, 0, new Vector2(0.5f, 0.5f), 32, SpriteEffects.None, 1);
                    }
                }
            }
            //*/
        }

        // https://gamedev.stackexchange.com/a/111115
        // This seems to do lines though, not segments...
        private static Vector2? LSegsIntersectionPoint(Vector2 ps1, Vector2 pe1, Vector2 ps2, Vector2 pe2)
        {
            // Get A,B of first line - points : ps1 to pe1
            float a1 = pe1.Y - ps1.Y;
            float b1 = ps1.X - pe1.X;
            // Get A,B of second line - points : ps2 to pe2
            float a2 = pe2.Y - ps2.Y;
            float b2 = ps2.X - pe2.X;

            // Get delta and check if the lines are parallel
            float delta = a1 * b2 - a2 * b1;
            if (delta == 0) return null;

            // Get C of first and second lines
            float c2 = a2 * ps2.X + b2 * ps2.Y;
            float c1 = a1 * ps1.X + b1 * ps1.Y;
            //invert delta to make division cheaper
            float invdelta = 1 / delta;
            // now return the Vector2 intersection point
            return new Vector2((b2 * c1 - b1 * c2) * invdelta, (a1 * c2 - a2 * c1) * invdelta);
        }
    }
}
