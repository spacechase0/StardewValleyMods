using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace SpaceShared
{
    internal static partial class Util
    {
        public static Texture2D DoPaletteSwap(Texture2D baseTex, Texture2D from, Texture2D to)
        {
            var fromCols = new Color[from.Height];
            var toCols = new Color[to.Height];
            from.GetData(fromCols);
            to.GetData(toCols);
            return Util.DoPaletteSwap(baseTex, fromCols, toCols);
        }

        public static Texture2D DoPaletteSwap(Texture2D baseTex, Color[] fromCols, Color[] toCols)
        {
            var colMap = new Dictionary<Color, Color>();
            for (int i = 0; i < fromCols.Length; ++i)
            {
                colMap.Add(fromCols[i], toCols[i]);
            }

            var cols = new Color[baseTex.Width * baseTex.Height];
            baseTex.GetData(cols);
            for (int i = 0; i < cols.Length; ++i)
            {
                if (colMap.TryGetValue(cols[i], out Color color))
                    cols[i] = color;
            }

            var newTex = new Texture2D(Game1.graphics.GraphicsDevice, baseTex.Width, baseTex.Height);
            newTex.SetData(cols);
            return newTex;
        }

        // https://stackoverflow.com/a/1626175/17827276
        public static Color ColorFromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return new Color(v, t, p);
            else if (hi == 1)
                return new Color(q, v, p);
            else if (hi == 2)
                return new Color(p, v, t);
            else if (hi == 3)
                return new Color(p, q, v);
            else if (hi == 4)
                return new Color(t, p, v);
            else
                return new Color(v, p, q);
        }

        // https://stackoverflow.com/a/57385008
        public static IEnumerable<Color> GetColorGradient(Color from, Color to, int totalNumberOfColors)
        {
            if (totalNumberOfColors< 2)
            {
                throw new ArgumentException("Gradient cannot have less than two colors.", nameof(totalNumberOfColors));
            }

            double diffA = to.A - from.A;
            double diffR = to.R - from.R;
            double diffG = to.G - from.G;
            double diffB = to.B - from.B;

            int steps = totalNumberOfColors - 1;

            double stepA = diffA / steps;
            double stepR = diffR / steps;
            double stepG = diffG / steps;
            double stepB = diffB / steps;

            yield return from;

            for (int i = 1; i<steps; ++i)
            {
                yield return new Color(
                    c(from.R, stepR),
                    c(from.G, stepG),
                    c(from.B, stepB),
                    c(from.A, stepA));

                    int c(int fromC, double stepC)
                {
                    return (int)Math.Round(fromC + stepC * i);
                }
            }

            yield return to;
        }
    }
}
