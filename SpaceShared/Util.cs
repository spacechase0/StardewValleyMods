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

#nullable enable

    internal static class Util
    {
        public static bool UsingMono => Type.GetType("Mono.Runtime") != null;

        public static Texture2D FetchTexture( IModRegistry modRegistry, string modIdAndPath )
        {
            if ( modIdAndPath == null || modIdAndPath.IndexOf( '/' ) == -1 )
                return Game1.staminaRect;

            string packId = modIdAndPath.Substring( 0, modIdAndPath.IndexOf( '/' ) );
            string path = modIdAndPath.Substring( modIdAndPath.IndexOf( '/' ) + 1 );

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get( packId );

            if (modInfo is null)
                return Game1.staminaRect;

            if ( modInfo.GetType().GetProperty( "Mod" )?.GetValue( modInfo ) is IMod mod )
                return mod.Helper.ModContent.Load<Texture2D>( path );
            else if ( modInfo.GetType().GetProperty( "ContentPack" )?.GetValue( modInfo ) is IContentPack pack )
                return pack.ModContent.Load<Texture2D>( path );

            return Game1.staminaRect;
        }

        public static IAssetName? FetchTextureLocation(IModRegistry modRegistry, string modIdAndPath)
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo is null)
                return null;

            if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
                return mod.Helper.ModContent.GetInternalAssetName(path);
            else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
                return pack.ModContent.GetInternalAssetName(path);

            return null;
        }

        public static string? FetchTexturePath( IModRegistry modRegistry, string modIdAndPath )
            => FetchTextureLocation(modRegistry, modIdAndPath)?.BaseName;

        public static string FetchFullPath(IModRegistry modRegistry, string modIdAndPath)
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo is null)
                return null;

            if (modInfo.GetType().GetProperty("Mod")?.GetValue(modInfo) is IMod mod)
                return Path.Combine(mod.Helper.DirectoryPath, path);
            else if (modInfo.GetType().GetProperty("ContentPack")?.GetValue(modInfo) is IContentPack pack)
                return Path.Combine(pack.DirectoryPath, path);

            return null;
        }

#nullable restore

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

        public static T Clamp<T>(T min, T t, T max)
        {
            if (Comparer<T>.Default.Compare(min, t) > 0)
                return min;
            if (Comparer<T>.Default.Compare(max, t) < 0)
                return max;
            return t;
        }

        public static T Adjust<T>(T value, T interval)
        {
            if (value is float vFloat && interval is float iFloat)
                value = (T)(object)(float)((decimal)vFloat - ((decimal)vFloat % (decimal)iFloat));

            if (value is int vInt && interval is int iInt)
                value = (T)(object)(vInt - vInt % iInt);

            return value;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
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

        // Stolen from SMAPI
        public static void InvokeEvent(string name, IEnumerable<Delegate> handlers, object sender)
        {
            var args = new EventArgs();
            foreach (EventHandler handler in handlers.Cast<EventHandler>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception while handling event {name}:\n{e}");
                }
            }
        }

        public static void InvokeEvent<T>(string name, IEnumerable<Delegate> handlers, object sender, T args)
        {
            foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception while handling event {name}:\n{e}");
                }
            }
        }

        // Returns if the event was canceled or not
        public static bool InvokeEventCancelable<T>(string name, IEnumerable<Delegate> handlers, object sender, T args) where T : CancelableEventArgs
        {
            foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Log.Error($"Exception while handling event {name}:\n{e}");
                }
            }

            return args.Cancel;
        }
    }
}
