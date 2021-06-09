using System;
using StardewModdingAPI;

namespace Terraforming
{
    class Log
    {
        public static void trace(string str)
        {
            Mod.instance.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(string str)
        {
            Mod.instance.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(string str)
        {
            Mod.instance.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(string str)
        {
            Mod.instance.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(string str)
        {
            Mod.instance.Monitor.Log(str, LogLevel.Error);
        }
    }
}
