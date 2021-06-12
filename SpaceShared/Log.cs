using StardewModdingAPI;

namespace SpaceShared
{
    internal class Log
    {
        public static IMonitor Monitor;

        public static void verbose(string str)
        {
            Log.Monitor.VerboseLog(str);
        }

        public static void trace(string str)
        {
            Log.Monitor.Log(str, LogLevel.Trace);
        }

        public static void debug(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        public static void info(string str)
        {
            Log.Monitor.Log(str, LogLevel.Info);
        }

        public static void warn(string str)
        {
            Log.Monitor.Log(str, LogLevel.Warn);
        }

        public static void error(string str)
        {
            Log.Monitor.Log(str, LogLevel.Error);
        }
    }
}
