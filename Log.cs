using StardewModdingAPI;
using System;

namespace SpaceCore
{
    class Log
    {
        public static void trace(String str)
        {
            SpaceCore.instance.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(String str)
        {
            SpaceCore.instance.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(String str)
        {
            SpaceCore.instance.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(String str)
        {
            SpaceCore.instance.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(String str)
        {
            SpaceCore.instance.Monitor.Log(str, LogLevel.Error);
        }
    }
}
