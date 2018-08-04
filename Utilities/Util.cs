using SpaceCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Utilities
{
    public class Util
    {
        public static bool UsingMono
        {
            get { return Type.GetType("Mono.Runtime") != null; }
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

        public static void invokeEvent< T >( string name, IEnumerable<Delegate> handlers, object sender, T args )
        {
            foreach ( EventHandler< T > handler in handlers.Cast< EventHandler< T > >() )
            {
                try
                {
                    handler.Invoke(sender, args);
                }
                catch ( Exception e )
                {
                    Log.error( $"Exception while handling event {name}:\n{e}" );
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
