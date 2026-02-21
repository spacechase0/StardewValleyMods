using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpaceShared
{
    internal static partial class Util
    {
        public static bool UsingMono => Type.GetType("Mono.Runtime") != null;

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

        public static int Wrap(int value, int min, int max)
        {
            int interval = max - min;
            return (value - min) % interval + min;
        }

        public static float Wrap(float value, float min, float max)
        {
            float interval = max - min;
            return (value - min) % interval + min;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        static partial void LogError(string str);

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
                    LogError($"Exception while handling event {name}:\n{e}");
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
                    LogError($"Exception while handling event {name}:\n{e}");
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
                    LogError($"Exception while handling event {name}:\n{e}");
                }
            }

            return args.Cancel;
        }
    }
}
