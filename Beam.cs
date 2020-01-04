using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheftOfTheWinterSTar
{
    public class Beam
    {
        private Farmer shooter;
        private Vector2 basePos;
        private float angle;
        private ICue sound;
        
        private int timer = 30;

        public Beam(Farmer who, Vector2 aim)
        {
            shooter = who;
            basePos = shooter.getStandingPosition();
            sound = Game1.soundBank.GetCue("throwDownITem");
            sound.Play();

            switch (who.FacingDirection)
            {
                case 2: basePos.X +=  44; basePos.Y +=   12; break;
                case 0: basePos.X += -26; basePos.Y += -100; break;
                case 3: basePos.X += -40; basePos.Y +=  -90; break;
                case 1: basePos.X +=  40; basePos.Y +=  -90; break;
            }

            angle = (float)Math.Atan2(basePos.Y - aim.Y, basePos.X - aim.X);

            Mod.instance.Helper.Events.GameLoop.UpdateTicked += update;
            Mod.instance.Helper.Events.Display.RenderedWorld += render;

        }
        
        private void update(object sender, UpdateTickedEventArgs e)
        {
            if ( timer-- <= 0 )
            {
                Mod.instance.Helper.Events.GameLoop.UpdateTicked -= update;
                Mod.instance.Helper.Events.Display.RenderedWorld -= render;
                sound.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
            }

            Vector2 lineEnd = new Vector2(basePos.X + (float)Math.Cos(angle + Math.PI) * 10000, basePos.Y + (float)Math.Sin(angle + Math.PI) * 10000);

            Vector2 higher = lineEnd.Y < basePos.Y ? lineEnd : basePos;
            Vector2 lower = lineEnd.Y < basePos.Y ? basePos : lineEnd;
            Vector2 lefter = lineEnd.X < basePos.X ? lineEnd : basePos;
            Vector2 righter = lineEnd.X < basePos.X ? basePos : lineEnd;
            foreach ( var character in shooter.currentLocation.characters.ToList() )
            {
                if ( character is Monster mob )
                {
                    var bb = mob.GetBoundingBox();
                    var tl = new Vector2(bb.Left, bb.Top);
                    var tr = new Vector2(bb.Right, bb.Top);
                    var bl = new Vector2(bb.Left, bb.Bottom);
                    var br = new Vector2(bb.Right, bb.Bottom);

                    var i1 = LSegsIntersectionPoint(basePos, lineEnd, tl, tr);
                    var i2 = LSegsIntersectionPoint(basePos, lineEnd, tr, br);
                    var i3 = LSegsIntersectionPoint(basePos, lineEnd, bl, br);
                    var i4 = LSegsIntersectionPoint(basePos, lineEnd, tl, bl);

                    Vector2 cont = Vector2.Zero;
                    if (i1.HasValue && bb.Contains((int)i1.Value.X, (int)i1.Value.Y)) cont = i1.Value;
                    if (i2.HasValue && bb.Contains((int)i2.Value.X, (int)i2.Value.Y)) cont = i2.Value;
                    if (i3.HasValue && bb.Contains((int)i3.Value.X, (int)i3.Value.Y)) cont = i3.Value;
                    if (i4.HasValue && bb.Contains((int)i4.Value.X, (int)i4.Value.Y)) cont = i4.Value;
                    if ( cont.X >= lefter.X && cont.X <= righter.X &&
                         cont.Y >= higher.Y && cont.Y <= lower.Y &&
                         bb.Contains((int) cont.X, (int) cont.Y) )
                    {
                        shooter.currentLocation.damageMonster(bb, 6, 8, false, shooter);
                        //mob.takeDamage(3, 0, 0, false, 0, shooter);
                    }
                }
            }
        }

        private void render(object sender, RenderedWorldEventArgs e)
        {
            var b = e.SpriteBatch;
            Vector2 pos = Game1.GlobalToLocal(basePos);

            Color[] colors = new Color[]
            {
                new Color(109, 180, 181),
                new Color(194, 238, 229),
                new Color(232, 255, 244),
                Color.White,
            };

            int minW = 2, maxW = 12;
            int unitW = (maxW - minW) / (colors.Length - 1);
            for ( int i = 0; i < colors.Length; ++i )
            {
                Color col = colors[i];
                int currW = minW + unitW * (colors.Length - i - 1);
                var drawPos = new Vector2(pos.X - currW / 2, pos.Y);
                var origin = new Vector2(Game1.staminaRect.Width / 2f, 0f);
                var scale = new Vector2(currW, 10000);
                b.Draw(Game1.staminaRect, drawPos, null, col, angle + (float)Math.PI / 2, origin, scale, SpriteEffects.None, 1);
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
            float A1 = pe1.Y - ps1.Y;
            float B1 = ps1.X - pe1.X;
            // Get A,B of second line - points : ps2 to pe2
            float A2 = pe2.Y - ps2.Y;
            float B2 = ps2.X - pe2.X;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0) return null;

            // Get C of first and second lines
            float C2 = A2 * ps2.X + B2 * ps2.Y;
            float C1 = A1 * ps1.X + B1 * ps1.Y;
            //invert delta to make division cheaper
            float invdelta = 1 / delta;
            // now return the Vector2 intersection point
            return new Vector2((B2 * C1 - B1 * C2) * invdelta, (A1 * C2 - A2 * C1) * invdelta);
        }
    }
}
