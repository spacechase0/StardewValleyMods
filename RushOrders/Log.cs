using StardewModdingAPI;
using System;

namespace RushOrders
{
    class Log
    {
        public static void trace(String str)
        {
            RushOrdersMod.instance.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(String str)
        {
            RushOrdersMod.instance.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(String str)
        {
            RushOrdersMod.instance.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(String str)
        {
            RushOrdersMod.instance.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(String str)
        {
            RushOrdersMod.instance.Monitor.Log(str, LogLevel.Error);
        }
    }
}
