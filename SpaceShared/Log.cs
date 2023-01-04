using System.Diagnostics;

using StardewModdingAPI;

namespace SpaceShared
{
    internal class Log
    {
        public static IMonitor Monitor;

        public static bool IsVerbose => Monitor.IsVerbose;

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str, bool pred)
        {
            if (pred)
                Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        public static void Verbose(string str)
        {
            Log.Monitor.VerboseLog(str);
        }

        public static void Trace(string str)
        {
            Log.Monitor.Log(str, LogLevel.Trace);
        }

        public static void Debug(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        public static void Info(string str)
        {
            Log.Monitor.Log(str, LogLevel.Info);
        }

        public static void Warn(string str)
        {
            Log.Monitor.Log(str, LogLevel.Warn);
        }

        public static void Error(string str)
        {
            Log.Monitor.Log(str, LogLevel.Error);
        }
    }
}
