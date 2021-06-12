using System;
using StardewModdingAPI;

namespace Terraforming
{
    internal class Log
    {
        public static void Trace(string str)
        {
            Mod.Instance.Monitor.Log(str, LogLevel.Trace);
        }

        public static void Debug(string str)
        {
            Mod.Instance.Monitor.Log(str, LogLevel.Debug);
        }

        public static void Info(string str)
        {
            Mod.Instance.Monitor.Log(str, LogLevel.Info);
        }

        public static void Warn(string str)
        {
            Mod.Instance.Monitor.Log(str, LogLevel.Warn);
        }

        public static void Error(string str)
        {
            Mod.Instance.Monitor.Log(str, LogLevel.Error);
        }
    }
}
