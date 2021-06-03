using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceShared
{
    public struct Range<T>
    {
        public T Minimum;
        public T Maximum;
    }
    
    public class Util
    {
        public static bool UsingMono
        {
            get { return Type.GetType("Mono.Runtime") != null; }
        }

        public static Texture2D DoPaletteSwap( Texture2D baseTex, Texture2D from, Texture2D to )
        {
            var fromCols = new Color[ from.Height ];
            var toCols = new Color[ to.Height ];
            from.GetData( fromCols );
            to.GetData( toCols );
            return DoPaletteSwap( baseTex, fromCols, toCols );
        }
        
        public static Texture2D DoPaletteSwap( Texture2D baseTex, Color[] fromCols, Color[] toCols )
        {
            var colMap = new Dictionary< Color, Color >();
            for ( int i = 0; i < fromCols.Length; ++i )
            {
                colMap.Add( fromCols[ i ], toCols[ i ] );
            }

            var cols = new Color[ baseTex.Width * baseTex.Height ];
            baseTex.GetData( cols );
            for ( int i = 0; i < cols.Length; ++i )
            {
                if ( colMap.ContainsKey( cols[ i ] ) )
                    cols[ i ] = colMap[ cols[ i ] ];
            }

            var newTex = new Texture2D(Game1.graphics.GraphicsDevice, baseTex.Width, baseTex.Height );
            newTex.SetData( cols );
            return newTex;
        }

        public static T Clamp<T>( T min, T t, T max )
        {
            if ( Comparer<T>.Default.Compare( min, t ) > 0 )
                return min;
            if ( Comparer<T>.Default.Compare( max, t ) < 0 )
                return max;
            return t;
        }

        public static T Adjust<T>( T value, T interval )
        {
            if ( value is float vFloat && interval is float iFloat )
                value = ( T ) ( object ) ( float ) ( ( decimal ) vFloat - ( ( decimal ) vFloat % ( decimal ) iFloat ) );

            if ( value is int vInt && interval is int iInt )
                value = ( T ) ( object ) ( vInt - vInt % iInt );

            return value;
        }

        public static void swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        // Stolen from SMAPI
        public static void invokeEvent(string name, IEnumerable<Delegate> handlers, object sender)
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
                    Log.error($"Exception while handling event {name}:\n{e}");
                }
            }
        }

        public static void invokeEvent<T>(string name, IEnumerable<Delegate> handlers, object sender, T args)
        {
            foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Log.error($"Exception while handling event {name}:\n{e}");
                }
            }
        }

        // Returns if the event was canceled or not
        public static bool invokeEventCancelable<T>(string name, IEnumerable<Delegate> handlers, object sender, T args) where T : CancelableEventArgs
        {
            foreach (EventHandler<T> handler in handlers.Cast<EventHandler<T>>())
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch (Exception e)
                {
                    Log.error($"Exception while handling event {name}:\n{e}");
                }
            }

            return args.Cancel;
        }
    }
}
