using System;
using StardewModdingAPI;

namespace SpaceShared
{
    class Log
    {
        public static IMonitor Monitor;

        public static void verbose(String str)
        {
            Log.Monitor.VerboseLog(str);
        }

        public static void trace(String str)
        {
            Log.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(String str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(String str)
        {
            Log.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(String str)
        {
            Log.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(String str)
        {
            Log.Monitor.Log(str, LogLevel.Error);
        }
    }
}
