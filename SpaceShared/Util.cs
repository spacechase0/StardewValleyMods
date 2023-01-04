using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SpaceShared
{
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
            if ( modInfo.GetType().GetProperty( "Mod" ).GetValue( modInfo ) is IMod mod )
                return mod.Helper.ModContent.Load<Texture2D>( path );
            else if ( modInfo.GetType().GetProperty( "ContentPack" ).GetValue( modInfo ) is IContentPack pack )
                return pack.ModContent.Load<Texture2D>( path );

            return Game1.staminaRect;
        }

        public static string FetchTexturePath( IModRegistry modRegistry, string modIdAndPath )
        {
            if ( modIdAndPath == null || modIdAndPath.IndexOf( '/' ) == -1 )
                return null;

            string packId = modIdAndPath.Substring( 0, modIdAndPath.IndexOf( '/' ) );
            string path = modIdAndPath.Substring( modIdAndPath.IndexOf( '/' ) + 1 );

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get( packId );
            if ( modInfo.GetType().GetProperty( "Mod" ).GetValue( modInfo ) is IMod mod )
                return mod.Helper.ModContent.GetInternalAssetName( path ).Name;
            else if ( modInfo.GetType().GetProperty( "ContentPack" ).GetValue( modInfo ) is IContentPack pack )
                return pack.ModContent.GetInternalAssetName( path ).Name;

            return null;
        }

        public static string FetchFullPath(IModRegistry modRegistry, string modIdAndPath)
        {
            if (modIdAndPath == null || modIdAndPath.IndexOf('/') == -1)
                return null;

            string packId = modIdAndPath.Substring(0, modIdAndPath.IndexOf('/'));
            string path = modIdAndPath.Substring(modIdAndPath.IndexOf('/') + 1);

            // This is really bad. Pathos don't kill me.
            var modInfo = modRegistry.Get(packId);
            if (modInfo.GetType().GetProperty("Mod").GetValue(modInfo) is IMod mod)
                return Path.Combine(mod.Helper.DirectoryPath, path);
            else if (modInfo.GetType().GetProperty("ContentPack").GetValue(modInfo) is IContentPack pack)
                return Path.Combine(pack.DirectoryPath, path);

            return null;
        }

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
