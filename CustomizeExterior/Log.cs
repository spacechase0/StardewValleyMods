using StardewModdingAPI;
using System;

namespace CustomizeExterior
{
    class Log
    {
        public static void trace(String str)
        {
            CustomizeExteriorMod.instance.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(String str)
        {
            CustomizeExteriorMod.instance.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(String str)
        {
            CustomizeExteriorMod.instance.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(String str)
        {
            CustomizeExteriorMod.instance.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(String str)
        {
            CustomizeExteriorMod.instance.Monitor.Log(str, LogLevel.Error);
        }
    }
}
